namespace RLAudio;

public class MagicWandExplosionProjectileDamageAudioData : ProjectileDamageAudioData
{
	public override string CharacterDamageAudioPath
	{
		get
		{
			string result = base.CharacterDamageAudioPath;
			if (GetComponent<Projectile_RL>().ActualCritChance >= 100f)
			{
				result = "event:/SFX/Weapons/sfx_weapon_crit_wand_hit";
			}
			return result;
		}
	}
}
