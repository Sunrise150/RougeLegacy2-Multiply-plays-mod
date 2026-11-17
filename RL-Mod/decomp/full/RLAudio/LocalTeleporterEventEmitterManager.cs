using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class LocalTeleporterEventEmitterManager : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	private LocalTeleporterController m_teleporter;

	[SerializeField]
	[EventRef]
	private string m_enterAudioPath;

	[SerializeField]
	[EventRef]
	private string m_exitAudioPath;

	[SerializeField]
	[EventRef]
	private string m_cornerHitAudioPath;

	[SerializeField]
	[EventRef]
	private string m_travelAudioPath;

	private EventInstance m_travelEventInstance;

	private float m_teletubeLength = -1f;

	private WaitForSeconds m_delayBeforeSettingParameters = new WaitForSeconds(0.25f);

	private const string TELETUBE_PROGRESS_PARAMETER = "teletube_progress";

	private const string TELETUBE_SPEED_PARAMETER = "teletube_speed";

	private const float TELETUBE_END_POINT_SQUARE_MAGNITUDE = 10f;

	public string Description => ToString();

	private void Awake()
	{
		m_teleporter.OnEnterEvent.AddListener(OnPlayerEnter);
		m_teleporter.OnExitEvent.AddListener(OnPlayerExit);
		m_teleporter.OnHitLeyCornerEvent.AddListener(OnPlayerHitCorner);
		m_travelEventInstance = AudioUtility.GetEventInstance(m_travelAudioPath, base.transform);
	}

	private void OnDestroy()
	{
		if (m_travelEventInstance.isValid())
		{
			m_travelEventInstance.release();
		}
	}

	private void OnPlayerEnter()
	{
		AudioManager.PlayOneShot(this, m_enterAudioPath, m_teleporter.transform.position);
		StartCoroutine(UpdateTravelAudioParameters());
	}

	private IEnumerator CalculateTeletubeLength()
	{
		while (LocalTeleporterController.LeylinePoints == null || LocalTeleporterController.LeylinePoints.Count == 0)
		{
			yield return null;
		}
		for (int i = 0; i < LocalTeleporterController.LeylinePoints.Count - 1; i++)
		{
			m_teletubeLength += (LocalTeleporterController.LeylinePoints[i + 1] - LocalTeleporterController.LeylinePoints[i]).magnitude;
		}
	}

	private IEnumerator UpdateTravelAudioParameters()
	{
		if (m_teletubeLength == -1f)
		{
			yield return CalculateTeletubeLength();
		}
		yield return m_delayBeforeSettingParameters;
		AudioManager.PlayAttached(this, m_travelEventInstance, PlayerManager.GetPlayerController().gameObject);
		m_travelEventInstance.setParameterByName("teletube_speed", 1f);
		float totalDistanceTravelled = 0f;
		Vector2 previousPlayerPositionInTube = PlayerManager.GetPlayerController().transform.position;
		while (true)
		{
			Vector2 vector = PlayerManager.GetPlayerController().transform.position;
			Vector2 vector2 = vector - previousPlayerPositionInTube;
			previousPlayerPositionInTube = vector;
			float magnitude = vector2.magnitude;
			totalDistanceTravelled += magnitude;
			float value = totalDistanceTravelled / m_teletubeLength;
			if (m_teleporter.CurrentCornerIndex == m_teleporter.CornerCount && ((Vector2)m_teleporter.LeyLines.transform.TransformPoint(LocalTeleporterController.LeylinePoints[LocalTeleporterController.LeylinePoints.Count - 1]) - vector).sqrMagnitude < 10f)
			{
				break;
			}
			m_travelEventInstance.setParameterByName("teletube_progress", value);
			yield return null;
		}
		m_travelEventInstance.setParameterByName("teletube_speed", 0f);
	}

	private void OnPlayerExit()
	{
		StopAllCoroutines();
		AudioManager.PlayOneShot(this, m_exitAudioPath, m_teleporter.TeleporterLocation.transform.position);
		AudioManager.Stop(m_travelEventInstance);
	}

	private void OnPlayerHitCorner(Vector2 coordinates)
	{
		AudioManager.PlayOneShot(this, m_cornerHitAudioPath, coordinates);
	}
}
