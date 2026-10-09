using UnityEngine;

public class SynchronizedObjectLodSelector
{
	private SynchronizedObjectBandSettings[] lodBands;

	private SynchronizedObjectBandSettings culling;

	private byte noOriginTickRateDivisor = 1;

	private float hysteresis;

	private bool useHighPrecisionRotation = true;

	private bool hasOrigin;

	private Vector3 origin = Vector3.zero;

	private Vector3 viewDirection = Vector3.zero;

	public void Configure(SynchronizedObjectBandSettings[] lodBands, SynchronizedObjectBandSettings culling, byte noOriginTickRateDivisor, float hysteresis, bool useHighPrecisionRotation)
	{
		this.lodBands = lodBands;
		this.culling = culling;
		this.noOriginTickRateDivisor = noOriginTickRateDivisor;
		this.hysteresis = hysteresis;
		this.useHighPrecisionRotation = useHighPrecisionRotation;
	}

	public void BeginPlayer(Player player)
	{
		PlayerCamera playerCamera = ((player != null) ? player.PlayerCamera : null);
		hasOrigin = playerCamera != null;
		if (!hasOrigin)
		{
			origin = Vector3.zero;
			viewDirection = Vector3.zero;
		}
		else
		{
			origin = playerCamera.transform.position;
			viewDirection = playerCamera.transform.forward;
			viewDirection.y = 0f;
		}
	}

	public SynchronizedObjectLodSelection Select(Vector3 position, SynchronizedObjectLodSelection previousSelection, bool usesLod, bool usesCulling)
	{
		return WithHighPrecisionRotation(GetSelection(position, previousSelection, usesLod, usesCulling));
	}

	private SynchronizedObjectLodSelection GetSelection(Vector3 position, SynchronizedObjectLodSelection previousSelection, bool usesLod, bool usesCulling)
	{
		if (!hasOrigin)
		{
			if (!usesLod)
			{
				return SynchronizedObjectLodSelection.Default;
			}
			return GetNoOriginSelection();
		}
		Vector3 offset = position - origin;
		float sqrMagnitude = offset.sqrMagnitude;
		SynchronizedObjectLodSelection result = (usesLod ? GetBandSelection(sqrMagnitude, previousSelection.BandIndex) : SynchronizedObjectLodSelection.Default);
		if (!usesCulling || !IsCulled(offset, sqrMagnitude, previousSelection.IsCulled))
		{
			return result;
		}
		result.Source = SynchronizedObjectLodSource.Culled;
		result.TickRateDivisor = (byte)Mathf.Max(1, culling.TickRateDivisor);
		return result;
	}

	private SynchronizedObjectLodSelection WithHighPrecisionRotation(SynchronizedObjectLodSelection selection)
	{
		selection.UseHighPrecisionRotation = useHighPrecisionRotation && selection.Source == SynchronizedObjectLodSource.Default;
		return selection;
	}

	private bool IsCulled(Vector3 offset, float sqrDistance, bool wasCulled)
	{
		if (culling.TickRateDivisor <= 1)
		{
			return false;
		}
		float num = culling.MinDistance * GetHysteresisScale(wasCulled);
		if (sqrDistance > num * num)
		{
			return Vector3.Dot(offset, viewDirection) < 0f;
		}
		return false;
	}

	private SynchronizedObjectLodSelection GetBandSelection(float sqrDistance, int previousBandIndex)
	{
		SynchronizedObjectLodSelection result = SynchronizedObjectLodSelection.Default;
		if (lodBands == null)
		{
			return result;
		}
		for (int i = 0; i < lodBands.Length; i++)
		{
			float num = lodBands[i].MinDistance * GetHysteresisScale(i <= previousBandIndex);
			if (sqrDistance < num * num)
			{
				break;
			}
			result.BandIndex = i;
			result.Source = SynchronizedObjectLodSource.Band;
			result.TickRateDivisor = (byte)Mathf.Max(1, lodBands[i].TickRateDivisor);
		}
		return result;
	}

	private SynchronizedObjectLodSelection GetNoOriginSelection()
	{
		return new SynchronizedObjectLodSelection
		{
			BandIndex = -1,
			Source = SynchronizedObjectLodSource.NoOrigin,
			TickRateDivisor = (byte)Mathf.Max(1, noOriginTickRateDivisor)
		};
	}

	private float GetHysteresisScale(bool isAlreadySelected)
	{
		if (!isAlreadySelected)
		{
			return 1f + hysteresis;
		}
		return 1f - hysteresis;
	}
}
