using DG.Tweening;
using UnityEngine.UIElements;

public class UIChatMessage
{
	public ChatMessage ChatMessage;

	public VisualElement VisualElement;

	public float ExpiryTime;

	private Label label;

	private Tween blurTween;

	private double expiryTimestamp => ChatMessage.Timestamp + (double)(ExpiryTime * 1000f);

	private float expiresInTime => (float)(expiryTimestamp - Utils.GetTimestamp()) / 1000f;

	private bool isExpired => Utils.GetTimestamp() > expiryTimestamp;

	public UIChatMessage(ChatMessage chatMessage, VisualElement visualElement, float expiryTime = 5f)
	{
		ChatMessage = chatMessage;
		VisualElement = visualElement;
		ExpiryTime = expiryTime;
		label = VisualElement.Query<Label>();
		Focus();
		StartExpiryTween();
	}

	public void Focus()
	{
		blurTween?.Kill();
		label.EnableInClassList("blurred", enable: false);
	}

	public void Blur()
	{
		if (!isExpired)
		{
			StartExpiryTween();
			return;
		}
		blurTween?.Kill();
		label.EnableInClassList("blurred", enable: true);
	}

	public void Dispose()
	{
		blurTween?.Kill();
	}

	private void StartExpiryTween()
	{
		blurTween?.Kill();
		blurTween = DOVirtual.DelayedCall(expiresInTime, () =>
		{
			Blur();
		});
	}
}
