using System;
using System.Collections;
using UnityEngine;

public class BouncySpike_AIScript : BaseAIScript
{
	private int m_startingAngle;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[0];
	}

	private void PerformAwayFromPlayerCheck()
	{
		float num = m_startingAngle;
		bool flag = false;
		float num2 = CDGHelper.AngleBetweenPts(base.EnemyController.Midpoint, PlayerManager.GetPlayerController().Midpoint);
		if (num2 < 90f && num > 270f)
		{
			num = CDGHelper.WrapAngleDegrees(num, convertToHalfRadius: true);
		}
		else if (num2 > 270f && num < 90f)
		{
			num2 = 0f - CDGHelper.WrapAngleDegrees(num2, convertToHalfRadius: true);
		}
		float num3 = num2 - num;
		if ((num3 < 45f && num3 > 0f) || (num3 > -45f && num3 < 0f))
		{
			flag = true;
		}
		int num4 = m_startingAngle;
		if (flag)
		{
			num4 = ((num4 <= 0) ? (num4 + 180) : (num4 - 180));
		}
		base.EnemyController.Orientation = (float)Math.PI / 180f * (float)num4;
		SetVelocity(base.EnemyController.HeadingX * base.EnemyController.ActualSpeed, base.EnemyController.HeadingY * base.EnemyController.ActualSpeed, additive: false);
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		int num = UnityEngine.Random.Range(0, 4);
		m_startingAngle = 45 + num * 90;
		base.EnemyController.OnResetPositionRelay.AddListener(OnResetPosition);
		base.EnemyController.OnPositionedForSummoningRelay.AddListener(OnPositionedForSummoning);
		if (PlayerManager.IsInstantiated)
		{
			PerformAwayFromPlayerCheck();
		}
	}

	private void OnResetPosition(object sender, EventArgs args)
	{
		if (base.IsInitialized && PlayerManager.IsInstantiated)
		{
			PerformAwayFromPlayerCheck();
		}
	}

	private void OnPositionedForSummoning(object sender, EventArgs args)
	{
		if (base.IsInitialized && PlayerManager.IsInstantiated)
		{
			PerformAwayFromPlayerCheck();
		}
	}

	public override IEnumerator Idle()
	{
		base.EnemyController.FlyingMovementType = FlyingMovementType.Override;
		while (true)
		{
			yield return null;
		}
	}

	private void Update()
	{
		if (!base.IsInitialized)
		{
			return;
		}
		if (base.IsPaused)
		{
			if (base.EnemyController.Velocity != Vector2.zero)
			{
				base.EnemyController.SetVelocity(0f, 0f, additive: false);
			}
			return;
		}
		float num = base.EnemyController.ActualSpeed * 20f;
		Vector3 localEulerAngles = base.EnemyController.Visuals.transform.localEulerAngles;
		if (m_startingAngle < 90 || m_startingAngle > 270)
		{
			localEulerAngles.z += num * Time.deltaTime;
		}
		else
		{
			localEulerAngles.z -= num * Time.deltaTime;
		}
		base.EnemyController.Visuals.transform.localEulerAngles = localEulerAngles;
		if (base.EnemyController.Velocity == Vector2.zero)
		{
			SetVelocity(base.EnemyController.HeadingX * base.EnemyController.ActualSpeed, base.EnemyController.HeadingY * base.EnemyController.ActualSpeed, additive: false);
		}
	}

	private void OnDestroy()
	{
		if ((bool)base.EnemyController)
		{
			base.EnemyController.OnResetPositionRelay.RemoveListener(OnResetPosition);
			base.EnemyController.OnPositionedForSummoningRelay.RemoveListener(OnPositionedForSummoning);
		}
	}
}
