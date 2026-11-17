using RLAudio;

public class SwordKnight_Miniboss_AIScript : SwordKnight_Basic_AIScript
{
	private const string MURMUR_DEATH_SCREEN_FLASH_AUDIO_PATH = "event:/SFX/Enemies/sfx_statueBoss_death_flash";

	protected override float m_slash_TellIntroAndHold_Delay => 1.75f;

	protected override float m_slash_Attack_Speed => 0f;

	protected override float m_slash_Attack_Duration => 0.25f;

	protected override float m_cricket_Exit_ForceIdle => 0.65f;

	protected override float m_cricket_AttackCD => 8.5f;

	protected override int m_cricket_Attack_ProjectileAmount => 250;

	protected override float m_cricket_Attack_ProjectileDelay => 2.275f;

	protected override void PlayDeathAnimAudio()
	{
		AudioManager.PlayOneShot(this, "event:/SFX/Enemies/sfx_statueBoss_death_flash", base.gameObject.transform.position);
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		base.EnemyController.StatusEffectController.AddStatusEffectImmunity(StatusEffectType.Enemy_Freeze);
	}
}
