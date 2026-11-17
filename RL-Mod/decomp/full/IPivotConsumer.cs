public interface IPivotConsumer
{
	PivotPoint PivotPoint { get; }

	void SetPivot(PivotPoint pivotPoint);
}
