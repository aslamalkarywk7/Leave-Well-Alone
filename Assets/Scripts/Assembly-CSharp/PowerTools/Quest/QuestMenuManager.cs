using System;
using System.Collections.Generic;
using UnityEngine;

namespace PowerTools.Quest
{
	[Serializable]
	public class QuestMenuManager
	{
		private static readonly float KB_REPEAT_TIME_FIRST = 0.4f;

		private static readonly float KB_REPEAT_TIME_REPEAT = 0.1f;

		public static readonly string DEFAULT_FADE_SOURCE = "";

		[SerializeField]
		private SpriteRenderer m_prefabEffectMenuFadeOut;

		[SerializeField]
		private Color m_fadeColour = Color.black;

		[Tooltip("If using custom fade, poll GetFadeRatio() and do your own fading")]
		[SerializeField]
		private bool m_customFade;

		private SourceList m_fadeSources = new SourceList();

		private float m_fadeInTime;

		private float m_fadeOutTime;

		private float m_fadeAlpha;

		private SpriteRenderer m_sprite;

		private Color m_fadeColourDefault = Color.black;

		private bool m_kbActive;

		private bool m_kbFocus;

		private BitMask m_kbPrevState;

		private BitMask m_kbState;

		private float m_kbRepeatTimer = KB_REPEAT_TIME_FIRST;

		private bool m_kbWaitForRelease;

		private Vector2 m_cachedMousePos = Vector2.zero;

		private List<Gui> m_sortedGuis = new List<Gui>();

		public Action CallbackOnUpdateFade;

		public static QuestMenuManager Get => Singleton<PowerQuest>.Get.GetMenuManager();

		public bool KeyboardActive => m_kbActive;

		public Color FadeColor
		{
			get
			{
				return m_fadeColour;
			}
			set
			{
				m_fadeColour = value;
				UpdateFadeSprite();
			}
		}

		public Color FadeColorDefault
		{
			get
			{
				return m_fadeColourDefault;
			}
			set
			{
				m_fadeColourDefault = value;
				UpdateFadeSprite();
			}
		}

		public bool KeyboardInputValid => m_kbWaitForRelease = false;

		public void Awake()
		{
			m_fadeColourDefault = m_fadeColour;
		}

		public void FadeColorRestore()
		{
			m_fadeColour = m_fadeColourDefault;
		}

		public bool GetFading()
		{
			if (m_fadeSources.Empty() || !(m_fadeAlpha < 1f))
			{
				if (m_fadeSources.Empty())
				{
					return m_fadeAlpha > 0f;
				}
				return false;
			}
			return true;
		}

		public float GetFadeRatio()
		{
			return m_fadeAlpha;
		}

		public Color GetFadeColor()
		{
			return m_fadeColour;
		}

		public bool GetCustomFade()
		{
			return m_customFade;
		}

		public void ResetFade()
		{
			m_fadeInTime = 0f;
			m_fadeOutTime = 0f;
			m_fadeAlpha = 0f;
			m_fadeSources.Clear();
		}

		public void FadeOut(float time)
		{
			FadeOut(time, DEFAULT_FADE_SOURCE);
		}

		public void FadeOut(float time, string source)
		{
			m_fadeOutTime = time;
			if (!m_fadeSources.Contains(source))
			{
				m_fadeSources.Add(source);
			}
			if (m_fadeOutTime == 0f)
			{
				m_fadeAlpha = 1f;
				UpdateFadeSprite();
			}
		}

		public void FadeSkip()
		{
			m_fadeAlpha = ((!m_fadeSources.Empty()) ? 1 : 0);
			UpdateFadeSprite();
		}

		public void FadeIn(float time)
		{
			FadeIn(time, DEFAULT_FADE_SOURCE);
		}

		public void FadeIn(float time, string source)
		{
			m_fadeInTime = time;
			m_fadeSources.Remove(source);
			if (m_fadeInTime == 0f && m_fadeSources.Empty())
			{
				m_fadeAlpha = 0f;
				FadeColorRestore();
				UpdateFadeSprite();
			}
		}

