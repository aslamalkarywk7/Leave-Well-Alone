using System.Collections;
using PowerScript;
using PowerTools.Quest;
using UnityEngine;

public class GuiToolbar : GuiScript<GuiToolbar>
{
	private IEnumerator OnAnyClick(IGuiControl control)
	{
		yield return QuestScript.E.Break;
	}

	private void Update()
	{
	}

	private void OnPostRestore(int version)
	{
	}

	private void OnShow()
	{
		if (R.Current != null && R.Current.ScriptName.Contains("Title"))
		{
			Button("Save").Clickable = false;
		}
		else
		{
			Button("Save").Clickable = true;
		}
	}

	private IEnumerator OnClickQuit(IGuiControl control)
	{
		GuiScript<GuiPrompt>.Script.Show("Really Save and Quit?", "Yes", "Cancel", delegate
		{
			if (R.Current != R.Title)
			{
				QuestScript.E.Save(1, "Autosave");
			}
			Application.Quit();
		});
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnClickOptions(IGuiControl control)
	{
		G.Options.Show();
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnClickSave(IGuiControl control)
	{
		GuiScript<GuiSave>.Script.ShowSave();
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnClickRestore(IGuiControl control)
	{
		GuiScript<GuiSave>.Script.ShowRestore();
		yield return QuestScript.E.Break;
	}
}
