using UnityEngine.UIElements;

public class UIDebug : UIView
{
	private Label buildLabel;

	private Label hintLabel;

	private DebugMode debugMode;

	private VisualElement simpleView;

	private Label simpleHintLabel;

	private Label networkingHintLabel;

	private VisualElement simpleRoundTripTimeRow;

	private VisualElement simpleLossRow;

	private Label simpleFramesLabel;

	private Label simpleFrameTimeP95Label;

	private Label simpleRoundTripTimeLabel;

	private Label simpleLossLabel;

	private VisualElement networkingView;

	private bool isApplicationVisible;

	private bool isNetworkVisible;

	private bool isObjectSynchronizationVisible;

	private VisualElement applicationSectionView;

	private Label applicationFramesLabel;

	private Label applicationFrameTimeAverageLabel;

	private Label applicationFrameTimeP95Label;

	private Label applicationFrameTimeMaxLabel;

	private VisualElement networkSectionView;

	private Label networkSentWireBytesLabel;

	private Label networkSentPacketsLabel;

	private Label networkReceivedWireBytesLabel;

	private Label networkReceivedPacketsLabel;

	private Label networkRoundTripTimeLabel;

	private Label networkStallLabel;

	private VisualElement objectSynchronizationSectionView;

	private Label objectSynchronizationTicksLabel;

	private Label objectSynchronizationTickDeltaAverageLabel;

	private Label objectSynchronizationTickDeltaP95Label;

	private Label objectSynchronizationTickDeltaMaxLabel;

	private Label objectSynchronizationLossLabel;

	private Label objectSynchronizationReorderedLabel;

	private Label objectSynchronizationTimelineLabel;

	private Label objectSynchronizationSpeedLabel;

	private Label objectSynchronizationSnapsLabel;

