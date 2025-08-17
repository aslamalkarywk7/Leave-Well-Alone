using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class WalkableComponent : MonoBehaviour
{
	private PolygonCollider2D m_polygonCollider;

	public PolygonCollider2D PolygonCollider
	{
		get
		{
			if (m_polygonCollider == null)
			{
				m_polygonCollider = GetComponent<PolygonCollider2D>();
			}
			return m_polygonCollider;
		}
	}

	public Vector2[] Points => PolygonCollider.points;
}
