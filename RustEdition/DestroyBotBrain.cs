using UnityEngine;

public class DestroyBotBrain : BotBrain
{
	private enum Phase
	{
		Retreat,
		Charge
	}

	private const float RetreatDistance = 26f;

	private const float MaxRetreatDuration = 4f;

	private const float JumpRange = 2f;

	private const float ContactReached = 1.4f;

	private const float LeavingDistance = 3f;

	private const float MaxChargeDuration = 8f;

	private Phase phase;

	private bool isInitialized;

	private float phaseStartTime;

	private Vector3 retreatDirection = Vector3.forward;

	private bool hasRetreatDirection;

	private bool hasJumpedThisCharge;

	private float closestApproachThisCharge = float.MaxValue;

	public override void Tick(in BotContext context, BotInputDriver driver)
	{
		PlayerBody playerBody = driver.Player.PlayerBody;
		PlayerBody targetPlayerBody = context.TargetPlayerBody;
		if (!playerBody)
		{
			return;
		}
		if (!targetPlayerBody)
		{
			driver.HoldFacing(playerBody.transform.position + playerBody.transform.forward);
			return;
		}
		if (!isInitialized)
		{
			EnterPhase(Phase.Retreat);
			isInitialized = true;
		}
		Vector3 position = playerBody.transform.position;
		Vector3 position2 = targetPlayerBody.transform.position;
		Vector3 vector = position2 - position;
		vector.y = 0f;
		float magnitude = vector.magnitude;
		switch (phase)
		{
		case Phase.Retreat:
			TickRetreat(driver, position, position2, magnitude);
			break;
		case Phase.Charge:
			TickCharge(driver, playerBody, position2, magnitude);
			break;
		}
	}

	private void TickRetreat(BotInputDriver driver, Vector3 botPosition, Vector3 targetPosition, float distance)
	{
		if (!hasRetreatDirection)
		{
			Vector3 vector = botPosition - targetPosition;
			vector.y = 0f;
			retreatDirection = ((vector.sqrMagnitude > 0.01f) ? vector.normalized : Vector3.forward);
			hasRetreatDirection = true;
		}
		driver.MoveToward(botPosition + retreatDirection * 8f, sprint: true);
		if (distance >= 26f || Time.fixedTime - phaseStartTime >= 4f)
		{
			EnterPhase(Phase.Charge);
		}
	}

	private void TickCharge(BotInputDriver driver, PlayerBody body, Vector3 targetPosition, float distance)
	{
		driver.MoveToward(targetPosition, sprint: true);
		closestApproachThisCharge = Mathf.Min(closestApproachThisCharge, distance);
		if (!hasJumpedThisCharge && distance <= 2f && body.IsGrounded)
		{
			driver.Jump();
			hasJumpedThisCharge = true;
		}
		bool flag = closestApproachThisCharge <= 1.4f;
		bool flag2 = distance > 3f;
		if ((flag & flag2) || Time.fixedTime - phaseStartTime >= 8f)
		{
			EnterPhase(Phase.Retreat);
		}
	}

	private void EnterPhase(Phase newPhase)
	{
		phase = newPhase;
		phaseStartTime = Time.fixedTime;
		if (newPhase == Phase.Retreat)
		{
			hasRetreatDirection = false;
		}
		if (newPhase == Phase.Charge)
		{
			hasJumpedThisCharge = false;
			closestApproachThisCharge = float.MaxValue;
		}
	}
}
