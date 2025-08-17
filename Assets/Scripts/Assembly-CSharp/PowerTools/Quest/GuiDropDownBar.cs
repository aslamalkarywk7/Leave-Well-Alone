using UnityEngine;

namespace PowerTools.Quest
{
	[AddComponentMenu("Quest Gui Layout/Dropdown Bar")]
	public class GuiDropDownBar : MonoBehaviour
	{
		private enum eScreenEdgeX
		{
			Left = 0,
			Right = 1,
			Center = 2
		}

		private enum eScreenEdgeY
		{
			Bottom = 0,
			Top = 1,
			Middle = 2
		}

		private static readonly float BLEND_TIME = 0.1f;

		[Tooltip("Gui is shown whem mouse is this dist from edge of screen. \n Can be ratio (0.0-1.0), or pixel offset (>1)")]
		[SerializeField]
		private Vector2 m_mouseEdgeDistanceShow = Vector2.one;

		[Tooltip("Gui is hidden when mouse is this dist from edge of screen. \n Can be ratio (0.0-1.0), or pixel offset (>1)")]
		[SerializeField]
		private Vector2 m_mouseEdgeDistanceHide = Vector2.one;

		[Tooltip("Edge of screen that mouse has to be near to Show gui")]
		[SerializeField]
		private eScreenEdgeX m_edgeX;

		[Tooltip("Edge of screen that mouse has to be near to Show gui")]
		[SerializeField]
		private eScreenEdgeY m_edgeY = eScreenEdgeY.Top;

		[Tooltip("Should pause game while mouse is over the gui?")]
		[SerializeField]
		private bool m_pauseWhenVisible = true;

		[Tooltip("Whether to have the gui hide when a blocking script is running")]
		[SerializeField]
		private bool m_hideDuringCutscenes;

		[Tooltip("How far to move (multiplied by the result of the animation curves")]
		[SerializeField]
		private float m_dropDownDistance = 1f;

		[Tooltip("Anim curves that play when show/hiding the gui")]
		[SerializeField]
		private AnimationCurve m_curveIn = new AnimationCurve();

		[SerializeField]
		private AnimationCurve m_curveOut = new AnimationCurve();

		[Header("Sounds")]
		[SerializeField]
		private AudioCue m_soundShow;

		[SerializeField]
		private AudioCue m_soundHide;

		private GuiComponent m_guiComponent;

		private bool m_shown;

		private float m_ratio;

		private Vector2 m_offset = Vector2.zero;

		private float m_blendTimer;

		private Vector2 m_blendOffset = Vector2.zero;

		private bool m_delayShow;

		private float m_highlightPopupTimer;

		private bool m_triggeredPause;

		private bool m_forceOff;

		public Vector2 MouseEdgeDistanceShow
		{
			get
			{
				return m_mouseEdgeDistanceShow;
			}
			set
			{
				m_mouseEdgeDistanceShow = value;
			}
		}

		public void SetForceOff(bool forceOff)
		{
			m_forceOff = forceOff;
		}

		public bool GetDown()
		{
			if (!m_shown)
			{
				return m_ratio > 0.5f;
			}
			return true;
		}

		public Vector2 GetOffset()
		{
			return m_offset;
		}

		public void HighlightForTime(float time)
		{
			m_highlightPopupTimer = time;
		}

		public void Show()
		{
			if (m_forceOff)
			{
				return;
			}
			if (!m_shown)
			{
				SystemAudio.Play(m_soundShow);
			}
			m_shown = true;
			if (m_guiComponent != null)
			{
				m_guiComponent.GetData().Clickable = true;
				if (m_pauseWhenVisible && m_highlightPopupTimer <= 0f)
				{
					Singleton<PowerQuest>.Get.Pause(base.gameObject.name);
					m_triggeredPause = true;
				}
			}
			if (m_ratio > 0f)
			{
				m_blendTimer = BLEND_TIME;
				m_blendOffset = m_offset;
			}
		}

		public void Hide()
		{
			if (m_shown)
			{
				SystemAudio.Play(m_soundHide);
			}
			m_shown = false;
			if (m_guiComponent != null)
			{
				m_guiComponent.GetData().Clickable = true;
				if (m_pauseWhenVisible && m_triggeredPause)
				{
					Singleton<PowerQuest>.Get.UnPause(base.gameObject.name);
					m_triggeredPause = false;
				}
			}
			if (m_ratio < 1f)
			{
				m_blendTimer = BLEND_TIME;
				m_blendOffset = m_offset;
			}
			m_delayShow = true;
		}

		private void OnEnable()
		{
			m_guiComponent = GetComponentInParent<GuiComponent>();
			m_ratio = 0f;
			m_shown = false;
			m_blendTimer = 0f;
			Update();
		}

		private void OnDisable()
		{
			if (m_triggeredPause && Singleton<PowerQuest>.Exists)
			{
				Singleton<PowerQuest>.Get.UnPause(base.gameObject.name);
				m_triggeredPause = false;
			}
		}

