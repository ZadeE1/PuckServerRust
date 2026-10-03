using System;
using DG.Tweening;
using UnityEngine;

public class DeploymentManager : MonoBehaviour
{
	private static readonly Logger Logger = new Logger("DeploymentManager");

	private Tween emptyTween;

	private Tween authenticationTween;

	public DeploymentProvider Provider { get; private set; }

	public ushort DeploymentPort { get; private set; }

	public string DeploymentId { get; private set; }

	public bool StopWhenEmpty { get; private set; }

	public bool IsDeployment => Provider != DeploymentProvider.None;

	public bool BindsDeploymentPort => Provider == DeploymentProvider.PuckOrchestrator;

	private void Awake()
	{
		string environmentVariable = Environment.GetEnvironmentVariable("ARBITRIUM_PORT_PUCK_EXTERNAL");
		string environmentVariable2 = Environment.GetEnvironmentVariable("PUCK_DEPLOYMENT_PORT");
		string environmentVariable3 = Environment.GetEnvironmentVariable("PUCK_DEPLOYMENT_STOP_WHEN_EMPTY");
		StopWhenEmpty = string.Equals(environmentVariable3, "true", StringComparison.OrdinalIgnoreCase);
		DeploymentId = Environment.GetEnvironmentVariable("PUCK_DEPLOYMENT_ID") ?? Utils.GetCommandLineArgument("--deploymentId");
		if (!string.IsNullOrEmpty(environmentVariable))
		{
			Provider = DeploymentProvider.Edgegap;
			DeploymentPort = ushort.Parse(environmentVariable);
			Logger.Info($"Running in Edgegap deployment (Provider: {Provider}, DeploymentPort: {DeploymentPort}, StopWhenEmpty: {StopWhenEmpty})");
		}
		else if (!string.IsNullOrEmpty(environmentVariable2))
		{
			Provider = DeploymentProvider.PuckOrchestrator;
			DeploymentPort = ushort.Parse(environmentVariable2);
			Logger.Info($"Running in Puck Orchestrator deployment (Provider: {Provider}, DeploymentPort: {DeploymentPort}, StopWhenEmpty: {StopWhenEmpty})");
		}
	}

	public void StartEmptyTimeout()
	{
		if (StopWhenEmpty)
		{
			Logger.Info("Starting empty timeout");
			emptyTween?.Kill();
			emptyTween = DOVirtual.DelayedCall(60f, () =>
			{
				StopDeployment();
			});
		}
	}

	public void StopEmptyTimeout()
	{
		if (StopWhenEmpty)
		{
			Logger.Info("Stopping empty timeout");
			emptyTween?.Kill();
		}
	}

	public void StartAuthenticationTimeout()
	{
		if (IsDeployment)
		{
			Logger.Info("Starting authentication timeout");
			authenticationTween?.Kill();
			authenticationTween = DOVirtual.DelayedCall(60f, () =>
			{
				StopDeployment();
			});
		}
	}

	public void StopAuthenticationTimeout()
	{
		if (IsDeployment)
		{
			Logger.Info("Stopping authentication timeout");
			authenticationTween?.Kill();
		}
	}

	public void StopDeployment()
	{
		if (IsDeployment)
		{
			Logger.Info($"Stopping deployment (Provider: {Provider})");
			Application.Quit();
		}
	}
}
