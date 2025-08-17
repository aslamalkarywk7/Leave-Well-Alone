using System;
using PowerTools.Quest;
using UnityEngine;

namespace PowerTools.QuestGui
{
	[Serializable]
	[AddComponentMenu("Quest Gui/Text Field")]
	public class TextField : GuiControl, ITextField, IGuiControl
	{
		private enum eState
		{
			Default = 0,
			Hover = 1,
			Click = 2,
			Edit = 3,
			Off = 4
		}

		[Tooltip("Whether button can be clicked. When false, the button's anim/colour is set to the 'Inactive' one")]
		[SerializeField]
		private bool m_clickable = true;

		[Header("Mouse-over Defaults")]
		[TextArea(1, 10)]
		[SerializeField]
		private string m_description = "New Button";

		[Tooltip("If set, changes the name of the cursor when moused over")]
		[SerializeField]
		private string m_cursor;

		[Header("Text Field Options")]
		[SerializeField]
		private bool m_requireKeyboardFocus = true;

		[SerializeField]
		private int m_maxCharacters = -1;

		[SerializeField]
		private string m_caretCharacter = "_";

		[SerializeField]
		private float m_caretBlinkRate = 0.4f;

		[Header("Visuals")]
		[SerializeField]
		private string m_anim;

		[SerializeField]
		private string m_animHover;

		[SerializeField]
		private string m_animClick;

		[SerializeField]
		private string m_animEdit;

		[SerializeField]
		private string m_animOff;

		[SerializeField]
		private Color m_color = new Color(0f, 0f, 0f, 1f);

		[SerializeField]
		private Color m_colorHover = new Color(0f, 0f, 0f, 0f);

		[SerializeField]
		private Color m_colorClick = new Color(0f, 0f, 0f, 0f);

		[SerializeField]
		private Color m_colorEdit = new Color(0f, 0f, 0f, 0f);

		[SerializeField]
		private Color m_colorOff = new Color(0f, 0f, 0f, 0f);

		[Header("Hotspot size")]
		[SerializeField]
		private Padding m_hotspotPadding = Padding.zero;

		[SerializeField]
		[HideInInspector]
		private RectCentered m_customSize = RectCentered.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_textPadding = Vector2.zero;

		public Action<GuiControl> OnClick;

		public Action<GuiControl> OnTextChange;

		public Action<GuiControl> OnTextConfirm;

		private SpriteRenderer m_sprite;

		private SpriteAnim m_spriteAnimator;

		private QuestText m_questText;

		private BoxCollider2D m_boxCollider2D;

		private eState m_state;

		private TextEditor m_textEditor = new TextEditor();

		private QuestAnimationTriggers m_animTriggerComponent;

		public IQuestClickable IClickable => this;

		public string Text
		{
			get
			{
				if (m_textEditor != null)
				{
					return m_textEditor.text;
				}
				if (m_questText == null)
				{
					m_questText = GetComponentInChildren<QuestText>();
				}
				if (m_questText != null)
				{
					return m_questText.text;
				}
				return string.Empty;
			}
			set
			{
				if (m_textEditor != null)
				{
					m_textEditor.text = value;
					m_textEditor.cursorIndex = m_textEditor.text.Length;
					m_textEditor.selectIndex = m_textEditor.cursorIndex;
				}
				if (m_questText == null)
				{
					m_questText = GetComponentInChildren<QuestText>();
				}
				if (m_questText != null)
				{
					if (base.HasKeyboardFocus)
					{
						UpdateVisualText(m_textEditor.text, showCaret: true);
					}
					else
					{
						UpdateVisualText(m_textEditor.text, showCaret: false);
					}
				}
			}
		}

		public string Anim
		{
			get
			{
				return m_anim;
			}
			set
			{
				m_anim = value;
				OnAnimationChanged();
			}
		}

		public string AnimHover
		{
			get
			{
				return m_animHover;
			}
			set
			{
				m_animHover = value;
				OnAnimationChanged();
			}
		}

		public string AnimClick
		{
			get
			{
				return m_animClick;
			}
			set
			{
				m_animClick = value;
				OnAnimationChanged();
			}
		}

