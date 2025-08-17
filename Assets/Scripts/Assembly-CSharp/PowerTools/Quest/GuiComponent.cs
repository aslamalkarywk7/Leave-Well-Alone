using System;
using System.Collections.Generic;
using UnityEngine;

namespace PowerTools.Quest
{
	[SelectionBase]
	public class GuiComponent : MonoBehaviour
	{
		[SerializeField]
		private Gui m_data = new Gui();

		[SerializeField]
		[ReadOnly]
		[NonReorderable]
		private List<AnimationClip> m_animations = new List<AnimationClip>();

		[SerializeField]
		[ReadOnly]
		[NonReorderable]
		private List<Sprite> m_sprites = new List<Sprite>();

		[SerializeField]
		[HideInInspector]
		private List<GuiControl> m_controlComponents = new List<GuiControl>();

		public Action CallbackOnFocus;

		public Action CallbackOnDefocus;

		public void OnFocus()
		{
			CallbackOnFocus?.Invoke();
		}

		public void OnDefocus()
		{
			CallbackOnDefocus?.Invoke();
		}

		public Gui GetData()
		{
			return m_data;
		}

		public void SetData(Gui data)
		{
			m_data = data;
		}

		public AnimationClip GetAnimation(string animName)
		{
			AnimationClip animationClip = m_animations.Find((AnimationClip item) => item != null && string.Equals(animName, item.name, StringComparison.OrdinalIgnoreCase));
			if (animationClip == null && Singleton<PowerQuest>.Get != null)
			{
				animationClip = Singleton<PowerQuest>.Get.GetGuiAnimation(animName);
			}
			if (animationClip == null && Singleton<PowerQuest>.Get != null)
			{
				animationClip = Singleton<PowerQuest>.Get.GetInventoryAnimation(animName);
			}
			return animationClip;
		}

		public List<AnimationClip> GetAnimations()
		{
			return m_animations;
		}

		public Sprite GetSprite(string animName)
		{
			Sprite sprite = PowerQuest.FindSpriteInList(m_sprites, animName);
			if (sprite == null && Singleton<PowerQuest>.Get != null)
			{
				sprite = Singleton<PowerQuest>.Get.GetGuiSprite(animName);
			}
			if (sprite == null && Singleton<PowerQuest>.Get != null)
			{
				sprite = Singleton<PowerQuest>.Get.GetInventorySprite(animName);
			}
			return sprite;
		}

		public List<Sprite> GetSprites()
		{
			return m_sprites;
		}

		public void OnSetBaseline()
		{
			foreach (GuiControl controlComponent in m_controlComponents)
			{
				if (controlComponent != null)
				{
					controlComponent.UpdateBaseline();
				}
			}
		}

		private void Awake()
		{
			m_controlComponents.Clear();
			m_controlComponents.AddRange(GetComponentsInChildren<GuiControl>());
		}

		private void Start()
		{
			OnSetBaseline();
		}

		private void OnEnable()
		{
			if (Singleton<PowerQuest>.Get != null && GetData() != null && GetData().Visible)
			{
				Singleton<PowerQuest>.Get.OnGuiShown(GetData());
			}
		}

		private void Update()
		{
			if (GetData().HasFocus)
			{
				if (Singleton<PowerQuest>.Get.GetFocusedGuiControl() == null && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
				{
					Singleton<PowerQuest>.Get.ProcessGuiClick(GetData());
				}
				if (Singleton<PowerQuest>.Get.GetFocusedGuiControl() != null)
				{
					GetData().LastFocusedControl = Singleton<PowerQuest>.Get.GetFocusedGuiControl();
				}
			}
		}

		public void RegisterControl(GuiControl control)
		{
			if (!m_controlComponents.Exists((GuiControl item) => item == control))
			{
				m_controlComponents.Add(control);
			}
		}

		public void EditorUpdateChildComponents()
		{
			m_controlComponents.Clear();
			m_controlComponents.AddRange(GetComponentsInChildren<GuiControl>(includeInactive: true));
		}

		public List<GuiControl> GetControlComponents()
		{
			return m_controlComponents;
		}
	}
}
