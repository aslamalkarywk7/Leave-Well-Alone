using System;
using System.Collections.Generic;

namespace PowerTools.Quest
{
	public class ShuffledIndex
	{
		private static Dictionary<int, ShuffledIndex> s_premadeShuffledIndexes = new Dictionary<int, ShuffledIndex>();

		private int m_current = -2;

		private int[] m_ids;

		public int Length => m_ids.Length;

		public int Count => m_ids.Length;

		public ShuffledIndex(int count)
		{
			m_ids = new int[count];
			for (int i = 0; i < count; i++)
			{
				m_ids[i] = i;
			}
		}

		public static int Random(int max)
		{
			int num = max + 1;
			ShuffledIndex value = null;
			if (!s_premadeShuffledIndexes.TryGetValue(num, out value))
			{
				value = new ShuffledIndex(num);
				s_premadeShuffledIndexes.Add(num, value);
			}
			return value.Next();
		}

		public int Next()
		{
			m_current++;
			return this;
		}

		public static implicit operator int(ShuffledIndex m)
		{
			if (m.m_ids.Length == 0)
			{
				return -1;
			}
			if (m.m_current < 0 || m.m_current >= m.m_ids.Length)
			{
				int num = ((m.m_current < 0) ? (-1) : m.m_ids[m.m_ids.Length - 1]);
				m.m_ids.Shuffle();
				m.m_current = 0;
				if (m.m_ids.Length > 1 && num == m.m_ids[0])
				{
					Utils.Swap(ref m.m_ids[0], ref m.m_ids[1]);
				}
			}
			return m.m_ids[m.m_current];
		}

		public void SetCurrent(int id)
		{
			if (m_ids == null || m_ids.Length == 0 || id < 0 || id >= m_ids.Length)
			{
				return;
			}
			if (m_current < 0)
			{
				Next();
			}
			int num = Array.FindIndex(m_ids, (int item) => item == id);
			if (num == m_current)
			{
				return;
			}
			if (num < m_current)
			{
				Utils.Swap(ref m_ids[num], ref m_ids[m_current]);
			}
			else if (num > m_current)
			{
				Next();
				if (m_current != num)
				{
					Utils.Swap(ref m_ids[num], ref m_ids[m_current]);
				}
			}
		}

		public static ShuffledIndex operator ++(ShuffledIndex m)
		{
			m.Next();
			return m;
		}
	}
}
