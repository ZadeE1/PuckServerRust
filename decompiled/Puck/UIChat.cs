using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public class UIChat : UIView
{
	private const float chatMessageMinHeight = 33f;

	[Header("References")]
	[SerializeField]
	private VisualTreeAsset messageAsset;

	[SerializeField]
	private VisualTreeAsset quickChatMessageAsset;

	private VisualElement chat;

	private VisualElement quickChat;

	private ScrollView scrollView;

	private VisualElement messages;

	private Label inputPrefix;

	private TextField textField;

	private string pendingOpenCharacter;

	private Label quickChatCategoryLabel;

	private VisualElement quickChatMessages;

	private List<UIChatMessage> uiChatMessages = new List<UIChatMessage>();

	private bool autoScroll = true;

	private bool isScrolling;

	private Tween smoothScrollTween;

	public bool IsTeamChat { get; private set; }

	public bool IsQuickChatVisible { get; private set; }

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("ChatView");
		chat = View.Query<VisualElement>("Chat");
		quickChat = View.Query<VisualElement>("QuickChat");
		scrollView = chat.Query<ScrollView>();
		messages = scrollView.Query<VisualElement>("Messages");
		inputPrefix = chat.Query<Label>("InputPrefix");
		textField = chat.Query<TextField>();
		quickChatCategoryLabel = quickChat.Query<Label>();
		quickChatMessages = quickChat.Query<VisualElement>("Messages");
		textField.RegisterCallback((NavigationSubmitEvent e) =>
		{
			SubmitMessage();
		}, TrickleDown.TrickleDown);
		textField.RegisterCallback((NavigationCancelEvent e) =>
		{
			StopInput();
		}, TrickleDown.TrickleDown);
		chat.RegisterCallback((FocusOutEvent e) =>
		{
			if (!UIUtils.GetVisualElementChildren(chat, recursive: true).Contains(e.relatedTarget))
			{
				StopInput();
			}
		}, TrickleDown.TrickleDown);
		scrollView.verticalScroller.valueChanged += (float value) =>
		{
			if (!isScrolling)
			{
				autoScroll = scrollView.verticalScroller.highValue - value <= 1f;
			}
		};
		UIUtils.GetVisualElementChildren(chat, recursive: true).ForEach((VisualElement visualElement) =>
		{
			visualElement.focusable = true;
		});
		ClearChatMessages();
		StopInput();
		HideQuickChat();
	}

	public override bool Show()
	{
		if (!SettingsManager.ShowGameUserInterface)
		{
			return false;
		}
		return base.Show();
	}

	public void StartInput(bool isTeamChat = false, string openCharacter = null)
	{
		IsFocused = true;
		IsTeamChat = isTeamChat;
		ShowTextField();
		SwallowOpenCharacter(openCharacter);
		uiChatMessages.ForEach((UIChatMessage uiChatMessage) =>
		{
			uiChatMessage.Focus();
		});
	}

	public void StopInput()
	{
		IsFocused = false;
		IsTeamChat = false;
		HideTextField();
		uiChatMessages.ForEach((UIChatMessage uiChatMessage) =>
		{
			uiChatMessage.Blur();
		});
	}

	private void ShowTextField()
	{
		scrollView.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
		inputPrefix.style.display = ((!IsTeamChat) ? DisplayStyle.None : DisplayStyle.Flex);
		textField.style.opacity = 1f;
		textField.pickingMode = PickingMode.Position;
		textField.value = string.Empty;
		textField.Focus();
	}

	private void HideTextField()
	{
		scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
		inputPrefix.style.display = DisplayStyle.None;
		textField.style.opacity = 0f;
		textField.pickingMode = PickingMode.Ignore;
		textField.value = string.Empty;
		textField.Blur();
		textField.UnregisterCallback<ChangeEvent<string>>(OnTextFieldFirstChange);
		pendingOpenCharacter = null;
	}

	private void SwallowOpenCharacter(string openCharacter)
	{
		if (!string.IsNullOrEmpty(openCharacter))
		{
			pendingOpenCharacter = openCharacter;
			textField.RegisterCallback<ChangeEvent<string>>(OnTextFieldFirstChange);
		}
	}

	private void OnTextFieldFirstChange(ChangeEvent<string> e)
	{
		textField.UnregisterCallback<ChangeEvent<string>>(OnTextFieldFirstChange);
		if (string.Equals(e.newValue, pendingOpenCharacter, StringComparison.OrdinalIgnoreCase))
		{
			textField.SetValueWithoutNotify(string.Empty);
		}
		pendingOpenCharacter = null;
	}

	public void ShowQuickChat(QuickChatCategory category, QuickChat[] quickChats)
	{
		quickChatCategoryLabel.text = category.ToString().ToUpper();
		for (int i = 0; i < quickChats.Length; i++)
		{
			VisualElement visualElement = quickChatMessageAsset.Instantiate();
			Label label = visualElement.Query<Label>();
			string content = quickChats[i].Content;
			label.text = $"{i + 1}. {content}";
			quickChatMessages.Add(visualElement);
		}
		quickChat.style.display = DisplayStyle.Flex;
		IsQuickChatVisible = true;
	}

	public void HideQuickChat()
	{
		quickChat.style.display = DisplayStyle.None;
		quickChatMessages.Clear();
		IsQuickChatVisible = false;
	}

	private void PinLabelHeight(Label label)
	{
		float width = label.contentRect.width;
		if (!(width <= 1f))
		{
			float num = label.MeasureTextSize(label.text, width, VisualElement.MeasureMode.Exactly, 0f, VisualElement.MeasureMode.Undefined).y + label.resolvedStyle.paddingTop + label.resolvedStyle.paddingBottom;
			if (!(num <= 33f) && !Mathf.Approximately(label.resolvedStyle.height, num))
			{
				label.style.height = num;
			}
		}
	}

	private void RefreshContentAndScroll()
	{
		float num = 0f;
		for (int i = 0; i < messages.childCount; i++)
		{
			num += messages[i].resolvedStyle.height;
		}
		if (float.IsNaN(num))
		{
			return;
		}
		if (!autoScroll)
		{
			if (!Mathf.Approximately(messages.resolvedStyle.height, num))
			{
				messages.style.height = num;
			}
			return;
		}
		isScrolling = true;
		if (!Mathf.Approximately(messages.resolvedStyle.height, num))
		{
			messages.style.height = num;
		}
		float num2 = Mathf.Max(0f, num - scrollView.contentViewport.resolvedStyle.height);
		scrollView.verticalScroller.highValue = num2;
		Vector2 endValue = new Vector2(scrollView.scrollOffset.x, num2);
		smoothScrollTween?.Kill();
		smoothScrollTween = DOTween.To(() => scrollView.scrollOffset, (Vector2 x) =>
		{
			scrollView.scrollOffset = x;
		}, endValue, 0.2f).OnComplete(() =>
		{
			isScrolling = false;
		}).SetEase(Ease.Linear);
	}

	private void SubmitMessage()
	{
		EventManager.TriggerEvent("Event_OnChatSubmitMessage", new Dictionary<string, object>
		{
			{ "content", textField.value },
			{ "isTeamChat", IsTeamChat }
		});
		StopInput();
	}

	public void AddChatMessage(ChatMessage chatMessage, Units units, bool filterProfanity)
	{
		VisualElement visualElement = messageAsset.Instantiate();
		visualElement.focusable = true;
		Label label = visualElement.Query<Label>();
		label.focusable = true;
		string chatMessagePrefix = GetChatMessagePrefix(chatMessage);
		string text = ParseChatContent(chatMessage.Content.ToString(), chatMessage.IsSystem, units, filterProfanity);
		label.text = chatMessagePrefix + text;
		label.style.display = ((text.Length <= 0) ? DisplayStyle.None : DisplayStyle.Flex);
		label.RegisterCallback<GeometryChangedEvent>(OnRowGeometryChanged);
		UIChatMessage uIChatMessage = new UIChatMessage(chatMessage, visualElement);
		messages.Add(uIChatMessage.VisualElement);
		uiChatMessages.Add(uIChatMessage);
		void OnRowGeometryChanged(GeometryChangedEvent e)
		{
			PinLabelHeight(label);
			RefreshContentAndScroll();
		}
	}

	public void RemoveChatMessage(ChatMessage chatMessage)
	{
		UIChatMessage uIChatMessage = uiChatMessages.FirstOrDefault((UIChatMessage m) => m.ChatMessage == chatMessage);
		if (uIChatMessage != null)
		{
			float height = uIChatMessage.VisualElement.resolvedStyle.height;
			uIChatMessage.Dispose();
			messages.Remove(uIChatMessage.VisualElement);
			uiChatMessages.Remove(uIChatMessage);
			if (!autoScroll)
			{
				scrollView.scrollOffset = new Vector2(scrollView.scrollOffset.x, Mathf.Max(0f, scrollView.scrollOffset.y - height));
			}
			scrollView.schedule.Execute(RefreshContentAndScroll);
		}
	}

	public void ClearChatMessages()
	{
		uiChatMessages.ForEach((UIChatMessage uiChatMessage) =>
		{
			uiChatMessage.Dispose();
		});
		messages.Clear();
		uiChatMessages.Clear();
		smoothScrollTween?.Kill();
		autoScroll = true;
		isScrolling = false;
	}

	public void SetOpacity(float opacity)
	{
		chat.style.opacity = new StyleFloat(opacity);
	}

	public void SetScale(float scale)
	{
		chat.style.scale = new StyleScale(new Scale(new Vector2(scale, scale)));
	}

	private string GetChatMessagePrefix(ChatMessage chatMessage)
	{
		string text = string.Empty;
		if (!chatMessage.IsSystem)
		{
			if (chatMessage.IsTeamChat)
			{
				text += "[TEAM] ";
			}
			string text2 = StringUtils.WrapInTeamColor(chatMessage.Username.ToString(), chatMessage.Team.Value);
			text = text + text2 + ": ";
		}
		return text;
	}

	private string ParseChatContent(string content, bool isSystem, Units units, bool filterProfanity)
	{
		if (isSystem)
		{
			content = Regex.Replace(content, "<united>([^<]+)</united>", (Match match) =>
			{
				string value = match.Groups[1].Value;
				float result;
				return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result) ? ((units == Units.Metric) ? Utils.GameUnitsToMetric(result) : Utils.GameUnitsToImperial(result)).ToString("F1", CultureInfo.InvariantCulture) : value;
			});
			content = Regex.Replace(content, "&units", (units == Units.Metric) ? "KPH" : "MPH");
		}
		else
		{
			content = StringUtils.FilterStringRichText(content);
			content = StringUtils.FilterStringSpecialCharacters(content, Constants.CHAT_WHITELIST, filterProfanity ? Constants.CHAT_BLACKLIST : null);
			if (filterProfanity)
			{
				content = StringUtils.FilterStringProfanity(content, replaceWithStars: true);
			}
		}
		return content;
	}
}
