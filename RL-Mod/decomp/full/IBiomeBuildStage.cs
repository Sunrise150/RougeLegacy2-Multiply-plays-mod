using System.Collections;

public interface IBiomeBuildStage
{
	IEnumerator Run(BiomeController biomeController);
}
