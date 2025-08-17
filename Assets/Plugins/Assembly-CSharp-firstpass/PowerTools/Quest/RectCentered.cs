using System;
using UnityEngine;

namespace PowerTools.Quest
{
	[Serializable]
	public struct RectCentered
	{
		[SerializeField]
		private Vector2 m_min;

		[SerializeField]
		private Vector2 m_max;

		public static readonly RectCentered zero = new RectCentered(0f, 0f, 0f, 0f);

		public Vector2 Center
		{
			get
			{
				return (m_min + m_max) * 0.5f;
			}
			set
			{
				Vector2 vector = value - Center;
				m_min += vector;
				m_max += vector;
			}
		}

		public float CenterX
		{
			get
			{
				return (m_min.x + m_max.x) * 0.5f;
			}
			set
			{
				float num = value - Center.x;
				m_min.x += num;
				m_max.x += num;
			}
		}

		public float CenterY
		{
			get
			{
				return (m_min.y + m_max.y) * 0.5f;
			}
			set
			{
				float num = value - Center.y;
				m_min.y += num;
				m_max.y += num;
			}
		}

		public Vector2 Size
		{
			get
			{
				return m_max - m_min;
			}
			set
			{
				Vector2 vector = (value - (m_max - m_min)) * 0.5f;
				m_min -= vector;
				m_max += vector;
			}
		}

		public float Width
		{
			get
			{
				return m_max.x - m_min.x;
			}
			set
			{
				float num = (value - (m_max.x - m_min.x)) * 0.5f;
				m_min.x -= num;
				m_max.x += num;
			}
		}

		public float Height
		{
			get
			{
				return m_max.y - m_min.y;
			}
			set
			{
				float num = (value - (m_max.y - m_min.y)) * 0.5f;
				m_min.y -= num;
				m_max.y += num;
			}
		}

		public Vector2 Min
		{
			get
			{
				return m_min;
			}
			set
			{
				m_min = value;
			}
		}

		public Vector2 Max
		{
			get
			{
				return m_max;
			}
			set
			{
				m_max = value;
			}
		}

		public float MinX
		{
			get
			{
				return m_min.x;
			}
			set
			{
				m_min.x = value;
			}
		}

		public float MaxX
		{
			get
			{
				return m_max.x;
			}
			set
			{
				m_max.x = value;
			}
		}

		public float MinY
		{
			get
			{
				return m_min.y;
			}
			set
			{
				m_min.y = value;
			}
		}

		public float MaxY
		{
			get
			{
				return m_max.y;
			}
			set
			{
				m_max.y = value;
			}
		}

		public RectCentered(float centerX, float centerY, float width, float height)
		{
			m_min = Vector2.zero;
			m_max = Vector2.zero;
			Center = new Vector2(centerX, centerY);
			Size = new Vector2(width, height);
		}

		public RectCentered(Rect rect)
		{
			m_min = rect.min;
			m_max = rect.max;
		}

		public RectCentered(RectCentered rect)
		{
			m_min = rect.Min;
			m_max = rect.Max;
		}

		public RectCentered(Vector2 min, Vector2 max)
		{
			m_min = min;
			m_max = max;
		}

		public RectCentered(Bounds bounds)
		{
			m_min = bounds.min;
			m_max = bounds.max;
		}

		public static implicit operator Rect(RectCentered self)
		{
			return new Rect(self.m_min, self.Size);
		}

		public static bool operator ==(RectCentered lhs, RectCentered rhs)
		{
			if (lhs.m_min == rhs.m_min)
			{
				return lhs.m_max == rhs.m_max;
			}
			return false;
		}

		public static bool operator !=(RectCentered lhs, RectCentered rhs)
		{
			if (!(lhs.m_min != rhs.m_min))
			{
				return lhs.m_max != rhs.m_max;
			}
			return true;
		}

		public override bool Equals(object rhs)
		{
			return this == (RectCentered)rhs;
		}

		public override int GetHashCode()
		{
			return m_min.GetHashCode() + (m_max.GetHashCode() + 1073741823);
		}

		public void Encapsulate(Vector2 point)
		{
			m_min.x = Mathf.Min(m_min.x, point.x);
			m_min.y = Mathf.Min(m_min.y, point.y);
			m_max.x = Mathf.Max(m_max.x, point.x);
			m_max.y = Mathf.Max(m_max.y, point.y);
		}

		public void Encapsulate(Vector2 point, float radius)
		{
			m_min.x = Mathf.Min(m_min.x, point.x - radius);
			m_min.y = Mathf.Min(m_min.y, point.y - radius);
			m_max.x = Mathf.Max(m_max.x, point.x + radius);
			m_max.y = Mathf.Max(m_max.y, point.y + radius);
		}

		public void EncapsulateLerp(Vector2 point, float radius, RectCentered original, float ratio)
		{
			if ((double)ratio >= 1.0)
			{
				Encapsulate(point, radius);
				return;
			}
			float num = point.x - radius;
			if (num < original.MinX)
			{
				num = Mathf.Lerp(original.MinX, num, ratio);
			}
			m_min.x = Mathf.Min(m_min.x, num);
			num = point.y - radius;
			if (num < original.MinY)
			{
				num = Mathf.Lerp(original.MinY, num, ratio);
			}
			m_min.y = Mathf.Min(m_min.y, num);
			num = point.x + radius;
			if (num > original.MaxX)
			{
				num = Mathf.Lerp(original.MaxX, num, ratio);
			}
			m_max.x = Mathf.Max(m_max.x, num);
			num = point.y + radius;
			if (num > original.MaxY)
			{
				num = Mathf.Lerp(original.MaxY, num, ratio);
			}
			m_max.y = Mathf.Max(m_max.y, num);
		}

		public void Encapsulate(RectCentered rect)
		{
			m_min.x = Mathf.Min(m_min.x, rect.Min.x);
			m_min.y = Mathf.Min(m_min.y, rect.Min.y);
			m_max.x = Mathf.Max(m_max.x, rect.Max.x);
			m_max.y = Mathf.Max(m_max.y, rect.Max.y);
		}

		public void Encapsulate(Bounds bounds)
		{
			m_min.x = Mathf.Min(m_min.x, bounds.min.x);
			m_min.y = Mathf.Min(m_min.y, bounds.min.y);
			m_max.x = Mathf.Max(m_max.x, bounds.max.x);
			m_max.y = Mathf.Max(m_max.y, bounds.max.y);
		}

		public void Transform(Transform transform)
		{
			Vector2 scale = transform.lossyScale;
			m_min.Scale(scale);
			m_max.Scale(scale);
			m_min += (Vector2)transform.position;
			m_max += (Vector2)transform.position;
		}

		public void UndoTransform(Transform transform)
		{
			m_min -= (Vector2)transform.position;
			m_max -= (Vector2)transform.position;
			Vector2 scale = new Vector2(1f / transform.lossyScale.x, 1f / transform.lossyScale.y);
			m_min.Scale(scale);
			m_max.Scale(scale);
		}

		public void AddPadding(Padding padding)
		{
			m_min.x -= padding.left;
			m_min.y -= padding.bottom;
			m_max.x += padding.right;
			m_max.y += padding.top;
		}

		public void RemovePadding(Padding padding)
		{
			m_min.x += padding.left;
			m_min.y += padding.bottom;
			m_max.x -= padding.right;
			m_max.y -= padding.top;
		}
	}
}
