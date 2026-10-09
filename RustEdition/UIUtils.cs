using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

public static class UIUtils
{
	public static void SetTeamClass(VisualElement element, PlayerTeam team)
	{
		foreach (PlayerTeam value in Enum.GetValues(typeof(PlayerTeam)))
		{
			element.EnableInClassList(GetClassFromTeam(value), enable: false);
		}
		element.EnableInClassList(GetClassFromTeam(team), enable: true);
	}

	public static string GetClassFromTeam(PlayerTeam team)
	{
		return "team" + team;
	}

	public static List<VisualElement> GetVisualElementChildren(VisualElement element, bool recursive = false)
	{
		if (recursive)
		{
			List<VisualElement> list = new List<VisualElement>();
			{
				foreach (VisualElement item in element.hierarchy.Children())
				{
					list.Add(item);
					list.AddRange(GetVisualElementChildren(item, recursive: true));
				}
				return list;
			}
		}
		return new List<VisualElement>(element.hierarchy.Children());
	}
}
