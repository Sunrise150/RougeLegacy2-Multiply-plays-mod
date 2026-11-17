using FMODUnity;

public class ToneDeaf_Trait : BaseTrait
{
	public override TraitType TraitType => TraitType.ToneDeaf;

	private void Start()
	{
		RuntimeManager.StudioSystem.setParameterByName("traitTonedeaf", 1f);
	}

	private void OnDestroy()
	{
		RuntimeManager.StudioSystem.setParameterByName("traitTonedeaf", 0f);
	}
}
