using System;
using System.Collections.Generic;
using Cinemachine;
using Cinemachine.Utility;
using UnityEngine;

public class ZoomTest : CinemachineExtension
{
	private class VcamExtraState
	{
		public Vector3 m_previousDisplacement;

		public float confinerDisplacement;
	}

	[SerializeField]
	private float m_maxOrthoSize = 12f;

	[SerializeField]
	private float m_minOrthoSize = 7f;

	[SerializeField]
	private bool ConstrainToBounds;

	[SerializeField]
	public Collider2D m_BoundingShape2D;

	private List<List<Vector2>> m_pathCache;

	public bool m_ConfineScreenEdges = true;

	[Range(0f, 10f)]
	public float m_Damping;

	public bool CameraWasDisplaced(CinemachineVirtualCameraBase vcam)
	{
		return GetExtraState<VcamExtraState>(vcam).confinerDisplacement > 0f;
	}

	private void OnValidate()
	{
		m_Damping = Mathf.Max(0f, m_Damping);
	}

	private bool ValidatePathCache()
	{
		m_pathCache = null;
		Type type = ((m_BoundingShape2D == null) ? null : m_BoundingShape2D.GetType());
		if (type == typeof(PolygonCollider2D))
		{
			PolygonCollider2D polygonCollider2D = m_BoundingShape2D as PolygonCollider2D;
			if (m_pathCache == null || m_pathCache.Count != polygonCollider2D.pathCount)
			{
				m_pathCache = new List<List<Vector2>>();
				for (int i = 0; i < polygonCollider2D.pathCount; i++)
				{
					Vector2[] path = polygonCollider2D.GetPath(i);
					List<Vector2> list = new List<Vector2>();
					for (int j = 0; j < path.Length; j++)
					{
						list.Add(path[j]);
					}
					m_pathCache.Add(list);
				}
			}
			return true;
		}
		if (type == typeof(CompositeCollider2D))
		{
			CompositeCollider2D compositeCollider2D = m_BoundingShape2D as CompositeCollider2D;
			if (m_pathCache == null || m_pathCache.Count != compositeCollider2D.pathCount)
			{
				m_pathCache = new List<List<Vector2>>();
				Vector2[] array = new Vector2[compositeCollider2D.pointCount];
				for (int k = 0; k < compositeCollider2D.pathCount; k++)
				{
					int path2 = compositeCollider2D.GetPath(k, array);
					List<Vector2> list2 = new List<Vector2>();
					for (int l = 0; l < path2; l++)
					{
						list2.Add(array[l]);
					}
					m_pathCache.Add(list2);
				}
			}
			return true;
		}
		return false;
	}

	private Vector3 ConfinePointMod(Vector3 camPos)
	{
		if (m_BoundingShape2D.OverlapPoint(camPos))
		{
			return Vector3.zero;
		}
		if (!ValidatePathCache())
		{
			return camPos;
		}
		Vector2 vector = camPos;
		Vector2 vector2 = vector;
		Vector2 vector3 = vector;
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		for (int i = 0; i < m_pathCache.Count; i++)
		{
			int count = m_pathCache[i].Count;
			if (count <= 0)
			{
				continue;
			}
			Vector2 vector4 = m_BoundingShape2D.transform.TransformPoint(m_pathCache[i][count - 1]);
			for (int j = 0; j < count; j++)
			{
				Vector2 vector5 = m_BoundingShape2D.transform.TransformPoint(m_pathCache[i][j]);
				float num3 = vector.ClosestPointOnSegment(vector4, vector5);
				if ((num3 == 0f || num3 == 1f) && vector.x != vector4.x && vector.x != vector5.x && vector.y != vector4.y && vector.y != vector5.y)
				{
					vector4 = vector5;
					continue;
				}
				Vector2 vector6 = Vector2.Lerp(vector4, vector5, num3);
				float num4 = Vector2.SqrMagnitude(vector - vector6);
				if (num4 < num)
				{
					num2 = num;
					vector3 = vector2;
					num = num4;
					vector2 = vector6;
				}
				else if (num4 < num2)
				{
					num2 = num4;
					vector3 = vector6;
				}
				vector4 = vector5;
			}
		}
		vector2 -= vector;
		vector3 -= vector;
		if (vector3.x != 0f && (Mathf.Abs(vector3.x) < Mathf.Abs(vector2.x) || vector2.x == 0f))
		{
			vector2.x = vector3.x;
		}
		if (vector3.y != 0f && (Mathf.Abs(vector3.y) < Mathf.Abs(vector2.y) || vector2.y == 0f))
		{
			vector2.y = vector3.y;
		}
		return vector2;
	}

