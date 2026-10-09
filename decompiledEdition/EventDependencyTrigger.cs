using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using UnityEngine;

public class EventDependencyTrigger : MonoBehaviour
{
	private static readonly Logger Logger = new Logger("EventDependencyTrigger");

	[Header("Settings")]
	[SerializeField]
	private SerializedDictionary<string, bool> dependencyEvents = new SerializedDictionary<string, bool>();

	[SerializeField]
	private string triggerEventName;

	[SerializeField]
	private float timeout = 3f;

	[SerializeField]
	private bool isRepeating = true;

	private Tween timeoutTween;

	private void Start()
	{
		foreach (string key in dependencyEvents.Keys)
		{
			EventManager.AddEventListener(key, OnDependencyEvent);
		}
	}

	private void OnDestroy()
	{
		timeoutTween?.Kill();
		foreach (string key in dependencyEvents.Keys)
		{
			EventManager.RemoveEventListener(key, OnDependencyEvent);
		}
	}

	private void OnDependencyEvent(Dictionary<string, object> message)
	{
		string key = (string)message["eventName"];
		if (!dependencyEvents.ContainsValue(value: true))
		{
			timeoutTween?.Kill();
			timeoutTween = DOVirtual.DelayedCall(timeout, () =>
			{
				Logger.Warning("Event " + triggerEventName + " timed out waiting for dependencies");
				if (isRepeating)
				{
					Reset();
				}
			});
		}
		dependencyEvents[key] = true;
		if (!dependencyEvents.ContainsValue(value: false))
		{
			Logger.Info("All dependencies met, triggering event " + triggerEventName);
			EventManager.TriggerEvent(triggerEventName);
			if (isRepeating)
			{
				Reset();
			}
		}
	}

	private void Reset()
	{
		timeoutTween?.Kill();
		dependencyEvents.Keys.ToList().ForEach((string key) =>
		{
			dependencyEvents[key] = false;
		});
	}
}
