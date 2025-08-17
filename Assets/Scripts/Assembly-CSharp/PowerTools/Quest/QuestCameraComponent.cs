using UnityEngine;

namespace PowerTools.Quest
{
	public class QuestCameraComponent : MonoBehaviour
	{
		public class StateData
		{
			public Vector2 position = Vector2.zero;

			public Vector2 targetPosition = Vector2.zero;

			public float zoom = 1f;

			public bool followPlayer = true;

			public Vector2 playerDragged = Vector2.zero;

			public Vector2 playerPosCached = Vector2.zero;
		}

		[SerializeField]
		private QuestCamera m_data;

		[SerializeField]
		private float m_smoothingFactor = 10f;

		[SerializeField]
		private float m_smoothingMinSpeed = 10f;

		[Tooltip("How far player has to move before scrolling starts")]
		[SerializeField]
		private Vector2 m_distFromPlayerBeforeScroll = new Vector2(25f, 10f);

		[SerializeField]
		private float m_characterFacingOffset;

		[Header("Screenshake global multipliers")]
		[SerializeField]
		private float m_shakeIntensityMult = 1f;

		[SerializeField]
		private float m_shakeFalloffMult = 1f;

		[SerializeField]
		private float m_shakeSpeed = 40f;

		[Header("Prefab References")]
		[SerializeField]
		private GameObject m_prefabPixelCam;

		private float m_shakeIntensity;

		private float m_shakeFalloff = 1f;

		private float m_shakeDurationTimer;

		private Vector2 m_screenShakeOffset = Vector2.zero;

		private bool m_onLerpChange;

		private float m_lerpTime;

		private float m_lerpTimer;

		private bool m_posLerpActive;

		private bool m_zoomLerpActive;

		private bool m_targetPositionChanged;

		private Vector2 m_cachedTargetNoPixelSnap = Vector2.zero;

		private Camera m_camera;

		private GameObject m_pixelCam;

		private StateData m_state = new StateData();

		private StateData m_statePrev = new StateData();

		private StateData m_stateParallax = new StateData();

		private StateData m_stateParallaxPrev = new StateData();

		private eFace m_playerFaceLast = eFace.Down;

		private bool m_snappedSinceUpdate = true;

		private bool m_snappedLastUpdate = true;

		private Vector2 m_parallaxPos = Vector2.zero;

		private Vector2 m_parallaxTargetPos = Vector2.zero;

		private RectCentered m_parallaxOffsetLimits;

		private Vector2 m_velocity = Vector2.zero;

		private bool m_lockParallaxAlignment;

		public Camera Camera => m_camera;

		public Vector2 Velocity => m_velocity;

		public bool LockParallaxAlignment
		{
			get
			{
				return m_lockParallaxAlignment;
			}
			set
			{
				m_lockParallaxAlignment = value;
				m_parallaxOffsetLimits = CalcOffsetLimits(1f);
			}
		}

		public float ShakeIntensity => m_shakeIntensity;

		public QuestCamera GetData()
		{
			return m_data;
		}

		public void SetData(QuestCamera data)
		{
			m_data = data;
		}

		public void OnEnterRoom()
		{
			Snap();
		}

		public void Snap()
		{
			ResetPlayerDragPos(m_stateParallax);
			ResetPlayerDragPos(m_state);
			UpdatePos(snap: true);
		}

		public bool GetSnappedLastUpdate()
		{
			return m_snappedLastUpdate;
		}

		public void OnOverridePosition(float transitionTime)
		{
			m_lerpTime = transitionTime;
			m_lerpTimer = transitionTime;
			m_onLerpChange = true;
			if (transitionTime <= 0f)
			{
				Snap();
			}
		}

		public bool GetTransitioning()
		{
			if (!m_posLerpActive)
			{
				return m_zoomLerpActive;
			}
			return true;
		}

		public bool GetHasPositionOverrideOrTransitioning()
		{
			if (!m_data.GetHasPositionOverride())
			{
				return m_posLerpActive;
			}
			return true;
		}

		public void OnZoom(float transitionTime)
		{
			OnOverridePosition(transitionTime);
		}

		public bool GetHasZoomOrTransitioning()
		{
			if (!m_data.GetHasZoom())
			{
				return m_zoomLerpActive;
			}
			return true;
		}

		private Vector2 GetHalfCamSize(float zoomMult)
		{
			float num = Singleton<PowerQuest>.Get.VerticalResolution * 0.5f / zoomMult;
			return new Vector2(num * m_camera.aspect, num);
		}

		public Vector2 GetPositionForParallax()
		{
			return m_parallaxPos;
		}

		public Vector2 GetParallaxTargetPosition()
		{
			return m_parallaxTargetPos;
		}

		public RectCentered GetParallaxOffsetLimits()
		{
			return m_parallaxOffsetLimits;
		}

