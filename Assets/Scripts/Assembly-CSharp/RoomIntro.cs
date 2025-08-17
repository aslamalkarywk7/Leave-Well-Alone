using System.Collections;
using PowerScript;
using PowerTools.Quest;

public class RoomIntro : RoomScript<RoomIntro>
{
	private IEnumerator OnEnterRoomAfterFade()
	{
		QuestScript.E.StartCutscene();
		QuestScript.Prop("Title").Visible = true;
		yield return QuestScript.Prop("Title").Fade(0f, 1f, 1f);
		yield return QuestScript.E.Wait(1f);
		QuestScript.Prop("NewGame").Show();
		QuestScript.Prop("NewGame").FadeBG(0f, 1f, 1f);
		QuestScript.Prop("Credits").Show();
		yield return QuestScript.Prop("Credits").Fade(0f, 1f, 1f);
		QuestScript.E.EndCutscene();
		yield return QuestScript.E.Break;
	}

	private void OnEnterRoom()
	{
		SystemAudio.PlayAmbientSound("cave", 0.5f);
		SystemAudio.PlayMusic("Nightmare");
		G.InventoryBar.Hide();
	}

	private IEnumerator OnInteractPropNewGame(IProp prop)
	{
		QuestScript.E.StartCutscene();
		QuestScript.Prop("NewGame").FadeBG(1f, 0f, 1f);
		QuestScript.Prop("Credits").FadeBG(1f, 0f, 1f);
		yield return QuestScript.Prop("Title").Fade(1f, 0f, 1f);
		QuestScript.Prop("Title").Visible = false;
		QuestScript.Prop("NewGame").Visible = false;
		QuestScript.Prop("Credits").Visible = false;
		yield return QuestScript.E.Wait(2f);
		QuestScript.E.EndCutscene();
		yield return QuestScript.E.Wait();
		QuestScript.E.StartCutscene();
		QuestScript.Prop("ClickToContinue").Show();
		yield return C.Display("I've been exploring remote caves my whole life.");
		QuestScript.Prop("ClickToContinue").Hide();
		yield return C.Display("Nothing beats the thrill of finding new underground tunnels where no human has ever been before.");
		yield return C.Display("But this time was different.");
		yield return C.Display("As I rounded a bend in the shallow tunnel, I saw something that had no business being there.");
		yield return C.Display("A stone well, with a rope dangling down into a black abyss.");
		yield return C.Display("I dropped a pebble down the well and heard no sound.");
		yield return C.Display("My curiosity got the better of me, and I started to climb down the rope.");
		yield return C.Display("After a few minutes I could see some light from a cavern below, and I saw a bucket tied to the end of the rope.");
		SystemAudio.Play("wellFall");
		yield return C.Display("As I rested my weight on the bucket, the rope snapped and I landed in a heap on the cavern floor.");
		QuestScript.E.EndCutscene();
		G.InventoryBar.Show();
		yield return QuestScript.E.ChangeRoom(R.Cave);
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnInteractPropCredits(IProp prop)
	{
		yield return QuestScript.E.ConsumeEvent;
		yield return QuestScript.E.ChangeRoom(R.Credits);
		yield return QuestScript.E.Break;
	}
}