		public string AnimEdit
		{
			get
			{
				return m_animEdit;
			}
			set
			{
				m_animEdit = value;
				OnAnimationChanged();
			}
		}

		public string AnimOff
		{
			get
			{
				return m_animOff;
			}
			set
			{
				m_animOff = value;
				OnAnimationChanged();
			}
		}

		public Color Color
		{
			get
			{
				return m_color;
			}
			set
			{
				m_color = value;
				OnColorChanged();
			}
		}

		public Color ColorHover
		{
			get
			{
				return m_colorHover;
			}
			set
			{
				m_colorHover = value;
				OnColorChanged();
			}
		}

		public Color ColorClick
		{
			get
			{
				return m_colorClick;
			}
			set
			{
				m_colorClick = value;
				OnColorChanged();
			}
		}

		public Color ColorEdit
		{
			get
			{
				return m_colorEdit;
			}
			set
			{
				m_colorEdit = value;
				OnColorChanged();
			}
		}

		public Color ColorOff
		{
			get
			{
				return m_colorOff;
			}
			set
			{
				m_colorOff = value;
				OnColorChanged();
			}
		}

		public Padding HotspotPadding
		{
			get
			{
				return m_hotspotPadding;
			}
			set
			{
				m_hotspotPadding = value;
			}
		}

		public override RectCentered CustomSize
		{
			get
			{
				return m_customSize;
			}
			set
			{
				m_customSize = value;
			}
		}

		public Vector2 TextPadding
		{
			get
			{
				return m_textPadding;
			}
			set
			{
				m_textPadding = value;
			}
		}

		public override string Description
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

		public override bool Clickable
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

		public override string Cursor
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

		public void FocusKeyboard()
		{
			base.HasKeyboardFocus = true;
		}

		public override void OnKeyboardFocus()
		{
			base.OnKeyboardFocus();
			StartEditingText();
		}

		public override void OnKeyboardDefocus()
		{
			base.OnKeyboardDefocus();
			StopEditingText();
		}

		private void StartEditingText()
		{
			m_textEditor.OnFocus();
			m_textEditor.text = m_questText.text;
			UpdateVisualText(m_textEditor.text, showCaret: true);
			m_textEditor.cursorIndex = m_textEditor.text.Length;
			m_textEditor.selectIndex = m_textEditor.cursorIndex;
			SetState(eState.Edit);
		}

		private void StopEditingText()
		{
			UpdateVisualText(m_textEditor.text, showCaret: false);
			m_textEditor.OnLostFocus();
			SetState(eState.Default);
		}

		public void UpdateHotspot()
		{
			if (m_boxCollider2D == null)
			{
				m_boxCollider2D = GetComponent<BoxCollider2D>();
				if (m_boxCollider2D == null)
				{
					Debug.LogWarning("Text fields need a BoxCollider2D to Auto-Scale their Hotspot");
				}
			}
			if (m_boxCollider2D != null)
			{
				InitComponentReferences();
				RectCentered rectCentered = GuiUtils.CalculateGuiRectInternal(base.transform, includeChildren: false, m_sprite, (m_questText == null) ? null : m_questText.GetComponent<MeshRenderer>());
				rectCentered.AddPadding(m_hotspotPadding);
				if (rectCentered.Center != m_boxCollider2D.offset || m_boxCollider2D.size != rectCentered.Size)
				{
					m_boxCollider2D.offset = rectCentered.Center;
					m_boxCollider2D.size = rectCentered.Size;
				}
			}
		}

		public SpriteRenderer GetSprite()
		{
			return m_sprite;
		}

		public SpriteAnim GetSpriteAnimator()
		{
			return m_spriteAnimator;
		}

		public QuestText GetQuestText()
		{
			return m_questText;
		}

		public void EditorUpdateSprite()
		{
			if (m_sprite == null)
			{
				m_sprite = GetComponentInChildren<SpriteRenderer>(includeInactive: true);
			}
			if (m_sprite != null)
			{
				GetComponentInParent<GuiComponent>().GetAnimation(m_anim);
			}
		}

