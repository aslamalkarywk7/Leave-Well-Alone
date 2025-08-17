using System;
using UnityEngine;

namespace PowerTools.Quest
{
	public class RegionComponent : MonoBehaviour
	{
		public enum eTriggerResult
		{
			None = 0,
			Enter = 1,
			Exit = 2,
			Stay = 3
		}

		public class RegionPolyUtil
		{
			public static float CalcDistToEdge(Vector2[] poly, Vector2 point)
			{
				float num = float.MaxValue;
				Vector2 a = poly[poly.Length - 1];
				foreach (Vector2 vector in poly)
				{
					float sqrMagnitude = (NearestPointOnLine(a, vector, point) - point).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
					}
					a = vector;
				}
				return Mathf.Sqrt(num);
			}

			public static Vector2 CalcClosestPointOnEdge(Vector2[] poly, Vector2 point)
			{
				Vector2[] array = ClosestSegment(poly, point);
				return NearestPointOnLine(array[0], array[1], point);
			}

			private static Vector2 NearestPointOnLine(Vector2 a, Vector2 b, Vector2 p)
			{
				float num = p.x - a.x;
				float num2 = p.y - a.y;
				float num3 = b.x - a.x;
				float num4 = b.y - a.y;
				float num5 = num3 * num3 + num4 * num4;
				float num6 = (num * num3 + num2 * num4) / num5;
				if (num6 < 0f)
				{
					num6 = 0f;
				}
				else if (num6 > 1f)
				{
					num6 = 1f;
				}
				return new Vector2(a.x + num3 * num6, a.y + num4 * num6);
			}

			private static Vector2[] ClosestSegment(Vector2[] points, Vector2 point)
			{
				Vector2[] array = new Vector2[2];
				int num = ClosestPointIndex(points, point);
				array[0] = points[num];
				Vector2[] array2 = new Vector2[2]
				{
					points[(num + 1 + points.Length) % points.Length],
					points[(num - 1 + points.Length) % points.Length]
				};
				float[] array3 = new float[2]
				{
					GetAngle(new Vector2[3]
					{
						point,
						array[0],
						array2[0]
					}),
					GetAngle(new Vector2[3]
					{
						point,
						array[0],
						array2[1]
					})
				};
				if (array3[0] < array3[1])
				{
					array[1] = array2[0];
				}
				else
				{
					array[1] = array2[1];
				}
				return array;
			}

			private static float GetAngle(Vector2[] abc)
			{
				return Mathf.Atan2(abc[2].x - abc[0].y, abc[2].x - abc[0].x) - Mathf.Atan2(abc[1].y - abc[0].y, abc[1].x - abc[0].x);
			}

			private static int ClosestPointIndex(Vector2[] points, Vector2 point)
			{
				int result = 0;
				float num = float.MaxValue;
				for (int i = 0; i < points.Length; i++)
				{
					float sqrMagnitude = (points[i] - point).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						result = i;
						num = sqrMagnitude;
					}
				}
				return result;
			}

			private static float GetThing(Vector2[] points, Vector2 point)
			{
				return 0f;
			}
		}

		[SerializeField]
		private Region m_data = new Region();

		private PolygonCollider2D m_polygonCollider;

		private float m_minColliderY;

		private float m_maxColliderY;

		public Region GetData()
		{
			return m_data;
		}

		public void SetData(Region data)
		{
			m_data = data;
		}

		public void OnSetWalkable(bool walkable)
		{
			if (m_polygonCollider == null)
			{
				m_polygonCollider = GetComponent<PolygonCollider2D>();
			}
			if (walkable)
			{
				Singleton<PowerQuest>.Get.Pathfinder.RemoveObstacle(m_polygonCollider);
			}
			else
			{
				Singleton<PowerQuest>.Get.Pathfinder.AddObstacle(m_polygonCollider);
			}
		}

		public PolygonCollider2D GetPolygonCollider()
		{
			return m_polygonCollider;
		}

		public bool UpdateCharactersOnRegion(int index, bool characterActiveInRoom, Vector2 characterPos)
		{
			bool flag = characterActiveInRoom && m_data.Enabled;
			if (flag)
			{
				flag = m_polygonCollider != null && m_polygonCollider.OverlapPoint(characterPos);
			}
			m_data.GetCharacterOnRegionMask().Set(index, flag);
			return flag;
		}

		public float GetScaleAt(Vector2 position)
		{
			if (GetData().ScaleTop == 1f && GetData().ScaleBottom == 1f)
			{
				return 1f;
			}
			return Mathf.Lerp(GetData().ScaleBottom, GetData().ScaleTop, Mathf.InverseLerp(m_minColliderY, m_maxColliderY, position.y));
		}

		public float GetDistanceIntoRegion(Vector2 point)
		{
			if (m_polygonCollider == null)
			{
				return 0f;
			}
			point -= (Vector2)base.transform.position;
			return RegionPolyUtil.CalcDistToEdge(m_polygonCollider.points, point);
		}

		public float GetFadeRatio(Vector2 point)
		{
			if (GetData().FadeDistance <= 0f)
			{
				return 1f;
			}
			if (m_polygonCollider == null)
			{
				return 1f;
			}
			return Mathf.Clamp01(GetDistanceIntoRegion(point) / GetData().FadeDistance);
		}

		public eTriggerResult UpdateCharacterOnRegionState(int index, bool background)
		{
			eTriggerResult result = eTriggerResult.None;
			bool flag = m_data.GetCharacterOnRegionMask().Get(index);
			if (m_data.GetCharacterOnRegionMaskOld(background).Get(index))
			{
				result = (flag ? eTriggerResult.Stay : eTriggerResult.Exit);
			}
			else if (flag)
			{
				result = eTriggerResult.Enter;
			}
			m_data.GetCharacterOnRegionMaskOld(background).Set(index, m_data.GetCharacterOnRegionMask().Get(index));
			return result;
		}

		public void OnRoomLoaded()
		{
			m_data.GetCharacterOnRegionMaskOld(background: true).SetAll(value: true);
			m_data.GetCharacterOnRegionMaskOld(background: true).And(m_data.GetCharacterOnRegionMask());
			m_data.GetCharacterOnRegionMaskOld(background: false).SetAll(value: true);
			m_data.GetCharacterOnRegionMaskOld(background: false).And(m_data.GetCharacterOnRegionMask());
		}

		private void Start()
		{
			if (m_polygonCollider == null)
			{
				m_polygonCollider = GetComponent<PolygonCollider2D>();
			}
			m_data.GetCharacterOnRegionMask().Length = Singleton<PowerQuest>.Get.GetCharacters().Count;
			m_data.GetCharacterOnRegionMaskOld(background: true).Length = m_data.GetCharacterOnRegionMask().Length;
			m_data.GetCharacterOnRegionMaskOld(background: false).Length = m_data.GetCharacterOnRegionMask().Length;
			if (!(m_polygonCollider != null))
			{
				return;
			}
			m_minColliderY = float.MaxValue;
			m_maxColliderY = float.MinValue;
			Array.ForEach(m_polygonCollider.points, delegate(Vector2 item)
			{
				if (item.y < m_minColliderY)
				{
					m_minColliderY = item.y;
				}
				if (item.y > m_maxColliderY)
				{
					m_maxColliderY = item.y;
				}
			});
		}

		public void OnLoadComplete()
		{
			OnSetWalkable(GetData().Walkable);
		}
	}
}
