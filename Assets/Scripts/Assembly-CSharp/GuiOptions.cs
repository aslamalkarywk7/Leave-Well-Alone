using System.Collections;
using System.Collections.Generic;
using PowerScript;
using PowerTools.Quest;
using UnityEngine;

public class GuiOptions : GuiScript<GuiOptions>
{
	private int m_resolution;

	private FullScreenMode m_fullScreenMode = FullScreenMode.Windowed;

	private bool m_resDirty;

	private List<int> m_uniqueResolutions = new List<int>();

	private IEnumerator OnClickBack(IGuiControl control)
	{
		G.Options.Hide();
		QuestScript.E.SaveSettings();
		yield return QuestScript.E.Break;
	}

	private void OnShow()
	{
		Slider("Volume").Ratio = QuestScript.Settings.Volume;
		UpdateResolutionList();
		Control("Apply").Hide();
		if (QuestScript.Settings.GetLanguages().Length < 2)
		{
			Control("Language").Hide();
			Container("GridContainer").Grid.RemoveItem(Control("Language").Instance.transform);
		}
		UpdateText();
	}

	public void UpdateResolutionList()
	{
		float num = Screen.width;
		float num2 = Screen.height;
		m_resDirty = false;
		m_uniqueResolutions.Clear();
		Resolution[] resolutions = Screen.resolutions;
		m_resolution = 0;
		Resolution resolution = default(Resolution);
		for (int i = 0; i < resolutions.Length; i++)
		{
			Resolution resolution2 = resolutions[i];
			if (resolution.width != resolution2.width || resolution.height != resolution2.height)
			{
				resolution = resolution2;
				m_uniqueResolutions.Add(i);
				if ((float)resolution2.width == num && (float)resolution2.height == num2)
				{
					m_resolution = m_uniqueResolutions.Count - 1;
				}
			}
		}
		Slider("Resolution").Ratio = (float)m_resolution / (float)(m_uniqueResolutions.Count - 1);
		m_fullScreenMode = Screen.fullScreenMode;
	}

	public void UpdateText()
	{
		Slider("Volume").Text = string.Format(SystemText.Localize("Volume: {0}"), Mathf.RoundToInt(QuestScript.Settings.Volume * 100f));
		Button("Language").Text = string.Format(SystemText.Localize("Language: {0}"), QuestScript.Settings.LanguageData.m_description);
		if (QuestScript.Settings.LockCursor == CursorLockMode.Confined)
		{
			Button("LockCursor").Text = SystemText.Localize("Lock Cursor: On");
		}
		else
		{
			Button("LockCursor").Text = SystemText.Localize("Lock Cursor: Off");
		}
		switch (QuestScript.Settings.DialogDisplay)
		{
		case QuestSettings.eDialogDisplay.TextAndSpeech:
			Button("Subtitles").Text = SystemText.Localize("Speech + Subtitles");
			break;
		case QuestSettings.eDialogDisplay.SpeechOnly:
			Button("Subtitles").Text = SystemText.Localize("Speech Only");
			break;
		case QuestSettings.eDialogDisplay.TextOnly:
			Button("Subtitles").Text = SystemText.Localize("Subtitles Only");
			break;
		}
		if (m_uniqueResolutions.IsIndexValid(m_resolution) && Screen.resolutions.IsIndexValid(m_uniqueResolutions[m_resolution]))
		{
			Resolution resolution = Screen.resolutions[m_uniqueResolutions[m_resolution]];
			Slider("Resolution").Text = $"{resolution.width}x{resolution.height}";
		}
		switch (m_fullScreenMode)
		{
		case FullScreenMode.FullScreenWindow:
			Button("Fullscreen").Text = "Fullscreen";
			break;
		case FullScreenMode.ExclusiveFullScreen:
			Button("Fullscreen").Text = "Exclusive Fullscreen";
			break;
		case FullScreenMode.Windowed:
			Button("Fullscreen").Text = "Windowed";
			break;
		}
		if (m_resDirty)
		{
			Button("Apply").Show();
		}
		Button("Apply").Clickable = m_resDirty;
	}

	private IEnumerator OnDragVolume(IGuiControl control)
	{
		QuestScript.Settings.Volume = Slider("Volume").Ratio;
		UpdateText();
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnClickVolume(IGuiControl control)
	{
		SystemAudio.Play("Bucket");
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnDragResolution(IGuiControl control)
	{
		m_resDirty = true;
		m_resolution = Mathf.RoundToInt(Slider("Resolution").Ratio * (float)(m_uniqueResolutions.Count - 1));
		UpdateText();
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnClickFullscreen(IGuiControl control)
	{
		switch (m_fullScreenMode)
		{
		case FullScreenMode.Windowed:
			m_fullScreenMode = FullScreenMode.FullScreenWindow;
			break;
		case FullScreenMode.FullScreenWindow:
			m_fullScreenMode = FullScreenMode.ExclusiveFullScreen;
			break;
		case FullScreenMode.ExclusiveFullScreen:
			m_fullScreenMode = FullScreenMode.Windowed;
			break;
		}
		m_resDirty = true;
		UpdateText();
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnClickApply(IGuiControl control)
	{
		if (m_uniqueResolutions.IsIndexValid(m_resolution) && Screen.resolutions.IsIndexValid(m_uniqueResolutions[m_resolution]))
		{
			Resolution resolution = Screen.resolutions[m_uniqueResolutions[m_resolution]];
			Screen.SetResolution(resolution.width, resolution.height, m_fullScreenMode, 0);
		}
		m_resDirty = false;
		UpdateText();
		QuestScript.E.DelayedInvoke(0.1f, UpdateResolutionList);
		yield return QuestScript.E.Break;
	}

	private void Update()
	{
		if (G.Options.HasFocus && Input.GetKeyUp(KeyCode.Escape))
		{
			G.Options.Hide();
		}
	}

	private IEnumerator OnClickSubtitles(IGuiControl control)
	{
		switch (QuestScript.Settings.DialogDisplay)
		{
		case QuestSettings.eDialogDisplay.TextAndSpeech:
			QuestScript.Settings.DialogDisplay = QuestSettings.eDialogDisplay.TextOnly;
			break;
		case QuestSettings.eDialogDisplay.TextOnly:
			QuestScript.Settings.DialogDisplay = QuestSettings.eDialogDisplay.SpeechOnly;
			break;
		case QuestSettings.eDialogDisplay.SpeechOnly:
			QuestScript.Settings.DialogDisplay = QuestSettings.eDialogDisplay.TextAndSpeech;
			break;
		}
		UpdateText();
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnClickLanguage(IGuiControl control)
	{
		QuestScript.Settings.LanguageId = (int)Mathf.Repeat(QuestScript.Settings.LanguageId + 1, QuestScript.Settings.GetLanguages().Length);
		UpdateText();
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnClickLockCursor(IGuiControl control)
	{
		QuestScript.Settings.LockCursor = ((QuestScript.Settings.LockCursor != CursorLockMode.Confined) ? CursorLockMode.Confined : CursorLockMode.None);
		UpdateText();
		yield return QuestScript.E.Break;
	}
}