		private void OnDestroy()
		{
			OnDisable();
		}

		private void Update()
		{
			if (m_highlightPopupTimer > 0f)
			{
				m_highlightPopupTimer -= Time.deltaTime;
			}
			RectTransform component = GetComponent<RectTransform>();
			AlignToScreen component2 = GetComponent<AlignToScreen>();
			if (component != null)
			{
				component.localPosition -= m_offset.WithZ(0f);
			}
			else if (component2 != null)
			{
				component2.Offset -= m_offset;
			}
			else
			{
				base.transform.Translate(-m_offset);
			}
			bool isGuiObscuredByModal = Singleton<PowerQuest>.Get.GetIsGuiObscuredByModal(m_guiComponent.GetData());
			if (m_shown)
			{
				if (m_ratio < 1f && m_curveIn.keys.Length != 0)
				{
					float time = m_curveIn.keys[m_curveIn.keys.Length - 1].time;
					m_ratio += 1f / time * Time.deltaTime;
					m_ratio = Mathf.Clamp01(m_ratio);
					m_offset.y = m_curveIn.Evaluate(m_ratio * time) * m_dropDownDistance;
				}
				else
				{
					m_offset.y = 0f;
				}
				if (m_forceOff || (!Singleton<PowerQuest>.Get.GetBlocked() && (!CalcMouseInBounds(m_mouseEdgeDistanceHide) || isGuiObscuredByModal) && m_highlightPopupTimer <= 0f))
				{
					Hide();
				}
			}
			else
			{
				if (m_ratio > 0f && m_curveOut.keys.Length != 0)
				{
					float time2 = m_curveOut.keys[m_curveOut.keys.Length - 1].time;
					m_ratio -= 1f / time2 * Time.deltaTime;
					m_ratio = Mathf.Clamp01(m_ratio);
					m_offset.y = m_curveOut.Evaluate((1f - m_ratio) * time2) * m_dropDownDistance;
				}
				else
				{
					m_offset.y = m_dropDownDistance;
				}
				if (!m_delayShow && !Singleton<PowerQuest>.Get.GetBlocked() && ((CalcMouseInBounds(m_mouseEdgeDistanceShow) && !isGuiObscuredByModal) || m_highlightPopupTimer > 0f))
				{
					Show();
				}
			}
			if (m_blendTimer > 0f)
			{
				m_blendTimer -= Time.deltaTime;
				m_offset = Vector2.Lerp(m_offset, m_blendOffset, m_blendTimer / BLEND_TIME);
			}
			if (m_hideDuringCutscenes && Singleton<PowerQuest>.Get.GetBlocked())
			{
				m_offset.y += m_dropDownDistance * 10f;
			}
			if (component != null)
			{
				component.localPosition += m_offset.WithZ(0f);
			}
			else if (component2 != null)
			{
				component2.Offset += m_offset;
				component2.ForceUpdate();
			}
			else
			{
				base.transform.Translate(m_offset);
			}
			m_delayShow = false;
		}

		private bool CalcMouseInBounds(Vector2 size)
		{
			Camera cameraGui = Singleton<PowerQuest>.Get.GetCameraGui();
			if (cameraGui == null)
			{
				return false;
			}
			Vector2 point = ((Vector2)cameraGui.ScreenToViewportPoint(Input.mousePosition)).Clamp(new Vector2(0.01f, 0.01f), new Vector2(0.99f, 0.99f));
			if (size.x > 1f)
			{
				size.x = cameraGui.WorldToViewportPoint(cameraGui.transform.position + Vector3.zero.WithX(size.x)).x - 0.5f;
			}
			if (size.y > 1f)
			{
				size.y = cameraGui.WorldToViewportPoint(cameraGui.transform.position + Vector3.zero.WithY(size.y)).y - 0.5f;
			}
			Vector2 zero = Vector2.zero;
			switch (m_edgeX)
			{
			case eScreenEdgeX.Center:
				zero.x = size.x;
				break;
			case eScreenEdgeX.Left:
				zero.x = 0f;
				break;
			case eScreenEdgeX.Right:
				zero.x = 1f - size.x;
				break;
			}
			switch (m_edgeY)
			{
			case eScreenEdgeY.Middle:
				zero.y = size.y;
				break;
			case eScreenEdgeY.Bottom:
				zero.y = 0f;
				break;
			case eScreenEdgeY.Top:
				zero.y = 1f - size.y;
				break;
			}
			if (m_edgeX == eScreenEdgeX.Center)
			{
				size.x = 1f - size.x * 2f;
			}
			if (m_edgeY == eScreenEdgeY.Middle)
			{
				size.y = 1f - size.y * 2f;
			}
			return new Rect(zero.x, zero.y, size.x, size.y).Contains(point);
		}
	}
}
