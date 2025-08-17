using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PowerTools
{
	[RequireComponent(typeof(Animator))]
	[DisallowMultipleComponent]
	[AddComponentMenu("PowerTools/PowerSpriteAnimation")]
	public class SpriteAnim : SpriteAnimEventHandler
	{
		private static readonly int STATE_NAME_HASH = Animator.StringToHash("a");

		private static readonly string CONTROLLER_PATH = "SpriteAnimController";

		[SerializeField]
		private AnimationClip m_defaultAnim;

		private static RuntimeAnimatorController m_sharedAnimatorController = null;

		private Animator m_animator;

		private AnimatorOverrideController m_controller;

		private SpriteAnimNodes m_nodes;

		private List<KeyValuePair<AnimationClip, AnimationClip>> m_clipPairList = new List<KeyValuePair<AnimationClip, AnimationClip>>(1);

		private AnimationClip m_currAnim;

		private float m_speed = 1f;

		public Action CallbackOnPlay;

		public Action CallbackOnStop;

		public Animator Animator => m_animator;

		public bool Playing => IsPlaying();

		public bool Paused
		{
			get
			{
				return IsPaused();
			}
			set
			{
				if (value)
				{
					Pause();
				}
				else
				{
					Resume();
				}
			}
		}

		public float Speed
		{
			get
			{
				return m_speed;
			}
			set
			{
				SetSpeed(value);
			}
		}

		public float Time
		{
			get
			{
				return GetTime();
			}
			set
			{
				SetTime(value);
			}
		}

		public float NormalizedTime
		{
			get
			{
				return GetNormalisedTime();
			}
			set
			{
				SetNormalizedTime(value);
			}
		}

		public AnimationClip Clip => m_currAnim;

		public string ClipName
		{
			get
			{
				if (!(m_currAnim != null))
				{
					return string.Empty;
				}
				return m_currAnim.name;
			}
		}

		public void Play(AnimationClip anim, float speed = 1f)
		{
			if (!(anim == null))
			{
				if (m_controller == null)
				{
					Debug.LogWarning("Attempted to play animation before object was Awake: " + anim.name);
					Awake();
				}
				if (m_nodes != null)
				{
					m_nodes.Reset();
				}
				m_clipPairList[0] = new KeyValuePair<AnimationClip, AnimationClip>(m_clipPairList[0].Key, anim);
				m_controller.ApplyOverrides(m_clipPairList);
				if (!m_animator.enabled)
				{
					m_animator.enabled = true;
				}
				m_animator.Update(0f);
				m_animator.Play(STATE_NAME_HASH, 0, 0f);
				m_speed = Mathf.Max(0f, speed);
				m_animator.speed = m_speed;
				m_currAnim = anim;
				if (CallbackOnPlay != null)
				{
					CallbackOnPlay();
				}
				m_animator.Update(0f);
			}
		}

		public void Stop()
		{
			if (m_animator != null)
			{
				m_animator.enabled = false;
			}
			if (CallbackOnStop != null)
			{
				CallbackOnStop();
			}
		}

		public void Pause()
		{
			if (m_animator != null)
			{
				m_animator.speed = 0f;
			}
		}

		public void Resume()
		{
			if (m_animator != null)
			{
				m_animator.speed = m_speed;
			}
		}

		public AnimationClip GetCurrentAnimation()
		{
			return m_currAnim;
		}

		public bool IsPlaying(AnimationClip clip = null)
		{
			if (m_currAnim == null || !m_animator.enabled)
			{
				return false;
			}
			if (clip != null && m_currAnim != clip)
			{
				return false;
			}
			if (!m_currAnim.isLooping)
			{
				return m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f;
			}
			return true;
		}

		public bool IsPlaying(string animName)
		{
			if (m_currAnim == null || m_currAnim.name != animName)
			{
				return false;
			}
			return IsPlaying(m_currAnim);
		}

		public bool IsPaused()
		{
			if (m_currAnim == null)
			{
				return false;
			}
			return m_animator.speed == 0f;
		}

		public void SetSpeed(float speed)
		{
			m_speed = Mathf.Max(0f, speed);
			m_animator.speed = m_speed;
		}

		public float GetSpeed()
		{
			return m_speed;
		}

		public float GetTime()
		{
			if (m_currAnim != null)
			{
				return m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f * m_currAnim.length;
			}
			return 0f;
		}

		public void SetTime(float time)
		{
			if (!(m_currAnim == null) && !(m_currAnim.length <= 0f))
			{
				SetNormalizedTime(time / m_currAnim.length);
			}
		}

		public float GetNormalisedTime()
		{
			if (m_currAnim != null)
			{
				return m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
			}
			return 0f;
		}

		public void SetNormalizedTime(float ratio)
		{
			if (!(m_currAnim == null))
			{
				m_animator.Play(STATE_NAME_HASH, 0, m_currAnim.isLooping ? Mathf.Repeat(ratio, 1f) : Mathf.Clamp01(ratio));
			}
		}

		public void RegisterSpriteAnimNodesComponent(SpriteAnimNodes nodes)
		{
			m_nodes = nodes;
		}

		private void Awake()
		{
			m_controller = new AnimatorOverrideController();
			if (m_sharedAnimatorController == null)
			{
				m_sharedAnimatorController = Resources.Load<RuntimeAnimatorController>(CONTROLLER_PATH);
			}
			m_controller.runtimeAnimatorController = m_sharedAnimatorController;
			m_animator = GetComponent<Animator>();
			m_animator.runtimeAnimatorController = m_controller;
			m_controller.GetOverrides(m_clipPairList);
			Play(m_defaultAnim);
		}

		private void Reset()
		{
			if (GetComponent<RectTransform>() == null)
			{
				if (GetComponent<SpriteRenderer>() == null)
				{
					base.gameObject.AddComponent<SpriteRenderer>();
				}
			}
			else if (GetComponent<Image>() == null)
			{
				base.gameObject.AddComponent<Image>();
			}
		}
	}
}
