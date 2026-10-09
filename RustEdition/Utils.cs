using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class Utils
{
	private static readonly Logger uPnPLogger = new Logger("uPnPHelper");

	public static string GetReadableTypeName(Type type)
	{
		return type.Name.Split('`')[0];
	}

	public static float WrapEulerAngle(float angle)
	{
		angle %= 360f;
		if (angle > 180f)
		{
			angle -= 360f;
		}
		if (angle < -180f)
		{
			angle += 360f;
		}
		return angle;
	}

	public static Vector3 WrapEulerAngles(Vector3 eulerAngles)
	{
		eulerAngles.x %= 360f;
		if (eulerAngles.x > 180f)
		{
			eulerAngles.x -= 360f;
		}
		if (eulerAngles.x < -180f)
		{
			eulerAngles.x += 360f;
		}
		eulerAngles.y %= 360f;
		if (eulerAngles.y > 180f)
		{
			eulerAngles.y -= 360f;
		}
		if (eulerAngles.y < -180f)
		{
			eulerAngles.y += 360f;
		}
		eulerAngles.z %= 360f;
		if (eulerAngles.z > 180f)
		{
			eulerAngles.z -= 360f;
		}
		if (eulerAngles.z < -180f)
		{
			eulerAngles.z += 360f;
		}
		return eulerAngles;
	}

	public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
	{
		Vector3 vector = point - pivot;
		vector = Quaternion.Euler(angles) * vector;
		point = vector + pivot;
		return point;
	}

	public static Vector3 Vector2Clamp(Vector2 value, Vector2 min, Vector2 max)
	{
		return new Vector3(Mathf.Clamp(value.x, min.x, max.x), Mathf.Clamp(value.y, min.y, max.y));
	}

	public static Vector3 Vector3Clamp(Vector3 value, Vector3 min, Vector3 max)
	{
		return new Vector3(Mathf.Clamp(value.x, min.x, max.x), Mathf.Clamp(value.y, min.y, max.y), Mathf.Clamp(value.z, min.z, max.z));
	}

	public static Vector3 Vector3Abs(Vector3 value)
	{
		return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
	}

	public static Vector3 Vector3Slerp3(Vector3 a, Vector3 b, Vector3 c, float t)
	{
		if (t <= 0f)
		{
			return Vector3.Slerp(a, b, t + 1f);
		}
		return Vector3.Slerp(b, c, t);
	}

	public static float Map(float value, float from1, float to1, float from2, float to2)
	{
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}

	public static Quaternion GetLocalLookRotation(Transform transform, Vector3 target)
	{
		if (transform.parent == null)
		{
			return Quaternion.LookRotation(target - transform.position);
		}
		Quaternion quaternion = Quaternion.LookRotation(target - transform.position);
		return Quaternion.Inverse(transform.parent.rotation) * quaternion;
	}

	public static float GameUnitsToMetric(float value)
	{
		return value * 3.6f;
	}

	public static float GameUnitsToImperial(float value)
	{
		return value * 2.2369363f;
	}

	public static float GetCollisionForce(Collision collision)
	{
		if (collision == null)
		{
			return 0f;
		}
		float result = 0f;
		if (collision.contactCount > 0)
		{
			result = Vector3.Dot(collision.GetContact(0).normal, collision.relativeVelocity.normalized) * collision.relativeVelocity.magnitude;
		}
		return result;
	}

	public static void SetRigidbodyCollisionDetectionMode(Rigidbody rigidbody, CollisionDetectionMode mode)
	{
		if (!(rigidbody == null) && rigidbody.collisionDetectionMode != mode)
		{
			bool isKinematic = rigidbody.isKinematic;
			Vector3 linearVelocity = rigidbody.linearVelocity;
			Vector3 angularVelocity = rigidbody.angularVelocity;
			rigidbody.collisionDetectionMode = mode;
			rigidbody.isKinematic = true;
			rigidbody.isKinematic = false;
			rigidbody.isKinematic = isKinematic;
			rigidbody.linearVelocity = linearVelocity;
			rigidbody.angularVelocity = angularVelocity;
		}
	}

	public static List<string> GetTeamNames()
	{
		return new List<string> { "BLUE", "RED" };
	}

	public static PlayerTeam GetTeamFromName(string name)
	{
		if (!(name == "BLUE"))
		{
			if (name == "RED")
			{
				return PlayerTeam.Red;
			}
			return PlayerTeam.Blue;
		}
		return PlayerTeam.Blue;
	}

	public static string GetNameFromTeam(PlayerTeam team)
	{
		return team switch
		{
			PlayerTeam.Blue => "BLUE", 
			PlayerTeam.Red => "RED", 
			_ => "UNKNOWN", 
		};
	}

	public static Color GetTeamColor(PlayerTeam team)
	{
		ColorUtility.TryParseHtmlString(team switch
		{
			PlayerTeam.Blue => "#3b82f6", 
			PlayerTeam.Red => "#d13333", 
			_ => "#404040", 
		}, out var color);
		return color;
	}

	public static List<string> GetRoleNames()
	{
		return new List<string> { "SKATER", "GOALIE" };
	}

	public static PlayerRole GetRoleFromName(string name)
	{
		if (!(name == "SKATER"))
		{
			if (name == "GOALIE")
			{
				return PlayerRole.Goalie;
			}
			return PlayerRole.Attacker;
		}
		return PlayerRole.Attacker;
	}

	public static string GetNameFromRole(PlayerRole role)
	{
		return role switch
		{
			PlayerRole.Attacker => "SKATER", 
			PlayerRole.Goalie => "GOALIE", 
			_ => "UNKNOWN", 
		};
	}

	public static List<string> GetHandednessNames()
	{
		return new List<string> { "LEFT", "RIGHT" };
	}

	public static PlayerHandedness GetHandednessFromName(string name)
	{
		if (!(name == "LEFT"))
		{
			if (name == "RIGHT")
			{
				return PlayerHandedness.Right;
			}
			return PlayerHandedness.Right;
		}
		return PlayerHandedness.Left;
	}

	public static string GetNameFromHandedness(PlayerHandedness handedness)
	{
		return handedness switch
		{
			PlayerHandedness.Left => "LEFT", 
			PlayerHandedness.Right => "RIGHT", 
			_ => "UNKNOWN", 
		};
	}

	public static List<string> GetUnitsNames()
	{
		return new List<string> { "METRIC", "IMPERIAL" };
	}

	public static Units GetUnitsFromName(string name)
	{
		if (!(name == "METRIC"))
		{
			if (name == "IMPERIAL")
			{
				return Units.Imperial;
			}
			return Units.Metric;
		}
		return Units.Metric;
	}

	public static string GetNameFromUnits(Units units)
	{
		return units switch
		{
			Units.Metric => "METRIC", 
			Units.Imperial => "IMPERIAL", 
			_ => "UNKNOWN", 
		};
	}

	public static NetworkBuffering GetNetworkBufferingFromName(string name)
	{
		return name switch
		{
			"PREDICTIVE" => NetworkBuffering.Predictive, 
			"RESPONSIVE" => NetworkBuffering.Responsive, 
			"BALANCED" => NetworkBuffering.Balanced, 
			"SMOOTH" => NetworkBuffering.Smooth, 
			_ => NetworkBuffering.Responsive, 
		};
	}

	public static string GetNameFromNetworkBuffering(NetworkBuffering networkBuffering)
	{
		return networkBuffering switch
		{
			NetworkBuffering.Predictive => "PREDICTIVE", 
			NetworkBuffering.Responsive => "RESPONSIVE", 
			NetworkBuffering.Balanced => "BALANCED", 
			NetworkBuffering.Smooth => "SMOOTH", 
			_ => "UNKNOWN", 
		};
	}

	public static float GetTargetTimelinePositionFromNetworkBuffering(NetworkBuffering networkBuffering)
	{
		return networkBuffering switch
		{
			NetworkBuffering.Predictive => 0.5f, 
			NetworkBuffering.Responsive => 0f, 
			NetworkBuffering.Balanced => -0.5f, 
			NetworkBuffering.Smooth => -1f, 
			_ => 0f, 
		};
	}

	public static List<string> GetFullScreenModeNames()
	{
		return new List<string> { "FULLSCREEN", "BORDERLESS", "WINDOWED" };
	}

	public static FullScreenMode GetFullScreenModeFromName(string name)
	{
		return name switch
		{
			"FULLSCREEN" => FullScreenMode.ExclusiveFullScreen, 
			"BORDERLESS" => FullScreenMode.FullScreenWindow, 
			"WINDOWED" => FullScreenMode.Windowed, 
			_ => FullScreenMode.FullScreenWindow, 
		};
	}

	public static string GetNameFromFullScreenMode(FullScreenMode mode)
	{
		return mode switch
		{
			FullScreenMode.ExclusiveFullScreen => "FULLSCREEN", 
			FullScreenMode.FullScreenWindow => "BORDERLESS", 
			FullScreenMode.Windowed => "WINDOWED", 
			_ => "UNKNOWN", 
		};
	}

	public static List<DisplayInfo> GetDisplayLayout()
	{
		List<DisplayInfo> list = new List<DisplayInfo>();
		Screen.GetDisplayLayout(list);
		return list;
	}

	public static List<string> GetDisplayNames()
	{
		return GetDisplayLayout().Select((DisplayInfo displayInfo, int index) => FormatDisplay(index, displayInfo)).ToList();
	}

	public static string GetDisplayNameFromIndex(int index)
	{
		List<DisplayInfo> displayLayout = GetDisplayLayout();
		if (index < 0 || index > displayLayout.Count - 1)
		{
			return "UNKNOWN";
		}
		return FormatDisplay(index, displayLayout[index]);
	}

	public static int GetDisplayIndexFromName(string name)
	{
		List<DisplayInfo> displayLayout = GetDisplayLayout();
		for (int i = 0; i < displayLayout.Count; i++)
		{
			if (FormatDisplay(i, displayLayout[i]) == name)
			{
				return i;
			}
		}
		return -1;
	}

	public static string FormatDisplay(int index, DisplayInfo displayInfo)
	{
		return $"{displayInfo.name} ({index})";
	}

	public static List<Resolution> GetResolutions()
	{
		return Screen.resolutions.ToList();
	}

	public static List<string> GetResolutionNames()
	{
		return (from resolution in GetResolutions()
			select FormatResolution(resolution)).ToList();
	}

	public static string GetResolutionNameFromIndex(int index)
	{
		List<Resolution> resolutions = GetResolutions();
		if (index < 0 || index > resolutions.Count - 1)
		{
			return "UNKNOWN";
		}
		return FormatResolution(resolutions[index]);
	}

	public static int GetResolutionIndexFromName(string name)
	{
		List<Resolution> resolutions = GetResolutions();
		for (int i = 0; i < resolutions.Count; i++)
		{
			if (FormatResolution(resolutions[i]) == name)
			{
				return i;
			}
		}
		return -1;
	}

	public static string FormatResolution(Resolution resolution)
	{
		return string.Format("{0}x{1} @ {2}Hz", resolution.width, resolution.height, resolution.refreshRateRatio.value.ToString("F0"));
	}

	public static List<string> GetApplicationQualityNames()
	{
		return new List<string> { "LOW", "MEDIUM", "HIGH", "ULTRA" };
	}

	public static ApplicationQuality GetApplicationQualityFromName(string name)
	{
		return name switch
		{
			"LOW" => ApplicationQuality.Low, 
			"MEDIUM" => ApplicationQuality.Medium, 
			"HIGH" => ApplicationQuality.High, 
			"ULTRA" => ApplicationQuality.Ultra, 
			_ => ApplicationQuality.High, 
		};
	}

	public static string GetNameFromApplicationQuality(ApplicationQuality quality)
	{
		return quality switch
		{
			ApplicationQuality.Low => "LOW", 
			ApplicationQuality.Medium => "MEDIUM", 
			ApplicationQuality.High => "HIGH", 
			ApplicationQuality.Ultra => "ULTRA", 
			_ => "UNKNOWN", 
		};
	}

	public static List<string> GetShadowQualityNames()
	{
		return new List<string> { "LOW", "MEDIUM", "HIGH", "ULTRA" };
	}

	public static ShadowQuality GetShadowQualityFromName(string name)
	{
		return name switch
		{
			"LOW" => ShadowQuality.Low, 
			"MEDIUM" => ShadowQuality.Medium, 
			"HIGH" => ShadowQuality.High, 
			"ULTRA" => ShadowQuality.Ultra, 
			_ => ShadowQuality.High, 
		};
	}

	public static string GetNameFromShadowQuality(ShadowQuality shadowQuality)
	{
		return shadowQuality switch
		{
			ShadowQuality.Low => "LOW", 
			ShadowQuality.Medium => "MEDIUM", 
			ShadowQuality.High => "HIGH", 
			ShadowQuality.Ultra => "ULTRA", 
			_ => "UNKNOWN", 
		};
	}

	public static KeyBindInteraction GetKeyBindInteractionFromInteraction(string interaction, KeyBindInteractionType interactionType)
	{
		switch (interaction)
		{
		case "Press(behavior=1)":
			return KeyBindInteraction.Release;
		case "DoublePress":
			return KeyBindInteraction.DoublePress;
		case "Hold":
			return KeyBindInteraction.Hold;
		case "Toggle":
			return KeyBindInteraction.Toggle;
		default:
			if (interactionType != KeyBindInteractionType.Press)
			{
				return KeyBindInteraction.Continuous;
			}
			return KeyBindInteraction.Press;
		}
	}

	public static string GetInteractionFromKeyBindInteraction(KeyBindInteraction keyBindInteraction)
	{
		return keyBindInteraction switch
		{
			KeyBindInteraction.Release => "Press(behavior=1)", 
			KeyBindInteraction.DoublePress => "DoublePress", 
			KeyBindInteraction.Hold => "Hold", 
			KeyBindInteraction.Toggle => "Toggle", 
			_ => string.Empty, 
		};
	}

	public static string GetHumanizedGamePhase(GamePhase phase, int period, bool isOvertime, int regulationPeriods = 3)
	{
		return phase switch
		{
			GamePhase.None => "", 
			GamePhase.Warmup => "WARMUP", 
			GamePhase.PreGame => "PRE-GAME", 
			GamePhase.FaceOff => "FACE-OFF", 
			GamePhase.Play => isOvertime ? GetHumanizedOvertime(period, regulationPeriods) : $"PERIOD {period}", 
			GamePhase.BlueScore => "SCORE!", 
			GamePhase.RedScore => "SCORE!", 
			GamePhase.Replay => "REPLAY", 
			GamePhase.Intermission => "INTERMISSION", 
			GamePhase.GameOver => "GAME OVER", 
			GamePhase.PostGame => "POST-GAME", 
			_ => phase.ToString(), 
		};
	}

	private static string GetHumanizedOvertime(int period, int regulationPeriods)
	{
		int num = period - Mathf.Max(regulationPeriods, 1);
		if (num < 2)
		{
			return "OVERTIME";
		}
		return $"OVERTIME {num}";
	}

	public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(sourceDir);
		if (!directoryInfo.Exists)
		{
			throw new DirectoryNotFoundException("Source directory not found: " + directoryInfo.FullName);
		}
		DirectoryInfo[] directories = directoryInfo.GetDirectories();
		Directory.CreateDirectory(destinationDir);
		FileInfo[] files = directoryInfo.GetFiles();
		foreach (FileInfo fileInfo in files)
		{
			string destFileName = Path.Combine(destinationDir, fileInfo.Name);
			fileInfo.CopyTo(destFileName, overwrite: true);
		}
		if (recursive)
		{
			DirectoryInfo[] array = directories;
			foreach (DirectoryInfo directoryInfo2 in array)
			{
				string destinationDir2 = Path.Combine(destinationDir, directoryInfo2.Name);
				CopyDirectory(directoryInfo2.FullName, destinationDir2, recursive: true);
			}
		}
	}

	public static string GetConnectionRejectionMessage(ConnectionRejectionCode code, string message = null)
	{
		if (!string.IsNullOrEmpty(message))
		{
			return message;
		}
		return code switch
		{
			ConnectionRejectionCode.ServerFull => "Server full", 
			ConnectionRejectionCode.TimedOut => "Timed out", 
			ConnectionRejectionCode.Banned => "Banned", 
			ConnectionRejectionCode.NotWhitelisted => "Not whitelisted", 
			ConnectionRejectionCode.MissingPassword => "Missing password", 
			ConnectionRejectionCode.InvalidPassword => "Invalid password", 
			ConnectionRejectionCode.MissingMods => "Missing mods", 
			ConnectionRejectionCode.ServerStarting => "Server is still starting...", 
			_ => "Server unreachable", 
		};
	}

	public static string GetDisconnectionMessage(DisconnectionCode code, string message = null)
	{
		if (!string.IsNullOrEmpty(message))
		{
			return message;
		}
		return code switch
		{
			DisconnectionCode.Disconnected => "Disconnected", 
			DisconnectionCode.Kicked => "Kicked", 
			DisconnectionCode.Banned => "Banned", 
			_ => "Connection lost", 
		};
	}

	public static string GetCommandLineArgument(string name, string[] args = null)
	{
		if (args == null)
		{
			args = Environment.GetCommandLineArgs();
		}
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == (name ?? ""))
			{
				if (i + 1 >= args.Length)
				{
					return null;
				}
				return args[i + 1];
			}
		}
		return null;
	}

	public static void PrintUPnPLogs()
	{
		if (uPnPHelper.DebugMode)
		{
			List<string> debugMessageArray = uPnPHelper.GetDebugMessageArray();
			foreach (string item in debugMessageArray.ToList())
			{
				uPnPLogger.Info(item ?? "");
			}
			debugMessageArray.Clear();
		}
		List<string> errorMessageArray = uPnPHelper.GetErrorMessageArray();
		foreach (string item2 in errorMessageArray.ToList())
		{
			uPnPLogger.Error(item2 ?? "");
		}
		errorMessageArray.Clear();
	}

	public static double GetTimestamp()
	{
		return DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
	}

	public static void WhenAllActions(Action callback, params Action<Action>[] subscriptions)
	{
		bool[] state = new bool[subscriptions.Length];
		bool callbackInvoked = false;
		for (int i = 0; i < subscriptions.Length; i++)
		{
			int index = i;
			subscriptions[index](() =>
			{
				if (!state[index])
				{
					state[index] = true;
					if (!callbackInvoked && state.All((bool s) => s))
					{
						callbackInvoked = true;
						callback?.Invoke();
					}
				}
			});
		}
	}

	public static bool IsValidSteamId64(string value)
	{
		if (ulong.TryParse(value, out var result))
		{
			return result >= 76561197960265728L;
		}
		return false;
	}

	public static int GetVoteMajority(int playerCount)
	{
		return playerCount switch
		{
			1 => 1, 
			2 => 2, 
			_ => Mathf.CeilToInt((float)(playerCount - 1) * 0.75f), 
		};
	}

	public static bool IsGameInProgress(GamePhase phase)
	{
		if (phase != GamePhase.FaceOff && phase != GamePhase.Play && phase != GamePhase.BlueScore && phase != GamePhase.RedScore && phase != GamePhase.Replay)
		{
			return phase == GamePhase.Intermission;
		}
		return true;
	}

	public static PlayerTeam? GetOpposingTeam(PlayerTeam team)
	{
		return team switch
		{
			PlayerTeam.Blue => (PlayerTeam?)PlayerTeam.Red, 
			PlayerTeam.Red => PlayerTeam.Blue, 
			_ => null, 
		};
	}

	public static void SwapMaterial(MeshRenderer meshRenderer, Material material, int index = 0)
	{
		Material[] materials = meshRenderer.materials;
		if (index < materials.Length)
		{
			UnityEngine.Object.Destroy(materials[index]);
			materials[index] = new Material(material);
			meshRenderer.materials = materials;
		}
	}
}
