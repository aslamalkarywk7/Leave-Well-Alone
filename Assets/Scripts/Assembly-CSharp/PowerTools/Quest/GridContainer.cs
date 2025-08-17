using System.Collections.Generic;
using UnityEngine;

namespace PowerTools.Quest
{
	[ExecuteInEditMode]
	[AddComponentMenu("Quest Gui Layout/Grid")]
	public class GridContainer : MonoBehaviour
	{
		[Tooltip("If true, child objects of the container are automatically added to the container")]
		[SerializeField]
		private bool m_autoLayoutChildren;

		[SerializeField]
		private Vector2 m_itemSpacing = new Vector2(16f, 16f);

		[Tooltip("How many items are displayed on a row before wrapping to the next one. If zero, it never wraps")]
		[Min(0f)]
		[SerializeField]
		private int m_columnsPerRow;

		[Tooltip("Number of columns visible before scrolling is necessary. Only used when m_rowWidth is Zero")]
		[Min(0f)]
		[SerializeField]
		private int m_scrollColumns;

		[Tooltip("Number of rows visible before scrolling. If zero, it's treated as infinite rows")]
		[Min(0f)]
		[SerializeField]
		private int m_scrollRows;

		[Tooltip("How many pixels outside the size to continue drawing objects. Useful for smooth scrolling with a sprite-mask")]
		[SerializeField]
		private Vector2 m_overdraw = Vector2.zero;

		[SerializeField]
		private List<Transform> m_items = new List<Transform>();

		[SerializeField]
		[HideInInspector]
		private Vector2 m_size = Vector2.zero;

		private Vector2 m_offset = Vector2.zero;

		private int m_numChildrenCached = -1;

		public RectCentered FilledRect
		{
			get
			{
				Vector2 size = new Vector2((float)FilledColumns * m_itemSpacing.x, (float)FilledRows * m_itemSpacing.y);
				RectCentered result = default(RectCentered);
				result.Size = size;
				result.Center = (Vector2)base.transform.position + (result.Size - ItemSpacing).WithFlippedY() * 0.5f;
				return result;
			}
		}

		public RectCentered Rect
		{
			get
			{
				if (m_size == Vector2.zero)
				{
					SetSizeFromGrid();
				}
				RectCentered result = default(RectCentered);
				result.Size = m_size;
				result.Center = (Vector2)base.transform.position + (result.Size - ItemSpacing).WithFlippedY() * 0.5f;
				return result;
			}
			set
			{
				int num = Mathf.Max(1, Mathf.FloorToInt(value.Height / ItemSpacing.y));
				int num2 = Mathf.Max(1, Mathf.FloorToInt(value.Width / ItemSpacing.x));
				if (num > 1)
				{
					ColumnsPerRow = num2;
					ScrollColumns = 0;
				}
				else
				{
					ColumnsPerRow = 0;
					ScrollColumns = num2;
				}
				ScrollRows = num;
				base.transform.position = (value.Center - (value.Size - ItemSpacing).WithFlippedY() * 0.5f).WithZ(base.transform.position.z);
				m_size = value.Size;
				DoLayout();
			}
		}

		public Vector2 ItemSpacing
		{
			get
			{
				return m_itemSpacing;
			}
			set
			{
				Vector2 vector = new Vector2(Mathf.Max(1f, value.x), Mathf.Max(1f, value.y));
				if (vector != m_itemSpacing)
				{
					m_itemSpacing = vector;
					SetSizeFromGrid();
					DoLayout();
				}
			}
		}

		public List<Transform> Items => m_items;

		public int ColumnsPerRow
		{
			get
			{
				return m_columnsPerRow;
			}
			set
			{
				m_columnsPerRow = Mathf.Max(0, value);
				DoLayout();
			}
		}

		public int ScrollColumns
		{
			get
			{
				return m_scrollColumns;
			}
			set
			{
				m_scrollColumns = Mathf.Max(0, value);
				DoLayout();
			}
		}

		public int ScrollRows
		{
			get
			{
				return m_scrollRows;
			}
			set
			{
				m_scrollRows = Mathf.Max(0, value);
				DoLayout();
			}
		}

		public int FilledColumns => Constrain(ItemCount, m_columnsPerRow, m_scrollColumns);

		public int FilledRows => Constrain(LastFilledRow, HeightInRows);

		public int LastFilledColumn => Constrain(ItemCount, m_columnsPerRow);

		public int LastFilledRow => IndexToRow(ItemCount - 1) + 1;

		public int WidthInColumns
		{
			get
			{
				if (m_scrollColumns > 0 && (m_columnsPerRow <= 0 || m_columnsPerRow > m_scrollColumns))
				{
					return m_scrollColumns;
				}
				if (m_columnsPerRow > 0)
				{
					return m_columnsPerRow;
				}
				return 0;
			}
		}

		public int HeightInRows => m_scrollRows;

		public Vector2 FilledSize => new Vector2((float)FilledColumns * m_itemSpacing.x, (float)FilledRows * m_itemSpacing.y);

		public Vector2 ScrollOffset
		{
			get
			{
				return m_offset;
			}
			set
			{
				if (value != m_offset)
				{
					m_offset = value;
					DoLayout();
				}
			}
		}

