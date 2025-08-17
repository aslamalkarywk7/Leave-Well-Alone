using System;
using System.Collections.Generic;
using UnityEngine;

namespace PowerTools.Quest
{
	public class Utils
	{
		private static readonly float ASPECT_16_9 = 1.7777778f;

		private static readonly float ASPECT_16_9_INV = 1f / ASPECT_16_9;

		public static float NormalizeMag(ref Vector2 vector)
		{
			if (ApproximatelyZero(vector.x, float.Epsilon))
			{
				if (ApproximatelyZero(vector.y, float.Epsilon))
				{
					vector = Vector2.zero;
					return 0f;
				}
				float result = Mathf.Abs(vector.y);
				vector.Set(0f, Mathf.Sign(vector.y));
				return result;
			}
			if (ApproximatelyZero(vector.y, float.Epsilon))
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

		public static Vector3 Snap(Vector3 pos, float snapTo = 1f)
		{
			return new Vector3(Snap(pos.x, snapTo), Snap(pos.y, snapTo), Snap(pos.z, snapTo));
		}

		public static Vector2 Snap(Vector2 pos, float snapTo = 1f)
		{
			return new Vector2(Snap(pos.x, snapTo), Snap(pos.y, snapTo));
		}

		public static float Snap(float pos, float snapTo = 1f)
		{
			if (snapTo < 0.001f)
			{
				return pos;
			}
			return Mathf.Floor(pos / snapTo) * snapTo;
		}

		public static Vector2 SnapRound(Vector2 pos, float snapTo = 1f)
		{
			return new Vector2(SnapRound(pos.x, snapTo), SnapRound(pos.y, snapTo));
		}

		public static float SnapRound(float pos, float snapTo = 1f)
		{
			if (snapTo < 0.001f)
			{
				return pos;
			}
			return Mathf.Round(pos / snapTo) * snapTo;
		}

		public static float Flip(float value, bool flip)
		{
			if (!flip)
			{
				return value;
			}
			return 0f - value;
		}

		public static bool Approximately(float a, float b, float epsilon)
		{
			if (!(a > b))
			{
				return a > b - epsilon;
			}
			return a < b + epsilon;
		}

		public static bool ApproximatelyZero(float a, float epsilon)
		{
			if (!(a > 0f))
			{
				return a > 0f - epsilon;
			}
			return a < epsilon;
		}

		public static bool ApproximatelyZero(float a)
		{
			if (!(a > 0f))
			{
				return a > 0f - Mathf.Epsilon;
			}
			return a < Mathf.Epsilon;
		}

		public static bool IsInLayerMask(GameObject obj, LayerMask mask)
		{
			return (mask.value & (1 << obj.layer)) > 0;
		}

		public static float EaseCubic(float ratio)
		{
			ratio = Mathf.Clamp01(ratio);
			return -2f * ratio * ratio * ratio + 3f * ratio * ratio;
		}

		public static float EaseInCubic(float ratio)
		{
			ratio = Mathf.Clamp01(ratio);
			ratio *= 0.5f;
			return (-2f * ratio * ratio * ratio + 3f * ratio * ratio) * 2f;
		}

		public static float EaseOutCubic(float ratio)
		{
			ratio = Mathf.Clamp01(ratio);
			ratio = ratio * 0.5f + 0.5f;
			return (-2f * ratio * ratio * ratio + 3f * ratio * ratio) * 2f - 1f;
		}

		public static float EaseCubic(float start, float end, float ratio)
		{
			return start + (end - start) * EaseCubic(ratio);
		}

		public static float Interpolate(float from, float to, float minVal, float maxVal, float val)
		{
			float num = maxVal - minVal;
			if (num == 0f)
			{
				return from;
			}
			return Mathf.Lerp(from, to, Mathf.Clamp01((val - minVal) / num));
		}

		public static float Loop(float val, float min, float max)
		{
			while (val < min)
			{
				val += max - min;
			}
			while (val > max)
			{
				val -= max - min;
			}
			return val;
		}

		public static Quaternion GetDirectionRotation(Vector2 direction)
		{
			if (!ApproximatelyZero(direction.y, float.Epsilon))
			{
				Quaternion result = Quaternion.FromToRotation(Vector3.right, direction);
				if (Approximately(result.z, 1f, float.Epsilon))
				{
					return result;
				}
			}
			return Quaternion.Euler(0f, 0f, 57.29578f * Mathf.Atan2(direction.y, direction.x));
		}

		public static float GetDirectionAngle(Vector2 directionNormalised)
		{
			if (ApproximatelyZero(directionNormalised.y))
			{
				if (directionNormalised.x < 0f)
				{
					return 180f;
				}
				return 0f;
			}
			if (ApproximatelyZero(directionNormalised.x))
			{
				if (directionNormalised.y < 0f)
				{
					return 270f;
				}
				return 90f;
			}
			return Mathf.Repeat(57.29578f * Mathf.Atan2(directionNormalised.y, directionNormalised.x), 360f);
		}

		public static void Swap<T>(ref T lhs, ref T rhs)
		{
			T val = lhs;
			lhs = rhs;
			rhs = val;
		}

		public static float ClampAngle(float angle, float min, float max)
		{
			float num = 180f - (min + max) * 0.5f;
			min = Mathf.Repeat(min + num, 360f);
			max = Mathf.Repeat(max + num, 360f);
			angle = Mathf.Repeat(angle + num, 360f);
			return Mathf.Clamp(angle, min, max) - num;
		}

		public static bool IsWithinAngle(float angle, float min, float max)
		{
			angle = Mathf.Repeat(angle - min, 360f);
			max = Mathf.Repeat(max - min, 360f);
			if (angle >= 0f)
			{
				return angle < max;
			}
			return false;
		}

		public static bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
		{
			bool flag = false;
			int count = polygon.Count;
			int num = 0;
			int index = count - 1;
			while (num < count)
			{
				if (polygon[num].y > point.y != polygon[index].y > point.y && point.x < (polygon[index].x - polygon[num].x) * (point.y - polygon[num].y) / (polygon[index].y - polygon[num].y) + polygon[num].x)
				{
					flag = !flag;
				}
				index = num++;
			}
			return flag;
		}

		public static int GetUnixTimestamp()
		{
			DateTime dateTime = new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			return (int)(DateTime.UtcNow - dateTime).TotalSeconds;
		}

		public static bool GetTimeIncrementPassed(float time)
		{
			return (Time.timeSinceLevelLoad - Time.deltaTime) % time > Time.timeSinceLevelLoad % time;
		}

		public static bool GetTimeIncrementPassed(float min, float max, ref float period)
		{
			if (period <= 0f)
			{
				period = UnityEngine.Random.Range(min, max);
			}
			period -= Time.deltaTime;
			if (period <= 0f)
			{
				period = UnityEngine.Random.Range(min, max);
				return true;
			}
			return false;
		}

		public static T[] CreateFilledArray<T>(int size, T value)
		{
			T[] array = new T[size];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = value;
			}
			return array;
		}

