using System;
using System.Collections.Generic;
using System.Text.Json;
using UnityEngine.InputSystem;

public static class InputManager
{
	private static readonly Logger Logger = new Logger("InputManager");

	public static InputAction MoveForwardAction;

	public static InputAction MoveBackwardAction;

	public static InputAction TurnLeftAction;

	public static InputAction TurnRightAction;

	public static InputAction StickAction;

	public static InputAction BladeAngleUpAction;

	public static InputAction BladeAngleDownAction;

	public static InputAction SlideAction;

	public static InputAction SprintAction;

	public static InputAction TrackAction;

	public static InputAction LookAction;

	public static InputAction JumpAction;

	public static InputAction StopAction;

	public static InputAction TwistLeftAction;

	public static InputAction TwistRightAction;

	public static InputAction DashLeftAction;

	public static InputAction DashRightAction;

	public static InputAction ExtendLeftAction;

	public static InputAction ExtendRightAction;

	public static InputAction LateralLeftAction;

	public static InputAction LateralRightAction;

	public static InputAction TalkAction;

	public static InputAction AllChatAction;

	public static InputAction TeamChatAction;

	public static InputAction PauseAction;

	public static InputAction PositionSelectAction;

	public static InputAction ScoreboardAction;

	public static InputAction QuickChat1Action;

	public static InputAction QuickChat2Action;

	public static InputAction QuickChat3Action;

	public static InputAction QuickChat4Action;

	public static InputAction QuickChat5Action;

	public static InputAction Debug1Action;

	public static InputAction Debug2Action;

	public static InputAction Debug3Action;

	public static InputAction Debug4Action;

	public static InputAction PointAction;

	public static InputAction ClickAction;

	public static Dictionary<string, KeyBind> KeyBinds = new Dictionary<string, KeyBind>();

	public static List<InputAction> InputActions = new List<InputAction>();

	public static List<InputAction> RebindableInputActions = new List<InputAction>();

	private const string ChatOpenInteractionReleaseToPressMigrationKey = "keyBindMigration_chatOpenInteractionReleaseToPress_v1";

	public static void Initialize()
	{
		InputSystem.RegisterInteraction<DoublePressInteraction>();
		InputSystem.RegisterInteraction<ToggleInteraction>();
		DisableControllers();
		InputSystem.onDeviceChange += OnInputDeviceChange;
		InputActionAsset actions = InputSystem.actions;
		MoveForwardAction = actions.FindAction("Move Forward");
		MoveBackwardAction = actions.FindAction("Move Backward");
		TurnLeftAction = actions.FindAction("Turn Left");
		TurnRightAction = actions.FindAction("Turn Right");
		StickAction = actions.FindAction("Stick");
		BladeAngleUpAction = actions.FindAction("Blade Angle Up");
		BladeAngleDownAction = actions.FindAction("Blade Angle Down");
		SlideAction = actions.FindAction("Slide");
		SprintAction = actions.FindAction("Sprint");
		TrackAction = actions.FindAction("Track");
		LookAction = actions.FindAction("Look");
		JumpAction = actions.FindAction("Jump");
		StopAction = actions.FindAction("Stop");
		TwistLeftAction = actions.FindAction("Twist Left");
		TwistRightAction = actions.FindAction("Twist Right");
		DashLeftAction = actions.FindAction("Dash Left");
		DashRightAction = actions.FindAction("Dash Right");
		ExtendLeftAction = actions.FindAction("Extend Left");
		ExtendRightAction = actions.FindAction("Extend Right");
		LateralLeftAction = actions.FindAction("Lateral Left");
		LateralRightAction = actions.FindAction("Lateral Right");
		TalkAction = actions.FindAction("Talk");
		AllChatAction = actions.FindAction("All Chat");
		TeamChatAction = actions.FindAction("Team Chat");
		PauseAction = actions.FindAction("Pause");
		PositionSelectAction = actions.FindAction("Position Select");
		ScoreboardAction = actions.FindAction("Scoreboard");
		QuickChat1Action = actions.FindAction("Quick Chat 1");
		QuickChat2Action = actions.FindAction("Quick Chat 2");
		QuickChat3Action = actions.FindAction("Quick Chat 3");
		QuickChat4Action = actions.FindAction("Quick Chat 4");
		QuickChat5Action = actions.FindAction("Quick Chat 5");
		Debug1Action = actions.FindAction("Debug 1");
		Debug2Action = actions.FindAction("Debug 2");
		Debug3Action = actions.FindAction("Debug 3");
		Debug4Action = actions.FindAction("Debug 4");
		PointAction = actions.FindAction("Point");
		ClickAction = actions.FindAction("Click");
		InputActions = new List<InputAction>
		{
			MoveForwardAction, MoveBackwardAction, TurnLeftAction, TurnRightAction, StickAction, BladeAngleUpAction, BladeAngleDownAction, SlideAction, SprintAction, TrackAction,
			LookAction, JumpAction, StopAction, TwistLeftAction, TwistRightAction, DashLeftAction, DashRightAction, ExtendLeftAction, ExtendRightAction, LateralLeftAction,
			LateralRightAction, TalkAction, AllChatAction, TeamChatAction, PauseAction, PositionSelectAction, ScoreboardAction, QuickChat1Action, QuickChat2Action, QuickChat3Action,
			QuickChat4Action, QuickChat5Action, Debug1Action, Debug2Action, Debug3Action, Debug4Action, PointAction, ClickAction
		};
		RebindableInputActions = new List<InputAction>
		{
			MoveForwardAction, MoveBackwardAction, TurnLeftAction, TurnRightAction, BladeAngleUpAction, BladeAngleDownAction, SlideAction, SprintAction, TrackAction, LookAction,
			JumpAction, StopAction, TwistLeftAction, TwistRightAction, DashLeftAction, DashRightAction, ExtendLeftAction, ExtendRightAction, LateralLeftAction, LateralRightAction,
			TalkAction, AllChatAction, TeamChatAction, PositionSelectAction, ScoreboardAction, Debug1Action
		};
		InputManagerController.Initialize();
	}

