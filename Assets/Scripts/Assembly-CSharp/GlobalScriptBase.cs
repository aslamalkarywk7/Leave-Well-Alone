using PowerTools.Quest;

public class GlobalScriptBase<T> : QuestScript where T : QuestScript
{
	public static T Script => QuestScript.E.GetScript<T>();
}
