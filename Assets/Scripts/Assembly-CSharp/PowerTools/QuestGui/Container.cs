using System;
using PowerTools.Quest;
using UnityEngine;

namespace PowerTools.QuestGui
{
	[Serializable]
	[AddComponentMenu("Quest Gui/Container")]
	public class Container : GuiControl, IContainer, IGuiControl
	{
		private GridContainer m_grid;

		[Tooltip("If true, the container's size (for Fit To/Align To purposes) expands as items are added/removed from the grid")]
		[SerializeField]
		private bool m_dynamicGridSize;

		public GridContainer Grid => m_grid;

		private void InitComponents()
		{
			if (m_grid == null)
			{
				m_grid = GetComponent<GridContainer>();
			}
		}

		private void Awake()
		{
			InitComponents();
		}

		private void Start()
		{
		}

		private void Update()
		{
		}

		public override void UpdateBaseline()
		{
		}

		public override RectCentered GetRect(Transform exclude = null)
		{
			if (Application.isEditor)
			{
				InitComponents();
			}
			if (m_grid != null)
			{
				if (m_dynamicGridSize)
				{
					return m_grid.FilledRect;
				}
				return m_grid.Rect;
			}
			return base.GetRect(exclude);
		}

		public bool GetIsControlInGrid(IGuiControl control)
		{
			if (Grid == null || Grid.Items == null || Grid.Items.Count == 0 || control == null || control.Instance == null)
			{
				return false;
			}
			return Grid.Items.Exists((Transform item) => item == control.Instance.transform);
		}

		public IGuiControl GetNextControlUp(IGuiControl current)
		{
			return GetNextControl(current, eGuiNav.Up);
		}

		public IGuiControl GetNextControlDown(IGuiControl current)
		{
			return GetNextControl(current, eGuiNav.Down);
		}

		public IGuiControl GetNextControlLeft(IGuiControl current)
		{
			return GetNextControl(current, eGuiNav.Left);
		}

		public IGuiControl GetNextControlRight(IGuiControl current)
		{
			return GetNextControl(current, eGuiNav.Right);
		}

		public IGuiControl GetNextControl(IGuiControl current, eGuiNav dir)
		{
			if (Grid == null || Grid.Items == null || Grid.Items.Count == 0)
			{
				return null;
			}
			int num = 0;
			if (current != null && current.Instance != null)
			{
				num = Grid.Items.FindIndex((Transform item) => item == current.Instance.transform);
			}
			IGuiControl guiControl = null;
			while (guiControl == null)
			{
				switch (dir)
				{
				case eGuiNav.Right:
					num++;
					if (num % Grid.ColumnsPerRow == 0 || num >= Grid.Items.Count)
					{
						return null;
					}
					break;
				case eGuiNav.Left:
					if (num % Grid.ColumnsPerRow == 0 || num <= 0)
					{
						return null;
					}
					num--;
					break;
				case eGuiNav.Up:
					if (num / Grid.ColumnsPerRow <= 0)
					{
						return null;
					}
					num -= Grid.ColumnsPerRow;
					break;
				case eGuiNav.Down:
					num += Grid.ColumnsPerRow;
					if (num >= Grid.Items.Count)
					{
						return null;
					}
					break;
				}
				guiControl = Grid.Items[num].GetComponent<IGuiControl>();
				if (guiControl == null || !(guiControl as IQuestClickable).Clickable)
				{
					guiControl = null;
				}
			}
			return guiControl;
		}
	}
}
