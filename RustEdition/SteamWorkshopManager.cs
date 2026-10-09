using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DG.Tweening;
using Steamworks;

public static class SteamWorkshopManager
{
	private static readonly Logger Logger = new Logger("SteamWorkshopManager");

	public static List<SteamWorkshopItem> Items = new List<SteamWorkshopItem>();

	private static Callback<DownloadItemResult_t> DownloadItemResult;

	private static Callback<UserSubscribedItemsListChanged_t> UserSubscribedItemsListChanged;

	private static Callback<RemoteStorageSubscribePublishedFileResult_t> RemoteStorageSubscribePublishedFileResult;

	private static Callback<RemoteStorageUnsubscribePublishedFileResult_t> RemoteStorageUnsubscribePublishedFileResult;

	private static Dictionary<UGCQueryHandle_t, CallResult<SteamUGCQueryCompleted_t>> ugcQueryCompletedCallResultMap = new Dictionary<UGCQueryHandle_t, CallResult<SteamUGCQueryCompleted_t>>();

	private static List<string> debouncedGetItemDetailsItemIds = new List<string>();

	private static Tween getItemDetailsDebounceTween;

	public static string[] ItemIds => Items.Select((SteamWorkshopItem item) => item.Id).ToArray();

	public static void Initialize()
	{
		RegisterCallbacks();
		SteamWorkshopManagerController.Initialize();
	}

	public static void Dispose()
	{
		ugcQueryCompletedCallResultMap.Values.ToList().ForEach((CallResult<SteamUGCQueryCompleted_t> callResult) =>
		{
			callResult.Dispose();
		});
		ugcQueryCompletedCallResultMap.Clear();
		debouncedGetItemDetailsItemIds.Clear();
		getItemDetailsDebounceTween?.Kill();
		SteamWorkshopManagerController.Dispose();
		UnregisterCallbacks();
	}

	private static void RegisterCallbacks()
	{
		if (SteamManager.IsInitialized)
		{
			if (ApplicationManager.IsDedicatedGameServer)
			{
				DownloadItemResult = Callback<DownloadItemResult_t>.CreateGameServer(OnDownloadItemResult);
				UserSubscribedItemsListChanged = Callback<UserSubscribedItemsListChanged_t>.CreateGameServer(OnUserSubscribedItemsListChanged);
				RemoteStorageSubscribePublishedFileResult = Callback<RemoteStorageSubscribePublishedFileResult_t>.CreateGameServer(OnRemoteStorageSubscribePublishedFileResult);
				RemoteStorageUnsubscribePublishedFileResult = Callback<RemoteStorageUnsubscribePublishedFileResult_t>.CreateGameServer(OnRemoteStorageUnsubscribePublishedFileResult);
			}
			else
			{
				DownloadItemResult = Callback<DownloadItemResult_t>.Create(OnDownloadItemResult);
				UserSubscribedItemsListChanged = Callback<UserSubscribedItemsListChanged_t>.Create(OnUserSubscribedItemsListChanged);
				RemoteStorageSubscribePublishedFileResult = Callback<RemoteStorageSubscribePublishedFileResult_t>.Create(OnRemoteStorageSubscribePublishedFileResult);
				RemoteStorageUnsubscribePublishedFileResult = Callback<RemoteStorageUnsubscribePublishedFileResult_t>.Create(OnRemoteStorageUnsubscribePublishedFileResult);
			}
		}
	}

	private static void UnregisterCallbacks()
	{
		if (SteamManager.IsInitialized)
		{
			DownloadItemResult.Unregister();
			UserSubscribedItemsListChanged.Unregister();
			RemoteStorageSubscribePublishedFileResult.Unregister();
			RemoteStorageUnsubscribePublishedFileResult.Unregister();
		}
	}

	private static SteamWorkshopItem AddItem(string id, string path = null)
	{
		if (GetItemById(id) != null)
		{
			return null;
		}
		SteamWorkshopItem steamWorkshopItem = new SteamWorkshopItem(id, path);
		Items.Add(steamWorkshopItem);
		steamWorkshopItem.Initialize();
		EventManager.TriggerEvent("Event_OnSteamWorkshopItemAdded", new Dictionary<string, object> { { "item", steamWorkshopItem } });
		return steamWorkshopItem;
	}

