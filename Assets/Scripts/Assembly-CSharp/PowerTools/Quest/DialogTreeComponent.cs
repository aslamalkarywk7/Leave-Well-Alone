using UnityEngine;

namespace PowerTools.Quest
{
	[SelectionBase]
	public class DialogTreeComponent : MonoBehaviour
	{
		[SerializeField]
		private DialogTree m_data = new DialogTree();

		public DialogTree GetData()
		{
			return m_data;
		}

		public void SetData(DialogTree data)
		{
			m_data = data;
		}
	}
}
