using System;
using UnityEngine;

namespace PowerTools.Quest
{
	[Serializable]
	public class QuestCursor : ICursor
	{
		public enum eInventoryOutlineOnGui
		{
			Never = 0,
			OtherItemsOnly = 1,
			Always = 2
		}

		[SerializeField]
		private bool m_visible = true;

		[SerializeField]
		private string m_animationClickable = "Active";

		[SerializeField]
		private string m_animationNonClickable = "Idle";

		[SerializeField]
		private string m_animationUseInv = "UseInv";

		[SerializeField]
		private string m_animationOverGui = "Idle";

		[SerializeField]
		private string m_animationWait = "Wait";

		[SerializeField]
		private Color m_inventoryOutlineColor = new Color(1f, 1f, 1f, 0f);

		[Tooltip("Controls when the inventory outline shows when hovered over other inventory items")]
		[SerializeField]
		private eInventoryOutlineOnGui m_inventoryOutlineOnGui = eInventoryOutlineOnGui.OtherItemsOnly;

		[Tooltip("If true, the cursor will be hidden when the game isn't interactive")]
		[SerializeField]
		private bool m_hideWhenBlocking = true;

		private GameObject m_prefab;

		private QuestCursorComponent m_instance;

		private string m_animationOverride;

		public MonoBehaviour Instance => m_instance;

		public bool Visible
		{
			get
			{
				return m_visible;
			}
			set
			{
				m_visible = value;
				if ((bool)m_instance)
				{
					m_instance.SetVisible(m_visible && (!m_hideWhenBlocking || !Singleton<PowerQuest>.Get.GetBlocked()));
				}
			}
		}

		public string AnimationOverride
		{
			get
			{
				return m_animationOverride;
			}
			set
			{
				m_animationOverride = value;
				OnChangeAnimation();
			}
		}

		public string AnimationClickable
		{
			get
			{
				return m_animationClickable;
			}
			set
			{
				m_animationClickable = value;
				OnChangeAnimation();
			}
		}

		public string AnimationNonClickable
		{
			get
			{
				return m_animationNonClickable;
			}
			set
			{
				m_animationNonClickable = value;
				OnChangeAnimation();
			}
		}

		public string AnimationUseInv
		{
			get
			{
				return m_animationUseInv;
			}
			set
			{
				m_animationUseInv = value;
				OnChangeAnimation();
			}
		}

		public string AnimationOverGui
		{
			get
			{
				return m_animationOverGui;
			}
			set
			{
				m_animationOverGui = value;
				OnChangeAnimation();
			}
		}

		public string AnimationWait
		{
			get
			{
				return m_animationWait;
			}
			set
			{
				m_animationWait = value;
				OnChangeAnimation();
			}
		}

		public bool HideWhenBlocking
		{
			get
			{
				return m_hideWhenBlocking;
			}
			set
			{
				m_hideWhenBlocking = value;
				OnChangeAnimation();
			}
		}

		public Color InventoryOutlineColor
		{
			get
			{
				return m_inventoryOutlineColor;
			}
			set
			{
				m_inventoryOutlineColor = value;
				OnChangeAnimation();
			}
		}

		public eInventoryOutlineOnGui InventoryOutlineOnGui
		{
			get
			{
				return m_inventoryOutlineOnGui;
			}
			set
			{
				m_inventoryOutlineOnGui = value;
				OnChangeAnimation();
			}
		}

		public bool NoneCursorActive => m_instance.GetNoneCursorActive();

		public bool InventoryCursorOverridden => m_instance.GetInventoryCursorOverridden();

		public Vector2 PositionOverride
		{
			get
			{
				return Singleton<PowerQuest>.Get.GetMousePosition();
			}
			set
			{
				Singleton<PowerQuest>.Get.SetMousePositionOverride(value);
			}
		}

		public bool HasPositionOverride => Singleton<PowerQuest>.Get.GetHasMousePositionOverride();

		public void PlayAnimation(string animation)
		{
			if (m_instance != null)
			{
				m_instance.PlayAnimation(animation);
			}
		}

		public void StopAnimation()
		{
			if (m_instance != null)
			{
				m_instance.StopAnimation();
			}
		}

		public void ResetAnimationOverride()
		{
			m_animationOverride = null;
			OnChangeAnimation();
		}

		public GameObject GetPrefab()
		{
			return m_prefab;
		}

		public QuestCursorComponent GetInstance()
		{
			return m_instance;
		}

		public void SetInstance(QuestCursorComponent instance)
		{
			m_instance = instance;
			m_instance.SetData(this);
		}

		public void SetPositionOverride(Vector2 position)
		{
			Singleton<PowerQuest>.Get.SetMousePositionOverride(position);
		}

		public void ClearPositionOverride()
		{
			Singleton<PowerQuest>.Get.ResetMousePositionOverride();
		}

		public void EditorInitialise(string name)
		{
		}

		public void EditorRename(string name)
		{
		}

		public void OnPostRestore(int version, GameObject prefab)
		{
			m_prefab = prefab;
		}

		public void Initialise(GameObject prefab)
		{
			m_prefab = prefab;
		}

		private void OnChangeAnimation()
		{
			if ((bool)m_instance)
			{
				m_instance.OnChangeAnimation();
			}
		}
	}
}
