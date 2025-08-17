using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using PowerTools.QuestGui;
using UnityEngine;
using UnityEngine.Serialization;

namespace PowerTools.Quest
{
	[Serializable]
	public class Gui : IQuestClickable, IQuestScriptable, IGui
	{
		[Tooltip("The sort order for the gui, like other hotspots, LOWER is IN-FRONT")]
		[Range(-319f, 319f)]
		[SerializeField]
		private float m_baseline = -1f;

		[Tooltip("Whether the gui is starts on")]
		[SerializeField]
		private bool m_visible = true;

		[Tooltip("Whether the gui hides itself during cutscenes")]
		[SerializeField]
		private bool m_visibleInCutscenes = true;

		[Tooltip("If true, the gui blocks input to the game or any guis behind it. Useful for popups and things")]
		[SerializeField]
		private bool m_modal;

		[Header("When Shown...")]
		[Tooltip("Whether the gui should pause the game when it's visible")]
		[SerializeField]
		private bool m_pauseGame;

		[Tooltip("Whether guis behind this one should be hidden")]
		[SerializeField]
		private bool m_hideObscuredGuis;

		[Tooltip("Other guis to hide when gui is visible")]
		[FormerlySerializedAs("m_hideGuisWhenActive")]
		[SerializeField]
		private string[] m_hideSpecificGuis;

		[Header("Mouse over")]
		[SerializeField]
		private string m_description = string.Empty;

		[Tooltip("If set, changes the name of the cursor when moused over")]
		[SerializeField]
		private string m_cursor = string.Empty;

		[Tooltip("Whether to show the inventory cursor while active")]
		[SerializeField]
		private bool m_allowInventoryCursor;

		[Header("Read only")]
		[ReadOnly]
		[SerializeField]
		private Vector2 m_position = Vector2.zero;

		[ReadOnly]
		[SerializeField]
		private string m_scriptName = "GuiNew";

		[ReadOnly]
		[SerializeField]
		private string m_scriptClass = "GuiNew";

		private bool m_clickable = true;

		private bool m_hiddenBySystem;

		private QuestScript m_script;

		private GameObject m_prefab;

		private GuiComponent m_instance;

		private List<GuiControl> m_controls = new List<GuiControl>();

		private IGuiControl m_lastFocusedControl;

		public eQuestClickableType ClickableType => eQuestClickableType.Gui;

		public string ScriptName => m_scriptName;

		public MonoBehaviour Instance => m_instance;

		public Gui Data => this;

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

		public bool Visible
		{
			get
			{
				return m_visible;
			}
			set
			{
				bool num = m_visible != value;
				m_visible = value;
				if (num && m_visible && !VisibleInCutscenes && Singleton<PowerQuest>.Get.GetBlocked())
				{
					HiddenBySystem = true;
				}
				if (num)
				{
					OnVisibilityChanged();
				}
			}
		}

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

		public bool HiddenBySystem
		{
			get
			{
				return m_hiddenBySystem;
			}
			set
			{
				bool num = IsActuallyVisible();
				m_hiddenBySystem = value;
				if (num != IsActuallyVisible())
				{
					OnVisibilityChanged();
				}
			}
		}

		public string[] HideSpecificGuis => m_hideSpecificGuis;

		public bool Modal
		{
			get
			{
				return m_modal;
			}
			set
			{
				m_modal = value;
			}
		}

		public bool PauseGame
		{
			get
			{
				return m_pauseGame;
			}
			set
			{
				m_pauseGame = value;
			}
		}

		public bool VisibleInCutscenes
		{
			get
			{
				return m_visibleInCutscenes;
			}
			set
			{
				m_visibleInCutscenes = value;
			}
		}

		public bool HideObscuredGuis
		{
			get
			{
				return m_hideObscuredGuis;
			}
			set
			{
				m_hideObscuredGuis = value;
			}
		}

		public bool ObscuredByModal => Singleton<PowerQuest>.Get.GetIsGuiObscuredByModal(this);

		public Vector2 Position
		{
			get
			{
				return m_position;
			}
			set
			{
				m_position = value;
				if (m_instance != null)
				{
					m_instance.transform.position = m_position.WithZ(m_instance.transform.position.z);
				}
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
				if (m_baseline != value)
				{
					m_baseline = value;
					if (m_instance != null)
					{
						m_instance.OnSetBaseline();
					}
				}
			}
		}

		public bool AllowInventoryCursor => m_allowInventoryCursor;

		public bool HasFocus => Singleton<PowerQuest>.Get.GetFocusedGui() == this;

		public IGuiControl LastFocusedControl
		{
			get
			{
				return m_lastFocusedControl;
			}
			set
			{
				m_lastFocusedControl = value;
			}
		}

		public GuiControl GetControl(string name)
		{
			GuiControl guiControl = QuestUtils.FindScriptableMono(m_controls, name);
			if (guiControl == null)
			{
				Debug.LogError("Gui Control '" + name + "' doesn't exist in " + ScriptName);
			}
			return guiControl;
		}

