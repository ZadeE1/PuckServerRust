using UnityEngine;
using UnityEngine.Splines;

public class MoveAlongSpline : MonoBehaviour
{
	[Header("Settings")]
	public float speed = 1f;

	[Header("References")]
	public SplineContainer spline;

	private float splinePosition;

	private void Update()
	{
		splinePosition += Time.deltaTime * speed;
		if (splinePosition >= 1f)
		{
			splinePosition = 0f;
		}
		Vector3 position = spline.EvaluatePosition(splinePosition);
		transform.position = position;
		transform.LookAt(Vector3.zero);
	}
}
