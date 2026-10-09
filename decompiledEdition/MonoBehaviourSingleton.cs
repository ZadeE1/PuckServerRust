using UnityEngine;
using UnityEngine.SceneManagement;

public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T instance;

	public static T Instance => instance;

	public virtual void Awake()
	{
		if (instance != null && instance != this)
		{
			Object.Destroy(gameObject);
		}
		else if (instance == null)
		{
			instance = this as T;
			Object.DontDestroyOnLoad(gameObject);
		}
	}

	public void AllowSceneDestruction()
	{
		UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(gameObject, UnityEngine.SceneManagement.SceneManager.GetActiveScene());
	}
}
