using UnityEngine;

public class DashFlipEFfectCheck : MonoBehaviour
{
	private BaseEffect m_effect;

	private void Awake()
	{
		m_effect = GetComponent<BaseEffect>();
	}

	private void FixedUpdate()
	{
		if (PlayerManager.IsInstantiated)
		{
			float x = PlayerManager.GetPlayerController().Pivot.transform.localScale.x;
			if ((bool)m_effect && ((x > 0f && m_effect.transform.localScale.x < 0f) || (x <= 0f && m_effect.transform.localScale.x > 0f)))
			{
				m_effect.transform.SetLocalScaleX(m_effect.transform.localScale.x * -1f);
			}
		}
	}
}
