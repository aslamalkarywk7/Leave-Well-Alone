using PowerTools;
using PowerTools.Quest;

namespace PowerScript
{
	public static class D
	{
		public static IDialogTree ChatWithBarney => Singleton<PowerQuest>.Get.GetDialogTree("ChatWithBarney");

		public static IDialogTree Current => Singleton<PowerQuest>.Get.GetCurrentDialog();

		public static IDialogTree Previous => Singleton<PowerQuest>.Get.GetPreviousDialog();

		public static IDialogTree Get(string name)
		{
			return Singleton<PowerQuest>.Get.GetDialogTree(name);
		}
	}
}
