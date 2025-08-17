using System;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

namespace PowerTools.Quest
{
	[Serializable]
	public class Hotspot : IQuestClickable, IHotspot, IQuestClickableInterface, IQuestScriptable
	{
		[Header("Mouse-over Defaults")]
		[TextArea(1, 10)]
		[SerializeField]
		private string m_description = "New Hotspot";

		[Tooltip("If set, changes the name of the cursor when moused over")]
		[SerializeField]
		private string m_cursor = "";

		[Header("Starting State")]
		[Tooltip("Whether clicking on hotspot triggers an event")]
		[SerializeField]
		private bool m_clickable = true;

		[Header("Editable in Scene")]
		[Tooltip("Handles the picking order of the hotspot (lower is picked first, as it's infront, same as objects/characters")]
		[SerializeField]
		private float m_baseline;

		[SerializeField]
		private Vector2 m_walkToPoint = Vector2.zero;

		[SerializeField]
		private Vector2 m_lookAtPoint = Vector2.zero;

		[ReadOnly]
		[SerializeField]
		private string m_scriptName = "HotspotNew";

		private HotspotComponent m_instance;

		private int m_useCount;

		private int m_lookCount;

		public eQuestClickableType ClickableType => eQuestClickableType.Hotspot;

		public string Description
		{
			get
			{
				return m_description;
			}
			set
			{
				m_description = value;
			}
		}

		public string ScriptName => m_scriptName;

		public MonoBehaviour Instance => m_instance;

		public Hotspot Data => this;

		public IQuestClickable IClickable => this;

		public bool Clickable
		{
			get
			{
				return m_clickable;
			}
			set
			{
				m_clickable = value;
			}
		}

		public float Baseline
		{
			get
			{
				return m_baseline;
			}
			set
			{
				m_baseline = value;
			}
		}

		public Vector2 WalkToPoint
		{
			get
			{
				return m_walkToPoint;
			}
			set
			{
				m_walkToPoint = value;
			}
		}

		public Vector2 LookAtPoint
		{
			get
			{
				return m_lookAtPoint;
			}
			set
			{
				m_lookAtPoint = value;
			}
		}

		public string Cursor
		{
			get
			{
				return m_cursor;
			}
			set
			{
				m_cursor = value;
			}
		}

		public bool FirstUse => UseCount == 0;

		public bool FirstLook => LookCount == 0;

		public int UseCount => m_useCount - (Singleton<PowerQuest>.Get.GetInteractionInProgress(this, eQuestVerb.Use) ? 1 : 0);

		public int LookCount => m_lookCount - (Singleton<PowerQuest>.Get.GetInteractionInProgress(this, eQuestVerb.Look) ? 1 : 0);

		public Vector2 Position => Vector2.zero;

		public void Show()
		{
			Enable();
		}

		public void Hide()
		{
			Disable();
		}

		public void Enable()
		{
			Clickable = true;
		}

		public void Disable()
		{
			Clickable = false;
		}

		public HotspotComponent GetInstance()
		{
			return m_instance;
		}

		public void SetInstance(HotspotComponent instance)
		{
			m_instance = instance;
			instance.SetData(this);
		}

		public QuestScript GetScript()
		{
			if (Singleton<PowerQuest>.Get.GetCurrentRoom() != null)
			{
				return Singleton<PowerQuest>.Get.GetCurrentRoom().GetScript();
			}
			return null;
		}

		public IQuestScriptable GetScriptable()
		{
			return this;
		}

		public void OnInteraction(eQuestVerb verb)
		{
			switch (verb)
			{
			case eQuestVerb.Look:
				m_lookCount++;
				break;
			case eQuestVerb.Use:
				m_useCount++;
				break;
			}
		}

		public void OnCancelInteraction(eQuestVerb verb)
		{
			switch (verb)
			{
			case eQuestVerb.Look:
				m_lookCount--;
				break;
			case eQuestVerb.Use:
				m_useCount--;
				break;
			}
		}

		public void EditorInitialise(string name)
		{
			m_description = name;
			m_scriptName = name;
		}

		public void EditorRename(string name)
		{
			m_scriptName = name;
		}

		public string GetScriptName()
		{
			return m_scriptName;
		}

		public string GetScriptClassName()
		{
			return PowerQuest.STR_HOTSPOT + m_scriptName;
		}

		public void HotLoadScript(Assembly assembly)
		{
		}

		[OnDeserializing]
		private void CopyDefaults(StreamingContext sc)
		{
			QuestUtils.InitWithDefaults(this);
		}
	}
}
