using System.Collections.Generic;

namespace PowerTools.Quest
{
	public interface IDialogTree
	{
		string ScriptName { get; }

		List<DialogOption> Options { get; }

		int NumOptionsEnabled { get; }

		int NumOptionsUnused { get; }

		bool FirstTimeShown { get; }

		int TimesShown { get; }

		IDialogOption this[string option] { get; }

		DialogTree Data { get; }

		void Start();

		void Stop();

		IDialogOption GetOption(string option);

		IDialogOption GetOption(int option);

		void OptionOn(params int[] option);

		void OptionOff(params int[] option);

		void OptionOffForever(params int[] option);

		void OptionOn(params string[] option);

		void OptionOff(params string[] option);

		void OptionOffForever(params string[] option);

		bool GetOptionOn(int option);

		bool GetOptionOffForever(int option);

		bool GetOptionUsed(int option);

		bool GetOptionOn(string option);

		bool GetOptionOffForever(string option);

		bool GetOptionUsed(string option);

		T GetScript<T>() where T : DialogTreeScript<T>;
	}
}
