using System.Collections;
using UnityEngine;

public class CDGSpriteAnimator : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer m_renderer;

	[SerializeField]
	private Sprite[] m_sprites;

	[SerializeField]
	private int m_introFrames;

	[SerializeField]
	private int m_fps;

	[SerializeField]
	private bool m_loop;

	[SerializeField]
	private float m_loopingDelay;

	private float m_frameCounter;

	private int m_lastSpriteIndex;

	private void OnEnable()
	{
		m_frameCounter = 0f;
		m_lastSpriteIndex = -1;
		StartCoroutine(Animate());
	}

	private IEnumerator Animate()
	{
		while (true)
		{
			int num = (int)m_frameCounter;
			if (num >= m_sprites.Length)
			{
				if (!m_loop)
				{
					break;
				}
				if (m_loopingDelay > 0f)
				{
					float delayTime = Time.time + m_loopingDelay;
					while (Time.time < delayTime)
					{
						yield return null;
					}
				}
				m_frameCounter = (float)m_introFrames + (m_frameCounter - (float)m_introFrames) % (float)(m_sprites.Length - m_introFrames);
				num = (int)m_frameCounter;
			}
			num = Mathf.Clamp(num, 0, m_sprites.Length - 1);
			if (num != m_lastSpriteIndex)
			{
				m_renderer.sprite = m_sprites[num];
			}
			m_lastSpriteIndex = num;
			m_frameCounter += Time.deltaTime * (float)m_fps;
			yield return null;
		}
	}
}
