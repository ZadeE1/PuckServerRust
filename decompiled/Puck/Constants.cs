using UnityEngine;

public static class Constants
{
	public const uint APP_ID = 2994020u;

	public const float STEAM_INITIALIZATION_RETRY_DELAY = 5f;

	public const int STEAM_INITIALIZATION_HELP_ATTEMPTS = 3;

	public const float STEAM_WEB_API_TICKET_TIMEOUT = 10f;

	public const float STEAM_WEB_API_TICKET_RETRY_DELAY = 3f;

	public const float DEPLOYMENT_EMPTY_TIMEOUT = 60f;

	public const float DEPLOYMENT_AUTHENTICATION_TIMEOUT = 60f;

	public const string DEFAULT_BACKEND_URL = "ws://localhost:8080";

	public const int WEB_SOCKET_CONNECTION_TIMEOUT = 5000;

	public const int WEB_SOCKET_RECONNECT_BASE_DELAY = 500;

	public const int WEB_SOCKET_RECONNECTION_DELAY_MAX = 30000;

	public const int WEB_SOCKET_PING_TIMEOUT = 8000;

	public const int WEB_SOCKET_PING_WATCHDOG_INTERVAL = 1000;

	public const float SERVER_MOD_READINESS_TIMEOUT = 300f;

	public const float SERVER_MOD_PROGRESS_LOG_INTERVAL = 2f;

	public const string SERVER_BIND_ADDRESS = "0.0.0.0";

	public const string TEAM_BLUE_COLOR = "#3b82f6";

	public const string TEAM_RED_COLOR = "#d13333";

	public const string TEAM_SPECTATOR_COLOR = "#404040";

	public const string PATREON_COLOR = "#f1c40f";

	public const string MODERATOR_COLOR = "#206694";

	public const string ADMIN_COLOR = "#992d22";

	public const string DEVELOPER_COLOR = "#71368a";

	public const string SERVER_COLOR = "#b8b8b8";

	public const string VOTE_COLOR = "#e67e22";

	public const string GAME_COLOR = "#ffe97f";

	public const string ERROR_COLOR = "#e74c3c";

	public const int CHAT_MESSAGE_MAX_BYTES = 509;

	public const DebugMode DEFAULT_SETTINGS_DEBUG = DebugMode.Off;

	public const float DEFAULT_SETTINGS_CAMERA_ANGLE = 30f;

	public const PlayerHandedness DEFAULT_SETTINGS_HANDEDNESS = PlayerHandedness.Right;

	public const bool DEFAULT_SETTINGS_SHOW_PUCK_SILHOUETTE = true;

	public const bool DEFAULT_SETTINGS_SHOW_PUCK_OUTLINE = false;

	public const bool DEFAULT_SETTINGS_SHOW_PUCK_ELEVATION = true;

	public const bool DEFAULT_SETTINGS_SHOW_PLAYER_USERNAMES = false;

	public const float DEFAULT_SETTINGS_PLAYER_USERNAMES_FADE_THRESHOLD = 1f;

	public const int DEFAULT_SETTINGS_MAX_MATCHMAKING_RTT = 50;

	public const NetworkBuffering DEFAULT_SETTINGS_NETWORK_BUFFERING = NetworkBuffering.Responsive;

	public const bool DEFAULT_SETTINGS_FILTER_CHAT_PROFANITY = true;

	public const Units DEFAULT_SETTINGS_UNITS = Units.Metric;

	public const bool DEFAULT_SETTINGS_SHOW_GAME_USER_INTERFACE = true;

	public const bool DEFAULT_SETTINGS_SHOW_TEAM_COLOR_BAR = true;

	public const float DEFAULT_SETTINGS_USER_INTERFACE_SCALE = 1f;

	public const float DEFAULT_SETTINGS_CHAT_OPACITY = 1f;

	public const float DEFAULT_SETTINGS_CHAT_SCALE = 1f;

	public const float DEFAULT_SETTINGS_MINIMAP_OPACITY = 1f;

	public const float DEFAULT_SETTINGS_MINIMAP_BACKGROUND_OPACITY = 1f;

	public const float DEFAULT_SETTINGS_MINIMAP_HORIZONTAL_POSITION = 100f;

	public const float DEFAULT_SETTINGS_MINIMAP_VERTICAL_POSITION = 0f;

	public const float DEFAULT_SETTINGS_MINIMAP_SCALE = 1f;

	public const bool DEFAULT_SETTINGS_SHOW_MINIMAP_STICKS = true;

