using System.Collections.Generic;
using PowerTools.Quest;
using UnityEngine;

namespace PowerTools
{
	public class Pathfinder
	{
		public class PathPoly
		{
			public PolygonCollider2D m_collider;

			public Transform m_transform;

			public Vector2[] m_verts;

			public Vector2[] m_vertsInflated;

			public Vector2 m_positionCached = Vector2.zero;

			public bool m_enabled = true;

			public bool m_wasEnabled = true;
		}

		public class PathNode
		{
			public PathPoly m_pathPoly;

			public Vector2 m_position = Vector2.zero;

			public List<PathLink> m_links = new List<PathLink>();

			public bool m_visited;

			public float m_cost = float.MaxValue;

			public PathNode m_next;

			public PathNode m_previous;
		}

		public class PathLink
		{
			public PathNode m_node;

			public float m_cost;
		}

		private List<PathPoly> m_pathPolys = new List<PathPoly>();

		private List<PathNode> m_nodes = new List<PathNode>();

		private PathPoly m_mainPoly;

		private bool m_dirty;

		private static readonly float INFLATE_AMOUNT = 0.01f;

		private LineIntersector m_lineIntersector = new LineIntersector();

		public bool GetValid()
		{
			return m_mainPoly != null;
		}

		public void SetMainPolygon(PolygonCollider2D collider)
		{
			RemovePolygon(m_mainPoly);
			m_mainPoly = AddPolygon(collider, isMain: true);
		}

		public void SetMainPolygon(PolygonCollider2D collider, Vector2[] pointsoverride = null)
		{
			RemovePolygon(m_mainPoly);
			m_mainPoly = AddPolygon(collider, isMain: true, pointsoverride);
		}

		public void AddObstacle(Transform transform, Vector2[] points)
		{
			if (!m_pathPolys.Exists((PathPoly item) => item.m_transform == transform))
			{
				AddPolygon(transform, isMain: false, points);
			}
		}

		public void RemoveObstacle(Transform transform)
		{
			if (!(transform == null))
			{
				PathPoly pathPoly = m_pathPolys.Find((PathPoly item) => item.m_transform == transform);
				if (pathPoly != null)
				{
					RemovePolygon(pathPoly);
				}
			}
		}

		public void AddObstacle(PolygonCollider2D collider, Vector2[] pointsoverride = null)
		{
			if (!m_pathPolys.Exists((PathPoly item) => item.m_collider == collider))
			{
				AddPolygon(collider, isMain: false, pointsoverride);
			}
		}

		public void RemoveObstacle(PolygonCollider2D collider)
		{
			if (!(collider == null))
			{
				PathPoly pathPoly = m_pathPolys.Find((PathPoly item) => item.m_collider == collider);
				if (pathPoly != null)
				{
					RemovePolygon(pathPoly);
				}
			}
		}

		public void EnableObstacle(Transform trans)
		{
			if (!(trans == null))
			{
				PathPoly pathPoly = m_pathPolys.Find((PathPoly item) => item.m_transform == trans);
				if (pathPoly != null)
				{
					pathPoly.m_enabled = true;
				}
			}
		}

		public void DisableObstacle(Transform trans)
		{
			if (!(trans == null))
			{
				PathPoly pathPoly = m_pathPolys.Find((PathPoly item) => item.m_transform == trans);
				if (pathPoly != null)
				{
					pathPoly.m_enabled = false;
				}
			}
		}

		public bool IsPointInArea(Vector2 point)
		{
			if (m_mainPoly == null)
			{
				return true;
			}
			UpdateObstacles();
			foreach (PathPoly pathPoly in m_pathPolys)
			{
				bool flag = pathPoly == m_mainPoly;
				if (pathPoly.m_enabled && IsPointInPoly(pathPoly.m_verts, point) != flag)
				{
					return false;
				}
			}
			return true;
		}

		public Vector2[] FindPath(Vector2 pointStart, Vector2 pointEnd)
		{
			if (!GetValid())
			{
				return new Vector2[2] { pointStart, pointEnd };
			}
			List<Vector2> list = new List<Vector2>();
			UpdateObstacles();
			if (m_dirty)
			{
				m_dirty = false;
				CalculateLinks(0);
			}
			if (!IsPointInArea(pointStart))
			{
				pointStart = GetClosestPointToArea(pointStart);
			}
			PathNode pathNode = new PathNode
			{
				m_position = pointStart
			};
			PathNode pathNode2 = new PathNode
			{
				m_position = pointEnd
			};
			m_nodes.Add(pathNode);
			m_nodes.Add(pathNode2);
			CalculateLinks(m_nodes.Count - 2);
			if (EvaluateDijkstra(pathNode, pathNode2))
			{
				PathNode pathNode3 = pathNode2;
				list.Insert(0, pathNode3.m_position);
				while (pathNode3 != pathNode)
				{
					pathNode3 = pathNode3.m_previous;
					if ((list[0] - pathNode3.m_position).sqrMagnitude > 1f)
					{
						list.Insert(0, pathNode3.m_position);
					}
				}
			}
			RemoveNode(pathNode);
			RemoveNode(pathNode2);
			return list.ToArray();
		}

