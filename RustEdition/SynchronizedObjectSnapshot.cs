using System.Collections.Generic;
using UnityEngine;

public class SynchronizedObjectSnapshot
{
	private static readonly Logger Logger = new Logger("SynchronizedObjectSnapshot");

	private List<SynchronizedObject> objects = new List<SynchronizedObject>();

	private List<Vector3> positions = new List<Vector3>();

	private List<SynchronizedObjectData> data = new List<SynchronizedObjectData>();

	public int Count => data.Count;

	public Vector3 GetPosition(int index)
	{
		return positions[index];
	}

	public bool GetUsesLod(int index)
	{
		return objects[index].UseLod;
	}

	public bool GetUsesCulling(int index)
	{
		return objects[index].UseCulling;
	}

	public SynchronizedObjectData GetData(int index)
	{
		return data[index];
	}

	public void Capture(SynchronizedObjectRegistry registry)
	{
		objects.Clear();
		positions.Clear();
		data.Clear();
		foreach (SynchronizedObject @object in registry.Objects)
		{
			if ((bool)@object)
			{
				ulong networkObjectId = @object.NetworkObjectId;
				if (networkObjectId > 65535)
				{
					Logger.Error($"NetworkObjectId {networkObjectId} does not fit in a synchronization record and was skipped");
					continue;
				}
				SynchronizedObjectPose pose = @object.GetPose();
				objects.Add(@object);
				positions.Add(pose.Position);
				SynchronizedObjectData cachedData;
				if (@object.TryGetCachedSyncData(in pose, out cachedData))
				{
					data.Add(cachedData);
				}
				else
				{
					SynchronizedObjectData freshData = new SynchronizedObjectData(networkObjectId, pose.Position, pose.Rotation, pose.LinearVelocity, pose.AngularVelocity);
					@object.StoreSyncData(in pose, in freshData);
					data.Add(freshData);
				}
			}
		}
	}

	public void Clear()
	{
		objects.Clear();
		positions.Clear();
		data.Clear();
	}
}
