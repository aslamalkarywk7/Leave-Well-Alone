using System;
using UnityEngine;

namespace PowerTools.Quest
{
	public class RendererHider
	{
		private Renderer[] m_hiddenRenderers;

		public void Hide(GameObject root)
		{
			if (m_hiddenRenderers == null)
			{
				m_hiddenRenderers = root.GetComponentsInChildren<Renderer>(includeInactive: false);
				Array.ForEach(m_hiddenRenderers, delegate(Renderer item)
				{
					item.enabled = false;
				});
			}
		}

		public void Show()
		{
			if (m_hiddenRenderers == null)
			{
				return;
			}
			Array.ForEach(m_hiddenRenderers, delegate(Renderer item)
			{
				if (item != null)
				{
					item.enabled = true;
				}
			});
			m_hiddenRenderers = null;
		}
	}
}
