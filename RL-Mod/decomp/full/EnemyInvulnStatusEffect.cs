using System.Collections;
using UnityEngine;

public class EnemyInvulnStatusEffect : BaseStatusEffect
{
	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_Invuln;

	public override float StartingDurationOverride => 0.5f;

	public GameObject Source { get; private set; }

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		EnemyController enemyController = m_charController as EnemyController;
		Source = caster.gameObject;
		bool flag = true;
		if ((enemyController.EnemyType == EnemyType.PaintingEnemy || enemyController.EnemyType == EnemyType.MimicChest) && !enemyController.LogicController.LogicIsActivated)
		{
			flag = false;
		}
		if (flag)
		{
			foreach (Renderer item in m_charController.RendererArray)
			{
				if (item.sharedMaterial.HasProperty(ShaderID_RL._ShieldToggle))
				{
					item.GetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
					BaseStatusEffect.m_matBlockHelper_STATIC.SetInt(ShaderID_RL._ShieldToggle, 1);
					item.SetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
				}
			}
		}
		while (Time.time < base.EndTime)
		{
			yield return null;
		}
		StopEffect();
	}

	public override void StopEffect(bool interrupted = false)
	{
		base.StopEffect(interrupted);
		if (GameManager.IsApplicationClosing)
		{
			return;
		}
		Source = null;
		if (!m_charController || BaseStatusEffect.m_matBlockHelper_STATIC == null || m_charController.RendererArray == null)
		{
			return;
		}
		foreach (Renderer item in m_charController.RendererArray)
		{
			if ((bool)item && item.sharedMaterial.HasProperty(ShaderID_RL._ShieldToggle))
			{
				item.GetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
				BaseStatusEffect.m_matBlockHelper_STATIC.SetInt(ShaderID_RL._ShieldToggle, 0);
				item.SetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
			}
		}
	}
}
