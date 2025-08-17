using UnityEngine;

namespace PowerTools.Quest
{
	public interface ICursor
	{
		bool Visible { get; set; }

		string AnimationOverride { get; set; }

		string AnimationClickable { get; set; }

		string AnimationNonClickable { get; set; }

		string AnimationUseInv { get; set; }

		string AnimationOverGui { get; set; }

		bool HideWhenBlocking { get; set; }

		bool NoneCursorActive { get; }

		bool InventoryCursorOverridden { get; }

		Vector2 PositionOverride { get; set; }

		bool HasPositionOverride { get; }

		Color InventoryOutlineColor { get; set; }

		void ResetAnimationOverride();

		void PlayAnimation(string anim);

		void StopAnimation();

		void SetPositionOverride(Vector2 position);

		void ClearPositionOverride();

		QuestCursorComponent GetInstance();
	}
}
