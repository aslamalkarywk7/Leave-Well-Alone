using UnityEngine;

namespace PowerTools
{
	public static class PathfinderExtentionMethods
	{
		public static Vector2 GetTangent(this Vector2 vector)
		{
			return new Vector2(0f - vector.y, vector.x);
		}

		public static Vector2 GetTangentR(this Vector2 vector)
		{
			return new Vector2(vector.y, 0f - vector.x);
		}
	}
}
