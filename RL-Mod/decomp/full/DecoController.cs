using UnityEngine;

public class DecoController : MonoBehaviour
{
	[SerializeField]
	private DecoLocation[] m_decoLocations;

	private Prop m_ownerProp;

	public DecoLocation[] DecoLocations
	{
		get
		{
			return m_decoLocations;
		}
		set
		{
			m_decoLocations = value;
		}
	}

	public Prop OwnerProp
	{
		get
		{
			if (!m_ownerProp)
			{
				m_ownerProp = GetComponent<Prop>();
			}
			return m_ownerProp;
		}
	}

	public void Initialize(DecoSpawnData[] decoLocationSpawnData)
	{
		int num = DecoLocations.Length;
		for (int i = 0; i < num; i++)
		{
			DecoLocation decoLocation = DecoLocations[i];
			decoLocation.SetDecoController(this);
			DecoSpawnData decoSpawnData = decoLocationSpawnData[i];
			if (decoSpawnData.ShouldSpawn)
			{
				DecoEntry decoEntry = decoLocation.PotentialDecos[decoSpawnData.DecoPropIndex];
				bool isFlipped = false;
				Deco decoPrefab = null;
				if (decoEntry != null)
				{
					decoPrefab = decoEntry.Deco;
					isFlipped = decoEntry.CanFlip && decoSpawnData.IsFlipped;
				}
				decoLocation.Initialize(this, decoPrefab, isFlipped);
			}
		}
	}
}
