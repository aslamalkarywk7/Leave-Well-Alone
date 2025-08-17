using UnityEngine;

namespace PowerTools.PowerTextUtils
{
	public static class Utils
	{
		public static void Shuffle<T>(this T[] list)
		{
			int num = 0;
			for (int num2 = list.Length - 1; num2 >= 1; num2--)
			{
				num = Random.Range(0, num2 + 1);
				T val = list[num2];
				list[num2] = list[num];
				list[num] = val;
			}
		}

		public static void Swap<T>(ref T lhs, ref T rhs)
		{
			T val = lhs;
			lhs = rhs;
			rhs = val;
		}
	}
}
