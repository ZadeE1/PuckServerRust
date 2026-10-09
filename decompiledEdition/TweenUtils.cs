using DG.Tweening;

public static class TweenUtils
{
	public static Sequence RampHoldRamp(float rest, float depth, float rampInSeconds, float holdSeconds, float rampOutSeconds, Ease rampInEase, Ease rampOutEase, TweenCallback<float> onValue)
	{
		return RampHoldRamp(rest, depth, rest, rampInSeconds, holdSeconds, rampOutSeconds, rampInEase, rampOutEase, onValue);
	}

	public static Sequence RampHoldRamp(float from, float depth, float to, float rampInSeconds, float holdSeconds, float rampOutSeconds, Ease rampInEase, Ease rampOutEase, TweenCallback<float> onValue)
	{
		Sequence sequence = DOTween.Sequence().SetUpdate(isIndependentUpdate: true);
		sequence.Append(DOVirtual.Float(from, depth, rampInSeconds, onValue).SetEase(rampInEase));
		sequence.AppendInterval(holdSeconds);
		sequence.Append(DOVirtual.Float(depth, to, rampOutSeconds, onValue).SetEase(rampOutEase));
		return sequence;
	}
}
