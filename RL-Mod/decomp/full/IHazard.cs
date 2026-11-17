using UnityEngine;

public interface IHazard : IRootObj
{
	StateID InitialState { get; }

	Animator Animator { get; }

	void Initialize(HazardArgs hazardArgs);

	void ResetHazard();

	void SetIsCulled(bool enabled);
}
