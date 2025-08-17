using System;
using System.Collections.Generic;
using ClipperLib;
using UnityEngine;

namespace PowerTools.Quest
{
	[SelectionBase]
	public class RoomComponent : MonoBehaviour
	{
		[SerializeField]
		private Room m_data = new Room();

		[SerializeField]
		[HideInInspector]
		public string m_debugStartFunction;

		[SerializeField]
		[HideInInspector]
		private List<HotspotComponent> m_hotspotComponents = new List<HotspotComponent>();

		[SerializeField]
		[HideInInspector]
		private List<PropComponent> m_propComponents = new List<PropComponent>();

		[SerializeField]
		[HideInInspector]
		private List<RegionComponent> m_regionComponents = new List<RegionComponent>();

		[SerializeField]
		[HideInInspector]
		private List<WalkableComponent> m_walkableAreas = new List<WalkableComponent>();

		[SerializeField]
		[ReadOnly]
		[NonReorderable]
		private List<AnimationClip> m_animations = new List<AnimationClip>();

		[SerializeField]
		[ReadOnly]
		[NonReorderable]
		private List<Sprite> m_sprites = new List<Sprite>();

		private Pathfinder m_pathfinder = new Pathfinder();

		private static readonly float TO_CLIPPER_MULT = 1000f;

		private static readonly float FROM_CLIPPER_MULT = 0.001f;

		public string DebugStartFunction => m_debugStartFunction;

		public Room GetData()
		{
			return m_data;
		}

		public void SetData(Room data)
		{
			m_data = data;
		}

		public GameObject GetPrefab()
		{
			if (!Application.isPlaying || !(m_data.GetPrefab() != null))
			{
				return base.gameObject;
			}
			return m_data.GetPrefab();
		}

		public List<HotspotComponent> GetHotspotComponents()
		{
			return m_hotspotComponents;
		}

		public List<PropComponent> GetPropComponents()
		{
			return m_propComponents;
		}

		public List<RegionComponent> GetRegionComponents()
		{
			return m_regionComponents;
		}

		public List<WalkableComponent> GetWalkableAreas()
		{
			return m_walkableAreas;
		}

		public AnimationClip GetAnimation(string animName)
		{
			return QuestUtils.FindByName(m_animations, animName);
		}

		public List<AnimationClip> GetAnimations()
		{
			return m_animations;
		}

		public Sprite GetSprite(string animName)
		{
			return PowerQuest.FindSpriteInList(m_sprites, animName);
		}

		public List<Sprite> GetSprites()
		{
			return m_sprites;
		}

		public void OnLoadComplete()
		{
			m_hotspotComponents.ForEach(delegate(HotspotComponent item)
			{
				item.OnLoadComplete();
			});
			m_propComponents.ForEach(delegate(PropComponent item)
			{
				item.OnLoadComplete();
			});
			m_regionComponents.ForEach(delegate(RegionComponent item)
			{
				item.OnLoadComplete();
			});
			if (!Singleton<PowerQuest>.Get.GetPixelCamEnabled())
			{
				return;
			}
			int layerHighRes = LayerMask.NameToLayer("HighRes");
			m_propComponents.ForEach(delegate(PropComponent item)
			{
				if (item.GetParallax() != 0f)
				{
					item.gameObject.layer = layerHighRes;
				}
			});
		}

		public void EditorUpdateChildComponents()
		{
			m_hotspotComponents.Clear();
			m_hotspotComponents.AddRange(GetComponentsInChildren<HotspotComponent>(includeInactive: true));
			m_propComponents.Clear();
			m_propComponents.AddRange(GetComponentsInChildren<PropComponent>(includeInactive: true));
			m_regionComponents.Clear();
			m_regionComponents.AddRange(GetComponentsInChildren<RegionComponent>(includeInactive: true));
			m_walkableAreas.Clear();
			m_walkableAreas.AddRange(GetComponentsInChildren<WalkableComponent>(includeInactive: true));
		}

		public bool SetActiveWalkableArea(int id)
		{
			if (m_walkableAreas.IsIndexValid(id))
			{
				m_pathfinder.SetMainPolygon(m_walkableAreas[id].PolygonCollider);
				if (m_walkableAreas.IsIndexValid(m_data.ActiveWalkableArea))
				{
					PolygonCollider2D[] componentsInChildren = m_walkableAreas[id].transform.GetComponentsInChildren<PolygonCollider2D>();
					foreach (PolygonCollider2D polygonCollider2D in componentsInChildren)
					{
						if (polygonCollider2D.transform != m_walkableAreas[id].transform)
						{
							m_pathfinder.AddObstacle(polygonCollider2D);
						}
					}
				}
				return true;
			}
			return false;
		}

		public Pathfinder GetPathfinder()
		{
			return m_pathfinder;
		}

		private void Awake()
		{
		}

		private void Start()
		{
		}

		private void Update()
		{
		}

		public bool BuildWalkableArea()
		{
			Clipper clipper = new Clipper();
			clipper.ReverseSolution = true;
			if (!BuildClipperWalkableArea(clipper, out var result))
			{
				return false;
			}
			bool flag = false;
			m_pathfinder = new Pathfinder();
			foreach (List<IntPoint> item in result)
			{
				Vector2[] array = item.ConvertAll((IntPoint item) => new Vector2((float)item.X * FROM_CLIPPER_MULT, (float)item.Y * FROM_CLIPPER_MULT)).ToArray();
				if (!Clipper.Orientation(item))
				{
					if (!flag)
					{
						m_pathfinder.SetMainPolygon(m_walkableAreas[0].PolygonCollider, array);
					}
					flag = true;
				}
				else
				{
					Pathfinder.ReversePoly(array);
					m_pathfinder.AddObstacle(m_walkableAreas[0].PolygonCollider, array);
				}
			}
			return true;
		}

