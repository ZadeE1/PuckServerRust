using UnityEngine;

public class UIViewController<T> : MonoBehaviour where T : UIView
{
	private T uiView;

	public virtual void Awake()
	{
		uiView = GetComponent<T>();
	}

	public virtual void OnDestroy()
	{
	}
}
