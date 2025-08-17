using System;
using System.Collections.Generic;
using UnityEngine;

namespace PowerTools.Quest
{
	public class SystemDebug : SingletonAuto<SystemDebug>
	{
		private abstract class DebugElement
		{
			public Color m_color = Color.yellow;

			public float m_time;

			public abstract void Draw();
		}

		private class DebugLine : DebugElement
		{
			public Vector2 m_start = Vector2.zero;

			public Vector2 m_end = Vector2.zero;

			public override void Draw()
			{
				Debug.DrawLine(m_start, m_end, m_color, 0f, depthTest: false);
			}
		}

		private class DebugPoint : DebugElement
		{
			public Vector2 m_point = Vector2.zero;

			public override void Draw()
			{
				Debug.DrawLine(m_point.WithOffset(-1f, -1f), m_point.WithOffset(1f, 1f), m_color, 0f, depthTest: false);
				Debug.DrawLine(m_point.WithOffset(-1f, 1f), m_point.WithOffset(1f, -1f), m_color, 0f, depthTest: false);
			}
		}

		private class DebugText : DebugElement
		{
			public string text = string.Empty;

			public override void Draw()
			{
			}
		}

		private class DebugCircle : DebugElement
		{
			public Vector2 m_pos = Vector2.zero;

			public float m_radius = 1f;

			public override void Draw()
			{
				int num = 36;
				for (int i = 0; i < 360; i += num)
				{
					float f = (float)Math.PI / 180f * (float)(i - num);
					Vector2 vector = m_pos + new Vector2(m_radius * Mathf.Sin(f), m_radius * Mathf.Cos(f));
					f = (float)Math.PI / 180f * (float)i;
					Debug.DrawLine(end: m_pos + new Vector2(m_radius * Mathf.Sin(f), m_radius * Mathf.Cos(f)), start: vector, color: m_color, duration: 0f, depthTest: false);
				}
			}
		}

		private List<DebugElement> m_elements = new List<DebugElement>();

		private void LateUpdate()
		{
			for (int num = m_elements.Count - 1; num >= 0; num--)
			{
				DebugElement debugElement = m_elements[num];
				debugElement.Draw();
				debugElement.m_time -= Time.deltaTime;
				if (m_elements[num].m_time <= 0f)
				{
					m_elements.RemoveAt(num);
				}
			}
		}

		public void DrawLine(Vector2 start, Vector2 end, Color color, float time = 0f)
		{
			m_elements.Add(new DebugLine
			{
				m_color = color,
				m_time = time,
				m_start = start,
				m_end = end
			});
		}

		public void DrawPoint(Vector2 point, Color color, float time = 0f)
		{
			m_elements.Add(new DebugPoint
			{
				m_color = color,
				m_time = time,
				m_point = point
			});
		}

		public void DrawCircle(Vector2 pos, float radius, Color color, float time = 0f)
		{
			m_elements.Add(new DebugCircle
			{
				m_color = color,
				m_time = time,
				m_pos = pos,
				m_radius = radius
			});
		}

		public void DrawRect(RectCentered rect, Color color, float time = 0f)
		{
			m_elements.Add(new DebugLine
			{
				m_color = color,
				m_time = time,
				m_start = new Vector2(rect.MinX, rect.MinY),
				m_end = new Vector2(rect.MinX, rect.MaxY)
			});
			m_elements.Add(new DebugLine
			{
				m_color = color,
				m_time = time,
				m_start = new Vector2(rect.MinX, rect.MaxY),
				m_end = new Vector2(rect.MaxX, rect.MaxY)
			});
			m_elements.Add(new DebugLine
			{
				m_color = color,
				m_time = time,
				m_start = new Vector2(rect.MaxX, rect.MaxY),
				m_end = new Vector2(rect.MaxX, rect.MinY)
			});
			m_elements.Add(new DebugLine
			{
				m_color = color,
				m_time = time,
				m_start = new Vector2(rect.MaxX, rect.MinY),
				m_end = new Vector2(rect.MinX, rect.MinY)
			});
		}

		public void DrawPoly(Vector2[] poly, Color color, float time = 0f)
		{
			for (int i = 0; i < poly.Length; i++)
			{
				int num = ((i == 0) ? (poly.Length - 1) : (i - 1));
				m_elements.Add(new DebugLine
				{
					m_color = color,
					m_time = time,
					m_start = poly[num],
					m_end = poly[i]
				});
			}
		}
	}
}
