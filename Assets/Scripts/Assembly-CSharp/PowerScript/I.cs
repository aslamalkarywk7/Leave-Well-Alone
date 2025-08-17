using PowerTools;
using PowerTools.Quest;

namespace PowerScript
{
	public static class I
	{
		public static IInventory Bucket => Singleton<PowerQuest>.Get.GetInventory("Bucket");

		public static IInventory Stalagmite => Singleton<PowerQuest>.Get.GetInventory("Stalagmite");

		public static IInventory BucketOfWater => Singleton<PowerQuest>.Get.GetInventory("BucketOfWater");

		public static IInventory Rope => Singleton<PowerQuest>.Get.GetInventory("Rope");

		public static IInventory Active
		{
			get
			{
				return Singleton<PowerQuest>.Get.GetPlayer().ActiveInventory;
			}
			set
			{
				Singleton<PowerQuest>.Get.GetPlayer().ActiveInventory = value;
			}
		}

		public static IInventory Current
		{
			get
			{
				return Singleton<PowerQuest>.Get.GetPlayer().ActiveInventory;
			}
			set
			{
				Singleton<PowerQuest>.Get.GetPlayer().ActiveInventory = value;
			}
		}
	}
}
