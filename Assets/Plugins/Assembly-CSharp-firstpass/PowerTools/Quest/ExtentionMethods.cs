using System;
using System.Collections.Generic;
using UnityEngine;

namespace PowerTools.Quest
{
	public static class ExtentionMethods
	{
		public static float GetWidth(this Camera cam)
		{
			return cam.orthographicSize * 2f * cam.aspect;
		}

		public static float GetHeight(this Camera cam)
		{
			return cam.orthographicSize * 2f;
		}

		public static Rect Encapsulate(this Rect rect, Rect other)
		{
			return Rect.MinMaxRect(Mathf.Min(rect.xMin, other.xMin), Mathf.Min(rect.yMin, other.yMin), Mathf.Max(rect.xMax, other.xMax), Mathf.Max(rect.yMax, other.yMax));
		}

		public static Vector2 CalcDistToPoint(this Rect rect, Vector2 point)
		{
			if (rect.Contains(point))
			{
				return Vector2.zero;
			}
			Vector2 zero = Vector2.zero;
			if (point.x < rect.xMin)
			{
				zero.x = rect.xMin - point.x;
			}
			if (point.x > rect.xMax)
			{
				zero.x = rect.xMax - point.x;
			}
			if (point.y < rect.yMin)
			{
				zero.y = rect.yMin - point.y;
			}
			if (point.y > rect.yMax)
			{
				zero.y = rect.yMax - point.y;
			}
			return zero;
		}

		public static float NormalizeMag(this ref Vector2 vector)
		{
			if (Utils.ApproximatelyZero(vector.x, float.Epsilon))
			{
				if (Utils.ApproximatelyZero(vector.y, float.Epsilon))
				{
					vector = Vector2.zero;
					return 0f;
				}
				float result = Mathf.Abs(vector.y);
				vector.Set(0f, Mathf.Sign(vector.y));
				return result;
			}
			if (Utils.ApproximatelyZero(vector.y, float.Epsilon))
			{
				float result2 = Mathf.Abs(vector.x);
				vector.Set(Mathf.Sign(vector.x), 0f);
				return result2;
			}
			float magnitude = vector.magnitude;
			if (magnitude == 0f)
			{
				vector = Vector2.zero;
			}
			else
			{
				vector /= magnitude;
			}
			return magnitude;
		}

		public static Vector2 WithOffset(this Vector2 vector, float x, float y)
		{
			return new Vector2(vector.x + x, vector.y + y);
		}

		public static Vector2 Scaled(this Vector2 vector, Vector2 scale)
		{
			return Vector2.Scale(vector, scale);
		}

		public static Vector2 WithX(this Vector2 vector, float x)
		{
			return new Vector2(x, vector.y);
		}

		public static Vector2 WithY(this Vector2 vector, float y)
		{
			return new Vector2(vector.x, y);
		}

		public static Vector3 WithZ(this Vector2 vector, float z)
		{
			return new Vector3(vector.x, vector.y, z);
		}

		public static Vector3 WithX(this Vector3 vector, float x)
		{
			return new Vector3(x, vector.y, vector.z);
		}

		public static Vector3 WithY(this Vector3 vector, float y)
		{
			return new Vector3(vector.x, y, vector.z);
		}

		public static Vector3 WithXY(this Vector3 vector, float x, float y)
		{
			return new Vector3(x, y, vector.z);
		}

		public static Vector3 WithZ(this Vector3 vector, float z)
		{
			return new Vector3(vector.x, vector.y, z);
		}

		public static Vector2 WithFlippedX(this Vector2 vector)
		{
			return new Vector2(0f - vector.x, vector.y);
		}

		public static Vector2 WithFlippedY(this Vector2 vector)
		{
			return new Vector2(vector.x, 0f - vector.y);
		}

		public static Vector3 WithFlippedX(this Vector3 vector)
		{
			return new Vector3(0f - vector.x, vector.y, vector.z);
		}

		public static Vector3 WithFlippedY(this Vector3 vector)
		{
			return new Vector3(vector.x, 0f - vector.y, vector.z);
		}

		public static Vector2 Clamp(this Vector2 vector, Vector2 min, Vector2 max)
		{
			vector.x = Mathf.Clamp(vector.x, min.x, max.x);
			vector.y = Mathf.Clamp(vector.y, min.y, max.y);
			return vector;
		}

