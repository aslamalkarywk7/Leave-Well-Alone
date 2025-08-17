using System;
using System.Collections.Generic;
using PowerTools.Quest;
using UnityEngine;

namespace PowerTools.QuestGui
{
	[Serializable]
	[AddComponentMenu("Quest Gui/InventoryPanel")]
	public class InventoryPanel : GuiControl, IInventoryPanel, IGuiControl
	{
		[Tooltip("Name of character to show inventory of. If empty will use the current player")]
		[SerializeField]
		private string m_targetCharacter;

		[SerializeField]
		private bool m_reverseOrder;

		[Tooltip("Sets the cursor to show if hovering over the item")]
		[SerializeField]
		private string m_itemCursor;

		[SerializeField]
		private InventoryPanelItem m_itemPrefab;

		[SerializeField]
		private Button m_buttonScrollBack;

		[SerializeField]
		private Button m_buttonScrollForward;

		[SerializeField]
		private SpriteMask m_mask;

		private GridContainer m_grid;

		private Character.CollectedItem m_lastCollectedItem;

		private Vector2 m_itemOffset = Vector2.zero;

		public ICharacter TargetCharacter
		{
			get
			{
				if (!string.IsNullOrEmpty(m_targetCharacter))
				{
					return Singleton<PowerQuest>.Get.GetCharacter(m_targetCharacter);
				}
				return Singleton<PowerQuest>.Get.GetPlayer();
			}
			set
			{
				m_targetCharacter = value.ScriptName;
			}
		}

		public IQuestClickable IClickable => this;

		public Vector2 ScrollOffset
		{
			get
			{
				return m_grid.ScrollOffset;
			}
			set
			{
				m_grid.ScrollOffset = value;
			}
		}

		public override RectCentered CustomSize
		{
			get
			{
				GridContainer component = GetComponent<GridContainer>();
				if (component == null)
				{
					return RectCentered.zero;
				}
				return component.Rect;
			}
			set
			{
				GridContainer component = GetComponent<GridContainer>();
				if (!(component == null))
				{
					component.Rect = value;
				}
			}
		}

		public void NextRow()
		{
			m_grid.NextRow();
		}

		public void NextColumn()
		{
			m_grid.NextColumn();
		}

		public void PrevRow()
		{
			m_grid.PrevRow();
		}

		public void PrevColumn()
		{
			m_grid.PrevColumn();
		}

		public bool HasNextColumn()
		{
			return m_grid.HasNextColumn();
		}

		public bool HasPrevColumn()
		{
			return m_grid.HasPrevColumn();
		}

		public bool HasNextRow()
		{
			return m_grid.HasNextRow();
		}

		public bool HasPrevRow()
		{
			return m_grid.HasPrevRow();
		}

		public bool ScrollForward()
		{
			if (HasNextColumn())
			{
				NextColumn();
				return true;
			}
			if (HasNextRow())
			{
				NextRow();
				return true;
			}
			return false;
		}

		public bool ScrollBack()
		{
			if (HasPrevColumn())
			{
				PrevColumn();
				return true;
			}
			if (HasPrevRow())
			{
				PrevRow();
				return true;
			}
			return false;
		}

		public override RectCentered GetRect(Transform excludeChild = null)
		{
			return CustomSize;
		}

		private void Awake()
		{
			m_grid = GetComponentInChildren<GridContainer>();
			if (m_buttonScrollBack != null)
			{
				Button buttonScrollBack = m_buttonScrollBack;
				buttonScrollBack.OnClick = (Action<GuiControl>)Delegate.Combine(buttonScrollBack.OnClick, new Action<GuiControl>(OnBackButton));
			}
			if (m_buttonScrollForward != null)
			{
				Button buttonScrollForward = m_buttonScrollForward;
				buttonScrollForward.OnClick = (Action<GuiControl>)Delegate.Combine(buttonScrollForward.OnClick, new Action<GuiControl>(OnForwardButton));
			}
		}

		private void OnDestroy()
		{
			if (m_buttonScrollBack != null)
			{
				Button buttonScrollBack = m_buttonScrollBack;
				buttonScrollBack.OnClick = (Action<GuiControl>)Delegate.Remove(buttonScrollBack.OnClick, new Action<GuiControl>(OnBackButton));
			}
			if (m_buttonScrollForward != null)
			{
				Button buttonScrollForward = m_buttonScrollForward;
				buttonScrollForward.OnClick = (Action<GuiControl>)Delegate.Remove(buttonScrollForward.OnClick, new Action<GuiControl>(OnForwardButton));
			}
		}

		private bool IsMouseOverItem()
		{
			if (Singleton<PowerQuest>.Get.GetMouseOverType() == eQuestClickableType.Inventory && Singleton<PowerQuest>.Get.GetFocusedGui() == m_gui)
			{
				for (int i = 0; i < m_grid.Items.Count; i++)
				{
					if (m_grid.GetItemVisible(i) && m_grid.Items[i] == Singleton<PowerQuest>.Get.GetFocusedGuiControl().Instance.transform)
					{
						return true;
					}
				}
			}
			return false;
		}

