using UnityEngine;

namespace PowerTools.Quest
{
	public interface IQuestClickable
	{
		eQuestClickableType ClickableType { get; }

		MonoBehaviour Instance { get; }

		string Description { get; set; }

		string ScriptName { get; }

		Vector2 WalkToPoint { get; set; }

		Vector2 LookAtPoint { get; set; }

		float Baseline { get; set; }

		bool Clickable { get; set; }

		string Cursor { get; set; }

		Vector2 Position { get; }

		void OnInteraction(eQuestVerb verb);

		void OnCancelInteraction(eQuestVerb verb);

		QuestScript GetScript();

		IQuestScriptable GetScriptable();
	}
}
