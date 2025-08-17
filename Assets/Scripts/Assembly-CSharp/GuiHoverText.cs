using PowerTools.Quest;
using UnityEngine;

public class GuiHoverText : GuiScript<GuiHoverText>
{
	private Vector2 m_offsetFromCursor = new Vector2(35f, -35f);

	private float m_offsetFromEdge = 20f;

	private void Update()
	{
		UpdateTextOnCursorPos();
		Label("Text").Text = QuestScript.E.GetMouseOverDescription();
		Label("Text").Visible = !QuestScript.E.GetBlocked();
	}

	private void UpdateTextOnCursorPos()
	{
		RectCentered rect = (Label("Text") as GuiControl).GetRect();
		float num = QuestScript.E.GetCameraGui().GetWidth() * 0.5f - m_offsetFromEdge;
		float num2 = QuestScript.E.GetCameraGui().GetHeight() * 0.5f - m_offsetFromEdge;
		base.Gui.Position = QuestScript.E.GetMousePositionGui() + m_offsetFromCursor;
		base.Gui.Position = new Vector2(Mathf.Min(base.Gui.Position.x, num - rect.Width), Mathf.Max(base.Gui.Position.y, 0f - num2 + rect.Height));
	}
}