	public const bool DEFAULT_SETTINGS_SHOW_MINIMAP_PUCK_ELEVATION = true;

	public const bool DEFAULT_SETTINGS_SHOW_MINIMAP_FALLEN_INDICATOR = true;

	public const float MINIMAP_STICK_MAX_HEIGHT = 3f;

	public const float MINIMAP_STICK_MIN_SCALE = 0.5f;

	public const float MINIMAP_PUCK_MAX_HEIGHT = 8f;

	public const float MINIMAP_PUCK_MIN_SCALE = 0.6f;

	public const float MINIMAP_FALLEN_OPACITY = 0.4f;

	public const float DEFAULT_SETTINGS_GLOBAL_STICK_SENSITIVITY = 0.2f;

	public const float DEFAULT_SETTINGS_HORIZONTAL_STICK_SENSITIVITY = 1f;

	public const float DEFAULT_SETTINGS_VERTICAL_STICK_SENSITIVITY = 1f;

	public const float DEFAULT_SETTINGS_LOOK_SENSITIVITY = 0.2f;

	public const float DEFAULT_SETTINGS_GLOBAL_VOLUME = 0.5f;

	public const float DEFAULT_SETTINGS_AMBIENT_VOLUME = 1f;

	public const float DEFAULT_SETTINGS_GAME_VOLUME = 1f;

	public const float DEFAULT_SETTINGS_VOICE_VOLUME = 1f;

	public const float DEFAULT_SETTINGS_UI_VOLUME = 0.5f;

	public const FullScreenMode DEFAULT_SETTINGS_FULL_SCREEN_MODE = FullScreenMode.FullScreenWindow;

	public const int DEFAULT_SETTINGS_DISPLAY_INDEX = 0;

	public const int DEFAULT_SETTINGS_RESOLUTION_INDEX = -1;

	public const bool DEFAULT_SETTINGS_VSYNC = false;

	public const int DEFAULT_SETTINGS_FPS_LIMIT = 240;

	public const float DEFAULT_SETTINGS_FOV = 90f;

	public const ApplicationQuality DEFAULT_SETTINGS_QUALITY = ApplicationQuality.High;

	public const ShadowQuality DEFAULT_SETTINGS_SHADOW_QUALITY = ShadowQuality.High;

	public const bool DEFAULT_SETTINGS_MOTION_BLUR = true;

	public const int SHADOW_CASCADE_COUNT = 4;

	public static readonly Vector3 SHADOW_CASCADE_RANGES = new Vector3(4.5f, 9f, 16.5f);

	public const PlayerTeam DEFAULT_TEAM = PlayerTeam.Blue;

	public const PlayerRole DEFAULT_ROLE = PlayerRole.Attacker;

	public const bool DEFAULT_APPLY_FOR_BOTH_TEAMS = false;

	public const int NO_ITEM_ID = -1;

	public const int DEFAULT_FLAG_ID = -1;

	public const int DEFAULT_HEADGEAR_ID_BLUE_ATTACKER = 513;

	public const int DEFAULT_HEADGEAR_ID_RED_ATTACKER = 513;

	public const int DEFAULT_HEADGEAR_ID_BLUE_GOALIE = 527;

	public const int DEFAULT_HEADGEAR_ID_RED_GOALIE = 527;

	public const int DEFAULT_MUSTACHE_ID = -1;

	public const int DEFAULT_BEARD_ID = -1;

	public const int DEFAULT_JERSEY_ID_BLUE_ATTACKER = 2048;

	public const int DEFAULT_JERSEY_ID_RED_ATTACKER = 2048;

	public const int DEFAULT_JERSEY_ID_BLUE_GOALIE = 2048;

	public const int DEFAULT_JERSEY_ID_RED_GOALIE = 2048;

	public const int DEFAULT_STICK_SKIN_ID_BLUE_ATTACKER = 2621;

	public const int DEFAULT_STICK_SKIN_ID_RED_ATTACKER = 2621;

	public const int DEFAULT_STICK_SKIN_ID_BLUE_GOALIE = 2621;

	public const int DEFAULT_STICK_SKIN_ID_RED_GOALIE = 2621;

	public const int DEFAULT_STICK_SHAFT_TAPE_ID_BLUE_ATTACKER = -1;

	public const int DEFAULT_STICK_SHAFT_TAPE_ID_RED_ATTACKER = -1;

	public const int DEFAULT_STICK_SHAFT_TAPE_ID_BLUE_GOALIE = -1;

	public const int DEFAULT_STICK_SHAFT_TAPE_ID_RED_GOALIE = -1;

