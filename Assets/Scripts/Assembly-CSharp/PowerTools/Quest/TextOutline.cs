using System;
using UnityEngine;

namespace PowerTools.Quest
{
	[Serializable]
	public class TextOutline
	{
		public enum eDirection
		{
			Top = 1,
			Bottom = 2,
			Left = 4,
			Right = 8,
			TopLeft = 0x10,
			TopRight = 0x20,
			BottomLeft = 0x40,
			BottomRight = 0x80
		}

		[BitMask(typeof(eDirection))]
		public int m_directions;

		public float m_width = 1f;

		public Color m_color = Color.black;
	}
}
