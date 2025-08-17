using PowerTools;
using UnityEngine;

public class InventoryPanelItem : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer m_itemSpriteComponent;

	private SpriteAnim m_animComponent;

	private string m_cachedImage;

	public Sprite GetSpriteName()
	{
		if (!(m_itemSpriteComponent != null))
		{
			return null;
		}
		return m_itemSpriteComponent.sprite;
	}

	public AnimationClip GetAnimName()
	{
		if (!(m_animComponent != null))
		{
			return null;
		}
		return m_animComponent.Clip;
	}

	public string GetCachedAnimSpriteName()
	{
		return m_cachedImage;
	}

	public void SetInventorySprite(Sprite sprite)
	{
		if (!(sprite == null))
		{
			if (m_itemSpriteComponent == null)
			{
				m_itemSpriteComponent = GetComponentInChildren<SpriteRenderer>();
			}
			if (m_animComponent != null)
			{
				m_animComponent.Stop();
			}
			if (m_itemSpriteComponent != null)
			{
				m_itemSpriteComponent.sprite = sprite;
				m_cachedImage = sprite.name;
			}
		}
	}

	public void SetInventoryAnim(AnimationClip anim)
	{
		if (!(anim == null))
		{
			if (m_animComponent == null)
			{
				m_itemSpriteComponent = GetComponentInChildren<SpriteRenderer>();
				m_animComponent = m_itemSpriteComponent.GetComponent<SpriteAnim>();
			}
			if (m_animComponent != null)
			{
				m_animComponent.Play(anim);
				m_cachedImage = anim.name;
			}
		}
	}
}
