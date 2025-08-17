using System;
using PowerTools.Quest;
using UnityEngine;

namespace PowerTools.QuestGui
{
	[Serializable]
	[AddComponentMenu("Quest Gui/Label")]
	public class Label : GuiControl, ILabel, IGuiControl
	{
		private QuestText m_questText;

		public QuestText TextComponent
		{
			get
			{
				if (m_questText == null)
				{
					m_questText = GetComponentInChildren<QuestText>();
				}
				return m_questText;
			}
		}

		public string Text
		{
			get
			{
				if (!(TextComponent != null))
				{
					return null;
				}
				return TextComponent.text;
			}
			set
			{
				if (TextComponent != null)
				{
					TextComponent.text = value;
				}
			}
		}

		public Color Color
		{
			get
			{
				if (!(TextComponent != null))
				{
					return Color.white;
				}
				return TextComponent.color;
			}
			set
			{
				if (TextComponent != null)
				{
					TextComponent.color = value;
				}
			}
		}

		public IQuestClickable IClickable => this;

		public override RectCentered GetRect(Transform excludeChild = null)
		{
			RectCentered result = GuiUtils.CalculateGuiRectFromRenderer(base.transform, includeChildren: false, GetComponent<MeshRenderer>(), excludeChild);
			result.Transform(base.transform);
			return result;
		}

		public QuestText GetQuestText()
		{
			return m_questText;
		}

		private void Awake()
		{
			m_questText = GetComponentInChildren<QuestText>();
		}
	}
}
