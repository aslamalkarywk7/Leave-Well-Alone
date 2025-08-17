using UnityEngine;

namespace PowerTools.Quest
{
	public class HotspotComponent : MonoBehaviour
	{
		[SerializeField]
		private Hotspot m_data = new Hotspot();

		public Hotspot GetData()
		{
			return m_data;
		}

		public void SetData(Hotspot data)
		{
			m_data = data;
		}

		public void OnLoadComplete()
		{
		}
	}
}