		public override RectCentered GetRect(Transform excludeChild = null)
		{
			InitComponentReferences();
			MeshRenderer meshRenderer = null;
			if (m_questText != null)
			{
				meshRenderer = m_questText.GetComponent<MeshRenderer>();
			}
			if (m_sprite != null || meshRenderer != null)
			{
				RectCentered result = GuiUtils.CalculateGuiRectInternal(base.transform, includeChildren: false, m_sprite, meshRenderer, excludeChild);
				result.Transform(base.transform);
				return result;
			}
			return RectCentered.zero;
		}

		private void InitComponentReferences()
		{
			if (m_sprite == null)
			{
				m_sprite = GetComponentInChildren<SpriteRenderer>(includeInactive: true);
			}
			if (m_sprite != null && m_spriteAnimator == null)
			{
				m_spriteAnimator = m_sprite.GetComponent<SpriteAnim>();
			}
			if (m_questText == null)
			{
				m_questText = GetComponentInChildren<QuestText>();
			}
		}

		private void Awake()
		{
			InitComponentReferences();
			SetState((!Clickable) ? eState.Off : eState.Default);
			m_textEditor.text = m_questText.text;
		}

		private void Start()
		{
			InitComponentReferences();
			StartStateAnimation();
			UpdateHotspot();
		}

		private void Update()
		{
			if (m_state != eState.Off != Clickable)
			{
				SetState((!Clickable) ? eState.Off : eState.Default);
			}
			switch (m_state)
			{
			case eState.Default:
				if (!m_requireKeyboardFocus)
				{
					if (!base.GuiData.ObscuredByModal)
					{
						StartEditingText();
					}
				}
				else if (base.Focused)
				{
					SetState(eState.Hover);
				}
				break;
			case eState.Hover:
				if (!base.Focused)
				{
					SetState(eState.Default);
				}
				else if (Input.GetMouseButtonDown(0))
				{
					SetState(eState.Click);
				}
				break;
			case eState.Click:
				if (!Input.GetMouseButton(0) && base.Focused)
				{
					FocusKeyboard();
					Singleton<PowerQuest>.Get.ProcessGuiClick(base.GuiData, this);
					SendMessageUpwards(PowerQuest.SCRIPT_FUNCTION_CLICKGUI + base.ScriptName, this, SendMessageOptions.DontRequireReceiver);
					if (OnClick != null)
					{
						OnClick(this);
					}
				}
				break;
			case eState.Edit:
			{
				bool flag = false;
				if (!(m_questText != null) || m_textEditor == null)
				{
					break;
				}
				string inputString = Input.inputString;
				foreach (char c in inputString)
				{
					switch (c)
					{
					case '\b':
						m_textEditor.Backspace();
						flag = true;
						break;
					case '\n':
					case '\r':
						if (m_requireKeyboardFocus)
						{
							Singleton<PowerQuest>.Get.ProcessGuiEvent(PowerQuest.SCRIPT_FUNCTION_ONTEXTCONFIRM + base.ScriptName, base.GuiData, this);
							SendMessageUpwards(PowerQuest.SCRIPT_FUNCTION_ONTEXTCONFIRM + base.ScriptName, this, SendMessageOptions.DontRequireReceiver);
							base.HasKeyboardFocus = false;
						}
						break;
					default:
						if (m_maxCharacters <= 0 || m_textEditor.text.Length < m_maxCharacters)
						{
							m_textEditor.Insert(c);
							flag = true;
						}
						break;
					}
				}
				if (flag || Utils.GetTimeIncrementPassed(m_caretBlinkRate))
				{
					UpdateVisualText(m_textEditor.text, showCaret: true);
					if (flag)
					{
						Singleton<PowerQuest>.Get.ProcessGuiEvent(PowerQuest.SCRIPT_FUNCTION_ONTEXTEDIT + base.ScriptName, base.GuiData, this);
					}
				}
				if (m_requireKeyboardFocus)
				{
					if (Input.GetKeyDown(KeyCode.Escape) || (Input.GetMouseButtonDown(0) && !base.Focused))
					{
						base.HasKeyboardFocus = false;
					}
				}
				else if (base.GuiData.ObscuredByModal)
				{
					StopEditingText();
				}
				break;
			}
			}
			if (Input.GetMouseButtonDown(1) && base.Focused)
			{
				Singleton<PowerQuest>.Get.ProcessGuiClick(base.GuiData, this);
			}
		}

