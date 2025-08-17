using UnityEngine;

namespace PowerTools.Quest
{
	public class QuestSinMotion : MonoBehaviour
	{
		[SerializeField]
		public Vector2 m_positionMag;

		[SerializeField]
		public Vector2 m_positionDelta;

		[SerializeField]
		public float m_rotationMag;

		[SerializeField]
		public float m_rotationDelta;

		[SerializeField]
		public Vector2 m_scaleMag;

		[SerializeField]
		public Vector2 m_scaleDelta;

		[SerializeField]
		public float m_timeStep;

		[SerializeField]
		public float m_snap;

		private Vector2 m_originalPosition;

		private Vector2 m_cachedPosition;

		private float m_originalAngle;

		private float m_cachedAngle;

		private Vector2 m_originalScale;

		private Vector2 m_cachedScale;

		private float m_timer;

		private float m_timer2;

		private PropComponent m_propComponent;

		private QuestCameraComponent m_cameraComponent;

		private Vector2 GetPosition()
		{
			if (m_propComponent != null)
			{
				return m_propComponent.GetData().Position;
			}
			if (m_cameraComponent != null)
			{
				m_cameraComponent.GetData().GetPositionOverride();
			}
			return base.transform.position;
		}

		private void SetPosition(Vector2 position)
		{
			if (m_propComponent != null)
			{
				m_propComponent.GetData().Position = position;
			}
			else if (m_cameraComponent != null)
			{
				m_cameraComponent.GetData().SetPositionOverride(position);
			}
			else
			{
				base.transform.position = position.WithZ(base.transform.position.z);
			}
		}

		private void Start()
		{
			m_timer = Random.Range(0f, 60f);
			m_timer2 = Random.Range(0f, m_timeStep);
			m_propComponent = GetComponent<PropComponent>();
			m_cameraComponent = GetComponent<QuestCameraComponent>();
			m_originalPosition = GetPosition();
			m_originalAngle = base.transform.eulerAngles.z;
			m_originalScale = Vector2.zero;
			m_cachedPosition = m_originalPosition;
			m_cachedAngle = m_originalAngle;
			m_cachedScale = base.transform.localScale;
		}

		private void Update()
		{
			m_timer += Time.deltaTime;
			m_timer2 -= Time.deltaTime;
			if (m_timer2 > 0f)
			{
				return;
			}
			m_timer2 = m_timeStep;
			m_originalPosition += GetPosition() - m_cachedPosition;
			if (m_positionMag.x > 1E-05f || m_positionMag.y > 1E-05f)
			{
				SetPosition(Utils.Snap(m_originalPosition + new Vector2(m_positionMag.x * Mathf.Sin(m_positionDelta.x * m_timer), m_positionMag.y * Mathf.Sin(m_positionDelta.y * m_timer)), m_snap));
			}
			m_originalAngle += base.transform.eulerAngles.z - m_cachedAngle;
			if (m_rotationMag > 1E-05f)
			{
				if (m_rotationMag > 359f)
				{
					m_originalAngle += Time.deltaTime * m_rotationDelta;
					base.transform.eulerAngles = new Vector3(0f, 0f, m_originalAngle * ((base.transform.localScale.x > 0f) ? 1f : (-1f)));
				}
				else
				{
					base.transform.eulerAngles = new Vector3(0f, 0f, m_originalAngle + m_rotationMag * Mathf.Sin(m_rotationDelta * m_timer));
				}
			}
			m_originalScale += (Vector2)base.transform.localScale - m_cachedScale;
			if (m_scaleMag.x > 1E-05f || m_scaleMag.y > 1E-05f)
			{
				base.transform.localScale = (m_originalScale + new Vector2(1f - m_scaleMag.x + m_scaleMag.x * Mathf.Sin(m_scaleDelta.x * m_timer), 1f - m_scaleMag.y + m_scaleMag.y * Mathf.Sin(m_scaleDelta.y * m_timer))).WithZ(base.transform.localScale.z);
			}
			m_cachedPosition = GetPosition();
			m_cachedAngle = base.transform.eulerAngles.z;
			m_cachedScale = base.transform.localScale;
		}
	}
}
