namespace PowerTools.Quest
{
	public interface IDialogOption
	{
		string ScriptName { get; }

		string Description { get; set; }

		bool Visible { get; }

		bool Disabled { get; }

		bool Used { get; set; }

		bool FirstUse { get; }

		int TimesUsed { get; }

		void On();

		void Off();

		void OffForever();
	}
}
