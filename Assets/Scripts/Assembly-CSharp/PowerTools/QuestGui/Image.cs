using System;
using System.Collections;
using PowerTools.Quest;
using UnityEngine;

namespace PowerTools.QuestGui
{
	[Serializable]
	[AddComponentMenu("Quest Gui/Image")]
	public class Image : GuiControl, IImage, IGuiControl
	{
		[SerializeField]
		private string m_anim;

		[SerializeField]
		[HideInInspector]
		private RectCentered m_customSize = RectCentered.zero;

		private SpriteRenderer m_sprite;

		private SpriteAnim m_spriteAnimator;

		private bool m_overrideAnimPlaying;

		private int m_stopOverrideAnimDelay = -1;

		private QuestAnimationTriggers m_animTriggerComponent;

		public string Anim
		{
			get
			{
				return m_anim;
			}
			set
			{
				if (m_anim != value)
				{
					m_anim = value;
					OnAnimationChanged();
				}
			}
		}

		public bool Animating => GetAnimating();

		public IQuestClickable IClickable => this;

		public override RectCentered CustomSize
		{
			get
			{
				return m_customSize;
			}
			set
			{
				m_customSize = value;
			}
		}

		private void Start()
		{
			OnAnimationChanged();
		}

		public void PauseAnimation()
		{
			if (m_spriteAnimator != null && m_overrideAnimPlaying)
			{
				m_spriteAnimator.Pause();
			}
		}

		public void ResumeAnimation()
		{
			if (m_spriteAnimator != null && m_overrideAnimPlaying)
			{
				m_spriteAnimator.Resume();
			}
		}

		public void StopAnimation()
		{
			if (m_spriteAnimator != null && m_overrideAnimPlaying)
			{
				PlayAnimInternal(m_anim);
			}
			m_overrideAnimPlaying = false;
		}

		public Coroutine PlayAnimation(string animName)
		{
			return Singleton<PowerQuest>.Get.StartCoroutine(CoroutinePlayAnimation(animName));
		}

		public void PlayAnimationBG(string animName)
		{
			if (!Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				PlayOverrideAnim(animName);
			}
		}

		public void AddAnimationTrigger(string triggerName, bool removeAfterTriggering, Action action)
		{
			QuestAnimationTriggers questAnimationTriggers = GetComponent<QuestAnimationTriggers>();
			if (questAnimationTriggers == null)
			{
				questAnimationTriggers = base.gameObject.AddComponent<QuestAnimationTriggers>();
			}
			if (questAnimationTriggers != null)
			{
				questAnimationTriggers.AddTrigger(triggerName, action, removeAfterTriggering);
			}
		}

		public void RemoveAnimationTrigger(string triggerName)
		{
			QuestAnimationTriggers component = GetComponent<QuestAnimationTriggers>();
			if (component != null)
			{
				component.RemoveTrigger(triggerName);
			}
		}

		public Coroutine WaitForAnimTrigger(string triggerName)
		{
			return Singleton<PowerQuest>.Get.StartCoroutine(CoroutineWaitForAnimTrigger(triggerName));
		}

		public SpriteRenderer GetSprite()
		{
			return m_sprite;
		}

		public SpriteAnim GetSpriteAnimator()
		{
			return m_spriteAnimator;
		}

		public override RectCentered GetRect(Transform excludeChild = null)
		{
			RectCentered result = RectCentered.zero;
			if (m_sprite == null)
			{
				m_sprite = GetComponentInChildren<SpriteRenderer>();
			}
			if (m_sprite != null)
			{
				result = GuiUtils.CalculateGuiRectFromSprite(base.transform, includeChildren: false, m_sprite, excludeChild);
				result.Transform(base.transform);
			}
			return result;
		}

		public void EditorUpdateSprite()
		{
			if (m_sprite == null)
			{
				m_sprite = GetComponentInChildren<SpriteRenderer>();
			}
			if (m_sprite != null)
			{
				GetComponentInParent<GuiComponent>().GetAnimation(m_anim);
			}
		}

