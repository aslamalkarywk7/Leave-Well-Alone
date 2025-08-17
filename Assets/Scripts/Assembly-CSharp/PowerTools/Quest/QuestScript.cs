using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace PowerTools.Quest
{
	[Serializable]
	public class QuestScript
	{
		protected static IPowerQuest E => Singleton<PowerQuest>.Get;

		protected static ICursor Cursor => Singleton<PowerQuest>.Get.Cursor;

		protected static ICamera Camera => Singleton<PowerQuest>.Get.Camera;

		protected static QuestSettings Settings => Singleton<PowerQuest>.Get.Settings;

		protected static GlobalScript Globals => GlobalScriptBase<GlobalScript>.Script;

		protected static IHotspot Hotspot(string name)
		{
			return Singleton<PowerQuest>.Get.GetCurrentRoom().GetHotspot(name);
		}

		protected static IProp Prop(string name)
		{
			return Singleton<PowerQuest>.Get.GetCurrentRoom().GetProp(name);
		}

		protected static IRegion Region(string name)
		{
			return Singleton<PowerQuest>.Get.GetCurrentRoom().GetRegion(name);
		}

		protected static Vector2 Point(string name)
		{
			return Singleton<PowerQuest>.Get.GetCurrentRoom().GetPoint(name);
		}

		[OnDeserializing]
		private void CopyDefaults(StreamingContext sc)
		{
			QuestUtils.InitWithDefaults(this);
		}
	}
}
