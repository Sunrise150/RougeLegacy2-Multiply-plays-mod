using System.Collections;
using UnityEngine;

public class HorizontalDarkness_Trait : BaseTrait
{
	public override TraitType TraitType => TraitType.HorizontalDarkness;

	protected IEnumerator Start()
	{
		while (!PlayerManager.IsInstantiated)
		{
			yield return null;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		float y = playerController.Midpoint.y - playerController.transform.position.y;
		m_traitMask.transform.SetParent(playerController.transform);
		m_traitMask.transform.localPosition = new Vector3(0f, y, 0f);
		Vector3 localScale = m_traitMask.transform.localScale;
		localScale.y = 6f;
		m_traitMask.transform.localScale = localScale;
	}

	public override void AssignGreenMask()
	{
		base.AssignGreenMask();
		m_postProcessOverrideController.Profile.OverrideTintGreenChannel = true;
		m_postProcessOverrideController.Profile.TintGreenChannel = m_postProcessOverrideController.Profile.TintRedChannel;
		m_postProcessOverrideController.Profile.TintRedChannel = 0f;
		m_postProcessOverrideController.Profile.OverrideTintRedChannel = false;
	}

	public override void DisableOnDeath()
	{
		base.DisableOnDeath();
		m_traitMask.gameObject.SetActive(value: false);
	}
}
