using UnityEngine;

public class ChangeShaderColorComponent : MonoBehaviour
{
	private static MaterialPropertyBlock m_matBlock;

	[SerializeField]
	private Color m_color;

	[SerializeField]
	private ShaderID[] m_shaderIDsToChange;

	private void Awake()
	{
		if (m_matBlock == null)
		{
			m_matBlock = new MaterialPropertyBlock();
		}
		Renderer component = GetComponent<Renderer>();
		if ((bool)component)
		{
			component.GetPropertyBlock(m_matBlock);
			ShaderID[] shaderIDsToChange = m_shaderIDsToChange;
			foreach (ShaderID shaderID in shaderIDsToChange)
			{
				m_matBlock.SetColor(ShaderID_RL.GetShaderID(shaderID), m_color);
			}
			component.SetPropertyBlock(m_matBlock);
		}
	}
}
