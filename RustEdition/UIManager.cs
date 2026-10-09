using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviourSingleton<UIManager>
{
	[Header("References")]
	[SerializeField]
	private AudioClip selectAudioClip;

	[SerializeField]
	private AudioClip clickAudioClip;

	[SerializeField]
	private AudioClip notificationAudioClip;

	[SerializeField]
	private AudioClip whooshAudioClip;

	[SerializeField]
	private AudioClip tickAudioClip;

	[HideInInspector]
	public UIDocument UIDocument;

	[HideInInspector]
	public AudioSource AudioSource;

	[HideInInspector]
	public UIMainMenu MainMenu;

	[HideInInspector]
	public UIPauseMenu PauseMenu;

	[HideInInspector]
	public UIServerBrowser ServerBrowser;

	[HideInInspector]
	public UIGameState GameState;

	[HideInInspector]
	public UIChat Chat;

	[HideInInspector]
	public UITeamSelect TeamSelect;

	[HideInInspector]
	public UIPositionSelect PositionSelect;

	[HideInInspector]
	public UIScoreboard Scoreboard;

	[HideInInspector]
	public UISettings Settings;

	[HideInInspector]
	public UIHUD Hud;

	[HideInInspector]
	public UIAnnouncements Announcements;

	[HideInInspector]
	public UIMinimap Minimap;

	[HideInInspector]
	public UINewServer NewServer;

	[HideInInspector]
	public UIDirectConnect DirectConnect;

	[HideInInspector]
	public UIToastManager ToastManager;

	[HideInInspector]
	public UIOverlayManager OverlayManager;

	[HideInInspector]
	public UIPlayerMenu PlayerMenu;

	[HideInInspector]
	public UIIdentity Identity;

	[HideInInspector]
	public UIAppearance Appearance;

	[HideInInspector]
	public UIPopupManager PopupManager;

	[HideInInspector]
	public UIUsernames Usernames;

	[HideInInspector]
	public UIDebug Debug;

	[HideInInspector]
	public UIMods Mods;

	[HideInInspector]
	public UIFooter Footer;

	[HideInInspector]
	public UIFriends Friends;

	[HideInInspector]
	public UIPlay Play;

	[HideInInspector]
	public UIMatchmaking Matchmaking;

	private List<UIView> views = new List<UIView>();

	private Vector2 lastPointerPosition = Vector2.zero;

	[HideInInspector]
	public PanelSettings PanelSettings => UIDocument.panelSettings;

	[HideInInspector]
	public VisualElement RootVisualElement => UIDocument.rootVisualElement;

	public override void Awake()
	{
		base.Awake();
		UIDocument = GetComponent<UIDocument>();
		AudioSource = GetComponent<AudioSource>();
		RootVisualElement.style.display = (ApplicationManager.IsDedicatedGameServer ? DisplayStyle.None : DisplayStyle.Flex);
		RootVisualElement.RegisterCallback((PointerEnterEvent e) =>
		{
			VisualElement visualElement = e.target as VisualElement;
			bool num = visualElement != null && visualElement is Button && visualElement.enabledInHierarchy;
			bool flag = visualElement != null && visualElement.name == "unity-tab__header";
			if (num | flag)
			{
				PlaySelectSound();
			}
		}, TrickleDown.TrickleDown);
		RootVisualElement.RegisterCallback((PointerCaptureOutEvent e) =>
		{
			if (e.target is VisualElement visualElement && visualElement is Button && visualElement.enabledInHierarchy)
			{
				PlayClickSound();
			}
		}, TrickleDown.TrickleDown);
		RootVisualElement.RegisterCallback((PointerDownEvent e) =>
		{
			if (e.target is VisualElement visualElement && visualElement.name.Contains("unity-tab__header"))
			{
				PlayClickSound();
			}
		}, TrickleDown.TrickleDown);
		MainMenu = gameObject.GetComponent<UIMainMenu>();
		MainMenu.Initialize(RootVisualElement);
		views.Add(MainMenu);
		PauseMenu = gameObject.GetComponent<UIPauseMenu>();
		PauseMenu.Initialize(RootVisualElement);
		views.Add(PauseMenu);
		ServerBrowser = gameObject.GetComponent<UIServerBrowser>();
		ServerBrowser.Initialize(RootVisualElement);
		views.Add(ServerBrowser);
		GameState = gameObject.GetComponent<UIGameState>();
		GameState.Initialize(RootVisualElement);
		views.Add(GameState);
		Chat = gameObject.GetComponent<UIChat>();
		Chat.Initialize(RootVisualElement);
		views.Add(Chat);
		TeamSelect = gameObject.GetComponent<UITeamSelect>();
		TeamSelect.Initialize(RootVisualElement);
		views.Add(TeamSelect);
		PositionSelect = gameObject.GetComponent<UIPositionSelect>();
		PositionSelect.Initialize(RootVisualElement);
		views.Add(PositionSelect);
		Scoreboard = gameObject.GetComponent<UIScoreboard>();
		Scoreboard.Initialize(RootVisualElement);
		views.Add(Scoreboard);
		Settings = gameObject.GetComponent<UISettings>();
		Settings.Initialize(RootVisualElement);
		views.Add(Settings);
		Hud = gameObject.GetComponent<UIHUD>();
		Hud.Initialize(RootVisualElement);
		views.Add(Hud);
		Announcements = gameObject.GetComponent<UIAnnouncements>();
		Announcements.Initialize(RootVisualElement);
		views.Add(Announcements);
		Minimap = gameObject.GetComponent<UIMinimap>();
		Minimap.Initialize(RootVisualElement);
		views.Add(Minimap);
		NewServer = gameObject.GetComponent<UINewServer>();
		NewServer.Initialize(RootVisualElement);
		views.Add(NewServer);
		DirectConnect = gameObject.GetComponent<UIDirectConnect>();
		DirectConnect.Initialize(RootVisualElement);
		views.Add(DirectConnect);
		ToastManager = gameObject.GetComponent<UIToastManager>();
		ToastManager.Initialize(RootVisualElement);
		views.Add(ToastManager);
		OverlayManager = gameObject.GetComponent<UIOverlayManager>();
		OverlayManager.Initialize(RootVisualElement);
		views.Add(OverlayManager);
		PlayerMenu = gameObject.GetComponent<UIPlayerMenu>();
		PlayerMenu.Initialize(RootVisualElement);
		views.Add(PlayerMenu);
		Identity = gameObject.GetComponent<UIIdentity>();
		Identity.Initialize(RootVisualElement);
		views.Add(Identity);
		Appearance = gameObject.GetComponent<UIAppearance>();
		Appearance.Initialize(RootVisualElement);
		views.Add(Appearance);
		PopupManager = gameObject.GetComponent<UIPopupManager>();
		PopupManager.Initialize(RootVisualElement);
		views.Add(PopupManager);
		Usernames = gameObject.GetComponent<UIUsernames>();
		Usernames.Initialize(RootVisualElement);
		views.Add(Usernames);
		Debug = gameObject.GetComponent<UIDebug>();
		Debug.Initialize(RootVisualElement);
		views.Add(Debug);
		Mods = gameObject.GetComponent<UIMods>();
		Mods.Initialize(RootVisualElement);
		views.Add(Mods);
		Footer = gameObject.GetComponent<UIFooter>();
		Footer.Initialize(RootVisualElement);
		views.Add(Footer);
		Friends = gameObject.GetComponent<UIFriends>();
		Friends.Initialize(RootVisualElement);
		views.Add(Friends);
		Play = gameObject.GetComponent<UIPlay>();
		Play.Initialize(RootVisualElement);
		views.Add(Play);
		Matchmaking = gameObject.GetComponent<UIMatchmaking>();
		Matchmaking.Initialize(RootVisualElement);
		views.Add(Matchmaking);
		foreach (UIView view in views)
		{
			view.OnVisibility = (Action<UIView>)Delegate.Combine(view.OnVisibility, new Action<UIView>(OnViewVisibilityChanged));
			view.OnFocus = (Action<UIView>)Delegate.Combine(view.OnFocus, new Action<UIView>(OnViewFocusChanged));
		}
		InputManager.PauseAction.performed += OnPauseActionPerformed;
		InputManager.AllChatAction.performed += OnAllChatActionPerformed;
		InputManager.TeamChatAction.performed += OnTeamChatActionPerformed;
		InputManager.ScoreboardAction.started += OnScoreboardActionStarted;
		InputManager.ScoreboardAction.canceled += OnScoreboardActionCanceled;
	}

	private void OnDestroy()
	{
		foreach (UIView view in views)
		{
			view.OnVisibility = (Action<UIView>)Delegate.Remove(view.OnVisibility, new Action<UIView>(OnViewVisibilityChanged));
			view.OnFocus = (Action<UIView>)Delegate.Remove(view.OnFocus, new Action<UIView>(OnViewFocusChanged));
		}
		InputManager.PauseAction.performed -= OnPauseActionPerformed;
		InputManager.AllChatAction.performed -= OnAllChatActionPerformed;
		InputManager.TeamChatAction.performed -= OnTeamChatActionPerformed;
		InputManager.ScoreboardAction.started -= OnScoreboardActionStarted;
		InputManager.ScoreboardAction.canceled -= OnScoreboardActionCanceled;
	}

	private void Update()
	{
		if (!ApplicationManager.IsDedicatedGameServer && GlobalStateManager.UIState.IsMouseRequired)
		{
			CheckMouseOverUI();
		}
	}

	private void HideAllViews()
	{
		foreach (UIView view in views)
		{
			view.Hide();
		}
	}

	public void ShowPhaseViews(UIPhase phase)
	{
		HideAllViews();
		switch (phase)
		{
		case UIPhase.LockerRoom:
			Chat.Show();
			MainMenu.Show();
			Footer.Show();
			break;
		case UIPhase.Playing:
			Chat.Show();
			GameState.Show();
			Announcements.Show();
			Usernames.Show();
			break;
		case UIPhase.None:
			break;
		}
	}

	public void SetUIScale(float value)
	{
		PanelSettings.scale = value;
	}

	public void PlaySelectSound()
	{
		if (selectAudioClip != null)
		{
			AudioSource.PlayOneShot(selectAudioClip);
		}
	}

	public void PlayClickSound()
	{
		if (clickAudioClip != null)
		{
			AudioSource.PlayOneShot(clickAudioClip);
		}
	}

	public void PlayNotificationSound()
	{
		if (notificationAudioClip != null)
		{
			AudioSource.PlayOneShot(notificationAudioClip);
		}
	}

	public void PlayWhooshSound()
	{
		if (whooshAudioClip != null)
		{
			AudioSource.PlayOneShot(whooshAudioClip);
		}
	}

	public void PlayTickSound()
	{
		if (tickAudioClip != null)
		{
			AudioSource.PlayOneShot(tickAudioClip);
		}
	}

	private void CheckMouseRequirement()
	{
		foreach (UIView view in views)
		{
			if ((view.VisibilityRequiresMouse && view.IsVisible) || (view.FocusRequiresMouse && view.IsFocused))
			{
				GlobalStateManager.SetUIState(new Dictionary<string, object> { { "isMouseRequired", true } });
				return;
			}
		}
		GlobalStateManager.SetUIState(new Dictionary<string, object> { { "isMouseRequired", false } });
	}

	private void CheckInteraction()
	{
		List<UIView> list = new List<UIView>();
		foreach (UIView view in views)
		{
			if ((view.VisibilityIsInteractive && view.IsVisible) || (view.FocusIsInteractive && view.IsFocused))
			{
				list.Add(view);
			}
		}
		GlobalStateManager.SetUIState(new Dictionary<string, object> { { "interactingViews", list } });
	}

	private void CheckMouseOverUI()
	{
		if (RootVisualElement == null)
		{
			return;
		}
		IPanel panel = RootVisualElement.panel;
		if (panel == null)
		{
			return;
		}
		Vector2 vector = InputManager.PointAction.ReadValue<Vector2>();
		vector.y = (float)Screen.height - vector.y;
		if (!(vector == lastPointerPosition))
		{
			lastPointerPosition = vector;
			Vector2 point = RuntimePanelUtils.ScreenToPanel(panel, vector);
			bool flag = panel.Pick(point) != null;
			if (flag != GlobalStateManager.UIState.IsMouseOverUI)
			{
				GlobalStateManager.SetUIState(new Dictionary<string, object> { { "isMouseOverUI", flag } });
			}
		}
	}

	private void OnViewVisibilityChanged(UIView uiView)
	{
		if (uiView.VisibilityRequiresMouse)
		{
			CheckMouseRequirement();
		}
		if (uiView.VisibilityIsInteractive)
		{
			CheckInteraction();
		}
	}

	private void OnViewFocusChanged(UIView uiView)
	{
		if (uiView.FocusRequiresMouse)
		{
			CheckMouseRequirement();
		}
		if (uiView.FocusIsInteractive)
		{
			CheckInteraction();
		}
	}

	private UIView GetTopmostBlockingInteractingView()
	{
		List<UIView> interactingViews = GlobalStateManager.UIState.InteractingViews;
		for (int num = interactingViews.Count - 1; num >= 0; num--)
		{
			UIView uIView = interactingViews[num];
			if (!(uIView == TeamSelect) && !(uIView == PositionSelect) && !(uIView == Footer))
			{
				return uIView;
			}
		}
		return null;
	}

	private void OnPauseActionPerformed(InputAction.CallbackContext context)
	{
		UIPhase phase = GlobalStateManager.UIState.Phase;
		if (phase == UIPhase.Playing)
		{
			if (Chat.IsFocused)
			{
				Chat.StopInput();
				return;
			}
			if (Chat.IsQuickChatVisible)
			{
				EventManager.TriggerEvent("Event_OnQuickChatCancel");
				return;
			}
		}
		if (Friends.IsVisible)
		{
			EventManager.TriggerEvent("Event_OnFriendsClickClose");
			return;
		}
		UIView topmostBlockingInteractingView = GetTopmostBlockingInteractingView();
		if (!TryCloseSecondaryView(topmostBlockingInteractingView) && phase == UIPhase.Playing)
		{
			if (PauseMenu.IsVisible && topmostBlockingInteractingView is UIPauseMenu)
			{
				PauseMenu.Hide();
			}
			else if (!PauseMenu.IsVisible)
			{
				PauseMenu.Show();
			}
		}
	}

	private bool TryCloseSecondaryView(UIView view)
	{
		if (!(view is UISettings))
		{
			if (!(view is UINewServer))
			{
				if (!(view is UIDirectConnect))
				{
					if (!(view is UIServerBrowser))
					{
						if (!(view is UIPlay))
						{
							if (!(view is UIPlayerMenu))
							{
								if (!(view is UIIdentity))
								{
									if (!(view is UIAppearance))
									{
										if (view is UIMods)
										{
											EventManager.TriggerEvent("Event_OnModsClickClose");
											return true;
										}
										return false;
									}
									EventManager.TriggerEvent("Event_OnAppearanceClickClose");
									return true;
								}
								EventManager.TriggerEvent("Event_OnIdentityClickClose");
								return true;
							}
							EventManager.TriggerEvent("Event_OnPlayerMenuClickBack");
							return true;
						}
						EventManager.TriggerEvent("Event_OnPlayClickClose");
						return true;
					}
					EventManager.TriggerEvent("Event_OnServerBrowserClickClose");
					return true;
				}
				EventManager.TriggerEvent("Event_OnDirectConnectClickClose");
				return true;
			}
			EventManager.TriggerEvent("Event_OnNewServerClickClose");
			return true;
		}
		EventManager.TriggerEvent("Event_OnSettingsClickClose");
		return true;
	}

	private void OnAllChatActionPerformed(InputAction.CallbackContext context)
	{
		if (GlobalStateManager.UIState.Phase == UIPhase.Playing && !(GetTopmostBlockingInteractingView() != null))
		{
			Chat.StartInput(isTeamChat: false, GetChatOpenCharacter(context));
		}
	}

	private void OnTeamChatActionPerformed(InputAction.CallbackContext context)
	{
		if (GlobalStateManager.UIState.Phase == UIPhase.Playing && !(GetTopmostBlockingInteractingView() != null))
		{
			Chat.StartInput(isTeamChat: true, GetChatOpenCharacter(context));
		}
	}

	private static string GetChatOpenCharacter(InputAction.CallbackContext context)
	{
		if (context.control == null || !context.ReadValueAsButton())
		{
			return null;
		}
		return context.control.displayName;
	}

	private void OnScoreboardActionStarted(InputAction.CallbackContext context)
	{
		if (GlobalStateManager.UIState.Phase == UIPhase.Playing && !(GetTopmostBlockingInteractingView() != null))
		{
			Scoreboard.Show();
		}
	}

	private void OnScoreboardActionCanceled(InputAction.CallbackContext context)
	{
		Scoreboard.Hide();
	}
}
