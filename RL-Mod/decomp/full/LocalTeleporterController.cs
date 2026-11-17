using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

[SelectionBase]
public class LocalTeleporterController : MonoBehaviour, IRootObj
{
	private const float TELEPORTER_SPEED = 40f;

	private static LocalTeleporterController m_closestTeleporter_STATIC;

	private static WaitForEndOfFrame m_endOfFrameYield_STATIC = new WaitForEndOfFrame();

	[SerializeField]
	private LocalTeleporterController m_teleportLocation;

	[SerializeField]
	private Ferr2DT_PathTerrain m_leyLines;

	[SerializeField]
	private SpriteRenderer m_teleporterSprite;

	[SerializeField]
	private Color m_teleporterColor = Color.white;

	[SerializeField]
	public UnityEvent_Vector2 OnHitLeyCornerEvent;

	[SerializeField]
	public UnityEvent OnEnterEvent;

	[SerializeField]
	public UnityEvent OnExitEvent;

	[SerializeField]
	private GameObject m_spark;

	private Interactable m_interactable;

	private Animator m_animator;

	private static bool m_isTeleporting;

	private static List<Vector2> m_leylineHelperList = new List<Vector2>();

	public static bool IsTeleporting => m_isTeleporting;

	public Color TeleporterColor
	{
		get
		{
			return m_teleporterColor;
		}
		set
		{
			if (m_teleporterColor != value)
			{
				m_teleporterColor = value;
				UpdateTeleporterColour();
			}
		}
	}

	public LocalTeleporterController TeleporterLocation => m_teleportLocation;

	public Animator Animator => m_animator;

	public Room Room { get; set; }

	public Ferr2DT_PathTerrain LeyLines => m_leyLines;

	public int CurrentCornerIndex { get; private set; }

	public int CornerCount { get; private set; }

	public static List<Vector2> LeylinePoints => m_leylineHelperList;

	private void UpdateTeleporterColour()
	{
		if ((bool)m_teleporterSprite && (bool)m_teleportLocation)
		{
			m_teleporterSprite.color = m_teleporterColor;
			if (m_teleportLocation != null && m_teleportLocation.m_teleporterSprite.color != m_teleporterColor)
			{
				m_teleportLocation.m_teleporterSprite.color = m_teleporterColor;
				m_teleportLocation.m_teleporterColor = m_teleporterColor;
			}
			if (m_leyLines != null)
			{
				m_leyLines.vertexColor = m_teleporterColor;
				m_leyLines.CreateVertColors();
			}
		}
	}

	private void OnDisable()
	{
		if ((bool)m_interactable)
		{
			m_interactable.TriggerOnEnterEvent.RemoveListener(OnInteract);
		}
		m_isTeleporting = false;
	}

	private void OnDestroy()
	{
		if (m_closestTeleporter_STATIC == this)
		{
			m_closestTeleporter_STATIC = null;
		}
	}

	private void OnEnable()
	{
		if ((bool)m_interactable)
		{
			m_interactable.TriggerOnEnterEvent.AddListener(OnInteract);
		}
		if ((bool)m_teleportLocation)
		{
			m_animator.SetBool("Open", value: true);
		}
		else
		{
			m_animator.SetBool("Open", value: false);
		}
	}

	private void Awake()
	{
		Room = GetComponent<Room>();
		m_interactable = GetComponent<Interactable>();
		m_animator = GetComponent<Animator>();
		if (m_teleportLocation != null)
		{
			m_animator.SetBool("Open", value: true);
		}
		else
		{
			m_animator.SetBool("Open", value: false);
			m_interactable.SetIsInteractableActive(isActive: false);
		}
		if (Application.isPlaying && TeleporterColor != Color.white)
		{
			TeleporterColor = Color.white;
		}
	}

	private void OnInteract(GameObject otherObj)
	{
		if (m_isTeleporting)
		{
			return;
		}
		if ((bool)m_teleportLocation)
		{
			if ((bool)m_closestTeleporter_STATIC)
			{
				PlayerController playerController = PlayerManager.GetPlayerController();
				float num = CDGHelper.DistanceBetweenPts(m_closestTeleporter_STATIC.transform.position, playerController.transform.localPosition);
				if (CDGHelper.DistanceBetweenPts(base.transform.position, playerController.transform.localPosition) < num)
				{
					m_closestTeleporter_STATIC = this;
				}
			}
			else
			{
				m_closestTeleporter_STATIC = this;
				StartCoroutine(StartTeleporterAtEndOfFrameCoroutine());
			}
		}
		else
		{
			Debug.Log("<color=red>Could not locally teleport player. Teleport location null.</color>");
		}
	}

