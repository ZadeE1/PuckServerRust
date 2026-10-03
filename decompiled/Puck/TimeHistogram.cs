using System;

public class TimeHistogram
{
	public const float DEFAULT_BUCKET_SIZE = 0.0005f;

	public const int DEFAULT_BUCKET_COUNT = 101;

	private int[] buckets;

	private double sum;

	private double squaredSum;

	private double min = double.MaxValue;

	private double max;

	public float BucketSize { get; }

	public int Count { get; private set; }

	public double Min
	{
		get
		{
			if (Count <= 0)
			{
				return 0.0;
			}
			return min;
		}
	}

	public double Max => max;

	public double Average
	{
		get
		{
			if (Count <= 0)
			{
				return 0.0;
			}
			return sum / (double)Count;
		}
	}

	public double StandardDeviation
	{
		get
		{
			if (Count <= 0)
			{
				return 0.0;
			}
			return Math.Sqrt(Math.Max(0.0, squaredSum / (double)Count - Average * Average));
		}
	}

	public TimeHistogram(float bucketSize = 0.0005f, int bucketCount = 101)
	{
		BucketSize = bucketSize;
		buckets = new int[bucketCount];
	}

	public void Add(float time)
	{
		int num = Math.Clamp((int)(time / BucketSize), 0, buckets.Length - 1);
		buckets[num]++;
		Count++;
		sum += time;
		squaredSum += (double)time * (double)time;
		min = Math.Min(min, time);
		max = Math.Max(max, time);
	}

	public double GetPercentile(double percentile)
	{
		if (Count == 0)
		{
			return 0.0;
		}
		int num = (int)Math.Ceiling(percentile * (double)Count);
		int num2 = 0;
		for (int i = 0; i < buckets.Length - 1; i++)
		{
			num2 += buckets[i];
			if (num2 >= num)
			{
				return Math.Min((double)(i + 1) * (double)BucketSize, max);
			}
		}
		return max;
	}

	public int[] ToArray()
	{
		return (int[])buckets.Clone();
	}

	public void Reset()
	{
		Array.Clear(buckets, 0, buckets.Length);
		Count = 0;
		sum = 0.0;
		squaredSum = 0.0;
		min = double.MaxValue;
		max = 0.0;
	}
}
