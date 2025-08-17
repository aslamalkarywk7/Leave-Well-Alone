using UnityEngine;

namespace PowerTools
{
	public class LineIntersector
	{
		private Vector2 m_start1;

		private Vector2 m_end1MinusStart1 = Vector2.zero;

		private float m_end1MinusStart1Mag;

		private float m_resultRatioFromStart;

		public Vector2 GetResult()
		{
			return m_start1 + m_resultRatioFromStart * m_end1MinusStart1;
		}

		public float GetResultDistFromStart()
		{
			return m_resultRatioFromStart * m_end1MinusStart1Mag;
		}

		public void SetFirstLine(Vector2 start, Vector2 end)
		{
			m_start1 = start;
			m_end1MinusStart1 = end - m_start1;
			m_end1MinusStart1Mag = m_end1MinusStart1.magnitude;
		}

		public bool Calculate(Vector2 secondLineStart, Vector2 secondLineEnd)
		{
			Vector2 vector = secondLineEnd - secondLineStart;
			float num = m_end1MinusStart1.x * vector.y - m_end1MinusStart1.y * vector.x;
			if (num == 0f)
			{
				return false;
			}
			num = 1f / num;
			float num2 = ((m_start1.y - secondLineStart.y) * vector.x - (m_start1.x - secondLineStart.x) * vector.y) * num;
			float num3 = ((m_start1.y - secondLineStart.y) * m_end1MinusStart1.x - (m_start1.x - secondLineStart.x) * m_end1MinusStart1.y) * num;
			if (num2 <= 0f || num2 >= 1f || num3 <= 0f || num3 >= 1f)
			{
				return false;
			}
			m_resultRatioFromStart = num2;
			return true;
		}

		public static bool FindIntersection(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2, out Vector2 result)
		{
			Vector2 vector = end1 - start1;
			Vector2 vector2 = end2 - start2;
			Vector2 vector3 = start1 - start2;
			float num = vector.x * vector2.y - vector.y * vector2.x;
			if (num == 0f)
			{
				result = Vector2.zero;
				return false;
			}
			num = 1f / num;
			float num2 = (vector3.y * vector2.x - vector3.x * vector2.y) * num;
			float num3 = (vector3.y * vector.x - vector3.x * vector.y) * num;
			if (num2 <= 0f || num2 >= 1f || num3 <= 0f || num3 >= 1f)
			{
				result = Vector2.zero;
				return false;
			}
			result = start1 + num2 * vector;
			return true;
		}

		public static bool HasIntersection(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2)
		{
			Vector2 vector = end1 - start1;
			Vector2 vector2 = end2 - start2;
			Vector2 vector3 = start1 - start2;
			float num = vector.x * vector2.y - vector.y * vector2.x;
			if (num == 0f)
			{
				return false;
			}
			num = 1f / num;
			float num2 = (vector3.y * vector2.x - vector3.x * vector2.y) * num;
			float num3 = (vector3.y * vector.x - vector3.x * vector.y) * num;
			if (num2 <= 0f || num2 >= 1f || num3 <= 0f || num3 >= 1f)
			{
				return false;
			}
			return true;
		}
	}
}
