using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UIElements;

public class UIFooter : UIView
{
	[Header("References")]
	public VisualTreeAsset userAsset;

	private VisualElement footer;

	private VisualElement left;

	private VisualElement center;

	private VisualElement right;

	private VisualElement localUserContainer;

	private Mmr mmr;

	private VisualElement party;

	private VisualElement partyUsers;

	private TemplateContainer createPartyIconButtonInstance;

	private IconButton createPartyIconButton;

	private TemplateContainer inviteIconButtonInstance;

	private IconButton inviteIconButton;

	private TemplateContainer leavePartyIconButtonInstance;

	private IconButton leavePartyIconButton;

	private TemplateContainer disbandPartyIconButtonInstance;

	private IconButton disbandPartyIconButton;

	private Dictionary<string, TemplateContainer> partyUserMap = new Dictionary<string, TemplateContainer>();

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("FooterView");
		footer = View.Query<VisualElement>("Footer");
		left = footer.Query<VisualElement>("Left");
		center = footer.Query<VisualElement>("Center");
		right = footer.Query<VisualElement>("Right");
		localUserContainer = left.Query<VisualElement>("LocalUserContainer");
		mmr = left.Query<VisualElement>("LocalUserMmr").First().Query<Mmr>();
		party = right.Query<VisualElement>("Party");
		partyUsers = party.Query<VisualElement>("Users");
		createPartyIconButtonInstance = party.Query<TemplateContainer>("CreatePartyIconButtonContainer");
		createPartyIconButton = createPartyIconButtonInstance.Query<IconButton>();
		createPartyIconButton.clicked += OnClickCreateParty;
		inviteIconButtonInstance = party.Query<TemplateContainer>("InviteIconButtonContainer");
		inviteIconButton = inviteIconButtonInstance.Query<IconButton>();
		inviteIconButton.clicked += OnClickInvite;
		leavePartyIconButtonInstance = party.Query<TemplateContainer>("LeavePartyIconButtonContainer");
		leavePartyIconButton = leavePartyIconButtonInstance.Query<IconButton>();
		leavePartyIconButton.clicked += OnClickLeaveParty;
		disbandPartyIconButtonInstance = party.Query<TemplateContainer>("DisbandPartyIconButtonContainer");
		disbandPartyIconButton = disbandPartyIconButtonInstance.Query<IconButton>();
		disbandPartyIconButton.clicked += OnClickDisbandParty;
		ClearLocalUser();
		ClearPartyUsers();
		SetCreatePartyButtonVisibility(show: false);
		SetInviteButtonVisibility(show: false);
		SetDisbandPartyButtonVisibility(show: false);
		SetLeavePartyButtonVisibility(show: false);
	}

	public void SetLocalUser(string username, Texture2D avatar)
	{
		ClearLocalUser();
		TemplateContainer child = CreateUser(username, avatar);
		localUserContainer.Add(child);
	}

	public void ClearLocalUser()
	{
		localUserContainer.Clear();
	}

	public void SetMmr(int value)
	{
		mmr.TargetValue = value;
	}

	public void AddPartyUser(string steamId, string username, Texture2D texture)
	{
		if (!partyUserMap.ContainsKey(steamId))
		{
			TemplateContainer templateContainer = CreateUser(username, texture, small: true, hideUsername: true);
			partyUsers.Add(templateContainer);
			partyUsers.style.display = DisplayStyle.Flex;
			partyUserMap.Add(steamId, templateContainer);
		}
	}

	public void RemovePartyUser(string steamId)
	{
		if (partyUserMap.ContainsKey(steamId))
		{
			VisualElement element = partyUserMap[steamId];
			partyUsers.Remove(element);
			partyUsers.style.display = ((partyUsers.childCount <= 0) ? DisplayStyle.None : DisplayStyle.Flex);
			partyUserMap.Remove(steamId);
		}
	}

	public void ClearPartyUsers()
	{
		partyUsers.Clear();
		partyUserMap.Clear();
		partyUsers.style.display = DisplayStyle.None;
	}

	private TemplateContainer CreateUser(string username, Texture2D texture, bool small = false, bool hideUsername = false)
	{
		TemplateContainer templateContainer = userAsset.Instantiate();
		User user = templateContainer.Query<User>();
		user.AvatarTexture = texture;
		user.Username = username;
		templateContainer.EnableInClassList("small", small);
		templateContainer.EnableInClassList("hideUsername", hideUsername);
		return templateContainer;
	}

	public void SetCreatePartyButtonVisibility(bool show)
	{
		createPartyIconButtonInstance.style.display = ((!show) ? DisplayStyle.None : DisplayStyle.Flex);
	}

	public void SetInviteButtonVisibility(bool show)
	{
		inviteIconButtonInstance.style.display = ((!show) ? DisplayStyle.None : DisplayStyle.Flex);
	}

	public void SetLeavePartyButtonVisibility(bool show)
	{
		leavePartyIconButtonInstance.style.display = ((!show) ? DisplayStyle.None : DisplayStyle.Flex);
	}

	public void SetDisbandPartyButtonVisibility(bool show)
	{
		disbandPartyIconButtonInstance.style.display = ((!show) ? DisplayStyle.None : DisplayStyle.Flex);
	}

	private void OnClickCreateParty()
	{
		EventManager.TriggerEvent("Event_OnFooterClickCreateParty");
	}

	private void OnClickInvite()
	{
		EventManager.TriggerEvent("Event_OnFooterClickInvite");
	}

	private void OnClickLeaveParty()
	{
		EventManager.TriggerEvent("Event_OnFooterClickLeaveParty");
	}

	private void OnClickDisbandParty()
	{
		EventManager.TriggerEvent("Event_OnFooterClickDisbandParty");
	}
}
