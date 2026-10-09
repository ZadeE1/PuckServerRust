using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using UnityEngine;

public class SmoothPositioner : MonoBehaviour
{
	private static readonly Logger Logger = new Logger("SmoothPositioner");

	[Header("Settings")]
	[SerializeField]
	private SerializedDictionary<string, Transform> positions = new SerializedDictionary<string, Transform>();

	[SerializeField]
	private string initialPosition;

	[SerializeField]
	private float transitionDuration = 0.5f;

	[SerializeField]
	private Ease transitionEase = Ease.Linear;

	private string currentPosition;

	private Tween positionTween;

	private Tween rotationTween;

	private void Start()
	{
		SetPosition(initialPosition, instant: true);
	}

	private void OnDestroy()
	{
		positionTween?.Kill();
		rotationTween?.Kill();
	}

	public void SetPosition(string positionName, bool instant = false)
	{
		if (!positions.ContainsKey(positionName))
		{
			Logger.Error("Target position " + positionName + " does not exist");
			return;
		}
		if (instant)
		{
			positionTween?.Kill();
			rotationTween?.Kill();
			transform.position = positions[positionName].position;
			transform.rotation = positions[positionName].rotation;
		}
		else
		{
			if (currentPosition == positionName)
			{
				return;
			}
			positionTween?.Kill();
			rotationTween?.Kill();
			positionTween = transform.DOMove(positions[positionName].position, transitionDuration).SetEase(transitionEase);
			rotationTween = transform.DORotateQuaternion(positions[positionName].rotation, transitionDuration).SetEase(transitionEase);
		}
		currentPosition = positionName;
	}

	private void OnDrawGizmos()
	{
		if (!Application.isEditor)
		{
			return;
		}
		foreach (KeyValuePair<string, Transform> position in positions)
		{
			_ = position.Key;
			Transform value = position.Value;
			Gizmos.color = Color.black;
			Gizmos.DrawSphere(value.position, 0.05f);
			Gizmos.matrix = value.localToWorldMatrix;
			Gizmos.DrawFrustum(Vector3.zero, 60f, 1f, 0f, 1f);
			Gizmos.matrix = Matrix4x4.identity;
			Gizmos.color = Color.green;
			Gizmos.DrawLine(value.position, value.position + value.forward * 1f);
		}
	}
}
