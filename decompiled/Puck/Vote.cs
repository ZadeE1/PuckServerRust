using System;
using System.Collections.Generic;
using DG.Tweening;

public class Vote
{
	private static readonly Logger Logger = new Logger("Vote");

	public string Name;

	public string Title;

	public string Description;

	public PlayerTeam[] Teams;

	public float Timeout;

	public string SteamId;

	public int RequiredVotes;

	public object Data;

	public List<string> InFavourSteamIds = new List<string>();

	public List<string> AgainstSteamIds = new List<string>();

	public Action<Vote, string, bool> Progressed;

	public Action<Vote> Ended;

	public bool Passed;

	private Tween timeoutTween;

	public int InFavourVotes => InFavourSteamIds.Count;

	public int AgainstVotes => AgainstSteamIds.Count;

	public Vote(string name, string title, string description, PlayerTeam[] teams, float timeout, string steamId, int requiredVotes, object data = null)
	{
		Name = name;
		Title = title;
		Description = description;
		Teams = teams;
		Timeout = timeout;
		SteamId = steamId;
		RequiredVotes = requiredVotes;
		Data = data;
	}

	public void Initialize()
	{
		timeoutTween?.Kill();
		timeoutTween = DOVirtual.DelayedCall(Timeout, () =>
		{
			End();
		});
		CastVote(SteamId, inFavour: true);
	}

	public void Dispose()
	{
		timeoutTween?.Kill();
	}

	public void CastVote(string steamId, bool inFavour)
	{
		if (!InFavourSteamIds.Contains(steamId) && !AgainstSteamIds.Contains(steamId))
		{
			if (inFavour)
			{
				InFavourSteamIds.Add(steamId);
			}
			else
			{
				AgainstSteamIds.Add(steamId);
			}
			Logger.Info("Vote '" + Name + "': " + steamId + " voted " + (inFavour ? "in favour" : "against") + " " + $"({InFavourVotes} for, {AgainstVotes} against, {RequiredVotes} required)");
			if (InFavourVotes >= RequiredVotes)
			{
				End();
			}
			else if (SteamId != steamId)
			{
				Progressed?.Invoke(this, steamId, inFavour);
			}
		}
	}

	private void End()
	{
		Passed = InFavourVotes >= RequiredVotes;
		Logger.Info(string.Format("Vote '{0}' ended: {1} ({2}/{3})", Name, Passed ? "passed" : "failed", InFavourVotes, RequiredVotes));
		Ended?.Invoke(this);
	}
}
