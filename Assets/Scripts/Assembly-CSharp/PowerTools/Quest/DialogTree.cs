using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

namespace PowerTools.Quest
{
	[Serializable]
	public class DialogTree : IQuestScriptable, IDialogTree, IQuestSaveCachable
	{
		[SerializeField]
		private List<DialogOption> m_options = new List<DialogOption>();

		[ReadOnly]
		[SerializeField]
		private string m_scriptName = "DialogNew";

		[ReadOnly]
		[SerializeField]
		private string m_scriptClass = "DialogNew";

		private int m_timesShown;

		private QuestScript m_script;

		private GameObject m_prefab;

		private DialogTreeComponent m_instance;

		private bool m_saveDirty = true;

		public string ScriptName => m_scriptName;

		public DialogTree Data => this;

		public List<DialogOption> Options => m_options;

		public int NumOptionsEnabled
		{
			get
			{
				int result = 0;
				m_options.ForEach(delegate(DialogOption item)
				{
					if (item.Visible)
					{
						int num = result + 1;
						result = num;
					}
				});
				return result;
			}
		}

		public int NumOptionsUnused
		{
			get
			{
				int result = 0;
				m_options.ForEach(delegate(DialogOption item)
				{
					if (!item.Used)
					{
						int num = result + 1;
						result = num;
					}
				});
				return result;
			}
		}

		public bool FirstTimeShown => m_timesShown <= 1;

		public int TimesShown => m_timesShown;

		public IDialogOption this[int index] => this[index.ToString()];

		public IDialogOption this[string name]
		{
			get
			{
				DialogOption dialogOption = m_options.Find((DialogOption item) => string.Compare(item.Name, name, ignoreCase: true) == 0);
				if (dialogOption == null)
				{
					Debug.LogError("Failed to find option " + name + " in dialog " + m_scriptName);
				}
				return dialogOption;
			}
		}

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

		public QuestScript GetScript()
		{
			return m_script;
		}

		public IQuestScriptable GetScriptable()
		{
			return this;
		}

		public T GetScript<T>() where T : DialogTreeScript<T>
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
			if (m_script != null)
			{
				m_script.GetType().GetField("m_data", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).SetValue(m_script, Data);
			}
		}

		public GameObject GetPrefab()
		{
			return m_prefab;
		}

		public DialogTreeComponent GetInstance()
		{
			return m_instance;
		}

		public void SetInstance(DialogTreeComponent instance)
		{
			m_instance = instance;
			m_instance.SetData(this);
		}

		public void OnStart()
		{
			m_timesShown++;
		}

		public void Start()
		{
			Singleton<PowerQuest>.Get.StartDialog(ScriptName);
		}

		public void Stop()
		{
			Singleton<PowerQuest>.Get.StopDialog();
		}

		public IDialogOption GetOption(string name)
		{
			return this[name];
		}

		public IDialogOption GetOption(int index)
		{
			return this[index];
		}

		public void OptionOn(params int[] id)
		{
			Array.ForEach(id, delegate(int item)
			{
				this[item].On();
			});
		}

		public void OptionOff(params int[] id)
		{
			Array.ForEach(id, delegate(int item)
			{
				this[item].Off();
			});
		}

		public void OptionOffForever(params int[] id)
		{
			Array.ForEach(id, delegate(int item)
			{
				this[item].OffForever();
			});
		}

		public void OptionOn(params string[] id)
		{
			Array.ForEach(id, delegate(string item)
			{
				this[item].On();
			});
		}

		public void OptionOff(params string[] id)
		{
			Array.ForEach(id, delegate(string item)
			{
				this[item].Off();
			});
		}

		public void OptionOffForever(params string[] id)
		{
			Array.ForEach(id, delegate(string item)
			{
				this[item].OffForever();
			});
		}

		public bool GetOptionOn(int option)
		{
			return this[option]?.Visible ?? false;
		}

		public bool GetOptionOffForever(int option)
		{
			return this[option]?.Disabled ?? false;
		}

		public bool GetOptionUsed(int option)
		{
			return this[option]?.Used ?? false;
		}

		public bool GetOptionOn(string option)
		{
			return this[option]?.Visible ?? false;
		}

		public bool GetOptionOffForever(string option)
		{
			return this[option]?.Disabled ?? false;
		}

		public bool GetOptionUsed(string option)
		{
			return this[option]?.Used ?? false;
		}

		public void EditorInitialise(string name)
		{
			m_scriptName = name;
			m_scriptClass = "Dialog" + name;
		}

		public void EditorRename(string name)
		{
			m_scriptName = name;
			m_scriptClass = "Dialog" + name;
		}

		public void OnPostRestore(int version, GameObject prefab)
		{
			m_prefab = prefab;
			if (m_script == null)
			{
				m_script = QuestUtils.ConstructByName<QuestScript>(m_scriptClass);
			}
			if (m_script != null)
			{
				m_script.GetType().GetField("m_data", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).SetValue(m_script, Data);
			}
			SaveDirty = Singleton<PowerQuest>.Get.GetCurrentDialog() == this;
		}

		public void Initialise(GameObject prefab)
		{
			m_prefab = prefab;
			m_script = QuestUtils.ConstructByName<QuestScript>(m_scriptClass);
			List<DialogOption> options = m_options;
			m_options = new List<DialogOption>(options.Count);
			for (int i = 0; i < options.Count; i++)
			{
				m_options.Add(new DialogOption());
				QuestUtils.CopyFields(m_options[i], options[i]);
			}
			if (m_script != null)
			{
				m_script.GetType().GetField("m_data", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).SetValue(m_script, Data);
			}
		}

		[OnDeserializing]
		private void CopyDefaults(StreamingContext sc)
		{
			QuestUtils.InitWithDefaults(this);
		}
	}
}