	private Vector3 ConfinePoint(Vector3 camPos)
	{
		if (m_BoundingShape2D.OverlapPoint(camPos))
		{
			return Vector3.zero;
		}
		if (!ValidatePathCache())
		{
			return camPos;
		}
		Vector2 vector = camPos;
		Vector2 vector2 = vector;
		float num = float.MaxValue;
		for (int i = 0; i < m_pathCache.Count; i++)
		{
			int count = m_pathCache[i].Count;
			if (count <= 0)
			{
				continue;
			}
			Vector2 vector3 = m_BoundingShape2D.transform.TransformPoint(m_pathCache[i][count - 1]);
			for (int j = 0; j < count; j++)
			{
				Vector2 vector4 = m_BoundingShape2D.transform.TransformPoint(m_pathCache[i][j]);
				Vector2 vector5 = Vector2.Lerp(vector3, vector4, vector.ClosestPointOnSegment(vector3, vector4));
				float num2 = Vector2.SqrMagnitude(vector - vector5);
				if (num2 < num)
				{
					num = num2;
					vector2 = vector5;
				}
				vector3 = vector4;
			}
		}
		return vector2 - vector;
	}

	protected virtual void PostPipelineStageCallback3(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		if (!(m_BoundingShape2D == null))
		{
			Vector3 correctedPosition = state.CorrectedPosition;
			float maxOrthoSize = m_maxOrthoSize;
			float num = maxOrthoSize * state.Lens.Aspect;
			Vector3 vector = Vector3.right * num;
			Vector3 vector2 = Vector3.up * maxOrthoSize;
			Vector3 camPos = correctedPosition - vector + vector2;
			Vector3 camPos2 = correctedPosition - vector - vector2;
			Vector3 camPos3 = correctedPosition + vector + vector2;
			Vector3 camPos4 = correctedPosition + vector - vector2;
			Vector3 vector3 = ConfinePointMod(camPos);
			camPos += vector3;
			Vector3 vector4 = ConfinePointMod(camPos2);
			camPos2 += vector4;
			Vector3 vector5 = ConfinePointMod(camPos3);
			camPos3 += vector5;
			Vector3 vector6 = ConfinePointMod(camPos4);
			camPos4 += vector6;
			Vector2 vector7 = default(Vector2);
			vector7.x = ((camPos.x > camPos2.x) ? camPos.x : camPos2.x);
			vector7.y = ((camPos.y < camPos3.y) ? camPos.y : camPos3.y);
			Vector2 vector8 = default(Vector2);
			vector8.x = vector7.x;
			vector8.y = ((camPos2.y > camPos4.y) ? camPos2.y : camPos4.y);
			Vector2 vector9 = default(Vector2);
			vector9.x = ((camPos3.x < camPos4.x) ? camPos3.x : camPos4.x);
			vector9.y = vector7.y;
			Vector2 vector10 = default(Vector2);
			vector10.x = vector9.x;
			vector10.y = vector8.y;
		}
	}