		private RectCentered CalcOffsetLimits(float zoomMult)
		{
			Vector2 halfCamSize = GetHalfCamSize(zoomMult);
			RectCentered bounds = Singleton<PowerQuest>.Get.GetCurrentRoom().Bounds;
			bounds.Min += halfCamSize;
			bounds.Max -= halfCamSize;
			if (bounds.Width < 0f)
			{
				bounds.Width = 0f;
			}
			if (bounds.Height < 0f)
			{
				bounds.Height = 0f;
			}
			return bounds;
		}

		public Vector2 ClampPositionToRoomBounds(float zoomMult, Vector2 position)
		{
			if (m_data.IgnoreBounds)
			{
				return position;
			}
			RectCentered rectCentered = CalcOffsetLimits(zoomMult);
			position.x = Mathf.Clamp(position.x, rectCentered.Min.x, rectCentered.Max.x);
			position.y = Mathf.Clamp(position.y, rectCentered.Min.y, rectCentered.Max.y);
			return position;
		}

		private void ResetPlayerDragPos(StateData s)
		{
			s.playerDragged = (s.playerPosCached = GetCharacterTargetPos(s));
		}

		public Vector2 GetCharacterTargetPos(StateData s)
		{
			Vector2 result = Vector2.zero;
			ICharacter characterToFollow = m_data.GetCharacterToFollow();
			if (characterToFollow != null || Singleton<PowerQuest>.Get.GetCurrentRoom() != characterToFollow.Room)
			{
				if (m_playerFaceLast != characterToFollow.Facing && (characterToFollow.Facing == eFace.Left || characterToFollow.Facing == eFace.Right))
				{
					m_playerFaceLast = characterToFollow.Facing;
				}
				result = characterToFollow.Position + m_data.OffsetFromCharacter;
				if (characterToFollow.Walking)
				{
					if (m_playerFaceLast == eFace.Left)
					{
						result.x -= m_characterFacingOffset / s.zoom;
					}
					else if (m_playerFaceLast == eFace.Right)
					{
						result.x += m_characterFacingOffset / s.zoom;
					}
				}
			}
			return result;
		}

		public Vector2 GetCameraFollowTargetPosition(StateData s, bool disablePixelSnap = false)
		{
			if (Singleton<PowerQuest>.Get == null && m_data == null)
			{
				return Vector2.zero;
			}
			Vector2 vector = s.position;
			ICharacter characterToFollow = m_data.GetCharacterToFollow();
			if (characterToFollow != null && Singleton<PowerQuest>.Get.GetCurrentRoom() == characterToFollow.Room)
			{
				Vector2 characterTargetPos = GetCharacterTargetPos(s);
				Vector2 vector2 = m_distFromPlayerBeforeScroll / s.zoom;
				if (characterTargetPos.x > s.playerPosCached.x)
				{
					if (characterTargetPos.x > s.playerDragged.x + vector2.x)
					{
						s.playerDragged.x = characterTargetPos.x - vector2.x;
					}
				}
				else if (characterTargetPos.x < s.playerDragged.x - vector2.x)
				{
					s.playerDragged.x = characterTargetPos.x + vector2.x;
				}
				if (characterTargetPos.y > s.playerPosCached.y)
				{
					if (characterTargetPos.y > s.playerDragged.y + vector2.y)
					{
						s.playerDragged.y = characterTargetPos.y - vector2.y;
					}
				}
				else if (characterTargetPos.y < s.playerDragged.y - vector2.y)
				{
					s.playerDragged.y = characterTargetPos.y + vector2.y;
				}
				s.playerPosCached = characterTargetPos;
				if (m_camera != null)
				{
					RectCentered scrollBounds = Singleton<PowerQuest>.Get.GetCurrentRoom().ScrollBounds;
					RectCentered rectCentered = CalcOffsetLimits(s.zoom);
					if (scrollBounds.Width <= 0f)
					{
						vector.x = s.playerDragged.x;
					}
					else
					{
						if (s.zoom != 1f)
						{
							scrollBounds.MinX = rectCentered.MinX + (scrollBounds.MinX - rectCentered.MinX) / s.zoom;
							scrollBounds.MaxX = rectCentered.MaxX + (scrollBounds.MaxX - rectCentered.MaxX) / s.zoom;
						}
						vector.x = Mathf.Lerp(rectCentered.Min.x, rectCentered.Max.x, Mathf.InverseLerp(scrollBounds.Min.x, scrollBounds.Max.x, s.playerDragged.x));
					}
					if (scrollBounds.Height <= 0f)
					{
						vector.y = s.playerDragged.y;
					}
					else
					{
						if (s.zoom != 1f)
						{
							scrollBounds.MinY = rectCentered.MinY + (scrollBounds.MinY - rectCentered.MinY) / s.zoom;
							scrollBounds.MaxY = rectCentered.MaxY + (scrollBounds.MaxY - rectCentered.MaxY) / s.zoom;
						}
						vector.y = Mathf.Lerp(rectCentered.Min.y, rectCentered.Max.y, Mathf.InverseLerp(scrollBounds.Min.y, scrollBounds.Max.y, s.playerDragged.y));
					}
				}
			}
			if (!disablePixelSnap)
			{
				vector = Utils.Snap(vector, Singleton<PowerQuest>.Get.SnapAmount);
			}
			return ClampPositionToRoomBounds(s.zoom, vector);
		}

