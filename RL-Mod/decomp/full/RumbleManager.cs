using Rewired;
using UnityEngine;

public class RumbleManager
{
	private const float MOTOR_DISTANCE_DELTA = 2f;

	private const float MAX_DISTANCE = 10f;

	private static float m_leftStartRumbleTimer;

	private static float m_rightStartRumbleTimer;

	private static float m_leftPausedDuration;

	private static float m_rightPausedDuration;

	private static float m_leftMotorPauseAmount;

	private static float m_rightMotorPauseAmount;

	private static bool m_isPaused;

	public static bool IsPaused => m_isPaused;

	public static bool IsRumbling
	{
		get
		{
			if (!IsLeftRumbling)
			{
				return IsRightRumbling;
			}
			return true;
		}
	}

	public static bool IsLeftRumbling => Time.unscaledTime < m_leftStartRumbleTimer;

	public static bool IsRightRumbling => Time.unscaledTime < m_rightStartRumbleTimer;

	public static void StartRumble(bool useLeftMotor, bool useRightMotor, float amount, float duration = 0f, bool stopAllRumbleFirst = true)
	{
		if (!ReInput.isReady || RewiredOnStartupController.CurrentActiveControllerType != ControllerType.Joystick || SaveManager.ConfigData.DisableRumble)
		{
			return;
		}
		m_isPaused = false;
		m_leftMotorPauseAmount = 0f;
		m_rightMotorPauseAmount = 0f;
		duration = Mathf.Max(0f, duration);
		if (duration > 0f)
		{
			if (useLeftMotor)
			{
				m_leftStartRumbleTimer = Time.unscaledTime + duration;
			}
			if (useRightMotor)
			{
				m_rightStartRumbleTimer = Time.unscaledTime + duration;
			}
		}
		else
		{
			if (useLeftMotor)
			{
				m_leftStartRumbleTimer = Time.unscaledTime + 99999f;
			}
			if (useRightMotor)
			{
				m_rightStartRumbleTimer = Time.unscaledTime + 99999f;
			}
		}
		foreach (Joystick joystick in Rewired_RL.Player.controllers.Joysticks)
		{
			if (joystick.supportsVibration && joystick.vibrationMotorCount > 0)
			{
				int motorIndex = ((joystick.vibrationMotorCount > 1) ? 1 : 0);
				float leftMotorLevel;
				float rightMotorLevel;
				if (stopAllRumbleFirst)
				{
					leftMotorLevel = (useLeftMotor ? amount : 0f);
					rightMotorLevel = (useRightMotor ? amount : 0f);
				}
				else
				{
					leftMotorLevel = (useLeftMotor ? amount : joystick.GetVibration(0));
					rightMotorLevel = (useRightMotor ? amount : joystick.GetVibration(motorIndex));
				}
				joystick.SetVibration(leftMotorLevel, rightMotorLevel, duration, duration);
			}
		}
	}

	public static void Start3DRumble(float amount, Vector2 emitterPos, Vector2 listenerPos, float duration = 0f, float distDeltaMod = 1f)
	{
		float num = 2f;
		float num2 = 10f * distDeltaMod;
		float magnitude = (emitterPos - new Vector2(listenerPos.x - num, listenerPos.y)).magnitude;
		float magnitude2 = (emitterPos - new Vector2(listenerPos.x + num, listenerPos.y)).magnitude;
		float num3 = 1f - Mathf.Min(magnitude, num2) / num2;
		if (emitterPos.x < listenerPos.x)
		{
			num3 = Mathf.Max(0.1f, num3);
		}
		float num4 = 1f - Mathf.Min(magnitude2, num2) / num2;
		if (emitterPos.x > listenerPos.x)
		{
			num4 = Mathf.Max(0.1f, num4);
		}
		StartRumble(useLeftMotor: true, useRightMotor: false, num3 * amount, duration, stopAllRumbleFirst: false);
		StartRumble(useLeftMotor: false, useRightMotor: true, num4 * amount, duration, stopAllRumbleFirst: false);
	}

	public static void StopRumble(bool stopLeftMotor = true, bool stopRightMotor = true)
	{
		m_isPaused = false;
		if (!ReInput.isReady || !IsRumbling)
		{
			return;
		}
		foreach (Joystick joystick in Rewired_RL.Player.controllers.Joysticks)
		{
			if (joystick.supportsVibration && joystick.vibrationMotorCount > 0)
			{
				int motorIndex = ((joystick.vibrationMotorCount > 1) ? 1 : 0);
				float leftMotorLevel = (stopLeftMotor ? 0f : joystick.GetVibration(0));
				float rightMotorLevel = (stopRightMotor ? 0f : joystick.GetVibration(motorIndex));
				joystick.SetVibration(leftMotorLevel, rightMotorLevel);
			}
		}
		if (stopLeftMotor)
		{
			m_leftStartRumbleTimer = 0f;
		}
		if (stopRightMotor)
		{
			m_rightStartRumbleTimer = 0f;
		}
	}

	public static void SetRumblePaused(bool paused)
	{
		if (m_isPaused == paused)
		{
			return;
		}
		m_isPaused = paused;
		if (paused)
		{
			if (IsLeftRumbling)
			{
				m_leftPausedDuration = m_leftStartRumbleTimer - Time.unscaledTime;
				m_leftMotorPauseAmount = GetMotorAmount(getRightMotor: false);
			}
			else
			{
				m_leftPausedDuration = 0f;
				m_leftMotorPauseAmount = 0f;
			}
			if (IsRightRumbling)
			{
				m_rightPausedDuration = m_rightStartRumbleTimer - Time.unscaledTime;
				m_rightMotorPauseAmount = GetMotorAmount(getRightMotor: true);
			}
			else
			{
				m_rightPausedDuration = 0f;
				m_rightMotorPauseAmount = 0f;
			}
			StopRumble();
		}
		else
		{
			if (m_leftPausedDuration > 0f)
			{
				StartRumble(useLeftMotor: true, useRightMotor: false, m_leftMotorPauseAmount, m_leftPausedDuration, stopAllRumbleFirst: false);
			}
			if (m_rightMotorPauseAmount > 0f)
			{
				StartRumble(useLeftMotor: false, useRightMotor: true, m_rightMotorPauseAmount, m_rightPausedDuration, stopAllRumbleFirst: false);
			}
		}
	}

	private static float GetMotorAmount(bool getRightMotor)
	{
		if (!ReInput.isReady)
		{
			return 0f;
		}
		float num = 0f;
		foreach (Joystick joystick in Rewired_RL.Player.controllers.Joysticks)
		{
			if (joystick.supportsVibration && joystick.vibrationMotorCount > 0)
			{
				int motorIndex = ((joystick.vibrationMotorCount > 1) ? 1 : 0);
				float num2 = ((!getRightMotor) ? joystick.GetVibration(0) : joystick.GetVibration(motorIndex));
				if (num2 > num)
				{
					num = num2;
				}
			}
		}
		return num;
	}
}
