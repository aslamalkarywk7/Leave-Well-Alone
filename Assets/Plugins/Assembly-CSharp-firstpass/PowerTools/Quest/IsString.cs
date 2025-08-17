namespace PowerTools.Quest
{
	public static class IsString
	{
		public static bool Empty(string str)
		{
			return string.IsNullOrEmpty(str);
		}

		public static bool Set(string str)
		{
			return !string.IsNullOrEmpty(str);
		}

		public static bool NotEmpty(string str)
		{
			return !string.IsNullOrEmpty(str);
		}

		public static bool NonEmpty(string str)
		{
			return !string.IsNullOrEmpty(str);
		}

		public static bool Valid(string str)
		{
			return !string.IsNullOrEmpty(str);
		}

		public static bool There(string str)
		{
			return !string.IsNullOrEmpty(str);
		}

		public static bool Ok(string str)
		{
			return !string.IsNullOrEmpty(str);
		}

		public static bool EqualIgnoreCase(string first, string second)
		{
			if (first == null || second == null)
			{
				return first == second;
			}
			return first.EqualsIgnoreCase(second);
		}
	}
}
