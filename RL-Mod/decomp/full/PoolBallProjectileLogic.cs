using System;
using Sigtrap.Relays;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TerrainRicochetProjectile_Logic))]
public class PoolBallProjectileLogic : BaseProjectileLogic
{
	[SerializeField]
	private TMP_Text m_poolBallText;

	public Relay<int> RicochetRelay = new Relay<int>();

	private TerrainRicochetProjectile_Logic m_ricochetLogic;

	private int m_currentBounces;

	private float m_startingMagic;

	private float m_startingCritDamage;

	protected override void Awake()
	{
		base.Awake();
		m_ricochetLogic = base.gameObject.GetComponent<TerrainRicochetProjectile_Logic>();
	}

	private void OnRicochet(object sender, EventArgs args)
	{
		BaseEffect baseEffect = EffectManager.PlayEffect(base.SourceProjectile.gameObject, base.SourceProjectile.Animator, "PoolBallHit_Effect", base.SourceProjectile.transform.localPosition, -2f, EffectStopType.Gracefully);
		baseEffect.transform.SetParent(base.SourceProjectile.gameObject.transform, worldPositionStays: true);
		baseEffect.AddDetachListener(base.SourceProjectile);
		if (m_currentBounces == 0)
		{
			BaseEffect baseEffect2 = EffectManager.PlayEffect(base.SourceProjectile.gameObject, base.SourceProjectile.Animator, "PoolBallTrail_Effect", base.SourceProjectile.transform.localPosition, -2f, EffectStopType.Gracefully);
			baseEffect2.transform.SetParent(base.SourceProjectile.gameObject.transform, worldPositionStays: true);
			baseEffect2.AddDetachListener(base.SourceProjectile);
		}
		m_currentBounces++;
		if (m_currentBounces >= 2 && base.SourceProjectile.ActualCritChance < 100f)
		{
			base.SourceProjectile.ActualCritChance += 100f;
		}
		if (m_currentBounces >= 4)
		{
			base.SourceProjectile.FlagForDestruction();
			return;
		}
		base.SourceProjectile.Magic = m_startingMagic * (1f + Mathf.Min(7f * (float)m_currentBounces, 7f));
		base.SourceProjectile.ActualCritDamage = m_startingCritDamage * (1f + Mathf.Min(7f * (float)m_currentBounces, 7f));
		m_poolBallText.text = (m_currentBounces + 1).ToString();
		RicochetRelay.Dispatch(m_currentBounces);
	}

	public void OnEnable()
	{
		m_startingMagic = base.SourceProjectile.Magic;
		m_startingCritDamage = base.SourceProjectile.ActualCritDamage;
		m_currentBounces = 0;
		m_ricochetLogic.OnRicochet -= OnRicochet;
		m_ricochetLogic.OnRicochet += OnRicochet;
		m_poolBallText.text = (m_currentBounces + 1).ToString();
		base.SourceProjectile.transform.localScale = Vector3.one;
	}

	private void OnDestroy()
	{
		m_ricochetLogic.OnRicochet -= OnRicochet;
	}
}
