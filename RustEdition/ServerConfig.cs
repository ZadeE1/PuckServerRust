using System.Linq;
using System.Text.Json.Serialization;

public class ServerConfig
{
	public ushort port { get; set; } = 30609;

	public string name { get; set; } = "MY PUCK SERVER";

	public int maxPlayers { get; set; } = 12;

	public string password { get; set; }

	public int tickRate { get; set; } = 200;

	public bool isPublic { get; set; } = true;

	public bool useVoip { get; set; }

	public bool useWhitelist { get; set; }

	public ModConfig[] mods { get; set; } = Constants.DEFAULT_SERVER_MODS;

	public string gameMode { get; set; } = "public";

	public string level { get; set; } = "default";

	[JsonIgnore]
	public string[] EnabledModIds => (from mod in mods
		where mod.isEnabled
		select mod.id).ToArray();

	[JsonIgnore]
	public string[] ClientRequiredModIds => (from mod in mods
		where mod.isClientRequired
		select mod.id).ToArray();
}
