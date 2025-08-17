using System;
using UnityEngine;

namespace PowerTools.Quest.Text
{
	public class BitMaskAttribute : PropertyAttribute
	{
		public Type propType;

		public BitMaskAttribute(Type aType)
		{
			propType = aType;
		}
	}
}
