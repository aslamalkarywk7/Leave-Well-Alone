using UnityEngine;

namespace PowerTools
{
	public class SingletonAuto<T> : MonoBehaviour where T : MonoBehaviour
	{
		protected static T m_instance;

		public static T Get
		{
			get
			{
				if (m_instance == null)
				{
					m_instance = (T)Object.FindObjectOfType(typeof(T));
					if (m_instance == null)
					{
						GameObject gameObject = new GameObject();
						gameObject.name = typeof(T).ToString();
						m_instance = (T)gameObject.AddComponent(typeof(T));
						if (!Application.isPlaying)
						{
							gameObject.hideFlags = HideFlags.HideAndDontSave;
						}
					}
				}
				return m_instance;
			}
		}

		public static T Instance => Get;

		protected void SetSingleton()
		{
			if (m_instance == null)
			{
				m_instance = base.gameObject.GetComponent<T>();
			}
		}

		public static bool GetValid()
		{
			return m_instance != null;
		}

		public static bool HasInstance()
		{
			return m_instance != null;
		}
	}
}