		public bool GetTargetChangedLastUpdate()
		{
			return m_targetPositionChanged;
		}

		public float GetTransitionTime()
		{
			return m_lerpTime;
		}

		private void Awake()
		{
			m_camera = GetComponent<Camera>();
		}

		private void Start()
		{
			if (Singleton<PowerQuest>.Get.GetPixelCamEnabled() && m_prefabPixelCam != null)
			{
				m_pixelCam = Object.Instantiate(m_prefabPixelCam);
				int num = LayerMask.NameToLayer("HighRes");
				m_pixelCam.GetComponent<Camera>().cullingMask = Utils.MaskUnsetAt(m_camera.cullingMask, num);
				m_pixelCam.transform.GetChild(0).gameObject.layer = num;
				m_camera.cullingMask = 1 << num;
			}
			m_onLerpChange = true;
		}

		private void Update()
		{
			if (!m_snappedSinceUpdate)
			{
				m_snappedLastUpdate = false;
			}
			m_snappedSinceUpdate = false;
			UpdatePos(Singleton<PowerQuest>.Get.GetSkippingCutscene());
		}

		private void LateUpdate()
		{
			if (m_pixelCam != null)
			{
				m_pixelCam.transform.position = Utils.Snap(base.transform.position).WithZ(m_pixelCam.transform.position.z);
			}
		}

		private void UpdatePos(bool snap)
		{
			if (snap)
			{
				m_snappedSinceUpdate = true;
				m_snappedLastUpdate = true;
			}
			if (!m_data.Enabled || Time.deltaTime == 0f)
			{
				return;
			}
			Vector2 position = m_data.GetPosition();
			Vector2 vector = position;
			Vector2 vector2 = position;
			float num = 1f;
			float num2 = Singleton<PowerQuest>.Get.VerticalResolution * 0.5f;
			if (snap)
			{
				m_velocity = Vector2.zero;
			}
			else
			{
				m_velocity = (position - vector) / Time.deltaTime;
			}
			m_targetPositionChanged = false;
			if (m_onLerpChange)
			{
				m_onLerpChange = false;
				QuestUtils.CopyFields(m_stateParallaxPrev, m_stateParallax);
				QuestUtils.CopyFields(m_statePrev, m_state);
				m_state.zoom = (m_data.GetHasZoom() ? m_data.GetZoom() : 1f);
				if (m_data.GetHasPositionOverride())
				{
					if (!m_statePrev.followPlayer)
					{
						m_statePrev.position = position;
						m_statePrev.targetPosition = position;
						m_stateParallaxPrev.position = position;
						m_stateParallaxPrev.targetPosition = position;
					}
					m_stateParallax.followPlayer = false;
					m_stateParallax.position = ClampPositionToRoomBounds(1f, m_data.GetPositionOverride());
					m_stateParallax.targetPosition = m_stateParallax.position;
					m_state.followPlayer = false;
					m_state.position = ClampPositionToRoomBounds(m_state.zoom, m_data.GetPositionOverride());
					m_state.targetPosition = m_state.position;
					m_targetPositionChanged = true;
				}
				else
				{
					m_stateParallax.followPlayer = true;
					m_state.followPlayer = true;
					ResetPlayerDragPos(m_stateParallax);
					ResetPlayerDragPos(m_state);
				}
				m_posLerpActive = m_stateParallax.followPlayer != m_stateParallaxPrev.followPlayer || (!m_stateParallax.followPlayer && m_stateParallax.position != m_stateParallaxPrev.position);
				m_zoomLerpActive = m_state.zoom != m_statePrev.zoom;
			}
			float t = 1f;
			if (snap)
			{
				m_lerpTimer = 0f;
			}
			if (m_lerpTimer > 0f)
			{
				m_lerpTimer -= Time.deltaTime;
				if (m_lerpTimer <= 0f)
				{
					m_posLerpActive = false;
					m_zoomLerpActive = false;
				}
				else if (m_lerpTime > 0f)
				{
					t = Mathf.Clamp01(1f - m_lerpTimer / m_lerpTime);
					t = QuestUtils.Ease(t);
				}
			}
			UpdateCameraState(m_stateParallax, snap, allowZoom: false);
			if (m_posLerpActive)
			{
				UpdateCameraState(m_stateParallaxPrev, snap, allowZoom: false);
			}
			UpdateCameraState(m_state, snap, allowZoom: true);
			if (m_posLerpActive || m_zoomLerpActive)
			{
				UpdateCameraState(m_statePrev, snap, allowZoom: true);
			}
			vector2 = Vector2.Lerp(m_statePrev.targetPosition, m_state.targetPosition, t);
			RectCentered rectCentered = new RectCentered(m_statePrev.position.x, m_statePrev.position.y, GetHalfCamSize(m_statePrev.zoom).x, GetHalfCamSize(m_statePrev.zoom).y);
			RectCentered rectCentered2 = new RectCentered(m_state.position.x, m_state.position.y, GetHalfCamSize(m_state.zoom).x, GetHalfCamSize(m_state.zoom).y);
			RectCentered rectCentered3 = new RectCentered(Vector2.Lerp(rectCentered.Min, rectCentered2.Min, t), Vector2.Lerp(rectCentered.Max, rectCentered2.Max, t));
			position = rectCentered3.Center;
			num2 = rectCentered3.Height;
			num = Singleton<PowerQuest>.Get.VerticalResolution * 0.5f / num2;
			position = ClampPositionToRoomBounds(num, position);
			m_parallaxPos = Vector2.Lerp(m_stateParallaxPrev.position, m_stateParallax.position, t);
			m_parallaxTargetPos = m_stateParallax.targetPosition;
			if (!LockParallaxAlignment)
			{
				m_parallaxOffsetLimits = CalcOffsetLimits(1f);
			}
			m_screenShakeOffset = Vector2.zero;
			if (m_shakeIntensity > 0f)
			{
				m_screenShakeOffset = (new Vector2(Mathf.PerlinNoise(m_shakeSpeed * Time.time, 0f), Mathf.PerlinNoise(1f, m_shakeSpeed * Time.time)) * 2f - Vector2.one) * m_shakeIntensity * m_shakeIntensityMult / num;
				if (m_shakeDurationTimer > 0f)
				{
					m_shakeDurationTimer -= Time.deltaTime;
					if (m_shakeDurationTimer <= 0f)
					{
						if (m_shakeFalloff > 0f)
						{
							m_shakeIntensity -= (0f - m_shakeDurationTimer) / m_shakeFalloff;
						}
						else
						{
							m_shakeIntensity = 0f;
						}
					}
				}
				else if (m_shakeFalloff > 0f)
				{
					m_shakeIntensity -= Time.deltaTime / m_shakeFalloff;
				}
				else
				{
					m_shakeIntensity = 0f;
				}
			}
			m_data.SetPosition(position);
			m_data.SetTargetPosition(vector2);
			base.transform.position = (m_screenShakeOffset + position).WithZ(base.transform.position.z);
			m_camera.orthographicSize = num2;
		}

