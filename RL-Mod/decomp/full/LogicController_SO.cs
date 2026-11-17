using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Rogue Legacy 2/Enemy Logic Controller")]
public class LogicController_SO : ScriptableObject
{
	[Space(10f)]
	public StringInt_Dictionary BasicCloseLogic;

	[Space(10f)]
	public StringInt_Dictionary BasicMediumLogic;

	[Space(10f)]
	public StringInt_Dictionary BasicFarLogic;

	[Space(10f)]
	public StringInt_Dictionary BasicRestLogic;

	[Space(10f)]
	public StringInt_Dictionary BasicWanderLogic;

	[Space(10f)]
	public StringInt_Dictionary AdvancedCloseLogic;

	[Space(10f)]
	public StringInt_Dictionary AdvancedMediumLogic;

	[Space(10f)]
	public StringInt_Dictionary AdvancedFarLogic;

	[Space(10f)]
	public StringInt_Dictionary AdvancedRestLogic;

	[Space(10f)]
	public StringInt_Dictionary AdvancerWanderLogic;

	[Space(10f)]
	public StringInt_Dictionary ExpertCloseLogic;

	[Space(10f)]
	public StringInt_Dictionary ExpertMediumLogic;

	[Space(10f)]
	public StringInt_Dictionary ExpertFarLogic;

	[Space(10f)]
	public StringInt_Dictionary ExpertRestLogic;

	[Space(10f)]
	public StringInt_Dictionary ExpertWanderLogic;

	[Space(10f)]
	public StringInt_Dictionary MinibossCloseLogic;

	[Space(10f)]
	public StringInt_Dictionary MinibossMediumLogic;

	[Space(10f)]
	public StringInt_Dictionary MinibossFarLogic;

	[Space(10f)]
	public StringInt_Dictionary MinibossRestLogic;

	[Space(10f)]
	public StringInt_Dictionary MinibossWanderLogic;
}
