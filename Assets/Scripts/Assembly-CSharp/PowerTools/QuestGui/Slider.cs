using System;
using PowerTools.Quest;
using UnityEngine;

namespace PowerTools.QuestGui
{
	[Serializable]
	[AddComponentMenu("Quest Gui/Slider")]
	public class Slider : GuiControl, ISlider, IGuiControl
	{
		private enum eElement
		{
			Bar = 0,
			Handle = 1
		}

		private enum eState
		{
			Default = 0,
			Hover = 1,
			Click = 2,
			Off = 3
		}

		private enum eDirection
		{
			Horizontal = 0,
			Vertical = 1
		}

		public enum eColorUse
		{
			None = 0,
			Text = 1,
			Image = 2
		}

		[Tooltip("Whether button can be clicked. When false, the button's anim/colour is set to the 'Inactive' one")]
		[SerializeField]
		private bool m_clickable = true;

		[SerializeField]
		private eDirection m_direction;

		[SerializeField]
		[HideInInspector]
		private RectCentered m_customSize = RectCentered.zero;

		[Header("Mouse-over Defaults")]
		[TextArea(1, 10)]
		[SerializeField]
		private string m_description = "New Button";

		[Tooltip("If set, changes the name of the cursor when moused over")]
		[SerializeField]
		private string m_cursor;

		[Header("Visuals")]
		[SerializeField]
		private string m_barAnim;

		[SerializeField]
		private string m_barAnimHover;

		[SerializeField]
		private string m_barAnimClick;

		[SerializeField]
		private string m_barAnimOff;

		[SerializeField]
		private string m_handleAnim;

		[SerializeField]
		private string m_handleAnimHover;

		[SerializeField]
		private string m_handleAnimClick;

		[SerializeField]
		private string m_handleAnimOff;

		[SerializeField]
		private eColorUse m_colorWhat = eColorUse.Text;

		[SerializeField]
		private Color m_color = new Color(0f, 0f, 0f, 1f);

		[SerializeField]
		private Color m_colorHover = new Color(0f, 0f, 0f, 0f);

		[SerializeField]
		private Color m_colorClick = new Color(0f, 0f, 0f, 0f);

		[SerializeField]
		private Color m_colorOff = new Color(0f, 0f, 0f, 0f);

		[Header("Audio")]
		[SerializeField]
		private string m_soundHover = string.Empty;

		[SerializeField]
		private string m_soundClick = string.Empty;

		[SerializeField]
		private string m_soundSlide = string.Empty;

		[Header("Hotspot size")]
		[SerializeField]
		private Padding m_hotspotPadding = Padding.zero;

		[SerializeField]
		[Tooltip("Padding on the handle to stop it going too far of edges of hotspot")]
		private Padding m_handlePadding = Padding.zero;

		public Action<GuiControl> OnClick;

		public Action<GuiControl> OnDrag;

		[Header("Children")]
		[SerializeField]
		private SpriteRenderer m_barSprite;

		[SerializeField]
		private SpriteRenderer m_handleSprite;

		[SerializeField]
		private QuestText m_questText;

		private SpriteAnim m_barSpriteAnimator;

		private SpriteAnim m_handleSpriteAnimator;

		private BoxCollider2D m_bgBoxCollider2D;

		private eState m_state;

		private float m_ratio = -1f;

		private float m_keyboardIncrement = 0.1f;

		public IQuestClickable IClickable => this;

		public float Ratio
		{
			get
			{
				return m_ratio;
			}
			set
			{
				value = Mathf.Clamp01(value);
				if (m_ratio != value)
				{
					m_ratio = value;
					RectCentered customSize = m_customSize;
					customSize.AddPadding(m_hotspotPadding);
					customSize.RemovePadding(m_handlePadding);
					if (m_direction == eDirection.Horizontal)
					{
						m_handleSprite.transform.localPosition = m_handleSprite.transform.localPosition.WithX(Utils.SnapRound(Mathf.Lerp(customSize.MinX, customSize.MaxX, m_ratio)));
					}
					else
					{
						m_handleSprite.transform.localPosition = m_handleSprite.transform.localPosition.WithY(Utils.SnapRound(Mathf.Lerp(customSize.MinY, customSize.MaxY, m_ratio)));
					}
				}
			}
		}

		public float KeyboardIncrement
		{
			get
			{
				return m_keyboardIncrement;
			}
			set
			{
				m_keyboardIncrement = value;
			}
		}

		public string Text
		{
			get
			{
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
				if (m_questText == null)
				{
					m_questText = GetComponentInChildren<QuestText>();
				}
				if (m_questText != null)
				{
					m_questText.text = value;
				}
			}
		}

		public string AnimBar
		{
			get
			{
				return m_barAnim;
			}
			set
			{
				m_barAnim = value;
				OnAnimationChanged();
			}
		}

		public string AnimBarHover
		{
			get
			{
				return m_barAnimHover;
			}
			set
			{
				m_barAnimHover = value;
				OnAnimationChanged();
			}
		}

		public string AnimBarClick
		{
			get
			{
				return m_barAnimClick;
			}
			set
			{
				m_barAnimClick = value;
				OnAnimationChanged();
			}
		}

