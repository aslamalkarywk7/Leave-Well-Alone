using UnityEngine;

namespace PowerTools.Quest
{
	[ExecuteInEditMode]
	[AddComponentMenu("Quest Gui Layout/Align To Screen")]
	public class AlignToScreen : MonoBehaviour
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

		[Header("Align to the Screen's...")]
		[SerializeField]
		private eAlignVertical m_vertical;

		[SerializeField]
		private eAlignHorizontal m_horizontal;

		[Header("With offset...")]
		[SerializeField]
		private Vector2 m_offset = Vector2.zero;

		[SerializeField]
		private Vector2 m_offsetRatio = Vector2.zero;

		[Header("Optional camera override")]
		[SerializeField]
		private Camera m_camera;

		private float m_timeToUpdate;

		public Vector2 Offset
		{
			get
			{
				return m_offset;
			}
			set
			{
				m_offset = value;
				ForceUpdate();
			}
		}

		public Vector2 OffsetRatio
		{
			get
			{
				return m_offsetRatio;
			}
			set
			{
				m_offsetRatio = value;
				ForceUpdate();
			}
		}

		public void ForceUpdate()
		{
			m_timeToUpdate = 0f;
			if (base.isActiveAndEnabled)
			{
				Update();
			}
		}

		private void Start()
		{
			ForceUpdate();
		}

		private void OnEnable()
		{
			ForceUpdate();
		}

		private void Update()
		{
			if (Application.isPlaying)
			{
				m_timeToUpdate -= Time.deltaTime;
				if (m_timeToUpdate >= 0f)
				{
					return;
				}
				m_timeToUpdate = 0.25f;
			}
			if (m_camera == null)
			{
				m_camera = GuiUtils.FindGuiCamera();
			}
			if (!(m_camera == null))
			{
				Rect pixelRect = m_camera.pixelRect;
				Vector3 zero = Vector3.zero;
				switch (m_horizontal)
				{
				case eAlignHorizontal.Left:
					zero.x = pixelRect.xMin;
					break;
				case eAlignHorizontal.Center:
					zero.x = pixelRect.center.x;
					break;
				case eAlignHorizontal.Right:
					zero.x = pixelRect.xMax;
					break;
				}
				switch (m_vertical)
				{
				case eAlignVertical.Top:
					zero.y = pixelRect.yMax;
					break;
				case eAlignVertical.Middle:
					zero.y = pixelRect.center.y;
					break;
				case eAlignVertical.Bottom:
					zero.y = pixelRect.yMin;
					break;
				}
				Vector2 offsetRatio = m_offsetRatio;
				offsetRatio.Scale(new Vector2(pixelRect.width, pixelRect.height));
				zero += (Vector3)offsetRatio;
				zero = m_camera.ScreenToWorldPoint(zero);
				zero += (Vector3)m_offset;
				if (Singleton<PowerQuest>.GetValid() && Singleton<PowerQuest>.Get.GetSnapToPixel())
				{
					zero = zero.Snap(Singleton<PowerQuest>.Get.SnapAmount);
				}
				zero.z = base.transform.position.z;
				base.transform.position = new Vector3((m_horizontal == eAlignHorizontal.None) ? base.transform.position.x : zero.x, (m_vertical == eAlignVertical.None) ? base.transform.position.y : zero.y, zero.z);
			}
		}
	}
}