		private void UpdateVisualText(string newText, bool showCaret)
		{
			if (CustomSize.Width > 0f)
			{
				float num = CustomSize.Width - TextPadding.x * 2f;
				TextWrapper textWrapper = new TextWrapper(m_questText.GetComponent<TextMesh>());
				while (textWrapper.GetTextWidth(newText + (showCaret ? m_caretCharacter : "")) > num && newText.Length > 0)
				{
					newText = newText.Substring(1);
				}
			}
			if (showCaret && (m_caretBlinkRate <= 0f || Time.timeSinceLevelLoad % (m_caretBlinkRate * 2f) > m_caretBlinkRate))
			{
				newText += m_caretCharacter;
			}
			m_questText.text = newText;
		}

		private void SetState(eState newState)
		{
			m_state = newState;
			UpdateColor();
			StartStateAnimation();
		}

		private void OnColorChanged()
		{
			UpdateColor();
		}

		private void OnAnimationChanged()
		{
			StartStateAnimation();
		}

		private void UpdateColor()
		{
			Color color = m_color;
			switch (m_state)
			{
			case eState.Hover:
				color = m_colorHover;
				break;
			case eState.Click:
				color = m_colorClick;
				break;
			case eState.Edit:
				color = m_colorEdit;
				break;
			case eState.Off:
				color = m_colorOff;
				break;
			}
			if (m_questText != null)
			{
				m_questText.color = color;
			}
		}

		private void StartStateAnimation()
		{
			bool flag = false;
			switch (m_state)
			{
			case eState.Hover:
				flag = PlayAnimInternal(m_animHover);
				break;
			case eState.Click:
				flag = PlayAnimInternal(m_animClick);
				if (!flag)
				{
					flag = PlayAnimInternal(m_animHover);
				}
				break;
			case eState.Edit:
				flag = PlayAnimInternal(m_animEdit);
				if (!flag)
				{
					flag = PlayAnimInternal(m_animHover);
				}
				break;
			case eState.Off:
				flag = PlayAnimInternal(m_animOff);
				break;
			}
			if (!flag)
			{
				PlayAnimInternal(m_anim);
			}
		}

		private bool PlayAnimInternal(string animName, bool fromStart = true)
		{
			if (m_spriteAnimator == null)
			{
				return true;
			}
			if (string.IsNullOrEmpty(animName))
			{
				return false;
			}
			AnimationClip animation = GetAnimation(animName);
			if (animation != null && m_spriteAnimator != null)
			{
				if (fromStart || m_spriteAnimator.Clip == null)
				{
					m_spriteAnimator.Play(animation);
				}
				else
				{
					float time = m_spriteAnimator.Time;
					m_spriteAnimator.Play(animation);
					m_spriteAnimator.Time = time;
				}
				return true;
			}
			Sprite sprite = GetSprite(animName);
			if (sprite != null)
			{
				m_spriteAnimator.Stop();
				m_sprite.sprite = sprite;
				return true;
			}
			return false;
		}

		private void AnimSound(UnityEngine.Object obj)
		{
			if (obj != null && obj as GameObject != null)
			{
				SystemAudio.Play((obj as GameObject).GetComponent<AudioCue>());
			}
		}

		private void AnimSound(string sound)
		{
			SystemAudio.Play(sound);
		}

		private void _Anim(string function)
		{
			if (m_animTriggerComponent == null)
			{
				m_animTriggerComponent = base.transform.GetComponent<QuestAnimationTriggers>();
				if (m_animTriggerComponent == null)
				{
					m_animTriggerComponent = base.transform.gameObject.AddComponent<QuestAnimationTriggers>();
				}
			}
		}
	}
}
