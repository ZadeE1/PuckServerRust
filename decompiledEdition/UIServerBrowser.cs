using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UI;
using UnityEngine;
using UnityEngine.UIElements;

public class UIServerBrowser : UIView
{
	private static readonly Logger Logger = new Logger("UIServerBrowser");

	private const int PingConcurrency = 8;

	private const int PingConnectTimeoutMs = 1000;

	private const int PingResponseTimeoutMs = 1000;

	private const int SortDebounceMs = 200;

	private const int DefaultMaxPing = 100;

	[Header("References")]
	[SerializeField]
	private VisualTreeAsset serverAsset;

	private VisualElement serverBrowser;

	private VisualElement filters;

	private IconButton closeIconButton;

	private VisualElement serverList;

	private Button nameButton;

	private Button playersButton;

	private Button pingButton;

	private VisualElement nameSortArrow;

	private VisualElement playersSortArrow;

	private VisualElement pingSortArrow;

	private Button refreshButton;

	private Button hostGameButton;

	private Button directConnectButton;

	private TextField searchTextField;

	private CurvedSlider maxPingSlider;

	private int maxPingValue = 100;

	private Label resultCountLabel;

	private Toggle showFullToggle;

	private Toggle showEmptyToggle;

	private Toggle showPasswordProtectedToggle;

	private Toggle showModdedToggle;

	private Toggle showUnreachableToggle;

	private Dictionary<EndPoint, VisualElement> endPointVisualElementMap = new Dictionary<EndPoint, VisualElement>();

	private ServerSortType sortType = ServerSortType.Players;

	private ServerSortDirection sortDirection = ServerSortDirection.Descending;

	private int refreshGeneration;

	private IVisualElementScheduledItem pendingSortFlush;

	private CancellationTokenSource pingWaveCts;

