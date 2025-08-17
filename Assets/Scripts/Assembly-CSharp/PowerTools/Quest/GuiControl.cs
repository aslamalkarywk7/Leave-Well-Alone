using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PowerTools.Quest
{
	public class GuiControl : MonoBehaviour, IQuestClickable, IQuestScriptable, IGuiControl
	{
		public enum eAlignHorizontal
		{
			None = 0,
			Left = 1,
			Center = 2,
			Right = 3
		}

		public enum eAlignVertical
		{
			None = 0,
			Top = 1,
			Middle = 2,
			Bottom = 3
		}

		[Range(-98f, 98f)]
		[SerializeField]
		protected float m_baseline;

		[SerializeField]
		protected bool m_visible = true;

		protected Gui m_gui;

		protected GuiComponent m_guiComponent;

		public Action CallbackOnFocus;

		public Action CallbackOnDefocus;

		public Action CallbackOnKeyboardFocus;

		public Action CallbackOnKeyboardDefocus;

		private float m_alpha = 1f;

		private static readonly Regex REGEX_SANITIZE = new Regex("(\\W|_)+", RegexOptions.Compiled);

		private static readonly string REGEX_REPLACE = "";

		private static readonly string STR_UNINSTNATIATED = "UninstantiatedGui";

		public virtual RectCentered CustomSize
		{
			get
			{
				return RectCentered.zero;
			}
			set
			{
			}
		}

		public bool Visible
		{
			get
			{
				return base.gameObject.activeSelf;
			}
			set
			{
				bool visible = m_visible;
				m_visible = value;
				base.gameObject.SetActive(value);
				if (visible != m_visible)
				{
					_ = m_visible;
				}
			}
		}

		public Gui GuiData => m_gui;

		public GuiComponent GuiComponent
		{
			get
			{
				if (m_guiComponent == null)
				{
					m_guiComponent = GetComponentInParent<GuiComponent>();
				}
				return m_guiComponent;
			}
		}

		public bool Focused => Singleton<PowerQuest>.Get.GetFocusedGuiControl() == this;

		public bool HasKeyboardFocus
		{
			get
			{
				return Singleton<PowerQuest>.Get.GetKeyboardFocus() == this;
			}
			set
			{
				if (value)
				{
					Singleton<PowerQuest>.Get.SetKeyboardFocus(this);
				}
				else if (HasKeyboardFocus)
				{
					Singleton<PowerQuest>.Get.SetKeyboardFocus(null);
				}
			}
		}

		public float Alpha
		{
			get
			{
				return m_alpha;
			}
			set
			{
				m_alpha = value;
				SpriteRenderer[] sprites = Instance.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
				QuestText[] texts = Instance.GetComponentsInChildren<QuestText>(includeInactive: true);
			}
		}

		public virtual Vector2 Position
		{
			get
			{
				return base.transform.position;
			}
			set
			{
				base.transform.position = value.WithZ(base.transform.position.z);
			}
		}

		public virtual float Baseline
		{
			get
			{
				return m_baseline;
			}
			set
			{
				m_baseline = value;
				if (Application.isPlaying)
				{
					UpdateBaseline();
				}
			}
		}

		public virtual bool Clickable
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public virtual string Description
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public virtual string Cursor
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public eQuestClickableType ClickableType => eQuestClickableType.Gui;

		public MonoBehaviour Instance => this;

		public string ScriptName
		{
			get
			{
				if (base.gameObject == null)
				{
					return STR_UNINSTNATIATED;
				}
				return REGEX_SANITIZE.Replace(base.gameObject.name, REGEX_REPLACE);
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

		public virtual void UpdateFitAndAlign()
		{
			if (base.gameObject.activeInHierarchy)
			{
				FitToObject componentInChildren = GetComponentInChildren<FitToObject>(includeInactive: false);
				if ((bool)componentInChildren && componentInChildren.isActiveAndEnabled)
				{
					componentInChildren.UpdateSize();
				}
			}
			AlignToObject component = GetComponent<AlignToObject>();
			if ((bool)component && component.enabled)
			{
				component.UpdatePos();
			}
		}

		public virtual RectCentered GetRect(Transform exclude = null)
		{
			RectCentered result = GuiUtils.CalculateGuiRectInternal(base.transform, includeChildren: true, null, null, exclude);
			result.Transform(base.transform);
			return result;
		}

		public virtual void OnFocus()
		{
			CallbackOnFocus?.Invoke();
		}

		public virtual void OnDefocus()
		{
			CallbackOnDefocus?.Invoke();
		}

		public virtual void OnKeyboardFocus()
		{
			CallbackOnKeyboardFocus?.Invoke();
			Singleton<PowerQuest>.Get.ProcessGuiEvent(PowerQuest.SCRIPT_FUNCTION_ONKBFOCUS, GuiData, this);
		}

		public virtual void OnKeyboardDefocus()
		{
			CallbackOnKeyboardDefocus?.Invoke();
			Singleton<PowerQuest>.Get.ProcessGuiEvent(PowerQuest.SCRIPT_FUNCTION_ONKBDEFOCUS, GuiData, this);
		}

		public virtual bool HandleKeyboardInput(eGuiNav input)
		{
			return false;
		}

		private void Start()
		{
			GuiComponent.EditorUpdateChildComponents();
			GuiComponent.RegisterControl(this);
			Visible = m_visible;
			UpdateBaseline();
		}

		public void Show()
		{
			Visible = true;
		}

		public void Hide()
		{
			Visible = false;
		}

		public void SetGui(Gui gui)
		{
			m_gui = gui;
		}

		public AnimationClip GetAnimation(string animName)
		{
			return GuiComponent.GetAnimation(animName);
		}

		public List<AnimationClip> GetAnimations()
		{
			return GuiComponent.GetAnimations();
		}

		public Sprite GetSprite(string name)
		{
			return GuiComponent.GetSprite(name);
		}

		public List<Sprite> GetSprites()
		{
			return GuiComponent.GetSprites();
		}

		private List<T> GetThisControlsComponents<T>() where T : Component
		{
			List<T> list = new List<T>();
			GetControlsComponents(base.transform, list);
			return list;
		}

		private static void GetControlsComponents<T>(Transform from, List<T> list) where T : Component
		{
			list.AddRange(from.GetComponents<T>());
			for (int i = 0; i < from.childCount; i++)
			{
				Transform child = from.transform.GetChild(i);
				if (!child.GetComponent<GuiControl>())
				{
					GetControlsComponents(child, list);
				}
			}
		}

		public virtual void UpdateBaseline()
		{
			GuiComponent guiComponent = GuiComponent;
			if (guiComponent == null)
			{
				return;
			}
			m_baseline = Mathf.Clamp(Baseline, -99f, 99f);
			int num = -Mathf.RoundToInt(guiComponent.GetData().Baseline * 100f + m_baseline * 1f);
			foreach (SpriteRenderer thisControlsComponent in GetThisControlsComponents<SpriteRenderer>())
			{
				thisControlsComponent.sortingOrder = num;
			}
			QuestText componentInChildren = GetComponentInChildren<QuestText>();
			if (componentInChildren != null)
			{
				componentInChildren.OrderInLayer = num;
			}
		}

		public Coroutine Fade(float start, float end, float duration, eEaseCurve curve = eEaseCurve.InOutSmooth)
		{
			return Singleton<PowerQuest>.Get.StartCoroutine(CoroutineFade(start, end, duration, curve));
		}

		public void FadeBG(float start, float end, float duration, eEaseCurve curve = eEaseCurve.InOutSmooth)
		{
			Singleton<PowerQuest>.Get.StartCoroutine(CoroutineFade(start, end, duration, curve));
		}

		protected IEnumerator CoroutineFade(float start, float end, float duration, eEaseCurve curve = eEaseCurve.InOutSmooth)
		{
			if (Instance == null)
			{
				yield break;
			}
			SpriteRenderer[] sprites = Instance.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
			QuestText[] texts = Instance.GetComponentsInChildren<QuestText>(includeInactive: true);
			float time = 0f;
			m_alpha = start;
			Action FadeSetAlpha = delegate
			{
				Array.ForEach(sprites, delegate(SpriteRenderer sprite)
				{
					if (sprite != null)
					{
						sprite.color = sprite.color.WithAlpha(m_alpha);
					}
				});
				Array.ForEach(texts, delegate(QuestText text)
				{
					if (text != null)
					{
						text.color = text.color.WithAlpha(m_alpha);
					}
				});
			};
			FadeSetAlpha();
			while (time < duration)
			{
				yield return new WaitForEndOfFrame();
				time += Time.deltaTime;
				float ratio = time / duration;
				ratio = QuestUtils.Ease(ratio, curve);
				m_alpha = Mathf.Lerp(start, end, ratio);
				FadeSetAlpha();
			}
			m_alpha = end;
			FadeSetAlpha();
		}

		public virtual void SetPosition(float x, float y)
		{
			Position = new Vector2(x, y);
		}

		public virtual void OnInteraction(eQuestVerb verb)
		{
		}

		public virtual void OnCancelInteraction(eQuestVerb verb)
		{
		}

		public QuestScript GetScript()
		{
			return GuiData?.GetScript();
		}

		public IQuestScriptable GetScriptable()
		{
			return this;
		}

		public string GetScriptName()
		{
			return ScriptName;
		}

		public string GetScriptClassName()
		{
			return ScriptName;
		}

		public void HotLoadScript(Assembly assembly)
		{
		}

		public void EditorRename(string name)
		{
			base.gameObject.name = name;
		}

		private void OnDrawGizmosSelected()
		{
			_ = GetComponentInParent<GuiComponent>() == null;
		}
	}
}
