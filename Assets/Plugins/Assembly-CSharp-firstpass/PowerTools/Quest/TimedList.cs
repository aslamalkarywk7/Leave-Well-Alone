using System.Collections.Generic;
using UnityEngine;

namespace PowerTools.Quest
{
	public class TimedList<T>
	{
		private Dictionary<T, float> m_list = new Dictionary<T, float>();

		private List<T> m_keys = new List<T>();

		public void Add(T source)
		{
			m_list[source] = 0f;
			m_keys = new List<T>(m_list.Keys);
		}

		public void Add(T source, float time)
		{
			m_list[source] = time;
			m_keys = new List<T>(m_list.Keys);
		}

		public void AddAdditive(T source, float time)
		{
			if (m_list.ContainsKey(source))
			{
				m_list[source] += time;
				return;
			}
			m_list[source] = time;
			m_keys = new List<T>(m_list.Keys);
		}

		public bool Remove(T source)
		{
			if (m_list.Remove(source))
			{
				m_keys = new List<T>(m_list.Keys);
				return true;
			}
			return false;
		}

		public bool Empty()
		{
			return m_list.Count == 0;
		}

		public void Clear()
		{
			m_list.Clear();
			m_keys.Clear();
		}

		public int Count()
		{
			return m_list.Count;
		}

		public bool Contains(T source)
		{
			return m_list.ContainsKey(source);
		}

		public Dictionary<T, float> GetList()
		{
			return m_list;
		}

		public bool Update()
		{
			foreach (T key in m_keys)
			{
				float num = m_list[key];
				if (num > 0f)
				{
					num -= Time.smoothDeltaTime;
					if (num <= 0f)
					{
						m_list.Remove(key);
						m_keys = new List<T>(m_list.Keys);
					}
					else
					{
						m_list[key] = num;
					}
				}
			}
			return m_list.Count > 0;
		}

		public bool UpdateReturnModified()
		{
			bool result = false;
			foreach (T key in m_keys)
			{
				float num = m_list[key];
				if (num > 0f)
				{
					num -= Time.smoothDeltaTime;
					if (num <= 0f)
					{
						result = true;
						m_list.Remove(key);
						m_keys = new List<T>(m_list.Keys);
					}
					else
					{
						m_list[key] = num;
					}
				}
			}
			return result;
		}

		public bool Update(out List<T> removed)
		{
			removed = null;
			foreach (T key in m_keys)
			{
				float num = m_list[key];
				if (!(num > 0f))
				{
					continue;
				}
				num -= Time.smoothDeltaTime;
				if (num <= 0f)
				{
					if (removed == null)
					{
						removed = new List<T>();
					}
					removed.Add(key);
					m_list.Remove(key);
					m_keys = new List<T>(m_list.Keys);
				}
				else
				{
					m_list[key] = num;
				}
			}
			return m_list.Count > 0;
		}
	}
}