		public string AnimBarOff
		{
			get
			{
				return m_barAnimOff;
			}
			set
			{
				m_barAnimOff = value;
				OnAnimationChanged();
			}
		}

		public string AnimHandle
		{
			get
			{
				return m_handleAnim;
			}
			set
			{
				m_handleAnim = value;
				OnAnimationChanged();
			}
		}

		public string AnimHandleHover
		{
			get
			{
				return m_handleAnimHover;
			}
			set
			{
				m_handleAnimHover = value;
				OnAnimationChanged();
			}
		}

		public string AnimHandleClick
		{
			get
			{
				return m_handleAnimClick;
			}
			set
			{
				m_handleAnimClick = value;
				OnAnimationChanged();
			}
		}

		public string AnimHandleOff
		{
			get
			{
				return m_handleAnimOff;
			}
			set
			{
				m_handleAnimOff = value;
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

		public eColorUse ColorWhat => m_colorWhat;

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

		public void UpdateHotspot()
		{
			if (m_bgBoxCollider2D == null)
			{
				m_bgBoxCollider2D = GetComponent<BoxCollider2D>();
				if (m_bgBoxCollider2D == null)
				{
					Debug.LogWarning("Buttons need a BoxCollider2D to Auto-Scale their Hotspot");
				}
			}
			if (m_bgBoxCollider2D != null)
			{
				InitComponentReferences();
				RectCentered rectCentered = GuiUtils.CalculateGuiRectInternal(base.transform, includeChildren: false, m_barSprite);
				rectCentered.AddPadding(m_hotspotPadding);
				if (rectCentered.Center != m_bgBoxCollider2D.offset || m_bgBoxCollider2D.size != rectCentered.Size)
				{
					m_bgBoxCollider2D.offset = rectCentered.Center;
					m_bgBoxCollider2D.size = rectCentered.Size;
				}
			}
		}

		public SpriteRenderer GetSprite()
		{
			return m_barSprite;
		}

		public SpriteAnim GetSpriteAnimator()
		{
			return m_barSpriteAnimator;
		}

		public SpriteRenderer GetHandleSprite()
		{
			return m_handleSprite;
		}

		public SpriteAnim GetHandleSpriteAnimator()
		{
			return m_handleSpriteAnimator;
		}

		public QuestText GetQuestText()
		{
			return m_questText;
		}

		public void EditorUpdateSprite()
		{
			if (m_barSprite == null)
			{
				m_barSprite = GetComponentInChildren<SpriteRenderer>();
			}
			if (m_barSprite != null)
			{
				GetComponentInParent<GuiComponent>().GetAnimation(m_barAnim);
			}
			if (m_handleSprite == null)
			{
				m_handleSprite = GetComponentInChildren<SpriteRenderer>();
			}
			if (m_handleSprite != null)
			{
				GetComponentInParent<GuiComponent>().GetAnimation(m_handleAnim);
			}
		}

		public override RectCentered GetRect(Transform excludeChild = null)
		{
			InitComponentReferences();
			if (m_barSprite != null)
			{
				RectCentered result = GuiUtils.CalculateGuiRectInternal(base.transform, includeChildren: false, m_barSprite, null, excludeChild);
				result.Transform(base.transform);
				return result;
			}
			return RectCentered.zero;
		}

		public override bool HandleKeyboardInput(eGuiNav input)
		{
			if (m_direction == eDirection.Horizontal && input != eGuiNav.Left && input != eGuiNav.Right)
			{
				return false;
			}
			if (m_direction == eDirection.Vertical && input != eGuiNav.Up && input != eGuiNav.Down)
			{
				return false;
			}
			float ratio = m_ratio;
			ratio = ((input != eGuiNav.Left && input != eGuiNav.Down) ? (ratio + m_keyboardIncrement) : (ratio - m_keyboardIncrement));
			if (m_ratio != ratio)
			{
				if (IsString.Valid(m_soundSlide))
				{
					SystemAudio.Play(m_soundSlide);
				}
				Ratio = ratio;
				Singleton<PowerQuest>.Get.ProcessGuiEvent(PowerQuest.SCRIPT_FUNCTION_DRAGGUI, base.GuiData, this);
				SendMessageUpwards(PowerQuest.SCRIPT_FUNCTION_DRAGGUI + base.ScriptName, this, SendMessageOptions.DontRequireReceiver);
				OnDrag?.Invoke(this);
				Singleton<PowerQuest>.Get.ProcessGuiClick(base.GuiData, this);
				SendMessageUpwards(PowerQuest.SCRIPT_FUNCTION_CLICKGUI + base.ScriptName, this, SendMessageOptions.DontRequireReceiver);
				OnClick?.Invoke(this);
			}
			return true;
		}

		private void Awake()
		{
			InitComponentReferences();
		}

		private void InitComponentReferences()
		{
			if (m_barSprite != null && m_barSpriteAnimator == null)
			{
				m_barSpriteAnimator = m_barSprite.GetComponent<SpriteAnim>();
			}
			if (m_handleSprite != null && m_handleSpriteAnimator == null)
			{
				m_handleSpriteAnimator = m_handleSprite.GetComponent<SpriteAnim>();
			}
			if (m_questText == null)
			{
				m_questText = GetComponentInChildren<QuestText>();
			}
		}

		private void Start()
		{
			InitComponentReferences();
			SetState((!Clickable) ? eState.Off : eState.Default);
			StartStateAnimation();
			if (Ratio < 0f)
			{
				Ratio = 0f;
			}
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
				if (base.Focused)
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
				if (Input.GetMouseButton(0))
				{
					float num = 0f;
					RectCentered customSize = m_customSize;
					customSize.AddPadding(m_hotspotPadding);
					num = ((m_direction != eDirection.Horizontal) ? Mathf.InverseLerp(customSize.MinY, customSize.MaxY, Singleton<PowerQuest>.Get.GetMousePositionGui().y - base.transform.position.y) : Mathf.InverseLerp(customSize.MinX, customSize.MaxX, Singleton<PowerQuest>.Get.GetMousePositionGui().x - base.transform.position.x));
					if (m_ratio != num)
					{
						Ratio = num;
						Singleton<PowerQuest>.Get.ProcessGuiEvent(PowerQuest.SCRIPT_FUNCTION_DRAGGUI, base.GuiData, this);
						SendMessageUpwards(PowerQuest.SCRIPT_FUNCTION_DRAGGUI + base.ScriptName, this, SendMessageOptions.DontRequireReceiver);
						OnDrag?.Invoke(this);
					}
				}
				else
				{
					Singleton<PowerQuest>.Get.ProcessGuiClick(base.GuiData, this);
					SendMessageUpwards(PowerQuest.SCRIPT_FUNCTION_CLICKGUI + base.ScriptName, this, SendMessageOptions.DontRequireReceiver);
					OnClick?.Invoke(this);
					if (base.Focused)
					{
						SetState(eState.Hover);
					}
					else
					{
						SetState(eState.Default);
					}
				}
				break;
			}
			if (Input.GetMouseButtonDown(1) && base.Focused)
			{
				Singleton<PowerQuest>.Get.ProcessGuiClick(base.GuiData, this);
			}
		}

		private void SetState(eState newState)
		{
			if (m_state != newState)
			{
				if (newState == eState.Hover && IsString.Valid(m_soundHover))
				{
					SystemAudio.Play(m_soundHover);
				}
				if (newState == eState.Click && IsString.Valid(m_soundClick))
				{
					SystemAudio.Play(m_soundClick);
				}
			}
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
			case eState.Off:
				color = m_colorOff;
				break;
			}
			if (m_questText != null && m_colorWhat == eColorUse.Text)
			{
				m_questText.color = color;
			}
			else if (m_barSprite != null && m_colorWhat == eColorUse.Image)
			{
				m_barSprite.color = color;
			}
		}

