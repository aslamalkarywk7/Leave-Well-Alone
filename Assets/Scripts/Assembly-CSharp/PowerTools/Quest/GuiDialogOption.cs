using UnityEngine;

namespace PowerTools.Quest
{
	public class GuiDialogOption : MonoBehaviour
	{
		public IQuestClickable Clickable { get; set; }

		public DialogOption Option { get; set; }
	}
}