		public static float NormalizeScreenRatioTo1080X(Camera camera, float ratio)
		{
			if (camera == null)
			{
				return ratio;
			}
			return ratio * (camera.aspect * ASPECT_16_9_INV);
		}

		public static float NormalizeScreenRatioTo1080Y(Camera camera, float ratio)
		{
			if (camera == null)
			{
				return ratio;
			}
			return ratio * (1f / camera.aspect * ASPECT_16_9);
		}

		public static int MaskSetAt(int mask, int index, bool value)
		{
			if (!value)
			{
				return mask & ~(1 << index);
			}
			return mask | (1 << index);
		}

		public static int MaskSetAt(int mask, int index)
		{
			return mask | (1 << index);
		}

		public static int MaskUnsetAt(int mask, int index)
		{
			return mask & ~(1 << index);
		}

		public static bool MaskIsSet(int mask, int index)
		{
			return (mask & (1 << index)) != 0;
		}

		public static tEnum ToEnum<tEnum>(string str) where tEnum : struct, IConvertible
		{
			if (Enum.TryParse<tEnum>(str, ignoreCase: true, out var result))
			{
				return result;
			}
			if (Debug.isDebugBuild)
			{
				Debug.LogWarning("Failed to parse enum " + str + " from " + typeof(tEnum).ToString());
			}
			return default(tEnum);
		}

		public static Vector2 RandomDirection(float minAngle = 0f, float maxAngle = 360f)
		{
			float f = UnityEngine.Random.Range(minAngle, maxAngle) * ((float)Math.PI / 180f);
			return new Vector2(Mathf.Cos(f), Mathf.Sin(f));
		}

		public static Vector2 RandomPointInCircle(float radius)
		{
			radius = Mathf.Sqrt(UnityEngine.Random.value) * radius;
			float f = UnityEngine.Random.value * (float)Math.PI * 2f;
			return new Vector2(radius * Mathf.Cos(f), radius * Mathf.Sin(f));
		}

		public static Vector2 RandomPointInCircle(float minRadius, float maxRadius, float minAngle = 0f, float maxAngle = 360f)
		{
			float num = Mathf.Sqrt(UnityEngine.Random.Range(Mathf.Pow(minRadius / maxRadius, 2f), 1f)) * maxRadius;
			float f = UnityEngine.Random.Range(minAngle, maxAngle) * ((float)Math.PI / 180f);
			return new Vector2(num * Mathf.Cos(f), num * Mathf.Sin(f));
		}

		public static bool IsEmpty(string str)
		{
			return string.IsNullOrEmpty(str);
		}

		public static bool IsNotEmpty(string str)
		{
			return !string.IsNullOrEmpty(str);
		}

		public static bool HasText(string str)
		{
			return !string.IsNullOrEmpty(str);
		}

		public static Color HexToColor(string hex = "abcdef")
		{
			return ColorX.HexToRGB(hex);
		}

		public static Color ColorFromHex(string hex = "abcdef")
		{
			return ColorX.HexToRGB(hex);
		}
	}
}
