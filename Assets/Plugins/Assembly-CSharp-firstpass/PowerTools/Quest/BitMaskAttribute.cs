using System;
using UnityEngine;

namespace PowerTools.Quest
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