	private Label objectSynchronizationExtrapolationLabel;

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("DebugView");
		buildLabel = View.Query<Label>("BuildLabel");
		hintLabel = View.Query<Label>("HintLabel");
		simpleView = View.Query<VisualElement>("Simple");
		simpleHintLabel = View.Query<Label>("SimpleHintLabel");
		networkingHintLabel = View.Query<Label>("NetworkingHintLabel");
		simpleRoundTripTimeRow = View.Query<VisualElement>("SimpleRoundTripTimeRow");
		simpleLossRow = View.Query<VisualElement>("SimpleLossRow");
		simpleFramesLabel = View.Query<Label>("SimpleFramesLabel");
		simpleFrameTimeP95Label = View.Query<Label>("SimpleFrameTimeP95Label");
		simpleRoundTripTimeLabel = View.Query<Label>("SimpleRoundTripTimeLabel");
		simpleLossLabel = View.Query<Label>("SimpleLossLabel");
		networkingView = View.Query<VisualElement>("Networking");
		applicationSectionView = View.Query<VisualElement>("Application");
		applicationFramesLabel = View.Query<Label>("ApplicationFramesLabel");
		applicationFrameTimeAverageLabel = View.Query<Label>("ApplicationFrameTimeAverageLabel");
		applicationFrameTimeP95Label = View.Query<Label>("ApplicationFrameTimeP95Label");
		applicationFrameTimeMaxLabel = View.Query<Label>("ApplicationFrameTimeMaxLabel");
		networkSectionView = View.Query<VisualElement>("Network");
		networkSentWireBytesLabel = View.Query<Label>("NetworkSentWireBytesLabel");
		networkSentPacketsLabel = View.Query<Label>("NetworkSentPacketsLabel");
		networkReceivedWireBytesLabel = View.Query<Label>("NetworkReceivedWireBytesLabel");
		networkReceivedPacketsLabel = View.Query<Label>("NetworkReceivedPacketsLabel");
		networkRoundTripTimeLabel = View.Query<Label>("NetworkRoundTripTimeLabel");
		networkStallLabel = View.Query<Label>("NetworkStallLabel");
		objectSynchronizationSectionView = View.Query<VisualElement>("ObjectSynchronization");
		objectSynchronizationTicksLabel = View.Query<Label>("ObjectSynchronizationTicksLabel");
		objectSynchronizationTickDeltaAverageLabel = View.Query<Label>("ObjectSynchronizationTickDeltaAverageLabel");
		objectSynchronizationTickDeltaP95Label = View.Query<Label>("ObjectSynchronizationTickDeltaP95Label");
		objectSynchronizationTickDeltaMaxLabel = View.Query<Label>("ObjectSynchronizationTickDeltaMaxLabel");
		objectSynchronizationLossLabel = View.Query<Label>("ObjectSynchronizationLossLabel");
		objectSynchronizationReorderedLabel = View.Query<Label>("ObjectSynchronizationReorderedLabel");
		objectSynchronizationTimelineLabel = View.Query<Label>("ObjectSynchronizationTimelineLabel");
		objectSynchronizationSpeedLabel = View.Query<Label>("ObjectSynchronizationSpeedLabel");
		objectSynchronizationSnapsLabel = View.Query<Label>("ObjectSynchronizationSnapsLabel");
		objectSynchronizationExtrapolationLabel = View.Query<Label>("ObjectSynchronizationExtrapolationLabel");
		SetApplicationVisible(value: false);
		SetNetworkVisible(value: false);
		SetObjectSynchronizationVisible(value: false);
	}

	public void SetMode(DebugMode value)
	{
		debugMode = value;
		RefreshSectionsVisible();
	}

	public void SetSimpleApplication(string framesText, string frameTimeP95Text)
	{
		simpleFramesLabel.text = framesText;
		simpleFrameTimeP95Label.text = frameTimeP95Text;
	}

	public void SetSimpleRoundTripTime(string text)
	{
		simpleRoundTripTimeLabel.text = text;
	}

	public void SetSimpleLoss(string text)
	{
		simpleLossLabel.text = text;
	}

	public void SetApplicationVisible(bool value)
	{
		isApplicationVisible = value;
		applicationSectionView.style.display = ((!value) ? DisplayStyle.None : DisplayStyle.Flex);
		RefreshSectionsVisible();
	}

	public void SetApplicationFrames(string text)
	{
		applicationFramesLabel.text = text;
	}

	public void SetApplicationFrameTime(string averageText, string p95Text, string maxText)
	{
		applicationFrameTimeAverageLabel.text = averageText;
		applicationFrameTimeP95Label.text = p95Text;
		applicationFrameTimeMaxLabel.text = maxText;
	}

	public void SetNetworkVisible(bool value)
	{
		isNetworkVisible = value;
		networkSectionView.style.display = ((!value) ? DisplayStyle.None : DisplayStyle.Flex);
		RefreshSectionsVisible();
	}

	public void SetNetworkSent(string wireBytesText, string packetsText)
	{
		networkSentWireBytesLabel.text = wireBytesText;
		networkSentPacketsLabel.text = packetsText;
	}

	public void SetNetworkReceived(string wireBytesText, string packetsText)
	{
		networkReceivedWireBytesLabel.text = wireBytesText;
		networkReceivedPacketsLabel.text = packetsText;
	}

	public void SetNetworkRoundTripTime(string text)
	{
		networkRoundTripTimeLabel.text = text;
	}

	public void SetNetworkStall(string text)
	{
		networkStallLabel.text = text;
	}

	public void SetObjectSynchronizationVisible(bool value)
	{
		isObjectSynchronizationVisible = value;
		objectSynchronizationSectionView.style.display = ((!value) ? DisplayStyle.None : DisplayStyle.Flex);
		RefreshSectionsVisible();
	}

	private void RefreshSectionsVisible()
	{
		bool flag = isApplicationVisible || isNetworkVisible || isObjectSynchronizationVisible;
		bool flag2 = (debugMode == DebugMode.Advanced) & flag;
		simpleView.style.display = ((debugMode != DebugMode.Simple) ? DisplayStyle.None : DisplayStyle.Flex);
		networkingView.style.display = ((!flag2) ? DisplayStyle.None : DisplayStyle.Flex);
		simpleRoundTripTimeRow.style.display = ((!isNetworkVisible) ? DisplayStyle.None : DisplayStyle.Flex);
		simpleLossRow.style.display = ((!isObjectSynchronizationVisible) ? DisplayStyle.None : DisplayStyle.Flex);
	}

	public override bool Show()
	{
		if (SettingsManager.Debug == DebugMode.Off)
		{
			return false;
		}
		if (!base.Show())
		{
			return false;
		}
		return true;
	}

	public override bool Hide()
	{
		if (SettingsManager.Debug != DebugMode.Off)
		{
			return false;
		}
		if (!base.Hide())
		{
			return false;
		}
		return true;
	}

	public void SetBuild(string text)
	{
		buildLabel.text = text;
	}

	public void SetHint(string text)
	{
		hintLabel.text = text;
		simpleHintLabel.text = text;
		networkingHintLabel.text = text;
	}

	public void SetObjectSynchronizationTicks(string text)
	{
		objectSynchronizationTicksLabel.text = text;
	}

	public void SetObjectSynchronizationTickDelta(string averageText, string p95Text, string maxText)
	{
		objectSynchronizationTickDeltaAverageLabel.text = averageText;
		objectSynchronizationTickDeltaP95Label.text = p95Text;
		objectSynchronizationTickDeltaMaxLabel.text = maxText;
	}

	public void SetObjectSynchronizationLoss(string text)
	{
		objectSynchronizationLossLabel.text = text;
	}

	public void SetObjectSynchronizationReordered(string text)
	{
		objectSynchronizationReorderedLabel.text = text;
	}

	public void SetObjectSynchronizationTimeline(string text)
	{
		objectSynchronizationTimelineLabel.text = text;
	}

	public void SetObjectSynchronizationSpeed(string text)
	{
		objectSynchronizationSpeedLabel.text = text;
	}

	public void SetObjectSynchronizationSnaps(string text)
	{
		objectSynchronizationSnapsLabel.text = text;
	}

	public void SetObjectSynchronizationExtrapolation(string text)
	{
		objectSynchronizationExtrapolationLabel.text = text;
	}
}
