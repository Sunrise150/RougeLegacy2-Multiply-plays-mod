public class ExpandingPlatform : SpecialPlatform
{
	public override void SetState(StateID state)
	{
		if (state != 0)
		{
			_ = 10;
		}
	}
}
