using UnityEngine;

public class ChangeEffectRendererColor : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer[] m_renderersToChange;

	public void ChangeColor(Color color)
	{
		SpriteRenderer[] renderersToChange = m_renderersToChange;
		for (int i = 0; i < renderersToChange.Length; i++)
		{
			renderersToChange[i].color = color;
		}
	}
}
