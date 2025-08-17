using System;
using UnityEngine;

namespace PowerTools.Quest
{
	[Serializable]
	public class DialogOption : IDialogOption, IQuestClickable
	{
		[SerializeField]
		private string m_name = string.Empty;

		[SerializeField]
		[Multiline]
		private string m_text = string.Empty;

		[SerializeField]
		private bool m_visible = true;

		[SerializeField]
		[HideInInspector]
		private bool m_disabled;

		[SerializeField]
		[HideInInspector]
		private bool m_used;

		[SerializeField]
		[HideInInspector]
		private int m_timesUsed;

		private int m_inlineId = -1;

		public string Name
		{
			get
			{
				return m_name;
			}
			set
			{
				m_name = value;
			}
		}

		public string Text
		{
			get
			{
				return m_text;
			}
			set
			{
				m_text = value;
			}
		}

		public string Description
		{
			get
			{
				return m_text;
			}
			set
			{
				m_text = value;
			}
		}

		public bool Visible
		{
			get
			{
				return m_visible;
			}
			set
			{
				m_visible = value;
			}
		}

		public bool Disabled
		{
			get
			{
				return m_disabled;
			}
			set
			{
				m_disabled = value;
			}
		}

		public bool Used
		{
			get
			{
				return m_used;
			}
			set
			{
				m_used = value;
			}
		}

		public int TimesUsed
		{
			get
			{
				return m_timesUsed;
			}
			set
			{
				m_timesUsed = value;
			}
		}

		public bool FirstUse => m_timesUsed <= 1;

		public int InlineId
		{
			get
			{
				return m_inlineId;
			}
			set
			{
				m_inlineId = value;
			}
		}

		public eQuestClickableType ClickableType => eQuestClickableType.Gui;

		public MonoBehaviour Instance => null;

		public string ScriptName => m_name;

		public Vector2 Position => Vector2.zero;

		public Vector2 WalkToPoint { get; set; }

		public Vector2 LookAtPoint { get; set; }

		public float Baseline { get; set; }

		public bool Clickable { get; set; }

		public string Cursor
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public void Show()
		{
			if (!Disabled)
			{
				Visible = true;
			}
		}

		public void Hide()
		{
			Visible = false;
		}

		public void HideForever()
		{
			Visible = false;
			Disabled = true;
		}

		public void On()
		{
			if (!Disabled)
			{
				Visible = true;
			}
		}

		public void Off()
		{
			Visible = false;
		}

		public void OffForever()
		{
			Visible = false;
			Disabled = true;
		}

		public void OnInteraction(eQuestVerb verb)
		{
		}

		public void OnCancelInteraction(eQuestVerb verb)
		{
		}

		public QuestScript GetScript()
		{
			return null;
		}

		public IQuestScriptable GetScriptable()
		{
			return null;
		}
	}
}
