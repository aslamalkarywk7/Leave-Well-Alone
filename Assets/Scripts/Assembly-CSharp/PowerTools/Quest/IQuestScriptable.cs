using System.Reflection;

namespace PowerTools.Quest
{
	public interface IQuestScriptable
	{
		string GetScriptName();

		string GetScriptClassName();

		QuestScript GetScript();

		void HotLoadScript(Assembly assembly);

		void EditorRename(string name);
	}
}
