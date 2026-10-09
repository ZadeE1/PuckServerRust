using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

public abstract class BasePlugin<T> where T : BasePluginState, new()
{
	private static readonly Logger Logger = new Logger("BasePlugin");

	protected T state;

	private string assemblyPath;

	private Assembly assembly;

	private object instance;

	private MethodInfo onEnableMethod;

	private MethodInfo onDisableMethod;

	public T State
	{
		get
		{
			return state;
		}
		set
		{
			if (!state.Equals(value))
			{
				T oldState = state;
				state = value;
				OnStateChanged(oldState, state);
			}
		}
	}

	public string Path => state.Path;

	public bool IsReady => state.IsReady;

	public bool IsEnabled => state.IsEnabled;

	public bool HasAssembly => assemblyPath != null;

	public BasePlugin(T state)
	{
		this.state = state;
	}

	public virtual void Initialize()
	{
		assemblyPath = GetAssemblyPath();
	}

	public virtual void Dispose()
	{
		if (IsEnabled)
		{
			Disable();
		}
	}

	public virtual void SetState(Dictionary<string, object> updates)
	{
		T val = new T
		{
			Path = (updates.ContainsKey("path") ? ((string)updates["path"]) : state.Path),
			IsReady = (updates.ContainsKey("isReady") ? ((bool)updates["isReady"]) : state.IsReady),
			IsEnabled = (updates.ContainsKey("isEnabled") ? ((bool)updates["isEnabled"]) : state.IsEnabled)
		};
		State = val;
	}

	private string GetAssemblyPath()
	{
		if (Path == null || !Directory.Exists(Path))
		{
			return null;
		}
		return Directory.GetFiles(Path, "*.dll", SearchOption.TopDirectoryOnly).FirstOrDefault();
	}

	private void LoadAssembly(string path)
	{
		if (instance == null)
		{
			assembly = Assembly.LoadFile(path);
			Type type = assembly.GetTypes().FirstOrDefault((Type type2) => type2.IsClass && !type2.IsAbstract && typeof(IPuckPlugin).IsAssignableFrom(type2));
			if (type == null)
			{
				throw new Exception("IPuckPlugin missing from assembly");
			}
			instance = Activator.CreateInstance(type);
			onEnableMethod = type.GetMethod("OnEnable");
			onDisableMethod = type.GetMethod("OnDisable");
		}
	}

	private void UnloadAssembly()
	{
		instance = null;
		assembly = null;
		onEnableMethod = null;
		onDisableMethod = null;
	}

	public bool Enable()
	{
		if (IsEnabled)
		{
			return false;
		}
		if (!IsReady)
		{
			return false;
		}
		try
		{
			if (HasAssembly)
			{
				LoadAssembly(assemblyPath);
				if (!(bool)onEnableMethod.Invoke(instance, null))
				{
					throw new Exception("OnEnable returned false");
				}
			}
			SetState(new Dictionary<string, object> { { "isEnabled", true } });
		}
		catch (Exception exception)
		{
			OnEnableFailed(exception);
			return false;
		}
		return true;
	}

	public bool Disable()
	{
		if (!IsEnabled)
		{
			return false;
		}
		try
		{
			if (instance != null)
			{
				if (!(bool)onDisableMethod.Invoke(instance, null))
				{
					throw new Exception("OnDisable returned false");
				}
				UnloadAssembly();
			}
			SetState(new Dictionary<string, object> { { "isEnabled", false } });
		}
		catch (Exception exception)
		{
			OnDisableFailed(exception);
			return false;
		}
		return true;
	}

	public virtual void OnEnableFailed(Exception exception)
	{
	}

	public virtual void OnDisableFailed(Exception exception)
	{
	}

	protected virtual void OnStateChanged(T oldState, T newState)
	{
		if (oldState.Path != newState.Path)
		{
			assemblyPath = GetAssemblyPath();
		}
	}
}
