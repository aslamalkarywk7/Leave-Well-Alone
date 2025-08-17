using System.Collections;
using PowerScript;
using PowerTools.Quest;

public class RoomOldCave : RoomScript<RoomOldCave>
{
	private void OnEnterRoom()
	{
		C.Explorer.Position = QuestScript.Point("Portal");
		C.Explorer.FaceLeft();
		SystemAudio.PlayAmbientSound("caveReverse", 0.5f);
		if (QuestScript.Globals.m_bucketOnRock || I.BucketOfWater.EverCollected)
		{
			SystemAudio.Stop("drip");
			SystemAudio.Stop("dripFull");
			SystemAudio.Play("dripBucket");
		}
	}

	private IEnumerator OnInteractPropPortal(IProp prop)
	{
		yield return C.WalkToClicked();
		yield return C.FaceClicked();
		QuestScript.E.FadeColor = Utils.ColorFromHex("1BBFBF");
		SystemAudio.Play("phase");
		yield return QuestScript.E.FadeOut(0.5f);
		QuestScript.E.ChangeRoomBG(R.Cave);
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnInteractHotspotFlatRock(IHotspot hotspot)
	{
		yield return C.WalkToClicked();
		yield return C.FaceClicked();
		if (QuestScript.Hotspot("FlatRock").FirstUse)
		{
			yield return C.Explorer.Say("There's no stalagmite. That can only mean...");
			yield return C.Explorer.Say("It hasn't formed yet.");
		}
		else
		{
			yield return C.Explorer.Say("Water is dripping onto the rock surface... very slowly.");
		}
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnUseInvHotspotFlatRock(IHotspot hotspot, IInventory item)
	{
		if (I.Active == I.Bucket)
		{
			yield return C.WalkToClicked();
			yield return C.FaceClicked();
			yield return C.Explorer.Say("If I leave the bucket here...");
			yield return C.Explorer.PlayAnimation("take");
			I.Bucket.Remove();
			QuestScript.Prop("BucketUpright").Show();
			QuestScript.Hotspot("FlatRock").Hide();
			QuestScript.Globals.m_bucketOnRock = true;
			QuestScript.Globals.m_progress = GlobalScript.eGameProgress.BucketOnRock;
			SystemAudio.Stop("drip");
			SystemAudio.Play("dripBucket");
		}
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnInteractPropBucketUpright(IProp prop)
	{
		yield return C.WalkToClicked();
		yield return C.FaceClicked();
		if (I.BucketOfWater.Owned || I.Bucket.Owned)
		{
			yield return C.Explorer.Say("I've already got a bucket. Better not take another one since they're technically the same bucket. Things could get weird.");
		}
		else if (QuestScript.Prop("BucketUpright").FirstUse)
		{
			yield return C.Explorer.Say("It's very slowly being filled with drops of water.");
		}
		else
		{
			yield return C.Explorer.Say("I think I'll leave it where it is.");
		}
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnInteractPropTreeThen(IProp prop)
	{
		yield return C.WalkToClicked();
		yield return C.FaceClicked();
		if (QuestScript.Prop("WetEarth").Visible)
		{
			yield return C.Explorer.Say("That water should help it grow.");
		}
		else if (QuestScript.Prop("TreeThen").FirstUse)
		{
			yield return C.Explorer.Say("The sapling. It's alive. How is this possible?");
		}
		else
		{
			yield return C.Explorer.Say("It's growing, but it'll need some help if it's going to survive.");
		}
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnUseInvPropTreeThen(IProp prop, IInventory item)
	{
		if (I.Active == I.BucketOfWater)
		{
			yield return C.WalkToClicked();
			yield return C.FaceClicked();
			yield return C.Explorer.Say("Here you go. Water from the future.");
			yield return C.Explorer.PlayAnimation("take");
			I.Bucket.Add();
			I.BucketOfWater.Remove();
			QuestScript.Prop("WetEarth").Show();
			QuestScript.Globals.m_treeWatered = true;
		}
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnEnterRoomAfterFade()
	{
		yield return QuestScript.E.FadeIn(0.5f);
		QuestScript.E.FadeColorRestore();
		if (R.OldCave.FirstTimeVisited)
		{
			yield return C.Explorer.Say("What the hell?");
			yield return C.Explorer.WalkTo(QuestScript.Point("Explore"));
			yield return C.Explorer.Say("It's the same cave, but it feels different.");
		}
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnUseInvPropBucketUpright(IProp prop, IInventory item)
	{
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnInteractHotspotCeiling(IHotspot hotspot)
	{
		if (QuestScript.Hotspot("Ceiling").FirstUse)
		{
			yield return C.Explorer.Say("The well has gone! This cave is completely enclosed.");
		}
		else
		{
			yield return C.Explorer.Say("There's no way out, aside from back through that portal.");
		}
		yield return QuestScript.E.Break;
	}
}
