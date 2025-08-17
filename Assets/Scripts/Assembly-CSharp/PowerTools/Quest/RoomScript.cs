using System;
using PowerScript;

namespace PowerTools.Quest
{
	[Serializable]
	public class RoomScript<T> : QuestScript where T : QuestScript
	{
		public static T Script => QuestScript.E.GetScript<T>();

		protected bool EnteredFromEditor => R.EnteredFromEditor;

		protected bool FirstTimeVisited => R.Current.FirstTimeVisited;
	}
}
