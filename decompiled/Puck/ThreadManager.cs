using System;
using System.Collections;
using System.Collections.Generic;

public class ThreadManager : MonoBehaviourSingleton<ThreadManager>
{
	private Queue<Action> executionQueue = new Queue<Action>();

	private void Update()
	{
		lock (executionQueue)
		{
			while (executionQueue.Count > 0)
			{
				executionQueue.Dequeue()();
			}
		}
	}

	public void Enqueue(IEnumerator action)
	{
		lock (executionQueue)
		{
			executionQueue.Enqueue(() =>
			{
				StartCoroutine(action);
			});
		}
	}

	public void Enqueue(Action action)
	{
		Enqueue(ActionWrapper(action));
	}

	private IEnumerator ActionWrapper(Action action)
	{
		action();
		yield return null;
	}
}
