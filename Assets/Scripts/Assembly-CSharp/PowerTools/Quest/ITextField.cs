namespace PowerTools.Quest
{
	public interface ITextField : IGuiControl
	{
		string Description { get; set; }

		string Cursor { get; set; }

		string Text { get; set; }

		bool Clickable { get; set; }

		void FocusKeyboard();
	}
}
