public class ChaseBotBrain : BotBrain
{
	public override void Tick(in BotContext context, BotInputDriver driver)
	{
		Puck targetPuck = context.TargetPuck;
		PlayerBody playerBody = driver.Player.PlayerBody;
		if ((bool)targetPuck && (bool)playerBody)
		{
			float magnitude = (targetPuck.transform.position - playerBody.transform.position).magnitude;
			driver.MoveToward(targetPuck.transform.position, magnitude > 10f);
		}
		else
		{
			driver.SkateForwardWeaving();
		}
		driver.WaveStick();
		driver.LookAround();
	}
}