		private void Awake()
		{
			if (m_sprite == null)
			{
				m_sprite = GetComponentInChildren<SpriteRenderer>(includeInactive: true);
			}
			if (m_sprite != null)
			{
				m_spriteAnimator = m_sprite.GetComponent<SpriteAnim>();
			}
		}

		private void OnAnimationChanged()
		{
			PlayAnimInternal(m_anim);
		}

		private bool PlayAnimInternal(string animName, bool fromStart = true)
		{
			m_stopOverrideAnimDelay = 0;
			if (string.IsNullOrEmpty(animName) || base.GuiComponent == null)
			{
				return false;
			}
			AnimationClip animation = GetAnimation(animName);
			if (animation != null && m_spriteAnimator != null)
			{
				if (fromStart || m_spriteAnimator.Clip == null)
				{
					m_spriteAnimator.Play(animation);
				}
				else
				{
					float time = m_spriteAnimator.Time;
					m_spriteAnimator.Play(animation);
					m_spriteAnimator.Time = time;
				}
				return true;
			}
			Sprite sprite = GetSprite(animName);
			if (sprite != null)
			{
				if (m_spriteAnimator != null)
				{
					m_spriteAnimator.Stop();
				}
				m_sprite.sprite = sprite;
				return true;
			}
			return false;
		}

		private void UpdateOverrideAnim()
		{
			if (m_overrideAnimPlaying && Singleton<PowerQuest>.Get.GetSkippingCutscene() && !m_spriteAnimator.GetCurrentAnimation().isLooping)
			{
				StopAnimation();
			}
		}

		private void LateUpdateOverrideAnim()
		{
			if (m_stopOverrideAnimDelay > 0)
			{
				m_stopOverrideAnimDelay--;
				if (m_stopOverrideAnimDelay == 0)
				{
					PlayAnimInternal(m_anim);
				}
			}
			if (m_overrideAnimPlaying && !m_spriteAnimator.Playing)
			{
				if (m_overrideAnimPlaying)
				{
					m_stopOverrideAnimDelay = 1;
				}
				m_overrideAnimPlaying = false;
			}
		}

		private void PlayOverrideAnim(string animName)
		{
			if (!PlayAnimInternal(animName) && Singleton<PowerQuest>.Get.IsDebugBuild)
			{
				Debug.LogWarning("Failed to find Gui Image animation: " + animName);
			}
			m_overrideAnimPlaying = true;
		}

		private bool GetAnimating()
		{
			if (m_overrideAnimPlaying)
			{
				return m_spriteAnimator.Playing;
			}
			return false;
		}

		private IEnumerator CoroutinePlayAnimation(string animName)
		{
			PlayOverrideAnim(animName);
			while (GetAnimating() && !Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				yield return new WaitForEndOfFrame();
			}
			if (Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				SpriteAnim component = GetComponent<SpriteAnim>();
				if (component != null)
				{
					component.NormalizedTime = 1f;
					GetComponent<Animator>().Update(0f);
				}
				StopAnimation();
			}
		}

		private IEnumerator CoroutineWaitForAnimTrigger(string triggerName)
		{
			if (!Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				bool hit = false;
				AddAnimationTrigger(triggerName, removeAfterTriggering: true, delegate
				{
					hit = true;
				});
				yield return Singleton<PowerQuest>.Get.WaitUntil(() => hit || !GetSpriteAnimator().Playing);
			}
		}

		private void AnimSound(UnityEngine.Object obj)
		{
			if (obj != null && obj as GameObject != null)
			{
				SystemAudio.Play((obj as GameObject).GetComponent<AudioCue>());
			}
		}

		private void AnimSound(string sound)
		{
			SystemAudio.Play(sound);
		}

		private void _Anim(string function)
		{
			if (m_animTriggerComponent == null)
			{
				m_animTriggerComponent = base.transform.GetComponent<QuestAnimationTriggers>();
				if (m_animTriggerComponent == null)
				{
					m_animTriggerComponent = base.transform.gameObject.AddComponent<QuestAnimationTriggers>();
				}
			}
		}
	}
}
