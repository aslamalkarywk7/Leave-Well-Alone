using System.Collections.Generic;
using UnityEngine;

namespace PowerTools.Quest
{
	public static class GuiUtils
	{
		public static RectCentered CalculateGuiRect(Transform transform, bool includeChildren, SpriteRenderer spriteRenderer = null, MeshRenderer textMesh = null, Transform excludeChildren = null)
		{
			if (spriteRenderer == null && textMesh == null)
			{
				GuiControl guiControl = (includeChildren ? transform.GetComponentInChildren<GuiControl>(includeInactive: false) : transform.GetComponent<GuiControl>());
				if (guiControl != null && (transform == guiControl.transform || guiControl.transform != excludeChildren))
				{
					RectCentered rect = guiControl.GetRect(transform);
					rect.UndoTransform(transform);
					return rect;
				}
			}
			return CalculateGuiRectInternal(transform, includeChildren, spriteRenderer, textMesh, excludeChildren);
		}

		public static RectCentered CalculateGuiRectInternal(Transform transform, bool includeChildren, SpriteRenderer spriteRenderer = null, MeshRenderer textMesh = null, Transform excludeChildren = null)
		{
			if (spriteRenderer == null)
			{
				spriteRenderer = (includeChildren ? transform.GetComponentInChildren<SpriteRenderer>(includeInactive: false) : transform.GetComponent<SpriteRenderer>());
			}
			if (spriteRenderer != null && spriteRenderer.sprite != null)
			{
				return CalculateGuiRectFromSprite(transform, includeChildren, spriteRenderer, excludeChildren);
			}
			return CalculateGuiRectFromRenderer(transform, includeChildren, textMesh, excludeChildren);
		}

		public static RectCentered CalculateGuiRectFromSprite(Transform transform, bool includeChildren, SpriteRenderer spriteRenderer = null, Transform excludeChildren = null)
		{
			RectCentered result = RectCentered.zero;
			if (spriteRenderer == null)
			{
				spriteRenderer = (includeChildren ? transform.GetComponentInChildren<SpriteRenderer>() : transform.GetComponent<SpriteRenderer>());
			}
			if (spriteRenderer != null && transform != spriteRenderer.transform && spriteRenderer.transform == excludeChildren)
			{
				return result;
			}
			Sprite sprite = ((spriteRenderer == null) ? null : spriteRenderer.sprite);
			if (spriteRenderer == null || sprite == null)
			{
				return result;
			}
			if (spriteRenderer.drawMode != SpriteDrawMode.Simple)
			{
				result.Center = spriteRenderer.bounds.center - transform.position;
				result.Size = spriteRenderer.size;
			}
			else
			{
				bool flag = true;
				bool flag2 = false;
				if (sprite.bounds.size.x < 32f || sprite.bounds.size.y < 32f)
				{
					flag2 = true;
					List<Vector2> list = new List<Vector2>();
					for (int i = 0; i < sprite.GetPhysicsShapeCount(); i++)
					{
						int physicsShape = sprite.GetPhysicsShape(i, list);
						for (int j = 0; j < physicsShape; j++)
						{
							Vector2 vector = list[j];
							if (flag)
							{
								result = new RectCentered(vector, vector);
							}
							else
							{
								result.Encapsulate(vector);
							}
							flag = false;
						}
					}
				}
				else
				{
					Vector2[] vertices = sprite.vertices;
					foreach (Vector2 vector2 in vertices)
					{
						if (flag)
						{
							result = new RectCentered(vector2, vector2);
						}
						else
						{
							result.Encapsulate(vector2);
						}
						flag = false;
					}
				}
				if (sprite.textureRectOffset != Vector2.zero)
				{
					result.Width -= 4f;
					result.Height -= 4f;
				}
				else if (flag2)
				{
					result.Width -= 2f;
					result.Height -= 2f;
				}
				if (transform != spriteRenderer.transform)
				{
					result.Size = result.Size.Scaled(spriteRenderer.transform.localScale);
					result.Center += (Vector2)(spriteRenderer.transform.position - transform.position);
				}
			}
			return result;
		}

		public static RectCentered CalculateGuiRectFromRenderer(Transform transform, bool includeChildren, MeshRenderer renderer = null, Transform exclude = null)
		{
			RectCentered result = default(RectCentered);
			if (renderer == null)
			{
				renderer = (includeChildren ? transform.GetComponentInChildren<MeshRenderer>() : transform.GetComponent<MeshRenderer>());
			}
			if (renderer != null && transform != renderer.transform && renderer.transform == exclude)
			{
				return RectCentered.zero;
			}
			if (renderer == null)
			{
				return result;
			}
			result = new RectCentered(renderer.bounds);
			result.UndoTransform(transform);
			return result;
		}

		public static Camera FindGuiCamera()
		{
			Camera[] array = new Camera[10];
			int allCameras = Camera.GetAllCameras(array);
			for (int i = 0; i < allCameras && i < array.Length; i++)
			{
				Camera camera = array[i];
				if (camera.gameObject.layer == 5 || camera.gameObject.name.Contains("GUI"))
				{
					return camera;
				}
			}
			if (array.Length != 0)
			{
				return array[0];
			}
			return null;
		}
	}
}
