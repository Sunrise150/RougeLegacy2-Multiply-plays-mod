using System.Collections.Generic;

public class FreeRelic_Trait : BaseTrait
{
	public override TraitType TraitType => TraitType.FreeRelic;

	private void Start()
	{
		GiveFreeRelic();
	}

	private void GiveFreeRelic()
	{
		bool flag = false;
		foreach (KeyValuePair<RelicType, RelicObj> item in SaveManager.PlayerSaveData.RelicObjTable)
		{
			if (item.Value.IsFreeRelic)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			RelicType randomRelic = RelicLibrary.GetRandomRelic(RngID.None, limitMaxStacks: true, Antique_Trait.RelicExceptionArray);
			RelicObj relic = SaveManager.PlayerSaveData.GetRelic(randomRelic);
			relic.SetLevel(1, additive: false);
			relic.IsFreeRelic = true;
		}
	}
}
