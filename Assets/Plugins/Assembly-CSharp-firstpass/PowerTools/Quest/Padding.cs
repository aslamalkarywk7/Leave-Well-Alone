using System;
using UnityEngine;

namespace PowerTools.Quest
{
	[Serializable]
	public struct Padding
	{
		public static readonly Padding zero = new Padding(0f, 0f, 0f, 0f);

		public float left;

		public float right;

		public float top;

		public float bottom;

		public float width => left + right;

		public float height => top + bottom;

		public Vector2 size => new Vector2(width, height);

		public Padding(float l, float r, float t, float b)
		{
			left = l;
			right = r;
			top = t;
			bottom = b;
		}
	}
}
