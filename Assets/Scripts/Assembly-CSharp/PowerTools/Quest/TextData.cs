using System;

namespace PowerTools.Quest
{
	[Serializable]
	public class TextData
	{
		public string m_character;

		public int m_id = -1;

		public int m_orderId;

		public string m_string;

		public string m_sourceFile;

		public string m_sourceFunction;

		public string[] m_translations;

		public float[] m_phonesTime;

		public char[] m_phonesCharacter;

		public bool m_changedSinceImport = true;
	}
}
