using System.Collections.Generic;
using UnityEngine;

public class SoftCollider : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private Vector3 localOrigin = Vector3.zero;

	[SerializeField]
	private LayerMask layerMask;

	[SerializeField]
	private float distance = 0.5f;

	[SerializeField]
	private float force = 10f;

	[HideInInspector]
	public float Intensity = 1f;

	[HideInInspector]
	public Rigidbody Rigidbody;

	[HideInInspector]
	public SynchronizedObject SynchronizedObject;

	private readonly HashSet<Rigidbody> ignoredRigidbodies = new HashSet<Rigidbody>();

	private Vector3 worldOrigin = Vector3.zero;

	private readonly Vector3[] rayDirections = new Vector3[4];

	private void Awake()
	{
		Rigidbody = GetComponent<Rigidbody>();
		SynchronizedObject = GetComponent<SynchronizedObject>();
	}

	public void SetSoftCollisionIgnored(Rigidbody other, bool ignored)
	{
		if (ignored && other != null)
		{
			ignoredRigidbodies.Add(other);
		}
		else
		{
			ignoredRigidbodies.Remove(other);
		}
		ignoredRigidbodies.RemoveWhere((Rigidbody rigidbody) => rigidbody == null);
	}

	private void FixedUpdate()
	{
		if (SynchronizedObject.IsDrivenExternally)
		{
			return;
		}
		worldOrigin = transform.TransformPoint(localOrigin);
		Vector3 forward = transform.forward;
		Vector3 right = transform.right;
		rayDirections[0] = (forward + right).normalized;
		rayDirections[1] = (forward - right).normalized;
		rayDirections[2] = (-forward + right).normalized;
		rayDirections[3] = (-forward - right).normalized;
		Vector3[] array = rayDirections;
		foreach (Vector3 vector in array)
		{
			Debug.DrawRay(worldOrigin, vector * distance, Color.black);
			if (Physics.Raycast(worldOrigin, vector, out var hitInfo, distance, layerMask) && (!(hitInfo.rigidbody != null) || !ignoredRigidbodies.Contains(hitInfo.rigidbody)))
			{
				Debug.DrawRay(worldOrigin, vector * hitInfo.distance, Color.white);
				float num = distance - hitInfo.distance;
				float magnitude = Vector3.Cross(hitInfo.normal, vector).magnitude;
				float num2 = 1f - magnitude;
				Debug.DrawRay(hitInfo.point, hitInfo.normal * num * force, Color.green);
				Rigidbody.AddForceAtPosition(hitInfo.normal * num * (force * num2), hitInfo.point, ForceMode.Acceleration);
			}
		}
	}
}
