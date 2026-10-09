public class UIPlayerMenuController : UIViewController<UIPlayerMenu>
{
	private UIPlayerMenu uiPlayerMenu;

	public override void Awake()
	{
		base.Awake();
		uiPlayerMenu = GetComponent<UIPlayerMenu>();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
	}
}
