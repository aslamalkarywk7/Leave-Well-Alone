using System.Collections.Generic;
using System.Text;

namespace PowerTools
{
	public class PowerTextNodeLine : IPowerTextNode
	{
		public List<IPowerTextNode> m_strings;

		public float m_weight = 1f;

		public void Build(StringBuilder builder, ref bool plural)
		{
			if (m_strings != null)
			{
				for (int i = 0; i < m_strings.Count; i++)
				{
					m_strings[i].Build(builder, ref plural);
				}
			}
		}

		public void Build(List<string> parts, ref bool plural)
		{
			if (m_strings != null)
			{
				for (int i = 0; i < m_strings.Count; i++)
				{
					m_strings[i].Build(parts, ref plural);
				}
			}
		}

		public void GetParts(List<string> parts)
		{
			if (m_strings != null)
			{
				for (int i = 0; i < m_strings.Count; i++)
				{
					m_strings[i].GetParts(parts);
				}
			}
		}

		public float GetWeight()
		{
			return m_weight;
		}

		public void Append(IPowerTextNode node)
		{
			if (m_strings == null)
			{
				m_strings = new List<IPowerTextNode>();
			}
			m_strings.Add(node);
		}
	}
}
