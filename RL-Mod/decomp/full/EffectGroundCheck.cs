using UnityEngine;

public class EffectGroundCheck : MonoBehaviour
{
	[SerializeField]
	private GameObject m_raycastCheckPos;

	private BaseEffect m_effect;

	private void Awake()
	{
		m_effect = GetComponent<BaseEffect>();
		m_effect.OnPlayRelay.AddListener(PerformGroundCheck);
	}

	private void PerformGroundCheck(BaseEffect effect)
	{
		if (PlayerManager.IsInstantiated)
		{
			Vector2 vector = new Vector2((m_raycastCheckPos.transform.position.x - base.transform.position.x) * 2f, 0f);
			RaycastHit2D raycastHit2D = ((!PlayerManager.GetPlayerController().IsFacingRight) ? Physics2D.Raycast((Vector2)m_raycastCheckPos.transform.position - vector, Vector2.down, 0.2f, PlayerManager.GetPlayerController().ControllerCorgi.SavedPlatformMask) : Physics2D.Raycast(m_raycastCheckPos.transform.position, Vector2.down, 0.2f, PlayerManager.GetPlayerController().ControllerCorgi.SavedPlatformMask));
			if (!raycastHit2D)
			{
				Invoke("StopEffect", 0f);
			}
		}
	}

	private void StopEffect()
	{
		m_effect.Stop(EffectStopType.Immediate);
	}

	private void OnDestroy()
	{
		if (m_effect != null)
		{
			m_effect.OnPlayRelay.RemoveListener(PerformGroundCheck);
		}
	}
}
