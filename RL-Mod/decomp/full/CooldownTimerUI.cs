using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CooldownTimerUI : MonoBehaviour
{
	[SerializeField]
	private Image m_image;

	private ICooldown m_cooldown;

	public ICooldown Cooldown
	{
		get
		{
			return m_cooldown;
		}
		set
		{
			m_cooldown = value;
			if (m_cooldown != null)
			{
				m_cooldown.OnBeginCooldownRelay.AddListener(OnBeginCooldown);
			}
		}
	}

	private void OnBeginCooldown(object sender, CooldownEventArgs eventArgs)
	{
		if (!Cooldown.IsNativeNull())
		{
			StopAllCoroutines();
			if (m_image.type == Image.Type.Filled)
			{
				StartCoroutine(CooldownRunning());
				return;
			}
			Debug.LogFormat("{0}: Image should be of Type (Filled) in order to work with Cooldown Timer", Time.frameCount);
		}
	}

	private IEnumerator CooldownRunning()
	{
		while (Cooldown.IsOnCooldown)
		{
			float fillAmount = 1f - Cooldown.CooldownTimer / Cooldown.ActualCooldownTime;
			m_image.fillAmount = fillAmount;
			yield return null;
		}
		m_image.fillAmount = 1f;
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		if (Cooldown != null)
		{
			Cooldown.OnBeginCooldownRelay.RemoveListener(OnBeginCooldown);
		}
	}
}