	public static void Dispose()
	{
		InputSystem.onDeviceChange -= OnInputDeviceChange;
		InputManagerController.Dispose();
	}

	private static void OnInputDeviceChange(InputDevice device, InputDeviceChange change)
	{
		if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected || change == InputDeviceChange.Enabled)
		{
			TryDisableController(device);
		}
	}

	private static void DisableControllers()
	{
		foreach (InputDevice device in InputSystem.devices)
		{
			TryDisableController(device);
		}
	}

	private static bool IsController(InputDevice device)
	{
		if (!(device is Gamepad))
		{
			return device is Joystick;
		}
		return true;
	}

	private static void TryDisableController(InputDevice device)
	{
		if (IsController(device) && device.enabled)
		{
			InputSystem.DisableDevice(device);
		}
	}

	public static void LoadKeyBinds()
	{
		try
		{
			string text = SaveManager.GetString("keyBinds", null);
			if (string.IsNullOrEmpty(text))
			{
				throw new Exception("No saved key binds found");
			}
			Dictionary<string, KeyBind> dictionary = JsonSerializer.Deserialize<Dictionary<string, KeyBind>>(text);
			List<string> list = new List<string>();
			foreach (InputAction rebindableInputAction in RebindableInputActions)
			{
				if (!dictionary.ContainsKey(rebindableInputAction.name))
				{
					list.Add(rebindableInputAction.name);
				}
			}
			if (list.Count > 0)
			{
				throw new Exception("Missing keys in loaded key binds (" + string.Join(", ", list) + ")");
			}
			KeyBinds.Clear();
			foreach (KeyValuePair<string, KeyBind> item in dictionary)
			{
				string actionName = item.Key;
				KeyBind value = item.Value;
				InputAction inputAction = RebindableInputActions.Find((InputAction action) => action.name == actionName);
				value.InputAction = inputAction;
				if (value.InputAction == null)
				{
					Logger.Warning("Cannot load key bind for " + actionName + " because it is not rebindable");
					continue;
				}
				KeyBinds.Add(actionName, value);
				ApplyKeyBind(value);
			}
			Logger.Info($"Loaded {KeyBinds.Count} key binds: {text}");
			EventManager.TriggerEvent("Event_OnKeyBindsLoaded", new Dictionary<string, object> { { "keyBinds", KeyBinds } });
		}
		catch (Exception ex)
		{
			Logger.Warning("Failed to load key binds: " + ex.Message);
			SaveKeyBinds();
			LoadKeyBinds();
		}
	}

	public static void ApplyKeyBinds()
	{
		foreach (KeyBind value in KeyBinds.Values)
		{
			ApplyKeyBind(value);
		}
	}

	public static void ApplyKeyBind(KeyBind keyBind)
	{
		string name = keyBind.InputAction.name;
		RebindAction(name, keyBind.ModifierPath, keyBind.Path);
		SetActionInteractions(name, keyBind.Interactions);
	}

	public static void MigrateChatOpenInteractionToPress()
	{
		if (SaveManager.GetBool("keyBindMigration_chatOpenInteractionReleaseToPress_v1", defaultValue: false))
		{
			return;
		}
		bool flag = false;
		string[] array = new string[2] { "All Chat", "Team Chat" };
		foreach (string text in array)
		{
			if (KeyBinds.TryGetValue(text, out var value) && value.Interactions == "Press(behavior=1)")
			{
				SetActionInteractions(text, string.Empty);
				flag = true;
			}
		}
		if (flag)
		{
			SaveKeyBinds();
		}
		SaveManager.SetBool("keyBindMigration_chatOpenInteractionReleaseToPress_v1", value: true);
	}

	public static string GetKeyBindDisplayString(KeyBind keyBind)
	{
		if (keyBind.IsComposite)
		{
			string text = null;
			if (!string.IsNullOrEmpty(keyBind.ModifierPath))
			{
				text = text + keyBind.InputAction.GetBindingDisplayString(1, InputBinding.DisplayStringOptions.DontIncludeInteractions).ToUpper() + "+";
			}
			return text + keyBind.InputAction.GetBindingDisplayString(2, InputBinding.DisplayStringOptions.DontIncludeInteractions).ToUpper();
		}
		return keyBind.InputAction.GetBindingDisplayString(0, InputBinding.DisplayStringOptions.DontIncludeInteractions).ToUpper();
	}

	public static void SaveKeyBinds()
	{
		try
		{
			foreach (InputAction rebindableInputAction in RebindableInputActions)
			{
				if (!KeyBinds.ContainsKey(rebindableInputAction.name))
				{
					KeyBind value = new KeyBind(rebindableInputAction);
					KeyBinds.Add(rebindableInputAction.name, value);
				}
				else
				{
					KeyBinds[rebindableInputAction.name].Update(rebindableInputAction);
				}
			}
			string text = JsonSerializer.Serialize(KeyBinds, new JsonSerializerOptions
			{
				WriteIndented = true
			});
			SaveManager.SetString("keyBinds", text);
			Logger.Info($"Saved {KeyBinds.Count} key binds: {text}");
			EventManager.TriggerEvent("Event_OnKeyBindsSaved", new Dictionary<string, object> { { "keyBinds", KeyBinds } });
		}
		catch (Exception ex)
		{
			Logger.Error("Failed to save key binds: " + ex.Message);
		}
	}

	public static void ResetToDefault()
	{
		Logger.Info("Resetting key binds to default");
		foreach (InputAction rebindableInputAction in RebindableInputActions)
		{
			rebindableInputAction.RemoveAllBindingOverrides();
		}
		SaveKeyBinds();
	}

	public static void RebindButtonInteractively(string actionName)
	{
		InputAction inputAction = RebindableInputActions.Find((InputAction action) => action.name == actionName);
		if (inputAction == null)
		{
			Logger.Warning("Cannot rebind action " + actionName + " because it is not rebindable");
			return;
		}
		inputAction.Disable();
		InputActionRebindingExtensions.RebindingOperation rebindingOperation = GenerateRebindingOperation();
		InputActionRebindingExtensions.RebindingOperation rebindingOperation2 = GenerateRebindingOperation();
		bool isComposite = inputAction.bindings[0].isComposite;
		string interactions = inputAction.bindings[0].effectiveInteractions;
		Logger.Info("Rebinding " + actionName);
		if (isComposite)
		{
			Logger.Info("Rebinding " + actionName + " as composite");
			rebindingOperation.WithTargetBinding(1).OnComplete((InputActionRebindingExtensions.RebindingOperation modifierOperation) =>
			{
				string modifierPath = modifierOperation.action.bindings[1].effectivePath;
				Logger.Info("Rebound " + actionName + " modifierPath to " + modifierPath);
				rebindingOperation2.WithControlsExcluding(modifierPath).WithTargetBinding(2).WithTimeout(0.5f)
					.OnComplete((InputActionRebindingExtensions.RebindingOperation operation) =>
					{
						inputAction.Enable();
						string effectivePath = operation.action.bindings[2].effectivePath;
						RebindAction(actionName, modifierPath, effectivePath);
						SetActionInteractions(actionName, interactions);
						Logger.Info("Rebound " + actionName + " to " + modifierPath + " + " + effectivePath);
						EventManager.TriggerEvent("Event_OnKeyBindRebindComplete", new Dictionary<string, object> { { "actionName", actionName } });
						SaveKeyBinds();
					})
					.OnCancel((InputActionRebindingExtensions.RebindingOperation operation) =>
					{
						inputAction.Enable();
						RebindAction(actionName, null, modifierPath);
						SetActionInteractions(actionName, interactions);
						Logger.Info("Rebinding " + actionName + " path was cancelled, using modifier path as path " + modifierPath);
						EventManager.TriggerEvent("Event_OnKeyBindRebindComplete", new Dictionary<string, object> { { "actionName", actionName } });
						SaveKeyBinds();
					})
					.Start();
			}).OnCancel((InputActionRebindingExtensions.RebindingOperation operation) =>
			{
				inputAction.Enable();
				Logger.Info("Rebinding " + actionName + " was cancelled");
				EventManager.TriggerEvent("Event_OnKeyBindRebindCancel", new Dictionary<string, object> { { "actionName", actionName } });
			});
		}
		else
		{
			rebindingOperation.OnComplete((InputActionRebindingExtensions.RebindingOperation operation) =>
			{
				inputAction.Enable();
				string effectivePath = operation.action.bindings[0].effectivePath;
				RebindAction(actionName, null, effectivePath);
				SetActionInteractions(actionName, interactions);
				Logger.Info("Rebound " + actionName + " to " + effectivePath);
				EventManager.TriggerEvent("Event_OnKeyBindRebindComplete", new Dictionary<string, object> { { "actionName", actionName } });
				SaveKeyBinds();
			}).OnCancel((InputActionRebindingExtensions.RebindingOperation operation) =>
			{
				inputAction.Enable();
				Logger.Info("Rebinding " + actionName + " was cancelled");
				EventManager.TriggerEvent("Event_OnKeyBindRebindCancel", new Dictionary<string, object> { { "actionName", actionName } });
			});
		}
		rebindingOperation.Start();
		EventManager.TriggerEvent("Event_OnKeyBindRebindStart", new Dictionary<string, object>
		{
			{ "actionName", actionName },
			{ "isComposite", isComposite }
		});
		InputActionRebindingExtensions.RebindingOperation GenerateRebindingOperation()
		{
			InputActionRebindingExtensions.RebindingOperation rebindingOperation3 = inputAction.PerformInteractiveRebinding().WithCancelingThrough("<Keyboard>/escape").OnMatchWaitForAnother(0.1f);
			if (inputAction.expectedControlType == "Axis")
			{
				rebindingOperation3.WithControlsExcluding("<Mouse>/scroll/y").WithControlsExcluding("<Mouse>/scroll/x");
			}
			return rebindingOperation3;
		}
	}

	public static void RebindAction(string actionName, string modifierPath = null, string path = null)
	{
		InputAction inputAction = RebindableInputActions.Find((InputAction action) => action.name == actionName);
		if (inputAction == null)
		{
			Logger.Warning("Cannot rebind action " + actionName + " because it is not rebindable");
		}
		else if (inputAction.bindings[0].isComposite)
		{
			inputAction.ApplyBindingOverride(1, new InputBinding
			{
				overridePath = modifierPath
			});
			inputAction.ApplyBindingOverride(2, new InputBinding
			{
				overridePath = path
			});
		}
		else
		{
			inputAction.ApplyBindingOverride(0, new InputBinding
			{
				overridePath = path
			});
		}
	}

	public static void SetActionInteractions(string actionName, string interactions)
	{
		InputAction inputAction = RebindableInputActions.Find((InputAction action) => action.name == actionName);
		if (inputAction == null)
		{
			Logger.Warning("Cannot set interactions for action " + actionName + " because it is not rebindable");
			return;
		}
		InputBinding inputBinding = inputAction.bindings[0];
		inputAction.ApplyBindingOverride(0, new InputBinding
		{
			overridePath = inputBinding.overridePath,
			overrideProcessors = inputBinding.overrideProcessors,
			overrideInteractions = interactions
		});
	}
}
