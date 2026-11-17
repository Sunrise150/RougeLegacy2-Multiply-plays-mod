public class CastleToGardenTunnel : BossTunnel
{
	protected override PlayerSaveFlag GetBossFreeHealUsedFlag()
	{
		return PlayerSaveFlag.GardenBoss_FreeHeal_Used;
	}
}