	private static SteamWorkshopItem RemoveItem(string id)
	{
		SteamWorkshopItem itemById = GetItemById(id);
		if (itemById == null)
		{
			return null;
		}
		Items.Remove(itemById);
		itemById.Dispose();
		EventManager.TriggerEvent("Event_OnSteamWorkshopItemRemoved", new Dictionary<string, object> { { "item", itemById } });
		return itemById;
	}

	public static SteamWorkshopItem GetItemById(string id)
	{
		return Items.Find((SteamWorkshopItem item) => item.Id == id);
	}

	public static void VerifyIntegrity()
	{
		string[] subscribedItemIds = GetSubscribedItemIds();
		string[] array = ItemIds.Union(subscribedItemIds).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			VerifyItemIntegrity(array[i]);
		}
	}

	public static void VerifyItemIntegrity(string itemId)
	{
		SteamWorkshopItem steamWorkshopItem = GetItemById(itemId);
		if (IsItemSubscribed(itemId) || ApplicationManager.IsDedicatedGameServer)
		{
			if (steamWorkshopItem == null)
			{
				steamWorkshopItem = AddItem(itemId);
			}
			if (IsItemInstalled(itemId))
			{
				string path;
				if (IsItemNeedsUpdate(itemId))
				{
					steamWorkshopItem.SetState(new Dictionary<string, object> { 
					{
						"phase",
						SteamWorkshopItemPhase.Updating
					} });
					DownloadItem(itemId);
				}
				else if (GetItemInstallInfo(itemId, out path))
				{
					steamWorkshopItem.SetState(new Dictionary<string, object>
					{
						{ "path", path },
						{
							"phase",
							SteamWorkshopItemPhase.Installed
						}
					});
				}
				else
				{
					Logger.Error("Failed to get install info for item " + itemId);
				}
			}
			else
			{
				steamWorkshopItem.SetState(new Dictionary<string, object> { 
				{
					"phase",
					SteamWorkshopItemPhase.Downloading
				} });
				DownloadItem(itemId);
			}
		}
		else if (!IsItemSubscribed(itemId) && !ApplicationManager.IsDedicatedGameServer && steamWorkshopItem != null)
		{
			RemoveItem(itemId);
		}
	}

	public static bool IsItemInstalled(string itemId)
	{
		if (!SteamManager.IsInitialized)
		{
			return false;
		}
		return (GetItemState(itemId) & 4) != 0;
	}

	public static bool IsItemSubscribed(string itemId)
	{
		if (!SteamManager.IsInitialized)
		{
			return false;
		}
		return (GetItemState(itemId) & 1) != 0;
	}

	public static bool IsItemNeedsUpdate(string itemId)
	{
		if (!SteamManager.IsInitialized)
		{
			return false;
		}
		return (GetItemState(itemId) & 8) != 0;
	}

	public static uint GetNumSubscribedItems()
	{
		if (!SteamManager.IsInitialized)
		{
			return 0u;
		}
		if (ApplicationManager.IsDedicatedGameServer)
		{
			return SteamGameServerUGC.GetNumSubscribedItems();
		}
		return SteamUGC.GetNumSubscribedItems();
	}

	public static string[] GetSubscribedItemIds()
	{
		if (!SteamManager.IsInitialized)
		{
			return null;
		}
		uint numSubscribedItems = GetNumSubscribedItems();
		PublishedFileId_t[] array = new PublishedFileId_t[numSubscribedItems];
		if (ApplicationManager.IsDedicatedGameServer)
		{
			SteamGameServerUGC.GetSubscribedItems(array, numSubscribedItems);
		}
		else
		{
			SteamUGC.GetSubscribedItems(array, numSubscribedItems);
		}
		return array.Select((PublishedFileId_t id) => id.m_PublishedFileId.ToString()).ToArray();
	}

	public static bool GetItemInstallInfo(string itemId, out string path)
	{
		path = null;
		if (!SteamManager.IsInitialized)
		{
			return false;
		}
		PublishedFileId_t nPublishedFileID = new PublishedFileId_t(ulong.Parse(itemId));
		ulong punSizeOnDisk;
		uint punTimeStamp;
		if (ApplicationManager.IsDedicatedGameServer)
		{
			return SteamGameServerUGC.GetItemInstallInfo(nPublishedFileID, out punSizeOnDisk, out path, 4096u, out punTimeStamp);
		}
		ulong punSizeOnDisk2;
		uint punTimeStamp2;
		return SteamUGC.GetItemInstallInfo(nPublishedFileID, out punSizeOnDisk2, out path, 4096u, out punTimeStamp2);
	}

	public static bool GetItemDownloadInfo(string itemId, out ulong bytesDownloaded, out ulong bytesTotal)
	{
		bytesDownloaded = 0uL;
		bytesTotal = 0uL;
		if (!SteamManager.IsInitialized)
		{
			return false;
		}
		PublishedFileId_t nPublishedFileID = new PublishedFileId_t(ulong.Parse(itemId));
		if (ApplicationManager.IsDedicatedGameServer)
		{
			return SteamGameServerUGC.GetItemDownloadInfo(nPublishedFileID, out bytesDownloaded, out bytesTotal);
		}
		return SteamUGC.GetItemDownloadInfo(nPublishedFileID, out bytesDownloaded, out bytesTotal);
	}

	public static uint GetItemState(string itemId)
	{
		if (!SteamManager.IsInitialized)
		{
			return 0u;
		}
		PublishedFileId_t nPublishedFileID = new PublishedFileId_t(ulong.Parse(itemId));
		if (ApplicationManager.IsDedicatedGameServer)
		{
			return SteamGameServerUGC.GetItemState(nPublishedFileID);
		}
		return SteamUGC.GetItemState(nPublishedFileID);
	}

	public static void GetItemDetails(params string[] itemIds)
	{
		if (!SteamManager.IsInitialized || itemIds == null || itemIds.Length == 0)
		{
			return;
		}
		debouncedGetItemDetailsItemIds.AddRange(itemIds.Where((string id) => !debouncedGetItemDetailsItemIds.Contains(id)));
		getItemDetailsDebounceTween?.Kill();
		getItemDetailsDebounceTween = DOVirtual.DelayedCall(0f, () =>
		{
			UGCQueryHandle_t uGCQueryHandle_t = CreateQueryUGCDetailsRequest(debouncedGetItemDetailsItemIds.ToArray());
			debouncedGetItemDetailsItemIds.Clear();
			CallResult<SteamUGCQueryCompleted_t> callResult = CallResult<SteamUGCQueryCompleted_t>.Create(OnUGCQueryCompleted);
			ugcQueryCompletedCallResultMap[uGCQueryHandle_t] = callResult;
			SteamAPICall_t steamAPICall_t = SendQueryUGCRequest(uGCQueryHandle_t);
			if (!(steamAPICall_t == SteamAPICall_t.Invalid))
			{
				callResult.Set(steamAPICall_t);
			}
		});
	}

	public static UGCQueryHandle_t CreateQueryUGCDetailsRequest(string[] itemIds)
	{
		if (!SteamManager.IsInitialized)
		{
			return UGCQueryHandle_t.Invalid;
		}
		PublishedFileId_t[] array = new PublishedFileId_t[itemIds.Length];
		for (int i = 0; i < itemIds.Length; i++)
		{
			array[i] = new PublishedFileId_t(ulong.Parse(itemIds[i]));
		}
		UGCQueryHandle_t uGCQueryHandle_t;
		if (ApplicationManager.IsDedicatedGameServer)
		{
			uGCQueryHandle_t = SteamGameServerUGC.CreateQueryUGCDetailsRequest(array, (uint)array.Length);
			SteamGameServerUGC.SetReturnLongDescription(uGCQueryHandle_t, bReturnLongDescription: true);
		}
		else
		{
			uGCQueryHandle_t = SteamUGC.CreateQueryUGCDetailsRequest(array, (uint)array.Length);
			SteamUGC.SetReturnLongDescription(uGCQueryHandle_t, bReturnLongDescription: true);
		}
		return uGCQueryHandle_t;
	}

	public static SteamAPICall_t SendQueryUGCRequest(UGCQueryHandle_t queryHandle)
	{
		if (!SteamManager.IsInitialized)
		{
			return SteamAPICall_t.Invalid;
		}
		if (ApplicationManager.IsDedicatedGameServer)
		{
			return SteamGameServerUGC.SendQueryUGCRequest(queryHandle);
		}
		return SteamUGC.SendQueryUGCRequest(queryHandle);
	}

	private static bool GetQueryUGCResult(UGCQueryHandle_t queryHandle, uint index, out SteamUGCDetails_t details)
	{
		details = default;
		if (!SteamManager.IsInitialized)
		{
			return false;
		}
		if (ApplicationManager.IsDedicatedGameServer)
		{
			return SteamGameServerUGC.GetQueryUGCResult(queryHandle, index, out details);
		}
		return SteamUGC.GetQueryUGCResult(queryHandle, index, out details);
	}

	private static bool GetQueryUGCPreviewURL(UGCQueryHandle_t queryHandle, uint index, out string previewUrl)
	{
		previewUrl = null;
		if (!SteamManager.IsInitialized)
		{
			return false;
		}
		if (ApplicationManager.IsDedicatedGameServer)
		{
			return SteamGameServerUGC.GetQueryUGCPreviewURL(queryHandle, index, out previewUrl, 2048u);
		}
		return SteamUGC.GetQueryUGCPreviewURL(queryHandle, index, out previewUrl, 2048u);
	}

	private static bool GetQueryUGCMetadata(UGCQueryHandle_t queryHandle, uint index, out string metadata)
	{
		metadata = null;
		if (!SteamManager.IsInitialized)
		{
			return false;
		}
		if (ApplicationManager.IsDedicatedGameServer)
		{
			return SteamGameServerUGC.GetQueryUGCMetadata(queryHandle, index, out metadata, 8000u);
		}
		return SteamUGC.GetQueryUGCMetadata(queryHandle, index, out metadata, 8000u);
	}

	private static bool GetQueryUGCStatistic(UGCQueryHandle_t queryHandle, uint index, EItemStatistic eStatType, out ulong statValue)
	{
		statValue = 0uL;
		if (!SteamManager.IsInitialized)
		{
			return false;
		}
		if (ApplicationManager.IsDedicatedGameServer)
		{
			return SteamGameServerUGC.GetQueryUGCStatistic(queryHandle, index, eStatType, out statValue);
		}
		return SteamUGC.GetQueryUGCStatistic(queryHandle, index, eStatType, out statValue);
	}

	public static bool DownloadItem(string itemId)
	{
		if (!SteamManager.IsInitialized)
		{
			return false;
		}
		Logger.Info("Downloading item " + itemId);
		PublishedFileId_t nPublishedFileID = new PublishedFileId_t(ulong.Parse(itemId));
		if (ApplicationManager.IsDedicatedGameServer)
		{
			return SteamGameServerUGC.DownloadItem(nPublishedFileID, bHighPriority: true);
		}
		return SteamUGC.DownloadItem(nPublishedFileID, bHighPriority: true);
	}

	public static void SubscribeItem(string itemId)
	{
		if (SteamManager.IsInitialized)
		{
			Logger.Info("Subscribing item " + itemId);
			PublishedFileId_t nPublishedFileID = new PublishedFileId_t(ulong.Parse(itemId));
			if (ApplicationManager.IsDedicatedGameServer)
			{
				SteamGameServerUGC.SubscribeItem(nPublishedFileID);
			}
			else
			{
				SteamUGC.SubscribeItem(nPublishedFileID);
			}
		}
	}

	public static void UnsubscribeItem(string itemId)
	{
		if (SteamManager.IsInitialized)
		{
			Logger.Info("Unsubscribing item " + itemId);
			PublishedFileId_t nPublishedFileID = new PublishedFileId_t(ulong.Parse(itemId));
			if (ApplicationManager.IsDedicatedGameServer)
			{
				SteamGameServerUGC.UnsubscribeItem(nPublishedFileID);
			}
			else
			{
				SteamUGC.UnsubscribeItem(nPublishedFileID);
			}
		}
	}

	private static void OnItemUpdated(object sender, PropertyChangedEventArgs e)
	{
		SteamWorkshopItem value = (SteamWorkshopItem)sender;
		EventManager.TriggerEvent("Event_OnSteamWorkshopItemUpdated", new Dictionary<string, object> { { "item", value } });
	}

	private static void OnDownloadItemResult(DownloadItemResult_t response)
	{
		if (!(response.m_unAppID != new AppId_t(2994020u)) && response.m_eResult == EResult.k_EResultOK)
		{
			string value = response.m_nPublishedFileId.ToString();
			EventManager.TriggerEvent("Event_OnSteamWorkshopItemDownloaded", new Dictionary<string, object> { { "itemId", value } });
		}
	}

	private static void OnUserSubscribedItemsListChanged(UserSubscribedItemsListChanged_t response)
	{
		if (!(response.m_nAppID != new AppId_t(2994020u)))
		{
			EventManager.TriggerEvent("Event_OnSteamWorkshopSubscribedItemsListChanged");
		}
	}

	private static void OnRemoteStorageSubscribePublishedFileResult(RemoteStorageSubscribePublishedFileResult_t response)
	{
		if (response.m_eResult == EResult.k_EResultOK)
		{
			string value = response.m_nPublishedFileId.ToString();
			EventManager.TriggerEvent("Event_OnSteamWorkshopItemSubscribed", new Dictionary<string, object> { { "itemId", value } });
		}
	}

	private static void OnRemoteStorageUnsubscribePublishedFileResult(RemoteStorageUnsubscribePublishedFileResult_t response)
	{
		if (response.m_eResult == EResult.k_EResultOK)
		{
			string value = response.m_nPublishedFileId.ToString();
			EventManager.TriggerEvent("Event_OnSteamWorkshopItemUnsubscribed", new Dictionary<string, object> { { "itemId", value } });
		}
	}

	private static void OnUGCQueryCompleted(SteamUGCQueryCompleted_t response, bool bIOFailure)
	{
		if (response.m_eResult != EResult.k_EResultOK)
		{
			return;
		}
		for (uint num = 0u; num < response.m_unNumResultsReturned; num++)
		{
			if (GetQueryUGCResult(response.m_handle, num, out var details) && details.m_eResult == EResult.k_EResultOK)
			{
				PublishedFileId_t nPublishedFileId = details.m_nPublishedFileId;
				string rgchTitle = details.m_rgchTitle;
				string rgchDescription = details.m_rgchDescription;
				int unVotesUp = (int)details.m_unVotesUp;
				int unVotesDown = (int)details.m_unVotesDown;
				GetQueryUGCPreviewURL(response.m_handle, num, out var previewUrl);
				GetQueryUGCStatistic(response.m_handle, num, EItemStatistic.k_EItemStatistic_NumSubscriptions, out var statValue);
				GetQueryUGCMetadata(response.m_handle, num, out var metadata);
				EventManager.TriggerEvent("Event_OnSteamWorkshopItemDetails", new Dictionary<string, object>
				{
					{
						"id",
						nPublishedFileId.ToString()
					},
					{ "title", rgchTitle },
					{ "description", rgchDescription },
					{ "previewUrl", previewUrl },
					{
						"subscriptions",
						(int)statValue
					},
					{ "upvotes", unVotesUp },
					{ "downvotes", unVotesDown },
					{ "metadata", metadata }
				});
			}
		}
		if (ugcQueryCompletedCallResultMap.ContainsKey(response.m_handle))
		{
			ugcQueryCompletedCallResultMap[response.m_handle].Dispose();
			ugcQueryCompletedCallResultMap.Remove(response.m_handle);
		}
		if (ApplicationManager.IsDedicatedGameServer)
		{
			SteamGameServerUGC.ReleaseQueryUGCRequest(response.m_handle);
		}
		else
		{
			SteamUGC.ReleaseQueryUGCRequest(response.m_handle);
		}
	}
}
