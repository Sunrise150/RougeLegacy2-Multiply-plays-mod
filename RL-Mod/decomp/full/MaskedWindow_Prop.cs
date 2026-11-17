using UnityEngine;
using UnityEngine.Serialization;

public class MaskedWindow_Prop : Prop
{
	[Tooltip("Calculated by: (sprite tiled size in units - stencil mask scale in units")]
	[SerializeField]
	private Vector2 m_referenceWindowBorderSize;

	[SerializeField]
	[FormerlySerializedAs("m_spriteMaskObject")]
	private GameObject m_maskObject;

	protected override void ResizeSlicedOrTiledSpriteRenderer(SpriteRenderer spriteRenderer, Vector2 newSize)
	{
		float x = newSize.x - m_referenceWindowBorderSize.x;
		float y = newSize.y - m_referenceWindowBorderSize.y;
		m_maskObject.transform.localScale = new Vector3(x, y, m_maskObject.transform.localScale.z);
		Vector3 localPosition = m_maskObject.transform.localPosition;
		localPosition.y = newSize.y / 2f;
		m_maskObject.transform.localPosition = localPosition;
		base.ResizeSlicedOrTiledSpriteRenderer(spriteRenderer, newSize);
	}
}