	private void StartTeleporterCoroutine()
	{
		StopAllCoroutines();
		StartCoroutine(TeleportPlayerCoroutine());
	}

	private IEnumerator StartTeleporterAtEndOfFrameCoroutine()
	{
		if ((bool)m_closestTeleporter_STATIC)
		{
			yield return m_endOfFrameYield_STATIC;
			if (!PlayerManager.GetPlayerController().IsDead)
			{
				m_closestTeleporter_STATIC.StartTeleporterCoroutine();
			}
		}
	}

	public static void StopTeleportPlayer()
	{
		if ((bool)m_closestTeleporter_STATIC)
		{
			m_closestTeleporter_STATIC.StopTeleportPlayer_Internal();
		}
	}

	private void StopTeleportPlayer_Internal()
	{
		StopAllCoroutines();
		TweenManager.StopAllTweensContaining("LocalTeleporter", runEndHandlers: false);
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.Visuals.SetActive(value: true);
		playerController.ControllerCorgi.GravityActive(state: true);
		playerController.ControllerCorgi.CollisionsOn();
		playerController.HitboxController.SetHitboxActiveState(HitboxType.Body, active: true);
		playerController.HitboxController.SetHitboxActiveState(HitboxType.Terrain, active: true);
		m_spark.transform.SetParent(base.transform, worldPositionStays: false);
		m_spark.gameObject.SetActive(value: false);
		playerController.CharacterHitResponse.StopInvincibleTime();
		m_interactable.SetIsInteractableActive(isActive: true);
		m_closestTeleporter_STATIC = null;
		m_isTeleporting = false;
	}

