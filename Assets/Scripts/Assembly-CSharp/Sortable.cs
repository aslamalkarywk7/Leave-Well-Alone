using UnityEngine;

public class Sortable : MonoBehaviour
{
	[SerializeField]
	private float m_baseline;

	[SerializeField]
	[Tooltip("If true, the baseline will be in world position, instead of local to the object. So y position of the sortable is ignored")]
	private bool m_fixed;

	[SerializeField]
	[Tooltip("If true, renderers are cached on Start for efficiency (rather than retrieved on update)")]
	private bool m_cacheRenderers = true;

	private Renderer[] m_renderers;

	private int m_sortOrderCached = int.MinValue;

	public float Baseline
	{
		get
		{
			return m_baseline;
		}
		set
		{
			m_baseline = value;
		}
	}

	public int SortOrder => -Mathf.RoundToInt(((m_fixed ? 0f : base.transform.position.y) + Baseline) * 10f);

	public bool Fixed
	{
		get
		{
			return m_fixed;
		}
		set
		{
			m_fixed = value;
		}
	}

	public void EditorRefresh()
	{
		Start();
		LateUpdate();
	}

	private void Start()
	{
		if (m_cacheRenderers)
		{
			m_renderers = GetComponentsInChildren<Renderer>();
			Renderer[] renderers = m_renderers;
			for (int i = 0; i < renderers.Length; i++)
			{
				renderers[i].sortingLayerName = "Default";
			}
		}
	}

	private void LateUpdate()
	{
		int sortOrder = SortOrder;
		if (m_cacheRenderers && m_sortOrderCached == sortOrder)
		{
			return;
		}
		if (!m_cacheRenderers)
		{
			m_renderers = GetComponentsInChildren<Renderer>();
		}
		Renderer[] renderers = m_renderers;
		foreach (Renderer renderer in renderers)
		{
			if (!m_cacheRenderers)
			{
				renderer.sortingLayerName = "Default";
			}
			renderer.sortingOrder = sortOrder;
		}
		m_sortOrderCached = sortOrder;
	}
}
