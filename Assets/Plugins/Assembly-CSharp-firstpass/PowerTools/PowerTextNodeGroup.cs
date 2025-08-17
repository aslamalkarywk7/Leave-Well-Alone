using System.Collections.Generic;
using System.Text;
using PowerTools.PowerTextUtils;
using UnityEngine;

namespace PowerTools
{
	public class PowerTextNodeGroup : IPowerTextNode
	{
		public List<IPowerTextNode> m_options;

		private ShuffledIndex m_shuffledIndex;

		private float m_maxWeight = 1f;

		public void Build(StringBuilder builder, ref bool plural)
		{
			if (m_options == null || m_options.Count <= 0)
			{
				return;
			}
			if (m_shuffledIndex == null)
			{
				m_shuffledIndex = new ShuffledIndex(m_options.Count);
			}
			for (int i = 0; i <= m_shuffledIndex.Length * 2; i++)
			{
				m_shuffledIndex.Next();
				float weight = m_options[m_shuffledIndex].GetWeight();
				if (weight >= m_maxWeight || (weight > 0f && weight >= Random.value * m_maxWeight))
				{
					m_options[m_shuffledIndex].Build(builder, ref plural);
					break;
				}
			}
		}

		public void Build(List<string> parts, ref bool plural)
		{
			if (m_options == null || m_options.Count <= 0)
			{
				return;
			}
			if (m_shuffledIndex == null)
			{
				m_shuffledIndex = new ShuffledIndex(m_options.Count);
			}
			for (int i = 0; i <= m_shuffledIndex.Length * 2; i++)
			{
				m_shuffledIndex.Next();
				float weight = m_options[m_shuffledIndex].GetWeight();
				if (weight >= m_maxWeight || (weight > 0f && weight >= Random.value * m_maxWeight))
				{
					m_options[m_shuffledIndex].Build(parts, ref plural);
					break;
				}
			}
		}

		public void GetParts(List<string> parts)
		{
			if (m_options != null && m_options.Count > 0)
			{
				for (int i = 0; i < m_options.Count; i++)
				{
					m_options[i].GetParts(parts);
				}
			}
		}

		public float GetWeight()
		{
			return 1f;
		}

		public void AddOption(IPowerTextNode node)
		{
			if (m_options == null)
			{
				m_options = new List<IPowerTextNode>();
			}
			m_options.Add(node);
			m_maxWeight = Mathf.Max(m_maxWeight, node.GetWeight());
		}
	}
}
