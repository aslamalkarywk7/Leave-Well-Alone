using UnityEngine;

namespace PowerTools.Quest
{
	[SelectionBase]
	public class InventoryComponent : MonoBehaviour
	{
		[SerializeField]
		private Inventory m_data = new Inventory();

		public Inventory GetData()
		{
			return m_data;
		}

		public void SetData(Inventory data)
		{
			m_data = data;
		}
	}
}
