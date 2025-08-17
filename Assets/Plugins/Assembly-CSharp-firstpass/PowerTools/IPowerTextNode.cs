using System.Collections.Generic;
using System.Text;

namespace PowerTools
{
	public interface IPowerTextNode
	{
		void Build(StringBuilder builder, ref bool pluralize);

		void Build(List<string> parts, ref bool pluralize);

		void GetParts(List<string> parts);

		float GetWeight();
	}
}
