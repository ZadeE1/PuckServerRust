public class SynchronizedObjectSampleRing
{
	private SynchronizedObjectSample[] samples;

	private int head;

	public int Count { get; private set; }

	public SynchronizedObjectSample this[int index] => samples[(head + index) % samples.Length];

	public SynchronizedObjectSample Newest => this[Count - 1];

	public SynchronizedObjectSample Oldest => this[0];

	public SynchronizedObjectSampleRing(int capacity)
	{
		samples = new SynchronizedObjectSample[capacity];
	}

	public void Append(in SynchronizedObjectSample sample)
	{
		samples[(head + Count) % samples.Length] = sample;
		if (Count < samples.Length)
		{
			Count++;
		}
		else
		{
			head = (head + 1) % samples.Length;
		}
	}

	public void Reset()
	{
		head = 0;
		Count = 0;
	}
}