		public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max)
		{
			vector.x = Mathf.Clamp(vector.x, min.x, max.x);
			vector.y = Mathf.Clamp(vector.y, min.y, max.y);
			vector.z = Mathf.Clamp(vector.z, min.z, max.z);
			return vector;
		}

		public static Vector2 Clamp01(this Vector2 vector)
		{
			vector.x = Mathf.Clamp01(vector.x);
			vector.y = Mathf.Clamp01(vector.y);
			return vector;
		}

		public static Vector3 Clamp01(this Vector3 vector)
		{
			vector.x = Mathf.Clamp01(vector.x);
			vector.y = Mathf.Clamp01(vector.y);
			vector.z = Mathf.Clamp01(vector.z);
			return vector;
		}

		public static Vector2 Rotate(this Vector2 v, float degrees)
		{
			float f = degrees * ((float)Math.PI / 180f);
			float num = Mathf.Sin(f);
			float num2 = Mathf.Cos(f);
			float x = v.x;
			float y = v.y;
			v.x = num2 * x - num * y;
			v.y = num * x + num2 * y;
			return v;
		}

		public static Vector2 GetTangent(this Vector2 vector)
		{
			return new Vector2(0f - vector.y, vector.x);
		}

		public static Vector2 GetTangentR(this Vector2 vector)
		{
			return new Vector2(vector.y, 0f - vector.x);
		}

		public static Vector3 Snap(this Vector3 pos, float snapTo)
		{
			return new Vector3(Utils.Snap(pos.x, snapTo), Utils.Snap(pos.y, snapTo), Utils.Snap(pos.z, snapTo));
		}

		public static Vector2 Snap(this Vector2 pos, float snapTo)
		{
			return new Vector2(Utils.Snap(pos.x, snapTo), Utils.Snap(pos.y, snapTo));
		}

		public static Vector3 SnapRound(this Vector3 pos, float snapTo)
		{
			return new Vector3(Utils.SnapRound(pos.x, snapTo), Utils.Snap(pos.y, snapTo), Utils.Snap(pos.z, snapTo));
		}

		public static Vector2 SnapRound(this Vector2 pos, float snapTo)
		{
			return new Vector2(Utils.SnapRound(pos.x, snapTo), Utils.Snap(pos.y, snapTo));
		}

		public static bool ApproximatelyEquals(this Vector3 pos, Vector3 other)
		{
			return (pos - other).sqrMagnitude < float.Epsilon;
		}

		public static bool ApproximatelyEquals(this Vector2 pos, Vector2 other)
		{
			return (pos - other).sqrMagnitude < float.Epsilon;
		}

		public static bool IsInLayerMask(this GameObject obj, LayerMask mask)
		{
			return (mask.value & (1 << obj.layer)) > 0;
		}

		public static bool EqualsIgnoreCase(this string first, string second)
		{
			return first.Equals(second, StringComparison.OrdinalIgnoreCase);
		}

		public static bool StartsWithIgnoreCase(this string first, string second)
		{
			return first.StartsWith(second, StringComparison.OrdinalIgnoreCase);
		}

		public static bool ContainsIgnoreCase(this string first, string second)
		{
			return first.IndexOf(second, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public static Quaternion GetDirectionRotation(this Vector2 direction)
		{
			if (direction.y != 0f)
			{
				return Quaternion.FromToRotation(Vector3.right, direction);
			}
			Quaternion identity = Quaternion.identity;
			identity.eulerAngles = new Vector3(0f, 0f, 57.29578f * Mathf.Atan2(direction.y, direction.x));
			return identity;
		}

		public static float GetDirectionAngle(this Vector2 directionNormalised)
		{
			if (Utils.ApproximatelyZero(directionNormalised.y))
			{
				if (directionNormalised.x < 0f)
				{
					return 180f;
				}
				return 0f;
			}
			if (Utils.ApproximatelyZero(directionNormalised.x))
			{
				if (directionNormalised.y < 0f)
				{
					return 270f;
				}
				return 90f;
			}
			return Mathf.Repeat(57.29578f * Mathf.Atan2(directionNormalised.y, directionNormalised.x), 360f);
		}

		public static T GetComponentInParents<T>(this GameObject gameObject) where T : Component
		{
			Transform transform = gameObject.transform;
			while (transform != null)
			{
				T component = transform.GetComponent<T>();
				if (component != null)
				{
					return component;
				}
				transform = transform.parent;
			}
			return null;
		}

		public static void RemoveDefaultElements<T>(this List<T> list)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (object.Equals(list[num], default(T)))
				{
					list.RemoveAt(num);
				}
			}
		}

