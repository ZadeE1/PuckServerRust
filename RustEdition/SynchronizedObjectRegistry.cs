using System.Collections.Generic;

public class SynchronizedObjectRegistry
{
	private List<SynchronizedObject> objects = new List<SynchronizedObject>();

	private Dictionary<ulong, SynchronizedObject> networkObjectIdObjectMap = new Dictionary<ulong, SynchronizedObject>();

	public List<SynchronizedObject> Objects => objects;

	public void Add(SynchronizedObject synchronizedObject)
	{
		objects.Add(synchronizedObject);
		networkObjectIdObjectMap[synchronizedObject.NetworkObjectId] = synchronizedObject;
	}

	public void Remove(SynchronizedObject synchronizedObject)
	{
		objects.Remove(synchronizedObject);
		networkObjectIdObjectMap.Remove(synchronizedObject.NetworkObjectId);
	}

	public bool TryGet(ulong networkObjectId, out SynchronizedObject synchronizedObject)
	{
		if (networkObjectIdObjectMap.TryGetValue(networkObjectId, out synchronizedObject))
		{
			return synchronizedObject != null;
		}
		return false;
	}

	public void Clear()
	{
		objects.Clear();
		networkObjectIdObjectMap.Clear();
	}
}