	protected virtual void PostPipelineStageCallback5(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		if (m_BoundingShape2D == null || stage != 0)
		{
			return;
		}
		Vector3 correctedPosition = state.CorrectedPosition;
		float maxOrthoSize = m_maxOrthoSize;
		float num = maxOrthoSize * state.Lens.Aspect;
		float orthographicSize = state.Lens.OrthographicSize;
		float num2 = orthographicSize * state.Lens.Aspect;
		Vector3 vector = Vector3.right * num;
		Vector3 vector2 = Vector3.up * maxOrthoSize;
		Vector3 vector3 = correctedPosition - vector + vector2;
		Vector3 vector4 = correctedPosition - vector - vector2;
		Vector3 vector5 = correctedPosition + vector + vector2;
		Vector3 vector6 = correctedPosition + vector - vector2;
		Vector3 vector7 = ConfinePoint(vector3);
		Vector3 vector8 = ConfinePoint(vector4);
		Vector3 vector9 = ConfinePoint(vector5);
		Vector3 vector10 = ConfinePoint(vector6);
		Vector3 zero = Vector3.zero;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		bool flag7 = false;
		bool flag8 = false;
		if (!AlmostZero(vector7.x) && AlmostZero(vector9.x))
		{
			Vector2 vector11 = vector5 + Vector3.right * vector7.x;
			vector9 = ConfinePoint(vector11);
			zero.x = vector7.x;
			flag = true;
		}
		else if (AlmostZero(vector7.x) && !AlmostZero(vector9.x))
		{
			Vector2 vector12 = vector3 + Vector3.right * vector9.x;
			vector7 = ConfinePoint(vector12);
			zero.x = vector9.x;
			flag2 = true;
		}
		if (!AlmostZero(vector8.x) && AlmostZero(vector10.x))
		{
			Vector2 vector13 = vector6 + Vector3.right * vector8.x;
			vector10 = ConfinePoint(vector13);
			if (Mathf.Abs(vector8.x) > Mathf.Abs(zero.x))
			{
				zero.x = vector8.x;
			}
			flag3 = true;
		}
		else if (AlmostZero(vector8.x) && !AlmostZero(vector10.x))
		{
			Vector2 vector14 = vector4 + Vector3.right * vector10.x;
			vector8 = ConfinePoint(vector14);
			if (Mathf.Abs(vector10.x) > Mathf.Abs(zero.x))
			{
				zero.x = vector10.x;
			}
			flag4 = true;
		}
		if (!AlmostZero(vector7.y) && AlmostZero(vector8.y))
		{
			Vector2 vector15 = vector4 + Vector3.up * vector7.y;
			vector8 = ConfinePoint(vector15);
			zero.y = vector7.y;
			flag5 = true;
		}
		else if (AlmostZero(vector7.y) && !AlmostZero(vector8.y))
		{
			Vector2 vector16 = vector3 + Vector3.up * vector8.y;
			vector7 = ConfinePoint(vector16);
			zero.y = vector8.y;
			flag7 = true;
		}
		if (!AlmostZero(vector9.y) && AlmostZero(vector10.y))
		{
			Vector2 vector17 = vector6 + Vector3.up * vector9.y;
			vector10 = ConfinePoint(vector17);
			if (Mathf.Abs(vector9.y) > Mathf.Abs(zero.y))
			{
				zero.y = vector9.y;
			}
			flag6 = true;
		}
		else if (AlmostZero(vector9.y) && !AlmostZero(vector10.y))
		{
			Vector2 vector18 = vector5 + Vector3.up * vector10.y;
			vector9 = ConfinePoint(vector18);
			if (Mathf.Abs(vector10.y) > Mathf.Abs(zero.y))
			{
				zero.y = vector10.y;
			}
			flag8 = true;
		}
		if (flag)
		{
			vector7.x = 0f;
		}
		if (flag5)
		{
			vector7.y = 0f;
		}
		if (flag2)
		{
			vector9.x = 0f;
		}
		if (flag6)
		{
			vector9.y = 0f;
		}
		if (flag3)
		{
			vector8.x = 0f;
		}
		if (flag7)
		{
			vector8.y = 0f;
		}
		if (flag4)
		{
			vector10.x = 0f;
		}
		if (flag8)
		{
			vector10.y = 0f;
		}
		int num3;
		if (vector8.x == 0f || vector10.x == 0f)
		{
			if (vector7.x != 0f)
			{
				num3 = ((vector9.x != 0f) ? 1 : 0);
				if (num3 != 0)
				{
					goto IL_0512;
				}
			}
			else
			{
				num3 = 0;
			}
			goto IL_051e;
		}
		num3 = 1;
		goto IL_0512;
		IL_0562:
		zero.y = 0f;
		goto IL_056e;
		IL_056e:
		float num4 = vector5.x + vector9.x - (vector3.x + vector7.x);
		float num5 = vector6.x + vector10.x - (vector4.x + vector8.x);
		float num6 = ((num4 < num5) ? num4 : num5);
		float num7 = vector3.y + vector7.y - (vector4.y + vector8.y);
		float num8 = vector5.y + vector9.y - (vector6.y + vector10.y);
		float num9 = ((num7 < num8) ? num7 : num8);
		float num10 = maxOrthoSize / orthographicSize;
		float num11 = num9 / (orthographicSize * 2f);
		float num12 = num6 / (num2 * 2f);
		if (num11 < num10)
		{
			num10 = num11;
		}
		if (num12 < num10)
		{
			num10 = num12;
		}
		LensSettings lens = state.Lens;
		lens.OrthographicSize *= num10;
		Debug.Log("percent is: " + num10 + " new orthosize: " + lens.OrthographicSize);
		state.Lens = lens;
		float num13 = maxOrthoSize - lens.OrthographicSize;
		float num14 = num - lens.OrthographicSize * state.Lens.Aspect;
		int num15;
		if (num15 != 0)
		{
			float num16 = ((vector7.y < vector9.y) ? vector7.y : vector9.y);
			float num17 = ((vector8.y > vector10.y) ? vector8.y : vector10.y);
			float y = ((Mathf.Abs(num16) > Mathf.Abs(num17)) ? num16 : num17);
			zero.y = y;
		}
		if (Mathf.Abs(zero.y) > num13)
		{
			if (zero.y < 0f)
			{
				zero.y += num13;
			}
			else if (zero.y > 0f)
			{
				zero.y -= num13;
			}
		}
		else
		{
			zero.y = 0f;
		}
		if (num3 != 0)
		{
			float num18 = ((vector7.x > vector8.x) ? vector7.x : vector8.x);
			float num19 = ((vector9.x < vector10.x) ? vector9.x : vector10.x);
			float num20 = ((Mathf.Abs(num18) > Mathf.Abs(num19)) ? num18 : num19);
			zero.x += num20;
		}
		if (Mathf.Abs(zero.x) > num14)
		{
			if (zero.x < 0f)
			{
				zero.x += num14;
			}
			else if (zero.x > 0f)
			{
				zero.x -= num14;
			}
		}
		else
		{
			zero.x = 0f;
		}
		state.PositionCorrection += zero;
		return;
		IL_051e:
		if (vector8.y == 0f || vector7.y == 0f)
		{
			if (vector10.y != 0f)
			{
				num15 = ((vector9.y != 0f) ? 1 : 0);
				if (num15 != 0)
				{
					goto IL_0562;
				}
			}
			else
			{
				num15 = 0;
			}
			goto IL_056e;
		}
		num15 = 1;
		goto IL_0562;
		IL_0512:
		zero.x = 0f;
		goto IL_051e;
	}

	protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		if (!(m_BoundingShape2D == null) && stage == CinemachineCore.Stage.Body)
		{
			Vector3 correctedPosition = state.CorrectedPosition;
			float maxOrthoSize = m_maxOrthoSize;
			float num = maxOrthoSize * state.Lens.Aspect;
			float orthographicSize = state.Lens.OrthographicSize;
			float num2 = orthographicSize * state.Lens.Aspect;
			Vector3 vector = Vector3.right * num;
			Vector3 vector2 = Vector3.up * maxOrthoSize;
			Vector3 camPos = correctedPosition - vector + vector2;
			Vector3 camPos2 = correctedPosition - vector - vector2;
			Vector3 camPos3 = correctedPosition + vector + vector2;
			Vector3 camPos4 = correctedPosition + vector - vector2;
			Vector3 vector3 = ConfinePoint(camPos);
			Vector3 vector4 = ConfinePoint(camPos2);
			Vector3 vector5 = ConfinePoint(camPos3);
			Vector3 vector6 = ConfinePoint(camPos4);
			if (!AlmostZero(vector3.x) && AlmostZero(vector5.x))
			{
				camPos3 += Vector3.right * vector3.x;
				camPos += Vector3.right * vector3.x;
				vector5 = ConfinePoint(camPos3);
				vector3 = ConfinePoint(camPos);
			}
			else if (AlmostZero(vector3.x) && !AlmostZero(vector5.x))
			{
				camPos3 += Vector3.right * vector5.x;
				camPos += Vector3.right * vector5.x;
				vector5 = ConfinePoint(camPos3);
				vector3 = ConfinePoint(camPos);
			}
			if (!AlmostZero(vector4.x) && AlmostZero(vector6.x))
			{
				camPos4 += Vector3.right * vector4.x;
				camPos2 += Vector3.right * vector4.x;
				vector6 = ConfinePoint(camPos4);
				vector4 = ConfinePoint(camPos2);
			}
			else if (AlmostZero(vector4.x) && !AlmostZero(vector6.x))
			{
				camPos4 += Vector3.right * vector6.x;
				camPos2 += Vector3.right * vector6.x;
				vector6 = ConfinePoint(camPos4);
				vector4 = ConfinePoint(camPos2);
			}
			if (!AlmostZero(vector3.y) && AlmostZero(vector4.y))
			{
				camPos2 += Vector3.up * vector3.y;
				camPos += Vector3.up * vector3.y;
				vector4 = ConfinePoint(camPos2);
				vector3 = ConfinePoint(camPos);
			}
			else if (AlmostZero(vector3.y) && !AlmostZero(vector4.y))
			{
				camPos2 += Vector3.up * vector4.y;
				camPos += Vector3.up * vector4.y;
				vector4 = ConfinePoint(camPos2);
				vector3 = ConfinePoint(camPos);
			}
			if (!AlmostZero(vector5.y) && AlmostZero(vector6.y))
			{
				camPos4 += Vector3.up * vector5.y;
				camPos3 += Vector3.up * vector5.y;
				vector6 = ConfinePoint(camPos4);
				vector5 = ConfinePoint(camPos3);
			}
			else if (AlmostZero(vector5.y) && !AlmostZero(vector6.y))
			{
				camPos4 += Vector3.up * vector6.y;
				camPos3 += Vector3.up * vector6.y;
				vector6 = ConfinePoint(camPos4);
				vector5 = ConfinePoint(camPos3);
			}
			float num3 = camPos3.x + vector5.x - (camPos.x + vector3.x);
			float num4 = camPos4.x + vector6.x - (camPos2.x + vector4.x);
			float num5 = ((num3 < num4) ? num3 : num4);
			float num6 = camPos.y + vector3.y - (camPos2.y + vector4.y);
			float num7 = camPos3.y + vector5.y - (camPos4.y + vector6.y);
			float num8 = ((num6 < num7) ? num6 : num7);
			float num9 = maxOrthoSize / orthographicSize;
			float num10 = num8 / (orthographicSize * 2f);
			float num11 = num5 / (num2 * 2f);
			if (num10 < num9)
			{
				num9 = num10;
			}
			if (num11 < num9)
			{
				num9 = num11;
			}
			LensSettings lens = state.Lens;
			lens.OrthographicSize *= num9;
			state.Lens = lens;
			orthographicSize = lens.OrthographicSize;
			num2 = orthographicSize * state.Lens.Aspect;
			vector = Vector3.right * num2;
			vector2 = Vector3.up * orthographicSize;
			camPos = correctedPosition - vector + vector2;
			camPos2 = correctedPosition - vector - vector2;
			camPos3 = correctedPosition + vector + vector2;
			camPos4 = correctedPosition + vector - vector2;
			vector3 = ConfinePoint(camPos);
			vector4 = ConfinePoint(camPos2);
			vector5 = ConfinePoint(camPos3);
			vector6 = ConfinePoint(camPos4);
			float num12 = ((vector3.x > vector4.x) ? vector3.x : vector4.x);
			float num13 = ((vector5.x < vector6.x) ? vector5.x : vector6.x);
			float x = ((Mathf.Abs(num12) > Mathf.Abs(num13)) ? num12 : num13);
			float num14 = ((vector3.y < vector5.y) ? vector3.y : vector5.y);
			float num15 = ((vector4.y > vector6.y) ? vector4.y : vector6.y);
			float y = ((Mathf.Abs(num14) > Mathf.Abs(num15)) ? num14 : num15);
			state.PositionCorrection = new Vector3(x, y);
		}
	}

	private bool AlmostZero(float value)
	{
		if (value < 0.01f)
		{
			return value > -0.01f;
		}
		return false;
	}

	protected void PostPipelineStageCallbackOld(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		if (!(m_BoundingShape2D == null) && stage == CinemachineCore.Stage.Body)
		{
			Vector3 vector = ((!m_ConfineScreenEdges || !state.Lens.Orthographic) ? ConfinePoint(state.CorrectedPosition) : ConfineScreenEdges(vcam, ref state));
			VcamExtraState extraState = GetExtraState<VcamExtraState>(vcam);
			if (m_Damping > 0f && deltaTime >= 0f)
			{
				Vector3 initial = vector - extraState.m_previousDisplacement;
				initial = Damper.Damp(initial, m_Damping, deltaTime);
				vector = extraState.m_previousDisplacement + initial;
			}
			extraState.m_previousDisplacement = vector;
			state.PositionCorrection += vector;
			extraState.confinerDisplacement = vector.magnitude;
			Vector3 correctedPosition = state.CorrectedPosition;
			float orthographicSize = state.Lens.OrthographicSize;
			float num = orthographicSize * state.Lens.Aspect;
			float maxOrthoSize = m_maxOrthoSize;
			float minOrthoSize = m_minOrthoSize;
			float f = maxOrthoSize * state.Lens.Aspect;
			f = Mathf.RoundToInt(f);
			Vector3 vector2 = Vector3.right * f;
			Vector3 vector3 = Vector3.up * maxOrthoSize;
			Vector3 camPos = correctedPosition - vector2 - vector3;
			Vector3 camPos2 = correctedPosition - vector2 + vector3;
			Vector3 camPos3 = correctedPosition + vector2 - vector3;
			Vector3 camPos4 = correctedPosition + vector2 + vector3;
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			Vector3 zero3 = Vector3.zero;
			Vector3 zero4 = Vector3.zero;
			zero = ConfinePoint(camPos);
			zero2 = ConfinePoint(camPos3);
			zero3 = ConfinePoint(camPos2);
			zero4 = ConfinePoint(camPos4);
			bool num2 = (zero.x != 0f && zero2.x != 0f) || (zero3.x != 0f && zero4.x != 0f);
			bool num3 = (zero.y != 0f && zero3.y != 0f) || (zero2.y != 0f && zero4.y != 0f);
			camPos += zero;
			camPos3 += zero2;
			camPos2 += zero3;
			camPos4 += zero4;
			float num4 = camPos3.x - camPos.x;
			float num5 = camPos4.x - camPos2.x;
			float num6 = ((num4 < num5) ? num4 : num5) / (num * 2f);
			float num7 = camPos2.y - camPos.y;
			float num8 = camPos4.y - camPos3.y;
			float num9 = ((num7 < num8) ? num7 : num8) / (orthographicSize * 2f);
			float num10 = maxOrthoSize / orthographicSize;
			if (num3 && num9 < num10)
			{
				num10 = num9;
			}
			if (num2 && num6 < num10)
			{
				num10 = num6;
			}
			float num11 = minOrthoSize / orthographicSize;
			if (num10 < num11)
			{
				num10 = num11;
			}
			LensSettings lens = state.Lens;
			lens.OrthographicSize *= num10;
			Debug.Log("percent is: " + num10 + " new orthosize: " + lens.OrthographicSize);
			state.Lens = lens;
		}
	}

	private Vector3 ConfineScreenEdges(CinemachineVirtualCameraBase vcam, ref CameraState state)
	{
		Quaternion quaternion = Quaternion.Inverse(state.CorrectedOrientation);
		float orthographicSize = state.Lens.OrthographicSize;
		float f = orthographicSize * state.Lens.Aspect;
		f = Mathf.RoundToInt(f);
		Vector3 vector = quaternion * Vector3.right * f;
		Vector3 vector2 = quaternion * Vector3.up * orthographicSize;
		Vector3 zero = Vector3.zero;
		Vector3 correctedPosition = state.CorrectedPosition;
		for (int i = 0; i < 12; i++)
		{
			Vector3 vector3 = ConfinePoint(correctedPosition - vector2 - vector);
			if (vector3.AlmostZero())
			{
				vector3 = ConfinePoint(correctedPosition - vector2 + vector);
			}
			if (vector3.AlmostZero())
			{
				vector3 = ConfinePoint(correctedPosition + vector2 - vector);
			}
			if (vector3.AlmostZero())
			{
				vector3 = ConfinePoint(correctedPosition + vector2 + vector);
			}
			if (vector3.AlmostZero())
			{
				break;
			}
			zero += vector3;
			correctedPosition += vector3;
		}
		return zero;
	}
}
