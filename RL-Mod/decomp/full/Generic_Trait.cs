using UnityEngine;

public class Generic_Trait : BaseTrait
{
	[SerializeField]
	private TraitType m_traitType;

	public override TraitType TraitType => m_traitType;
}
