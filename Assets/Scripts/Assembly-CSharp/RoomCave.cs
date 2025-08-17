using System.Collections;
using PowerScript;
using PowerTools.Quest;

public class RoomCave : RoomScript<RoomCave>
{
	private void OnEnterRoom()
	{
		if (R.FirstTimeVisited)
		{
			C.Explorer.Position = QuestScript.Point("Start");
		}
		else
		{
			C.Explorer.Position = QuestScript.Point("Portal");
			C.Explorer.FaceLeft();
		}
		if (QuestScript.Globals.m_bucketOnRock && !I.BucketOfWater.EverCollected)
		{
			QuestScript.Prop("StalagmiteSnapped").Hide();
			QuestScript.Prop("BucketofWater").Show();
		}
		else
		{
			QuestScript.Prop("StalagmiteSnapped").Visible = true;
			QuestScript.Prop("BucketofWater").Hide();
		}
		if (QuestScript.Globals.m_treeWatered)
		{
			QuestScript.Prop("BigTree").Show();
			QuestScript.Prop("Branch").Show();
			QuestScript.Prop("Treeroot").Hide();
			QuestScript.Prop("Beam").Hide();
		}
		else
		{
			QuestScript.Prop("BigTree").Hide();
			QuestScript.Prop("Branch").Hide();
			QuestScript.Prop("Treeroot").Show();
			QuestScript.Prop("Beam").Show();
		}
		if (QuestScript.Globals.m_bucketOnRock)
		{
			SystemAudio.Stop("dripBucket");
			SystemAudio.Stop("dripFull");
			SystemAudio.Play("dripFull");
		}
		else
		{
			SystemAudio.Stop("dripBucket");
			SystemAudio.Stop("drip");
			SystemAudio.Play("drip");
		}
	}

	[QuestPlayFromFunction]
	private void PlayFromEndGame()
	{
		QuestScript.Globals.m_progress = GlobalScript.eGameProgress.EndGame;
		QuestScript.Globals.m_treeWatered = true;
		QuestScript.Prop("FallenBranch").Show();
		QuestScript.Prop("Portal").Show();
		QuestScript.Prop("Beam").Hide();
	}

