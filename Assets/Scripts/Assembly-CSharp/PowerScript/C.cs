using PowerTools;
using PowerTools.Quest;
using UnityEngine;

namespace PowerScript
{
	public static class C
	{
		public static ICharacter Dave => Singleton<PowerQuest>.Get.GetCharacter("Dave");

		public static ICharacter Barney => Singleton<PowerQuest>.Get.GetCharacter("Barney");

		public static ICharacter Explorer => Singleton<PowerQuest>.Get.GetCharacter("Explorer");

		public static ICharacter Player
		{
			get
			{
				Systems.Text.LastPlayerName = SystemText.ePlayerName.Player;
				return Singleton<PowerQuest>.Get.GetPlayer();
			}
		}

		public static ICharacter Plr
		{
			get
			{
				Systems.Text.LastPlayerName = SystemText.ePlayerName.Plr;
				return Singleton<PowerQuest>.Get.GetPlayer();
			}
		}

		public static ICharacter Ego
		{
			get
			{
				Systems.Text.LastPlayerName = SystemText.ePlayerName.Ego;
				return Singleton<PowerQuest>.Get.GetPlayer();
			}
		}

		public static Coroutine Display(string dialog)
		{
			return Singleton<PowerQuest>.Get.Display(dialog);
		}

		public static Coroutine Display(string dialog, int id)
		{
			return Singleton<PowerQuest>.Get.Display(dialog, id);
		}

		public static Coroutine DisplayBG(string dialog)
		{
			return Singleton<PowerQuest>.Get.Display(dialog);
		}

		public static Coroutine DisplayBG(string dialog, int id)
		{
			return Singleton<PowerQuest>.Get.Display(dialog, id);
		}

		public static void Section(string dialog)
		{
		}

		public static Coroutine WalkToClicked()
		{
			return Player.WalkToClicked();
		}

		public static Coroutine FaceClicked()
		{
			return Player.FaceClicked();
		}
	}
}
