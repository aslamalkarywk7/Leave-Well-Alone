using PowerTools;
using PowerTools.Quest;

namespace PowerScript
{
	public static class R
	{
		public static IRoom Title => Singleton<PowerQuest>.Get.GetRoom("Title");

		public static IRoom Forest => Singleton<PowerQuest>.Get.GetRoom("Forest");

		public static IRoom Cave => Singleton<PowerQuest>.Get.GetRoom("Cave");

		public static IRoom OldCave => Singleton<PowerQuest>.Get.GetRoom("OldCave");

		public static IRoom Intro => Singleton<PowerQuest>.Get.GetRoom("Intro");

		public static IRoom Credits => Singleton<PowerQuest>.Get.GetRoom("Credits");

		public static IRoom Current => Singleton<PowerQuest>.Get.GetCurrentRoom();

		public static IRoom Previous => Singleton<PowerQuest>.Get.GetPlayer().LastRoom;

		public static bool EnteredFromEditor
		{
			get
			{
				if (Singleton<PowerQuest>.Get.IsDebugBuild)
				{
					return Singleton<PowerQuest>.Get.GetPlayer().LastRoom == null;
				}
				return false;
			}
		}

		public static bool FirstTimeVisited => Current.FirstTimeVisited;
	}
}