		private void UpdateCameraState(StateData s, bool snap, bool allowZoom)
		{
			Vector2 vector = (s.targetPosition = s.position);
			Vector2 vector2 = vector;
			if (s.followPlayer)
			{
				s.position = GetCameraFollowTargetPosition(s);
				s.targetPosition = s.position;
				vector2 = GetCameraFollowTargetPosition(s, disablePixelSnap: true);
				if (s == m_stateParallax)
				{
					m_targetPositionChanged = vector2 != m_cachedTargetNoPixelSnap;
					m_cachedTargetNoPixelSnap = vector2;
				}
			}
			if (!snap && s.followPlayer)
			{
				Vector2 vector3 = s.position - vector;
				float magnitude = vector3.magnitude;
				float num = Mathf.Max(m_smoothingMinSpeed, m_smoothingFactor * magnitude) * Time.deltaTime * s.zoom;
				if (magnitude > num)
				{
					s.position = vector + num * vector3.normalized;
				}
			}
		}

		public void Shake(CameraShakeData shakeData)
		{
			Shake(shakeData.m_intensity, shakeData.m_duration, shakeData.m_falloff);
		}

		public void Shake(float intensity = 1f, float duration = 0.1f, float falloff = 0.15f)
		{
			if (intensity > m_shakeIntensity)
			{
				m_shakeDurationTimer = duration;
				m_shakeIntensity = intensity;
				m_shakeFalloff = ((m_shakeIntensity <= 0f) ? 0f : (falloff * m_shakeFalloffMult / m_shakeIntensity));
			}
			else if (intensity == 0f)
			{
				m_shakeFalloff = 0f;
			}
		}

		private void MsgShake(float intensity, float duration, float falloff)
		{
			Shake(intensity, duration, falloff);
		}

		private void MsgShake(float intensity, float duration)
		{
			Shake(intensity, duration);
		}

		private void MsgShake(float intensity)
		{
			Shake(intensity);
		}
	}
}
