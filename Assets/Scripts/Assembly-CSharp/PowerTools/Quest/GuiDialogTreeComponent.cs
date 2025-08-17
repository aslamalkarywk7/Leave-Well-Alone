using System;
using System.Collections.Generic;
using PowerTools.QuestGui;
using UnityEngine;

namespace PowerTools.Quest
{
	public class GuiDialogTreeComponent : MonoBehaviour
	{
		private enum eAlignVertical
		{
			Bottom = 0,
			Top = 1,
			Middle = 2
		}

		[SerializeField]
		[Tooltip("The padding between each option")]
		private float m_itemSpacing = 2f;

		[SerializeField]
		private Color m_colorDefault = Color.white;

		[SerializeField]
		private Color m_colorUsed = Color.white;

		[SerializeField]
		private Color m_colorHover = Color.white;

		[Tooltip("Whether any images nested under the option should be coloured along with the option text")]
		[SerializeField]
		private bool m_changeImageColor = true;

		[Tooltip("If active, long lines will be wrapped (and stretch the interface vertically)")]
		[SerializeField]
		private bool m_wrapText = true;

		[SerializeField]
		private bool m_showOptionNumbers;

		[SerializeField]
		private eAlignVertical m_verticalAlignment;

		[SerializeField]
		[Tooltip("Max number of items to show in the list (0 for no limits)")]
		private int m_maxVisibleItems;

		[SerializeField]
		[Tooltip("How much to offset text when back/forward buttons are OFF.")]
		private float m_arrowButtonWidth;

		[Header("Internal references")]
		[SerializeField]
		private GameObject m_textInstance;

		[SerializeField]
		private FitToObject m_background;

		[SerializeField]
		private GameObject m_btnScrollBack;

		[SerializeField]
		private GameObject m_btnScrollForward;

		private int m_itemsOffset;

		private int m_maxNumOptions;

		private float m_textContainerOffsetX;

		private List<QuestText> m_items = new List<QuestText>();

		private float m_defaultItemHeight;

		private void Start()
		{
			if (m_btnScrollBack != null && m_btnScrollBack.GetComponent<RectTransform>() != null)
			{
				Transform parent = base.transform.parent.Find("QuestCanvas");
				base.transform.SetParent(parent, worldPositionStays: true);
			}
		}

		private void Awake()
		{
			m_textContainerOffsetX = m_textInstance.transform.parent.localPosition.x;
		}

		private void OnEnable()
		{
			if (m_textInstance.activeSelf)
			{
				Renderer component = m_textInstance.gameObject.GetComponent<Renderer>();
				if (component != null)
				{
					m_defaultItemHeight = component.bounds.size.y;
				}
				m_textInstance.SetActive(value: false);
			}
			if ((bool)m_btnScrollForward)
			{
				m_btnScrollForward.gameObject.SetActive(value: false);
			}
			if ((bool)m_btnScrollForward)
			{
				m_btnScrollBack.gameObject.SetActive(value: false);
			}
			m_textInstance.GetComponent<QuestText>().Truncate = !m_wrapText;
			UpdateItems();
		}

		private void Update()
		{
			UpdateItems();
		}

