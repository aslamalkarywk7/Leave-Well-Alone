using System.Collections;
using UnityEngine;

namespace PowerTools
{
	public class TextWrapper
	{
		private Hashtable dict;

		private TextMesh textMesh;

		private Renderer renderer;

		private static readonly string STRING_SPACE = " ";

		public float Width => GetTextWidth(textMesh.text);

		public float Height => renderer.bounds.size.y;

		public Bounds Bounds => renderer.bounds;

		public TextWrapper(TextMesh tm)
		{
			textMesh = tm;
			renderer = tm.GetComponent<Renderer>();
			dict = new Hashtable();
			GetSpace();
		}

		private void GetSpace()
		{
			string text = textMesh.text;
			textMesh.text = "a";
			float x = renderer.bounds.size.x;
			textMesh.text = "a a";
			float num = renderer.bounds.size.x - 2f * x;
			dict.Add(' ', num);
			dict.Add('a', x);
			textMesh.text = text;
		}

		public float GetTextWidth(string s)
		{
			char[] array = s.ToCharArray();
			float num = 0f;
			string text = textMesh.text;
			bool flag = false;
			for (int i = 0; i < array.Length; i++)
			{
				char c = array[i];
				if (c == '<')
				{
					flag = true;
				}
				if (!flag)
				{
					if (dict.ContainsKey(c))
					{
						num += (float)dict[c];
					}
					else
					{
						textMesh.text = c.ToString();
						float x = renderer.bounds.size.x;
						dict.Add(c, x);
						num += x;
					}
				}
				if (c == '>')
				{
					flag = false;
				}
			}
			textMesh.text = text;
			return num;
		}

		public string WrapText(string input, float width)
		{
			string text = string.Empty;
			string[] array = input.Split('\n');
			for (int i = 0; i < array.Length; i++)
			{
				if (i > 0)
				{
					text += "\n";
				}
				string[] array2 = array[i].Split(' ');
				string text2 = string.Empty;
				string text3 = array2[0];
				for (int j = 0; j < array2.Length; j++)
				{
					if (j > 0)
					{
						text3 = text2 + STRING_SPACE + array2[j];
					}
					if (GetTextWidth(text3) > width)
					{
						text = text + text2 + "\n";
						text2 = array2[j];
					}
					else
					{
						text2 = text3;
					}
				}
				text += text2;
			}
			return text;
		}

		public string WrapTextMinimiseWidth(string input, float width, float minWidth = 0f)
		{
			int numLines = 0;
			string result = string.Empty;
			float textWidth = GetTextWidth("ABC");
			string text = WrapTextNicerInternal(input, width, out numLines);
			if (numLines <= 1)
			{
				return text;
			}
			int numLines2 = numLines;
			while (numLines2 == numLines && (minWidth <= 0f || width > minWidth))
			{
				result = text;
				width -= textWidth;
				text = WrapTextNicerInternal(input, width, out numLines2);
			}
			return result;
		}

		private string WrapTextNicerInternal(string input, float width, out int numLines)
		{
			string text = string.Empty;
			numLines = 0;
			string[] array = input.Split('\n');
			if (width <= 0f)
			{
				numLines = array.Length;
				return input;
			}
			for (int i = 0; i < array.Length; i++)
			{
				numLines++;
				if (i > 0)
				{
					text += "\n";
				}
				string[] array2 = array[i].Split(' ');
				string text2 = string.Empty;
				string text3 = array2[0];
				for (int j = 0; j < array2.Length; j++)
				{
					if (j > 0)
					{
						text3 = text2 + STRING_SPACE + array2[j];
					}
					if (GetTextWidth(text3) > width)
					{
						text = text + text2 + "\n";
						numLines++;
						text2 = array2[j];
					}
					else
					{
						text2 = text3;
					}
				}
				text += text2;
			}
			return text;
		}

		public string Truncate(string input, float width)
		{
			float textWidth = GetTextWidth("...");
			string text = string.Empty;
			string[] array = input.Split('\n');
			for (int i = 0; i < array.Length; i++)
			{
				if (i > 0)
				{
					text += "\n";
				}
				if (GetTextWidth(array[i]) <= width)
				{
					text += array[i];
					continue;
				}
				string[] array2 = array[i].Split(' ');
				int num = 0;
				string text2 = string.Empty + array2[num];
				while (GetTextWidth(text2) + textWidth <= width && num++ < array2.Length)
				{
					text2 = text2 + STRING_SPACE + array2[num];
				}
				if (num < array2.GetUpperBound(0))
				{
					text2 += "...";
				}
				text += text2;
			}
			return text;
		}
	}
}
