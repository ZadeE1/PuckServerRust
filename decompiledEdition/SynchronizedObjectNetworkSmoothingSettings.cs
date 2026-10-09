using System;
using UnityEngine;

[Serializable]
public struct SynchronizedObjectNetworkSmoothingSettings
{
	[Tooltip("Playback speed correction applied per second of timeline error (2.0 = a 5ms error corrects at 1% speed).")]
	public float TimescaleCorrectionGain;

	[Tooltip("Maximum playback speed deviation used for timeline corrections (0.04 = plus or minus 4%).")]
	public float MaxTimescaleCorrection;

	[Tooltip("Seconds of history averaged when measuring timeline drift for catchup and slowdown decisions.")]
	public int DriftEmaDuration;

	[Tooltip("Timeline error in seconds beyond which playback snaps straight to server time instead of easing back through playback speed. A frame hitch on join can throw the timeline far enough ahead that the speed correction needs many seconds to recover, freezing every object at its extrapolation limit until it does. 0 disables snapping.")]
	public float MaxTimelineDriftTime;

	[Tooltip("How far an object may coast in position on its last known linear velocity when the timeline passes its newest sample. Measured in the object's own update interval, so objects throttled by LOD get proportionally more headroom. Coasting eases off as it approaches this limit rather than stopping dead at it, so the object never travels further than this many updates of its last velocity no matter how long the gap lasts.")]
	public float MaxPositionExtrapolationTicks;

	[Tooltip("The same limit for rotation, kept separate because it can afford to be far larger. Over-projecting a position drives objects through walls, while over-projecting a rotation only leaves them at a wrong angle, so rotation is allowed to coast much further before easing off. A player cannot react to a stall shorter than their own reaction time, so through any stall brief enough to matter their turn rate is almost certainly unchanged.")]
	public float MaxRotationExtrapolationTicks;

	[Tooltip("Playback position in ticks relative to the newest received server tick. 0 plays at the newest tick, hiding the interpolation buffer through extrapolation; -1 plays one tick in the past so extrapolation only occurs when a tick is lost or late, at the cost of one tick of added latency.")]
	public float TargetTimelinePosition;

	public static readonly SynchronizedObjectNetworkSmoothingSettings Default = new SynchronizedObjectNetworkSmoothingSettings
	{
		TimescaleCorrectionGain = 2f,
		MaxTimescaleCorrection = 0.04f,
		DriftEmaDuration = 1,
		MaxTimelineDriftTime = 0.25f,
		MaxPositionExtrapolationTicks = 5f,
		MaxRotationExtrapolationTicks = 25f,
		TargetTimelinePosition = 0f
	};
}
