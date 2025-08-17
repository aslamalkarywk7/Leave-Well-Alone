using System;

public class MinMaxRangeAttribute : Attribute
{
	public float Min { get; private set; }

	public float Max { get; private set; }

	public float ScaleFactor { get; private set; }

	public MinMaxRangeAttribute(float min, float max)
	{
		Min = min;
		Max = max;
		ScaleFactor = 1f;
	}

	public MinMaxRangeAttribute(float min, float max, float scaleFactor)
	{
		Min = min;
		Max = max;
		ScaleFactor = scaleFactor;
	}
}
