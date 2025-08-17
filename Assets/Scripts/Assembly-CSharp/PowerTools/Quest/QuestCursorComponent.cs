using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PowerTools.Quest
{
	public class QuestCursorComponent : MonoBehaviour
	{
		private static readonly string STR_NONE = "None";

		[SerializeField]
		private QuestCursor m_data = new QuestCursor();

		[Tooltip("List of animations that cursor will use even if an inv item is selected")]
		[SerializeField]
		private List<string> m_inventoryOverrideAnims = new List<string>();

		[SerializeField]
		[ReadOnly]
		private List<AnimationClip> m_animations = new List<AnimationClip>();

		[SerializeField]
		[ReadOnly]
		private List<Sprite> m_sprites = new List<Sprite>();

		private SpriteRenderer m_sprite;

		private SpriteAnim m_spriteAnimator;

		private bool m_noneCursor;

		private bool m_inventoryOverride;

		private PowerSprite m_powerSprite;

		private string m_playingAnim;

		private string m_currAnim;

		public string CurrentAnim => m_currAnim;

		public SpriteRenderer SpriteRenderer => m_sprite;

		public bool GetNoneCursorActive()
		{
			return m_noneCursor;
		}

		public bool GetInventoryCursorOverridden()
		{
			return m_inventoryOverride;
		}

		public QuestCursor GetData()
		{
			return m_data;
		}

		public void SetData(QuestCursor data)
		{
			m_data = data;
		}

		public void SetVisible(bool visible)
		{
			GetComponent<Renderer>().enabled = visible;
		}

		public List<AnimationClip> GetAnimations()
		{
			return m_animations;
		}

		public List<Sprite> GetSprites()
		{
			return m_sprites;
		}

		public AnimationClip GetAnimation(string animName)
		{
			AnimationClip animationClip = QuestUtils.FindByName(GetAnimations(), animName);
			if (animationClip == null)
			{
				animationClip = Singleton<PowerQuest>.Get.GetInventoryAnimation(animName);
			}
			return animationClip;
		}

		public Sprite GetSprite(string animName)
		{
			Sprite sprite = PowerQuest.FindSpriteInList(m_sprites, animName);
			if (sprite == null)
			{
				sprite = Singleton<PowerQuest>.Get.GetInventorySprite(animName);
			}
			return sprite;
		}

		public void CalcCursorVisuals(IQuestClickable clickable, out string newAnim, out Color outlineColor)
		{
			newAnim = null;
			outlineColor = new Color(1f, 1f, 1f, 0f);
			if (Utils.HasText(m_playingAnim))
			{
				newAnim = m_playingAnim;
				return;
			}
			if (Utils.HasText(m_data.AnimationOverride))
			{
				newAnim = m_data.AnimationOverride;
				return;
			}
			if (Singleton<PowerQuest>.Get.GetBlocked() && Singleton<PowerQuest>.Get.GetCurrentDialog() == null && Singleton<PowerQuest>.Get.GetBlockingGui() == null)
			{
				newAnim = m_data.AnimationWait;
				return;
			}
			if (Singleton<PowerQuest>.Get.GetCurrentDialog() != null && Singleton<PowerQuest>.Get.DialogTreeGui == "DialogTree")
			{
				newAnim = m_data.AnimationOverGui;
			}
			Character player = Singleton<PowerQuest>.Get.GetPlayer();
			string text = ((clickable == null || clickable.Cursor == null) ? string.Empty : clickable.Cursor);
			if (Utils.IsEmpty(newAnim) && clickable != null)
			{
				bool flag = m_inventoryOverrideAnims.Contains(text);
				if (clickable.ClickableType == eQuestClickableType.Gui || clickable.ClickableType == eQuestClickableType.Inventory)
				{
					Gui gui = clickable as Gui;
					GuiControl guiControl = clickable as GuiControl;
					if (clickable.ClickableType == eQuestClickableType.Inventory)
					{
						guiControl = Singleton<PowerQuest>.Get.GetFocusedGuiControl() as GuiControl;
					}
					if (guiControl != null)
					{
						gui = guiControl.GuiData;
					}
					if (player.HasActiveInventory && (text.EqualsIgnoreCase(PowerQuest.STR_INVENTORY) || (gui != null && gui.AllowInventoryCursor) || (gui == null && clickable.ClickableType == eQuestClickableType.Inventory)))
					{
						if (Utils.IsEmpty(clickable.Description) && !Utils.IsEmpty(player.ActiveInventory.AnimCursorInactive))
						{
							newAnim = player.ActiveInventory.AnimCursorInactive;
						}
						else
						{
							newAnim = player.ActiveInventory.AnimCursor;
							if ((m_data.InventoryOutlineOnGui == QuestCursor.eInventoryOutlineOnGui.Always || (m_data.InventoryOutlineOnGui == QuestCursor.eInventoryOutlineOnGui.OtherItemsOnly && clickable != player.ActiveInventory)) && !text.EqualsIgnoreCase(STR_NONE))
							{
								outlineColor = m_data.InventoryOutlineColor;
							}
						}
					}
					if (Utils.IsEmpty(newAnim))
					{
						newAnim = text;
					}
					if (Utils.IsEmpty(newAnim))
					{
						newAnim = m_data.AnimationOverGui;
					}
				}
				if (Utils.IsEmpty(newAnim) && player.HasActiveInventory && !flag)
				{
					if (!text.EqualsIgnoreCase(STR_NONE))
					{
						outlineColor = m_data.InventoryOutlineColor;
					}
					if (!Utils.IsEmpty(player.ActiveInventory.AnimCursor))
					{
						newAnim = player.ActiveInventory.AnimCursor;
					}
					else
					{
						newAnim = m_data.AnimationUseInv;
					}
				}
				if (Utils.IsEmpty(newAnim))
				{
					string text2 = PowerQuest.SCRIPT_FUNCTION_GETCURSOR;
					if (clickable.ClickableType != eQuestClickableType.Character)
					{
						text2 = text2 + clickable.ClickableType.ToString() + clickable.ScriptName;
					}
					string cursorScriptOverride = GetCursorScriptOverride(clickable.GetScript(), text2);
					if (!Utils.IsEmpty(cursorScriptOverride))
					{
						newAnim = cursorScriptOverride;
					}
				}
				if (IsString.Empty(newAnim) && !IsString.Empty(text) && (!player.HasActiveInventory || flag))
				{
					newAnim = text;
				}
				if (Utils.IsEmpty(newAnim))
				{
					newAnim = m_data.AnimationClickable;
				}
			}
			if (Utils.IsEmpty(newAnim) && player.HasActiveInventory && !Singleton<PowerQuest>.Get.Paused)
			{
				if (!Utils.IsEmpty(player.ActiveInventory.AnimCursorInactive))
				{
					newAnim = player.ActiveInventory.AnimCursorInactive;
				}
				else if (!Utils.IsEmpty(player.ActiveInventory.AnimCursor))
				{
					newAnim = player.ActiveInventory.AnimCursor;
				}
				else
				{
					newAnim = m_data.AnimationUseInv;
				}
			}
			if (Utils.IsEmpty(newAnim))
			{
				newAnim = m_data.AnimationNonClickable;
			}
			if (newAnim.EqualsIgnoreCase(STR_NONE) || text.EqualsIgnoreCase(STR_NONE))
			{
				outlineColor = new Color(1f, 1f, 1f, 0f);
			}
		}

		private void Awake()
		{
			m_sprite = GetComponent<SpriteRenderer>();
			m_spriteAnimator = m_sprite.GetComponent<SpriteAnim>();
			m_powerSprite = m_sprite.GetComponent<PowerSprite>();
		}

		private void Start()
		{
			m_spriteAnimator.Play(GetAnimations().Find((AnimationClip item) => string.Equals(m_data.AnimationClickable, item.name, StringComparison.OrdinalIgnoreCase)));
		}

		public void PlayAnimation(string animation)
		{
			m_playingAnim = animation;
			m_spriteAnimator.Play(GetAnimation(animation));
		}

		public void StopAnimation()
		{
			if (!Utils.IsEmpty(m_playingAnim))
			{
				m_playingAnim = null;
				if (m_spriteAnimator.IsPlaying(m_playingAnim))
				{
					m_spriteAnimator.Stop();
				}
				OnChangeAnimation();
			}
		}

		public void OnChangeAnimation()
		{
			Update();
		}

		private void Update()
		{
			if (m_data.HideWhenBlocking)
			{
				SetVisible(!Singleton<PowerQuest>.Get.GetBlocked() && m_data.Visible);
			}
			Camera main = Camera.main;
			Camera cameraGui = Singleton<PowerQuest>.Get.GetCameraGui();
			if (main != null && cameraGui != null)
			{
				base.transform.position = cameraGui.ScreenToWorldPoint(main.WorldToScreenPoint(Singleton<PowerQuest>.Get.GetMousePosition())).WithZ(0f);
			}
			IQuestClickable mouseOverClickable = Singleton<PowerQuest>.Get.GetMouseOverClickable();
			string newAnim = null;
			Color outlineColor = new Color(1f, 1f, 1f, 0f);
			CalcCursorVisuals(mouseOverClickable, out newAnim, out outlineColor);
			if (m_currAnim != newAnim)
			{
				m_currAnim = newAnim;
				AnimationClip animation = GetAnimation(newAnim);
				if (animation != null)
				{
					m_spriteAnimator.Play(animation);
				}
				else
				{
					Sprite sprite = GetSprite(newAnim);
					if (sprite != null)
					{
						m_spriteAnimator.Stop();
						m_sprite.GetComponent<SpriteRenderer>().sprite = sprite;
					}
				}
			}
			m_noneCursor = m_currAnim.EqualsIgnoreCase(STR_NONE) || (mouseOverClickable != null && mouseOverClickable.Cursor != null && mouseOverClickable.Cursor.EqualsIgnoreCase(STR_NONE));
			if (m_noneCursor)
			{
				outlineColor = new Color(1f, 1f, 1f, 0f);
			}
			m_inventoryOverride = m_inventoryOverrideAnims.Contains(m_currAnim);
			if (m_powerSprite != null)
			{
				m_powerSprite.Outline = outlineColor;
			}
		}

		private string GetCursorScriptOverride(QuestScript scriptClass, string methodName)
		{
			string result = null;
			if (scriptClass != null)
			{
				MethodInfo method = scriptClass.GetType().GetMethod(methodName);
				if (method != null)
				{
					result = method.Invoke(scriptClass, null) as string;
				}
			}
			return result;
		}
	}
}