		public Vector2 FindNextPoint(Vector2 pointStart, Vector2 pointEnd)
		{
			if (m_dirty)
			{
				m_dirty = false;
				CalculateLinks(0);
			}
			if (!IsPointInArea(pointStart))
			{
				pointStart = GetClosestPointToArea(pointStart);
			}
			PathNode pathNode = new PathNode
			{
				m_position = pointStart
			};
			PathNode pathNode2 = new PathNode
			{
				m_position = pointEnd
			};
			m_nodes.Add(pathNode);
			m_nodes.Add(pathNode2);
			CalculateLinks(m_nodes.Count - 2);
			Vector2 result = pointEnd;
			if (EvaluateDijkstra(pathNode, pathNode2))
			{
				PathNode pathNode3 = pathNode2;
				while (pathNode3.m_previous != pathNode && (pointStart - pathNode3.m_previous.m_position).sqrMagnitude > 1f)
				{
					pathNode3 = pathNode3.m_previous;
				}
				result = pathNode3.m_position;
			}
			RemoveNode(pathNode);
			RemoveNode(pathNode2);
			return result;
		}

		private void UpdateObstacles()
		{
			foreach (PathPoly pathPoly in m_pathPolys)
			{
				if (pathPoly.m_enabled != pathPoly.m_wasEnabled)
				{
					pathPoly.m_wasEnabled = pathPoly.m_enabled;
					if (pathPoly.m_enabled)
					{
						AddPathPolyNodes(pathPoly);
					}
					else
					{
						RemovePathPolyNodes(pathPoly);
					}
					m_dirty = true;
				}
			}
			m_pathPolys.ForEach(delegate(PathPoly pathPoly)
			{
				UpdatePolyNodePosition(pathPoly);
			});
		}

		public void DrawDebugLines()
		{
			foreach (PathPoly pathPoly in m_pathPolys)
			{
				DrawDebugPoly(pathPoly.m_vertsInflated, (pathPoly == m_mainPoly) ? Color.green : (pathPoly.m_enabled ? Color.red : Color.grey));
			}
			foreach (PathNode node in m_nodes)
			{
				foreach (PathLink link in node.m_links)
				{
					if (link.m_node != null)
					{
						Debug.DrawLine(node.m_position, link.m_node.m_position, Color.yellow);
					}
				}
			}
		}

		private void DrawDebugPoly(Vector2[] points, Color color)
		{
			for (int i = 0; i < points.Length; i++)
			{
				Debug.DrawLine(points[i], points[(i + 1) % points.Length], color);
			}
		}

		private void RemovePolygon(PathPoly polygon)
		{
			int num = m_pathPolys.FindIndex((PathPoly item) => item == polygon);
			if (num >= 0)
			{
				PathPoly pathPoly = m_pathPolys[num];
				m_pathPolys.RemoveAt(num);
				RemovePathPolyNodes(pathPoly);
				m_dirty = true;
			}
		}

		private void RemovePathPolyNodes(PathPoly pathPoly)
		{
			for (int num = m_nodes.Count - 1; num >= 0; num--)
			{
				if (m_nodes[num].m_pathPoly == pathPoly)
				{
					RemoveNode(m_nodes[num]);
				}
			}
		}

		private PathPoly AddPolygon(PolygonCollider2D collider, bool isMain, Vector2[] pointsOverride = null)
		{
			if (collider == null)
			{
				return null;
			}
			return AddPolygon(collider, collider.transform, isMain, pointsOverride);
		}

		private PathPoly AddPolygon(Transform transform, bool isMain, Vector2[] pointsOverride = null)
		{
			if (transform == null)
			{
				return null;
			}
			return AddPolygon(null, transform, isMain, pointsOverride);
		}

		private PathPoly AddPolygon(PolygonCollider2D collider, Transform transform, bool isMain, Vector2[] pointsOverride = null)
		{
			m_dirty = true;
			PathPoly pathPoly = new PathPoly();
			pathPoly.m_collider = collider;
			pathPoly.m_transform = transform;
			pathPoly.m_positionCached = transform.position;
			Vector2 positionCached = pathPoly.m_positionCached;
			Vector2[] array = null;
			array = ((pointsOverride == null) ? (collider.GetPath(0).Clone() as Vector2[]) : pointsOverride);
			for (int i = 0; i < array.Length; i++)
			{
				array[i] += positionCached;
			}
			pathPoly.m_verts = array;
			if (isMain != CheckWindingClockwise(array))
			{
				ReversePoly(array);
			}
			Vector2[] array2 = new Vector2[array.Length];
			array2 = InflatePoly(array, INFLATE_AMOUNT);
			pathPoly.m_vertsInflated = array2;
			AddPathPolyNodes(pathPoly);
			m_pathPolys.Add(pathPoly);
			return pathPoly;
		}

