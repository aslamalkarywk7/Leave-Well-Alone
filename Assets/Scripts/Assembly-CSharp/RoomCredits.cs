using System.Collections;
using PowerScript;
using PowerTools.Quest;

public class RoomCredits : RoomScript<RoomCredits>
{
	private IEnumerator OnInteractPropRestart(IProp prop)
	{
		yield return QuestScript.E.ConsumeEvent;
		QuestScript.E.Restart();
		yield return QuestScript.E.Break;
	}

	private void OnEnterRoom()
	{
	}

	private IEnumerator OnEnterRoomAfterFade()
	{
		G.InventoryBar.Hide();
		yield return QuestScript.E.FadeIn(2f);
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnExitRoom(IRoom oldRoom, IRoom newRoom)
	{
		yield return QuestScript.E.Break;
	}

	private IEnumerator UpdateBlocking()
	{
		yield return QuestScript.E.Break;
	}

	private void Update()
	{
	}

	private IEnumerator OnAnyClick()
	{
		yield return QuestScript.E.Break;
	}

	private IEnumerator AfterAnyClick()
	{
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnInteractPropCred(IProp prop)
	{
		yield return QuestScript.E.Break;
	}
}
