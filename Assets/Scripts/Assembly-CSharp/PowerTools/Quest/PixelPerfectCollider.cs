using UnityEngine;

namespace PowerTools.Quest
{
	public class PixelPerfectCollider : MonoBehaviour
	{
		private SpriteRenderer m_sprite;

		private void Awake()
		{
			m_sprite = GetComponent<SpriteRenderer>();
		}

		private void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				if (PointOverlapping(Singleton<PowerQuest>.Get.GetMousePosition()))
				{
					Debug.Log("Clicked " + base.gameObject.name);
				}
				else
				{
					Debug.Log("Missed " + base.gameObject.name);
				}
			}
		}

		private bool PointOverlapping(Vector2 point)
		{
			Sprite sprite = m_sprite.sprite;
			Rect rect = sprite.rect;
			if (rect.Contains(point))
			{
				float num = Mathf.InverseLerp(rect.xMin, rect.xMax, point.x);
				float num2 = Mathf.InverseLerp(rect.yMin, rect.yMax, point.y);
				float num3 = Mathf.Lerp(m_sprite.sprite.uv[0].x, m_sprite.sprite.uv[2].x, num);
				float num4 = Mathf.Lerp(m_sprite.sprite.uv[0].y, m_sprite.sprite.uv[1].y, num2);
				Texture2D texture = sprite.texture;
				Debug.Log($"Ratio: ( {num}, {num2} ), UV: ( {num3}, {num4} ), Pixel: ( {(int)(num3 * (float)texture.width)}, {(int)(num4 * (float)texture.height)} )");
				if (texture.GetPixelBilinear(num3, num4).a > 0.1f)
				{
					return true;
				}
			}
			return false;
		}
	}
}