		public bool HasControl(string name)
		{
			return m_controls.Exists((GuiControl prop) => prop != null && string.Equals(prop.ScriptName, name, StringComparison.OrdinalIgnoreCase));
		}

		public void Show()
		{
			ResetNavigation();
			QuestMenuManager.Get.IgnoreNextKeypress();
			Visible = true;
			Clickable = true;
		}

		public void ShowAtFront()
		{
			Show();
			float minBaseline = float.MinValue;
			Singleton<PowerQuest>.Get.GetGuis().ForEach(delegate(Gui item)
			{
				if (item != null && item.Baseline < minBaseline)
				{
					minBaseline = item.Baseline;
				}
			});
			if (minBaseline > float.MinValue)
			{
				Baseline = minBaseline - 1f;
			}
		}

		public void ShowAtBack()
		{
			Show();
			float minBaseline = float.MaxValue;
			Singleton<PowerQuest>.Get.GetGuis().ForEach(delegate(Gui item)
			{
				if (item != null && item.Baseline > minBaseline)
				{
					minBaseline = item.Baseline;
				}
			});
			if (minBaseline < float.MaxValue)
			{
				Baseline = minBaseline + 1f;
			}
		}

		public void ShowBehind(IGui gui)
		{
			Show();
			if (gui != null)
			{
				Baseline = gui.Baseline + 1f;
			}
		}

		public void ShowInfront(IGui gui)
		{
			Show();
			if (gui != null)
			{
				Baseline = gui.Baseline - 1f;
			}
		}

		public void Hide()
		{
			QuestMenuManager.Get.IgnoreNextKeypress();
			Visible = false;
			Clickable = false;
		}

		public void OnFocus()
		{
			if (Instance != null)
			{
				(Instance as GuiComponent).OnFocus();
			}
			if (QuestMenuManager.Get.KeyboardActive)
			{
				if (m_lastFocusedControl != null)
				{
					NavigateToControl(m_lastFocusedControl);
				}
				else
				{
					NavigateToControl(GetFirstClickableControl());
				}
			}
		}

