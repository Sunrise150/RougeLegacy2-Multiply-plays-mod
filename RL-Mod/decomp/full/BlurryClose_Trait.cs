using System.Collections;
using UnityEngine;

public class BlurryClose_Trait : BaseTrait
{
	public override TraitType TraitType => TraitType.BlurryClose;

	protected IEnumerator Start()
	{
		while (!PlayerManager.IsInstantiated)
		{
			yield return null;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		m_traitMask.transform.SetParent(playerController.transform);
		Vector3 zero = Vector3.zero;
		zero.y = playerController.Midpoint.y - playerController.transform.position.y;
		m_traitMask.transform.localPosition = zero;
		m_traitMask.transform.localScale = new Vector3(20f, 20f, 1f);
	}

	public override void AssignGreenMask()
	{
		base.AssignGreenMask();
		m_postProcessOverrideController.Profile.OverrideBlurGreenChannel = true;
		m_postProcessOverrideController.Profile.BlurGreenChannel = m_postProcessOverrideController.Profile.BlurRedChannel;
		m_postProcessOverrideController.Profile.BlurRedChannel = 0f;
		m_postProcessOverrideController.Profile.OverrideBlurRedChannel = false;
	}

	protected void OnDestroy()
	{
		if ((bool)m_traitMask)
		{
			Object.Destroy(m_traitMask.gameObject);
		}
	}
}
