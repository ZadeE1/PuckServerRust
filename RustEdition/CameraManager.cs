using System.Collections.Generic;

public static class CameraManager
{
	private static readonly Logger Logger = new Logger("CameraManager");

	private static List<BaseCamera> cameras = new List<BaseCamera>();

	private static BaseCamera activeCamera = null;

	private static CameraType activeCameraType = CameraType.None;

	private static ulong? activeCameraOwnerClientId = null;

	public static void Initialize()
	{
		CameraManagerController.Initialize();
	}

	public static void Dispose()
	{
		CameraManagerController.Dispose();
		DisableAllCameras();
		cameras.Clear();
	}

	public static void RegisterCamera(BaseCamera camera)
	{
		if (!cameras.Contains(camera))
		{
			cameras.Add(camera);
			EventManager.TriggerEvent("Event_OnCameraRegistered", new Dictionary<string, object> { { "camera", camera } });
			if (IsActiveCamera(camera))
			{
				EnableCamera(camera);
			}
		}
	}

	public static void UnregisterCamera(BaseCamera camera)
	{
		if (cameras.Contains(camera))
		{
			cameras.Remove(camera);
		}
		if (activeCamera == camera)
		{
			activeCameraType = CameraType.None;
			activeCameraOwnerClientId = null;
			activeCamera = null;
		}
		EventManager.TriggerEvent("Event_OnCameraUnregistered", new Dictionary<string, object> { { "camera", camera } });
	}

	public static BaseCamera GetCameraByType(CameraType cameraType)
	{
		return cameras.Find((BaseCamera camera) => camera.Type == cameraType);
	}

	public static BaseCamera GetCameraByOwnerClientId(ulong ownerClientId)
	{
		return cameras.Find((BaseCamera camera) => camera.OwnerClientId == ownerClientId);
	}

	public static BaseCamera GetActiveCamera()
	{
		return cameras.Find((BaseCamera camera) => IsActiveCamera(camera));
	}

	public static void SetActiveCamera(CameraType type, ulong? ownerClientId = null)
	{
		Logger.Info($"Setting active camera to type {type}");
		activeCameraType = type;
		activeCameraOwnerClientId = ownerClientId;
		BaseCamera baseCamera = GetActiveCamera();
		if (baseCamera != null)
		{
			EnableCamera(baseCamera);
		}
	}

	public static bool IsActiveCamera(BaseCamera camera)
	{
		if (activeCameraType == camera.Type)
		{
			if (activeCameraOwnerClientId.HasValue)
			{
				return activeCameraOwnerClientId == camera.OwnerClientId;
			}
			return true;
		}
		return false;
	}

	public static void EnableCamera(BaseCamera camera)
	{
		if (!camera.IsEnabled)
		{
			DisableAllCameras();
			Logger.Info($"Enabling camera of type {camera.Type}");
			camera.Enable();
		}
	}

	public static void DisableCamera(BaseCamera camera)
	{
		if (camera.IsEnabled)
		{
			Logger.Info($"Disabling camera of type {camera.Type}");
			camera.Disable();
		}
	}

	public static void DisableAllCameras()
	{
		foreach (BaseCamera camera in cameras)
		{
			DisableCamera(camera);
		}
	}
}
