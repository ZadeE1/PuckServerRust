using System;
using UnityEngine;

[Serializable]
public struct SynchronizedObjectBandSettings
{
	[Tooltip("Distance from the player beyond which this band applies.")]
	public float MinDistance;

	[Range(1f, 16f)]
	[Tooltip("Objects in this band update every TickRateDivisor ticks, so 2 sends at 50 Hz from a 100 Hz tick rate. A value of 1 disables throttling for the band.")]
	public byte TickRateDivisor;
}
