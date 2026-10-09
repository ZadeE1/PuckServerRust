using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class ConnectionManager : MonoBehaviourSingleton<ConnectionManager>
{
	private static readonly Logger Logger = new Logger("ConnectionManager");

	[HideInInspector]
	public UnityTransport UnityTransport;

	private void Start()
	{
		UnityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
		NetworkManager.Singleton.OnClientStarted += Client_OnClientStarted;
		NetworkManager.Singleton.OnClientStopped += Client_OnClientStopped;
		NetworkManager.Singleton.NetworkConfig.ProtocolVersion = ApplicationManager.Version;
	}

	private void OnDestroy()
	{
		if (NetworkManager.Singleton != null)
		{
			NetworkManager.Singleton.OnClientStarted -= Client_OnClientStarted;
			NetworkManager.Singleton.OnClientStopped -= Client_OnClientStopped;
		}
	}

	public void Client_StartClient(string ipAddress, ushort port, string password = null)
	{
		Logger.Info($"Starting client {ipAddress}:{port}");
		if (NetworkManager.Singleton.IsClient)
		{
			Connection value = new Connection
			{
				EndPoint = new EndPoint(ipAddress, port),
				Password = password
			};
			GlobalStateManager.SetConnectionState(new Dictionary<string, object> { { "pendingConnection", value } });
			Client_Disconnect();
			return;
		}
		string s = JsonSerializer.Serialize(new ConnectionData
		{
			SteamId = BackendManager.PlayerState.PlayerData.steamId,
			Key = BackendManager.PlayerState.Key,
			Password = password,
			EnabledModIds = ModManager.EnabledMods.Select((Mod mod) => mod.Id).ToArray(),
			Handedness = SettingsManager.Handedness,
			FlagID = SettingsManager.FlagID,
			HeadgearIDBlueAttacker = SettingsManager.HeadgearIDBlueAttacker,
			HeadgearIDRedAttacker = SettingsManager.HeadgearIDRedAttacker,
			HeadgearIDBlueGoalie = SettingsManager.HeadgearIDBlueGoalie,
			HeadgearIDRedGoalie = SettingsManager.HeadgearIDRedGoalie,
			MustacheID = SettingsManager.MustacheID,
			BeardID = SettingsManager.BeardID,
			JerseyIDBlueAttacker = SettingsManager.JerseyIDBlueAttacker,
			JerseyIDRedAttacker = SettingsManager.JerseyIDRedAttacker,
			JerseyIDBlueGoalie = SettingsManager.JerseyIDBlueGoalie,
			JerseyIDRedGoalie = SettingsManager.JerseyIDRedGoalie,
			StickSkinIDBlueAttacker = SettingsManager.StickSkinIDBlueAttacker,
			StickSkinIDRedAttacker = SettingsManager.StickSkinIDRedAttacker,
			StickSkinIDBlueGoalie = SettingsManager.StickSkinIDBlueGoalie,
			StickSkinIDRedGoalie = SettingsManager.StickSkinIDRedGoalie,
			StickShaftTapeIDBlueAttacker = SettingsManager.StickShaftTapeIDBlueAttacker,
			StickShaftTapeIDRedAttacker = SettingsManager.StickShaftTapeIDRedAttacker,
			StickShaftTapeIDBlueGoalie = SettingsManager.StickShaftTapeIDBlueGoalie,
			StickShaftTapeIDRedGoalie = SettingsManager.StickShaftTapeIDRedGoalie,
			StickBladeTapeIDBlueAttacker = SettingsManager.StickBladeTapeIDBlueAttacker,
			StickBladeTapeIDRedAttacker = SettingsManager.StickBladeTapeIDRedAttacker,
			StickBladeTapeIDBlueGoalie = SettingsManager.StickBladeTapeIDBlueGoalie,
			StickBladeTapeIDRedGoalie = SettingsManager.StickBladeTapeIDRedGoalie
		});
		NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(s);
		UnityTransport.SetConnectionData(ipAddress, port);
		Connection value2 = new Connection
		{
			EndPoint = new EndPoint(ipAddress, port),
			Password = password
		};
		GlobalStateManager.ClearReconnectionState();
		GlobalStateManager.SetConnectionState(new Dictionary<string, object>
		{
			{ "connection", value2 },
			{ "pendingConnection", null },
			{
				"phase",
				ConnectionPhase.Connecting
			}
		});
		NetworkManager.Singleton.StartClient();
	}

	private void Client_OnClientStarted()
	{
		EventManager.TriggerEvent("Event_OnClientStarted");
	}

	private void Client_OnClientStopped(bool wasHost)
	{
		StartCoroutine(DelayedOnClientStopped(wasHost));
	}

	private IEnumerator DelayedOnClientStopped(bool wasHost)
	{
		yield return new WaitForEndOfFrame();
		EventManager.TriggerEvent("Event_OnClientStopped", new Dictionary<string, object> { { "wasHost", wasHost } });
	}

	public void Client_Disconnect()
	{
		if (NetworkManager.Singleton.IsClient)
		{
			Logger.Info($"Puck ({ApplicationManager.Version}) network shutdown");
			NetworkManager.Singleton.Shutdown(discardMessageQueue: true);
		}
	}
}
