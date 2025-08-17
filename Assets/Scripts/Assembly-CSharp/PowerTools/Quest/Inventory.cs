using System;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

namespace PowerTools.Quest
{
	[Serializable]
	public class Inventory : IQuestClickable, IQuestScriptable, IInventory, IQuestSaveCachable
	{
		[TextArea(1, 10)]
		[SerializeField]
		private string m_description = "New Item";

		[Tooltip("Sprite animation for inventory in GUI")]
		[SerializeField]
		private string m_animGui = string.Empty;

		[Tooltip("Sprite animation for inventory cursor")]
		[SerializeField]
		private string m_animCursor = string.Empty;

		[Tooltip("Sprite animation for inventory cursor when not hovering over clickable")]
		[SerializeField]
		private string m_animCursorInactive = string.Empty;

		[Tooltip("When picking up multiple, do you get multiple in your inventory, or do they just stack up")]
		[SerializeField]
		private bool m_stack;

		[ReadOnly]
		[SerializeField]
		private string m_scriptName = "InventoryNew";

		[ReadOnly]
		[SerializeField]
		private string m_scriptClass = "InventoryNew";

		private string m_cursor = string.Empty;

		private QuestScript m_script;

		private GameObject m_prefab;

		private bool m_everCollected;

		private int m_useCount;

		private int m_lookCount;

		private bool m_saveDirty = true;

		public eQuestClickableType ClickableType => eQuestClickableType.Inventory;

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

		public string Anim
		{
			get
			{
				return m_animGui;
			}
			set
			{
				AnimGui = value;
				AnimCursor = value;
				AnimCursorInactive = value;
			}
		}

		public string AnimGui
		{
			get
			{
				return m_animGui;
			}
			set
			{
				m_animGui = value;
			}
		}

		public string AnimCursor
		{
			get
			{
				return m_animCursor;
			}
			set
			{
				m_animCursor = value;
			}
		}

		public string AnimCursorInactive
		{
			get
			{
				return m_animCursorInactive;
			}
			set
			{
				m_animCursorInactive = value;
			}
		}

		public bool Stack
		{
			get
			{
				return m_stack;
			}
			set
			{
				m_stack = value;
			}
		}

		public string ScriptName => m_scriptName;

		public Inventory Data => this;

		public bool FirstUse => UseCount == 0;

		public bool FirstLook => LookCount == 0;

		public int UseCount => m_useCount - (Singleton<PowerQuest>.Get.GetInteractionInProgress(this, eQuestVerb.Use) ? 1 : 0);

		public int LookCount => m_lookCount - (Singleton<PowerQuest>.Get.GetInteractionInProgress(this, eQuestVerb.Look) ? 1 : 0);

		public bool SaveDirty
		{
			get
			{
				return m_saveDirty;
			}
			set
			{
				m_saveDirty = value;
			}
		}

		public Vector2 WalkToPoint
		{
			get
			{
				return Vector2.zero;
			}
			set
			{
			}
		}

		public Vector2 LookAtPoint
		{
			get
			{
				return Vector2.zero;
			}
			set
			{
			}
		}

		public float Baseline
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public bool Clickable
		{
			get
			{
				return true;
			}
			set
			{
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

		public Vector2 Position => Vector2.zero;

		public MonoBehaviour Instance => null;

		public bool Active
		{
			get
			{
				return Singleton<PowerQuest>.Get.GetPlayer().ActiveInventory == this;
			}
			set
			{
				if (Singleton<PowerQuest>.Get.GetPlayer().ActiveInventory == this != value)
				{
					Singleton<PowerQuest>.Get.GetPlayer().ActiveInventory = (value ? this : null);
				}
			}
		}

		public bool Owned
		{
			get
			{
				return Singleton<PowerQuest>.Get.GetPlayer().HasInventory(this);
			}
			set
			{
				if (value && !Owned)
				{
					Add();
				}
				else if (!value && Owned)
				{
					Remove(Mathf.RoundToInt(Singleton<PowerQuest>.Get.GetPlayer().GetInventoryItemCount()));
				}
			}
		}

		public bool EverCollected => m_everCollected;

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

		public string GetAnimGui()
		{
			return m_animGui;
		}

		public string GetAnimCursor()
		{
			return m_animCursor;
		}

		public QuestScript GetScript()
		{
			return m_script;
		}

		public IQuestScriptable GetScriptable()
		{
			return this;
		}

		public T GetScript<T>() where T : InventoryScript<T>
		{
			if (m_script == null)
			{
				return null;
			}
			return m_script as T;
		}

		public string GetScriptName()
		{
			return m_scriptName;
		}

		public string GetScriptClassName()
		{
			return m_scriptClass;
		}

		public void HotLoadScript(Assembly assembly)
		{
			QuestUtils.HotSwapScript(ref m_script, m_scriptClass, assembly);
		}

		public GameObject GetPrefab()
		{
			return m_prefab;
		}

		public void Add(int quantity = 1)
		{
			Singleton<PowerQuest>.Get.GetPlayer().AddInventory(this, quantity);
		}

		public void AddAsActive(int quantity = 1)
		{
			Add(quantity);
			Active = true;
		}

		public void Remove(int quantity = 1)
		{
			Singleton<PowerQuest>.Get.GetPlayer().RemoveInventory(this, quantity);
		}

		public void SetActive()
		{
			Singleton<PowerQuest>.Get.GetPlayer().ActiveInventory = this;
		}

		public void OnCollected()
		{
			m_everCollected = true;
		}

		public static Inventory FromInterface(IInventory inv)
		{
			return inv as Inventory;
		}

		public void EditorInitialise(string name)
		{
			m_scriptName = name;
			m_scriptClass = PowerQuest.STR_INVENTORY + name;
			m_description = name;
			m_animGui = name;
			m_animCursor = name;
			m_animCursorInactive = name;
		}

		public void EditorRename(string name)
		{
			m_scriptName = name;
			m_scriptClass = "Inventory" + name;
		}

		public void OnPostRestore(int version, GameObject prefab)
		{
			m_prefab = prefab;
			if (m_script == null)
			{
				m_script = QuestUtils.ConstructByName<QuestScript>(m_scriptClass);
			}
			SaveDirty = Active;
		}

		public void Initialise(GameObject prefab)
		{
			m_prefab = prefab;
			m_script = QuestUtils.ConstructByName<QuestScript>(m_scriptClass);
		}

		[OnDeserializing]
		private void CopyDefaults(StreamingContext sc)
		{
			QuestUtils.InitWithDefaults(this);
		}
	}
}
