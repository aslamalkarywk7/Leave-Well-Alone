using System;

namespace PowerTools.Quest
{
	[Serializable]
	public class QuestAction
	{
		public string m_editorName = "On Interact";

		public string m_editorNameLong = "Use";

		public string m_scriptName = "Interact";

		public eQuestVerb m_verb;
	}
}
