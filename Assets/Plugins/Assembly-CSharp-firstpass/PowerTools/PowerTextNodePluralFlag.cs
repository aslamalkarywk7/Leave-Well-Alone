using System.Collections.Generic;
using System.Text;

namespace PowerTools
{
	public class PowerTextNodePluralFlag : IPowerTextNode
	{
		private bool m_pluralize;

		public PowerTextNodePluralFlag(bool pluralize)
		{
			m_pluralize = pluralize;
		}

		public void Build(StringBuilder builder, ref bool pluralize)
		{
			pluralize = m_pluralize;
		}

		public void Build(List<string> parts, ref bool pluralize)
		{
			pluralize = m_pluralize;
		}

		public void GetParts(List<string> parts)
		{
		}

		public float GetWeight()
		{
			return 1f;
		}
	}
}