	public int ServerCount => endPointVisualElementMap.Count;

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("ServerBrowserView");
		serverBrowser = View.Query<VisualElement>("ServerBrowser");
		serverList = serverBrowser.Query<VisualElement>("ServerList");
		filters = View.Query<VisualElement>("Filters");
		closeIconButton = serverBrowser.Query<TemplateContainer>("CloseIconButtonContainer").First().Query<IconButton>();
		closeIconButton.clicked += OnServerBrowserClickClose;
		nameButton = serverBrowser.Query<Button>("NameButton");
		nameButton.clicked += OnClickNameSort;
		nameSortArrow = nameButton.Query<VisualElement>("NameSortArrow");
		playersButton = serverBrowser.Query<Button>("PlayersButton");
		playersButton.clicked += OnClickPlayersSort;
		playersSortArrow = playersButton.Query<VisualElement>("PlayersSortArrow");
		pingButton = serverBrowser.Query<Button>("PingButton");
		pingButton.clicked += OnClickPingSort;
		pingSortArrow = pingButton.Query<VisualElement>("PingSortArrow");
		refreshButton = filters.Query<Button>("RefreshButton");
		refreshButton.clicked += OnClickRefresh;
		hostGameButton = View.Query<Button>("HostGameButton");
		hostGameButton.clicked += OnClickHostGame;
		directConnectButton = View.Query<Button>("DirectConnectButton");
		directConnectButton.clicked += OnClickDirectConnect;
		searchTextField = filters.Query<VisualElement>("SearchTextField").First().Query<TextField>();
		searchTextField.value = string.Empty;
		searchTextField.RegisterCallback<ChangeEvent<string>>(OnChangeSearchTextField);
		resultCountLabel = filters.Query<Label>("ResultCountLabel");
		maxPingSlider = filters.Query<CurvedSlider>("MaxPingSlider");
		maxPingSlider.SetMappedValueWithoutNotify(100);
		CurvedSlider curvedSlider = maxPingSlider;
		curvedSlider.MappedValueChanged = (Action<int>)Delegate.Combine(curvedSlider.MappedValueChanged, new Action<int>(OnChangeMaxPing));
		showFullToggle = filters.Query<VisualElement>("ShowFullToggle").First().Query<Toggle>();
		showFullToggle.value = true;
		showFullToggle.RegisterCallback<ChangeEvent<bool>>(OnChangeShowFullToggle);
		showEmptyToggle = filters.Query<VisualElement>("ShowEmptyToggle").First().Query<Toggle>();
		showEmptyToggle.value = true;
		showEmptyToggle.RegisterCallback<ChangeEvent<bool>>(OnChangeShowEmptyToggle);
		showPasswordProtectedToggle = filters.Query<VisualElement>("ShowPasswordProtectedToggle").First().Query<Toggle>();
		showPasswordProtectedToggle.value = true;
		showPasswordProtectedToggle.RegisterCallback<ChangeEvent<bool>>(OnChangeShowPasswordProtectedToggle);
		showModdedToggle = filters.Query<VisualElement>("ShowModdedToggle").First().Query<Toggle>();
		showModdedToggle.value = true;
		showModdedToggle.RegisterCallback<ChangeEvent<bool>>(OnChangeShowModdedToggle);
		showUnreachableToggle = filters.Query<VisualElement>("ShowUnreachableToggle").First().Query<Toggle>();
		showUnreachableToggle.value = false;
		showUnreachableToggle.RegisterCallback<ChangeEvent<bool>>(OnChangeShowUnreachableToggle);
		serverList.Clear();
		StyleSortButtons();
	}

	public override bool Show()
	{
		bool flag = base.Show();
		if (flag)
		{
			EventManager.TriggerEvent("Event_OnServerBrowserShow");
		}
		return flag;
	}

	public void Refresh()
	{
		WebSocketManager.Emit("playerGetServerBrowserEndPointsRequest", null, "playerGetServerBrowserEndPointsResponse");
	}

	public void UpdateEndPoints(EndPoint[] endPoints)
	{
		RemoveAllServers();
		refreshButton.SetEnabled(value: false);
		foreach (EndPoint endPoint in endPoints)
		{
			AddServer(endPoint);
		}
		FilterServers();
		SortServers();
		int generation = ++refreshGeneration;
		if (endPoints.Length == 0)
		{
			refreshButton.SetEnabled(value: true);
			return;
		}
		ConcurrentQueue<EndPoint> queue = new ConcurrentQueue<EndPoint>(endPoints);
		int num = Math.Max(1, Math.Min(8, endPoints.Length));
		pingWaveCts?.Cancel();
		CancellationTokenSource cts = (pingWaveCts = new CancellationTokenSource());
		CancellationToken token = cts.Token;
		EnsurePingThreadPoolFloor(num);
		Task[] workers = new Task[num];
		for (int j = 0; j < num; j++)
		{
			workers[j] = Task.Factory.StartNew(() =>
			{
				while (true)
				{
					if (token.IsCancellationRequested || !queue.TryDequeue(out var endPoint2))
					{
						break;
					}
					ServerPreviewData previewData = PingServer(endPoint2, 1000, 1000);
					MonoBehaviourSingleton<ThreadManager>.Instance.Enqueue(() =>
					{
						if (generation == refreshGeneration)
						{
							SetServerPreviewData(endPoint2, previewData);
							StyleServer(endPoint2);
							FilterServer(endPoint2);
							ScheduleSortFlush();
						}
					});
				}
			}, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
		}
		Task.Run(async () =>
		{
			try
			{
				await Task.WhenAll(workers);
			}
			catch (Exception ex)
			{
				Logger.Error("Server browser ping wave failed: " + ex.Message);
			}
			MonoBehaviourSingleton<ThreadManager>.Instance.Enqueue(() =>
			{
				if (pingWaveCts == cts)
				{
					pingWaveCts = null;
				}
				cts.Dispose();
				if (generation == refreshGeneration)
				{
					FlushSort();
					refreshButton.SetEnabled(value: true);
				}
			});
		});
	}

	private void ScheduleSortFlush()
	{
		if (pendingSortFlush == null)
		{
			pendingSortFlush = serverList.schedule.Execute(FlushSort).StartingIn(200L);
		}
	}

	private void FlushSort()
	{
		pendingSortFlush?.Pause();
		pendingSortFlush = null;
		FilterServers();
		SortServers();
	}

	private static void EnsurePingThreadPoolFloor(int workerCount)
	{
		int num = workerCount * 3;
		ThreadPool.GetMinThreads(out var workerThreads, out var completionPortThreads);
		if (workerThreads < num || completionPortThreads < num)
		{
			ThreadPool.SetMinThreads(Math.Max(workerThreads, num), Math.Max(completionPortThreads, num));
		}
	}

	private void AddServer(EndPoint endPoint)
	{
		if (!endPointVisualElementMap.ContainsKey(endPoint))
		{
			VisualElement visualElement = serverAsset.Instantiate();
			visualElement.userData = new Dictionary<string, object>();
			VisualElement visualElement2 = visualElement.Query<VisualElement>("Server");
			visualElement2.userData = new Dictionary<string, object>();
			((Button)visualElement2.Query<Button>()).RegisterCallback<ClickEvent, EndPoint>(OnClickServer, endPoint);
			endPointVisualElementMap.Add(endPoint, visualElement);
			serverList.Add(visualElement);
			StyleServer(endPoint);
		}
	}

	private EndPoint GetServerEndPoint(VisualElement visualElement)
	{
		if (!endPointVisualElementMap.ContainsValue(visualElement))
		{
			return null;
		}
		return endPointVisualElementMap.FirstOrDefault((KeyValuePair<EndPoint, VisualElement> x) => x.Value == visualElement).Key;
	}

	private void SetServerPreviewData(EndPoint endPoint, ServerPreviewData previewData)
	{
		if (endPointVisualElementMap.ContainsKey(endPoint))
		{
			(((VisualElement)endPointVisualElementMap[endPoint].Query<VisualElement>("Server")).userData as Dictionary<string, object>)["previewData"] = previewData;
		}
	}

	private ServerPreviewData GetServerPreviewData(EndPoint endPoint)
	{
		if (!endPointVisualElementMap.ContainsKey(endPoint))
		{
			return null;
		}
		return (((VisualElement)endPointVisualElementMap[endPoint].Query<VisualElement>("Server")).userData as Dictionary<string, object>).GetValueOrDefault("previewData", null) as ServerPreviewData;
	}

	private void StyleServer(EndPoint endPoint)
	{
		if (endPointVisualElementMap.ContainsKey(endPoint))
		{
			VisualElement visualElement = endPointVisualElementMap[endPoint].Query<VisualElement>("Server");
			ServerPreviewData serverPreviewData = GetServerPreviewData(endPoint);
			Label label = visualElement.Query<Label>("NameLabel");
			Label label2 = visualElement.Query<Label>("PlayersLabel");
			Label label3 = visualElement.Query<Label>("PingLabel");
			if (serverPreviewData == null)
			{
				visualElement.EnableInClassList("passwordProtected", enable: false);
				visualElement.EnableInClassList("modded", enable: false);
				visualElement.EnableInClassList("unreachable", enable: true);
				label.text = endPoint.ToString();
				label2.text = "?";
				label3.text = "?";
			}
			else
			{
				visualElement.EnableInClassList("passwordProtected", serverPreviewData.isPasswordProtected);
				visualElement.EnableInClassList("modded", serverPreviewData.clientRequiredModIds.Length != 0);
				visualElement.EnableInClassList("unreachable", enable: false);
				label.text = serverPreviewData.name;
				label2.text = $"{serverPreviewData.players}/{serverPreviewData.maxPlayers}";
				label3.text = $"{serverPreviewData.ping}ms";
			}
		}
	}

	private void RemoveServer(EndPoint endPoint)
	{
		if (endPointVisualElementMap.ContainsKey(endPoint))
		{
			((Button)UQueryExtensions.Query<Button>(endPointVisualElementMap[endPoint].Query<VisualElement>("Server"))).UnregisterCallback<ClickEvent, EndPoint>(OnClickServer);
			serverList.Remove(endPointVisualElementMap[endPoint]);
			endPointVisualElementMap.Remove(endPoint);
		}
	}

	private void RemoveAllServers()
	{
		foreach (EndPoint item in endPointVisualElementMap.Keys.ToList())
		{
			RemoveServer(item);
		}
	}

	private void StyleSortButtons()
	{
		StyleSortArrow(nameSortArrow, ServerSortType.Name);
		StyleSortArrow(playersSortArrow, ServerSortType.Players);
		StyleSortArrow(pingSortArrow, ServerSortType.Ping);
	}

	private void StyleSortArrow(VisualElement arrow, ServerSortType columnSortType)
	{
		bool flag = sortType == columnSortType;
		arrow.EnableInClassList("active", flag);
		arrow.EnableInClassList("descending", flag && sortDirection == ServerSortDirection.Descending);
	}

	private void SortServers()
	{
		serverList.hierarchy.Sort((VisualElement a, VisualElement b) =>
		{
			EndPoint serverEndPoint = GetServerEndPoint(a);
			EndPoint serverEndPoint2 = GetServerEndPoint(b);
			ServerPreviewData serverPreviewData = GetServerPreviewData(serverEndPoint);
			ServerPreviewData serverPreviewData2 = GetServerPreviewData(serverEndPoint2);
			string text = ((serverPreviewData != null) ? serverPreviewData.name : serverEndPoint.ToString());
			string strB = ((serverPreviewData2 != null) ? serverPreviewData2.name : serverEndPoint2.ToString());
			int num = serverPreviewData?.players ?? 0;
			int value = serverPreviewData2?.players ?? 0;
			int num2 = serverPreviewData?.ping ?? int.MaxValue;
			int value2 = serverPreviewData2?.ping ?? int.MaxValue;
			int num3 = 0;
			switch (sortType)
			{
			case ServerSortType.Name:
				num3 = text.CompareTo(strB) * ((sortDirection == ServerSortDirection.Ascending) ? 1 : (-1));
				break;
			case ServerSortType.Players:
				num3 = num.CompareTo(value) * ((sortDirection == ServerSortDirection.Ascending) ? 1 : (-1));
				if (num3 == 0)
				{
					num3 = text.CompareTo(strB);
				}
				break;
			case ServerSortType.Ping:
				num3 = num2.CompareTo(value2) * ((sortDirection == ServerSortDirection.Ascending) ? 1 : (-1));
				if (num3 == 0)
				{
					num3 = text.CompareTo(strB);
				}
				break;
			}
			return num3;
		});
	}

	private void FilterServers()
	{
		foreach (EndPoint key in endPointVisualElementMap.Keys)
		{
			FilterServer(key);
		}
		UpdateResultCount();
	}

	private void FilterServer(EndPoint endPoint)
	{
		if (endPointVisualElementMap.ContainsKey(endPoint))
		{
			VisualElement visualElement = endPointVisualElementMap[endPoint];
			ServerPreviewData serverPreviewData = GetServerPreviewData(endPoint);
			bool flag;
			if (serverPreviewData == null)
			{
				string text = endPoint.ipAddress.ToLower();
				string value = (string.IsNullOrEmpty(searchTextField.value) ? null : searchTextField.value.ToLower());
				flag = (string.IsNullOrEmpty(value) || text.Contains(value)) && showUnreachableToggle.value;
			}
			else
			{
				string text2 = serverPreviewData.name.ToLower();
				string value2 = (string.IsNullOrEmpty(searchTextField.value) ? null : searchTextField.value.ToLower());
				flag = (string.IsNullOrEmpty(value2) || text2.Contains(value2)) && serverPreviewData.ping <= maxPingValue && (serverPreviewData.players > 0 || showEmptyToggle.value) && (serverPreviewData.players < serverPreviewData.maxPlayers || showFullToggle.value) && (!serverPreviewData.isPasswordProtected || showPasswordProtectedToggle.value) && (serverPreviewData.clientRequiredModIds.Length == 0 || showModdedToggle.value);
			}
			visualElement.style.display = ((!flag) ? DisplayStyle.None : DisplayStyle.Flex);
		}
	}

	private void UpdateResultCount()
	{
		int num = endPointVisualElementMap.Values.Count((VisualElement serverAssetInstance) => serverAssetInstance.style.display.value == DisplayStyle.Flex);
		int count = endPointVisualElementMap.Count;
		resultCountLabel.text = string.Format("SHOWING {0} / {1} {2}", num, count, (count == 1) ? "SERVER" : "SERVERS");
	}

	private ServerPreviewData PingServer(EndPoint endPoint, int connectTimeout, int responseTimeout)
	{
		TCPClient tcpClient = new TCPClient(endPoint, connectTimeout);
		double pingTimestamp = 0.0;
		ServerPreviewData previewData = null;
		ManualResetEventSlim responseEvent = new ManualResetEventSlim(initialState: false);
		tcpClient.OnConnected += () =>
		{
			string message = JsonSerializer.Serialize(new TCPServerPreviewRequest());
			tcpClient.SendMessage(message);
		};
		tcpClient.OnMessageSent += (string message) =>
		{
			try
			{
				if (JsonSerializer.Deserialize<TCPServerMessage>(message).type == TCPServerMessageType.PreviewRequest)
				{
					pingTimestamp = Utils.GetTimestamp();
				}
			}
			catch (Exception ex)
			{
				Logger.Error($"Error parsing message sent to {endPoint}: {ex.Message}");
			}
		};
		tcpClient.OnMessageReceived += (string message) =>
		{
			try
			{
				if (JsonSerializer.Deserialize<TCPServerMessage>(message).type == TCPServerMessageType.PreviewResponse)
				{
					TCPServerPreviewResponse tCPServerPreviewResponse = JsonSerializer.Deserialize<TCPServerPreviewResponse>(message);
					int ping = (int)(Utils.GetTimestamp() - pingTimestamp);
					previewData = new ServerPreviewData
					{
						name = tCPServerPreviewResponse.name,
						players = tCPServerPreviewResponse.players,
						maxPlayers = tCPServerPreviewResponse.maxPlayers,
						isPasswordProtected = tCPServerPreviewResponse.isPasswordProtected,
						clientRequiredModIds = tCPServerPreviewResponse.clientRequiredModIds,
						ping = ping
					};
					responseEvent.Set();
				}
			}
			catch (Exception ex)
			{
				Logger.Error($"Error parsing message from {endPoint}: {ex.Message}");
			}
		};
		tcpClient.Connect();
		if (tcpClient.IsConnected)
		{
			responseEvent.Wait(responseTimeout);
			tcpClient.Disconnect();
		}
		return previewData;
	}

	private void OnClickNameSort()
	{
		if (sortType == ServerSortType.Name)
		{
			sortDirection = ((sortDirection == ServerSortDirection.Ascending) ? ServerSortDirection.Descending : ServerSortDirection.Ascending);
		}
		else
		{
			sortType = ServerSortType.Name;
			sortDirection = ServerSortDirection.Ascending;
		}
		StyleSortButtons();
		SortServers();
	}

	private void OnClickPlayersSort()
	{
		if (sortType == ServerSortType.Players)
		{
			sortDirection = ((sortDirection == ServerSortDirection.Ascending) ? ServerSortDirection.Descending : ServerSortDirection.Ascending);
		}
		else
		{
			sortType = ServerSortType.Players;
			sortDirection = ServerSortDirection.Descending;
		}
		StyleSortButtons();
		SortServers();
	}

	private void OnClickPingSort()
	{
		if (sortType == ServerSortType.Ping)
		{
			sortDirection = ((sortDirection == ServerSortDirection.Ascending) ? ServerSortDirection.Descending : ServerSortDirection.Ascending);
		}
		else
		{
			sortType = ServerSortType.Ping;
			sortDirection = ServerSortDirection.Ascending;
		}
		StyleSortButtons();
		SortServers();
	}

	private void OnServerBrowserClickClose()
	{
		EventManager.TriggerEvent("Event_OnServerBrowserClickClose");
	}

	private void OnClickRefresh()
	{
		EventManager.TriggerEvent("Event_OnServerBrowserClickRefresh");
	}

	private void OnClickHostGame()
	{
		EventManager.TriggerEvent("Event_OnServerBrowserClickNewServer");
	}

	private void OnClickDirectConnect()
	{
		EventManager.TriggerEvent("Event_OnServerBrowserClickDirectConnect");
	}

	private void OnClickServer(ClickEvent e, EndPoint endPoint)
	{
		EventManager.TriggerEvent("Event_OnServerBrowserClickEndPoint", new Dictionary<string, object> { { "endPoint", endPoint } });
	}

	private void OnChangeSearchTextField(ChangeEvent<string> e)
	{
		FilterServers();
	}

	private void OnChangeMaxPing(int maxPing)
	{
		maxPingValue = maxPing;
		FilterServers();
	}

	private void OnChangeShowFullToggle(ChangeEvent<bool> e)
	{
		FilterServers();
	}

	private void OnChangeShowEmptyToggle(ChangeEvent<bool> e)
	{
		FilterServers();
	}

	private void OnChangeShowPasswordProtectedToggle(ChangeEvent<bool> e)
	{
		FilterServers();
	}

	private void OnChangeShowModdedToggle(ChangeEvent<bool> e)
	{
		FilterServers();
	}

	private void OnChangeShowUnreachableToggle(ChangeEvent<bool> e)
	{
		FilterServers();
	}
}
