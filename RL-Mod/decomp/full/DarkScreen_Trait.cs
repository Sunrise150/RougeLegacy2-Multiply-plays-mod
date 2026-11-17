using System.Collections;
using UnityEngine;

public class DarkScreen_Trait : BaseTrait
{
	public override TraitType TraitType => TraitType.DarkScreen;

	protected IEnumerator Start()
	{
		while (!PlayerManager.IsInstantiated)
		{
			yield return null;
		}
		m_traitMask.transform.position = PlayerManager.GetPlayerController().Midpoint;
		m_traitMask.transform.SetParent(PlayerManager.GetPlayerController().transform, worldPositionStays: true);
		m_traitMask.transform.localScale = new Vector3(31f, 31f, 1f);
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

	protected void OnDestroy()
	{
		if ((bool)m_traitMask)
		{
			Object.Destroy(m_traitMask.gameObject);
		}
	}
}
