using UnityEngine;

namespace PowerTools.Quest
{
	public interface IGui
	{
		string ScriptName { get; }

		MonoBehaviour Instance { get; }

		bool Visible { get; set; }

		bool Clickable { get; set; }

		bool HasFocus { get; }

		bool Modal { get; set; }

		bool PauseGame { get; set; }

		Vector2 Position { get; set; }

		float Baseline { get; set; }

		string Cursor { get; set; }

		Gui Data { get; }

		void Show();

		void Hide();

		void ShowAtFront();

		void ShowAtBack();

		void ShowBehind(IGui gui);

		void ShowInfront(IGui gui);

		bool Navigate(eGuiNav button);

		void NavigateToControl(IGuiControl control);

		void ResetNavigation();

		GuiControl GetControl(string name);

		bool HasControl(string name);

		T GetScript<T>() where T : GuiScript<T>;
	}
}
