using System;
using System.Collections.Generic;
using Unity.Netcode;

public class CompressedNetworkVariable<TRaw, TNetwork> : NetworkVariable<TNetwork> where TRaw : struct where TNetwork : struct
{
	private readonly Func<TRaw, TNetwork> compressor;

	private readonly Func<TNetwork, TRaw> decompressor;

	private TRaw cachedValue;

	public new TRaw Value
	{
		get
		{
			return cachedValue;
		}
		set
		{
			TRaw val = cachedValue;
			cachedValue = value;
			base.Value = compressor(value);
			if (!EqualityComparer<TRaw>.Default.Equals(cachedValue, val))
			{
				OnRawValueChanged?.Invoke(val, cachedValue);
			}
		}
	}

	public event Action<TRaw, TRaw> OnRawValueChanged;

	public CompressedNetworkVariable(Func<TRaw, TNetwork> compressor, Func<TNetwork, TRaw> decompressor, TRaw initialValue = default(TRaw), NetworkVariableReadPermission readPerm = NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission writePerm = NetworkVariableWritePermission.Server)
		: base(compressor(initialValue), readPerm, writePerm)
	{
		this.compressor = compressor;
		this.decompressor = decompressor;
		cachedValue = initialValue;
		OnValueChanged = (OnValueChangedDelegate)Delegate.Combine(OnValueChanged, new OnValueChangedDelegate(OnCompressedValueChanged));
	}

	private void OnCompressedValueChanged(TNetwork previousCompressed, TNetwork newCompressed)
	{
		TRaw arg = cachedValue;
		cachedValue = decompressor(newCompressed);
		OnRawValueChanged?.Invoke(arg, cachedValue);
	}

	public static CompressedNetworkVariable<float, short> CreateFloatToShort(float minValue, float maxValue, float initialValue = 0f, NetworkVariableReadPermission readPerm = NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission writePerm = NetworkVariableWritePermission.Server)
	{
		return new CompressedNetworkVariable<float, short>((float value) =>
		{
			float val = (value - minValue) / (maxValue - minValue);
			return (short)(Math.Max(0f, Math.Min(1f, val)) * 32767f);
		}, (short compressed) =>
		{
			float num = (float)compressed / 32767f;
			return minValue + num * (maxValue - minValue);
		}, initialValue, readPerm, writePerm);
	}

	public static CompressedNetworkVariable<float, byte> CreateFloatToByte(float minValue, float maxValue, float initialValue = 0f, NetworkVariableReadPermission readPerm = NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission writePerm = NetworkVariableWritePermission.Server)
	{
		return new CompressedNetworkVariable<float, byte>((float value) =>
		{
			float val = (value - minValue) / (maxValue - minValue);
			return (byte)(Math.Max(0f, Math.Min(1f, val)) * 255f);
		}, (byte compressed) =>
		{
			float num = (float)(int)compressed / 255f;
			return minValue + num * (maxValue - minValue);
		}, initialValue, readPerm, writePerm);
	}
}
