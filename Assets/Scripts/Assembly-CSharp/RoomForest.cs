using System.Collections;
using PowerScript;
using PowerTools.Quest;

public class RoomForest : RoomScript<RoomForest>
{
	private enum eThingsYouveDone
	{
		Start = 0,
		InsultedChimp = 1,
		EatenSandwich = 2,
		LoadedCrossbow = 3,
		AttackedFlyingNun = 4,
		PhonedAlbatross = 5
	}

	private int m_timesClickedSky;

	private eThingsYouveDone m_thingsDone;

	public void OnEnterRoom()
	{
	}

	public IEnumerator OnEnterRoomAfterFade()
	{
		if (base.FirstTimeVisited && !base.EnteredFromEditor)
		{
			yield return C.Dave.Say("Well, I guess this is a test project for an adventure game");
			yield return C.Dave.WalkTo(QuestScript.Point("EntryWalk"));
			yield return C.Dave.Say("Sure looks adventurey!");
			SystemAudio.PlayMusic("MusicExample");
			yield return QuestScript.E.WaitSkip();
			yield return C.Display("Left Click to Walk & Interact\nRight Click to Look At");
		}
		C.Dave.WalkToBG(QuestScript.Point("EntryWalk"));
		yield return QuestScript.E.Break;
	}

	public IEnumerator OnInteractHotspotForest(Hotspot hotspot)
	{
		yield return C.Dave.WalkTo(QuestScript.E.GetMousePosition());
		yield return C.Dave.FaceUp();
		yield return C.Dave.Say("Feels impenetrable");
		if (QuestScript.E.Occurrence("useForest") == 2)
		{
			yield return C.Dave.Say("Same as it did the last two times");
		}
		yield return QuestScript.E.Break;
	}

	public IEnumerator OnInteractPropWell(Prop prop)
	{
		yield return C.WalkToClicked();
		yield return C.FaceClicked();
		QuestScript.E.StartCutscene();
		yield return QuestScript.E.WaitSkip();
		yield return C.Dave.Say("I can't see anything in the well");
		yield return QuestScript.E.WaitSkip();
		yield return C.Dave.Say("And I'm certainly not climbing down there");
		yield return QuestScript.E.WaitSkip();
		yield return C.Barney.Face(C.Dave);
		yield return C.Barney.Say("Oh go on!");
		yield return C.Dave.Face(C.Barney);
		yield return C.Dave.Say("Ummmm...");
		yield return QuestScript.E.WaitSkip();
		yield return C.FaceClicked();
		yield return QuestScript.E.WaitSkip(1f);
		yield return C.Dave.Face(C.Barney);
		yield return QuestScript.E.WaitSkip();
		yield return C.FaceClicked();
		yield return QuestScript.E.WaitSkip(1f);
		yield return C.Dave.Face(C.Barney);
		yield return QuestScript.E.WaitSkip(1f);
		yield return C.Dave.Say("No");
		yield return QuestScript.E.WaitSkip();
		QuestScript.E.EndCutscene();
		yield return QuestScript.E.Break;
	}

	public IEnumerator OnInteractHotspotCave(Hotspot hotspot)
	{
		yield return C.WalkToClicked();
		yield return C.FaceClicked();
		yield return QuestScript.E.WaitSkip();
		yield return C.Dave.Say("No way am I going in there!");
		yield return C.Dave.FaceDown();
		yield return QuestScript.E.WaitSkip();
		yield return C.Dave.Say("There might be beetles");
		yield return QuestScript.E.Break;
	}

	public IEnumerator OnInteractPropBucket(Prop prop)
	{
		yield return C.WalkToClicked();
		yield return C.FaceClicked();
		yield return C.Display("Dave stoops to pick up the bucket");
		SystemAudio.Play("Bucket");
		prop.Disable();
		I.Bucket.AddAsActive();
		yield return QuestScript.E.WaitSkip();
		yield return C.Player.FaceDown();
		yield return C.Dave.Say("Yaaay! I got a bucket!");
		yield return QuestScript.E.WaitSkip();
		yield return C.Display("Access your Inventory from the top of the screen");
		yield return QuestScript.E.Break;
	}

	public IEnumerator OnUseInvPropWell(Prop prop, Inventory item)
	{
		if (item == I.Bucket)
		{
			yield return C.WalkToClicked();
			yield return C.FaceClicked();
			yield return C.Display("Dave lowers the bucket down, and collects some juicy well water");
			QuestScript.Globals.m_progressExample = GlobalScript.eProgress.GotWater;
			yield return C.Dave.Say("Yaaay! I solved the real hard puzzle!");
			yield return QuestScript.E.Wait(1f);
			yield return C.Display("THE END");
			yield return QuestScript.E.WaitSkip();
			yield return C.Dave.FaceDown();
			yield return QuestScript.E.WaitSkip();
			yield return C.Dave.Say("Yaay!");
			QuestScript.Globals.m_progressExample = GlobalScript.eProgress.WonGame;
		}
		yield return QuestScript.E.Break;
	}

	public IEnumerator OnUseInvPropBucket(Prop prop, Inventory item)
	{
		yield return QuestScript.E.Break;
	}

	public IEnumerator OnInteractHotspotSky(Hotspot hotspot)
	{
		m_timesClickedSky++;
		yield return C.Display($"You've clicked the sky {m_timesClickedSky} times");
	}

	public IEnumerator OnLookAtHotspotForest(IHotspot hotspot)
	{
		yield return C.Dave.FaceUp();
		if (hotspot.FirstLook)
		{
			yield return C.Dave.Say("Looks impenetrable");
		}
		else
		{
			yield return C.Dave.Say("Still looks impenetrable");
		}
		yield return QuestScript.E.Break;
	}

	public IEnumerator OnEnterRegionCorner(IRegion region, ICharacter character)
	{
		yield return QuestScript.E.WaitSkip();
		C.Dave.StopWalking();
		yield return QuestScript.E.WaitSkip();
		yield return C.Dave.FaceDown();
		yield return QuestScript.E.WaitSkip();
		yield return C.Dave.Say("This corner gives me the heebie jeebies");
		yield return QuestScript.E.WaitSkip(0.25f);
		yield return C.Barney.Face(C.Dave);
		yield return C.Barney.Say("Yeah, stay away from that corner dude, it has a Tint color.");
		yield return QuestScript.E.WaitSkip(0.25f);
		yield return C.Dave.FaceUp();
		yield return C.Dave.Say("Good idea, Let's set it's Walkable property to false so I'll never make the same mistake again");
		yield return C.Plr.WalkTo(750f, -335f);
		QuestScript.Region("Corner").Walkable = false;
	}

	public IEnumerator OnLookAtPropWell(IProp prop)
	{
		yield return C.FaceClicked();
		yield return C.Dave.Say("Well well well");
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnLookAtPropBucket(IProp prop)
	{
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnInteractCharacterBarney(ICharacter character)
	{
		if (m_thingsDone == eThingsYouveDone.EatenSandwich)
		{
			yield return C.Dave.Say("I ate your sandwich");
			yield return C.Barney.Say("You monster");
		}
		yield return QuestScript.E.Break;
	}
}
