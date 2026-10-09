using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : MonoBehaviourSingleton<PhysicsManager>
{
	[Header("Settings")]
	[SerializeField]
	private SimulationMode simulationMode;

	[SerializeField]
	private int tickRate = 100;

	private float tickAccumulator;

	[HideInInspector]
	public SimulationMode SimulationMode
	{
		get
		{
			return Physics.simulationMode;
		}
		set
		{
			if (Physics.simulationMode != value)
			{
				Physics.simulationMode = value;
				tickAccumulator = 0f;
			}
		}
	}

	[HideInInspector]
	public int TickRate
	{
		get
		{
			return tickRate;
		}
		set
		{
			if (tickRate != value)
			{
				tickRate = value;
				Time.fixedDeltaTime = TickInterval;
				EventManager.TriggerEvent("Event_OnPhysicsManagerTickRateChanged", new Dictionary<string, object> { { "value", tickRate } });
			}
		}
	}

	[HideInInspector]
	public float TickInterval => 1f / (float)TickRate;

	public static event Action<float> OnBeforeSimulate;

	public static event Action<float> OnAfterSimulate;

	public override void Awake()
	{
		base.Awake();
		Physics.simulationMode = simulationMode;
		Time.fixedDeltaTime = TickInterval;
		StartCoroutine(AfterFixedUpdateSimulate());
	}

	private void Start()
	{
		EventManager.TriggerEvent("Event_OnPhysicsManagerInitialized", new Dictionary<string, object> { { "tickRate", tickRate } });
	}

	private void FixedUpdate()
	{
		if (Physics.simulationMode == SimulationMode.FixedUpdate)
		{
			OnBeforeSimulate?.Invoke(Time.fixedDeltaTime);
		}
	}

	private void Update()
	{
		if (Physics.simulationMode == SimulationMode.Script)
		{
			tickAccumulator += Time.deltaTime;
			if (tickAccumulator >= TickInterval)
			{
				OnBeforeSimulate?.Invoke(TickInterval);
				Physics.Simulate(TickInterval);
				OnAfterSimulate?.Invoke(TickInterval);
				tickAccumulator -= TickInterval;
			}
		}
	}

	private IEnumerator AfterFixedUpdateSimulate()
	{
		WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
		while (true)
		{
			yield return waitForFixedUpdate;
			if (Physics.simulationMode == SimulationMode.FixedUpdate)
			{
				OnAfterSimulate?.Invoke(Time.fixedDeltaTime);
			}
		}
	}
}
