using System;
using UnityEngine;

namespace PowerTools.Quest
{
	public class GuiSpeechBoxComponent : GuiComponent, ISpeechGui
	{
		[SerializeField]
		private bool m_usePlayerTextColour = true;

		private QuestText m_text;

		private SpriteRenderer m_sprite;

		private SpriteAnim m_spriteAnimator;

		private Character m_character;

		private int m_currLineId = -1;

		public void StartSay(Character character, string text, int currLineId, bool background)
		{
			GetData().Show();
			m_character = character;
			m_currLineId = currLineId;
			m_text.text = text;
			if (m_usePlayerTextColour)
			{
				m_text.color = m_character.TextColour;
			}
			CharacterComponent component = character.GetPrefab().GetComponent<CharacterComponent>();
			bool activeSelf = base.gameObject.activeSelf;
			base.gameObject.SetActive(value: true);
			AnimationClip animationClip = component.GetAnimations().Find((AnimationClip item) => string.Equals(m_character.AnimTalk, item.name, StringComparison.OrdinalIgnoreCase));
			if (animationClip != null)
			{
				m_sprite.enabled = true;
				base.gameObject.SetActive(value: true);
				m_spriteAnimator.Play(animationClip);
			}
			else
			{
				m_sprite.enabled = false;
			}
			Update();
			base.gameObject.SetActive(activeSelf);
		}

		public void EndSay(Character character)
		{
			GetData().Hide();
		}

		private void Awake()
		{
			m_text = GetComponentInChildren<QuestText>(includeInactive: true);
			m_spriteAnimator = GetComponentInChildren<SpriteAnim>(includeInactive: true);
			m_sprite = m_spriteAnimator.GetComponent<SpriteRenderer>();
		}

		private void Update()
		{
			if (m_character == null || !m_character.LipSyncEnabled)
			{
				return;
			}
			TextData textData = SystemText.FindTextData(m_currLineId, m_character.ScriptName);
			float time = 0f;
			if (m_character.GetDialogAudioSource() != null)
			{
				time = m_character.GetDialogAudioSource().time;
			}
			int num = -1;
			if (textData != null)
			{
				num = Array.FindIndex(textData.m_phonesTime, (float item) => item > time);
			}
			num--;
			char c = 'X';
			if (num >= 0 && num < textData.m_phonesCharacter.Length)
			{
				c = textData.m_phonesCharacter[num];
			}
			int num2 = c - 65;
			m_spriteAnimator.SetNormalizedTime(((float)num2 + 0.5f) / 7f);
			m_spriteAnimator.Pause();
		}
	}
}
