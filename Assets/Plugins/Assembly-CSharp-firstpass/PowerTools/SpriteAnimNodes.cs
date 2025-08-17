using UnityEngine;

namespace PowerTools
{
	public class SpriteAnimNodes : MonoBehaviour
	{
		public static readonly int NUM_NODES = 10;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node0 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node1 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node2 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node3 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node4 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node5 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node6 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node7 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node8 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node9 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private float m_ang0;

		[SerializeField]
		[HideInInspector]
		private float m_ang1;

		[SerializeField]
		[HideInInspector]
		private float m_ang2;

		[SerializeField]
		[HideInInspector]
		private float m_ang3;

		[SerializeField]
		[HideInInspector]
		private float m_ang4;

		[SerializeField]
		[HideInInspector]
		private float m_ang5;

		[SerializeField]
		[HideInInspector]
		private float m_ang6;

		[SerializeField]
		[HideInInspector]
		private float m_ang7;

		[SerializeField]
		[HideInInspector]
		private float m_ang8;

		[SerializeField]
		[HideInInspector]
		private float m_ang9;

		private SpriteRenderer m_spriteRenderer;

		private void Start()
		{
			SpriteAnim component = GetComponent<SpriteAnim>();
			if (component != null)
			{
				component.RegisterSpriteAnimNodesComponent(this);
			}
		}

		public Vector3 GetPosition(int nodeId, bool ignoredPivot = false)
		{
			if (m_spriteRenderer == null)
			{
				m_spriteRenderer = GetComponent<SpriteRenderer>();
			}
			if (m_spriteRenderer == null || m_spriteRenderer.sprite == null)
			{
				return Vector2.zero;
			}
			Vector3 vector = GetPositionRaw(nodeId);
			if (Mathf.Abs(vector.x) <= 0.00011f)
			{
				vector.x = 0f;
			}
			if (Mathf.Abs(vector.y) <= 0.00011f)
			{
				vector.y = 0f;
			}
			vector.y = 0f - vector.y;
			if (ignoredPivot)
			{
				vector += (Vector3)(m_spriteRenderer.sprite.rect.size * 0.5f - m_spriteRenderer.sprite.pivot);
			}
			vector *= 1f / m_spriteRenderer.sprite.pixelsPerUnit;
			if (m_spriteRenderer.flipX)
			{
				vector.x = 0f - vector.x;
			}
			if (m_spriteRenderer.flipY)
			{
				vector.y = 0f - vector.y;
			}
			vector.Scale(base.transform.lossyScale);
			vector = base.transform.rotation * vector;
			return vector + base.transform.position;
		}

		public float GetAngle(int nodeId)
		{
			if (m_spriteRenderer == null)
			{
				m_spriteRenderer = GetComponent<SpriteRenderer>();
			}
			if (m_spriteRenderer == null || m_spriteRenderer.sprite == null)
			{
				return 0f;
			}
			float num = GetAngleRaw(nodeId);
			if (m_spriteRenderer.flipX != m_spriteRenderer.transform.lossyScale.x < 0f)
			{
				num = 180f - num;
			}
			if (m_spriteRenderer.flipY != m_spriteRenderer.transform.lossyScale.y < 0f)
			{
				num = 180f - (num + 90f) - 90f;
			}
			return num + base.transform.eulerAngles.z;
		}

		public void SetTransformFromNode(Transform attachment, int nodeId)
		{
			if (m_spriteRenderer == null)
			{
				m_spriteRenderer = GetComponent<SpriteRenderer>();
			}
			if (!(m_spriteRenderer == null) && !(m_spriteRenderer.sprite == null) && !(attachment == null))
			{
				attachment.position = GetPosition(nodeId);
				float num = GetAngleRaw(nodeId);
				bool flag = m_spriteRenderer.flipX != m_spriteRenderer.transform.lossyScale.x < 0f;
				bool flag2 = m_spriteRenderer.flipY != m_spriteRenderer.transform.lossyScale.y < 0f;
				if (flag)
				{
					num = 0f - num;
				}
				if (flag2)
				{
					num = 0f - num;
				}
				if (attachment.IsChildOf(base.transform))
				{
					flag = m_spriteRenderer.flipX;
					flag2 = m_spriteRenderer.flipY;
				}
				if (flag != attachment.localScale.x < 0f)
				{
					attachment.localScale = new Vector3(0f - attachment.localScale.x, attachment.localScale.y, attachment.localScale.z);
				}
				if (flag2 != attachment.localScale.y < 0f)
				{
					attachment.localScale = new Vector3(attachment.localScale.x, 0f - attachment.localScale.y, attachment.localScale.z);
				}
				num += base.transform.eulerAngles.z;
				attachment.eulerAngles = new Vector3(0f, 0f, num);
			}
		}

		public Vector2 GetPositionRaw(int nodeId)
		{
			return nodeId switch
			{
				0 => m_node0, 
				1 => m_node1, 
				2 => m_node2, 
				3 => m_node3, 
				4 => m_node4, 
				5 => m_node5, 
				6 => m_node6, 
				7 => m_node7, 
				8 => m_node8, 
				9 => m_node9, 
				_ => Vector2.zero, 
			};
		}

		public float GetAngleRaw(int nodeId)
		{
			return nodeId switch
			{
				0 => m_ang0, 
				1 => m_ang1, 
				2 => m_ang2, 
				3 => m_ang3, 
				4 => m_ang4, 
				5 => m_ang5, 
				6 => m_ang6, 
				7 => m_ang7, 
				8 => m_ang8, 
				9 => m_ang9, 
				_ => 0f, 
			};
		}

		public void Reset()
		{
			m_node0 = Vector2.zero;
			m_node1 = Vector2.zero;
			m_node2 = Vector2.zero;
			m_node3 = Vector2.zero;
			m_node4 = Vector2.zero;
			m_node5 = Vector2.zero;
			m_node6 = Vector2.zero;
			m_node7 = Vector2.zero;
			m_node8 = Vector2.zero;
			m_node9 = Vector2.zero;
			m_ang0 = 0f;
			m_ang1 = 0f;
			m_ang2 = 0f;
			m_ang3 = 0f;
			m_ang4 = 0f;
			m_ang5 = 0f;
			m_ang6 = 0f;
			m_ang7 = 0f;
			m_ang8 = 0f;
			m_ang9 = 0f;
		}
	}
}