		private Transform GetMouseOverItem()
		{
			if (Singleton<PowerQuest>.Get.GetMouseOverType() == eQuestClickableType.Inventory && Singleton<PowerQuest>.Get.GetFocusedGuiControl() != null)
			{
				for (int i = 0; i < m_grid.Items.Count; i++)
				{
					if (m_grid.GetItemVisible(i))
					{
						Transform transform = m_grid.Items[i];
						if (transform == Singleton<PowerQuest>.Get.GetFocusedGuiControl().Instance.transform)
						{
							return transform;
						}
					}
				}
			}
			return null;
		}

		private void Update()
		{
			UpdateItems();
			List<Character.CollectedItem> inventory = (TargetCharacter as Character).GetInventory();
			if (inventory.Count > 0 && inventory[inventory.Count - 1] != m_lastCollectedItem)
			{
				if (m_reverseOrder)
				{
					m_grid.ScrollOffset = Vector2.zero;
				}
				else
				{
					while (m_grid.HasNextColumn())
					{
						m_grid.NextColumn();
					}
					while (m_grid.HasNextRow())
					{
						m_grid.NextRow();
					}
				}
				m_lastCollectedItem = inventory[inventory.Count - 1];
			}
			if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && (bool)GetMouseOverItem() && Singleton<PowerQuest>.Get.OnInventoryClick())
			{
				GetComponentInParent<GuiDropDownBar>()?.Hide();
			}
			if ((bool)m_mask)
			{
				int num = -Mathf.RoundToInt(base.GuiData.Baseline * 100f + Baseline);
				int num2 = SortingLayer.NameToID("Gui");
				m_mask.backSortingLayerID = num2;
				m_mask.frontSortingLayerID = num2;
				m_mask.frontSortingOrder = num;
				m_mask.backSortingOrder = num - 1;
				m_mask.transform.position = m_grid.Rect.Center.WithZ(0f);
				m_mask.transform.localScale = m_grid.Rect.Size.WithZ(1f);
			}
		}

		private GameObject CreateButton()
		{
			GameObject gameObject = null;
			if (m_itemPrefab != null)
			{
				gameObject = UnityEngine.Object.Instantiate(m_itemPrefab.gameObject);
			}
			else
			{
				gameObject = new GameObject("InvItem", typeof(InventoryPanelItem), typeof(GuiControl), typeof(SpriteAnim), typeof(PowerSprite));
				gameObject.layer = LayerMask.NameToLayer("UI");
				BoxCollider2D boxCollider2D = gameObject.AddComponent<BoxCollider2D>();
				boxCollider2D.size = m_grid.ItemSpacing.WithOffset(-2f, -2f);
				boxCollider2D.isTrigger = true;
				gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Gui";
			}
			gameObject.AddComponent<InventoryComponent>();
			gameObject.transform.SetParent(m_grid.transform, worldPositionStays: false);
			GuiControl component = gameObject.GetComponent<GuiControl>();
			if (component == null)
			{
				gameObject.AddComponent<GuiControl>();
			}
			component.Baseline = Baseline;
			component.SetGui(base.GuiData);
			if (m_mask != null)
			{
				SpriteRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<SpriteRenderer>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
				}
			}
			return gameObject;
		}

		private void UpdateItems()
		{
			List<Character.CollectedItem> inventory = (TargetCharacter as Character).GetInventory();
			while (m_grid.Items.Count < inventory.Count)
			{
				GameObject gameObject = CreateButton();
				m_grid.AddItem(gameObject.transform);
			}
			while (m_grid.Items.Count > inventory.Count)
			{
				Transform transform = m_grid.Items[m_grid.Items.Count - 1];
				m_grid.RemoveItem(transform);
				if (transform != null)
				{
					UnityEngine.Object.Destroy(transform.gameObject);
				}
			}
			for (int i = 0; i < m_grid.Items.Count; i++)
			{
				if (!m_grid.GetItemVisible(i))
				{
					continue;
				}
				Inventory inventory2 = Singleton<PowerQuest>.Get.GetInventory(m_reverseOrder ? inventory[inventory.Count - 1 - i].m_name : inventory[i].m_name);
				m_grid.Items[i].GetComponent<InventoryComponent>().SetData(inventory2);
				if (IsString.Set(m_itemCursor))
				{
					inventory2.Cursor = m_itemCursor;
				}
				InventoryPanelItem component = m_grid.Items[i].GetComponent<InventoryPanelItem>();
				if (component.GetCachedAnimSpriteName() != inventory2.AnimGui)
				{
					AnimationClip animation = base.GuiComponent.GetAnimation(inventory2.AnimGui);
					if (animation != null)
					{
						component.SetInventoryAnim(animation);
					}
					else if (animation == null)
					{
						component.SetInventorySprite(base.GuiComponent.GetSprite(inventory2.AnimGui));
					}
				}
			}
			if (m_buttonScrollForward != null)
			{
				if (m_grid.HasNextColumn() || m_grid.HasNextRow())
				{
					m_buttonScrollForward.Show();
				}
				else
				{
					m_buttonScrollForward.Hide();
				}
			}
			if (m_buttonScrollBack != null)
			{
				if (m_grid.HasPrevColumn() || m_grid.HasPrevRow())
				{
					m_buttonScrollBack.Show();
				}
				else
				{
					m_buttonScrollBack.Hide();
				}
			}
		}

		private void OnForwardButton(GuiControl control)
		{
			ScrollForward();
		}

		private void OnBackButton(GuiControl control)
		{
			ScrollBack();
		}
	}
}
