using System;
using System.Collections.Generic;
using UnityEngine;

namespace PowerTools.Quest
{
	public class WeightedShuffledIndex
	{
		public delegate float DelegateGetWeight<T>(T item);

		public float m_totalWeight;

		private float m_maxWeightInv;

		private ShuffledIndex m_shuffledIndex;

		private float[] m_weights;

		public int Length
		{
			get
			{
				if (m_weights == null)
				{
					return 0;
				}
				return m_weights.Length;
			}
		}

		public int Count
		{
			get
			{
				if (m_weights == null)
				{
					return 0;
				}
				return m_weights.Length;
			}
		}

		public bool GetInitialised<T>(List<T> list)
		{
			if (m_weights != null && m_shuffledIndex != null)
			{
				return m_shuffledIndex.Count == list.Count;
			}
			return false;
		}

		public float GetRatio(int item)
		{
			if (m_totalWeight <= 0f)
			{
				return 0f;
			}
			if (item < 0 || item >= m_weights.Length)
			{
				return 0f;
			}
			return m_weights[item] / m_totalWeight;
		}

		public float GetTotalWeight()
		{
			return m_totalWeight;
		}

		public float GetMaxWeight()
		{
			if (!(m_maxWeightInv <= 0f))
			{
				return 1f / m_maxWeightInv;
			}
			return 1f;
		}

		public static T Select<T>(T[] list, DelegateGetWeight<T> getWeightFunc) where T : class
		{
			if (list == null || list.Length == 0)
			{
				return null;
			}
			float maxWeight = 0f;
			Array.ForEach(list, delegate(T item)
			{
				maxWeight = Mathf.Max(maxWeight, getWeightFunc(item));
			});
			int num;
			float num2;
			do
			{
				num = ShuffledIndex.Random(list.Length - 1);
				num2 = getWeightFunc(list[num]);
			}
			while (!(num2 > 0f) || !(UnityEngine.Random.value <= num2 / maxWeight));
			return list[num];
		}

		public void SetWeights<T>(List<T> list, DelegateGetWeight<T> getWeightFunc)
		{
			if (list != null && list.Count != 0)
			{
				m_shuffledIndex = new ShuffledIndex(list.Count);
				m_weights = new float[list.Count];
				for (int i = 0; i < list.Count; i++)
				{
					m_weights[i] = getWeightFunc(list[i]);
				}
				UpdateWeights();
			}
		}

		public void Init(int size)
		{
			if (size != 0)
			{
				m_shuffledIndex = new ShuffledIndex(size);
				m_weights = new float[size];
			}
		}

		public void SetWeight(int index, float weight)
		{
			weight = Mathf.Max(0f, weight);
			if (m_weights != null && m_shuffledIndex != null && m_shuffledIndex.Count != m_weights.Length)
			{
				Debug.LogError("Call Init before SetWeight");
				return;
			}
			m_weights[index] = weight;
			UpdateWeights();
		}

		public int Next()
		{
			m_shuffledIndex.Next();
			return this;
		}

		public static implicit operator int(WeightedShuffledIndex m)
		{
			if (m.m_maxWeightInv <= 0f)
			{
				m.UpdateWeights();
			}
			if (m.m_maxWeightInv <= 0f)
			{
				return -1;
			}
			if (m.m_shuffledIndex == null || m.m_shuffledIndex.Count != m.m_weights.Length)
			{
				m.m_shuffledIndex = new ShuffledIndex(m.m_weights.Length);
			}
			int num = -1;
			while (num == -1)
			{
				int num2 = m.m_shuffledIndex.Next();
				if (m.m_weights[num2] > 0f && UnityEngine.Random.value <= m.m_weights[num2] * m.m_maxWeightInv)
				{
					num = num2;
				}
			}
			return num;
		}

		private void UpdateWeights()
		{
			m_totalWeight = 0f;
			m_maxWeightInv = 0f;
			float num = 0f;
			float[] weights = m_weights;
			foreach (float num2 in weights)
			{
				m_totalWeight += num2;
				if (num2 > num)
				{
					num = num2;
				}
			}
			if (num > 0f)
			{
				m_maxWeightInv = 1f / num;
			}
			if (m_totalWeight <= 0f)
			{
				m_totalWeight = 1f;
			}
		}
	}
}
