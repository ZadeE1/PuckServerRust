using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkBehaviourSingleton<T> : NetworkBehaviour where T : NetworkBehaviour
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

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected override string __getTypeName()
	{
		return "NetworkBehaviourSingleton`1";
	}
}
