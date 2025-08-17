using UnityEngine;

namespace PowerTools.Quest
{
	public interface IInventoryPanel : IGuiControl
	{
		ICharacter TargetCharacter { get; set; }

		Vector2 ScrollOffset { get; set; }

		bool ScrollForward();

		bool ScrollBack();

		void NextRow();

		void NextColumn();

		void PrevRow();

		void PrevColumn();

		bool HasNextColumn();

		bool HasPrevColumn();

		bool HasNextRow();

		bool HasPrevRow();
	}
}