		public int RowOffset
		{
			get
			{
				return -Mathf.RoundToInt(m_offset.y / m_itemSpacing.y);
			}
			set
			{
				ScrollOffset = ScrollOffset.WithY((float)(-value) * m_itemSpacing.y);
			}
		}

		public int ColumnOffset
		{
			get
			{
				return Mathf.RoundToInt(m_offset.x / m_itemSpacing.x);
			}
			set
			{
				ScrollOffset = ScrollOffset.WithX((float)value * m_itemSpacing.x);
			}
		}

		private int ItemCount => m_items.Count;

		public void SetSizeFromGrid()
		{
			m_size = new Vector2((float)WidthInColumns * m_itemSpacing.x, (float)HeightInRows * m_itemSpacing.y);
		}

		public void AddItem(Transform item)
		{
			m_items.Add(item);
			DoLayout();
		}

		public void RemoveItem(Transform item)
		{
			m_items.Remove(item);
			DoLayout();
		}

		public void RemoveAt(int index)
		{
			m_items.RemoveAt(index);
			DoLayout();
		}

		public bool GetItemVisible(int index)
		{
			Vector2 itemPos = GetItemPos(index);
			Vector2 filledSize = FilledSize;
			itemPos -= ScrollOffset;
			bool flag = itemPos.x >= 0f - m_overdraw.x && itemPos.y <= m_overdraw.y;
			if (filledSize.x > 0f)
			{
				flag &= itemPos.x < filledSize.x + m_overdraw.x;
			}
			if (filledSize.y > 0f)
			{
				flag &= itemPos.y > 0f - (filledSize.y + m_overdraw.y);
			}
			return flag;
		}

		public Vector2 GetItemPos(int index)
		{
			index = ItemIndex(index);
			Vector2 zero = Vector2.zero;
			zero.x = (float)IndexToColumn(index) * m_itemSpacing.x;
			zero.y = (float)(-IndexToRow(index)) * m_itemSpacing.y;
			return zero;
		}

		public int IndexToColumn(int index)
		{
			if (index > 0)
			{
				if (m_columnsPerRow > 0)
				{
					return index % m_columnsPerRow;
				}
				return index;
			}
			return 0;
		}

		public int IndexToRow(int index)
		{
			if (m_columnsPerRow > 0 && index > 0)
			{
				return index / m_columnsPerRow;
			}
			return 0;
		}

		public void NextRow()
		{
			ScrollOffset -= new Vector2(0f, m_itemSpacing.y);
		}

		public void NextColumn()
		{
			ScrollOffset += new Vector2(m_itemSpacing.x, 0f);
		}

		public void PrevRow()
		{
			ScrollOffset += new Vector2(0f, m_itemSpacing.y);
		}

		public void PrevColumn()
		{
			ScrollOffset -= new Vector2(m_itemSpacing.x, 0f);
		}

		public bool HasNextColumn()
		{
			if (WidthInColumns > 0)
			{
				return ColumnOffset + WidthInColumns < LastFilledColumn;
			}
			return false;
		}

		public bool HasPrevColumn()
		{
			return ColumnOffset > 0;
		}

		public bool HasNextRow()
		{
			if (m_columnsPerRow > 0 && HeightInRows > 0)
			{
				return RowOffset + HeightInRows < LastFilledRow;
			}
			return false;
		}

		public bool HasPrevRow()
		{
			return RowOffset > 0;
		}

		public void ForceUpdate()
		{
			DoLayout();
		}

		private void Start()
		{
			DoLayout();
		}

		private void LateUpdate()
		{
			DoLayout();
		}

		private int ItemIndex(int itemId)
		{
			return itemId;
		}

		private void DoLayout()
		{
			UpdateLayoutChildren();
			for (int i = 0; i < m_items.Count; i++)
			{
				Vector2 vector = GetItemPos(i) - ScrollOffset;
				Transform transform = m_items[i];
				if (m_items[i] == null)
				{
					break;
				}
				if (GetItemVisible(i))
				{
					transform.gameObject.SetActive(value: true);
					m_items[i].position = (base.transform.position + (Vector3)vector).WithZ(m_items[i].position.z);
				}
				else
				{
					transform.gameObject.SetActive(value: false);
				}
			}
		}

		private void UpdateLayoutChildren()
		{
			if (!m_autoLayoutChildren || base.transform.childCount == m_numChildrenCached)
			{
				return;
			}
			int childCount = base.transform.childCount;
			while (m_items.Count > childCount)
			{
				m_items.RemoveAt(m_items.Count - 1);
			}
			for (int i = 0; i < childCount; i++)
			{
				if (i < m_items.Count)
				{
					m_items[i] = base.transform.GetChild(i);
				}
				else
				{
					m_items.Add(base.transform.GetChild(i));
				}
			}
			m_numChildrenCached = childCount;
		}

		private int Constrain(int value, int constraint1 = 0, int constraint2 = 0)
		{
			if (constraint2 > 0)
			{
				value = Constrain(value, constraint2);
			}
			if (constraint1 > 0 && constraint1 < value)
			{
				return constraint1;
			}
			if (value <= 0)
			{
				return 0;
			}
			return value;
		}
	}
}
