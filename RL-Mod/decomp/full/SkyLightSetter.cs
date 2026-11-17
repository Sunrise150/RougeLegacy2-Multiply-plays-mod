using System;
using UnityEngine;

public class SkyLightSetter : MonoBehaviour
{
	private Light m_light;

	private Action<MonoBehaviour, EventArgs> m_setSkyLight;

	private void Awake()
	{
		m_light = GetComponent<Light>();
		m_setSkyLight = SetSkyLight;
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.SkyLightColorChanged, m_setSkyLight);
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.SkyLightColorChanged, m_setSkyLight);
	}

	private void SetSkyLight(MonoBehaviour sender, EventArgs args)
	{
		SkyLightChangedEventArgs skyLightChangedEventArgs = args as SkyLightChangedEventArgs;
		Color color = m_light.color;
		Color newSkyLightColor = skyLightChangedEventArgs.NewSkyLightColor;
		Color color2 = Color.Lerp(color, newSkyLightColor, skyLightChangedEventArgs.Lerp);
		m_light.color = color2;
	}
}
