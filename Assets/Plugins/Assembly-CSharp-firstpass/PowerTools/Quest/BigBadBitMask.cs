using System;

namespace PowerTools.Quest
{
	public class BigBadBitMask
	{
		public int[] m_masks = new int[1];

		public BigBadBitMask()
		{
		}

		public BigBadBitMask(int[] masks)
		{
			m_masks = masks;
		}

		public void Clear()
		{
			for (int i = 0; i < m_masks.Length; i++)
			{
				m_masks[i] = 0;
			}
		}

		public void SetAt(int index, bool value)
		{
			int maskIdFromIndex = GetMaskIdFromIndex(ref index);
			if (value)
			{
				m_masks[maskIdFromIndex] |= 1 << index;
			}
			else
			{
				m_masks[maskIdFromIndex] &= ~(1 << index);
			}
		}

		public bool GetAt(int index)
		{
			int maskIdFromIndex = GetMaskIdFromIndex(ref index);
			return (m_masks[maskIdFromIndex] & (1 << index)) != 0;
		}

		public override string ToString()
		{
			string text = string.Empty;
			for (int i = 0; i < m_masks.Length * 32; i++)
			{
				text += (GetAt(i) ? '0' : '1');
			}
			return text;
		}

		private int GetMaskIdFromIndex(ref int index)
		{
			int num = index / 32;
			index %= 32;
			while (num >= m_masks.Length)
			{
				Array.Resize(ref m_masks, num + 1);
			}
			return num;
		}
	}
}
