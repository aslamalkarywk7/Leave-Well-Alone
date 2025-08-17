using System;
using System.Collections;
using PowerScript;
using PowerTools.Quest;
using UnityEngine;

public class GuiSave : GuiScript<GuiSave>
{
	private readonly int ScreenshotWidth = 380;

	private readonly int ScreenshotHeight = 210;

	private readonly float ScreenshotZoom = 1.25f;

	private readonly int AutoSaveSlot = 1;

	private bool m_save = true;

	public void ShowSave()
	{
		m_save = true;
		base.Data.Show();
	}

	public void ShowRestore()
	{
		m_save = false;
		base.Data.Show();
	}

	private void OnShow()
	{
		if (m_save)
		{
			Label("LblSave").Text = SystemText.Localize("Save Game");
		}
		else
		{
			Label("LblSave").Text = SystemText.Localize("Restore Game");
		}
		IButton[] array = new IButton[7]
		{
			null,
			Button("SaveSlot1"),
			Button("SaveSlot2"),
			Button("SaveSlot3"),
			Button("SaveSlot4"),
			Button("SaveSlot5"),
			Button("SaveSlot6")
		};
		for (int i = 1; i <= 6; i++)
		{
			QuestSaveSlotData saveSlotData = QuestScript.E.GetSaveSlotData(i);
			array[i].Clickable = true;
			if (saveSlotData == null)
			{
				array[i].Text = SystemText.Localize("Empty");
				if (m_save)
				{
					array[i].Anim = "SaveSlotFree";
				}
				else
				{
					array[i].Anim = "SaveSlotEmpty";
					array[i].Clickable = false;
				}
			}
			else
			{
				array[i].Text = saveSlotData.m_description;
				array[i].Anim = "SaveSlot";
				SpriteRenderer componentInChildren = array[i].Instance.transform.Find("Screenshot").GetComponentInChildren<SpriteRenderer>(includeInactive: true);
				if (saveSlotData.m_image == null)
				{
					componentInChildren.enabled = false;
				}
				else
				{
					componentInChildren.enabled = true;
					Texture2D texture2D = TextureScaler.scaled(saveSlotData.m_image, (int)((float)ScreenshotWidth * ScreenshotZoom), (int)((float)ScreenshotHeight * ScreenshotZoom), FilterMode.Point);
					texture2D.filterMode = FilterMode.Bilinear;
					Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, ScreenshotWidth, ScreenshotHeight), new Vector2(0.5f, 0.5f), 1f);
					componentInChildren.sprite = sprite;
				}
			}
			if (i == AutoSaveSlot)
			{
				array[i].Text = SystemText.Localize("Autosave");
				if (m_save)
				{
					array[i].Clickable = false;
				}
			}
			if (!m_save && QuestScript.E.GetLastSaveSlotData() == saveSlotData)
			{
				IButton obj = array[i];
				obj.Text = obj.Text + " (" + SystemText.Localize("Latest") + ")";
			}
		}
	}

	private void OnClickSaveSlot(IGuiControl control, int slot)
	{
		if (m_save)
		{
			if (QuestScript.E.GetSaveSlotData(slot) != null)
			{
				GuiScript<GuiPrompt>.Script.Show(SystemText.Localize("Overwrite save data?"), SystemText.Localize("Yes"), SystemText.Localize("No"), delegate
				{
					Save(slot);
				});
			}
			else
			{
				Save(slot);
			}
		}
		else
		{
			Load(slot);
		}
	}

	private void Save(int slot)
	{
		base.Data.Hide();
		string description = DateTime.Now.ToString("d MMM yy");
		QuestScript.E.Save(slot, description);
		GuiScript<GuiPrompt>.Script.Show(SystemText.Localize("Game Saved"), SystemText.Localize("Ok"));
	}

	private void Load(int slot)
	{
		base.Data.Hide();
		QuestScript.E.RestoreSave(slot);
	}

	private IEnumerator OnClickCancel(IGuiControl control)
	{
		G.Save.Hide();
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnClickSaveSlot1(IGuiControl control)
	{
		OnClickSaveSlot(control, 1);
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnClickSaveSlot2(IGuiControl control)
	{
		OnClickSaveSlot(control, 2);
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnClickSaveSlot3(IGuiControl control)
	{
		OnClickSaveSlot(control, 3);
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnClickSaveSlot4(IGuiControl control)
	{
		OnClickSaveSlot(control, 4);
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnClickSaveSlot5(IGuiControl control)
	{
		OnClickSaveSlot(control, 5);
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnClickSaveSlot6(IGuiControl control)
	{
		OnClickSaveSlot(control, 6);
		yield return QuestScript.E.Break;
	}
}