		public bool BuildClipperWalkableArea(Clipper clipper, out List<List<IntPoint>> result)
		{
			bool flag = true;
			for (int i = 0; i < m_walkableAreas.Count; i++)
			{
				if (m_data.ActiveWalkableArea == i)
				{
					List<IntPoint> pg = new List<IntPoint>(Array.ConvertAll(m_walkableAreas[i].PolygonCollider.points, (Vector2 item) => new IntPoint(item.x * TO_CLIPPER_MULT, item.y * TO_CLIPPER_MULT)));
					clipper.AddPath(pg, (!flag) ? PolyType.ptClip : PolyType.ptSubject, Closed: true);
					flag = false;
				}
			}
			result = new List<List<IntPoint>>();
			if (!clipper.Execute(ClipType.ctUnion, result) || result.Count < 0)
			{
				return false;
			}
			int num = 0;
			clipper.Clear();
			clipper.AddPath(result[0], PolyType.ptSubject, Closed: true);
			for (int num2 = 0; num2 < m_walkableAreas.Count; num2++)
			{
				if (m_data.ActiveWalkableArea != num2)
				{
					continue;
				}
				WalkableComponent walkableComponent = m_walkableAreas[num2];
				PolygonCollider2D[] componentsInChildren = walkableComponent.transform.GetComponentsInChildren<PolygonCollider2D>();
				foreach (PolygonCollider2D polygonCollider2D in componentsInChildren)
				{
					if (!(polygonCollider2D.transform == walkableComponent.PolygonCollider.transform))
					{
						List<IntPoint> pg2 = new List<IntPoint>(Array.ConvertAll(polygonCollider2D.points, (Vector2 item) => new IntPoint(item.x * TO_CLIPPER_MULT, item.y * TO_CLIPPER_MULT)));
						clipper.AddPath(pg2, PolyType.ptClip, Closed: true);
						num++;
					}
				}
			}
			foreach (RegionComponent regionComponent in m_regionComponents)
			{
				if (!regionComponent.GetData().Walkable)
				{
					List<IntPoint> pg3 = new List<IntPoint>(Array.ConvertAll(regionComponent.GetPolygonCollider().points, (Vector2 item) => new IntPoint(item.x * TO_CLIPPER_MULT, item.y * TO_CLIPPER_MULT)));
					clipper.AddPath(pg3, PolyType.ptClip, Closed: true);
					num++;
				}
			}
			if (num > 0)
			{
				result.Clear();
				clipper.Execute(ClipType.ctDifference, result);
			}
			return true;
		}

		public Vector2 GetClosestPoint(Vector2 from, Vector2 to)
		{
			Clipper clipper = new Clipper();
			clipper.ReverseSolution = true;
			if (!BuildClipperWalkableArea(clipper, out var result))
			{
				Debug.Log("Clipper FAIL: couldn't build walkable");
				return to;
			}
			IntPoint fromInt = new IntPoint(from.x * TO_CLIPPER_MULT, from.y * TO_CLIPPER_MULT);
			IntPoint pt = new IntPoint(to.x * TO_CLIPPER_MULT, to.y * TO_CLIPPER_MULT);
			List<IntPoint> list = result.Find((List<IntPoint> path) => !Clipper.Orientation(path) && Clipper.PointInPolygon(fromInt, path) != 0);
			if (list == null || list.Count == 0)
			{
				Debug.Log("Clipper FAIL: Plr not on walkable");
				return to;
			}
			ClipperOffset clipperOffset = new ClipperOffset();
			clipperOffset.AddPath(list, JoinType.jtMiter, EndType.etClosedPolygon);
			clipperOffset.Execute(ref result, -10.0);
			list = result[0];
			if (result.Count > 1)
			{
				Debug.LogWarning("Multiple polygons created when offsetting walkable area. Move your area points further apart");
			}
			if (Clipper.PointInPolygon(pt, list) != 0)
			{
				return to;
			}
			Vector2[] poly = list.ConvertAll((IntPoint item) => new Vector2((float)item.X * FROM_CLIPPER_MULT, (float)item.Y * FROM_CLIPPER_MULT)).ToArray();
			return GetClosestPointToPoly(to, poly);
		}

		private Vector2 GetClosestPointToPoly(Vector2 x, Vector2[] poly)
		{
			float num = float.MaxValue;
			Vector2 result = Vector2.zero;
			for (int i = 0; i < poly.Length; i++)
			{
				int num2 = ((i == 0) ? (poly.Length - 1) : (i - 1));
				Vector2 vector = poly[num2];
				Vector2 vector2 = poly[i];
				Vector2 vector3 = vector2 - vector;
				Vector2 rhs = x - vector;
				if (!(vector == vector2))
				{
					float num3 = Vector2.Dot(vector3, rhs);
					num3 /= vector3.sqrMagnitude;
					float num4 = 0f;
					_ = Vector2.zero;
					num4 = ((num3 < 0f) ? rhs.sqrMagnitude : ((!(num3 > 1f)) ? (rhs.sqrMagnitude - Mathf.Pow(num3 * vector3.magnitude, 2f)) : (vector2 - x).sqrMagnitude));
					if (num4 < num)
					{
						num = num4;
						result = ((!(num3 < 0f)) ? ((!(num3 > 1f)) ? (vector + vector3 * num3) : vector2) : vector);
					}
				}
			}
			return result;
		}
	}
}
