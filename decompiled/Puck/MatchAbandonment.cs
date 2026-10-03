using System;
using DG.Tweening;
using UnityEngine;

public class MatchAbandonment
{
	private static readonly Logger Logger = new Logger("MatchAbandonment");

	private Tween timeout;

	private bool isDisposed;

	public string SteamId { get; }

	public float AbandonmentTimeout { get; }

	public MatchAbandonmentPhase Phase { get; private set; }

	public float RemainingSeconds
	{
		get
		{
			if (timeout == null || !timeout.IsActive())
			{
				return 0f;
			}
			return Mathf.Max(0f, timeout.Duration() - timeout.Elapsed());
		}
	}

	public bool HasAbandoned { get; private set; }

	public bool IsPaused => Phase == MatchAbandonmentPhase.Paused;

	public event Action<MatchAbandonment> Abandoned;

	public MatchAbandonment(string steamId, float abandonmentTimeout)
	{
		SteamId = steamId;
		AbandonmentTimeout = Mathf.Max(0f, abandonmentTimeout);
		Phase = MatchAbandonmentPhase.None;
		StartTimeout(AbandonmentTimeout);
		timeout?.Pause();
	}

	public void Stop()
	{
		if (!isDisposed && !HasAbandoned && Phase != MatchAbandonmentPhase.Paused)
		{
			Phase = MatchAbandonmentPhase.Paused;
			timeout?.Pause();
			Logger.Info($"{SteamId} abandonment paused with {RemainingSeconds:F2}s of budget remaining");
		}
	}

	public void Start()
	{
		if (!isDisposed && !HasAbandoned && Phase == MatchAbandonmentPhase.Paused)
		{
			Phase = MatchAbandonmentPhase.Running;
			timeout?.Play();
			Logger.Info($"{SteamId} abandonment running with {RemainingSeconds:F2}s of budget remaining");
		}
	}

	public void Dispose()
	{
		if (!isDisposed)
		{
			isDisposed = true;
			KillTimeout();
			Abandoned = null;
		}
	}

	private void StartTimeout(float duration)
	{
		timeout = DOVirtual.DelayedCall(duration, Abandon).SetTarget(this);
	}

	private void KillTimeout()
	{
		timeout?.Kill();
		timeout = null;
	}

	private void Abandon()
	{
		if (!isDisposed && !HasAbandoned)
		{
			timeout = null;
			HasAbandoned = true;
			Logger.Info(SteamId + " abandoned the match");
			Abandoned?.Invoke(this);
		}
	}
}
