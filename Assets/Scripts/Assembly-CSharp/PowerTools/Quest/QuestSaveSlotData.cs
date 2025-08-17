using System;
using UnityEngine;

namespace PowerTools.Quest
{
	[Serializable]
	public class QuestSaveSlotData
	{
		public int m_slotId = -1;

		public int m_version = -1;

		public int m_timestamp = int.MinValue;

		public string m_description;

		public Texture2D m_image;
	}
}
