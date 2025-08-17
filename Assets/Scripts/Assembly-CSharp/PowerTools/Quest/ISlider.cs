using UnityEngine;

namespace PowerTools.Quest
{
	public interface ISlider : IGuiControl
	{
		string Description { get; set; }

		string Cursor { get; set; }

		string Text { get; set; }

		float Ratio { get; set; }

		string AnimBar { get; set; }

		string AnimBarHover { get; set; }

		string AnimBarClick { get; set; }

		string AnimBarOff { get; set; }

		string AnimHandle { get; set; }

		string AnimHandleHover { get; set; }

		string AnimHandleClick { get; set; }

		string AnimHandleOff { get; set; }

		Color Color { get; set; }

		Color ColorHover { get; set; }

		Color ColorClick { get; set; }

		Color ColorOff { get; set; }

		bool Clickable { get; set; }

		float KeyboardIncrement { get; set; }
	}
}