		private void UpdateItems()
		{
			DialogTree currentDialog = Singleton<PowerQuest>.Get.GetCurrentDialog();
			if (currentDialog == null)
			{
				return;
			}
			Transform parent = m_textInstance.transform.parent;
			List<DialogOption> list = currentDialog.Options.FindAll((DialogOption item) => item.Visible);
			float num = 1f;
			_ = m_maxVisibleItems;
			_ = m_maxVisibleItems;
			if (list.Count != m_maxNumOptions)
			{
				m_maxNumOptions = list.Count;
				m_itemsOffset = 0;
			}
			int num2 = m_maxNumOptions;
			if (m_maxVisibleItems > 0)
			{
				int num3 = Math.Min(m_maxVisibleItems, m_maxNumOptions);
				list = list.GetRange(m_itemsOffset, num3);
				num2 = list.Count;
				m_btnScrollBack.gameObject.SetActive(m_itemsOffset > 0);
				m_btnScrollForward.gameObject.SetActive(m_itemsOffset + num3 != m_maxNumOptions);
			}
			while (m_items.Count < num2)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(m_textInstance.gameObject, m_textInstance.transform.position, Quaternion.identity, parent);
				m_items.Add(gameObject.GetComponent<QuestText>());
				if (m_background != null)
				{
					m_background.FitToObjectHeight(gameObject);
				}
			}
			if (m_maxVisibleItems > 0 && (m_btnScrollBack.gameObject.activeSelf || m_btnScrollForward.gameObject.activeSelf))
			{
				parent.localPosition = parent.localPosition.WithX(m_textContainerOffsetX);
			}
			else
			{
				parent.localPosition = parent.localPosition.WithX(m_textContainerOffsetX - m_arrowButtonWidth);
			}
			Vector2 vector = Singleton<PowerQuest>.Get.GetCameraGui().ScreenToWorldPoint(Input.mousePosition.WithZ(0f));
			DialogOption dialogOption = null;
			float num4 = (new float[3] { 1f, 0f, 0.5f })[(int)m_verticalAlignment];
			float num5 = 0f;
			for (int num6 = 0; num6 < num2; num6++)
			{
				DialogOption dialogOption2 = list[num6];
				QuestText questText = m_items[num6];
				Renderer component = questText.GetComponent<Renderer>();
				questText.gameObject.SetActive(value: true);
				questText.text = (m_showOptionNumbers ? $"{num6 + 1}. " : string.Empty) + dialogOption2.Text;
				num5 += component.bounds.size.y;
			}
			if (m_verticalAlignment == eAlignVertical.Bottom)
			{
				num5 -= m_defaultItemHeight;
			}
			num5 += m_itemSpacing * (float)(num2 - 1);
			float num7 = (0f - num5) * num4;
			for (int num8 = 0; num8 < num2; num8++)
			{
				DialogOption dialogOption3 = list[num8];
				QuestText questText2 = m_items[num8];
				Bounds bounds = questText2.GetComponent<Renderer>().bounds;
				questText2.gameObject.transform.localPosition = m_textInstance.transform.localPosition + new Vector3(0f, 0f - num7, 0f);
				num7 += bounds.size.y;
				bool flag = vector.y > bounds.min.y + 1f && vector.y < bounds.max.y - 1f && vector.x > bounds.min.x;
				if (flag)
				{
					dialogOption = dialogOption3;
				}
				questText2.color = (flag ? m_colorHover : (dialogOption3.Used ? m_colorUsed : m_colorDefault));
				if (m_changeImageColor)
				{
					SpriteRenderer[] componentsInChildren = questText2.GetComponentsInChildren<SpriteRenderer>();
					for (int num9 = 0; num9 < componentsInChildren.Length; num9++)
					{
						componentsInChildren[num9].color = questText2.color;
					}
				}
				num7 += m_itemSpacing;
			}
			for (int num10 = num2; num10 < m_items.Count; num10++)
			{
				m_items[num10].gameObject.SetActive(value: false);
			}
			Singleton<PowerQuest>.Get.SetDialogOptionSelected(dialogOption);
			if (dialogOption != null && Input.GetMouseButtonDown(0))
			{
				Singleton<PowerQuest>.Get.OnDialogOptionClick(dialogOption);
			}
		}

		public void ScrollBack()
		{
			if (m_itemsOffset != 0)
			{
				m_itemsOffset--;
			}
		}

		public void ScrollForward()
		{
			if (m_itemsOffset != m_maxNumOptions - m_maxVisibleItems)
			{
				m_itemsOffset++;
			}
		}

		private void OnClickScrollUp(Button button)
		{
			ScrollBack();
		}

		private void OnClickScrollDown(Button button)
		{
			ScrollForward();
		}
	}
}