		public void OnDefocus()
		{
			if (Instance != null)
			{
				(Instance as GuiComponent).OnDefocus();
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
				MethodInfo method = m_script.GetType().GetMethod("Initialise", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
				if (method != null)
				{
					method.Invoke(m_script, new object[1] { Data });
				}
			}
		}

		public T GetScript<T>() where T : GuiScript<T>
		{
			if (m_script == null)
			{
				return null;
			}
			return m_script as T;
		}

		public GameObject GetPrefab()
		{
			return m_prefab;
		}

		public GuiComponent GetInstance()
		{
			return m_instance;
		}

		public void SetInstance(GuiComponent instance)
		{
			m_instance = instance;
			m_instance.SetData(this);
			GuiControl[] componentsInChildren = m_instance.GetComponentsInChildren<GuiControl>(includeInactive: true);
			m_controls = new List<GuiControl>(componentsInChildren);
			GuiControl[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetGui(this);
			}
			if (Visible && !Singleton<PowerQuest>.Get.GetRestoringGame())
			{
				Singleton<PowerQuest>.Get.OnGuiShown(this);
			}
		}

		public void OnInteraction(eQuestVerb verb)
		{
		}

		public void OnCancelInteraction(eQuestVerb verb)
		{
		}

		public void EditorInitialise(string name)
		{
			m_scriptName = name;
			m_scriptClass = "Gui" + name;
			m_description = string.Empty;
		}

		public void EditorRename(string name)
		{
			m_scriptName = name;
			m_scriptClass = "Gui" + name;
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
				MethodInfo method = m_script.GetType().GetMethod("Initialise", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
				if (method != null)
				{
					method.Invoke(m_script, new object[1] { Data });
				}
			}
			if (m_instance != null)
			{
				m_instance.gameObject.SetActive(value: false);
				m_instance.OnSetBaseline();
			}
			OnVisibilityChanged();
		}

		public void Initialise(GameObject prefab)
		{
			m_prefab = prefab;
			m_script = QuestUtils.ConstructByName<QuestScript>(m_scriptClass);
			if (m_script != null)
			{
				MethodInfo method = m_script.GetType().GetMethod("Initialise", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
				if (method != null)
				{
					method.Invoke(m_script, new object[1] { Data });
				}
			}
		}

		public bool Navigate(eGuiNav key)
		{
			if (!QuestMenuManager.Get.ProcessKeyboardInput(key))
			{
				return false;
			}
			GuiControl guiControl = Singleton<PowerQuest>.Get.GetFocusedGuiControl() as GuiControl;
			if (guiControl != null && guiControl.isActiveAndEnabled && guiControl.HandleKeyboardInput(key))
			{
				return true;
			}
			if (key == eGuiNav.Up || key == eGuiNav.Down || key == eGuiNav.Left || key == eGuiNav.Right)
			{
				NavigateToControl(GetNextNavControl(key));
			}
			return true;
		}

		private bool IsActuallyVisible()
		{
			if (Visible)
			{
				return !HiddenBySystem;
			}
			return false;
		}

		private void OnVisibilityChanged()
		{
			if (m_instance != null)
			{
				m_instance.gameObject.SetActive(IsActuallyVisible());
			}
			if (PauseGame)
			{
				if (IsActuallyVisible())
				{
					Singleton<PowerQuest>.Get.Pause(m_scriptName);
				}
				else
				{
					Singleton<PowerQuest>.Get.UnPause(m_scriptName);
				}
			}
		}

		public void ResetNavigation()
		{
			m_lastFocusedControl = null;
		}

		public IGuiControl GetNextNavControl(eGuiNav dir)
		{
			ClearRemovedControls();
			GuiControl guiControl = Singleton<PowerQuest>.Get.GetFocusedGuiControl() as GuiControl;
			if (guiControl == null && m_lastFocusedControl != null)
			{
				return m_lastFocusedControl;
			}
			if (guiControl == null)
			{
				return GetFirstClickableControl();
			}
			Container container = null;
			foreach (GuiControl control in m_controls)
			{
				Container container2 = control as Container;
				if (container2 != null && container2.GetIsControlInGrid(guiControl))
				{
					container = container2;
					IGuiControl nextControl = container2.GetNextControl(guiControl, dir);
					if (nextControl != null)
					{
						return nextControl;
					}
					break;
				}
			}
			GuiControl guiControl2 = null;
			float num = float.MaxValue;
			float num2 = float.MaxValue;
			RectCentered navHotspotRect = GetNavHotspotRect(guiControl);
			foreach (GuiControl control2 in m_controls)
			{
				if (control2 == guiControl || !control2.Clickable || (container != null && container.GetIsControlInGrid(control2)))
				{
					continue;
				}
				RectCentered navHotspotRect2 = GetNavHotspotRect(control2);
				if (dir == eGuiNav.Right && (navHotspotRect2.MaxY < navHotspotRect.MinY || navHotspotRect2.MinY < navHotspotRect.MaxY))
				{
					float num3 = navHotspotRect2.MinX - navHotspotRect.MaxX;
					if (num3 > 0f && num3 < num)
					{
						num = num3;
						guiControl2 = control2;
					}
				}
				if (dir == eGuiNav.Left && (navHotspotRect2.MaxY < navHotspotRect.MinY || navHotspotRect2.MinY < navHotspotRect.MaxY))
				{
					float num4 = 0f - (navHotspotRect2.MaxX - navHotspotRect.MinX);
					if (num4 > 0f && num4 < num)
					{
						num = num4;
						guiControl2 = control2;
					}
				}
				if (dir == eGuiNav.Up)
				{
					float num5 = navHotspotRect2.MinY - navHotspotRect.MaxY;
					if (Utils.Approximately(num5, num, 1f))
					{
						float sqrMagnitude = (navHotspotRect.Center - navHotspotRect2.Center).sqrMagnitude;
						if (sqrMagnitude < num2)
						{
							num = num5;
							guiControl2 = control2;
							num2 = sqrMagnitude;
						}
					}
					else if (num5 > 0f && num5 < num)
					{
						num = num5;
						guiControl2 = control2;
						num2 = (navHotspotRect.Center - navHotspotRect2.Center).sqrMagnitude;
					}
				}
				if (dir != eGuiNav.Down)
				{
					continue;
				}
				float num6 = navHotspotRect.MinY - navHotspotRect2.MaxY;
				if (Utils.Approximately(num6, num, 1f))
				{
					float sqrMagnitude2 = (navHotspotRect.Center - navHotspotRect2.Center).sqrMagnitude;
					if (sqrMagnitude2 < num2)
					{
						num = num6;
						guiControl2 = control2;
						num2 = sqrMagnitude2;
					}
				}
				else if (num6 > 0f && num6 < num)
				{
					num = num6;
					guiControl2 = control2;
					num2 = (navHotspotRect.Center - navHotspotRect2.Center).sqrMagnitude;
				}
			}
			if (guiControl2 != null)
			{
				return guiControl2;
			}
			return guiControl;
		}

		private RectCentered GetNavHotspotRect(GuiControl control)
		{
			if (control is Button || control is Slider || control is TextField)
			{
				Collider2D component = control.GetComponent<Collider2D>();
				if (component != null)
				{
					return new RectCentered(component.bounds);
				}
			}
			return control.GetRect();
		}

		public void NavigateToControl(IGuiControl control)
		{
			m_lastFocusedControl = control;
			QuestMenuManager.Get.SetKeyboardFocus(control);
		}

		private GuiControl GetFirstClickableControl()
		{
			return m_controls.Find((GuiControl item) => item != null && item.Clickable);
		}

		private void ClearRemovedControls()
		{
			m_controls.RemoveAll((GuiControl item) => item == null);
		}

		[OnDeserializing]
		private void CopyDefaults(StreamingContext sc)
		{
			QuestUtils.InitWithDefaults(this);
		}
	}
}