	private IEnumerator TeleportPlayerCoroutine()
	{
		if (OnEnterEvent != null)
		{
			OnEnterEvent.Invoke();
		}
		m_isTeleporting = true;
		m_interactable.SetIsInteractableActive(isActive: false);
		RumbleManager.StartRumble(useLeftMotor: true, useRightMotor: true, 0.5f, 0.2f);
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.Visuals.SetActive(value: false);
		playerController.StopActiveAbilities(stopPersistentAbilities: false);
		playerController.ControllerCorgi.GravityActive(state: false);
		playerController.ControllerCorgi.CollisionsOff();
		playerController.SetVelocity(0f, 0f, additive: false);
		playerController.CharacterHitResponse.SetInvincibleTime(999f, additive: false, playInvincibility: false);
		playerController.HitboxController.SetHitboxActiveState(HitboxType.Body, active: false);
		playerController.HitboxController.SetHitboxActiveState(HitboxType.Terrain, active: false);
		m_spark.gameObject.SetActive(value: true);
		m_spark.transform.SetParent(playerController.transform, worldPositionStays: false);
		if ((bool)m_leyLines)
		{
			LeylineController leylineController = m_leyLines.GetComponent<LeylineController>();
			m_leylineHelperList.Clear();
			m_leylineHelperList.AddRange(m_leyLines.PathData.GetFinalPath());
			CornerCount = m_leylineHelperList.Count - 2;
			bool reversed = false;
			if (m_leylineHelperList[0] + (Vector2)m_leyLines.transform.position != (Vector2)base.transform.position)
			{
				m_leylineHelperList.Reverse();
				reversed = true;
			}
			bool flag = m_leylineHelperList[0] + (Vector2)m_leyLines.transform.position == (Vector2)base.transform.position;
			bool flag2 = m_leylineHelperList[m_leylineHelperList.Count - 1] + (Vector2)m_leyLines.transform.position == (Vector2)m_teleportLocation.transform.position;
			if (!flag || !flag2)
			{
				if (!flag)
				{
					Debug.LogWarning("<color=yellow>Could not activate leyline animation.  No starting leyline attached to the entry teleporter.");
				}
				if (!flag2)
				{
					Debug.LogWarning("<color=yellow>Could not activate leyline animation.  No ending leyline attached to the exit teleporter.");
				}
				Vector3 position = m_teleportLocation.transform.position;
				position.y += playerController.Midpoint.y - playerController.transform.position.y;
				playerController.transform.position = position;
			}
			else
			{
				Tween tween = TweenManager.TweenTo(playerController.transform, 0.1f, Ease.None, "position.x", base.transform.position.x, "position.y", base.transform.position.y);
				tween.ID = "LocalTeleporter";
				yield return tween.TweenCoroutine;
				int rawPointIndex = (reversed ? (m_leyLines.PathData.Count - 1) : 0);
				int leypointCount = m_leylineHelperList.Count;
				for (int i = 0; i < leypointCount - 1; i++)
				{
					CurrentCornerIndex = i;
					Vector2 vector = m_leylineHelperList[i];
					Vector2 vector2 = m_leylineHelperList[i + 1];
					Vector2 vector3 = vector2 - vector;
					Vector3 vector4 = m_leyLines.transform.TransformPoint(vector);
					Vector3 vector5 = m_leyLines.transform.TransformPoint(vector2);
					int smoothIndex = m_leyLines.PathData.GetSmoothIndex(rawPointIndex);
					int num = ((!reversed) ? i : (leypointCount - 1 - i));
					bool num2 = smoothIndex == num;
					if (num2)
					{
						rawPointIndex = (reversed ? (rawPointIndex - 1) : (rawPointIndex + 1));
					}
					if (num2 && rawPointIndex >= 0 && rawPointIndex < m_leyLines.PathData.Count && OnHitLeyCornerEvent != null && i > 0 && i < leypointCount - 1)
					{
						OnHitLeyCornerEvent.Invoke(vector4);
					}
					EaseDelegate ease = Ease.None;
					if (leypointCount > 2)
					{
						if (i == 0)
						{
							ease = Ease.Cubic.EaseIn;
						}
						else if (i == leypointCount - 2)
						{
							ease = Ease.Cubic.EaseOut;
						}
					}
					else
					{
						ease = Ease.Cubic.EaseInOut;
					}
					float num3 = CDGHelper.DistanceBetweenPts(vector, vector2) / 40f;
					if (leypointCount > 2 && (i == 0 || i == leypointCount - 2))
					{
						num3 *= 2f;
					}
					float deltaTime = Time.deltaTime;
					deltaTime *= 2f;
					if (num3 < deltaTime)
					{
						playerController.transform.localPosition = new Vector3(vector5.x, vector5.y, playerController.transform.localPosition.z);
						yield return null;
						continue;
					}
					tween = ((!leylineController.UseCircleCorners) ? TweenManager.TweenBy(playerController.transform, num3, ease, "localPosition.x", vector3.x, "localPosition.y", vector3.y) : TweenManager.TweenBy(playerController.transform, num3, ease, "subtract", Time.deltaTime, "localPosition.x", vector3.x, "localPosition.y", vector3.y));
					tween.ID = "LocalTeleporter";
					yield return tween.TweenCoroutine;
				}
				m_teleportLocation.Animator.SetTrigger("Expel");
				Vector3 position2 = m_teleportLocation.transform.position;
				position2.y -= playerController.Midpoint.y - playerController.transform.position.y;
				tween = TweenManager.TweenTo(playerController.transform, 0.1f, Ease.None, "position.x", position2.x, "position.y", position2.y);
				tween.ID = "LocalTeleporter";
				yield return tween.TweenCoroutine;
			}
		}
		else
		{
			m_teleportLocation.Animator.SetTrigger("Expel");
			Vector3 position3 = m_teleportLocation.transform.position;
			position3.y += playerController.Midpoint.y - playerController.transform.position.y;
			playerController.transform.position = position3;
		}
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		playerController.Visuals.SetActive(value: true);
		playerController.ControllerCorgi.GravityActive(state: true);
		playerController.ControllerCorgi.CollisionsOn();
		playerController.HitboxController.SetHitboxActiveState(HitboxType.Body, active: true);
		playerController.HitboxController.SetHitboxActiveState(HitboxType.Terrain, active: true);
		m_spark.transform.SetParent(base.transform, worldPositionStays: false);
		m_spark.gameObject.SetActive(value: false);
		RumbleManager.StartRumble(useLeftMotor: true, useRightMotor: true, 0.5f, 0.2f);
		if (OnExitEvent != null)
		{
			OnExitEvent.Invoke();
		}
		playerController.CharacterHitResponse.SetInvincibleTime(0.2f, additive: false, playInvincibility: false);
		m_interactable.SetIsInteractableActive(isActive: true);
		m_closestTeleporter_STATIC = null;
		m_isTeleporting = false;
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
