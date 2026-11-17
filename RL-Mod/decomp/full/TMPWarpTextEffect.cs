using TMPro;
using UnityEngine;

[ExecuteAlways]
public class TMPWarpTextEffect : MonoBehaviour
{
	private TMP_Text m_text;

	public AnimationCurve VertexCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.25f, 2f), new Keyframe(0.5f, 0f), new Keyframe(0.75f, 2f), new Keyframe(1f, 0f));

	public float AngleMultiplier = 1f;

	public float SpeedMultiplier = 1f;

	public float CurveScale = 1f;

	private bool m_hasTextChanged;

	private void OnEnable()
	{
		TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
	}

	private void OnDisable()
	{
		TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
	}

	private void OnTextChanged(Object obj)
	{
		if (obj == m_text)
		{
			m_hasTextChanged = true;
		}
	}

	private void Awake()
	{
		m_text = base.gameObject.GetComponent<TMP_Text>();
		VertexCurve.preWrapMode = WrapMode.Once;
		VertexCurve.postWrapMode = WrapMode.Once;
	}

	private void Start()
	{
		WarpText();
	}

	private void Update()
	{
		if (!Application.isPlaying || m_hasTextChanged)
		{
			WarpText();
		}
	}

	private AnimationCurve CopyAnimationCurve(AnimationCurve curve)
	{
		return new AnimationCurve
		{
			keys = curve.keys
		};
	}

	private void WarpText()
	{
		float curveScale = CurveScale;
		AnimationCurve vertexCurve = CopyAnimationCurve(VertexCurve);
		m_text.havePropertiesChanged = true;
		CurveScale *= 10f;
		m_text.ForceMeshUpdate();
		TMP_TextInfo textInfo = m_text.textInfo;
		int characterCount = textInfo.characterCount;
		if (characterCount == 0)
		{
			return;
		}
		float x = m_text.bounds.min.x;
		float x2 = m_text.bounds.max.x;
		for (int i = 0; i < characterCount; i++)
		{
			if (textInfo.characterInfo[i].isVisible)
			{
				int vertexIndex = textInfo.characterInfo[i].vertexIndex;
				int materialReferenceIndex = textInfo.characterInfo[i].materialReferenceIndex;
				Vector3[] vertices = textInfo.meshInfo[materialReferenceIndex].vertices;
				Vector3 vector = new Vector2((vertices[vertexIndex].x + vertices[vertexIndex + 2].x) / 2f, textInfo.characterInfo[i].baseLine);
				vertices[vertexIndex] += -vector;
				vertices[vertexIndex + 1] += -vector;
				vertices[vertexIndex + 2] += -vector;
				vertices[vertexIndex + 3] += -vector;
				float num = (vector.x - x) / (x2 - x);
				float num2 = num + 0.0001f;
				float y = VertexCurve.Evaluate(num) * CurveScale;
				float y2 = VertexCurve.Evaluate(num2) * CurveScale;
				Vector3 lhs = new Vector3(1f, 0f, 0f);
				Vector3 rhs = new Vector3(num2 * (x2 - x) + x, y2) - new Vector3(vector.x, y);
				float num3 = Mathf.Acos(Vector3.Dot(lhs, rhs.normalized)) * 57.29578f;
				float z = ((Vector3.Cross(lhs, rhs).z > 0f) ? num3 : (360f - num3));
				Matrix4x4 matrix4x = Matrix4x4.TRS(new Vector3(0f, y, 0f), Quaternion.Euler(0f, 0f, z), Vector3.one);
				vertices[vertexIndex] = matrix4x.MultiplyPoint3x4(vertices[vertexIndex]);
				vertices[vertexIndex + 1] = matrix4x.MultiplyPoint3x4(vertices[vertexIndex + 1]);
				vertices[vertexIndex + 2] = matrix4x.MultiplyPoint3x4(vertices[vertexIndex + 2]);
				vertices[vertexIndex + 3] = matrix4x.MultiplyPoint3x4(vertices[vertexIndex + 3]);
				vertices[vertexIndex] += vector;
				vertices[vertexIndex + 1] += vector;
				vertices[vertexIndex + 2] += vector;
				vertices[vertexIndex + 3] += vector;
			}
		}
		m_text.UpdateVertexData();
		VertexCurve = vertexCurve;
		CurveScale = curveScale;
		m_hasTextChanged = false;
	}
}