		private void StartStateAnimation()
		{
			eElement element = eElement.Bar;
			bool flag = false;
			switch (m_state)
			{
			case eState.Hover:
				flag = PlayAnimInternal(element, m_barAnimHover);
				break;
			case eState.Click:
				flag = PlayAnimInternal(element, m_barAnimClick);
				if (!flag)
				{
					flag = PlayAnimInternal(element, m_barAnimHover);
				}
				break;
			case eState.Off:
				flag = PlayAnimInternal(element, m_barAnimOff);
				break;
			}
			if (!flag)
			{
				PlayAnimInternal(element, m_barAnim);
			}
			eElement element2 = eElement.Handle;
			bool flag2 = false;
			switch (m_state)
			{
			case eState.Hover:
				flag2 = PlayAnimInternal(element2, m_handleAnimHover);
				break;
			case eState.Click:
				flag2 = PlayAnimInternal(element2, m_handleAnimClick);
				if (!flag2)
				{
					flag2 = PlayAnimInternal(element2, m_handleAnimHover);
				}
				break;
			case eState.Off:
				flag2 = PlayAnimInternal(element2, m_handleAnimOff);
				break;
			}
			if (!flag2)
			{
				PlayAnimInternal(element2, m_handleAnim);
			}
		}

		private bool PlayAnimInternal(eElement element, string animName, bool fromStart = true)
		{
			SpriteAnim spriteAnim = ((element == eElement.Bar) ? m_barSpriteAnimator : m_handleSpriteAnimator);
			SpriteRenderer spriteRenderer = ((element == eElement.Bar) ? m_barSprite : m_handleSprite);
			if (string.IsNullOrEmpty(animName))
			{
				return false;
			}
			AnimationClip animation = GetAnimation(animName);
			if (animation != null && spriteAnim != null)
			{
				if (fromStart || spriteAnim.Clip == null)
				{
					spriteAnim.Play(animation);
				}
				else
				{
					float time = spriteAnim.Time;
					spriteAnim.Play(animation);
					spriteAnim.Time = time;
				}
				return true;
			}
			Sprite sprite = GetSprite(animName);
			if (sprite != null)
			{
				spriteAnim.Stop();
				spriteRenderer.sprite = sprite;
				return true;
			}
			return false;
		}
	}
}