		private bool CheckWindingClockwise(Vector2[] verts)
		{
			float num = 0f;
			for (int i = 0; i < verts.Length; i++)
			{
				int num2 = ((i != verts.Length - 1) ? (i + 1) : 0);
				num += verts[i].x * verts[num2].y - verts[num2].x * verts[i].y;
			}
			return num > 0f;
		}

		private void UpdatePolyNodePosition(PathPoly pathPoly)
		{
			if (pathPoly.m_enabled && !((pathPoly.m_positionCached - (Vector2)pathPoly.m_transform.position).sqrMagnitude <= float.Epsilon))
			{
				Vector2 vector = (Vector2)pathPoly.m_transform.position - pathPoly.m_positionCached;
				pathPoly.m_positionCached = pathPoly.m_transform.position;
				for (int i = 0; i < pathPoly.m_verts.Length; i++)
				{
					pathPoly.m_verts[i] = pathPoly.m_verts[i] + vector;
				}
				for (int j = 0; j < pathPoly.m_vertsInflated.Length; j++)
				{
					pathPoly.m_vertsInflated[j] = pathPoly.m_vertsInflated[j] + vector;
				}
				RemovePathPolyNodes(pathPoly);
				AddPathPolyNodes(pathPoly);
				m_dirty = true;
			}
		}

		private void AddPathPolyNodes(PathPoly pathPoly)
		{
			Vector2[] vertsInflated = pathPoly.m_vertsInflated;
			int num = vertsInflated.Length;
			for (int i = 0; i < num; i++)
			{
				if (!IsPointConcave(vertsInflated, i))
				{
					m_nodes.Add(new PathNode
					{
						m_position = vertsInflated[i],
						m_pathPoly = pathPoly
					});
				}
			}
		}

		private void CalculateLinks(int fromNode)
		{
			if (fromNode <= 0)
			{
				for (int i = 0; i < m_nodes.Count; i++)
				{
					m_nodes[i].m_links.Clear();
				}
			}
			for (int j = 0; j < m_nodes.Count; j++)
			{
				PathNode pathNode = m_nodes[j];
				for (int k = Mathf.Max(j + 1, fromNode); k < m_nodes.Count; k++)
				{
					PathNode pathNode2 = m_nodes[k];
					if (HasLineOfSight(pathNode.m_position, pathNode2.m_position))
					{
						float magnitude = (pathNode.m_position - pathNode2.m_position).magnitude;
						pathNode.m_links.Add(new PathLink
						{
							m_node = pathNode2,
							m_cost = magnitude
						});
						pathNode2.m_links.Add(new PathLink
						{
							m_node = pathNode,
							m_cost = magnitude
						});
					}
				}
			}
		}

		private void RemoveNode(PathNode node)
		{
			for (int i = 0; i < node.m_links.Count; i++)
			{
				node.m_links[i].m_node.m_links.RemoveAll((PathLink link) => link.m_node == node);
			}
			m_nodes.Remove(node);
		}

		private bool EvaluateDijkstra(PathNode startNode, PathNode endNode)
		{
			for (int i = 0; i < m_nodes.Count; i++)
			{
				PathNode pathNode = m_nodes[i];
				pathNode.m_visited = false;
				pathNode.m_cost = float.MaxValue;
				pathNode.m_next = null;
				pathNode.m_previous = null;
			}
			startNode.m_cost = 0f;
			List<PathNode> list = new List<PathNode>();
			list.Add(startNode);
			float num = 0f;
			while (list.Count > 0)
			{
				PathNode pathNode2 = null;
				for (int j = 0; j < list.Count; j++)
				{
					PathNode pathNode3 = list[j];
					if (!pathNode3.m_visited && (pathNode2 == null || pathNode3.m_cost < pathNode2.m_cost))
					{
						pathNode2 = pathNode3;
					}
				}
				if (pathNode2 == endNode)
				{
					return true;
				}
				if (pathNode2 == null)
				{
					return false;
				}
				list.Remove(pathNode2);
				pathNode2.m_visited = true;
				for (int k = 0; k < pathNode2.m_links.Count; k++)
				{
					PathLink pathLink = pathNode2.m_links[k];
					num = pathNode2.m_cost + pathLink.m_cost;
					if (num < pathLink.m_node.m_cost)
					{
						pathLink.m_node.m_cost = num;
						pathLink.m_node.m_previous = pathNode2;
						if (!pathLink.m_node.m_visited)
						{
							list.Add(pathLink.m_node);
						}
					}
				}
			}
			return false;
		}

