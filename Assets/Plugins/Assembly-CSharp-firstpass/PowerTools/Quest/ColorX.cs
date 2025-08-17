using UnityEngine;

namespace PowerTools.Quest
{
	public class ColorX
	{
		private static readonly string ALPHA_STRING = "0123456789abcdef";

		private static string GetHex(int num)
		{
			return ALPHA_STRING[num].ToString();
		}

		private static int HexToInt(char hexChar)
		{
			switch (hexChar)
			{
			case '0':
				return 0;
			case '1':
				return 1;
			case '2':
				return 2;
			case '3':
				return 3;
			case '4':
				return 4;
			case '5':
				return 5;
			case '6':
				return 6;
			case '7':
				return 7;
			case '8':
				return 8;
			case '9':
				return 9;
			case 'A':
			case 'a':
				return 10;
			case 'B':
			case 'b':
				return 11;
			case 'C':
			case 'c':
				return 12;
			case 'D':
			case 'd':
				return 13;
			case 'E':
			case 'e':
				return 14;
			case 'F':
			case 'f':
				return 15;
			default:
				return -1;
			}
		}

		public static string RGBToHex(Color color)
		{
			float num = color.r * 255f;
			float num2 = color.g * 255f;
			float num3 = color.b * 255f;
			string hex = GetHex(Mathf.FloorToInt(num / 16f));
			string hex2 = GetHex(Mathf.RoundToInt(num) % 16);
			string hex3 = GetHex(Mathf.FloorToInt(num2 / 16f));
			string hex4 = GetHex(Mathf.RoundToInt(num2) % 16);
			string hex5 = GetHex(Mathf.FloorToInt(num3 / 16f));
			string hex6 = GetHex(Mathf.RoundToInt(num3) % 16);
			return hex + hex2 + hex3 + hex4 + hex5 + hex6;
		}

		public static Color HexToRGB(string color)
		{
			Color result = Color.magenta;
			if (color.Length > 0 && color[0] == '#')
			{
				color = color.Substring(1);
			}
			if (color.Length == 3)
			{
				float r = (float)HexToInt(color[0]) / 255f;
				float g = (float)HexToInt(color[1]) / 255f;
				float b = (float)HexToInt(color[2]) / 255f;
				result = new Color
				{
					r = r,
					g = g,
					b = b,
					a = 1f
				};
			}
			else if (color.Length == 6)
			{
				float r2 = ((float)HexToInt(color[1]) + (float)HexToInt(color[0]) * 16f) / 255f;
				float g2 = ((float)HexToInt(color[3]) + (float)HexToInt(color[2]) * 16f) / 255f;
				float b2 = ((float)HexToInt(color[5]) + (float)HexToInt(color[4]) * 16f) / 255f;
				result = new Color
				{
					r = r2,
					g = g2,
					b = b2,
					a = 1f
				};
			}
			return result;
		}
	}
}
