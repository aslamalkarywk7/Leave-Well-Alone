using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PowerTools
{
	public class PowerText
	{
		private static readonly Regex s_regexGroup = new Regex("^:(?<group> \\S+?):", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

		private static readonly Regex s_regexLine = new Regex("(^//.*) |(^\\[(?<weight>\\d*\\.?\\d+)\\]) | (?<empty>\\[\\s*\\]) | (?<ps> \\[\\#\\]) | (?<pe> \\[\\\\\\#\\]) | (?<s>\\[s\\]) | (?<es>\\[es\\]) | (\\[/(?<plr> \\w+)\\]) | (\\[(?<ref> \\S+?)\\]) | (?<text> [^\\[\\r]+)", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

		private static readonly Regex s_regexCapital = new Regex("((?<=^\\W*)\\w) | ((?<=[.:!?]\\s*)\\w)", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

		private static readonly Regex s_regexAn = new Regex("(?<!\\S)a(?=\\s+[aeiou])", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

		private static readonly char[] LINEDELIM = new char[1] { '\n' };

		private static readonly string MATCH_GROUP = "group";

		private static readonly string MATCH_EMPTY = "empty";

		private static readonly string MATCH_WEIGHT = "weight";

		private static readonly string MATCH_REF = "ref";

		private static readonly string MATCH_TEXT = "text";

		private static readonly string MATCH_PLURALISE_START = "ps";

		private static readonly string MATCH_PLURALISE_END = "pe";

		private static readonly string MATCH_PLURAL_S = "s";

		private static readonly string MATCH_PLURAL_ES = "es";

		private static readonly string MATCH_PLURAL = "plr";

		private Dictionary<string, PowerTextNodeGroup> m_groups = new Dictionary<string, PowerTextNodeGroup>();

		public string GetString(string groupName)
		{
			string result = string.Empty;
			PowerTextNodeGroup value = null;
			if (m_groups.TryGetValue(groupName.ToUpper(), out value))
			{
				bool plural = false;
				StringBuilder stringBuilder = new StringBuilder();
				value.Build(stringBuilder, ref plural);
				result = stringBuilder.ToString();
				result = PostProcess(result);
			}
			return result;
		}

		public List<string> GetStringList(string groupName)
		{
			List<string> list = new List<string>();
			PowerTextNodeGroup value = null;
			if (m_groups.TryGetValue(groupName.ToUpper(), out value))
			{
				bool plural = false;
				value.Build(list, ref plural);
			}
			return list;
		}

		public string[] GetAllParts()
		{
			HashSet<string> hashSet = new HashSet<string>();
			List<string> list = new List<string>(256);
			foreach (PowerTextNodeGroup value in m_groups.Values)
			{
				if (value == null || value.m_options == null)
				{
					continue;
				}
				foreach (IPowerTextNode option in value.m_options)
				{
					option.GetParts(list);
				}
			}
			foreach (string item in list)
			{
				hashSet.Add(item.ToLower());
			}
			string[] array = new string[hashSet.Count];
			hashSet.CopyTo(array);
			return array;
		}

		public void Parse(string text)
		{
			string[] array = text.Split(LINEDELIM, 10000);
			PowerTextNodeGroup powerTextNodeGroup = null;
			foreach (string text2 in array)
			{
				Match match = s_regexGroup.Match(text2);
				if (match.Success)
				{
					string value = match.Groups[MATCH_GROUP].Value;
					powerTextNodeGroup = FindOrCreateGroup(value);
				}
				else if (powerTextNodeGroup != null)
				{
					ParseLine(powerTextNodeGroup, text2);
				}
			}
		}

		private PowerTextNodeLine ParseLine(PowerTextNodeGroup currentGroup, string lineText)
		{
			PowerTextNodeLine powerTextNodeLine = new PowerTextNodeLine();
			bool flag = false;
			PowerTextNodeString powerTextNodeString = null;
			foreach (Match item in s_regexLine.Matches(lineText))
			{
				if (item.Success && item.Groups == null)
				{
					continue;
				}
				if (item.Groups[MATCH_EMPTY].Success)
				{
					flag = true;
					powerTextNodeLine.Append((PowerTextNodeString)string.Empty);
				}
				if (item.Groups[MATCH_WEIGHT].Success)
				{
					flag = true;
					float.TryParse(item.Groups[MATCH_WEIGHT].Value, out powerTextNodeLine.m_weight);
				}
				if (item.Groups[MATCH_REF].Success)
				{
					flag = true;
					powerTextNodeLine.Append(FindOrCreateGroup(item.Groups[MATCH_REF].Value));
				}
				if (item.Groups[MATCH_TEXT].Success)
				{
					string value = item.Groups[MATCH_TEXT].Value;
					if (!string.IsNullOrEmpty(value))
					{
						flag = true;
						powerTextNodeString = value;
						powerTextNodeLine.Append(powerTextNodeString);
					}
				}
				if (item.Groups[MATCH_PLURALISE_START].Success)
				{
					flag = true;
					powerTextNodeLine.Append(new PowerTextNodePluralFlag(pluralize: true));
				}
				if (item.Groups[MATCH_PLURALISE_END].Success)
				{
					powerTextNodeLine.Append(new PowerTextNodePluralFlag(pluralize: false));
				}
				if (powerTextNodeString == null)
				{
					continue;
				}
				if (item.Groups[MATCH_PLURAL_S].Success)
				{
					powerTextNodeString.SetPlural(PowerTextNodeString.ePluralType.s);
				}
				if (item.Groups[MATCH_PLURAL_ES].Success)
				{
					powerTextNodeString.SetPlural(PowerTextNodeString.ePluralType.es);
				}
				if (item.Groups[MATCH_PLURAL].Success)
				{
					string value2 = item.Groups[MATCH_PLURAL].Value;
					if (!string.IsNullOrEmpty(value2))
					{
						powerTextNodeString.SetPlural(PowerTextNodeString.ePluralType.Custom, value2);
					}
				}
			}
			if (flag)
			{
				currentGroup.AddOption(powerTextNodeLine);
			}
			return powerTextNodeLine;
		}

		public void SetVariable(string name, string value)
		{
			PowerTextNodeGroup powerTextNodeGroup = FindOrCreateGroup(name);
			if (powerTextNodeGroup.m_options != null)
			{
				powerTextNodeGroup.m_options.Clear();
			}
			ParseLine(powerTextNodeGroup, value);
		}

		public void SetVariable(string name, int value)
		{
			if (value != 1)
			{
				SetVariable(name, value + "[#]");
			}
			else
			{
				SetVariable(name, value.ToString());
			}
		}

		private PowerTextNodeGroup FindOrCreateGroup(string name)
		{
			PowerTextNodeGroup value = null;
			name = name.ToUpper();
			if (m_groups.TryGetValue(name, out value))
			{
				return value;
			}
			value = new PowerTextNodeGroup();
			m_groups.Add(name, value);
			return value;
		}

		private string PostProcess(string text)
		{
			text = s_regexCapital.Replace(text, ReplaceCapital);
			return s_regexAn.Replace(text, ReplaceAn);
		}

		private string ReplaceAn(Match match)
		{
			return match.ToString() + 'n';
		}

		private string ReplaceCapital(Match match)
		{
			return match.ToString().ToUpper();
		}
	}
}
