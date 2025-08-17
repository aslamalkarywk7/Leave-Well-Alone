using System.Collections;
using PowerScript;
using PowerTools.Quest;

public class RoomTitle : RoomScript<RoomTitle>
{
	public void OnEnterRoom()
	{
		G.InventoryBar.Hide();
	}

	public IEnumerator OnEnterRoomAfterFade()
	{
		QuestScript.E.StartCutscene();
		QuestScript.Prop("Title").Visible = true;
		yield return QuestScript.Prop("Title").Fade(0f, 1f, 1f);
		yield return QuestScript.E.Wait();
		if (QuestScript.E.GetSaveSlotData().Count > 0)
		{
			QuestScript.Prop("Continue").Enable();
			QuestScript.Prop("Continue").FadeBG(0f, 1f, 1f);
		}
		QuestScript.Prop("New").Enable();
		yield return QuestScript.Prop("New").Fade(0f, 1f, 1f);
		QuestScript.E.EndCutscene();
	}

	public IEnumerator OnInteractPropNew(Prop prop)
	{
		G.InventoryBar.Show();
		QuestScript.E.ChangeRoomBG(R.Forest);
		yield return QuestScript.E.ConsumeEvent;
	}

	public IEnumerator OnInteractPropContinue(Prop prop)
	{
		QuestScript.E.RestoreLastSave();
		yield return QuestScript.E.ConsumeEvent;
	}
}
