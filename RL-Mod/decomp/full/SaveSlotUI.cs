using UnityEngine;
using UnityEngine.UI;

public class SaveSlotUI : MonoBehaviour
{
	private Button m_button;

	public byte SlotIndex { get; set; }

	public Button Button => m_button;

	public string PlayerData_FullFilePath { get; set; }

	protected virtual void Awake()
	{
		m_button = GetComponent<Button>();
	}
}
