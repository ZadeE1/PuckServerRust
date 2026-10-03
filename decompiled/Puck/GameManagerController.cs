using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GameManagerController : MonoBehaviour
{
	private GameManager gameManager;

	private Sequence slowMotionSequence;

	private float slowMotionTargetScale = 1f;

	private float slowMotionAudioPitchFloor = 1f;

	private float slowMotionScale = 1f;

	private Sequence cameraPunchSequence;

	private float slowMotionCameraFovPunch;

	private float slowMotionCameraAmount;

	private void Awake()
	{
		gameManager = GetComponent<GameManager>();
		EventManager.AddEventListener("Event_Everyone_OnSlowMotionStarted", Event_Everyone_OnSlowMotionStarted);
		EventManager.AddEventListener("Event_Everyone_OnSlowMotionStopped", Event_Everyone_OnSlowMotionStopped);
	}

	private void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnSlowMotionStarted", Event_Everyone_OnSlowMotionStarted);
		EventManager.RemoveEventListener("Event_Everyone_OnSlowMotionStopped", Event_Everyone_OnSlowMotionStopped);
		slowMotionSequence?.Kill();
		ApplySlowMotionScale(1f);
		cameraPunchSequence?.Kill();
		ApplyCameraPunch(0f);
	}

	private void Event_Everyone_OnSlowMotionStarted(Dictionary<string, object> message)
	{
		float depth = (float)message["scale"];
		float num = (float)message["rampInSeconds"];
		float num2 = (float)message["holdSeconds"];
		float num3 = (float)message["rampOutSeconds"];
		slowMotionTargetScale = depth;
		slowMotionAudioPitchFloor = (float)message["audioPitchFloor"];
		slowMotionCameraFovPunch = (float)message["cameraFovPunch"];
		slowMotionSequence?.Kill();
		slowMotionSequence = TweenUtils.RampHoldRamp(slowMotionScale, depth, 1f, num, num2, num3, Ease.InQuad, Ease.OutQuad, ApplySlowMotionScale).SetTarget(this);
		slowMotionSequence.OnComplete(() =>
		{
			ApplySlowMotionScale(1f);
		});
		slowMotionSequence.OnKill(() =>
		{
			slowMotionSequence = null;
		});
		cameraPunchSequence?.Kill();
		if (slowMotionCameraFovPunch > 0f)
		{
			cameraPunchSequence = DOTween.Sequence().SetUpdate(isIndependentUpdate: true).SetTarget(this);
			cameraPunchSequence.Append(DOVirtual.Float(slowMotionCameraAmount, 1f, num + num2, ApplyCameraPunch).SetEase(Ease.InOutSine));
			cameraPunchSequence.Append(DOVirtual.Float(1f, 0f, num3, ApplyCameraPunch).SetEase(Ease.InOutSine));
			cameraPunchSequence.OnComplete(() =>
			{
				ApplyCameraPunch(0f);
			});
			cameraPunchSequence.OnKill(() =>
			{
				cameraPunchSequence = null;
			});
		}
		else
		{
			ApplyCameraPunch(0f);
		}
	}

	private void Event_Everyone_OnSlowMotionStopped(Dictionary<string, object> message)
	{
		slowMotionSequence?.Kill();
		cameraPunchSequence?.Kill();
		ApplySlowMotionScale(1f);
		ApplyCameraPunch(0f);
	}

	private void ApplySlowMotionScale(float scale)
	{
		slowMotionScale = scale;
		if (!(MonoBehaviourSingleton<AudioManager>.Instance == null))
		{
			float t = ((slowMotionTargetScale < 1f) ? Mathf.InverseLerp(1f, slowMotionTargetScale, scale) : 0f);
			float slowMotionLowpassCutoff = Mathf.Exp(Mathf.Lerp(Mathf.Log(22000f), Mathf.Log(900f), t));
			MonoBehaviourSingleton<AudioManager>.Instance.SetSlowMotionLowpassCutoff(slowMotionLowpassCutoff);
			MonoBehaviourSingleton<AudioManager>.Instance.SetSlowMotionPitch(Mathf.Lerp(1f, slowMotionAudioPitchFloor, t));
		}
	}

	private void ApplyCameraPunch(float amount)
	{
		slowMotionCameraAmount = amount;
		BaseCamera activeCamera = CameraManager.GetActiveCamera();
		if (!(activeCamera == null) && activeCamera.Type != CameraType.Spectator)
		{
			activeCamera.SetFieldOfView(SettingsManager.Fov - slowMotionCameraFovPunch * amount);
		}
	}
}