		private bool HasLineOfSight(Vector2 pointA, Vector2 pointB)
		{
			if ((pointA - pointB).sqrMagnitude < float.Epsilon)
			{
				return true;
			}
			m_lineIntersector.SetFirstLine(pointA, pointB);
			for (int i = 0; i < m_pathPolys.Count; i++)
			{
				if (!m_pathPolys[i].m_enabled)
				{
					continue;
				}
				Vector2[] verts = m_pathPolys[i].m_verts;
				int num = verts.Length;
				Vector2 secondLineStart = verts[num - 1];
				for (int j = 0; j < num; j++)
				{
					if (m_lineIntersector.Calculate(secondLineStart, verts[j]))
					{
						return false;
					}
					secondLineStart = verts[j];
				}
			}
			return true;
		}

		public static bool IsPointInPoly(Vector2[] polyPoints, Vector2 point)
		{
			float num = 0f;
			for (int i = 0; i < polyPoints.Length; i++)
			{
				num = Mathf.Min(num, polyPoints[i].x);
			}
			Vector2 start = new Vector2(num - 0.1f, point.y);
			int num2 = 0;
			for (int j = 0; j < polyPoints.Length; j++)
			{
				Vector2 start2 = polyPoints[j];
				Vector2 end = polyPoints[(j + 1) % polyPoints.Length];
				if (LineIntersector.HasIntersection(start, point, start2, end))
				{
					num2++;
				}
			}
			return (num2 & 1) == 1;
		}

		public Vector2 GetClosestPointToArea(Vector2 point)
		{
			UpdateObstacles();
			List<Vector2> list = new List<Vector2>();
			Vector2 item = Vector2.zero;
			float num = float.PositiveInfinity;
			for (int i = 0; i < m_pathPolys.Count; i++)
			{
				if (!m_pathPolys[i].m_enabled)
				{
					continue;
				}
				Vector2[] verts = m_pathPolys[i].m_verts;
				Vector2[] vertsInflated = m_pathPolys[i].m_vertsInflated;
				for (int j = 0; j < vertsInflated.Length; j++)
				{
					Vector2 vector = vertsInflated[j];
					Vector2 vector2 = vertsInflated[(j + 1) % vertsInflated.Length];
					Vector2 start = verts[j];
					Vector2 end = verts[(j + 1) % verts.Length];
					Vector2 vector3 = (Vector2)Vector3.Project(point - vector, vector2 - vector) + vector;
					if (LineIntersector.HasIntersection(point, vector3, start, end) && IsPointInArea(vector3))
					{
						list.Add(vector3);
					}
					float sqrMagnitude = (point - vertsInflated[j]).sqrMagnitude;
					if (sqrMagnitude < num && IsPointInArea(vertsInflated[j]))
					{
						num = sqrMagnitude;
						item = vertsInflated[j];
					}
				}
			}
			list.Add(item);
			float num2 = float.PositiveInfinity;
			int index = 0;
			for (int k = 0; k < list.Count; k++)
			{
				float sqrMagnitude2 = (point - list[k]).sqrMagnitude;
				if (sqrMagnitude2 < num2)
				{
					num2 = sqrMagnitude2;
					index = k;
				}
			}
			return list[index];
		}

		public static Vector2[] InflatePoly(Vector2[] poly, float amount)
		{
			Vector2[] array = new Vector2[poly.Length];
			for (int i = 0; i < poly.Length; i++)
			{
				Vector2 vector = poly[(i == 0) ? (poly.Length - 1) : (i - 1)];
				Vector2 vector2 = poly[i];
				Vector2 vector3 = poly[(i + 1) % poly.Length];
				Vector2 normalized = (vector - vector2).normalized;
				Vector2 normalized2 = (vector3 - vector2).normalized;
				Vector2 vector4 = normalized + normalized2;
				vector4 *= (IsPointConcave(poly, i) ? amount : (0f - amount));
				array[i] = poly[i] + vector4;
			}
			return array;
		}

		public static bool IsPointConcave(Vector2[] points, int point)
		{
			Vector2 vector = points[point];
			Vector2 vector2 = points[(point + 1) % points.Length];
			Vector2 vector3 = points[(point == 0) ? (points.Length - 1) : (point - 1)];
			return Vector2.Dot((vector - vector3).GetTangentR(), vector2 - vector) <= 0f;
		}

		public static void ReversePoly(Vector2[] poly)
		{
			for (int i = 0; (float)i < (float)poly.Length * 0.5f; i++)
			{
				Utils.Swap(ref poly[i], ref poly[poly.Length - 1 - i]);
			}
		}
	}
}
