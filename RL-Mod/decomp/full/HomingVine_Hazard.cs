using System.Runtime.CompilerServices;
using UnityEngine;

public class HomingVine_Hazard : Hazard, IPointHazard, IHazard, IRootObj
{
	[SerializeField]
	private SpriteRenderer m_vineSprite;

	private float m_length;

	private float m_currentAngle;

	public override float BaseDamage => 30f;

	private void OnEnable()
	{
		ResetHazard();
	}

	private void Update()
	{
		m_currentAngle = Mathf.LerpAngle(m_currentAngle, CDGHelper.AngleBetweenPts(m_pivot.transform.position, PlayerManager.GetPlayerController().Midpoint), Time.fixedDeltaTime);
		m_pivot.transform.rotation = Quaternion.Euler(0f, 0f, m_currentAngle - 90f);
		m_length += 0.01f;
		m_vineSprite.size = new Vector2(m_vineSprite.size.x, m_length);
	}

	public override void ResetHazard()
	{
		m_length = 0f;
		m_vineSprite.size = new Vector2(m_vineSprite.size.x, 0f);
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
