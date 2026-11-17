public interface IStateConsumer
{
	StateID InitialState { get; }

	void SetInitialState(StateID state);
}
