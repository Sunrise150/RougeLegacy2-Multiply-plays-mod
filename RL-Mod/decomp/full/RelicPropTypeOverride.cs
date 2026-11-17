using UnityEngine;

public class RelicPropTypeOverride : MonoBehaviour
{
	[SerializeField]
	private RelicType m_relic1Override;

	[SerializeField]
	private RelicType m_relic2Override;

	public RelicType Relic1Override => m_relic1Override;

	public RelicType Relic2Override => m_relic2Override;
}
