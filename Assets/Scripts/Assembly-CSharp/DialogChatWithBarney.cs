using System.Collections;
using PowerScript;
using PowerTools.Quest;

public class DialogChatWithBarney : DialogTreeScript<DialogChatWithBarney>
{
	public IEnumerator OnStart()
	{
		yield return C.WalkToClicked();
		yield return C.FaceClicked();
		QuestScript.Camera.SetZoom(2f, 2f);
		yield return C.Barney.Face(C.Dave);
		yield return C.Barney.Say("What is it?");
		yield return QuestScript.E.Break;
	}

	public IEnumerator Option1(IDialogOption option)
	{
		yield return C.Dave.Say("Rad cave");
		yield return QuestScript.E.WaitSkip();
		yield return C.Barney.FaceUp();
		yield return QuestScript.E.WaitSkip();
		yield return C.Barney.Say("Uh-huh");
		yield return QuestScript.E.WaitSkip();
		yield return C.Barney.Face(C.Dave);
		yield return QuestScript.E.Break;
	}

	public IEnumerator Option2(IDialogOption option)
	{
		yield return C.Dave.Say("What's with this forest anyway?");
		yield return C.Barney.Say("Whaddaya mean?");
		OptionOff(1, 2, 3);
		OptionOff("bye");
		OptionOn("tree", "leaf", "forestdone");
		yield return QuestScript.E.Break;
	}

	public IEnumerator Option3(IDialogOption option)
	{
		yield return C.Dave.Say("Nice well there, huh?");
		yield return C.Barney.Say("No. I hate it. Lets never speak of it again");
		yield return C.Dave.Say("Alright");
		option.OffForever();
		yield return QuestScript.E.Break;
	}

	public IEnumerator OptionForestDone(IDialogOption option)
	{
		OptionOff("tree", "leaf", "forestdone");
		OptionOn(1, 2, 3);
		OptionOn("bye");
		Option(2).Used = Option("tree").Used && Option("leaf").Used;
		yield return QuestScript.E.Break;
	}

	public IEnumerator OptionTree(IDialogOption option)
	{
		yield return C.Dave.Say("The trees are cool");
		yield return C.Barney.Say(" I guess");
		yield return QuestScript.E.Break;
	}

	public IEnumerator OptionLeaf(IDialogOption option)
	{
		yield return C.Dave.Say("I like the foliage");
		yield return C.Barney.Say("Yes. It is pleasant foliage");
		yield return QuestScript.E.Break;
	}

	public IEnumerator OptionBye(IDialogOption option)
	{
		yield return C.Dave.Say("Later!");
		yield return QuestScript.E.WaitSkip();
		yield return C.Barney.FaceRight();
		yield return QuestScript.E.WaitSkip();
		yield return C.Barney.Say("Whatever");
		option.Used = false;
		Stop();
	}

	private IEnumerator OnStop()
	{
		QuestScript.Camera.ResetZoom(2f);
		yield return QuestScript.E.Break;
	}
}
