using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class ApplicationManagerController
{
	public static async Task Initialize()
	{
		if (ApplicationManager.IsDedicatedGameServer)
		{
			Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
		}
		else
		{
			await ApplicationManager.SetDisplay(SettingsManager.DisplayIndex);
			ApplicationManager.SetResolution(SettingsManager.ResolutionIndex, SettingsManager.FullScreenMode);
			ApplicationManager.SetVSync(SettingsManager.VSync);
			ApplicationManager.SetTargetFrameRate(SettingsManager.FpsLimit);
			ApplicationManager.SetQuality(SettingsManager.Quality);
			ApplicationManager.SetShadowQuality(SettingsManager.ShadowQuality);
			ApplicationManager.SetMouseVisibility(GlobalStateManager.UIState.IsMouseRequired);
		}
		EventManager.AddEventListener("Event_OnFullScreenModeChanged", Event_OnFullScreenModeChanged);
		EventManager.AddEventListener("Event_OnDisplayIndexChanged", Event_OnDisplayIndexChanged);
		EventManager.AddEventListener("Event_OnResolutionIndexChanged", Event_OnResolutionIndexChanged);
		EventManager.AddEventListener("Event_OnVSyncChanged", Event_OnVSyncChanged);
		EventManager.AddEventListener("Event_OnFpsLimitChanged", Event_OnFpsLimitChanged);
		EventManager.AddEventListener("Event_OnQualityChanged", Event_OnQualityChanged);
		EventManager.AddEventListener("Event_OnShadowQualityChanged", Event_OnShadowQualityChanged);
		EventManager.AddEventListener("Event_OnUIStateChanged", Event_OnUIStateChanged);
		EventManager.AddEventListener("Event_OnSocialClickDiscord", Event_OnSocialClickDiscord);
		EventManager.AddEventListener("Event_OnSocialClickPatreon", Event_OnSocialClickPatreon);
		EventManager.AddEventListener("Event_OnPopupClickOk", Event_OnPopupClickOk);
		EventManager.AddEventListener("Event_Server_OnServerStarted", Event_Server_OnServerStarted);
	}

	public static void Dispose()
	{
		EventManager.RemoveEventListener("Event_OnFullScreenModeChanged", Event_OnFullScreenModeChanged);
		EventManager.RemoveEventListener("Event_OnDisplayIndexChanged", Event_OnDisplayIndexChanged);
		EventManager.RemoveEventListener("Event_OnResolutionIndexChanged", Event_OnResolutionIndexChanged);
		EventManager.RemoveEventListener("Event_OnVSyncChanged", Event_OnVSyncChanged);
		EventManager.RemoveEventListener("Event_OnFpsLimitChanged", Event_OnFpsLimitChanged);
		EventManager.RemoveEventListener("Event_OnQualityChanged", Event_OnQualityChanged);
		EventManager.RemoveEventListener("Event_OnShadowQualityChanged", Event_OnShadowQualityChanged);
		EventManager.RemoveEventListener("Event_OnUIStateChanged", Event_OnUIStateChanged);
		EventManager.RemoveEventListener("Event_OnSocialClickDiscord", Event_OnSocialClickDiscord);
		EventManager.RemoveEventListener("Event_OnSocialClickPatreon", Event_OnSocialClickPatreon);
		EventManager.RemoveEventListener("Event_OnPopupClickOk", Event_OnPopupClickOk);
		EventManager.RemoveEventListener("Event_Server_OnServerStarted", Event_Server_OnServerStarted);
	}

	private static void Event_OnFullScreenModeChanged(Dictionary<string, object> message)
	{
		FullScreenMode mode = (FullScreenMode)message["value"];
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			ApplicationManager.SetResolution(SettingsManager.ResolutionIndex, mode);
		}
	}

	private static async void Event_OnDisplayIndexChanged(Dictionary<string, object> message)
	{
		int display = (int)message["value"];
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			await ApplicationManager.SetDisplay(display);
			ApplicationManager.SetResolution(SettingsManager.ResolutionIndex, SettingsManager.FullScreenMode);
		}
	}

	private static void Event_OnResolutionIndexChanged(Dictionary<string, object> message)
	{
		int index = (int)message["value"];
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			ApplicationManager.SetResolution(index, SettingsManager.FullScreenMode);
		}
	}

	private static void Event_OnVSyncChanged(Dictionary<string, object> message)
	{
		bool vSync = (bool)message["value"];
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			ApplicationManager.SetVSync(vSync);
		}
	}

	private static void Event_OnFpsLimitChanged(Dictionary<string, object> message)
	{
		int targetFrameRate = (int)message["value"];
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			ApplicationManager.SetTargetFrameRate(targetFrameRate);
		}
	}

	private static void Event_OnQualityChanged(Dictionary<string, object> message)
	{
		ApplicationQuality quality = (ApplicationQuality)message["value"];
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			ApplicationManager.SetQuality(quality);
		}
	}

	private static void Event_OnShadowQualityChanged(Dictionary<string, object> message)
	{
		ShadowQuality shadowQuality = (ShadowQuality)message["value"];
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			ApplicationManager.SetShadowQuality(shadowQuality);
		}
	}

	private static void Event_OnUIStateChanged(Dictionary<string, object> message)
	{
		UIState uIState = (UIState)message["oldUIState"];
		UIState uIState2 = (UIState)message["newUIState"];
		if (!ApplicationManager.IsDedicatedGameServer && uIState.IsMouseRequired != uIState2.IsMouseRequired)
		{
			ApplicationManager.SetMouseVisibility(uIState2.IsMouseRequired);
		}
	}

	private static void Event_OnSocialClickDiscord(Dictionary<string, object> message)
	{
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			Application.OpenURL("https://discord.gg/AZDBj6XsGg");
		}
	}

	private static void Event_OnSocialClickPatreon(Dictionary<string, object> message)
	{
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			Application.OpenURL("https://www.patreon.com/c/PuckGame");
		}
	}

	private static void Event_OnPopupClickOk(Dictionary<string, object> message)
	{
		string name = ((Popup)message["popup"]).Name;
		if (!(name == "mainMenuExitGame"))
		{
			if (name == "pauseMenuExitGame")
			{
				Application.Quit();
			}
		}
		else
		{
			Application.Quit();
		}
	}

	private static void Event_Server_OnServerStarted(Dictionary<string, object> message)
	{
		ServerConfig serverConfig = (ServerConfig)message["serverConfig"];
		if (ApplicationManager.IsDedicatedGameServer)
		{
			ApplicationManager.SetTargetFrameRate(serverConfig.tickRate);
		}
	}
}