	private IEnumerator OnInteractPropPortal(IProp prop)
	{
		yield return C.WalkToClicked();
		yield return C.FaceClicked();
		if (QuestScript.Globals.m_progress == GlobalScript.eGameProgress.EndGame)
		{
			SystemAudio.Play("phase");
			C.Player.Disable();
			yield return QuestScript.E.Wait(4f);
			yield return QuestScript.E.FadeOut(0f);
			yield return QuestScript.E.Wait(0.1f);
			yield return QuestScript.E.FadeIn(0f);
			C.Player.StopWalking();
			SystemAudio.Play("glitch");
			QuestScript.Prop("Glitch").Show();
			yield return QuestScript.E.Wait(0.2f);
			QuestScript.Prop("Glitch").Hide();
			yield return QuestScript.E.Wait(0.05f);
			QuestScript.Prop("Glitch").Show();
			yield return QuestScript.E.Wait(0.05f);
			QuestScript.Prop("Glitch").Hide();
			yield return QuestScript.E.Wait(0.2f);
			SystemAudio.Play("braam");
			SystemAudio.Play("breath");
			QuestScript.Prop("Glitch").Show();
			QuestScript.Prop("Death").Show();
			yield return QuestScript.E.Wait(4f);
			QuestScript.Prop("Glitch").Hide();
			yield return QuestScript.E.Wait(0.05f);
			QuestScript.Prop("Glitch").Show();
			yield return QuestScript.E.Wait(0.05f);
			QuestScript.Prop("Glitch").Hide();
			QuestScript.Prop("Death").Hide();
			yield return QuestScript.E.Wait(2f);
			yield return QuestScript.E.FadeOut(2f);
			yield return QuestScript.E.ChangeRoom(R.Credits);
		}
		else
		{
			if (QuestScript.Prop("Portal").FirstUse)
			{
				yield return C.Explorer.Say("Guess I'm stepping into the random glowing portal. Wish me luck!");
				yield return QuestScript.E.WaitSkip();
			}
			QuestScript.E.FadeColor = Utils.ColorFromHex("1BBFBF");
			SystemAudio.Play("phase");
			yield return QuestScript.E.FadeOut(0.5f);
			QuestScript.E.ChangeRoomBG(R.OldCave);
		}
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnLookAtPropStalagmite(IProp prop)
	{
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnInteractPropStalagmite(IProp prop)
	{
		yield return C.WalkToClicked();
		yield return C.FaceClicked();
		if (QuestScript.Prop("Stalagmite").FirstUse)
		{
			yield return C.Explorer.Say("It's a stalagmite, formed by mineralized water dripping from above over hundreds of years.");
		}
		else
		{
			yield return C.Explorer.Say("Let's see...");
			yield return C.Explorer.PlayAnimation("take");
			yield return QuestScript.E.FadeOut();
			yield return C.Display("I pull the stalagmite, and it snaps off near the base.");
			QuestScript.Prop("Stalagmite").Hide();
			I.Stalagmite.Add();
			yield return QuestScript.E.FadeIn();
		}
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnInteractPropBucket(IProp prop)
	{
		yield return C.WalkToClicked();
		yield return C.FaceClicked();
		if (QuestScript.Prop("Bucket").FirstUse)
		{
			yield return C.Explorer.Say("That's the bucket that was attached to the well. There's a short length of rope tied to the handle.");
		}
		else
		{
			yield return C.Explorer.PlayAnimation("take");
			QuestScript.Prop("Bucket").Hide();
			yield return C.Explorer.Say("I'll untie the rope.");
			I.Bucket.Add();
			I.Rope.Add();
		}
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnInteractHotspotRecess(IHotspot hotspot)
	{
		yield return C.WalkToClicked();
		yield return C.FaceClicked();
		if (QuestScript.Hotspot("Recess").FirstUse)
		{
			yield return C.Explorer.Say("There's a circular recess in the cave wall, with a hole in the centre.");
			QuestScript.Hotspot("Hole").Show();
		}
		else
		{
			yield return C.Explorer.Say("I'm not sure what this is for, but that hole in the centre seems interesting.");
		}
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnInteractHotspotHole(IHotspot hotspot)
	{
		yield return C.WalkToClicked();
		yield return C.FaceClicked();
		if (QuestScript.Hotspot("Hole").FirstUse)
		{
			yield return C.Explorer.Say("The hole is pretty deep, and there's a faint blue glow inside it.");
		}
		else
		{
			yield return C.Explorer.Say("What could this be for?");
		}
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnUseInvHotspotHole(IHotspot hotspot, IInventory item)
	{
		if (I.Rope == I.Active)
		{
			yield return C.Explorer.Say("I don't think the rope will do any good there.");
		}
		else if (I.Stalagmite == I.Active)
		{
			yield return C.WalkToClicked();
			yield return C.FaceClicked();
			yield return QuestScript.E.FadeOut();
			I.Stalagmite.Remove();
			yield return C.Display("I push the stalagmite into the hole. Nothing happens at first, but then...");
			QuestScript.Hotspot("Recess").Hide();
			QuestScript.Hotspot("Hole").Hide();
			yield return QuestScript.E.FadeIn();
			QuestScript.Prop("Portal").Show();
			SystemAudio.Play("hum");
			yield return QuestScript.Prop("Portal").Fade(0f, 1f, 1f);
			yield return C.Explorer.Say("What in God's name...");
		}
		else
		{
			yield return C.Explorer.Say("I don't think that would work.");
		}
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnInteractPropBucketOfWater(IProp prop)
	{
		yield return C.WalkToClicked();
		yield return C.FaceClicked();
		if (QuestScript.Prop("BucketOfWater").FirstUse)
		{
			yield return C.Explorer.Say("Great Scott, it worked! The bucket is full of water.");
		}
		else
		{
			yield return C.Explorer.PlayAnimation("take");
			QuestScript.Prop("BucketOfWater").Hide();
			yield return C.Explorer.Say("Heavy!");
			I.BucketOfWater.Add();
			SystemAudio.Stop("dripFull");
			SystemAudio.Play("drip");
			QuestScript.Globals.m_bucketOnRock = false;
			QuestScript.Globals.m_progress = GlobalScript.eGameProgress.GotWaterBucket;
		}
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnInteractPropTreeroot(IProp prop)
	{
		yield return C.WalkToClicked();
		yield return C.FaceClicked();
		if (QuestScript.Prop("Treeroot").FirstUse)
		{
			yield return C.Explorer.Say("It's a dead sapling.");
			yield return C.Explorer.Say("It's getting light from a small shaft in the cave ceiling, but it looks like it didn't get enough water.");
		}
		else
		{
			yield return C.Explorer.Say("I don't need a dead sapling.");
		}
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnEnterRoomAfterFade()
	{
		if (R.Cave.FirstTimeVisited)
		{
			yield return QuestScript.E.FadeIn(2f);
			yield break;
		}
		yield return QuestScript.E.FadeIn(0.5f);
		QuestScript.E.FadeColorRestore();
	}

	private IEnumerator OnInteractPropBigTree(IProp prop)
	{
		yield return C.WalkToClicked();
		yield return C.FaceClicked();
		if (!QuestScript.Prop("Branch").Visible)
		{
			yield return C.Explorer.Say("It's no use to me now.");
		}
		else
		{
			yield return C.Explorer.Say("The tree has blocked the shaft where the light was coming from, but maybe I can reach the rope in the well.");
			yield return QuestScript.E.FadeOut(0.5f);
			yield return C.Display("I climb the tree and ease my way out onto the branch.");
			QuestScript.Prop("ExplorerReach").Show();
			C.Player.Visible = false;
			yield return QuestScript.E.FadeIn(0.5f);
			yield return QuestScript.E.WaitSkip();
			C.Explorer.TextPositionOverride = QuestScript.Point("TextPosition");
			yield return C.Explorer.Say("Almost... got it...");
			C.Explorer.ResetTextPosition();
			yield return QuestScript.E.WaitSkip();
			yield return QuestScript.E.FadeOut(0.5f);
			yield return C.Display("But the branch isn't strong enough.");
			SystemAudio.Play("branchCreak");
			yield return QuestScript.E.Wait(1.5f);
			SystemAudio.Play("fall");
			QuestScript.Prop("ExplorerReach").Hide();
			yield return QuestScript.E.Wait(1f);
			QuestScript.Prop("Branch").Hide();
			C.Player.Show();
			QuestScript.Prop("FallenBranch").Show();
			yield return QuestScript.E.FadeIn(0.5f);
			yield return C.Explorer.Say("There goes my last hope of getting out of here.");
			yield return C.Explorer.Say("Guess I'd better go through the portal again, see if I missed anything.");
			QuestScript.Globals.m_progress = GlobalScript.eGameProgress.EndGame;
		}
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnInteractPropBucketUpright(IProp prop)
	{
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnUseInvPropBucketUpright(IProp prop, IInventory item)
	{
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnEnterRegionGlitchTrigger(IRegion region, ICharacter character)
	{
		if (QuestScript.Globals.m_progress == GlobalScript.eGameProgress.BucketOnRock && QuestScript.E.FirstOccurrence("Glitch1"))
		{
			yield return QuestScript.E.FadeOut(0f);
			yield return QuestScript.E.Wait(0.1f);
			yield return QuestScript.E.FadeIn(0f);
			C.Player.StopWalking();
			SystemAudio.Play("glitch");
			QuestScript.Prop("Glitch").Show();
			yield return QuestScript.E.Wait(0.2f);
			QuestScript.Prop("Glitch").Hide();
			yield return QuestScript.E.Wait(0.05f);
			QuestScript.Prop("Glitch").Show();
			yield return QuestScript.E.Wait(0.05f);
			QuestScript.Prop("Glitch").Hide();
			yield return QuestScript.E.Wait(0.2f);
			SystemAudio.Play("breath");
			QuestScript.Prop("Glitch").Show();
			QuestScript.Prop("BigTree").Show();
			QuestScript.Prop("Branch").Show();
			yield return QuestScript.E.Wait(3f);
			QuestScript.Prop("Glitch").Hide();
			yield return QuestScript.E.Wait(0.05f);
			QuestScript.Prop("Glitch").Show();
			yield return QuestScript.E.Wait(0.05f);
			QuestScript.Prop("Glitch").Hide();
			QuestScript.Prop("BigTree").Hide();
			QuestScript.Prop("Branch").Hide();
			QuestScript.Globals.m_progress = GlobalScript.eGameProgress.SeenFirstGlitch;
			yield return QuestScript.E.WaitSkip();
			yield return C.Explorer.Say("What the hell was that??");
		}
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnExitRegionGlitchTrigger(IRegion region, ICharacter character)
	{
		if (QuestScript.Globals.m_progress == GlobalScript.eGameProgress.GotWaterBucket)
		{
			yield return QuestScript.E.FadeOut(0f);
			yield return QuestScript.E.Wait(0.1f);
			yield return QuestScript.E.FadeIn(0f);
			C.Player.StopWalking();
			SystemAudio.Play("glitch");
			QuestScript.Prop("Glitch").Show();
			yield return QuestScript.E.Wait(0.2f);
			QuestScript.Prop("Glitch").Hide();
			yield return QuestScript.E.Wait(0.05f);
			QuestScript.Prop("Glitch").Show();
			yield return QuestScript.E.Wait(0.05f);
			QuestScript.Prop("Glitch").Hide();
			yield return QuestScript.E.Wait(0.2f);
			SystemAudio.Play("breath");
			QuestScript.Prop("Glitch").Show();
			QuestScript.Prop("BigTree").Show();
			QuestScript.Prop("FallenBranch").Show();
			yield return QuestScript.E.Wait(3f);
			QuestScript.Prop("Glitch").Hide();
			yield return QuestScript.E.Wait(0.05f);
			QuestScript.Prop("Glitch").Show();
			yield return QuestScript.E.Wait(0.05f);
			QuestScript.Prop("Glitch").Hide();
			QuestScript.Prop("BigTree").Hide();
			QuestScript.Prop("FallenBranch").Hide();
			QuestScript.Globals.m_progress = GlobalScript.eGameProgress.SeenSecondGlitch;
			yield return QuestScript.E.WaitSkip();
			yield return C.Explorer.Say("Okay, this is really starting to creep me out!");
		}
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnInteractPropWell(IProp prop)
	{
		if (QuestScript.Prop("Well").FirstUse)
		{
			yield return C.Explorer.Say("That's the well shaft I came down. There's a rope, but it's out of reach.");
		}
		else if (QuestScript.Prop("Branch").Visible)
		{
			yield return C.Explorer.Say("If that branch can hold my weight, I might just be able to reach the rope.");
		}
		else
		{
			yield return C.Explorer.Say("It's too high, I'll never get back up there.");
		}
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnUseInvPropWell(IProp prop, IInventory item)
	{
		if (I.Active == I.Rope)
		{
			yield return C.Explorer.Say("That's not gonna work.");
		}
		if (I.Active == I.Stalagmite)
		{
			yield return C.Explorer.Say("It's not long enough, and even if it was, there's no way to grab the end of the rope.");
		}
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnUseInvPropPortal(IProp prop, IInventory item)
	{
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnInteractPropStalactites(IProp prop)
	{
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnUseInvPropStalactites(IProp prop, IInventory item)
	{
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnUseInvPropTreeroot(IProp prop, IInventory item)
	{
		if (I.Active == I.Bucket)
		{
			yield return C.Explorer.Say("The bucket's empty. And even if it wasn't, the tree is already dead.");
		}
		if (I.Active == I.BucketOfWater)
		{
			yield return C.Explorer.Say("It's no use watering it now, it's already dead.");
		}
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnUseInvPropBucketOfWater(IProp prop, IInventory item)
	{
		yield return QuestScript.E.Break;
	}
}