		public static T LastOrDefault<T>(this IList<T> list)
		{
			if (list != null && list.Count != 0)
			{
				return list[list.Count - 1];
			}
			return default(T);
		}

		public static T FirstOrDefault<T>(this IList<T> list)
		{
			if (list != null && list.Count != 0)
			{
				return list[0];
			}
			return default(T);
		}

		public static T ElementAtOrDefault<T>(this IList<T> list, int index)
		{
			if (list != null && index >= 0 && index >= list.Count)
			{
				return list[index];
			}
			return default(T);
		}

		public static T LastOrDefault<T>(this T[] list)
		{
			if (list != null && list.Length != 0)
			{
				return list[list.Length - 1];
			}
			return default(T);
		}

		public static T FirstOrDefault<T>(this T[] list)
		{
			if (list != null && list.Length != 0)
			{
				return list[0];
			}
			return default(T);
		}

		public static T ElementAtOrDefault<T>(this T[] list, int index)
		{
			if (list != null && index >= 0 && index >= list.Length)
			{
				return list[index];
			}
			return default(T);
		}

		public static List<T> Swap<T>(this List<T> list, int indexA, int indexB)
		{
			T value = list[indexA];
			list[indexA] = list[indexB];
			list[indexB] = value;
			return list;
		}

		public static void Shuffle<T>(this IList<T> list)
		{
			int num = 0;
			for (int num2 = list.Count - 1; num2 >= 1; num2--)
			{
				num = UnityEngine.Random.Range(0, num2 + 1);
				T value = list[num2];
				list[num2] = list[num];
				list[num] = value;
			}
		}

		public static List<T> ShuffleListCopy<T>(this List<T> list)
		{
			int count = list.Count;
			List<T> list2 = new List<T>(list);
			int num = 0;
			for (int i = 1; i < count; i++)
			{
				num = UnityEngine.Random.Range(0, i + 1);
				if (num != i)
				{
					list2[i] = list2[num];
				}
				list2[num] = list[i];
			}
			return list2;
		}

		public static void Shuffle<T>(this T[] list)
		{
			int num = 0;
			for (int num2 = list.Length - 1; num2 >= 1; num2--)
			{
				num = UnityEngine.Random.Range(0, num2 + 1);
				T val = list[num2];
				list[num2] = list[num];
				list[num] = val;
			}
		}

		public static T[] ShuffleCopy<T>(this T[] list)
		{
			int num = list.Length;
			T[] array = new T[num];
			array[0] = list[0];
			int num2 = 0;
			for (int i = 1; i < num; i++)
			{
				num2 = UnityEngine.Random.Range(0, i + 1);
				if (num2 != i)
				{
					array[i] = array[num2];
				}
				array[num2] = list[i];
			}
			return array;
		}

		public static T Choose<T>(this IList<T> values, Func<T, float> getWeight)
		{
			float num = 0f;
			for (int i = 0; i < values.Count; i++)
			{
				num += getWeight(values[i]);
			}
			float num2 = UnityEngine.Random.value * num;
			for (int j = 0; j < values.Count; j++)
			{
				num2 -= getWeight(values[j]);
				if (num2 < 0f)
				{
					return values[j];
				}
			}
			return values[values.Count - 1];
		}

		public static Color WithAlpha(this Color col, float alpha)
		{
			return new Color(col.r, col.g, col.b, alpha);
		}

		public static bool IsIndexValid<T>(this List<T> list, int index)
		{
			if (index >= 0)
			{
				return index < list.Count;
			}
			return false;
		}

		public static bool IsIndexValid<T>(this T[] list, int index)
		{
			if (index >= 0)
			{
				return index < list.Length;
			}
			return false;
		}

		public static T[] Populate<T>(this T[] arr, T value)
		{
			for (int i = 0; i < arr.Length; i++)
			{
				arr[i] = value;
			}
			return arr;
		}

		public static Rect GetWorldRect(this RectTransform rectTransform)
		{
			Vector3[] array = new Vector3[4];
			rectTransform.GetWorldCorners(array);
			Vector3 vector = array[0];
			Vector2 size = new Vector2(rectTransform.lossyScale.x * rectTransform.rect.size.x, rectTransform.lossyScale.y * rectTransform.rect.size.y);
			return new Rect(vector, size);
		}
	}
}
