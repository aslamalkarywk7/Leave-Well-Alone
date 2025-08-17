using System;
using UnityEngine;

namespace PowerTools.Quest
{
	[Serializable]
	public class QuestCamera : ICamera
	{
		[Tooltip("Offset from the target character that the camera tries to center on")]
		[SerializeField]
		private Vector2 m_offsetFromCharacter = new Vector2(0f, 30f);

		private QuestCameraComponent m_instance;

		private string m_characterToFollow;

		private bool m_hasPositionOverride;

		private Vector2 m_positionOverride = new Vector2(float.MaxValue, float.MaxValue);

		private Vector2 m_positionOverridePrev = new Vector2(float.MaxValue, float.MaxValue);

		private float m_zoom = 1f;

		private float m_zoomPrev = 1f;

		private bool m_hasZoom;

		private Vector2 m_position = Vector2.zero;

		private Vector2 m_targetPosition = Vector2.zero;

		private bool m_enabled = true;

		private bool m_ignoreBounds;

		public bool Enabled
		{
			get
			{
				return m_enabled;
			}
			set
			{
				bool flag = !m_enabled && value;
				m_enabled = value;
				if (m_instance != null)
				{
					m_instance.enabled = value;
					if (flag)
					{
						m_instance.Snap();
					}
				}
			}
		}

		public bool IgnoreBounds
		{
			get
			{
				return m_ignoreBounds;
			}
			set
			{
				m_ignoreBounds = value;
				if (m_instance != null)
				{
					m_instance.Snap();
				}
			}
		}

		public Vector2 OffsetFromCharacter
		{
			get
			{
				return m_offsetFromCharacter;
			}
			set
			{
				m_offsetFromCharacter = value;
			}
		}

		public float ShakeIntensity => m_instance?.ShakeIntensity ?? 0f;

		public float Zoom
		{
			get
			{
				return m_zoom;
			}
			set
			{
				SetZoom(value);
			}
		}

		public Vector2 Position
		{
			get
			{
				return m_position;
			}
			set
			{
				SetPositionOverride(value);
			}
		}

		public Vector2 Velocity
		{
			get
			{
				if (!(m_instance == null))
				{
					return m_instance.Velocity;
				}
				return Vector2.zero;
			}
		}

		public Camera Camera
		{
			get
			{
				if (!(m_instance == null))
				{
					return m_instance.Camera;
				}
				return null;
			}
		}

		public QuestCameraComponent GetInstance()
		{
			return m_instance;
		}

		public void SetInstance(QuestCameraComponent instance)
		{
			m_instance = instance;
			m_instance.SetData(this);
		}

		public ICharacter GetCharacterToFollow()
		{
			return Singleton<PowerQuest>.Get.GetCharacter(m_characterToFollow);
		}

		public bool GetHasPositionOverride()
		{
			return m_hasPositionOverride;
		}

		public Vector2 GetPositionOverride()
		{
			return m_positionOverride;
		}

		public bool GetHasPositionOverrideOrTransition()
		{
			if (m_instance != null)
			{
				return m_instance.GetHasPositionOverrideOrTransitioning();
			}
			return m_hasPositionOverride;
		}

		public bool GetTransitioning()
		{
			if (!(m_instance == null))
			{
				return m_instance.GetTransitioning();
			}
			return false;
		}

		public void SetCharacterToFollow(ICharacter character, float overTime = 0f)
		{
			m_characterToFollow = character.ScriptName;
			if (overTime > 0f)
			{
				SetPositionOverride(m_position);
				ResetPositionOverride(overTime);
			}
			else if (m_instance != null)
			{
				m_instance.Snap();
			}
		}

		public void SetPositionOverride(float x, float y = 0f, float transitionTime = 0f)
		{
			m_hasPositionOverride = true;
			m_positionOverride = new Vector2(x, y);
			if (m_instance != null)
			{
				m_instance.OnOverridePosition(transitionTime);
			}
		}

		public void SetPositionOverride(Vector2 positionOverride, float transitionTime = 0f)
		{
			SetPositionOverride(positionOverride.x, positionOverride.y, transitionTime);
		}

		public void ResetPositionOverride(float transitionTime = 0f)
		{
			m_hasPositionOverride = false;
			if (m_instance != null)
			{
				m_instance.OnOverridePosition(transitionTime);
			}
		}

		public float GetZoom()
		{
			if (!(m_zoom > 0f))
			{
				return 1f;
			}
			return m_zoom;
		}

		public bool GetHasZoom()
		{
			return m_hasZoom;
		}

		public float GetZoomPrev()
		{
			if (!(m_zoomPrev > 0f))
			{
				return 1f;
			}
			return m_zoomPrev;
		}

		public bool GetHasZoomOrTransition()
		{
			if (!(m_instance != null))
			{
				return m_hasZoom;
			}
			return m_instance.GetHasZoomOrTransitioning();
		}

		public void SetZoom(float zoom, float transitionTime = 0f)
		{
			if (m_hasZoom)
			{
				m_zoomPrev = m_zoom;
			}
			else
			{
				m_zoomPrev = zoom;
			}
			m_hasZoom = true;
			m_zoom = zoom;
			if (m_instance != null)
			{
				m_instance.OnZoom(transitionTime);
			}
		}

		public void ResetZoom(float transitionTime = 0f)
		{
			m_hasZoom = false;
			if (m_instance != null)
			{
				m_instance.OnZoom(transitionTime);
			}
		}

		public void Snap()
		{
			m_instance.Snap();
		}

		public Vector2 GetPosition()
		{
			return m_position;
		}

		public void SetPosition(Vector2 position)
		{
			m_position = position;
		}

		public Vector2 GetTargetPosition()
		{
			return m_targetPosition;
		}

		public void SetTargetPosition(Vector2 position)
		{
			m_targetPosition = position;
		}

		public bool GetSnappedLastUpdate()
		{
			if (!(m_instance == null))
			{
				return m_instance.GetSnappedLastUpdate();
			}
			return true;
		}

		public bool GetTargetPosChangedLastUpdate()
		{
			if (!(m_instance == null))
			{
				return m_instance.GetTargetChangedLastUpdate();
			}
			return true;
		}

		public float GetTransitionTime()
		{
			if (!(m_instance == null))
			{
				return m_instance.GetTransitionTime();
			}
			return 0f;
		}

		public void Shake(float intensity, float duration = 0.1f, float falloff = 0.15f)
		{
			m_instance.Shake(intensity, duration, falloff);
		}

		public void Shake(CameraShakeData data)
		{
			m_instance.Shake(data.m_intensity, data.m_duration, data.m_falloff);
		}
	}
}
