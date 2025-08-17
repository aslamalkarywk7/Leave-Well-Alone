using PowerTools;
using UnityEngine;

public class Effect : MonoBehaviour
{
	[SerializeField]
	private int m_baseline;

	[SerializeField]
	private bool m_destroyOnAnimEnd = true;

	private SpriteAnim m_spriteAnimator;

	private SpriteRenderer m_sprite;

	private void Start()
	{
		m_sprite = GetComponent<SpriteRenderer>();
		m_spriteAnimator = GetComponent<SpriteAnim>();
	}

	private void Update()
	{
		if (m_destroyOnAnimEnd && m_spriteAnimator != null && !m_spriteAnimator.IsPlaying())
		{
			Object.Destroy(base.gameObject);
		}
		if (m_sprite != null)
		{
			m_sprite.sortingOrder = -Mathf.RoundToInt((base.transform.position.y + (float)m_baseline) * 10f);
		}
	}
}
