using UnityEngine;

namespace PowerTools.Quest
{
	public class WeightedArrayAttribute : PropertyAttribute
	{
		public string m_weightPropertyName = "m_weight";

		public string m_dataPropertyName;

		public WeightedArrayAttribute()
		{
		}

		public WeightedArrayAttribute(string propertyName)
		{
			m_weightPropertyName = propertyName;
		}

		public WeightedArrayAttribute(string weightPropertyName, string dataPropertyName)
		{
			m_weightPropertyName = weightPropertyName;
			m_dataPropertyName = dataPropertyName;
		}
	}
}
