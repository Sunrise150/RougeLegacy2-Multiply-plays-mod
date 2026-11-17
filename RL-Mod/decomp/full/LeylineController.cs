using Ferr;
using UnityEngine;

[RequireComponent(typeof(Ferr2DT_PathTerrain))]
public class LeylineController : MonoBehaviour
{
	[SerializeField]
	private bool m_useCircleCorners;

	public bool UseCircleCorners => m_useCircleCorners;

	private void Start()
	{
		Ferr2DT_PathTerrain component = GetComponent<Ferr2DT_PathTerrain>();
		if (component != null)
		{
			foreach (PointControl control in component.PathData.GetControls())
			{
				if (m_useCircleCorners)
				{
					control.type = PointType.CircleCorner;
				}
				else
				{
					control.type = PointType.Sharp;
				}
			}
			return;
		}
		Debug.LogFormat("<color=red>| {0} | No Ferr2D Component found.</color>", this);
	}
}
