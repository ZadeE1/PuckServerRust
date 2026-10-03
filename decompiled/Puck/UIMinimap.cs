using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIMinimap : UIView
{
	[Header("Settings")]
	[SerializeField]
	private int updateRate = 120;

	[Header("References")]
	[SerializeField]
	private VisualTreeAsset playerAsset;

	[SerializeField]
	private VisualTreeAsset puckAsset;

	[SerializeField]
	private VisualTreeAsset stickAsset;

	[HideInInspector]
	public PlayerTeam Team;

	[HideInInspector]
	public Bounds Bounds;

	private VisualElement minimap;

	private VisualElement background;

	private VisualElement foreground;

	private VisualElement content;

	private Dictionary<PlayerBody, (VisualElement Root, VisualElement Body)> playerBodyVisualElementMap = new Dictionary<PlayerBody, (VisualElement, VisualElement)>();

	private Dictionary<Puck, VisualElement> puckVisualElementMap = new Dictionary<Puck, VisualElement>();

	private Dictionary<Stick, VisualElement> stickVisualElementMap = new Dictionary<Stick, VisualElement>();

	private float updateAccumulator;

	[HideInInspector]
	public Vector2 Position => new Vector2(minimap.style.left.value.value, minimap.style.top.value.value);

	private void Update()
	{
		if (ApplicationManager.IsDedicatedGameServer || !IsVisible)
		{
			return;
		}
		updateAccumulator += Time.deltaTime;
		if (updateAccumulator < 1f / (float)updateRate)
		{
			return;
		}
		updateAccumulator = Mathf.Min(updateAccumulator - 1f / (float)updateRate, 1f / (float)updateRate);
		foreach (KeyValuePair<PlayerBody, (VisualElement, VisualElement)> item3 in playerBodyVisualElementMap)
		{
			PlayerBody key = item3.Key;
			VisualElement item = item3.Value.Item1;
			VisualElement item2 = item3.Value.Item2;
			if ((bool)key)
			{
				float num = ((Team == PlayerTeam.Blue) ? key.transform.rotation.eulerAngles.y : (key.transform.rotation.eulerAngles.y + 180f));
				ApplyMinimapTranslate(item, key.transform.position);
				item2.style.rotate = new Rotate(num);
				bool flag = SettingsManager.ShowMinimapFallenIndicator && key.HasFallen.Value;
				item.style.opacity = (flag ? 0.4f : 1f);
			}
		}
		foreach (KeyValuePair<Stick, VisualElement> item4 in stickVisualElementMap)
		{
			Stick key2 = item4.Key;
			VisualElement value = item4.Value;
			if (!key2 || !key2.Player)
			{
				continue;
			}
			if (!SettingsManager.ShowMinimapSticks)
			{
				value.parent.style.display = DisplayStyle.None;
				continue;
			}
			value.parent.style.display = DisplayStyle.Flex;
			Vector3 bladeHandlePosition = key2.BladeHandlePosition;
			float num2 = key2.BladeHandleRotation.eulerAngles.y;
			if (Team == PlayerTeam.Red)
			{
				num2 += 180f;
			}
			num2 -= key2.BladeAngle;
			ApplyMinimapTranslate(value.parent, bladeHandlePosition);
			value.style.rotate = new Rotate(num2);
			float num3 = HeightToScale(bladeHandlePosition.y, 3f, 0.5f);
			float num4 = num3;
			if (SettingsManager.ShowMinimapFallenIndicator && (bool)key2.Player.PlayerBody && key2.Player.PlayerBody.HasFallen.Value)
			{
				num4 *= 0.4f;
			}
			value.style.scale = new StyleScale(new Scale(new Vector2(num3, 1f)));
			value.style.opacity = num4;
		}
		foreach (KeyValuePair<Puck, VisualElement> item5 in puckVisualElementMap)
		{
			Puck key3 = item5.Key;
			VisualElement value2 = item5.Value;
			if ((bool)key3)
			{
				ApplyMinimapTranslate(value2, key3.transform.position);
				if (SettingsManager.ShowMinimapPuckElevation)
				{
					float num5 = HeightToScale(key3.transform.position.y, 8f, 0.6f);
					value2.style.opacity = num5;
					value2.style.scale = new StyleScale(new Scale(new Vector2(num5, num5)));
				}
				else
				{
					value2.style.opacity = 1f;
					value2.style.scale = new StyleScale(new Scale(Vector2.one));
				}
			}
		}
	}

	private void ApplyMinimapTranslate(VisualElement element, Vector3 worldPosition)
	{
		Vector3 position = ((Team == PlayerTeam.Blue) ? worldPosition : (-worldPosition));
		Vector2 vector = WorldPositionToMinimapPosition(position, Bounds);
		element.style.translate = new Translate(0f - vector.x, vector.y);
	}

	private static float HeightToScale(float worldY, float maxHeight, float minScale)
	{
		return Mathf.Lerp(1f, minScale, Mathf.Clamp01(Mathf.Abs(worldY) / maxHeight));
	}

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("MinimapView");
		minimap = View.Query<VisualElement>("Minimap");
		background = minimap.Query<VisualElement>("Background");
		foreground = minimap.Query<VisualElement>("Foreground");
		content = minimap.Query<VisualElement>("Content");
		content.Clear();
	}

	public override bool Show()
	{
		if (!SettingsManager.ShowGameUserInterface)
		{
			return false;
		}
		return base.Show();
	}

	public void AddPlayerBody(PlayerBody playerBody)
	{
		if ((bool)playerBody && !playerBodyVisualElementMap.ContainsKey(playerBody))
		{
			TemplateContainer templateContainer = playerAsset.Instantiate();
			playerBodyVisualElementMap.Add(playerBody, (templateContainer, templateContainer.Query<VisualElement>("Body")));
			content.Add(templateContainer);
			templateContainer.SendToBack();
			StylePlayer(playerBody);
		}
	}

	public void StylePlayer(PlayerBody playerBody)
	{
		if ((bool)playerBody && playerBodyVisualElementMap.ContainsKey(playerBody))
		{
			Player player = playerBody.Player;
			if ((bool)player)
			{
				VisualElement visualElement = playerBodyVisualElementMap[playerBody].Root.Query<VisualElement>("Player");
				Label label = visualElement.Query<Label>("NumberLabel");
				UIUtils.SetTeamClass(visualElement, player.Team);
				visualElement.EnableInClassList("isLocalPlayer", player.IsLocalPlayer);
				label.text = player.Number.Value.ToString();
			}
		}
	}

	public void RemovePlayerBody(PlayerBody playerBody)
	{
		if ((bool)playerBody && playerBodyVisualElementMap.ContainsKey(playerBody))
		{
			content.Remove(playerBodyVisualElementMap[playerBody].Root);
			playerBodyVisualElementMap.Remove(playerBody);
		}
	}

	public void AddPuck(Puck puck)
	{
		if ((bool)puck && !puckVisualElementMap.ContainsKey(puck))
		{
			TemplateContainer templateContainer = puckAsset.Instantiate();
			VisualElement visualElement = templateContainer.Q(null, "minimapPuck");
			if (visualElement != null)
			{
				visualElement.generateVisualContent = DrawPuckDot;
			}
			puckVisualElementMap.Add(puck, templateContainer);
			content.Add(templateContainer);
			templateContainer.BringToFront();
		}
	}

	private static void DrawPuckDot(MeshGenerationContext context)
	{
		Rect contentRect = context.visualElement.contentRect;
		if (!(contentRect.width <= 0f) && !(contentRect.height <= 0f))
		{
			float radius = Mathf.Min(contentRect.width, contentRect.height) / 2f;
			Painter2D painter2D = context.painter2D;
			painter2D.fillColor = Color.black;
			painter2D.BeginPath();
			painter2D.Arc(contentRect.center, radius, new Angle(0f, AngleUnit.Degree), new Angle(360f, AngleUnit.Degree));
			painter2D.Fill();
		}
	}

	public void RemovePuck(Puck puck)
	{
		if ((bool)puck && puckVisualElementMap.ContainsKey(puck))
		{
			content.Remove(puckVisualElementMap[puck]);
			puckVisualElementMap.Remove(puck);
		}
	}

	public void AddStick(Stick stick)
	{
		if ((bool)stick && !stickVisualElementMap.ContainsKey(stick))
		{
			TemplateContainer templateContainer = stickAsset.Instantiate();
			VisualElement visualElement = templateContainer.Q(null, "minimapStick");
			UIUtils.SetTeamClass(visualElement, stick.Player ? stick.Player.Team : PlayerTeam.None);
			stickVisualElementMap.Add(stick, visualElement);
			content.Add(templateContainer);
			templateContainer.SendToBack();
		}
	}

	public void RemoveStick(Stick stick)
	{
		if ((bool)stick && stickVisualElementMap.ContainsKey(stick))
		{
			content.Remove(stickVisualElementMap[stick].parent);
			stickVisualElementMap.Remove(stick);
		}
	}

	private Vector2 WorldPositionToMinimapPosition(Vector3 position, Bounds bounds)
	{
		Vector2 vector = new Vector2((position.x + bounds.center.x) / bounds.size.x, (position.z + bounds.center.z) / bounds.size.z);
		Vector2 vector2 = new Vector2(content.resolvedStyle.width, content.resolvedStyle.height);
		return new Vector2(vector2.x * vector.x, vector2.y * vector.y);
	}

	public void SetOpacity(float opacity)
	{
		if (minimap != null)
		{
			minimap.style.opacity = opacity;
		}
	}

	public void SetPosition(Vector2 position)
	{
		if (minimap != null)
		{
			Length x = new Length(Utils.Map(position.x, 0f, 100f, 0f, -100f), LengthUnit.Percent);
			Length y = new Length(Utils.Map(position.y, 0f, 100f, 0f, -100f), LengthUnit.Percent);
			Length x2 = new Length(0f - x.value, LengthUnit.Percent);
			Length y2 = new Length(0f - y.value, LengthUnit.Percent);
			minimap.style.left = new Length(position.x, LengthUnit.Percent);
			minimap.style.top = new Length(position.y, LengthUnit.Percent);
			minimap.style.translate = new Translate(x, y);
			minimap.style.transformOrigin = new TransformOrigin(x2, y2);
		}
	}

	public void SetBackgroundOpacity(float opacity)
	{
		if (background != null)
		{
			background.style.opacity = opacity;
		}
	}

	public void SetScale(float scale)
	{
		if (minimap != null)
		{
			minimap.style.scale = new Vector2(scale, scale);
		}
	}
}
