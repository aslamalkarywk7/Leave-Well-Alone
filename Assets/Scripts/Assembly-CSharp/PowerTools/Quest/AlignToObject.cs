using UnityEngine;

namespace PowerTools.Quest
{
	[ExecuteInEditMode]
	[AddComponentMenu("Quest Gui Layout/Align To Object")]
	public class AlignToObject : MonoBehaviour
	{
		public enum eAlignHorizontal
		{
			None = 0,
			Left = 1,
			Center = 2,
			Right = 3,
			Position = 4
		}

		public enum eAlignVertical
		{
			None = 0,
			Top = 1,
			Middle = 2,
			Bottom = 3,
			Position = 4
		}

		[Header("Align to the...")]
		public eAlignVertical m_vertical;

		public eAlignHorizontal m_horizontal;

		[Header("...side of ...")]
		public Transform m_object;

		[Header("...with offset...")]
		public Vector2 m_offset = Vector2.zero;

		public Vector2 m_offsetRatio = Vector2.zero;

		private Transform m_transform;

		private Renderer m_objectRenderer;

		private GuiControl m_objectControl;

		private int m_callsThisFrame;

		private static int s_debugCalls;

		private void Awake()
		{
			m_transform = base.transform;
		}

		private void LateUpdate()
		{
			UpdatePos();
			m_callsThisFrame = 0;
		}

		public void UpdatePos()
		{
			if (m_object == null)
			{
				return;
			}
			if (base.gameObject.activeInHierarchy)
			{
				m_callsThisFrame++;
				if (m_callsThisFrame > s_debugCalls)
				{
					s_debugCalls = m_callsThisFrame;
					if (s_debugCalls > 2)
					{
						Debug.Log($"Detected {m_callsThisFrame} Recursive AlignToObject calls in: {base.gameObject.name}");
					}
				}
			}
			if (m_objectControl == null || m_objectControl.transform != m_object)
			{
				m_objectControl = m_object.GetComponent<GuiControl>();
			}
			if (m_objectControl == null && (m_objectRenderer == null || m_objectRenderer.transform != m_object))
			{
				m_objectRenderer = m_object.GetComponent<Renderer>();
			}
			bool flag = false;
			if (m_objectControl == null && m_object.gameObject.activeInHierarchy && m_objectRenderer != null)
			{
				FitToObject componentInChildren = m_object.GetComponentInChildren<FitToObject>(includeInactive: false);
				if ((bool)componentInChildren && componentInChildren.isActiveAndEnabled)
				{
					componentInChildren.UpdateSize();
				}
			}
			if (m_objectControl == null)
			{
				AlignToObject component = m_object.GetComponent<AlignToObject>();
				if ((bool)component && component.enabled)
				{
					component.UpdatePos();
				}
			}
			RectCentered rectCentered = new RectCentered(m_object.position, Vector2.zero);
			if (m_objectControl != null && m_objectControl.enabled)
			{
				m_objectControl.UpdateFitAndAlign();
				if (m_objectControl.isActiveAndEnabled)
				{
					rectCentered = m_objectControl.GetRect(base.transform);
					flag = true;
				}
			}
			else if (m_objectRenderer != null && m_objectRenderer.enabled && m_object != null)
			{
				rectCentered = new RectCentered(m_object.GetComponent<Renderer>().bounds);
				flag = true;
			}
			Vector3 zero = Vector3.zero;
			switch (m_horizontal)
			{
			case eAlignHorizontal.Left:
				zero.x = rectCentered.MinX;
				break;
			case eAlignHorizontal.Center:
				zero.x = rectCentered.CenterX;
				break;
			case eAlignHorizontal.Right:
				zero.x = rectCentered.MaxX;
				break;
			case eAlignHorizontal.Position:
				zero.x = m_object.position.x;
				break;
			}
			switch (m_vertical)
			{
			case eAlignVertical.Top:
				zero.y = rectCentered.MaxY;
				break;
			case eAlignVertical.Middle:
				zero.y = rectCentered.CenterY;
				break;
			case eAlignVertical.Bottom:
				zero.y = rectCentered.MinY;
				break;
			case eAlignVertical.Position:
				zero.y = m_object.position.y;
				break;
			}
			if (flag)
			{
				zero += (Vector3)m_offset;
				Vector2 offsetRatio = m_offsetRatio;
				offsetRatio.Scale(new Vector2(rectCentered.Width, rectCentered.Height));
				zero += (Vector3)offsetRatio;
			}
			m_transform.position = new Vector3((m_horizontal == eAlignHorizontal.None) ? m_transform.position.x : zero.x, (m_vertical == eAlignVertical.None) ? m_transform.position.y : zero.y, m_transform.position.z);
		}
	}
}
