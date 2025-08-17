using PowerTools;
using PowerTools.Quest;

namespace PowerScript
{
	public static class G
	{
		public static IGui DialogTree => Singleton<PowerQuest>.Get.GetGui("DialogTree");

		public static IGui SpeechBox => Singleton<PowerQuest>.Get.GetGui("SpeechBox");

		public static IGui HoverText => Singleton<PowerQuest>.Get.GetGui("HoverText");

		public static IGui DisplayBox => Singleton<PowerQuest>.Get.GetGui("DisplayBox");

		public static IGui Prompt => Singleton<PowerQuest>.Get.GetGui("Prompt");

		public static IGui Toolbar => Singleton<PowerQuest>.Get.GetGui("Toolbar");

		public static IGui InventoryBar => Singleton<PowerQuest>.Get.GetGui("InventoryBar");

		public static IGui Options => Singleton<PowerQuest>.Get.GetGui("Options");

		public static IGui Save => Singleton<PowerQuest>.Get.GetGui("Save");
	}
}
