using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace PowerTools.Quest
{
	[ExecuteInEditMode]
	[AddComponentMenu("Quest Gui Layout/Fit To Object")]
	public class FitToObject : MonoBehaviour
	{
		[Flags]
		private enum eFitWhat
		{
			Sprite = 1,
			Collider = 2,
			GridContainer = 4
		}

		[SerializeField]
		private eFitWhat m_fitWhat = eFitWhat.Sprite;

		[Header("Around Objects:")]
		[FormerlySerializedAs("m_containX")]
		[SerializeField]
		private List<GameObject> m_fitWidth = new List<GameObject>();

		[FormerlySerializedAs("m_containY")]
		[SerializeField]
		private List<GameObject> m_fitHeight = new List<GameObject>();

		[Header("With Padding:")]
		[SerializeField]
		private Padding m_padding = Padding.zero;

		[Header("Options")]
		[SerializeField]
		private bool m_snapToPixel = true;

		[SerializeField]
		[Tooltip("Fit the children of specified objects too?")]
		private bool m_includeChildren = true;

		private Vector2 m_spriteSizeInverted = Vector2.one;

		private SpriteRenderer m_sprite;

		private BoxCollider2D m_boxCollider;

		private GridContainer m_gridContainer;

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
			bool flag = (m_fitWhat & eFitWhat.Sprite) != 0;
			bool flag2 = (m_fitWhat & eFitWhat.Collider) != 0;
			bool flag3 = (m_fitWhat & eFitWhat.GridContainer) != 0;
			if (!Application.isPlaying && ((m_sprite == null && flag) || (m_boxCollider == null && flag2) || (m_gridContainer == null && flag3)))
			{
				SetupSprite();
			}
			flag &= m_sprite != null && m_sprite.drawMode != SpriteDrawMode.Simple;
			flag2 &= m_boxCollider != null;
			flag3 &= m_gridContainer != null;
			Vector2 vector = base.transform.position;
			Vector2 vector2 = base.transform.localScale;
			if (flag)
			{
				vector2 = m_sprite.size;
				vector = m_sprite.transform.position;
			}
			else if (flag2)
			{
				vector = (Vector2)m_boxCollider.transform.position + m_boxCollider.offset;
				vector2 = m_boxCollider.size;
			}
			else if (flag3)
			{
				RectCentered rect = m_gridContainer.Rect;
				vector = rect.Center;
				vector2 = rect.Size;
			}
			Vector2 vector3 = vector;
			Vector2 vector4 = vector2;
			if (m_fitWidth != null && m_fitWidth.Count > 0)
			{
				bool flag4 = true;
				RectCentered zero = RectCentered.zero;
				for (int i = 0; i < m_fitWidth.Count; i++)
				{
					GameObject gameObject = m_fitWidth[i];
					if (gameObject == null || gameObject == base.gameObject || !gameObject || !gameObject.activeInHierarchy)
					{
						continue;
					}
					FitToObject component = gameObject.GetComponent<FitToObject>();
					if ((bool)component)
					{
						component.UpdateSize();
					}
					RectCentered rectCentered = GuiUtils.CalculateGuiRect(gameObject.transform, m_includeChildren, null, null, base.transform);
					if (rectCentered != RectCentered.zero)
					{
						rectCentered.Transform(gameObject.transform);
						if (flag4 && rectCentered != RectCentered.zero)
						{
							flag4 = false;
							zero.CenterX = rectCentered.Center.x;
						}
						zero.Encapsulate(rectCentered);
					}
				}
				zero.MinX -= Padding.left;
				zero.MaxX += Padding.right;
				if (m_snapToPixel)
				{
					zero.MinX = Utils.Snap(zero.MinX);
					zero.MaxX = Utils.Snap(zero.MaxX);
				}
				vector2.x = zero.Width;
				vector.x = zero.Center.x;
			}
			if (m_fitHeight != null && m_fitHeight.Count > 0)
			{
				bool flag5 = true;
				RectCentered zero2 = RectCentered.zero;
				for (int j = 0; j < m_fitHeight.Count; j++)
				{
					GameObject gameObject2 = m_fitHeight[j];
					if (gameObject2 == null || gameObject2 == base.gameObject || !gameObject2 || !gameObject2.activeInHierarchy)
					{
						continue;
					}
					FitToObject component2 = gameObject2.GetComponent<FitToObject>();
					if ((bool)component2)
					{
						component2.UpdateSize();
					}
					RectCentered rectCentered2 = GuiUtils.CalculateGuiRect(gameObject2.transform, m_includeChildren, null, null, base.transform);
					if (rectCentered2 != RectCentered.zero)
					{
						rectCentered2.Transform(gameObject2.transform);
						if (flag5 && rectCentered2 != RectCentered.zero)
						{
							flag5 = false;
							zero2.CenterY = rectCentered2.Center.y;
						}
						zero2.Encapsulate(rectCentered2);
					}
					else
					{
						zero2.Encapsulate(gameObject2.transform.position);
					}
				}
				zero2.MinY -= Padding.bottom;
				zero2.MaxY += Padding.top;
				if (m_snapToPixel)
				{
					zero2.MinY = Utils.Snap(zero2.MinY);
					zero2.MaxY = Utils.Snap(zero2.MaxY);
				}
				vector2.y = zero2.Height;
				vector.y = zero2.Center.y;
			}
			if (vector4 != vector2 || vector3 != vector)
			{
				if (flag)
				{
					m_sprite.size = vector2;
					m_sprite.transform.position = vector.WithZ(base.transform.position.z);
				}
				if (flag2)
				{
					m_boxCollider.size = vector2;
					m_boxCollider.offset = vector - (Vector2)m_boxCollider.transform.position;
				}
				if (flag3)
				{
					m_gridContainer.Rect = new RectCentered(vector.x, vector.y, vector2.x, vector2.y);
				}
			}
		}

		public void FitToObjectWidth(GameObject obj)
		{
			m_fitWidth.Add(obj);
			UpdateSize();
		}

		public void FitToObjectHeight(GameObject obj)
		{
			m_fitHeight.Add(obj);
			UpdateSize();
		}

		private void SetupSprite()
		{
			if ((m_fitWhat & eFitWhat.Sprite) != 0)
			{
				m_sprite = GetComponentInChildren<SpriteRenderer>();
			}
			if ((m_fitWhat & eFitWhat.Collider) != 0)
			{
				m_boxCollider = GetComponent<BoxCollider2D>();
			}
			if ((m_fitWhat & eFitWhat.GridContainer) != 0)
			{
				m_gridContainer = GetComponent<GridContainer>();
			}
			Vector2 vector = Vector2.one;
			if (m_sprite != null && m_sprite.sharedMaterial != null && m_sprite.sharedMaterial.mainTexture != null)
			{
				vector = new Vector2(m_sprite.sharedMaterial.mainTexture.width, m_sprite.sharedMaterial.mainTexture.height);
			}
			m_spriteSizeInverted.x = 1f / vector.x;
			m_spriteSizeInverted.y = 1f / vector.y;
		}

		private void Awake()
		{
			SetupSprite();
		}

		private void OnEnable()
		{
			UpdateSize();
		}

		private void LateUpdate()
		{
			UpdateSize();
		}
	}
}
