namespace PowerTools.Quest
{
	public static class Systems
	{
		public static PowerQuest Quest => Singleton<PowerQuest>.Get;

		public static SystemAudio Audio => SingletonAuto<SystemAudio>.Get;

		public static SystemTime Time => Singleton<SystemTime>.Get;

		public static SystemDebug Debug => SingletonAuto<SystemDebug>.Get;

		public static SystemText Text => Singleton<SystemText>.Get;

		public static bool Valid
		{
			get
			{
				if (Singleton<PowerQuest>.GetValid() && SingletonAuto<SystemAudio>.GetValid())
				{
					return Singleton<SystemTime>.GetValid();
				}
				return false;
			}
		}
	}
}
