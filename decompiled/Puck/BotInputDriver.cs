using Unity.Netcode;
using UnityEngine;

public class BotInputDriver : MonoBehaviour
{
	private const float StickMaximumReach = 2.5f;

	private const float TargetRefreshInterval = 0.5f;

	private const float SwingRange = 3.5f;

	private const float SwingDuration = 0.4f;

	private const float SwingCooldown = 1.2f;

	private const float SwingSweepAngle = 50f;

	private BotBrain brain;

	private float seed;

	private BotContext context;

	private float nextTargetRefreshTime;

	private float swingStartTime = float.MinValue;

	private float nextSwingTime;

	public Player Player { get; private set; }

	private void Awake()
	{
		Player = GetComponent<Player>();
		seed = (float)(Player.OwnerClientId % 100) * 0.63f;
		brain = BotManager.CreateBrain(BotManager.DefaultBehavior);
	}

	public void SetBrain(BotBrain newBrain)
	{
		brain = newBrain;
	}

	private void FixedUpdate()
	{
		if (!(NetworkManager.Singleton == null) && NetworkManager.Singleton.IsServer && (bool)Player && (bool)Player.PlayerInput && Player.IsCharacterSpawned)
		{
			if (Time.fixedTime >= nextTargetRefreshTime)
			{
				RefreshContext();
				nextTargetRefreshTime = Time.fixedTime + 0.5f;
			}
			brain?.Tick(in context, this);
		}
	}

	private void RefreshContext()
	{
		context.Seed = seed;
		context.TargetPuck = (MonoBehaviourSingleton<PuckManager>.Instance ? MonoBehaviourSingleton<PuckManager>.Instance.GetPuck() : null);
		if ((bool)Player.PlayerPosition)
		{
			context.HomePosition = Player.PlayerPosition.transform.position;
			context.HasHomePosition = true;
		}
		else
		{
			context.HasHomePosition = false;
		}
		context.IsClosestTeammateToPuck = false;
		context.TargetPlayerBody = null;
		PlayerBody playerBody = Player.PlayerBody;
		if (!playerBody || !MonoBehaviourSingleton<PlayerManager>.Instance)
		{
			return;
		}
		Vector3 position = playerBody.transform.position;
		bool flag = context.TargetPuck;
		Vector3 vector = (flag ? context.TargetPuck.transform.position : Vector3.zero);
		float num = (flag ? (vector - position).sqrMagnitude : 0f);
		bool flag2 = flag;
		float num2 = float.MaxValue;
		foreach (Player player in MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayers())
		{
			if (player == Player || !player.IsCharacterSpawned || !player.PlayerBody)
			{
				continue;
			}
			Vector3 position2 = player.PlayerBody.transform.position;
			if (player.OwnerClientId < 100000)
			{
				float sqrMagnitude = (position2 - position).sqrMagnitude;
				if (sqrMagnitude < num2)
				{
					num2 = sqrMagnitude;
					context.TargetPlayerBody = player.PlayerBody;
				}
			}
			if (flag2 && player.Team == Player.Team && player.Role != PlayerRole.Goalie && (vector - position2).sqrMagnitude < num)
			{
				flag2 = false;
			}
		}
		context.IsClosestTeammateToPuck = flag2;
	}

	public void Jump()
	{
		Player.PlayerBody.Jump();
	}

	public void MoveToward(Vector3 position, bool sprint)
	{
		PlayerInput playerInput = Player.PlayerInput;
		PlayerBody playerBody = Player.PlayerBody;
		Vector3 to = position - playerBody.transform.position;
		to.y = 0f;
		float num = Vector3.SignedAngle(playerBody.transform.forward, to, Vector3.up);
		playerInput.MoveInput.ServerValue = new Vector2(Mathf.Clamp(num / 45f, -1f, 1f), 1f);
		playerInput.SprintInput.ServerValue = sprint;
		playerInput.StopInput.ServerValue = false;
	}

	public void HoldFacing(Vector3 facePosition)
	{
		PlayerInput playerInput = Player.PlayerInput;
		PlayerBody playerBody = Player.PlayerBody;
		Vector3 to = facePosition - playerBody.transform.position;
		to.y = 0f;
		float num = Vector3.SignedAngle(playerBody.transform.forward, to, Vector3.up);
		playerInput.MoveInput.ServerValue = new Vector2(Mathf.Clamp(num / 45f, -1f, 1f), 0f);
		playerInput.SprintInput.ServerValue = false;
		playerInput.StopInput.ServerValue = playerBody.Rigidbody.linearVelocity.magnitude > 1f;
	}

	public void SkateForwardWeaving()
	{
		float num = Time.fixedTime + seed;
		Player.PlayerInput.MoveInput.ServerValue = new Vector2(Mathf.Sin(num * 0.7f), 1f);
	}

	public void WaveStick()
	{
		float num = Time.fixedTime + seed;
		Player.PlayerInput.StickRaycastOriginAngleInput.ServerValue = new Vector2(Mathf.Lerp(20f, 70f, (Mathf.Sin(num * 1.3f) + 1f) * 0.5f), Mathf.Sin(num * 0.9f) * 80f);
	}

	public void LookAround()
	{
		float num = Time.fixedTime + seed;
		Player.PlayerInput.LookAngleInput.ServerValue = new Vector2(30f, Mathf.Sin(num * 0.5f) * 90f);
	}

	public void AimStickAt(Puck puck, Vector3 toPuck, float puckDistance)
	{
		PlayerInput playerInput = Player.PlayerInput;
		PlayerBody playerBody = Player.PlayerBody;
		float num = Vector3.SignedAngle(playerBody.transform.forward, toPuck, Vector3.up);
		float x = 40f;
		StickPositioner stickPositioner = Player.StickPositioner;
		if ((bool)stickPositioner && (bool)puck)
		{
			Vector3 raycastOriginPosition = stickPositioner.RaycastOriginPosition;
			Vector3 vector = puck.transform.position - raycastOriginPosition;
			float magnitude = new Vector2(vector.x, vector.z).magnitude;
			float num2 = Mathf.Max(raycastOriginPosition.y - puck.transform.position.y, 0.1f);
			float a = Mathf.Acos(Mathf.Clamp01(magnitude / 2.5f)) * 57.29578f;
			float b = Mathf.Asin(Mathf.Clamp01(num2 / 2.5f)) * 57.29578f + 3f;
			x = Mathf.Max(a, b);
			num = Vector3.SignedAngle(playerBody.transform.forward, new Vector3(vector.x, 0f, vector.z), Vector3.up);
		}
		float y = num;
		if (puckDistance < 3.5f)
		{
			if (Time.fixedTime >= nextSwingTime)
			{
				swingStartTime = Time.fixedTime;
				nextSwingTime = Time.fixedTime + 0.4f + 1.2f + Mathf.Sin(seed * 13f) * 0.3f;
			}
			float num3 = (Time.fixedTime - swingStartTime) / 0.4f;
			if (num3 <= 1f)
			{
				y = num + Mathf.Lerp(-50f, 50f, num3);
			}
		}
		Vector2 serverValue = Utils.Vector2Clamp(new Vector2(x, y), playerInput.MinimumStickRaycastOriginAngle, playerInput.MaximumStickRaycastOriginAngle);
		playerInput.StickRaycastOriginAngleInput.ServerValue = serverValue;
		playerInput.LookAngleInput.ServerValue = new Vector2(25f, Mathf.Clamp(num, -90f, 90f));
	}
}
