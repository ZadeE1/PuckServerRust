public readonly struct ChatCommandInfo(string usage, string description, int requiredAdminLevel = 0)
{
	public readonly string Usage = usage;

	public readonly string Description = description;

	public readonly int RequiredAdminLevel = requiredAdminLevel;
}
