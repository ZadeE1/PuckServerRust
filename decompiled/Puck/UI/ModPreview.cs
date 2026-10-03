using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI;

[UxmlElement]
public class ModPreview : VisualElement
{
	[Serializable]
	[CompilerGenerated]
	public new class UxmlSerializedData : VisualElement.UxmlSerializedData
	{
		[SerializeField]
		private bool IsStatisticsVisible;

		[SerializeField]
		private int Subscriptions;

		[SerializeField]
		private int Upvotes;

		[SerializeField]
		private int Downvotes;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags IsStatisticsVisible_UxmlAttributeFlags;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags Subscriptions_UxmlAttributeFlags;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags Upvotes_UxmlAttributeFlags;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags Downvotes_UxmlAttributeFlags;

		[RegisterUxmlCache]
		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
			UxmlDescriptionCache.RegisterType(typeof(UxmlSerializedData), new UxmlAttributeNames[4]
			{
				new UxmlAttributeNames("IsStatisticsVisible", "is-statistics-visible", null),
				new UxmlAttributeNames("Subscriptions", "subscriptions", null),
				new UxmlAttributeNames("Upvotes", "upvotes", null),
				new UxmlAttributeNames("Downvotes", "downvotes", null)
			});
		}

		public override object CreateInstance()
		{
			return new ModPreview();
		}

		public override void Deserialize(object obj)
		{
			base.Deserialize(obj);
			ModPreview modPreview = (ModPreview)obj;
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(IsStatisticsVisible_UxmlAttributeFlags))
			{
				modPreview.IsStatisticsVisible = IsStatisticsVisible;
			}
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(Subscriptions_UxmlAttributeFlags))
			{
				modPreview.Subscriptions = Subscriptions;
			}
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(Upvotes_UxmlAttributeFlags))
			{
				modPreview.Upvotes = Upvotes;
			}
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(Downvotes_UxmlAttributeFlags))
			{
				modPreview.Downvotes = Downvotes;
			}
		}
	}

	private bool isStatisticsVisible = true;

	private int subscriptions;

	private int upvotes;

	private int downvotes;

	public Action Ready;

	private Link link;

	private VisualElement statistics;

	private Label subscriptionsLabel;

	private Label upvotesLabel;

	private Label downvotesLabel;

	[UxmlAttribute]
	public bool IsStatisticsVisible
	{
		get
		{
			return isStatisticsVisible;
		}
		set
		{
			if (isStatisticsVisible != value)
			{
				isStatisticsVisible = value;
				Update();
			}
		}
	}

	[UxmlAttribute]
	public int Subscriptions
	{
		get
		{
			return subscriptions;
		}
		set
		{
			if (subscriptions != value)
			{
				subscriptions = value;
				Update();
			}
		}
	}

	[UxmlAttribute]
	public int Upvotes
	{
		get
		{
			return upvotes;
		}
		set
		{
			if (upvotes != value)
			{
				upvotes = value;
				Update();
			}
		}
	}

	[UxmlAttribute]
	public int Downvotes
	{
		get
		{
			return downvotes;
		}
		set
		{
			if (downvotes != value)
			{
				downvotes = value;
				Update();
			}
		}
	}

	public Link Link => link;

	public ModPreview()
	{
		RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
		RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
	}

	private void Update()
	{
		if (statistics != null)
		{
			statistics.style.display = ((!IsStatisticsVisible) ? DisplayStyle.None : DisplayStyle.Flex);
		}
		if (subscriptionsLabel != null)
		{
			subscriptionsLabel.text = Subscriptions.ToString();
		}
		if (upvotesLabel != null)
		{
			upvotesLabel.text = Upvotes.ToString();
		}
		if (downvotesLabel != null)
		{
			downvotesLabel.text = Downvotes.ToString();
		}
	}

	private void OnAttachToPanel(AttachToPanelEvent e)
	{
		link = this.Query<Link>("Link");
		statistics = this.Query("Statistics");
		subscriptionsLabel = statistics.Query<VisualElement>("Subscriptions").First().Query<Label>();
		upvotesLabel = statistics.Query<VisualElement>("Upvotes").First().Query<Label>();
		downvotesLabel = statistics.Query<VisualElement>("Downvotes").First().Query<Label>();
		Update();
		Utils.WhenAllActions(() =>
		{
			Ready?.Invoke();
		}, (Action a) =>
		{
			Link link = this.link;
			link.Ready = (Action)Delegate.Combine(link.Ready, a);
		});
	}

	private void OnDetachFromPanel(DetachFromPanelEvent e)
	{
	}
}
