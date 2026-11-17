using UnityEngine;

public class DialogueDisplayOverride_Johan : DialogueDisplayOverride
{
	[SerializeField]
	private JohanPropController.Johan_SpawnCondition m_spawnCondition;

	[SerializeField]
	private bool m_spawnIfFalse;

	public bool SpawnIfFalse => m_spawnIfFalse;

	public JohanPropController.Johan_SpawnCondition SpawnCondition => m_spawnCondition;
}
