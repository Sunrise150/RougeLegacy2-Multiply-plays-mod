using UnityEngine;

public interface IGenericPoolObj
{
	GameObject gameObject { get; }

	bool IsAwakeCalled { get; }

	bool IsFreePoolObj { get; set; }

	void ResetValues();
}
