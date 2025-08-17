using UnityEngine;

namespace PowerTools.Quest
{
	[ExecuteInEditMode]
	[AddComponentMenu("Quest Gui Layout/Fit To Screen")]
	public class FitToScreen : MonoBehaviour
	{
		[SerializeField]
		private bool m_fitWidth;

		[SerializeField]
		private bool m_fitHeight;

		[SerializeField]
		private Padding m_padding = Padding.zero;

		[SerializeField]
		private bool m_snapToPixel;

		[Header("Camera override (optional")]
		[SerializeField]
		private Camera m_camera;

		private Vector2 m_spriteSizeInverted = Vector2.one;

		private SpriteRenderer m_sprite;

		private float m_timeToUpdate;

		public Padding Padding
		{
			get
			{
				return m_padding;
			}
			set
			{
				m_padding = value;
				UpdateSize();
			}
		}

		public void UpdateSize()
		{
			if (m_sprite == null && !Application.isPlaying)
			{
				SetupSprite();
			}
			bool flag = m_sprite != null && m_sprite.drawMode != SpriteDrawMode.Simple;
			Vector2 vector = base.transform.position;
			Vector2 vector2 = base.transform.localScale;
			if (flag)
			{
				vector2 = m_sprite.size;
			}
			if (m_camera == null)
			{
				m_camera = GuiUtils.FindGuiCamera();
			}
			if (m_camera == null)
			{
				return;
			}
			RectCentered rectCentered = new RectCentered(m_camera.pixelRect);
			rectCentered.Min = m_camera.ScreenToWorldPoint(rectCentered.Min);
			rectCentered.Max = m_camera.ScreenToWorldPoint(rectCentered.Max);
			if (m_snapToPixel)
			{
				rectCentered.MinX = Utils.Snap(rectCentered.MinX);
				rectCentered.MaxX = Utils.Snap(rectCentered.MaxX);
			}
			if (m_fitWidth)
			{
				if (flag)
				{
					vector2.x = rectCentered.Width + m_padding.width;
				}
				else
				{
					vector2.x = rectCentered.Width * m_spriteSizeInverted.x + m_padding.width;
				}
				vector.x = rectCentered.Center.x + (0f - m_padding.left + m_padding.right) * 0.5f;
			}
			if (m_fitHeight)
			{
				if (flag)
				{
					vector2.y = rectCentered.Height + m_padding.height;
				}
				else
				{
					vector2.y = rectCentered.Height * m_spriteSizeInverted.y + m_padding.height;
				}
				vector.y = rectCentered.Center.y + (0f - m_padding.bottom + m_padding.top) * 0.5f;
			}
			if (flag)
			{
				m_sprite.size = vector2;
			}
			else
			{
				base.transform.localScale = vector2.WithZ(1f);
			}
			base.transform.position = vector.WithZ(base.transform.position.z);
		}

		private void Awake()
		{
			SetupSprite();
		}

		private void SetupSprite()
		{
			m_sprite = GetComponentInChildren<SpriteRenderer>();
			Vector2 vector = Vector2.one;
			if (m_sprite != null && m_sprite.sharedMaterial != null && m_sprite.sharedMaterial.mainTexture != null)
			{
				vector = new Vector2(m_sprite.sharedMaterial.mainTexture.width, m_sprite.sharedMaterial.mainTexture.height);
			}
			m_spriteSizeInverted.x = 1f / vector.x;
			m_spriteSizeInverted.y = 1f / vector.y;
		}

		private void OnEnable()
		{
			UpdateSize();
		}

		public void ForceUpdate()
		{
			m_timeToUpdate = 0f;
		}

		private void LateUpdate()
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
			UpdateSize();
		}
	}
}
