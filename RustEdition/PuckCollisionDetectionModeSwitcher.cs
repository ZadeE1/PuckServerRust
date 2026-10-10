using Unity.Netcode;
using UnityEngine;

public class PuckCollisionDetectionModeSwitcher : MonoBehaviour
{
	[HideInInspector]
	public Rigidbody Rigidbody;

	[HideInInspector]
	public bool IsContactingStick;

	// ponytail: FixedUpdate runs ~100Hz/puck; skip the Unity extern reads when
	// the mode is already correct (measured ~0.3ms/s, hygiene only).
	private CollisionDetectionMode lastMode;

	private void Awake()
	{
		Rigidbody = GetComponent<Rigidbody>();
		ApplyMode(CollisionDetectionMode.ContinuousDynamic);
	}

	private void ApplyMode(CollisionDetectionMode mode)
	{
		if (lastMode == mode)
		{
			return;
		}
		Utils.SetRigidbodyCollisionDetectionMode(Rigidbody, mode);
		lastMode = mode;
	}

	private void FixedUpdate()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			if (IsContactingStick)
			{
				ApplyMode(CollisionDetectionMode.ContinuousSpeculative);
			}
			else
			{
				ApplyMode(CollisionDetectionMode.ContinuousDynamic);
			}
			IsContactingStick = false;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.TryGetComponent<Stick>(out var _))
		{
			IsContactingStick = true;
			ApplyMode(CollisionDetectionMode.ContinuousSpeculative);
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (collision.gameObject.TryGetComponent<Stick>(out var _))
		{
			IsContactingStick = true;
			ApplyMode(CollisionDetectionMode.ContinuousSpeculative);
		}
	}

	public void OnDrawGizmos()
	{
		if (Application.isEditor)
		{
			if ((bool)Rigidbody)
			{
				Gizmos.color = ((Rigidbody.collisionDetectionMode == CollisionDetectionMode.ContinuousSpeculative) ? Color.red : Color.green);
			}
			Gizmos.DrawWireSphere(transform.position, 0.5f);
		}
	}
}
