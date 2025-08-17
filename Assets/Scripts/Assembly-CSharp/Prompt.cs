using System;
using PowerTools.Quest;

[QuestAutoCompletable]
public class Prompt
{
	private static void Show(string text, string buttonOk, Action onOk = null)
	{
		GuiScript<GuiPrompt>.Script.Show(text, buttonOk, onOk);
	}

	private static void Show(string text, string buttonOk, string buttonCancel, Action onOk = null, Action onCancel = null)
	{
		GuiScript<GuiPrompt>.Script.Show(text, buttonOk, buttonCancel, onOk, onCancel);
	}
}
