using System;
using UnityEngine;

namespace PowerTools.Quest
{
	[Serializable]
	public class MinMaxRange
	{
		public float m_min;

		public float m_max;

		public bool m_hasMax;

		public bool m_hasValue;

		private float m_value;

		public float Min => m_min;

		public float Max
		{
			get
			{
				if (!m_hasMax)
				{
					return m_min;
				}
				return m_max;
			}
		}

		public float Value => this;

		public MinMaxRange()
		{
		}

		public MinMaxRange(float val)
		{
			m_min = val;
			m_max = val;
			m_value = val;
			m_hasMax = false;
		}

		public MinMaxRange(float min, float max)
		{
			m_min = min;
			m_max = max;
			m_value = min;
			m_hasMax = true;
		}

		public float Lerp(float ratio)
		{
			if (!m_hasMax)
			{
				return m_min;
			}
			return Mathf.Lerp(m_min, m_max, ratio);
		}

		public static implicit operator float(MinMaxRange m)
		{
			if (!m.m_hasValue)
			{
				if (m.m_hasMax)
				{
					m.m_value = UnityEngine.Random.Range(m.m_min, m.m_max);
					m.m_hasValue = true;
					return m.m_value;
				}
				m.m_value = m.m_min;
				m.m_hasValue = true;
			}
			return m.m_value;
		}

		public static implicit operator int(MinMaxRange m)
		{
			return Mathf.RoundToInt(m);
		}

		public void Randomise()
		{
			if (m_hasMax)
			{
				m_value = UnityEngine.Random.Range(m_min, m_max);
				m_hasValue = true;
			}
			else
			{
				m_value = m_min;
				m_hasValue = true;
			}
		}

		public float GetRandom()
		{
			if (m_hasMax)
			{
				return UnityEngine.Random.Range(m_min, m_max);
			}
			return m_min;
		}

		public bool IsZero()
		{
			if (m_min == 0f)
			{
				return !m_hasMax;
			}
			return false;
		}
	}
}
