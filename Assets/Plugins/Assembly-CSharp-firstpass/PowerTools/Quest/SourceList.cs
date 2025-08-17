using System;
using System.Collections.Generic;

namespace PowerTools.Quest
{
	public class SourceList
	{
		private Dictionary<string, int> m_list = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

		public int Add(string source)
		{
			int num = ((!m_list.TryGetValue(source, out num)) ? 1 : (num + 1));
			m_list[source] = num;
			return num;
		}

		public void Remove(string source)
		{
			if (m_list.TryGetValue(source, out var value))
			{
				value--;
				if (value == 0)
				{
					m_list.Remove(source);
				}
				else
				{
					m_list[source] = value;
				}
			}
		}

		public bool Empty()
		{
			return m_list.Count == 0;
		}

		public void Clear()
		{
			m_list.Clear();
		}

		public int Count()
		{
			return m_list.Count;
		}

		public int CountAll()
		{
			int num = 0;
			foreach (int value in m_list.Values)
			{
				num += value;
			}
			return num;
		}

		public int Count(string source)
		{
			int value = 0;
			m_list.TryGetValue(source, out value);
			return value;
		}

		public bool Contains(string source)
		{
			return m_list.ContainsKey(source);
		}
	}
}