		public void Update()
		{
			if (!m_fadeSources.Empty() && m_fadeAlpha < 1f)
			{
				m_fadeAlpha += Time.deltaTime / m_fadeOutTime;
				if (m_fadeAlpha > 1f)
				{
					m_fadeAlpha = 1f;
				}
				UpdateFadeSprite();
			}
			if (m_fadeSources.Empty() && m_fadeAlpha > 0f)
			{
				m_fadeAlpha -= Time.deltaTime / m_fadeInTime;
				if (m_fadeAlpha <= 0f)
				{
					m_fadeAlpha = 0f;
					FadeColorRestore();
				}
				UpdateFadeSprite();
			}
			m_sortedGuis.Clear();
			m_sortedGuis.AddRange(Singleton<PowerQuest>.Get.GetGuis());
			m_sortedGuis.Sort(delegate(Gui a, Gui b)
			{
				int num2 = b.Baseline.CompareTo(a.Baseline);
				if (num2 == 0 && a.Instance != null && b.Instance != null)
				{
					num2 = a.Instance.GetInstanceID().CompareTo(b.Instance.GetInstanceID());
				}
				return num2;
			});
			int num = 0;
			foreach (Gui sortedGui in m_sortedGuis)
			{
				if (sortedGui.Instance != null)
				{
					sortedGui.Instance.transform.SetSiblingIndex(num++);
				}
			}
			UpdateKb();
		}

		public void IgnoreNextKeypress()
		{
			m_kbWaitForRelease = true;
		}

		public bool ProcessKeyboardInput(eGuiNav key)
		{
			m_kbActive = true;
			m_kbState.SetAt(key);
			if (m_kbWaitForRelease)
			{
				return false;
			}
			if (!m_kbPrevState.IsSet(key))
			{
				return true;
			}
			if (m_kbRepeatTimer <= 0f)
			{
				m_kbRepeatTimer = KB_REPEAT_TIME_REPEAT;
				return true;
			}
			return false;
		}

		public bool GetGuiKey(eGuiNav button)
		{
			return m_kbState.IsSet(button);
		}

		public bool GetGuiKeyPress(eGuiNav button)
		{
			if (m_kbState.IsSet(button))
			{
				return !m_kbPrevState.IsSet(button);
			}
			return false;
		}

		public bool GetGuiKeyRelease(eGuiNav button)
		{
			if (!m_kbState.IsSet(button))
			{
				return m_kbPrevState.IsSet(button);
			}
			return false;
		}

		private void UpdateFadeSprite()
		{
			if (m_prefabEffectMenuFadeOut != null && m_sprite == null && !m_customFade)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(m_prefabEffectMenuFadeOut.gameObject);
				if (gameObject != null)
				{
					m_sprite = gameObject.GetComponent<SpriteRenderer>();
					if (Singleton<PowerQuest>.Get.GetPixelCamEnabled())
					{
						gameObject.layer = LayerMask.NameToLayer("HighRes");
					}
				}
			}
			if (m_sprite != null && !m_customFade)
			{
				m_sprite.color = m_fadeColour.WithAlpha(m_fadeAlpha);
				m_sprite.gameObject.SetActive(m_fadeAlpha > 0f);
			}
			if (m_customFade && CallbackOnUpdateFade != null)
			{
				CallbackOnUpdateFade();
			}
		}

		private void UpdateKb()
		{
			if ((int)m_kbPrevState == 0)
			{
				_ = (int)m_kbState;
			}
			m_kbRepeatTimer -= Time.deltaTime;
			if (m_kbState.Value == 0)
			{
				m_kbRepeatTimer = KB_REPEAT_TIME_FIRST;
			}
			m_kbPrevState.Value = m_kbState.Value;
			m_kbState.Value = 0;
			if (m_kbWaitForRelease && m_kbPrevState.Value == 0)
			{
				m_kbWaitForRelease = false;
			}
			if (m_kbActive)
			{
				if (((Vector2)Input.mousePosition - m_cachedMousePos).magnitude > 5f)
				{
					m_kbActive = false;
				}
			}
			else
			{
				m_cachedMousePos = Input.mousePosition;
			}
			if (m_kbFocus && (!m_kbActive | (Singleton<PowerQuest>.Get.GetFocusedGui() == null || !Singleton<PowerQuest>.Get.GetFocusedGui().Visible)))
			{
				RelinquishKbControl();
			}
		}

		public void SetKeyboardFocus(IGuiControl control)
		{
			if (control == null)
			{
				RelinquishKbControl();
				return;
			}
			m_kbActive = true;
			m_kbFocus = true;
			Singleton<PowerQuest>.Get.SetMouseOverClickableOverride(control as IQuestClickable);
		}

		public void RelinquishKbControl()
		{
			if (m_kbFocus)
			{
				Singleton<PowerQuest>.Get.ResetMouseOverClickableOverride();
			}
			m_kbFocus = false;
		}
	}
}
