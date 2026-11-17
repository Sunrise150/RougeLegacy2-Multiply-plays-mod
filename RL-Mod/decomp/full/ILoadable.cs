using System.Collections;

public interface ILoadable
{
	void LoadSync();

	IEnumerator LoadAsync();
}
