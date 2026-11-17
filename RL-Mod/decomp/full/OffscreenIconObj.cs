using System.Runtime.CompilerServices;
using UnityEngine;

public class OffscreenIconObj : MonoBehaviour, IGenericPoolObj
{
	[SerializeField]
	private SpriteRenderer m_sprite;

	[SerializeField]
	private GameObject m_bg;

	[SerializeField]
	private GameObject m_iconGO;

	private Projectile_RL m_attachedProjectile;

	public bool IsFreePoolObj { get; set; }

	public bool IsAwakeCalled { get; protected set; } = true;


	public void AttachProjectile(Projectile_RL projectile)
	{
		m_attachedProjectile = projectile;
		m_iconGO.SetActive(value: false);
	}

	private void OnEnable()
	{
		if (CameraController.IsInstantiated && !base.transform.parent)
		{
			base.transform.SetParent(CameraController.GameCamera.transform);
		}
	}

	private void OnDisable()
	{
		m_attachedProjectile = null;
		DisablePooledObjectManager.DisablePooledObject(this);
	}

	private void FixedUpdate()
	{
		if (!m_attachedProjectile || !m_attachedProjectile.gameObject.activeSelf || TraitManager.IsTraitActive(TraitType.NoProjectileIndicators))
		{
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: false);
			}
			return;
		}
		float num = 1f;
		float orthographicSize = CameraController.GameCamera.orthographicSize;
		float num2 = orthographicSize * (float)Screen.width / (float)Screen.height;
		float x = CameraController.GameCamera.transform.localPosition.x - num2;
		float y = CameraController.GameCamera.transform.localPosition.y - orthographicSize;
		Rect rect = new Rect(x, y, num2 * 2f, orthographicSize * 2f);
		PlayerController playerController = PlayerManager.GetPlayerController();
		bool num3 = rect.Contains(m_attachedProjectile.Midpoint);
		bool flag = false;
		if (!num3)
		{
			Vector2 velocity = m_attachedProjectile.Velocity;
			Vector2 vector = m_attachedProjectile.transform.localPosition;
			Vector2 vector2 = playerController.Midpoint;
			bool flag2 = vector.y < rect.yMax && vector.y > rect.yMin;
			bool flag3 = vector.x > rect.xMin && vector.x < rect.xMax;
			float num4 = 0.01f;
			bool flag4 = (velocity.x > num4 && vector.x < vector2.x) || (velocity.x < 0f - num4 && vector.x > vector2.x);
			bool flag5 = (velocity.y > num4 && vector.y < vector2.y) || (velocity.y < 0f - num4 && vector.y > vector2.y);
			if ((flag4 && flag5) || (flag4 && flag2) || (flag5 && flag3))
			{
				flag = true;
			}
		}
		bool flag6 = !num3 && flag;
		if (!flag6 && m_iconGO.gameObject.activeSelf)
		{
			m_iconGO.gameObject.SetActive(value: false);
		}
		else if (flag6 && !m_iconGO.gameObject.activeSelf)
		{
			m_iconGO.gameObject.SetActive(value: true);
		}
		if (m_iconGO.gameObject.activeSelf)
		{
			Vector3 localPosition = m_attachedProjectile.transform.localPosition;
			Vector3 vector3 = localPosition - CameraController.GameCamera.gameObject.transform.localPosition;
			float num5 = rect.xMin + num;
			float num6 = rect.xMax - num;
			float num7 = rect.yMin + num;
			float num8 = rect.yMax - num;
			if (localPosition.x <= num5)
			{
				vector3.x = 0f - num2 + num;
			}
			else if (localPosition.x > num6)
			{
				vector3.x = num2 - num;
			}
			if (localPosition.y <= num7)
			{
				vector3.y = 0f - orthographicSize + num;
			}
			else if (localPosition.y > num8)
			{
				vector3.y = orthographicSize - num;
			}
			float z = 1f + (localPosition.y - rect.yMin) / (rect.yMax - rect.yMin);
			vector3.z = z;
			base.transform.localPosition = vector3;
			float z2 = CDGHelper.AngleBetweenPts(vector3 + CameraController.GameCamera.gameObject.transform.localPosition, m_attachedProjectile.Midpoint);
			Vector3 localEulerAngles = m_bg.transform.localEulerAngles;
			localEulerAngles.z = z2;
			m_bg.transform.localEulerAngles = localEulerAngles;
		}
	}

	public void ResetValues()
	{
	}

	[SpecialName]
	GameObject IGenericPoolObj.get_gameObject()
	{
		return base.gameObject;
	}
}
