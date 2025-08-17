using System;
using System.Collections;
using PowerTools.Quest;
using UnityEngine;

public class GuiPrompt : GuiScript<GuiPrompt>
{
	private Action m_onOk;

	private Action m_onCancel;

	private bool m_result = true;

	public bool Result => m_result;

	public bool OkClicked => m_result;

	public bool CancelClicked => !m_result;

	private void OnShow()
	{
	}

	private void Update()
	{
	}

	public void Show(string text, string buttonOk, string buttonCancel, Action onOk = null, Action onCancel = null)
	{
		if (base.Data.Visible)
		{
			Debug.LogWarning("Another prompt is already showing!");
		}
		Label("Text").Text = text;
		m_onOk = onOk;
		Button("BtnOk").Text = buttonOk;
		if (buttonCancel != null)
		{
			m_onCancel = onCancel;
			Button("BtnCancel").Text = buttonCancel;
			Button("BtnCancel").Show();
		}
		else
		{
			Button("BtnCancel").Hide();
		}
		base.Data.ShowAtFront();
	}

	public void Show(string text, string buttonOk, Action onOk = null)
	{
		Show(text, buttonOk, null, onOk);
	}

	public IEnumerator WaitForPrompt(string text, string buttonOk)
	{
		Show(text, buttonOk);
		yield return QuestScript.E.WaitForGui(base.Data);
	}

	public IEnumerator WaitForPrompt(string text, string buttonOk, string buttonCancel)
	{
		Show(text, buttonOk, buttonCancel);
		yield return QuestScript.E.WaitForGui(base.Data);
	}

	private IEnumerator OnClickBtnOk(IGuiControl control)
	{
		m_result = true;
		Action onOk = m_onOk;
		m_onOk = null;
		m_onCancel = null;
		base.Data.Hide();
		onOk?.Invoke();
		yield return QuestScript.E.Break;
	}

	private IEnumerator OnClickBtnCancel(IGuiControl control)
	{
		m_result = false;
		Action onCancel = m_onCancel;
		m_onOk = null;
		m_onOk = null;
		m_onCancel = null;
		base.Data.Hide();
		onCancel?.Invoke();
		yield return QuestScript.E.Break;
	}
}
