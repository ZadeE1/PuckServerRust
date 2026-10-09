using System;
using System.Text.Json.Serialization;

public class PlayerMatchData : MatchData
{
	[JsonIgnore]
	public int? JoinTimeoutRemainingSeconds
	{
		get
		{
			if (!startedAt.HasValue)
			{
				return null;
			}
			DateTime utcNow = DateTime.UtcNow;
			DateTime dateTime = DateTimeOffset.FromUnixTimeMilliseconds((long)startedAt.Value).DateTime;
			int num = (int)(60.0 - utcNow.Subtract(dateTime).TotalSeconds);
			if (num <= 0)
			{
				return null;
			}
			return num;
		}
	}
}
