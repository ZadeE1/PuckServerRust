using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class VoteManager : MonoBehaviourSingleton<VoteManager>
{
	private static readonly Logger Logger = new Logger("VoteManager");

	public List<Vote> Votes = new List<Vote>();

	public void Server_AddVote(string name, string title, string description, PlayerTeam[] teams, float timeout, string steamId, int requiredVotes, object data = null)
	{
		Vote vote = new Vote(name, title, description, teams, timeout, steamId, requiredVotes, data);
		Votes.Add(vote);
		Logger.Info(string.Format("Vote '{0}' started by {1} (teams: {2}, required: {3})", name, steamId, string.Join(", ", teams), requiredVotes));
		EventManager.TriggerEvent("Event_Server_OnVoteAdded", new Dictionary<string, object>
		{
			{ "vote", vote },
			{ "teams", teams }
		});
		vote.Progressed = (Action<Vote, string, bool>)Delegate.Combine(vote.Progressed, new Action<Vote, string, bool>(Server_OnVoteProgressed));
		vote.Ended = (Action<Vote>)Delegate.Combine(vote.Ended, new Action<Vote>(Server_OnVoteEnded));
		vote.Initialize();
	}

	public void Server_RemoveVote(Vote vote)
	{
		if (Votes.Contains(vote))
		{
			Votes.Remove(vote);
			EventManager.TriggerEvent("Event_Server_OnVoteRemoved", new Dictionary<string, object> { { "vote", vote } });
			vote.Progressed = (Action<Vote, string, bool>)Delegate.Remove(vote.Progressed, new Action<Vote, string, bool>(Server_OnVoteProgressed));
			vote.Ended = (Action<Vote>)Delegate.Remove(vote.Ended, new Action<Vote>(Server_OnVoteEnded));
			vote.Dispose();
		}
	}

	public Vote[] Server_GetVotesByName(string name)
	{
		return Votes.Where((Vote v) => v.Name == name).ToArray();
	}

	public Vote Server_GetVoteByName(string name)
	{
		return Votes.Find((Vote v) => v.Name == name);
	}

	public Vote[] Server_GetTeamVotesByName(string name, PlayerTeam team)
	{
		return Votes.Where((Vote v) => v.Name == name && Enumerable.Contains(v.Teams, team)).ToArray();
	}

	public Vote Server_GetTeamVoteByName(string name, PlayerTeam team)
	{
		return Votes.Find((Vote v) => v.Name == name && Enumerable.Contains(v.Teams, team));
	}

	[Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server, DeferLocal = true)]
	public void Server_NotifyVoteStartedRpc(string name, string description, string steamId, int inFavourVotes, int againstVotes, int requiredVotes, object data)
	{
		EventManager.TriggerEvent("Event_OnVoteStarted", new Dictionary<string, object>
		{
			{ "name", name },
			{ "description", description },
			{ "steamId", steamId },
			{ "inFavourVotes", inFavourVotes },
			{ "againstVotes", againstVotes },
			{ "requiredVotes", requiredVotes },
			{ "data", data }
		});
	}

	[Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server, DeferLocal = true)]
	public void Server_NotifyVoteProgressedRpc(string name, int inFavourVotes, int againstVotes)
	{
		EventManager.TriggerEvent("Event_OnVoteProgressed", new Dictionary<string, object>
		{
			{ "name", name },
			{ "inFavourVotes", inFavourVotes },
			{ "againstVotes", againstVotes }
		});
	}

	[Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server, DeferLocal = true)]
	public void Server_NotifyVoteEndedRpc(string name, bool succeeded)
	{
		EventManager.TriggerEvent("Event_OnVoteEnded", new Dictionary<string, object>
		{
			{ "name", name },
			{ "succeeded", succeeded }
		});
	}

	private void Server_OnVoteProgressed(Vote vote, string steamId, bool inFavour)
	{
		EventManager.TriggerEvent("Event_Server_OnVoteProgressed", new Dictionary<string, object>
		{
			{ "vote", vote },
			{ "steamId", steamId },
			{ "inFavour", inFavour }
		});
	}

	private void Server_OnVoteEnded(Vote vote)
	{
		Server_RemoveVote(vote);
	}
}
