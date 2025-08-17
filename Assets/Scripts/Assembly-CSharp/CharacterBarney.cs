using System.Collections;
using PowerScript;
using PowerTools.Quest;

public class CharacterBarney : CharacterScript<CharacterBarney>
{
	public IEnumerator OnInteract()
	{
		yield return C.FaceClicked();
		yield return C.Dave.Say("Hey Barney!");
		yield return C.Barney.Face(C.Dave);
		yield return C.Barney.Say("Yeah?");
		yield return C.WalkToClicked();
		D.ChatWithBarney.Start();
		QuestScript.Globals.m_spokeToBarney = true;
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnUseInv(IInventory item)
	{
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnLookAt()
	{
		yield return QuestScript.E.Break;
	}
}
