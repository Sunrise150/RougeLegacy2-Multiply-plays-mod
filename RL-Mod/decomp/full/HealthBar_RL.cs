using UnityEngine;

public class HealthBar_RL : MonoBehaviour
{
	private const float DEBUG_BAR_WIDTH = 2.5f;

	[Space(10f)]
	[SerializeField]
	private float m_currentHP;

	[SerializeField]
	private float m_maxHP = 1f;

	[Space(10f)]
	[SerializeField]
	private bool m_displayDebugUI = true;

	[SerializeField]
	private Vector2 m_UIOffset;

	private bool m_previousDisplayUI;

	private GameObject m_healthBarUI;

	private GameObject m_healthBarMaxHPUI;

	private GameObject m_healthBarCurrentHPUI;

	private GameObject m_healthBarScaler;

	public float GetCurrentHP()
	{
		return m_currentHP;
	}

	public void SetCurrentHP(float amount, bool additive)
	{
		if (additive)
		{
			m_currentHP += amount;
		}
		else
		{
			m_currentHP = amount;
		}
	}

	public float GetMaxHP()
	{
		return m_maxHP;
	}

	public void SetMaxHP(float amount)
	{
		m_maxHP = amount;
	}

	private void Awake()
	{
		GenerateDebugUI();
	}

	private void GenerateDebugUI()
	{
		m_healthBarUI = new GameObject();
		m_healthBarUI.name = "Health Bar Debug UI";
		m_healthBarUI.transform.SetParent(base.transform);
		m_healthBarUI.transform.localPosition = new Vector3(m_UIOffset.x, m_UIOffset.y, 0f);
		m_healthBarMaxHPUI = GameObject.CreatePrimitive(PrimitiveType.Cube);
		m_healthBarMaxHPUI.transform.SetParent(m_healthBarUI.transform);
		m_healthBarMaxHPUI.transform.localScale = new Vector3(2.5f, 0.25f, 1f);
		m_healthBarMaxHPUI.transform.localPosition = Vector3.zero;
		Renderer component = m_healthBarMaxHPUI.GetComponent<Renderer>();
		component.sortingLayerName = "Player";
		component.sortingOrder = 0;
		component.material.shader = Shader.Find("UI/Unlit/Transparent");
		component.material.color = Color.red;
		Collider component2 = m_healthBarMaxHPUI.GetComponent<Collider>();
		if (component2 != null)
		{
			Object.Destroy(component2);
		}
		m_healthBarScaler = new GameObject();
		m_healthBarScaler.transform.SetParent(m_healthBarUI.transform);
		m_healthBarScaler.transform.localPosition = new Vector3(-1.25f, 0f, 0f);
		m_healthBarCurrentHPUI = GameObject.CreatePrimitive(PrimitiveType.Cube);
		m_healthBarCurrentHPUI.transform.SetParent(m_healthBarScaler.transform);
		m_healthBarCurrentHPUI.transform.localScale = new Vector3(2.5f, 0.25f, 1f);
		m_healthBarCurrentHPUI.transform.localPosition = new Vector3(1.25f, 0f, 0f);
		Renderer component3 = m_healthBarCurrentHPUI.GetComponent<Renderer>();
		component3.sortingLayerName = "Player";
		component3.sortingOrder = 1;
		component3.material.shader = Shader.Find("UI/Unlit/Transparent");
		component3.material.color = Color.green;
		component2 = m_healthBarCurrentHPUI.GetComponent<Collider>();
		if (component2 != null)
		{
			Object.Destroy(component2);
		}
	}

	private void Update()
	{
		if (Application.isPlaying)
		{
			if (m_previousDisplayUI != m_displayDebugUI && m_displayDebugUI)
			{
				m_healthBarUI.SetActive(value: true);
			}
			else if (m_previousDisplayUI != m_displayDebugUI && !m_displayDebugUI)
			{
				m_healthBarUI.SetActive(value: false);
			}
			m_previousDisplayUI = m_displayDebugUI;
		}
		if (m_maxHP < 1f)
		{
			m_maxHP = 1f;
		}
		if (m_currentHP > m_maxHP)
		{
			m_currentHP = m_maxHP;
		}
		else if (m_currentHP <= 0f)
		{
			m_currentHP = 0f;
		}
		m_healthBarUI.transform.localPosition = new Vector3(m_UIOffset.x, m_UIOffset.y, 0f);
		m_healthBarScaler.transform.localScale = new Vector3(m_currentHP / m_maxHP, 1f, 1f);
	}
}
