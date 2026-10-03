using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

public static class HeapCensus
{
	private sealed class ReferenceComparer : IEqualityComparer<object>
	{
		public new bool Equals(object x, object y)
		{
			return ReferenceEquals(x, y);
		}

		public int GetHashCode(object obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}

	private const long TimeCapMs = 10000;

	private const int ObjectCap = 2000000;

	private static readonly Dictionary<Type, long> Counts = new Dictionary<Type, long>();

	private static readonly Dictionary<Type, long> Bytes = new Dictionary<Type, long>();

	public static void Run(string outPath)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		Counts.Clear();
		Bytes.Clear();
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
		long gcLive = GC.GetTotalMemory(true);
		long monoUsed = Profiler.GetMonoUsedSizeLong();
		long monoReserved = Profiler.GetMonoHeapSizeLong();
		long unityAllocated = Profiler.GetTotalAllocatedMemoryLong();
		long unityReserved = Profiler.GetTotalReservedMemoryLong();
		long workingSet = PerformanceMonitor.GetProcessMemoryBytes(workingSet: true);
		long privateBytes = PerformanceMonitor.GetProcessMemoryBytes(workingSet: false);
		HashSet<object> visited = new HashSet<object>(new ReferenceComparer());
		Queue<object> queue = new Queue<object>();
		long seeded = 0;
		foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (assembly.IsDynamic)
			{
				continue;
			}
			string assemblyName = assembly.GetName().Name ?? "";
			if (assemblyName.StartsWith("UnityEngine") || assemblyName.StartsWith("Unity.") || assemblyName == "Unity" || assemblyName.StartsWith("System") || assemblyName == "mscorlib" || assemblyName == "netstandard" || assemblyName.StartsWith("Mono.") || assemblyName.StartsWith("Microsoft.") || assemblyName.StartsWith("I18N"))
			{
				continue;
			}
			Type[] types;
			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				types = ex.Types;
			}
			foreach (Type type in types)
			{
				if (type == null)
				{
					continue;
				}
				FieldInfo[] fields;
				try
				{
					fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
				}
				catch (Exception)
				{
					continue;
				}
				foreach (FieldInfo fieldInfo in fields)
				{
					if (fieldInfo.IsLiteral)
					{
						continue;
					}
					try
					{
						object value = fieldInfo.GetValue(null);
						if (value != null)
						{
							queue.Enqueue(value);
							seeded++;
						}
					}
					catch (Exception)
					{
					}
				}
			}
		}
	 MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
	 foreach (MonoBehaviour monoBehaviour in behaviours)
	 {
	 	if (monoBehaviour != null)
	 	{
	 		queue.Enqueue(monoBehaviour);
	 		seeded++;
	 	}
	 }
	 UnityEngine.Object[] allObjects = Resources.FindObjectsOfTypeAll<UnityEngine.Object>();
	 foreach (UnityEngine.Object unityObject in allObjects)
	 {
	 	if (unityObject != null)
	 	{
	 		queue.Enqueue(unityObject);
	 		seeded++;
	 	}
	 }
		long visitedCount = 0;
		bool capped = false;
		while (queue.Count != 0)
		{
			if (stopwatch.ElapsedMilliseconds > TimeCapMs || visitedCount >= ObjectCap)
			{
				capped = true;
				break;
			}
			object current = queue.Dequeue();
			if (current == null || !visited.Add(current))
			{
				continue;
			}
			visitedCount++;
			if (current is string text)
			{
				Add(typeof(string), 24L + 2L * text.Length);
				continue;
			}
			if (current is Array array)
			{
				long length = array.LongLength;
				int elementSize = ElementSize(array.GetType().GetElementType());
				long size = 32L + length * elementSize;
				Add(current.GetType(), size);
				if (!array.GetType().GetElementType().IsValueType && length <= 1000000)
				{
					foreach (object element in array)
					{
						if (element != null)
						{
							queue.Enqueue(element);
						}
					}
				}
				continue;
			}
			Type type2 = current.GetType();
			if (type2.IsPrimitive || typeof(Enum).IsAssignableFrom(type2))
			{
				Add(type2, ElementSize(type2));
				continue;
			}
			if (type2.FullName != null && (type2.FullName.StartsWith("System.Reflection") || type2.FullName.StartsWith("System.RuntimeType") || type2.FullName.StartsWith("System.MonoType") || typeof(Type).IsAssignableFrom(type2)))
			{
				Add(type2, 64L);
				continue;
			}
			long shallow = 24L;
			FieldInfo[] fields2 = GetInstanceFields(type2);
			foreach (FieldInfo fieldInfo2 in fields2)
			{
				Type fieldType = fieldInfo2.FieldType;
				if (fieldType.IsValueType)
				{
					shallow += SizeOfValueType(fieldType, 0);
					continue;
				}
				shallow += IntPtr.Size;
				object fieldValue;
				try
				{
					fieldValue = fieldInfo2.GetValue(current);
				}
				catch (Exception)
				{
					continue;
				}
				if (fieldValue != null)
				{
					queue.Enqueue(fieldValue);
				}
			}
			Add(type2, shallow);
		}
		stopwatch.Stop();
		long gcTotal = gcLive;
		long accounted = 0;
		foreach (KeyValuePair<Type, long> item in Bytes)
		{
			accounted += item.Value;
		}
		List<KeyValuePair<Type, long>> list = new List<KeyValuePair<Type, long>>(Bytes);
		list.Sort((KeyValuePair<Type, long> a, KeyValuePair<Type, long> b) => b.Value.CompareTo(a.Value));
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("=== heap census ===\n");
		stringBuilder.Append("gc_live_mb=").Append(Mb(gcLive)).Append(" mono_used_mb=").Append(Mb(monoUsed))
			.Append(" mono_reserved_mb=")
			.Append(Mb(monoReserved))
			.Append(" unity_allocated_mb=")
			.Append(Mb(unityAllocated))
			.Append(" unity_reserved_mb=")
			.Append(Mb(unityReserved))
			.Append(" working_set_mb=")
			.Append(Mb(workingSet))
			.Append(" private_mb=")
			.Append(Mb(privateBytes))
			.Append('\n');
		stringBuilder.Append("accounted_mb=").Append(Mb(accounted)).Append(" coverage=")
			.Append((accounted * 100 / Math.Max(1L, gcLive)).ToString(CultureInfo.InvariantCulture))
			.Append("%\n");
		stringBuilder.Append("objects_visited=")
			.Append(visitedCount)
			.Append(" roots=")
			.Append(seeded)
			.Append(" duration_ms=")
			.Append(stopwatch.ElapsedMilliseconds)
			.Append(capped ? " CAPPED" : "")
			.Append('\n');
		stringBuilder.Append("type\tcount\tmb\tshare\n");
		int written = 0;
		foreach (KeyValuePair<Type, long> item2 in list)
		{
			if (written >= 80)
			{
				break;
			}
			Counts.TryGetValue(item2.Key, out long count);
			stringBuilder.Append(item2.Key.FullName ?? item2.Key.Name)
				.Append('\t')
				.Append(count)
				.Append('\t')
				.Append(Mb(item2.Value))
				.Append('\t')
				.Append((item2.Value * 100 / Math.Max(1L, gcTotal)).ToString(CultureInfo.InvariantCulture))
				.Append("%\n");
			written++;
		}
		File.AppendAllText(outPath, stringBuilder.ToString());
		visited = null;
		queue = null;
		Counts.Clear();
		Bytes.Clear();
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
	}

	private static void Add(Type type, long size)
	{
		Counts.TryGetValue(type, out long value);
		Counts[type] = value + 1;
		Bytes.TryGetValue(type, out long value2);
		Bytes[type] = value2 + size;
	}

	private static FieldInfo[] GetInstanceFields(Type type)
	{
		List<FieldInfo> list = new List<FieldInfo>();
		for (Type type2 = type; type2 != null && type2 != typeof(object); type2 = type2.BaseType)
		{
			list.AddRange(type2.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
		}
		return list.ToArray();
	}

	private static int ElementSize(Type type)
	{
		if (type == null)
		{
			return IntPtr.Size;
		}
		if (!type.IsValueType)
		{
			return IntPtr.Size;
		}
		return SizeOfValueType(type, 0);
	}

	private static int SizeOfValueType(Type type, int depth)
	{
		if (type.IsEnum)
		{
			type = Enum.GetUnderlyingType(type);
		}
		if (type == typeof(bool) || type == typeof(byte) || type == typeof(sbyte))
		{
			return 1;
		}
		if (type == typeof(char) || type == typeof(short) || type == typeof(ushort) || type == typeof(bool))
		{
			return 2;
		}
		if (type == typeof(int) || type == typeof(uint) || type == typeof(float))
		{
			return 4;
		}
		if (type == typeof(long) || type == typeof(ulong) || type == typeof(double) || type == typeof(IntPtr) || type == typeof(UIntPtr))
		{
			return 8;
		}
		if (type == typeof(decimal))
		{
			return 16;
		}
		if (depth > 3)
		{
			return 8;
		}
		int total = 0;
		foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
		{
			if (fieldInfo.FieldType.IsValueType)
			{
				total += SizeOfValueType(fieldInfo.FieldType, depth + 1);
			}
			else
			{
				total += IntPtr.Size;
			}
		}
		return (total == 0) ? 1 : total;
	}

	private static string Mb(long bytes)
	{
		return (bytes / 1048576.0).ToString("F1", CultureInfo.InvariantCulture);
	}
}
