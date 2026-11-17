public class RNGData
{
	private static int m_order;

	public string CallerDescription { get; private set; }

	public float Min { get; }

	public float Max { get; }

	public float Number { get; private set; }

	public int Order { get; private set; }

	public RNGData(string callerDescription, float min, float max, float number)
	{
		m_order++;
		Order = m_order;
		CallerDescription = callerDescription;
		Min = min;
		Max = max;
		Number = number;
	}
}
