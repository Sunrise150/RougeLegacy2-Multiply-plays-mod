using UnityEngine;

public class MobilePostProcessOverrideController : MonoBehaviour
{
	[SerializeField]
	private MobilePostProcessingProfile m_profile;

	[SerializeField]
	private bool m_isTraitEffect = true;

	private MobilePostProcessingProfile m_profileClone;

	public MobilePostProcessingProfile Profile => m_profileClone;

	private void Awake()
	{
		m_profileClone = m_profile.Clone();
	}

	private void OnEnable()
	{
		if (CameraController.IsInstantiated && (bool)CameraController.ForegroundPostProcessing && Profile != null)
		{
			if (m_isTraitEffect)
			{
				CameraController.ForegroundPostProcessing.AddTraitOverride(Profile);
			}
			else
			{
				CameraController.ForegroundPostProcessing.AddDimensionOverride(Profile);
			}
		}
	}

	private void OnDisable()
	{
		if (CameraController.IsInstantiated && (bool)CameraController.ForegroundPostProcessing && Profile != null)
		{
			if (m_isTraitEffect)
			{
				CameraController.ForegroundPostProcessing.RemoveTraitOverride(Profile);
			}
			else
			{
				CameraController.ForegroundPostProcessing.RemoveDimensionOverride(Profile);
			}
		}
	}
}
