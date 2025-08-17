using System;
using System.Collections;
using PowerTools.Quest;
using UnityEngine;
using UnityEngine.Serialization;

namespace PowerTools.QuestGui
{
	[Serializable]
	[AddComponentMenu("Quest Gui/Button")]
	public class Button : GuiControl, IButton, IGuiControl
	{
		private enum eState
		{
			Default = 0,
			Hover = 1,
			Click = 2,
			Off = 3
		}

		public enum eSizeSetting
		{
			Custom = 0,
			ResizableImage = 1,
			Image = 2,
			FitText = 3
		}

		public enum eColorUse
		{
			None = 0,
			Text = 1,
			Image = 2,
			Both = 3
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

		[Header("Visuals")]
		[SerializeField]
		private string m_anim;

		[SerializeField]
		private string m_animHover;

		[SerializeField]
		private string m_animClick;

		[SerializeField]
		private string m_animOff;

		[SerializeField]
		private eColorUse m_colorWhat = eColorUse.Text;

		[FormerlySerializedAs("m_textColor")]
		[SerializeField]
		private Color m_color = new Color(0f, 0f, 0f, 1f);

		[FormerlySerializedAs("m_textColorHover")]
		[SerializeField]
		private Color m_colorHover = new Color(0f, 0f, 0f, 0f);

		[FormerlySerializedAs("m_textColorClick")]
		[SerializeField]
		private Color m_colorClick = new Color(0f, 0f, 0f, 0f);

		[FormerlySerializedAs("m_textColorOff")]
		[SerializeField]
		private Color m_colorOff = new Color(0f, 0f, 0f, 0f);

		[Header("Audio")]
		[SerializeField]
		private string m_soundHover = string.Empty;

		[SerializeField]
		private string m_soundClick = string.Empty;

		[Header("Hotspot size")]
		[SerializeField]
		private Padding m_hotspotPadding = Padding.zero;

		[SerializeField]
		[HideInInspector]
		private eSizeSetting m_sizeSetting = eSizeSetting.Image;

		[SerializeField]
		[HideInInspector]
		private RectCentered m_customSize = RectCentered.zero;

		public Action<GuiControl> OnClick;

		private SpriteRenderer m_sprite;

		private SpriteAnim m_spriteAnimator;

		private QuestText m_questText;

		private FitToObject m_stretchComponent;

		private BoxCollider2D m_boxCollider2D;

		private eState m_state;

		private string m_cachedText;

		private bool m_overrideAnimPlaying;

		private int m_stopOverrideAnimDelay = -1;

		private bool m_forceClick;

		private QuestAnimationTriggers m_animTriggerComponent;

		public IQuestClickable IClickable => this;

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

		public bool Animating => GetAnimating();

