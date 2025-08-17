using System.Collections;
using PowerScript;
using PowerTools;
using PowerTools.Quest;
using UnityEngine;

public class GlobalScript : GlobalScriptBase<GlobalScript>
{
	public enum eProgress
	{
		None = 0,
		GotWater = 1,
		DrankWater = 2,
		WonGame = 3
	}

	public enum eGameProgress
	{
		None = 0,
		BucketOnRock = 1,
		GotWaterBucket = 2,
		SeenFirstGlitch = 3,
		WateredTree = 4,
		SeenSecondGlitch = 5,
		EndGame = 6
	}

	public eProgress m_progressExample;

	public eGameProgress m_progress;

	public bool m_bucketOnRock;

	public bool m_treeWatered;

	public bool m_spokeToBarney;

	public void OnGameStart()
	{
	}

	public void OnPostRestore(int version)
	{
	}

	public void OnEnterRoom()
	{
	}

	public IEnumerator OnEnterRoomAfterFade()
	{
		yield return QuestScript.E.Break;
	}

	public IEnumerator OnExitRoom(IRoom oldRoom, IRoom newRoom)
	{
		yield return QuestScript.E.Break;
	}

	public IEnumerator UpdateBlocking()
	{
		yield return QuestScript.E.Break;
	}

	public void Update()
	{
	}

	public void UpdateNoPause()
	{
		UpdateInput();
	}

	private void UpdateInput()
	{
		bool flag = QuestScript.E.IsDebugBuild && (Input.GetKey(KeyCode.BackQuote) || Input.GetKey(KeyCode.Backslash));
		if (!QuestScript.E.Paused)
		{
			if (Input.GetKeyUp(KeyCode.Escape))
			{
				QuestScript.E.SkipCutscene();
			}
			if (Input.GetMouseButtonDown(0))
			{
				QuestScript.E.SkipDialog();
			}
			if (Input.GetKey(KeyCode.Escape) || Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Space))
			{
				QuestScript.E.SkipDialog(preventEarlySkip: false);
			}
		}
		if (!QuestScript.E.GetBlocked() && !QuestScript.E.Paused)
		{
			if (Input.GetKeyDown(KeyCode.F1))
			{
				G.Options.Show();
			}
			if (Input.GetKeyDown(KeyCode.F5))
			{
				QuestScript.E.Save(1, "Quicksave");
			}
			if (Input.GetKeyDown(KeyCode.F7))
			{
				QuestScript.E.RestoreSave(1);
			}
			if (Input.GetKeyDown(KeyCode.F9))
			{
				if (flag)
				{
					QuestScript.E.Restart(QuestScript.E.GetCurrentRoom(), QuestScript.E.GetCurrentRoom().Instance.m_debugStartFunction);
				}
				else
				{
					QuestScript.E.Restart();
				}
			}
		}
		if (Input.GetKey(KeyCode.UpArrow))
		{
			QuestScript.E.NavigateGui(eGuiNav.Up);
		}
		if (Input.GetKey(KeyCode.DownArrow))
		{
			QuestScript.E.NavigateGui(eGuiNav.Down);
		}
		if (Input.GetKey(KeyCode.RightArrow))
		{
			QuestScript.E.NavigateGui(eGuiNav.Right);
		}
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			QuestScript.E.NavigateGui(eGuiNav.Left);
		}
		if (Input.GetKeyDown(KeyCode.Return))
		{
			QuestScript.E.NavigateGui();
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			QuestScript.E.NavigateGui(eGuiNav.Cancel);
		}
		if (flag)
		{
			if (Input.GetKeyDown(KeyCode.I))
			{
				Singleton<PowerQuest>.Get.GetInventoryItems().ForEach(delegate(Inventory item)
				{
					item.Owned = true;
				});
			}
			if (Input.GetKeyDown(KeyCode.PageDown))
			{
				Systems.Time.SetDebugTimeMultiplier(Systems.Time.GetDebugTimeMultiplier() * 0.8f);
			}
			if (Input.GetKeyDown(KeyCode.PageUp))
			{
				Systems.Time.SetDebugTimeMultiplier(Systems.Time.GetDebugTimeMultiplier() + 0.2f);
			}
			if (Input.GetKeyDown(KeyCode.End))
			{
				Systems.Time.SetDebugTimeMultiplier(1f);
			}
		}
		if (QuestScript.E.IsDebugBuild && Input.GetKeyDown(KeyCode.Period))
		{
			Systems.Time.SetDebugTimeMultiplier(4f);
		}
		else if (QuestScript.E.IsDebugBuild && Input.GetKeyUp(KeyCode.Period))
		{
			Systems.Time.SetDebugTimeMultiplier(1f);
		}
		if (QuestScript.E.IsDebugBuild && Input.GetKey(KeyCode.Period))
		{
			QuestScript.E.SkipDialog(preventEarlySkip: false);
		}
	}

	public IEnumerator OnAnyClick()
	{
		yield return QuestScript.E.Break;
	}

	public IEnumerator OnWalkTo()
	{
		yield return QuestScript.E.Break;
	}

	public void OnMouseClick(bool leftClick, bool rightClick)
	{
		bool flag = QuestScript.E.GetMouseOverClickable() != null;
		if (C.Plr.HasActiveInventory && (rightClick || (!flag && leftClick) || QuestScript.Cursor.NoneCursorActive))
		{
			I.Active = null;
		}
		else
		{
			if (QuestScript.Cursor.NoneCursorActive || QuestScript.E.GetMouseOverType() == eQuestClickableType.Gui)
			{
				return;
			}
			if (leftClick)
			{
				if (flag)
				{
					if (C.Plr.HasActiveInventory && !QuestScript.Cursor.InventoryCursorOverridden)
					{
						QuestScript.E.ProcessClick(eQuestVerb.Inventory);
					}
					else if (QuestScript.E.GetMouseOverType() == eQuestClickableType.Inventory)
					{
						I.Active = (IInventory)QuestScript.E.GetMouseOverClickable();
					}
					else
					{
						QuestScript.E.ProcessClick(eQuestVerb.Use);
					}
				}
				else
				{
					QuestScript.E.ProcessClick(eQuestVerb.Walk);
				}
			}
			else if (rightClick && flag)
			{
				QuestScript.E.ProcessClick(eQuestVerb.Look);
			}
		}
	}

	public IEnumerator UnhandledInteract(IQuestClickable mouseOver)
	{
		if (mouseOver.ClickableType == eQuestClickableType.Inventory)
		{
			QuestScript.E.ActiveInventory = (IInventory)mouseOver;
			yield break;
		}
		switch (QuestScript.E.Occurrence("unhandledInteract") % 3)
		{
		case 0:
			yield return C.Display("You can't use that");
			break;
		case 1:
			yield return C.Display("That doesn't work");
			break;
		case 2:
			yield return C.Display("Nothing happened");
			break;
		}
	}

	public IEnumerator UnhandledLookAt(IQuestClickable mouseOver)
	{
		yield break;
	}

	public IEnumerator UnhandledUseInvInv(Inventory invA, Inventory invB)
	{
		yield return C.Display("You can't use those together");
	}

	public IEnumerator UnhandledUseInv(IQuestClickable mouseOver, Inventory item)
	{
		yield return C.Display("You can't use that");
	}
}
