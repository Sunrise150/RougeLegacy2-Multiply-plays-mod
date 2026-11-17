using UnityEngine;

public class HolidayBiomeArtDataOverrideController : MonoBehaviour
{
	[SerializeField]
	private HolidayType m_holidayType;

	[SerializeField]
	private BiomeArtData m_biomeArtData;

	private void Awake()
	{
		Room component = GetComponent<Room>();
		if ((bool)component)
		{
			if (HolidayLookController.IsHoliday(m_holidayType) && !string.IsNullOrEmpty(component.gameObject.scene.name))
			{
				component.ForceBiomeArtDataOverride(m_biomeArtData);
			}
		}
		else
		{
			Debug.Log("<color=yellow>Could not apply HolidayBiomeArtData.  No room found.</color>");
		}
	}
}
