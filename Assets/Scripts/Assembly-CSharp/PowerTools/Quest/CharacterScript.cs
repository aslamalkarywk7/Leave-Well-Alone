using System;

namespace PowerTools.Quest
{
	[Serializable]
	public class CharacterScript<T> : QuestScript where T : QuestScript
	{
		public static T Script => QuestScript.E.GetScript<T>();
	}
}
