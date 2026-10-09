using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : NetworkBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private Vector3 initialLookAngle = new Vector3(30f, 0f, 0f);

	[Space(20f)]
	[SerializeField]
	private Vector2 initialStickRaycastOriginAngle = new Vector2(40f, 80f);

	[SerializeField]
	private Vector2 minimumStickRaycastOriginAngle = new Vector2(-25f, -92.5f);

	[SerializeField]
	private Vector2 maximumStickRaycastOriginAngle = new Vector2(80f, 92.5f);

	[Space(20f)]
	[SerializeField]
	private Vector2 minimumLookAngle = new Vector2(-25f, -135f);

	[SerializeField]
	private Vector2 maximumLookAngle = new Vector2(75f, 135f);

	[Space(20f)]
	[SerializeField]
	private int minimumBladeAngle = -4;

	[SerializeField]
	private int maximumBladeAngle = 4;

	public NetworkedInput<Vector2> MoveInput = new NetworkedInput<Vector2>();

	public NetworkedInput<Vector2> StickRaycastOriginAngleInput = new NetworkedInput<Vector2>(default, (Vector2 lastSentValue, Vector2 clientValue) => Vector2.Distance(lastSentValue, clientValue) > 0.1f);

	public NetworkedInput<Vector2> LookAngleInput = new NetworkedInput<Vector2>(default, (Vector2 lastSentValue, Vector2 clientValue) => Vector2.Distance(lastSentValue, clientValue) > 0.1f);

	public NetworkedInput<sbyte> BladeAngleInput = new NetworkedInput<sbyte>(0);

	public NetworkedInput<bool> SlideInput = new NetworkedInput<bool>(initialValue: false);

	public NetworkedInput<bool> SprintInput = new NetworkedInput<bool>(initialValue: false);

	public NetworkedInput<bool> TrackInput = new NetworkedInput<bool>(initialValue: false);

	public NetworkedInput<bool> LookInput = new NetworkedInput<bool>(initialValue: false);

	public NetworkedInput<byte> JumpInput = new NetworkedInput<byte>(0, null, (byte lastReceivedValue, double lastReceivedTime, byte serverValue) => Time.timeAsDouble - lastReceivedTime > 0.5);

	public NetworkedInput<bool> StopInput = new NetworkedInput<bool>(initialValue: false);

	public NetworkedInput<byte> TwistLeftInput = new NetworkedInput<byte>(0, null, (byte lastReceivedValue, double lastReceivedTime, byte serverValue) => Time.timeAsDouble - lastReceivedTime > 0.5);

	public NetworkedInput<byte> TwistRightInput = new NetworkedInput<byte>(0, null, (byte lastReceivedValue, double lastReceivedTime, byte serverValue) => Time.timeAsDouble - lastReceivedTime > 0.5);

	public NetworkedInput<byte> DashLeftInput = new NetworkedInput<byte>(0, null, (byte lastReceivedValue, double lastReceivedTime, byte serverValue) => Time.timeAsDouble - lastReceivedTime > 0.25);

	public NetworkedInput<byte> DashRightInput = new NetworkedInput<byte>(0, null, (byte lastReceivedValue, double lastReceivedTime, byte serverValue) => Time.timeAsDouble - lastReceivedTime > 0.25);

	public NetworkedInput<bool> ExtendLeftInput = new NetworkedInput<bool>(initialValue: false);

	public NetworkedInput<bool> ExtendRightInput = new NetworkedInput<bool>(initialValue: false);

	public NetworkedInput<bool> LateralLeftInput = new NetworkedInput<bool>(initialValue: false);

	public NetworkedInput<bool> LateralRightInput = new NetworkedInput<bool>(initialValue: false);

	public NetworkedInput<bool> TalkInput = new NetworkedInput<bool>(initialValue: false);

	[HideInInspector]
	public Player Player;

	[HideInInspector]
	public int TickRate = 200;

	private float bladeAngleBuffer;

	private bool shouldUpdateInputs;

	private bool shouldTickInputs;

	private float tickAccumulator;

	[HideInInspector]
	public Vector2 MinimumStickRaycastOriginAngle => minimumStickRaycastOriginAngle;

	[HideInInspector]
	public Vector2 MaximumStickRaycastOriginAngle => maximumStickRaycastOriginAngle;

	[HideInInspector]
	public Vector2 MinimumLookAngle => minimumLookAngle;

	[HideInInspector]
	public Vector2 MaximumLookAngle => maximumLookAngle;

	[HideInInspector]
	public int MinimumBladeAngle => minimumBladeAngle;

	[HideInInspector]
	public int MaximumBladeAngle => maximumBladeAngle;

	[HideInInspector]
	public float InitialLookAngle
	{
		get
		{
			return initialLookAngle.x;
		}
		set
		{
			initialLookAngle = new Vector3(value, 0f, 0f);
		}
	}

	private void Awake()
	{
		Player = GetComponent<Player>();
	}

	public override void OnNetworkSpawn()
	{
		if (Player.IsReplay.Value)
		{
			shouldTickInputs = true;
		}
		else if (IsOwner)
		{
			InputManager.BladeAngleUpAction.performed += OnBladeAngleUpActionPerformed;
			InputManager.BladeAngleDownAction.performed += OnBladeAngleDownActionPerformed;
			InputManager.JumpAction.performed += OnJumpActionPerformed;
			InputManager.TwistLeftAction.performed += OnTwistLeftActionPerformed;
			InputManager.TwistRightAction.performed += OnTwistRightActionPerformed;
			InputManager.DashLeftAction.performed += OnDashLeftActionPerformed;
			InputManager.DashRightAction.performed += OnDashRightActionPerformed;
			shouldUpdateInputs = true;
			shouldTickInputs = true;
			base.OnNetworkSpawn();
		}
	}

	public override void OnNetworkDespawn()
	{
		if (IsOwner)
		{
			shouldUpdateInputs = false;
			shouldTickInputs = false;
			InputManager.BladeAngleUpAction.performed -= OnBladeAngleUpActionPerformed;
			InputManager.BladeAngleDownAction.performed -= OnBladeAngleDownActionPerformed;
			InputManager.JumpAction.performed -= OnJumpActionPerformed;
			InputManager.TwistLeftAction.performed -= OnTwistLeftActionPerformed;
			InputManager.TwistRightAction.performed -= OnTwistRightActionPerformed;
			InputManager.DashLeftAction.performed -= OnDashLeftActionPerformed;
			InputManager.DashRightAction.performed -= OnDashRightActionPerformed;
			base.OnNetworkDespawn();
		}
	}

	private void Update()
	{
		if (shouldUpdateInputs)
		{
			UpdateInputs();
		}
		if (!shouldTickInputs)
		{
			return;
		}
		tickAccumulator += Time.deltaTime * (float)TickRate;
		if (tickAccumulator >= 1f)
		{
			while (tickAccumulator >= 1f)
			{
				tickAccumulator--;
			}
			ClientTick();
		}
	}

	private void UpdateInputs()
	{
		if (!GlobalStateManager.UIState.IsMouseRequired)
		{
			MoveInput.ClientValue = new Vector2((InputManager.TurnRightAction.IsInProgress() ? 1 : 0) + (InputManager.TurnLeftAction.IsInProgress() ? (-1) : 0), (InputManager.MoveForwardAction.IsInProgress() ? 1 : 0) + (InputManager.MoveBackwardAction.IsInProgress() ? (-1) : 0));
			if (!LookInput.ClientValue)
			{
				Vector2 vector = InputManager.StickAction.ReadValue<Vector2>();
				Vector2 vector2 = new Vector2(float.IsNaN(vector.y) ? 0f : ((0f - vector.y) * (SettingsManager.GlobalStickSensitivity / 2f) * SettingsManager.VerticalStickSensitivity), float.IsNaN(vector.x) ? 0f : (vector.x * (SettingsManager.GlobalStickSensitivity / 2f) * SettingsManager.HorizontalStickSensitivity));
				StickRaycastOriginAngleInput.ClientValue = Utils.Vector2Clamp(StickRaycastOriginAngleInput.ClientValue + vector2, minimumStickRaycastOriginAngle, maximumStickRaycastOriginAngle);
			}
			SlideInput.ClientValue = InputManager.SlideAction.IsInProgress();
			SprintInput.ClientValue = InputManager.SprintAction.IsInProgress();
			TrackInput.ClientValue = InputManager.TrackAction.IsInProgress();
			LookInput.ClientValue = InputManager.LookAction.IsInProgress();
			ExtendLeftInput.ClientValue = InputManager.ExtendLeftAction.IsInProgress();
			ExtendRightInput.ClientValue = InputManager.ExtendRightAction.IsInProgress();
			TalkInput.ClientValue = InputManager.TalkAction.IsInProgress();
			StopInput.ClientValue = InputManager.StopAction.IsInProgress();
		}
	}

	public void UpdateLookAngle(float deltaTime)
	{
		if (TrackInput.ClientValue && !LookInput.ClientValue)
		{
			Puck puck = MonoBehaviourSingleton<PuckManager>.Instance.GetPlayerPuck(OwnerClientId);
			if (!puck)
			{
				puck = MonoBehaviourSingleton<PuckManager>.Instance.GetPuck();
			}
			PlayerCamera playerCamera = Player.PlayerCamera;
			PlayerBody playerBody = Player.PlayerBody;
			if ((bool)puck && (bool)playerCamera && (bool)playerBody)
			{
				Quaternion quaternion = Quaternion.LookRotation(puck.transform.position - playerCamera.transform.position);
				Vector3 vector = Utils.WrapEulerAngles((Quaternion.Inverse(playerBody.transform.rotation) * quaternion).eulerAngles);
				vector = Utils.Vector2Clamp(vector, minimumLookAngle, maximumLookAngle);
				LookAngleInput.ClientValue = Vector3.LerpUnclamped(LookAngleInput.ClientValue, vector, deltaTime * 10f);
			}
		}
		if (LookInput.ClientValue)
		{
			Vector2 vector2 = InputManager.StickAction.ReadValue<Vector2>();
			Vector2 vector3 = new Vector2(float.IsNaN(vector2.y) ? 0f : ((0f - vector2.y) * (SettingsManager.LookSensitivity / 2f)), float.IsNaN(vector2.x) ? 0f : (vector2.x * (SettingsManager.LookSensitivity / 2f)));
			LookAngleInput.ClientValue = Utils.Vector2Clamp(LookAngleInput.ClientValue + vector3, minimumLookAngle, maximumLookAngle);
		}
		else if (!TrackInput.ClientValue)
		{
			LookAngleInput.ClientValue = Vector3.Lerp(LookAngleInput.ClientValue, initialLookAngle, deltaTime * 10f);
		}
	}

	public void ResetInputs(PlayerHandedness handedness)
	{
		MoveInput.ClientValue = Vector2.zero;
		LookAngleInput.ClientValue = initialLookAngle;
		StickRaycastOriginAngleInput.ClientValue = new Vector2(initialStickRaycastOriginAngle.x, (handedness == PlayerHandedness.Left) ? (0f - initialStickRaycastOriginAngle.y) : initialStickRaycastOriginAngle.y);
		BladeAngleInput.ClientValue = 0;
		SlideInput.ClientValue = false;
		SprintInput.ClientValue = false;
		TrackInput.ClientValue = false;
		LookInput.ClientValue = false;
		ExtendLeftInput.ClientValue = false;
		ExtendRightInput.ClientValue = false;
		LateralLeftInput.ClientValue = false;
		LateralRightInput.ClientValue = false;
		StopInput.ClientValue = false;
		bladeAngleBuffer = BladeAngleInput.ClientValue;
	}

	private void OnBladeAngleUpActionPerformed(InputAction.CallbackContext context)
	{
		if (!GlobalStateManager.UIState.IsMouseRequired && (bool)Player.Stick)
		{
			bladeAngleBuffer += context.ReadValue<float>();
			bladeAngleBuffer = Mathf.Clamp(bladeAngleBuffer, minimumBladeAngle, maximumBladeAngle);
			BladeAngleInput.ClientValue = (sbyte)bladeAngleBuffer;
		}
	}

	private void OnBladeAngleDownActionPerformed(InputAction.CallbackContext context)
	{
		if (!GlobalStateManager.UIState.IsMouseRequired && (bool)Player.Stick)
		{
			bladeAngleBuffer -= context.ReadValue<float>();
			bladeAngleBuffer = Mathf.Clamp(bladeAngleBuffer, minimumBladeAngle, maximumBladeAngle);
			BladeAngleInput.ClientValue = (sbyte)bladeAngleBuffer;
		}
	}

	private void OnJumpActionPerformed(InputAction.CallbackContext context)
	{
		if (!GlobalStateManager.UIState.IsMouseRequired)
		{
			JumpInput.ClientValue++;
		}
	}

	private void OnTwistLeftActionPerformed(InputAction.CallbackContext context)
	{
		if (!GlobalStateManager.UIState.IsMouseRequired)
		{
			TwistLeftInput.ClientValue++;
		}
	}

	private void OnTwistRightActionPerformed(InputAction.CallbackContext context)
	{
		if (!GlobalStateManager.UIState.IsMouseRequired)
		{
			TwistRightInput.ClientValue++;
		}
	}

	private void OnDashLeftActionPerformed(InputAction.CallbackContext context)
	{
		if (!GlobalStateManager.UIState.IsMouseRequired)
		{
			DashLeftInput.ClientValue++;
		}
	}

	private void OnDashRightActionPerformed(InputAction.CallbackContext context)
	{
		if (!GlobalStateManager.UIState.IsMouseRequired)
		{
			DashRightInput.ClientValue++;
		}
	}

	private void ClientTick()
	{
		if (MoveInput.HasChanged)
		{
			Client_MoveInputRpc(NetworkingUtils.CompressFloatToShort(MoveInput.ClientValue.x, -1f, 1f), NetworkingUtils.CompressFloatToShort(MoveInput.ClientValue.y, -1f, 1f));
			MoveInput.ClientTick();
		}
		if (StickRaycastOriginAngleInput.HasChanged)
		{
			Client_RaycastOriginAngleInputRpc(NetworkingUtils.CompressFloatToShort(StickRaycastOriginAngleInput.ClientValue.x, minimumStickRaycastOriginAngle.x, maximumStickRaycastOriginAngle.x), NetworkingUtils.CompressFloatToShort(StickRaycastOriginAngleInput.ClientValue.y, minimumStickRaycastOriginAngle.y, maximumStickRaycastOriginAngle.y));
			StickRaycastOriginAngleInput.ClientTick();
		}
		if (LookAngleInput.HasChanged)
		{
			Client_LookAngleInputRpc(NetworkingUtils.CompressFloatToShort(LookAngleInput.ClientValue.x, minimumLookAngle.x, maximumLookAngle.x), NetworkingUtils.CompressFloatToShort(LookAngleInput.ClientValue.y, minimumLookAngle.y, maximumLookAngle.y));
			LookAngleInput.ClientTick();
		}
		if (BladeAngleInput.HasChanged)
		{
			Client_BladeAngleInputRpc(BladeAngleInput.ClientValue);
			BladeAngleInput.ClientTick();
		}
		if (SlideInput.HasChanged)
		{
			Client_SlideInputRpc(SlideInput.ClientValue);
			SlideInput.ClientTick();
		}
		if (SprintInput.HasChanged)
		{
			Client_SprintInputRpc(SprintInput.ClientValue);
			SprintInput.ClientTick();
		}
		if (TrackInput.HasChanged)
		{
			Client_TrackInputRpc(TrackInput.ClientValue);
			TrackInput.ClientTick();
		}
		if (LookInput.HasChanged)
		{
			Client_LookInputRpc(LookInput.ClientValue);
			LookInput.ClientTick();
		}
		if (JumpInput.HasChanged)
		{
			Client_JumpInputRpc();
			JumpInput.ClientTick();
		}
		if (StopInput.HasChanged)
		{
			Client_StopInputRpc(StopInput.ClientValue);
			StopInput.ClientTick();
		}
		if (TwistLeftInput.HasChanged)
		{
			Client_TwistLeftInputRpc();
			TwistLeftInput.ClientTick();
		}
		if (TwistRightInput.HasChanged)
		{
			Client_TwistRightInputRpc();
			TwistRightInput.ClientTick();
		}
		if (DashLeftInput.HasChanged)
		{
			Client_DashLeftInputRpc();
			DashLeftInput.ClientTick();
		}
		if (DashRightInput.HasChanged)
		{
			Client_DashRightInputRpc();
			DashRightInput.ClientTick();
		}
		if (ExtendLeftInput.HasChanged)
		{
			Client_ExtendLeftInputRpc(ExtendLeftInput.ClientValue);
			ExtendLeftInput.ClientTick();
		}
		if (ExtendRightInput.HasChanged)
		{
			Client_ExtendRightInputRpc(ExtendRightInput.ClientValue);
			ExtendRightInput.ClientTick();
		}
		if (LateralLeftInput.HasChanged)
		{
			Client_LateralLeftInputRpc(LateralLeftInput.ClientValue);
			LateralLeftInput.ClientTick();
		}
		if (LateralRightInput.HasChanged)
		{
			Client_LateralRightInputRpc(LateralRightInput.ClientValue);
			LateralRightInput.ClientTick();
		}
		if (TalkInput.HasChanged)
		{
			Client_TalkInputRpc(TalkInput.ClientValue);
			TalkInput.ClientTick();
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_MoveInputRpc(short x, short y, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(2880114289u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, x);
			BytePacker.WriteValueBitPacked(bufferWriter, y);
			__endSendRpc(ref bufferWriter, 2880114289u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (rpcParams.Receive.SenderClientId == OwnerClientId || rpcParams.Receive.SenderClientId == 0L)
			{
				Server_MoveInputRpc(x, y, RpcTarget.Everyone);
			}
		}
	}

	[Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server, DeferLocal = true)]
	public void Server_MoveInputRpc(short x, short y, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				InvokePermission = RpcInvokePermission.Server,
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(2371540155u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, x);
			BytePacker.WriteValueBitPacked(bufferWriter, y);
			__endSendRpc(ref bufferWriter, 2371540155u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			Vector2 value = new Vector2(NetworkingUtils.DecompressShortToFloat(x, -1f, 1f), NetworkingUtils.DecompressShortToFloat(y, -1f, 1f));
			value = Utils.Vector2Clamp(value, -Vector2.one, Vector2.one);
			MoveInput.ServerValue = value;
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true, Delivery = RpcDelivery.Unreliable)]
	public void Client_RaycastOriginAngleInputRpc(short x, short y, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true,
				Delivery = RpcDelivery.Unreliable
			};
			FastBufferWriter bufferWriter = __beginSendRpc(4145643342u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Unreliable);
			BytePacker.WriteValueBitPacked(bufferWriter, x);
			BytePacker.WriteValueBitPacked(bufferWriter, y);
			__endSendRpc(ref bufferWriter, 4145643342u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Unreliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (rpcParams.Receive.SenderClientId == OwnerClientId || rpcParams.Receive.SenderClientId == 0L)
			{
				Server_RaycastOriginAngleInputRpc(x, y, RpcTarget.Everyone);
			}
		}
	}

	[Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server, DeferLocal = true, Delivery = RpcDelivery.Unreliable)]
	public void Server_RaycastOriginAngleInputRpc(short x, short y, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				InvokePermission = RpcInvokePermission.Server,
				DeferLocal = true,
				Delivery = RpcDelivery.Unreliable
			};
			FastBufferWriter bufferWriter = __beginSendRpc(3031376689u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Unreliable);
			BytePacker.WriteValueBitPacked(bufferWriter, x);
			BytePacker.WriteValueBitPacked(bufferWriter, y);
			__endSendRpc(ref bufferWriter, 3031376689u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Unreliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			Vector2 value = new Vector2(NetworkingUtils.DecompressShortToFloat(x, minimumStickRaycastOriginAngle.x, maximumStickRaycastOriginAngle.x), NetworkingUtils.DecompressShortToFloat(y, minimumStickRaycastOriginAngle.y, maximumStickRaycastOriginAngle.y));
			value = Utils.Vector2Clamp(value, minimumStickRaycastOriginAngle, maximumStickRaycastOriginAngle);
			StickRaycastOriginAngleInput.ServerValue = value;
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true, Delivery = RpcDelivery.Unreliable)]
	public void Client_LookAngleInputRpc(short x, short y, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true,
				Delivery = RpcDelivery.Unreliable
			};
			FastBufferWriter bufferWriter = __beginSendRpc(2301322626u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Unreliable);
			BytePacker.WriteValueBitPacked(bufferWriter, x);
			BytePacker.WriteValueBitPacked(bufferWriter, y);
			__endSendRpc(ref bufferWriter, 2301322626u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Unreliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (rpcParams.Receive.SenderClientId == OwnerClientId || rpcParams.Receive.SenderClientId == 0L)
			{
				Server_LookAngleInputRpc(x, y, RpcTarget.Everyone);
			}
		}
	}

	[Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server, DeferLocal = true, Delivery = RpcDelivery.Unreliable)]
	public void Server_LookAngleInputRpc(short x, short y, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				InvokePermission = RpcInvokePermission.Server,
				DeferLocal = true,
				Delivery = RpcDelivery.Unreliable
			};
			FastBufferWriter bufferWriter = __beginSendRpc(1047632353u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Unreliable);
			BytePacker.WriteValueBitPacked(bufferWriter, x);
			BytePacker.WriteValueBitPacked(bufferWriter, y);
			__endSendRpc(ref bufferWriter, 1047632353u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Unreliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			Vector2 value = new Vector2(NetworkingUtils.DecompressShortToFloat(x, minimumLookAngle.x, maximumLookAngle.x), NetworkingUtils.DecompressShortToFloat(y, minimumLookAngle.y, maximumLookAngle.y));
			value = Utils.Vector2Clamp(value, minimumLookAngle, maximumLookAngle);
			LookAngleInput.ServerValue = value;
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_BladeAngleInputRpc(sbyte value, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(4018011136u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 4018011136u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (rpcParams.Receive.SenderClientId == OwnerClientId || rpcParams.Receive.SenderClientId == 0L)
			{
				Server_BladeAngleInputRpc(value, RpcTarget.Everyone);
			}
		}
	}

	[Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server, DeferLocal = true)]
	public void Server_BladeAngleInputRpc(sbyte value, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				InvokePermission = RpcInvokePermission.Server,
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(817646686u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 817646686u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			BladeAngleInput.ServerValue = (sbyte)Mathf.Clamp(value, minimumBladeAngle, maximumBladeAngle);
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_SlideInputRpc(bool value, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(3775351339u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 3775351339u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (rpcParams.Receive.SenderClientId == OwnerClientId || rpcParams.Receive.SenderClientId == 0L)
			{
				Server_SlideInputRpc(value, RpcTarget.Everyone);
			}
		}
	}

	[Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server, DeferLocal = true)]
	public void Server_SlideInputRpc(bool value, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				InvokePermission = RpcInvokePermission.Server,
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(4107840079u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 4107840079u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			SlideInput.ServerValue = value;
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_SprintInputRpc(bool value, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(3297803930u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 3297803930u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (rpcParams.Receive.SenderClientId == OwnerClientId || rpcParams.Receive.SenderClientId == 0L)
			{
				Server_SprintInputRpc(value, RpcTarget.Everyone);
			}
		}
	}

	[Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server, DeferLocal = true)]
	public void Server_SprintInputRpc(bool value, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				InvokePermission = RpcInvokePermission.Server,
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(778340344u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 778340344u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			SprintInput.ServerValue = value;
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_TrackInputRpc(bool value, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(3231418942u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 3231418942u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (rpcParams.Receive.SenderClientId == OwnerClientId || rpcParams.Receive.SenderClientId == 0L)
			{
				Server_TrackInputRpc(value, RpcTarget.Everyone);
			}
		}
	}

	[Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server, DeferLocal = true)]
	public void Server_TrackInputRpc(bool value, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				InvokePermission = RpcInvokePermission.Server,
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(2722698928u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 2722698928u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			TrackInput.ServerValue = value;
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_LookInputRpc(bool value, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(894150284u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 894150284u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (rpcParams.Receive.SenderClientId == OwnerClientId || rpcParams.Receive.SenderClientId == 0L)
			{
				Server_LookInputRpc(value, RpcTarget.Everyone);
			}
		}
	}

	[Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server, DeferLocal = true)]
	public void Server_LookInputRpc(bool value, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				InvokePermission = RpcInvokePermission.Server,
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(3779091983u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 3779091983u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			LookInput.ServerValue = value;
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_JumpInputRpc(RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(566720222u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			__endSendRpc(ref bufferWriter, 566720222u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if ((rpcParams.Receive.SenderClientId == OwnerClientId || rpcParams.Receive.SenderClientId == 0L) && JumpInput.ShouldChange)
			{
				EventManager.TriggerEvent("Event_Server_OnPlayerJumpInput", new Dictionary<string, object> { { "player", Player } });
				JumpInput.ServerTick();
			}
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_StopInputRpc(bool value, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(212770831u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 212770831u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (rpcParams.Receive.SenderClientId == OwnerClientId || rpcParams.Receive.SenderClientId == 0L)
			{
				StopInput.ServerValue = value;
			}
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_DashLeftInputRpc(RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(1929006103u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			__endSendRpc(ref bufferWriter, 1929006103u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if ((rpcParams.Receive.SenderClientId == OwnerClientId || rpcParams.Receive.SenderClientId == 0L) && DashLeftInput.ShouldChange)
			{
				EventManager.TriggerEvent("Event_Server_OnPlayerDashLeftInput", new Dictionary<string, object> { { "player", Player } });
				DashLeftInput.ServerTick();
			}
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_DashRightInputRpc(RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(3135613427u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			__endSendRpc(ref bufferWriter, 3135613427u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if ((rpcParams.Receive.SenderClientId == OwnerClientId || rpcParams.Receive.SenderClientId == 0L) && DashRightInput.ShouldChange)
			{
				EventManager.TriggerEvent("Event_Server_OnPlayerDashRightInput", new Dictionary<string, object> { { "player", Player } });
				DashRightInput.ServerTick();
			}
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_TwistLeftInputRpc(RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(4104804754u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			__endSendRpc(ref bufferWriter, 4104804754u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if ((rpcParams.Receive.SenderClientId == OwnerClientId || rpcParams.Receive.SenderClientId == 0L) && TwistLeftInput.ShouldChange)
			{
				EventManager.TriggerEvent("Event_Server_OnPlayerTwistLeftInput", new Dictionary<string, object> { { "player", Player } });
				TwistLeftInput.ServerTick();
			}
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_TwistRightInputRpc(RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(2735818857u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			__endSendRpc(ref bufferWriter, 2735818857u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if ((rpcParams.Receive.SenderClientId == OwnerClientId || rpcParams.Receive.SenderClientId == 0L) && TwistRightInput.ShouldChange)
			{
				EventManager.TriggerEvent("Event_Server_OnPlayerTwistRightInput", new Dictionary<string, object> { { "player", Player } });
				TwistRightInput.ServerTick();
			}
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_ExtendLeftInputRpc(bool value, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(537498773u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 537498773u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (rpcParams.Receive.SenderClientId == OwnerClientId || rpcParams.Receive.SenderClientId == 0L)
			{
				Server_ExtendLeftInputRpc(value, RpcTarget.Everyone);
			}
		}
	}

	[Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server, DeferLocal = true)]
	public void Server_ExtendLeftInputRpc(bool value, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				InvokePermission = RpcInvokePermission.Server,
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(3288109408u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 3288109408u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			ExtendLeftInput.ServerValue = value;
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_ExtendRightInputRpc(bool value, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(4044541524u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 4044541524u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (rpcParams.Receive.SenderClientId == OwnerClientId || rpcParams.Receive.SenderClientId == 0L)
			{
				Server_ExtendRightInputRpc(value, RpcTarget.Everyone);
			}
		}
	}

	[Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server, DeferLocal = true)]
	public void Server_ExtendRightInputRpc(bool value, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				InvokePermission = RpcInvokePermission.Server,
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(152722375u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 152722375u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			ExtendRightInput.ServerValue = value;
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_LateralLeftInputRpc(bool value, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(867760499u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 867760499u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (rpcParams.Receive.SenderClientId == OwnerClientId || rpcParams.Receive.SenderClientId == 0L)
			{
				Server_LateralLeftInputRpc(value, RpcTarget.Everyone);
			}
		}
	}

	[Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server, DeferLocal = true)]
	public void Server_LateralLeftInputRpc(bool value, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				InvokePermission = RpcInvokePermission.Server,
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(1623507309u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 1623507309u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			LateralLeftInput.ServerValue = value;
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_LateralRightInputRpc(bool value, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(2139362476u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 2139362476u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (rpcParams.Receive.SenderClientId == OwnerClientId || rpcParams.Receive.SenderClientId == 0L)
			{
				Server_LateralRightInputRpc(value, RpcTarget.Everyone);
			}
		}
	}

	[Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server, DeferLocal = true)]
	public void Server_LateralRightInputRpc(bool value, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				InvokePermission = RpcInvokePermission.Server,
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(3221925618u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 3221925618u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			LateralRightInput.ServerValue = value;
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_TalkInputRpc(bool value, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(1563095812u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 1563095812u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (rpcParams.Receive.SenderClientId == OwnerClientId || rpcParams.Receive.SenderClientId == 0L)
			{
				Server_TalkInputRpc(value, RpcTarget.Everyone);
			}
		}
	}

	[Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server, DeferLocal = true)]
	public void Server_TalkInputRpc(bool value, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				InvokePermission = RpcInvokePermission.Server,
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(3713736028u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 3713736028u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			TalkInput.ServerValue = value;
			EventManager.TriggerEvent("Event_Everyone_OnPlayerTalkInput", new Dictionary<string, object>
			{
				{ "player", Player },
				{ "value", value }
			});
		}
	}

	public void Server_ForceSynchronizeClientId(ulong clientId)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Server_MoveInputRpc(NetworkingUtils.CompressFloatToShort(MoveInput.ServerValue.x, -1f, 1f), NetworkingUtils.CompressFloatToShort(MoveInput.ServerValue.y, -1f, 1f), RpcTarget.Single(clientId, RpcTargetUse.Persistent));
			Server_RaycastOriginAngleInputRpc(NetworkingUtils.CompressFloatToShort(StickRaycastOriginAngleInput.ServerValue.x, minimumStickRaycastOriginAngle.x, maximumStickRaycastOriginAngle.x), NetworkingUtils.CompressFloatToShort(StickRaycastOriginAngleInput.ServerValue.y, minimumStickRaycastOriginAngle.y, maximumStickRaycastOriginAngle.y), RpcTarget.Single(clientId, RpcTargetUse.Persistent));
			Server_LookAngleInputRpc(NetworkingUtils.CompressFloatToShort(LookAngleInput.ServerValue.x, minimumLookAngle.x, maximumLookAngle.x), NetworkingUtils.CompressFloatToShort(LookAngleInput.ServerValue.y, minimumLookAngle.y, maximumLookAngle.y), RpcTarget.Single(clientId, RpcTargetUse.Persistent));
			Server_BladeAngleInputRpc(BladeAngleInput.ServerValue, RpcTarget.Single(clientId, RpcTargetUse.Persistent));
			Server_SlideInputRpc(SlideInput.ServerValue, RpcTarget.Single(clientId, RpcTargetUse.Persistent));
			Server_SprintInputRpc(SprintInput.ServerValue, RpcTarget.Single(clientId, RpcTargetUse.Persistent));
			Server_TrackInputRpc(TrackInput.ServerValue, RpcTarget.Single(clientId, RpcTargetUse.Persistent));
			Server_LookInputRpc(LookInput.ServerValue, RpcTarget.Single(clientId, RpcTargetUse.Persistent));
			Server_ExtendLeftInputRpc(ExtendLeftInput.ServerValue, RpcTarget.Single(clientId, RpcTargetUse.Persistent));
			Server_ExtendRightInputRpc(ExtendRightInput.ServerValue, RpcTarget.Single(clientId, RpcTargetUse.Persistent));
			Server_LateralLeftInputRpc(LateralLeftInput.ServerValue, RpcTarget.Single(clientId, RpcTargetUse.Persistent));
			Server_LateralRightInputRpc(LateralRightInput.ServerValue, RpcTarget.Single(clientId, RpcTargetUse.Persistent));
			Server_TalkInputRpc(TalkInput.ServerValue, RpcTarget.Single(clientId, RpcTargetUse.Persistent));
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(2880114289u, __rpc_handler_2880114289, "Client_MoveInputRpc", RpcInvokePermission.Everyone);
		__registerRpc(2371540155u, __rpc_handler_2371540155, "Server_MoveInputRpc", RpcInvokePermission.Server);
		__registerRpc(4145643342u, __rpc_handler_4145643342, "Client_RaycastOriginAngleInputRpc", RpcInvokePermission.Everyone);
		__registerRpc(3031376689u, __rpc_handler_3031376689, "Server_RaycastOriginAngleInputRpc", RpcInvokePermission.Server);
		__registerRpc(2301322626u, __rpc_handler_2301322626, "Client_LookAngleInputRpc", RpcInvokePermission.Everyone);
		__registerRpc(1047632353u, __rpc_handler_1047632353, "Server_LookAngleInputRpc", RpcInvokePermission.Server);
		__registerRpc(4018011136u, __rpc_handler_4018011136, "Client_BladeAngleInputRpc", RpcInvokePermission.Everyone);
		__registerRpc(817646686u, __rpc_handler_817646686, "Server_BladeAngleInputRpc", RpcInvokePermission.Server);
		__registerRpc(3775351339u, __rpc_handler_3775351339, "Client_SlideInputRpc", RpcInvokePermission.Everyone);
		__registerRpc(4107840079u, __rpc_handler_4107840079, "Server_SlideInputRpc", RpcInvokePermission.Server);
		__registerRpc(3297803930u, __rpc_handler_3297803930, "Client_SprintInputRpc", RpcInvokePermission.Everyone);
		__registerRpc(778340344u, __rpc_handler_778340344, "Server_SprintInputRpc", RpcInvokePermission.Server);
		__registerRpc(3231418942u, __rpc_handler_3231418942, "Client_TrackInputRpc", RpcInvokePermission.Everyone);
		__registerRpc(2722698928u, __rpc_handler_2722698928, "Server_TrackInputRpc", RpcInvokePermission.Server);
		__registerRpc(894150284u, __rpc_handler_894150284, "Client_LookInputRpc", RpcInvokePermission.Everyone);
		__registerRpc(3779091983u, __rpc_handler_3779091983, "Server_LookInputRpc", RpcInvokePermission.Server);
		__registerRpc(566720222u, __rpc_handler_566720222, "Client_JumpInputRpc", RpcInvokePermission.Everyone);
		__registerRpc(212770831u, __rpc_handler_212770831, "Client_StopInputRpc", RpcInvokePermission.Everyone);
		__registerRpc(1929006103u, __rpc_handler_1929006103, "Client_DashLeftInputRpc", RpcInvokePermission.Everyone);
		__registerRpc(3135613427u, __rpc_handler_3135613427, "Client_DashRightInputRpc", RpcInvokePermission.Everyone);
		__registerRpc(4104804754u, __rpc_handler_4104804754, "Client_TwistLeftInputRpc", RpcInvokePermission.Everyone);
		__registerRpc(2735818857u, __rpc_handler_2735818857, "Client_TwistRightInputRpc", RpcInvokePermission.Everyone);
		__registerRpc(537498773u, __rpc_handler_537498773, "Client_ExtendLeftInputRpc", RpcInvokePermission.Everyone);
		__registerRpc(3288109408u, __rpc_handler_3288109408, "Server_ExtendLeftInputRpc", RpcInvokePermission.Server);
		__registerRpc(4044541524u, __rpc_handler_4044541524, "Client_ExtendRightInputRpc", RpcInvokePermission.Everyone);
		__registerRpc(152722375u, __rpc_handler_152722375, "Server_ExtendRightInputRpc", RpcInvokePermission.Server);
		__registerRpc(867760499u, __rpc_handler_867760499, "Client_LateralLeftInputRpc", RpcInvokePermission.Everyone);
		__registerRpc(1623507309u, __rpc_handler_1623507309, "Server_LateralLeftInputRpc", RpcInvokePermission.Server);
		__registerRpc(2139362476u, __rpc_handler_2139362476, "Client_LateralRightInputRpc", RpcInvokePermission.Everyone);
		__registerRpc(3221925618u, __rpc_handler_3221925618, "Server_LateralRightInputRpc", RpcInvokePermission.Server);
		__registerRpc(1563095812u, __rpc_handler_1563095812, "Client_TalkInputRpc", RpcInvokePermission.Everyone);
		__registerRpc(3713736028u, __rpc_handler_3713736028, "Server_TalkInputRpc", RpcInvokePermission.Server);
		base.__initializeRpcs();
	}

	private static void __rpc_handler_2880114289(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out short value);
			ByteUnpacker.ReadValueBitPacked(reader, out short value2);
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Client_MoveInputRpc(value, value2, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2371540155(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out short value);
			ByteUnpacker.ReadValueBitPacked(reader, out short value2);
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Server_MoveInputRpc(value, value2, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_4145643342(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out short value);
			ByteUnpacker.ReadValueBitPacked(reader, out short value2);
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Client_RaycastOriginAngleInputRpc(value, value2, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3031376689(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out short value);
			ByteUnpacker.ReadValueBitPacked(reader, out short value2);
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Server_RaycastOriginAngleInputRpc(value, value2, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2301322626(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out short value);
			ByteUnpacker.ReadValueBitPacked(reader, out short value2);
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Client_LookAngleInputRpc(value, value2, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1047632353(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out short value);
			ByteUnpacker.ReadValueBitPacked(reader, out short value2);
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Server_LookAngleInputRpc(value, value2, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_4018011136(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out sbyte value, default(FastBufferWriter.ForPrimitives));
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Client_BladeAngleInputRpc(value, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_817646686(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out sbyte value, default(FastBufferWriter.ForPrimitives));
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Server_BladeAngleInputRpc(value, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3775351339(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Client_SlideInputRpc(value, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_4107840079(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Server_SlideInputRpc(value, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3297803930(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Client_SprintInputRpc(value, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_778340344(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Server_SprintInputRpc(value, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3231418942(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Client_TrackInputRpc(value, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2722698928(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Server_TrackInputRpc(value, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_894150284(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Client_LookInputRpc(value, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3779091983(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Server_LookInputRpc(value, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_566720222(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Client_JumpInputRpc(ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_212770831(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Client_StopInputRpc(value, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1929006103(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Client_DashLeftInputRpc(ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3135613427(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Client_DashRightInputRpc(ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_4104804754(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Client_TwistLeftInputRpc(ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2735818857(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Client_TwistRightInputRpc(ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_537498773(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Client_ExtendLeftInputRpc(value, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3288109408(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Server_ExtendLeftInputRpc(value, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_4044541524(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Client_ExtendRightInputRpc(value, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_152722375(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Server_ExtendRightInputRpc(value, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_867760499(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Client_LateralLeftInputRpc(value, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1623507309(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Server_LateralLeftInputRpc(value, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2139362476(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Client_LateralRightInputRpc(value, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3221925618(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Server_LateralRightInputRpc(value, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1563095812(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Client_TalkInputRpc(value, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3713736028(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			RpcParams ext = rpcParams.Ext;
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerInput)target).Server_TalkInputRpc(value, ext);
			((PlayerInput)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected override string __getTypeName()
	{
		return "PlayerInput";
	}
}
