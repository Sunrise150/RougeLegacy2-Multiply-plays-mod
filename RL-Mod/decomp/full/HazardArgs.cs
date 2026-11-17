public class HazardArgs
{
	public StateID InitialState { get; protected set; }

	public HazardArgs(StateID initialState)
	{
		InitialState = initialState;
	}
}
