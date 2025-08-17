using UnityEngine;

namespace PowerTools.Quest
{
	public interface IGuiControl
	{
		MonoBehaviour Instance { get; }

		bool Visible { get; set; }

		Vector2 Position { get; set; }

		bool Focused { get; }

		bool HasKeyboardFocus { get; set; }

		void Show();

		void Hide();

		void SetPosition(float x, float y);
	}
}
