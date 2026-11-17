using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ParadePortrait : MonoBehaviour
{
	[Serializable]
	private class ParadeAnimatorEntry
	{
		public string StateName;

		public float AnimSpeed = 1f;

		public float ExitHoldDuration;
	}

	[SerializeField]
	private EnemyType m_enemyType;

	[SerializeField]
	private EnemyRank m_enemyRank;

	[SerializeField]
	private TMP_Text m_nameText;

	[SerializeField]
	private TMP_Text m_slainByPlayerText;

	[SerializeField]
	private TMP_Text m_slainPlayerText;

	[SerializeField]
	private Animator m_animator;

	[SerializeField]
	private GameObject m_model;

	[SerializeField]
	private GameObject m_hiddenGO;

	[SerializeField]
	private ParadeAnimatorEntry[] m_paradeAnimatorArray;

	[SerializeField]
	private bool m_runSimpleAnim;

	[SerializeField]
	private string m_startingAnimName;

	[SerializeField]
	private float m_loopDelay = 1f;

	[SerializeField]
	private bool m_loopAnim;

	private int m_animatorArrayIndex;

	public bool LoopAnim
	{
		get
		{
			return m_loopAnim;
		}
		set
		{
			m_loopAnim = value;
		}
	}

	public bool RunSimpleAnim
	{
		get
		{
			return m_runSimpleAnim;
		}
		set
		{
			m_runSimpleAnim = value;
		}
	}

	public Animator Animator => m_animator;

	public EnemyType EnemyType
	{
		get
		{
			return m_enemyType;
		}
		set
		{
			m_enemyType = value;
		}
	}

	public EnemyRank EnemyRank
	{
		get
		{
			return m_enemyRank;
		}
		set
		{
			m_enemyRank = value;
		}
	}

	public GameObject Model
	{
		get
		{
			return m_model;
		}
		set
		{
			m_model = value;
		}
	}

	public TMP_Text NameText => m_nameText;

	private void Awake()
	{
		if ((bool)m_animator)
		{
			EffectManager.AddAnimatorToDisableList(m_animator);
		}
		EnemyData enemyData = EnemyClassLibrary.GetEnemyData(m_enemyType, m_enemyRank);
		if ((bool)enemyData)
		{
			int enemiesDefeated = SaveManager.ModeSaveData.GetEnemiesDefeated(GameModeType.Regular, m_enemyType, m_enemyRank);
			int timesDefeatedByEnemy = SaveManager.ModeSaveData.GetTimesDefeatedByEnemy(GameModeType.Regular, m_enemyType, m_enemyRank);
			if (enemiesDefeated <= 0 && timesDefeatedByEnemy <= 0)
			{
				m_nameText.text = "??????";
				m_model.SetActive(value: false);
			}
			else
			{
				m_hiddenGO.SetActive(value: false);
				m_nameText.text = LocalizationManager.GetString(enemyData.Title, isFemale: false);
				if (m_enemyType == EnemyType.FlyingHunter)
				{
					m_animator.SetBool("Awake", value: true);
				}
			}
			m_slainByPlayerText.text = enemiesDefeated.ToString();
			m_slainPlayerText.text = timesDefeatedByEnemy.ToString();
		}
		if (m_runSimpleAnim)
		{
			m_paradeAnimatorArray = new ParadeAnimatorEntry[6];
			m_paradeAnimatorArray[1] = new ParadeAnimatorEntry
			{
				StateName = m_startingAnimName + "_Tell_Intro"
			};
			m_paradeAnimatorArray[2] = new ParadeAnimatorEntry
			{
				StateName = m_startingAnimName + "_Tell_Hold"
			};
			m_paradeAnimatorArray[3] = new ParadeAnimatorEntry
			{
				StateName = m_startingAnimName + "_Attack_Intro"
			};
			m_paradeAnimatorArray[4] = new ParadeAnimatorEntry
			{
				StateName = m_startingAnimName + "_Attack_Hold"
			};
			m_paradeAnimatorArray[5] = new ParadeAnimatorEntry
			{
				StateName = m_startingAnimName + "_Exit"
			};
			m_paradeAnimatorArray[0] = new ParadeAnimatorEntry
			{
				StateName = "Neutral"
			};
		}
		if (m_paradeAnimatorArray.Length != 0 && (bool)m_animator)
		{
			StartCoroutine(AnimateCoroutine());
		}
		if (m_enemyType == EnemyType.Slug)
		{
			m_animator.SetFloat("FacingSpeed", 1f);
		}
	}

	private IEnumerator AnimateCoroutine()
	{
		EffectManager.AddAnimatorToDisableList(m_animator);
		int stateArrayLength = m_paradeAnimatorArray.Length;
		while (m_animatorArrayIndex < stateArrayLength)
		{
			ParadeAnimatorEntry entry = m_paradeAnimatorArray[m_animatorArrayIndex];
			if (AnimatorUtility.HasState(m_animator, entry.StateName))
			{
				m_animator.Play(entry.StateName, 0, 0f);
				m_animator.SetFloat("Anim_Speed", entry.AnimSpeed);
			}
			yield return null;
			while (m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
			{
				yield return null;
			}
			float delay2 = Time.time + entry.ExitHoldDuration;
			while (Time.time < delay2)
			{
				yield return null;
			}
			m_animatorArrayIndex++;
			if (m_animatorArrayIndex >= stateArrayLength && m_loopAnim)
			{
				delay2 = Time.time + m_loopDelay;
				while (Time.time < delay2)
				{
					yield return null;
				}
				m_animatorArrayIndex = 0;
			}
		}
	}

	public void UpdateEnemyType()
	{
		EnemyController enemyPrefab = EnemyLibrary.GetEnemyPrefab(EnemyType, EnemyRank);
		if (!enemyPrefab)
		{
			return;
		}
		Transform transform = enemyPrefab.transform.FindDeep("Visuals");
		if ((bool)transform && transform.childCount > 0)
		{
			base.name = enemyPrefab.EnemyType.ToString() + " " + enemyPrefab.EnemyRank.ToString() + " Portrait";
			NameText.text = enemyPrefab.EnemyType.ToString() + " " + enemyPrefab.EnemyRank;
			UnityEngine.Object.DestroyImmediate(Model);
			int index = 0;
			for (int i = 0; i < transform.childCount; i++)
			{
				if (transform.GetChild(i).gameObject.activeSelf)
				{
					index = i;
					break;
				}
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(transform.GetChild(index).gameObject, base.transform);
			float scale = EnemyClassLibrary.GetEnemyData(enemyPrefab.EnemyType, enemyPrefab.EnemyRank).Scale;
			float x = gameObject.transform.localScale.x;
			gameObject.transform.localScale = new Vector3(scale, scale, scale) * x;
			Animator component = enemyPrefab.GetComponent<Animator>();
			Animator.runtimeAnimatorController = component.runtimeAnimatorController;
			Animator.avatar = component.avatar;
			Model = gameObject;
		}
		else
		{
			Debug.Log("<color=red>ERROR: Could not update portrait for enemy of type: " + enemyPrefab.EnemyType.ToString() + "." + enemyPrefab.EnemyRank.ToString() + " because Visuals child GO not found.</color>");
		}
	}
}
