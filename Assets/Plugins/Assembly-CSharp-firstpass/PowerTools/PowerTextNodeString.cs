using System.Collections.Generic;
using System.Text;

namespace PowerTools
{
	public class PowerTextNodeString : IPowerTextNode
	{
		public enum ePluralType
		{
			None = 0,
			s = 1,
			es = 2,
			Custom = 3
		}

		private string m_string;

		private ePluralType m_pluralType;

		private string m_pluralString;

		public PowerTextNodeString(string str)
		{
			m_string = str;
		}

		public static implicit operator PowerTextNodeString(string str)
		{
			return new PowerTextNodeString(str);
		}

		public void SetPlural(ePluralType type, string customString = null)
		{
			m_pluralType = type;
			m_pluralString = customString;
		}

		public void Build(StringBuilder builder, ref bool plural)
		{
			if ((m_pluralType != ePluralType.None) & plural)
			{
				if (m_pluralType == ePluralType.Custom)
				{
					int num = m_string.LastIndexOf(' ');
					if (num > 0 && num < m_string.Length)
					{
						builder.Append(m_string.Substring(0, num));
					}
					builder.Append(m_pluralString);
				}
				else if (m_pluralType == ePluralType.s)
				{
					builder.Append(m_string);
					builder.Append('s');
				}
				else if (m_pluralType == ePluralType.es)
				{
					builder.Append(m_string);
					builder.Append("es");
				}
			}
			else
			{
				builder.Append(m_string);
			}
		}

		public void Build(List<string> parts, ref bool plural)
		{
			if (m_pluralType == ePluralType.None)
			{
				parts.Add(m_string);
				return;
			}
			int num = m_string.LastIndexOf(' ');
			string text = m_string;
			if (num > 0 && num < m_string.Length)
			{
				parts.Add(m_string.Substring(0, num));
				text = m_string.Substring(num);
			}
			if (!plural)
			{
				parts.Add(text);
			}
			else if (m_pluralType == ePluralType.Custom)
			{
				parts.Add(m_pluralString);
			}
			else if (m_pluralType == ePluralType.s)
			{
				parts.Add(text + "s");
			}
			else if (m_pluralType == ePluralType.es)
			{
				parts.Add(text + "es");
			}
		}

		public void GetParts(List<string> parts)
		{
			if (m_pluralType == ePluralType.None)
			{
				parts.Add(m_string);
				return;
			}
			int num = m_string.LastIndexOf(' ');
			string text = m_string;
			if (num > 0 && num < m_string.Length)
			{
				parts.Add(m_string.Substring(0, num));
				text = m_string.Substring(num);
			}
			parts.Add(text);
			if (m_pluralType == ePluralType.Custom)
			{
				parts.Add(m_pluralString);
			}
			else if (m_pluralType == ePluralType.s)
			{
				parts.Add(text + "s");
			}
			else if (m_pluralType == ePluralType.es)
			{
				parts.Add(text + "es");
			}
		}

		public float GetWeight()
		{
			return 1f;
		}
	}
}