	public const int DEFAULT_STICK_BLADE_TAPE_ID_BLUE_ATTACKER = -1;

	public const int DEFAULT_STICK_BLADE_TAPE_ID_RED_ATTACKER = -1;

	public const int DEFAULT_STICK_BLADE_TAPE_ID_BLUE_GOALIE = -1;

	public const int DEFAULT_STICK_BLADE_TAPE_ID_RED_GOALIE = -1;

	public const ushort DEFAULT_SERVER_PORT = 30609;

	public const string DEFAULT_SERVER_NAME = "MY PUCK SERVER";

	public const ushort DEFAULT_SERVER_MAX_PLAYERS = 12;

	public const string DEFAULT_SERVER_PASSWORD = null;

	public const ushort DEFAULT_SERVER_TICK_RATE = 200;

	public const bool DEFAULT_SERVER_IS_PUBLIC = true;

	public const bool DEFAULT_SERVER_USE_VOIP = false;

	public const bool DEFAULT_SERVER_USE_WHITELIST = false;

	public static readonly ModConfig[] DEFAULT_SERVER_MODS = new ModConfig[0];

	public const string DEFAULT_SERVER_GAME_MODE = "public";

	public const string DEFAULT_SERVER_LEVEL = "default";

	public const float KICK_TIMEOUT = 60f;

	public const float VOICE_TRANSMIT_LEVEL_GAIN = 14f;

	public const float VOICE_LEVEL_SMOOTHING_RATE = 15f;

	public const float VOICE_LEVEL_HOLD_SECONDS = 0.1f;

	public static readonly string[] CHAT_BLACKLIST = new string[4] { "卐", "卍", "☭", "⛧" };

	public static readonly string[] CHAT_WHITELIST = new string[4] { "❤\ufe0f", "\ud83d\ude2d", "\ud83d\udd25", "\ud83d\udcaf" };

	public const float INPUT_DEADZONE = 0.05f;

	public const float SPRINT_STAMINA_THRESHOLD = 0.25f;

	public const float MATCH_PLAYER_JOIN_TIMEOUT = 60f;

	public const float MATCH_PLAYER_ABANDONMENT_TIMEOUT = 120f;

	public const float GOAL_SLOW_MOTION_SCALE = 0.35f;

	public const float GOAL_SLOW_MOTION_RAMP_IN_SECONDS = 0.08f;

	public const float GOAL_SLOW_MOTION_HOLD_SECONDS = 0.28f;

	public const float GOAL_SLOW_MOTION_RAMP_OUT_SECONDS = 0.42f;

	public const float SLOW_MOTION_AUDIO_LOWPASS_MIN_HZ = 900f;

	public const float SLOW_MOTION_AUDIO_LOWPASS_MAX_HZ = 22000f;

	public const float SLOW_MOTION_AUDIO_PITCH_FLOOR = 0.95f;

	public const float GOAL_SLOW_MOTION_CAMERA_FOV_PUNCH = 0f;

	public const float REPLAY_TOUCH_SLOW_MOTION_CAMERA_FOV_PUNCH = 12f;

	public const float REPLAY_GOAL_SLOW_MOTION_CAMERA_FOV_PUNCH = 12f;

	public const float REPLAY_TOUCH_SLOW_MOTION_SCALE = 0.3f;

	public const float REPLAY_TOUCH_SLOW_MOTION_LEAD_SECONDS = 0.6f;

	public const float REPLAY_TOUCH_SLOW_MOTION_RAMP_IN_SECONDS = 0.5f;

	public const float REPLAY_TOUCH_SLOW_MOTION_HOLD_SECONDS = 0.45f;

	public const float REPLAY_TOUCH_SLOW_MOTION_RAMP_OUT_SECONDS = 0.5f;

	public const float REPLAY_GOAL_SLOW_MOTION_SCALE = 0.3f;

	public const float REPLAY_GOAL_SLOW_MOTION_LEAD_SECONDS = 0.4f;

	public const float REPLAY_GOAL_SLOW_MOTION_RAMP_IN_SECONDS = 0.4f;

	public const float REPLAY_GOAL_SLOW_MOTION_HOLD_SECONDS = 0.4f;

	public const float REPLAY_GOAL_SLOW_MOTION_RAMP_OUT_SECONDS = 0.5f;

	public const float REPLAY_OWN_GOAL_TOUCH_THRESHOLD_SECONDS = 3f;

	public const float REPLAY_CELLY_CAM_DELAY_SECONDS = 0.75f;

	public const float GOAL_FLASH_PEAK_OPACITY = 0.4f;
}