		public eSizeSetting SizeSetting
		{
			get
			{
				return m_sizeSetting;
			}
			set
			{
				m_sizeSetting = value;
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

		public void PauseAnimation()
		{
			if (m_spriteAnimator != null && m_overrideAnimPlaying)
			{
				m_spriteAnimator.Pause();
			}
		}

		public void ResumeAnimation()
		{
			if (m_spriteAnimator != null && m_overrideAnimPlaying)
			{
				m_spriteAnimator.Resume();
			}
		}

		public void StopAnimation()
		{
			if (m_overrideAnimPlaying)
			{
				StartStateAnimation();
			}
			m_overrideAnimPlaying = false;
		}

		public Coroutine PlayAnimation(string animName)
		{
			return Singleton<PowerQuest>.Get.StartCoroutine(CoroutinePlayAnimation(animName));
		}

		public void PlayAnimationBG(string animName)
		{
			if (!Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				PlayOverrideAnim(animName);
			}
		}

		public void AddAnimationTrigger(string triggerName, bool removeAfterTriggering, Action action)
		{
			QuestAnimationTriggers questAnimationTriggers = GetComponent<QuestAnimationTriggers>();
			if (questAnimationTriggers == null)
			{
				questAnimationTriggers = base.gameObject.AddComponent<QuestAnimationTriggers>();
			}
			if (questAnimationTriggers != null)
			{
				questAnimationTriggers.AddTrigger(triggerName, action, removeAfterTriggering);
			}
		}

		public void RemoveAnimationTrigger(string triggerName)
		{
			QuestAnimationTriggers component = GetComponent<QuestAnimationTriggers>();
			if (component != null)
			{
				component.RemoveTrigger(triggerName);
			}
		}

		public Coroutine WaitForAnimTrigger(string triggerName)
		{
			return Singleton<PowerQuest>.Get.StartCoroutine(CoroutineWaitForAnimTrigger(triggerName));
		}

		public void UpdateHotspot()
		{
			if (m_boxCollider2D == null)
			{
				m_boxCollider2D = GetComponent<BoxCollider2D>();
				if (m_boxCollider2D == null)
				{
					Debug.LogWarning("Buttons need a BoxCollider2D to Auto-Scale their Hotspot");
					m_sizeSetting = eSizeSetting.Custom;
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
				m_sprite = GetComponentInChildren<SpriteRenderer>();
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

		public override bool HandleKeyboardInput(eGuiNav input)
		{
			if (input == eGuiNav.Ok)
			{
				if (!m_forceClick)
				{
					StartCoroutine(CoroutineClick());
				}
				return true;
			}
			return false;
		}

		private IEnumerator CoroutineClick()
		{
			Singleton<PowerQuest>.Get.LockFocusedControl();
			m_forceClick = true;
			yield return new WaitForSeconds(0.15f);
			m_forceClick = false;
			Update();
			Singleton<PowerQuest>.Get.UnlockFocusedControl();
			yield return null;
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
		}

		private void Start()
		{
			InitComponentReferences();
			StartStateAnimation();
			if (m_sizeSetting == eSizeSetting.FitText)
			{
				if (m_stretchComponent == null)
				{
					m_stretchComponent = GetComponentInChildren<FitToObject>();
				}
				if (m_stretchComponent != null)
				{
					m_stretchComponent.UpdateSize();
				}
				m_cachedText = Text;
			}
			if (m_sizeSetting == eSizeSetting.FitText || m_sizeSetting == eSizeSetting.Image)
			{
				UpdateHotspot();
			}
		}

		private void Update()
		{
			UpdateOverrideAnim();
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
				else if (Input.GetMouseButtonDown(0) || m_forceClick)
				{
					SetState(eState.Click);
				}
				break;
			case eState.Click:
				if (Input.GetMouseButton(0) || m_forceClick)
				{
					break;
				}
				if (base.Focused)
				{
					Singleton<PowerQuest>.Get.ProcessGuiClick(base.GuiData, this);
					SendMessageUpwards(PowerQuest.SCRIPT_FUNCTION_CLICKGUI + base.ScriptName, this, SendMessageOptions.DontRequireReceiver);
					if (OnClick != null)
					{
						OnClick(this);
					}
				}
				SetState(eState.Default);
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

		private void LateUpdate()
		{
			LateUpdateOverrideAnim();
			if (m_sizeSetting == eSizeSetting.FitText && m_cachedText != Text)
			{
				if (m_stretchComponent == null)
				{
					m_stretchComponent = GetComponentInChildren<FitToObject>();
				}
				if (m_stretchComponent != null)
				{
					m_stretchComponent.UpdateSize();
				}
				UpdateHotspot();
				m_cachedText = Text;
			}
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
			Color col = m_color;
			switch (m_state)
			{
			case eState.Hover:
				col = m_colorHover;
				break;
			case eState.Click:
				col = m_colorClick;
				break;
			case eState.Off:
				col = m_colorOff;
				break;
			}
			if (m_questText != null && (m_colorWhat == eColorUse.Text || m_colorWhat == eColorUse.Both))
			{
				m_questText.color = col.WithAlpha(col.a * base.Alpha);
			}
			if (m_sprite != null && (m_colorWhat == eColorUse.Image || m_colorWhat == eColorUse.Both))
			{
				m_sprite.color = col.WithAlpha(col.a * base.Alpha);
			}
		}

		private void StartStateAnimation()
		{
			m_stopOverrideAnimDelay = 0;
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
			m_stopOverrideAnimDelay = 0;
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

		private IEnumerator CoroutinePlayAnimation(string animName)
		{
			PlayOverrideAnim(animName);
			while (GetAnimating() && !Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				yield return new WaitForEndOfFrame();
			}
			if (Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				SpriteAnim component = GetComponent<SpriteAnim>();
				if (component != null)
				{
					component.NormalizedTime = 1f;
					GetComponent<Animator>().Update(0f);
				}
				StopAnimation();
			}
		}

		private IEnumerator CoroutineWaitForAnimTrigger(string triggerName)
		{
			if (!Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				bool hit = false;
				AddAnimationTrigger(triggerName, removeAfterTriggering: true, delegate
				{
					hit = true;
				});
				yield return Singleton<PowerQuest>.Get.WaitUntil(() => hit || !GetSpriteAnimator().Playing);
			}
		}

		private void UpdateOverrideAnim()
		{
			if (m_overrideAnimPlaying && Singleton<PowerQuest>.Get.GetSkippingCutscene() && !m_spriteAnimator.GetCurrentAnimation().isLooping)
			{
				StopAnimation();
			}
		}

		private void LateUpdateOverrideAnim()
		{
			if (m_stopOverrideAnimDelay > 0)
			{
				m_stopOverrideAnimDelay--;
				if (m_stopOverrideAnimDelay == 0)
				{
					StartStateAnimation();
				}
			}
			if (m_overrideAnimPlaying && !m_spriteAnimator.Playing)
			{
				if (m_overrideAnimPlaying)
				{
					m_stopOverrideAnimDelay = 1;
				}
				m_overrideAnimPlaying = false;
			}
		}

		private void PlayOverrideAnim(string animName)
		{
			if (!PlayAnimInternal(animName) && Singleton<PowerQuest>.Get.IsDebugBuild)
			{
				Debug.LogWarning("Failed to find Button animation: " + animName);
			}
			m_overrideAnimPlaying = true;
		}

		private bool GetAnimating()
		{
			if (m_overrideAnimPlaying)
			{
				return m_spriteAnimator.Playing;
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
