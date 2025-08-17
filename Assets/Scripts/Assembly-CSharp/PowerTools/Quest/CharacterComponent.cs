using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PowerTools.Quest
{
	[SelectionBase]
	public class CharacterComponent : MonoBehaviour
	{
		public enum eFaceMask
		{
			Left = 1,
			Right = 2,
			Down = 4,
			Up = 8,
			DownLeft = 0x10,
			DownRight = 0x20,
			UpLeft = 0x40,
			UpRight = 0x80
		}

		[Serializable]
		public class TransitionAnim
		{
			public string anim;

			public string from;

			public string to;

			public bool onFlip;
		}

		[Serializable]
		public class TurnAnim
		{
			public string fromAnim;

			[BitMask(typeof(eFaceMask))]
			public int fromDirection;

			[BitMask(typeof(eFaceMask))]
			public int toDirection;

			public string anim;

			public bool m_mirror = true;
		}

		private static readonly eFace[] TURN_ORDER = new eFace[8]
		{
			eFace.UpLeft,
			eFace.Left,
			eFace.DownLeft,
			eFace.Down,
			eFace.DownRight,
			eFace.Right,
			eFace.UpRight,
			eFace.Up
		};

		[Tooltip("Character data")]
		[SerializeField]
		private Character m_data = new Character();

		[Tooltip("If the character needs to swap between multiple clickable colliders, hook them up here")]
		[SerializeField]
		private Collider2D[] m_clickableColliders;

		[Tooltip("If character's changes from one anim to another, matching the from/to in this list, the transition anim will be played first")]
		[SerializeField]
		private TurnAnim[] m_turnAnims;

		[Tooltip("EXPERIMENTAL: If character's changes from one anim to another, matching the from/to in this list, the transition anim will be played first")]
		[SerializeField]
		private TransitionAnim[] m_transitionAnims;

		[Tooltip("This list is read only, animations are automatically added to it")]
		[ReadOnly]
		[NonReorderable]
		[SerializeField]
		private List<AnimationClip> m_animations = new List<AnimationClip>();

		private bool m_firstUpdate = true;

		private Vector2 m_targetPos = Vector2.zero;

		private Vector2 m_targetEndPos = Vector2.zero;

		private Character.eState m_state = Character.eState.None;

		private int m_playIdleDelayFrames;

		private Vector2[] m_path;

		private int m_pathPointNext = -1;

		private bool m_turningToWalk;

		private Sprite m_lastSprite;

		private eFace m_facing = eFace.None;

		private eFace m_fallbackDirection = eFace.None;

		private string m_currAnimBaseName;

		private int m_currLineId = -1;

		private float m_turnTimer;

		private bool m_addedSolidObstacle;

		private string m_transitionAnim;

		private string m_transitioningToAnim;

		private string m_transitioningFromAnim;

		private string m_playAfterTurnAnim;

		private string m_currTurnAnim;

		private bool m_flippedLastUpdate;

		private float m_animChangeTime;

		private bool m_playWalkAnim = true;

		private bool m_walking;

		private bool m_talking;

		private bool m_animating;

		private SpriteRenderer m_sprite;

		private PowerSprite m_powerSprite;

		private SpriteAnim m_spriteAnimator;

		private SpriteAnim m_mouth;

		private SpriteAnimNodes m_mouthNode;

		private GameObject m_shadow;

		private SpriteAnim m_shadowAnim;

		private PolygonCollider2D m_autoHotspotCollider;

		private Sprite m_lastHotspotSprite;

		private bool m_skipTransitionNextFrame;

		private static readonly string[] DIRECTION_POSTFIX = new string[8] { "L", "R", "D", "U", "DL", "DR", "UL", "UR" };

		private static readonly int DIRECTION_COUNT = 8;

		private Vector2 m_walkSpeedOverride = -Vector2.one;

		private bool m_animShadowOff;

		private static readonly int NUM_LIP_SYNC_FRAMES = 6;

		public bool Walking => m_walking;

		public bool Talking => m_talking;

		public bool Animating => m_animating;

		public Character GetData()
		{
			return m_data;
		}

		public void SetData(Character data)
		{
			m_data = data;
			OnClickableColliderIdChanged();
			if (!string.IsNullOrEmpty(m_data.Animation))
			{
				float loopStartTime = m_data.LoopStartTime;
				float loopEndTime = m_data.LoopEndTime;
				m_data.LoopStartTime = -1f;
				m_data.LoopEndTime = -1f;
				PlayAnimation(m_data.Animation);
				m_data.LoopStartTime = loopStartTime;
				m_data.LoopEndTime = loopEndTime;
			}
		}

		public SpriteRenderer GetSprite()
		{
			return m_sprite;
		}

		public SpriteAnim GetSpriteAnimator()
		{
			return m_spriteAnimator;
		}

		public Character.eState GetState()
		{
			return m_state;
		}

		public List<AnimationClip> GetAnimations()
		{
			return m_animations;
		}

		public Vector2 GetTargetPosition()
		{
			return m_targetEndPos;
		}

		public string GetTransitionAnim(string from, bool wasFlipped, string to, bool flip)
		{
			bool flipping = flip != wasFlipped;
			return Array.Find(m_transitionAnims, delegate(TransitionAnim item)
			{
				if (to == null || from == null)
				{
					return false;
				}
				if (!Regex.IsMatch(to, item.to, RegexOptions.IgnoreCase) || !Regex.IsMatch(from, item.from, RegexOptions.IgnoreCase))
				{
					return false;
				}
				return !item.onFlip || flipping;
			})?.anim;
		}

		public void OnRestoreSpriteOffset()
		{
			m_playIdleDelayFrames = 1;
			Update();
		}

		private string GetTurnAnim(eFace oldFacingVerticalFallback)
		{
			bool flip = false;
			eFace facingFrom = m_facing;
			eFace facingTo = m_data.GetTargetFaceDirection();
			if (string.IsNullOrEmpty(m_currAnimBaseName))
			{
				return null;
			}
			eFace facingVerticalFallback = m_data.GetFacingVerticalFallback();
			m_data.SetFacingVerticalFallback(oldFacingVerticalFallback);
			FindDirectionalAnimationName(facingFrom, m_currAnimBaseName, out flip, out var animFacing);
			if (animFacing != eFace.None)
			{
				facingFrom = animFacing;
			}
			m_data.SetFacingVerticalFallback(facingVerticalFallback);
			FindDirectionalAnimationName(facingTo, m_currAnimBaseName, out flip, out animFacing);
			if (animFacing != eFace.None)
			{
				facingTo = animFacing;
			}
			if (facingFrom == facingTo)
			{
				return null;
			}
			return Array.Find(m_turnAnims, delegate(TurnAnim item)
			{
				eFace index = facingFrom;
				bool flag = BitMask.IsSet(item.fromDirection, (int)index) && BitMask.IsSet(item.toDirection, (int)facingTo) && Regex.IsMatch(m_currAnimBaseName, "^(" + item.fromAnim + ")$", RegexOptions.IgnoreCase);
				if (!flag && item.m_mirror)
				{
					flag = BitMask.IsSet(item.toDirection, (int)index) && BitMask.IsSet(item.fromDirection, (int)facingTo) && Regex.IsMatch(m_currAnimBaseName, "^(" + item.fromAnim + ")$", RegexOptions.IgnoreCase);
				}
				return flag;
			})?.anim;
		}

		public bool StartTurnAnimation(eFace oldFacingVerticalFallback)
		{
			string turnAnim = GetTurnAnim(oldFacingVerticalFallback);
			if (turnAnim == null)
			{
				return false;
			}
			m_playAfterTurnAnim = m_currAnimBaseName;
			m_currTurnAnim = turnAnim;
			PlayAnimInternal(turnAnim);
			return true;
		}

		public void EndTurnAnimation()
		{
			if (m_playAfterTurnAnim != null)
			{
				PlayAnimInternal(m_playAfterTurnAnim);
			}
			m_playAfterTurnAnim = null;
		}

		public bool GetPlayingTurnAnimation()
		{
			return m_playAfterTurnAnim != null;
		}

		public void UpdateVisibility()
		{
			if (m_data != null)
			{
				bool shouldShow = m_data.Visible;
				if (GetSpriteAnimator() != null && GetSpriteAnimator().Animator != null)
				{
					GetSpriteAnimator().Animator.enabled = shouldShow;
				}
				Array.ForEach(GetComponentsInChildren<Renderer>(includeInactive: true), delegate(Renderer renderer)
				{
					renderer.enabled = shouldShow;
				});
				if (m_playIdleDelayFrames > 0)
				{
					m_playIdleDelayFrames = 1;
					Update();
				}
			}
		}

		public void UpdateEnabled()
		{
			UpdateVisibility();
			UpdateSolid();
		}

		public void UpdateSolid()
		{
			if (GetData() != null && Singleton<PowerQuest>.Get.Pathfinder != null)
			{
				if (!m_addedSolidObstacle && GetData().Solid)
				{
					Singleton<PowerQuest>.Get.Pathfinder.AddObstacle(base.transform, CalcSolidPoly());
					m_addedSolidObstacle = true;
				}
				else if (m_addedSolidObstacle && !GetData().Solid)
				{
					Singleton<PowerQuest>.Get.Pathfinder.RemoveObstacle(base.transform);
					m_addedSolidObstacle = false;
				}
			}
		}

		public void UpdateSolidSize()
		{
			if (m_addedSolidObstacle && Singleton<PowerQuest>.Get.Pathfinder != null)
			{
				Singleton<PowerQuest>.Get.Pathfinder.RemoveObstacle(base.transform);
				m_addedSolidObstacle = false;
			}
			UpdateSolid();
		}

		public Vector2[] CalcSolidPoly()
		{
			Vector2 vector = GetData().SolidSize * 0.5f;
			return new Vector2[4]
			{
				new Vector2(0f - vector.x, 0f - vector.y),
				new Vector2(0f - vector.x, vector.y),
				new Vector2(vector.x, vector.y),
				new Vector2(vector.x, 0f - vector.y)
			};
		}

		public void UpdateUseSpriteAsHotspot()
		{
			if (GetData() == null)
			{
				return;
			}
			if (m_data.UseSpriteAsHotspot)
			{
				if (m_clickableColliders == null || m_clickableColliders.Length == 0)
				{
					Collider2D component = GetComponent<Collider2D>();
					if (component != null)
					{
						m_clickableColliders = new Collider2D[1] { component };
						m_data.ClickableColliderId = 0;
					}
				}
				if (m_autoHotspotCollider == null)
				{
					m_autoHotspotCollider = base.gameObject.AddComponent<PolygonCollider2D>();
					m_autoHotspotCollider.isTrigger = true;
				}
				m_autoHotspotCollider.enabled = true;
				for (int i = 0; i < m_clickableColliders.Length; i++)
				{
					if (m_clickableColliders[i] != null)
					{
						m_clickableColliders[i].enabled = false;
					}
				}
			}
			else
			{
				if (m_autoHotspotCollider != null)
				{
					m_autoHotspotCollider.enabled = false;
				}
				if (GetData().ClickableColliderId >= 0)
				{
					OnClickableColliderIdChanged();
				}
			}
		}

		public void UpdateShadow()
		{
			if (!(m_shadow == null))
			{
				bool flag = GetData().ShadowEnabled && !m_animShadowOff;
				m_shadow.SetActive(flag);
				if (flag && m_shadowAnim != null && !string.IsNullOrEmpty(GetData().AnimShadow))
				{
					AnimationClip anim = QuestUtils.FindByName(GetAnimations(), GetData().AnimShadow);
					m_shadowAnim.Play(anim);
				}
			}
		}

		private void Awake()
		{
			m_sprite = GetComponentInChildren<SpriteRenderer>();
			m_powerSprite = m_sprite.GetComponent<PowerSprite>();
			m_spriteAnimator = m_sprite.GetComponent<SpriteAnim>();
			if (base.transform.GetComponent<QuestAnimationTriggers>() == null)
			{
				base.transform.gameObject.AddComponent<QuestAnimationTriggers>();
			}
			SpriteAnim spriteAnimator = m_spriteAnimator;
			spriteAnimator.CallbackOnPlay = (Action)Delegate.Combine(spriteAnimator.CallbackOnPlay, new Action(OnAnimationReset));
			SpriteAnim spriteAnimator2 = m_spriteAnimator;
			spriteAnimator2.CallbackOnStop = (Action)Delegate.Combine(spriteAnimator2.CallbackOnStop, new Action(OnAnimationReset));
			Transform transform = base.transform.Find("Shadow");
			if (transform != null)
			{
				m_shadow = transform.gameObject;
				m_shadowAnim = transform.GetComponentInChildren<SpriteAnim>();
			}
		}

		private void Start()
		{
			m_firstUpdate = true;
			m_spriteAnimator.NormalizedTime = m_data.AnimationTime;
		}

		private void OnDestroy()
		{
			Singleton<PowerQuest>.Get?.Pathfinder?.RemoveObstacle(base.transform);
		}

		private void OnTransitionAnimComplete()
		{
			if (string.IsNullOrEmpty(m_transitioningToAnim))
			{
				return;
			}
			string transitioningToAnim = m_transitioningToAnim;
			m_transitioningToAnim = null;
			m_transitionAnim = null;
			m_transitioningFromAnim = null;
			m_data.LoopStartTime = -1f;
			m_data.LoopEndTime = -1f;
			PlayAnimInternal(transitioningToAnim);
			if (Walking)
			{
				m_walking = false;
				WalkToInternal(m_targetPos);
				if (!m_walking)
				{
					OnAnimStateChange();
				}
			}
		}

		private void OnAnimStateChange()
		{
			if (m_animating)
			{
				if (m_state != Character.eState.Animate)
				{
					SetState(Character.eState.Animate);
				}
			}
			else if (m_walking)
			{
				if (m_state != Character.eState.Walk)
				{
					SetState(Character.eState.Walk);
				}
			}
			else if (m_talking)
			{
				if (m_state != Character.eState.Talk)
				{
					SetState(Character.eState.Talk);
				}
			}
			else if (m_state != Character.eState.Idle)
			{
				SetState(Character.eState.Idle);
			}
		}

		private void Update()
		{
			if (m_firstUpdate)
			{
				m_firstUpdate = false;
				UpdateVisibility();
				UpdateFacingVisuals(m_data.Facing);
				OnAnimStateChange();
				UpdateSolid();
				UpdateUseSpriteAsHotspot();
				UpdateShadow();
				if (m_animating)
				{
					m_spriteAnimator.NormalizedTime = m_data.AnimationTime;
				}
			}
			m_animChangeTime += Time.deltaTime;
			if (m_spriteAnimator != null && m_spriteAnimator.Clip != null && m_data.LoopStartTime >= 0f && m_data.LoopEndTime < 0f && m_spriteAnimator.ClipName != m_transitionAnim && (m_spriteAnimator.NormalizedTime > 1f || !m_spriteAnimator.IsPlaying()))
			{
				m_spriteAnimator.Play(m_spriteAnimator.Clip);
				m_spriteAnimator.NormalizedTime = m_data.LoopStartTime;
			}
			if (m_spriteAnimator != null && Animating)
			{
				m_data.AnimationTime = m_spriteAnimator.NormalizedTime;
			}
			else
			{
				m_data.AnimationTime = -1f;
			}
			if (!string.IsNullOrEmpty(m_transitioningToAnim) && !m_spriteAnimator.Playing)
			{
				OnTransitionAnimComplete();
			}
			if (!string.IsNullOrEmpty(m_playAfterTurnAnim) && !m_spriteAnimator.Playing)
			{
				string playAfterTurnAnim = m_playAfterTurnAnim;
				m_playAfterTurnAnim = null;
				PlayAnimInternal(playAfterTurnAnim);
			}
			m_data.UpdateFacingCharacter();
			UpdateTurnToFace();
			if (m_animating)
			{
				if (!m_spriteAnimator.Playing && !m_data.PauseAnimAtEnd)
				{
					m_data.StopAnimation();
				}
				else if (Singleton<PowerQuest>.Get.GetSkippingCutscene() && !m_spriteAnimator.GetCurrentAnimation().isLooping)
				{
					m_data.StopAnimation();
				}
			}
			UpdateWalking();
			UpdateAnimating();
			switch (m_state)
			{
			case Character.eState.Idle:
				if (m_playIdleDelayFrames <= 0)
				{
					break;
				}
				m_playIdleDelayFrames--;
				if (m_playIdleDelayFrames <= 0)
				{
					if (m_currTurnAnim != null)
					{
						m_playAfterTurnAnim = m_data.AnimIdle;
					}
					else
					{
						PlayAnimInternal(m_data.AnimIdle);
					}
				}
				break;
			}
			if (m_data.Visible)
			{
				UpdateLipSync();
			}
			if (m_sprite != null)
			{
				m_sprite.sortingOrder = -Mathf.RoundToInt((m_data.GetPosition().y + m_data.Baseline) * 10f);
			}
			if (m_data.UseSpriteAsHotspot && !Singleton<PowerQuest>.Get.GetBlocked() && m_data.Clickable && m_lastHotspotSprite != m_sprite.sprite && m_autoHotspotCollider != null && m_sprite != null)
			{
				Sprite sprite = m_sprite.sprite;
				m_autoHotspotCollider.pathCount = sprite.GetPhysicsShapeCount();
				List<Vector2> list = new List<Vector2>();
				for (int i = 0; i < m_autoHotspotCollider.pathCount; i++)
				{
					list.Clear();
					sprite.GetPhysicsShape(i, list);
					Vector2[] points = Pathfinder.InflatePoly(list.ToArray(), 2f);
					m_autoHotspotCollider.SetPath(i, points);
				}
				if (m_powerSprite != null)
				{
					m_autoHotspotCollider.offset = m_powerSprite.Offset;
				}
				m_lastHotspotSprite = sprite;
			}
			m_skipTransitionNextFrame = false;
		}

		private void UpdateAnimating()
		{
			if (Animating)
			{
				if (!m_spriteAnimator.Playing && !m_data.PauseAnimAtEnd)
				{
					m_data.Animation = null;
					m_animating = false;
				}
				else if (Singleton<PowerQuest>.Get.GetSkippingCutscene() && !m_spriteAnimator.GetCurrentAnimation().isLooping && !m_data.PauseAnimAtEnd && m_data.LoopStartTime < 0f)
				{
					m_data.StopAnimation();
				}
			}
		}

		private bool UpdateWalking()
		{
			if (!Walking)
			{
				return false;
			}
			Vector2 vector = m_data.GetPosition();
			bool flag = false;
			if (GetPlayingTransition())
			{
				return true;
			}
			if (m_turningToWalk)
			{
				if (m_data.GetFaceDirection() != m_data.GetTargetFaceDirection())
				{
					return true;
				}
				m_turningToWalk = false;
				PlayAnimInternal(m_data.AnimWalk, fromStart: false);
			}
			float num = Time.deltaTime;
			while (!flag && num > 0f)
			{
				if (m_path != null && m_pathPointNext > -1)
				{
					m_targetPos = m_path[m_pathPointNext];
				}
				else if (m_data.Waypoints.Count > 0)
				{
					m_targetPos = m_data.Waypoints[0];
				}
				Vector2 vector2 = m_targetPos - vector;
				float num2 = Utils.NormalizeMag(ref vector2);
				Vector2 vector3 = new Vector2((m_walkSpeedOverride.x != -1f) ? m_walkSpeedOverride.x : m_data.WalkSpeed.x, (m_walkSpeedOverride.y != -1f) ? m_walkSpeedOverride.y : m_data.WalkSpeed.y);
				float num3 = Mathf.Abs(vector2.x) * vector3.x + Mathf.Abs(vector2.y) * vector3.y;
				if (m_data.AdjustSpeedWithScaling)
				{
					num3 *= base.transform.localScale.y;
				}
				if (num2 > 0f)
				{
					m_data.FaceDirection(vector2, instant: true);
				}
				if (num2 == 0f || num2 < num3 * num)
				{
					if (num2 > 0f)
					{
						num -= num2 / num3;
					}
					vector = m_targetPos;
					if (m_path != null && m_pathPointNext > 0)
					{
						m_pathPointNext++;
						if (m_pathPointNext >= m_path.Length)
						{
							flag = m_data.Waypoints.Count == 0;
							m_path = null;
							m_pathPointNext = -1;
						}
					}
					else if (m_data.Waypoints.Count > 0)
					{
						m_data.Waypoints.RemoveAt(0);
						flag = m_data.Waypoints.Count == 0;
					}
					else
					{
						flag = true;
					}
				}
				else
				{
					vector += vector2 * num3 * num;
					num -= Time.deltaTime;
				}
			}
			if (GetData().AntiGlide && !flag)
			{
				Vector2 vector4 = base.transform.position;
				m_data.SetPosition(vector);
				base.transform.position = vector4;
			}
			else
			{
				m_data.SetPosition(vector);
			}
			if (flag)
			{
				m_walking = false;
				OnAnimStateChange();
				if (m_data.GetFaceAfterWalk() != eFace.None)
				{
					m_data.FaceBG(m_data.GetFaceAfterWalk());
				}
				m_path = null;
				m_pathPointNext = -1;
			}
			return !flag;
		}

		private void LateUpdate()
		{
			if (m_data != null && (m_flippedLastUpdate != Flipped() || m_lastSprite != m_sprite.sprite))
			{
				base.transform.position = m_data.Position;
				m_lastSprite = m_sprite.sprite;
			}
			m_flippedLastUpdate = Flipped();
		}

		private void UpdateTurnToFace()
		{
			eFace targetFaceDirection = m_data.GetTargetFaceDirection();
			if (targetFaceDirection == eFace.None || IsString.Set(m_transitioningToAnim))
			{
				return;
			}
			bool flag = targetFaceDirection == m_data.Facing;
			if (!flag && !CheckTargetDirectionWillChangeAnim())
			{
				m_data.SetFaceDirection(targetFaceDirection);
				flag = true;
			}
			if (!flag)
			{
				m_turnTimer -= Time.deltaTime;
			}
			else
			{
				m_turnTimer = -1f;
			}
			if (targetFaceDirection == m_data.Facing || !(m_turnTimer <= 0f))
			{
				return;
			}
			m_turnTimer = 1f / m_data.TurnSpeedFPS;
			bool flag2 = false;
			while (!flag2 && targetFaceDirection != m_data.Facing)
			{
				int num = Array.IndexOf(TURN_ORDER, m_data.Facing);
				int num2 = Array.IndexOf(TURN_ORDER, targetFaceDirection);
				int num3 = num2 - num;
				int num4 = (int)Mathf.Sign(num3);
				num4 = ((num3 != 0 && Mathf.Abs(num3) != TURN_ORDER.Length / 2) ? ((Mathf.Abs(num3) < Mathf.Abs(num2 - TURN_ORDER.Length * num4 - num)) ? num4 : (-num4)) : ((TURN_ORDER[num] != eFace.Up && TURN_ORDER[num] != eFace.Down) ? ((num < 3) ? 1 : (-1)) : ((m_data.GetFacingVerticalFallback() == eFace.Right != (TURN_ORDER[num] == eFace.Up)) ? 1 : (-1))));
				num += num4;
				if (num < 0)
				{
					num = TURN_ORDER.Length - 1;
				}
				else if (num >= TURN_ORDER.Length)
				{
					num = 0;
				}
				eFace eFace2 = TURN_ORDER[num];
				bool num5 = Flipped();
				string clipName = m_spriteAnimator.ClipName;
				m_data.SetFaceDirection(eFace2);
				UpdateFacingVisuals(eFace2);
				flag2 = num5 != Flipped();
				flag2 |= clipName != m_spriteAnimator.ClipName;
			}
			if (!CheckTargetDirectionWillChangeAnim())
			{
				m_data.SetFaceDirection(targetFaceDirection);
			}
		}

		public void PlayAnimation(string animName)
		{
			PlayAnimInternal(animName);
			m_animating = true;
			OnAnimStateChange();
		}

		public void PauseAnimation()
		{
			if (m_spriteAnimator != null && m_state == Character.eState.Animate)
			{
				m_spriteAnimator.Pause();
			}
		}

		public void ResumeAnimation()
		{
			if (m_spriteAnimator != null && m_state == Character.eState.Animate)
			{
				m_spriteAnimator.Resume();
			}
		}

		public void StopAnimation()
		{
			if (m_animating)
			{
				if (!string.IsNullOrEmpty(m_transitioningToAnim))
				{
					OnTransitionAnimComplete();
				}
				m_animating = false;
				OnAnimStateChange();
			}
		}

		public void SkipTransition()
		{
			m_skipTransitionNextFrame = true;
			if (m_data.LoopStartTime >= 0f && m_data.AnimationTime < m_data.LoopStartTime)
			{
				m_spriteAnimator.NormalizedTime = m_data.LoopStartTime - 0.002f;
			}
			if (!string.IsNullOrEmpty(m_transitioningToAnim))
			{
				OnTransitionAnimComplete();
			}
			if (!string.IsNullOrEmpty(m_playAfterTurnAnim))
			{
				string playAfterTurnAnim = m_playAfterTurnAnim;
				m_playAfterTurnAnim = null;
				PlayAnimInternal(playAfterTurnAnim);
			}
		}

		public bool GetPlayingTransition()
		{
			if (m_data.LoopStartTime >= 0f && m_spriteAnimator.NormalizedTime < m_data.LoopStartTime - 0.01f)
			{
				return true;
			}
			if (!IsString.Set(m_transitioningToAnim))
			{
				return IsString.Set(m_playAfterTurnAnim);
			}
			return true;
		}

		public void OnAnimationChanged(Character.eState animState = Character.eState.None)
		{
			if (animState == Character.eState.None)
			{
				animState = m_state;
			}
			if (animState != m_state)
			{
				return;
			}
			switch (m_state)
			{
			case Character.eState.Idle:
				PlayAnimInternal(m_data.AnimIdle);
				break;
			case Character.eState.Walk:
				if (m_playWalkAnim)
				{
					PlayAnimInternal(m_data.AnimWalk, fromStart: false);
				}
				break;
			case Character.eState.Talk:
				PlayAnimInternal(m_data.AnimTalk);
				break;
			}
			UpdateMouthAnim();
		}

		public void UpdateMouthAnim()
		{
			if (m_mouth != null)
			{
				bool activeSelf = m_mouth.gameObject.activeSelf;
				if (!activeSelf)
				{
					m_mouth.gameObject.SetActive(value: true);
				}
				m_mouth.Play(FindDirectionalAnimation(m_data.AnimMouth, out var _));
				m_mouth.Pause();
				if (!activeSelf)
				{
					m_mouth.gameObject.SetActive(value: false);
				}
			}
		}

		public void OnClickableColliderIdChanged()
		{
			if (m_clickableColliders == null || m_data.UseSpriteAsHotspot)
			{
				return;
			}
			for (int i = 0; i < m_clickableColliders.Length; i++)
			{
				if (m_clickableColliders[i] != null)
				{
					m_clickableColliders[i].enabled = i == m_data.ClickableColliderId;
				}
			}
		}

		public void StopWalk()
		{
			CancelWalk();
		}

		public void CancelWalk()
		{
			m_path = null;
			m_pathPointNext = -1;
			m_targetPos = m_data.GetPosition();
			m_targetEndPos = m_targetPos;
			m_turningToWalk = false;
			m_playWalkAnim = true;
			m_walking = false;
			OnAnimStateChange();
		}

		public void SkipWalk()
		{
			if (!Walking)
			{
				return;
			}
			if (m_path == null)
			{
				m_data.SetPosition(m_targetPos);
			}
			else
			{
				m_data.SetPosition(m_path[m_path.Length - 1]);
				Vector2 normalized = (m_path[m_path.Length - 1] - m_path[m_path.Length - 2]).normalized;
				if (normalized.sqrMagnitude > Mathf.Epsilon)
				{
					m_data.FaceDirection(normalized, instant: true);
				}
			}
			m_path = null;
			m_pathPointNext = -1;
			m_walking = false;
			OnAnimStateChange();
			if (m_data.GetFaceAfterWalk() != eFace.None)
			{
				m_data.FaceBG(m_data.GetFaceAfterWalk());
			}
		}

		public void UpdateFacingVisuals(eFace direction)
		{
			if (direction != m_facing || m_fallbackDirection != m_data.GetFacingVerticalFallback())
			{
				m_facing = direction;
				m_fallbackDirection = m_data.GetFacingVerticalFallback();
				if (!string.IsNullOrEmpty(m_currAnimBaseName))
				{
					PlayAnimInternal(m_currAnimBaseName, fromStart: false);
				}
				if (m_mouth != null && !string.IsNullOrEmpty(m_data.AnimMouth))
				{
					UpdateMouthAnim();
				}
			}
		}

		public void MoveToWalkableArea()
		{
			if (!Singleton<PowerQuest>.Get.Pathfinder.IsPointInArea(m_data.GetPosition()))
			{
				m_data.SetPosition(Singleton<PowerQuest>.Get.Pathfinder.GetClosestPointToArea(m_data.GetPosition()));
			}
		}

		public void WalkTo(Vector2 pos, bool anywhere, bool playWalkAnim, bool couldntFindPath = false)
		{
			if (m_state == Character.eState.Walk && m_targetEndPos == pos && m_playWalkAnim == playWalkAnim)
			{
				return;
			}
			m_playWalkAnim = playWalkAnim;
			Pathfinder pathfinder = Singleton<PowerQuest>.Get.Pathfinder;
			if (!anywhere && pathfinder.GetValid())
			{
				foreach (Character character in Singleton<PowerQuest>.Get.GetCharacters())
				{
					if (character.Instance != null)
					{
						if (GetData().Solid && character != GetData() && character.Solid && character.Room == Singleton<PowerQuest>.Get.GetCurrentRoom())
						{
							pathfinder.EnableObstacle(character.Instance.transform);
						}
						else
						{
							pathfinder.DisableObstacle(character.Instance.transform);
						}
					}
				}
				Vector2 to = pos;
				if (!pathfinder.IsPointInArea(pos))
				{
					pos = pathfinder.GetClosestPointToArea(pos);
				}
				m_targetPos = pos;
				m_targetEndPos = m_targetPos;
				Vector2[] array = pathfinder.FindPath(m_data.Position, pos);
				if (array != null && array.Length > 1)
				{
					m_path = array;
					m_pathPointNext = 1;
					WalkToInternal(m_path[1]);
				}
				else
				{
					if (array != null && array.Length != 0)
					{
						return;
					}
					if (!couldntFindPath)
					{
						RoomComponent instance = Singleton<PowerQuest>.Get.GetCurrentRoom().Instance;
						if (instance != null)
						{
							pos = instance.GetClosestPoint(m_data.Position, to);
						}
						WalkTo(pos, anywhere, playWalkAnim, couldntFindPath: true);
					}
					else if (GetData().Solid)
					{
						Debug.Log("Couldn't Find Path, trying with 'solid' flag off");
						GetData().Solid = false;
						WalkTo(pos, anywhere, playWalkAnim, couldntFindPath);
						GetData().Solid = true;
					}
					else
					{
						Debug.Log("Couldn't Find Path.");
					}
				}
			}
			else
			{
				m_path = null;
				m_pathPointNext = -1;
				WalkToInternal(pos);
				m_targetEndPos = pos;
			}
		}

		public void StartSay(string text, int id)
		{
			m_currLineId = id;
			m_talking = true;
			OnAnimStateChange();
		}

		public void EndSay()
		{
			m_currLineId = -1;
			m_talking = false;
			OnAnimStateChange();
		}

		public Vector2 GetTextPosition()
		{
			if (m_data.TextPositionOverride != Vector2.zero)
			{
				return m_data.TextPositionOverride;
			}
			if (m_sprite == null || m_sprite.sprite == null || !m_sprite.enabled)
			{
				return m_data.Position;
			}
			float maxHeight = 0f;
			if (m_sprite != null && m_sprite.sprite != null && m_sprite.enabled)
			{
				maxHeight = float.MinValue;
				Array.ForEach(m_sprite.sprite.vertices, delegate(Vector2 item)
				{
					if (item.y > maxHeight)
					{
						maxHeight = item.y;
					}
				});
				if (m_powerSprite != null)
				{
					maxHeight += m_powerSprite.Offset.y;
				}
			}
			maxHeight *= base.transform.localScale.y;
			return (Vector2)m_sprite.transform.position + new Vector2(0f, maxHeight) + Singleton<PowerQuest>.Get.GetDialogTextOffset() + m_data.TextPositionOffset.Scaled(base.transform.localScale);
		}

		public void OnSkipCutscene()
		{
			if (m_state == Character.eState.Walk && m_targetPos != m_data.GetPosition() && m_targetPos != Vector2.zero)
			{
				m_data.SetPosition(m_targetPos);
				m_targetPos = Vector2.zero;
				m_targetEndPos = m_data.Position;
				CancelWalk();
				if (m_data.GetFaceAfterWalk() != eFace.None)
				{
					m_data.Facing = m_data.GetFaceAfterWalk();
				}
			}
		}

		private void WalkToInternal(Vector2 pos)
		{
			Vector2 directionV = pos - m_data.Position;
			if (directionV.sqrMagnitude < Mathf.Epsilon)
			{
				return;
			}
			m_targetPos = pos;
			bool walking = m_walking;
			m_walking = true;
			if (IsString.Set(m_transitioningToAnim))
			{
				return;
			}
			OnAnimStateChange();
			if (!Animating && m_playWalkAnim && !walking)
			{
				m_data.FaceDirection(directionV);
				if (CheckTargetDirectionWillChangeAnim())
				{
					m_turningToWalk = true;
					PlayAnimInternal(m_data.AnimIdle);
				}
				else
				{
					m_data.SetFaceDirection(m_data.GetTargetFaceDirection());
				}
			}
		}

		private bool CheckTargetDirectionWillChangeAnim()
		{
			bool flip;
			eFace animFacing;
			string text = FindDirectionalAnimationName(m_data.GetTargetFaceDirection(), m_currAnimBaseName, out flip, out animFacing);
			if (flip == Flipped())
			{
				return text != m_spriteAnimator.ClipName;
			}
			return true;
		}

		private void SetState(Character.eState state)
		{
			Character.eState state2 = m_state;
			OnExitState(state2, state);
			m_state = state;
			OnEnterState(state2, state);
		}

		private void PlayAnimAfterTurn(string anim)
		{
			if (m_currTurnAnim != null)
			{
				m_playAfterTurnAnim = anim;
			}
			else
			{
				PlayAnimInternal(anim);
			}
		}

		private void OnEnterState(Character.eState oldState, Character.eState newState)
		{
			switch (newState)
			{
			case Character.eState.Idle:
				if (m_playIdleDelayFrames <= 0)
				{
					PlayAnimAfterTurn(m_data.AnimIdle);
				}
				break;
			case Character.eState.Walk:
				if (m_playWalkAnim)
				{
					if (!PlayAnimInternal(m_data.AnimWalk))
					{
						PlayAnimInternal(m_data.AnimIdle);
					}
					m_playIdleDelayFrames = 2;
				}
				break;
			case Character.eState.Talk:
				if (IsString.Set(m_data.AnimTalk) && HasAnimation(m_data.AnimTalk))
				{
					PlayAnimAfterTurn(m_data.AnimTalk);
				}
				else
				{
					PlayAnimAfterTurn(m_data.AnimIdle);
				}
				if (m_data.LipSyncEnabled && m_mouthNode == null)
				{
					m_spriteAnimator.Pause();
				}
				break;
			case Character.eState.Animate:
				m_playIdleDelayFrames = 2;
				break;
			}
		}

		private void OnExitState(Character.eState oldState, Character.eState newState)
		{
			switch (oldState)
			{
			}
		}

		private void OnAnimationReset()
		{
			AnimWalkSpeedReset();
		}

		private bool HasAnimation(string animName)
		{
			string text = animName;
			if (!string.IsNullOrEmpty(GetData().AnimPrefix))
			{
				animName = GetData().AnimPrefix + animName;
			}
			bool flip;
			AnimationClip animationClip = FindDirectionalAnimation(animName, out flip);
			if (animationClip == null && IsString.Set(GetData().AnimPrefix))
			{
				animName = text;
				animationClip = FindDirectionalAnimation(animName, out flip);
			}
			return animationClip != null;
		}

		private bool PlayAnimInternal(string animName, bool fromStart = true)
		{
			string text = animName;
			if (!string.IsNullOrEmpty(GetData().AnimPrefix))
			{
				animName = GetData().AnimPrefix + animName;
			}
			if (IsString.Set(m_transitionAnim) && m_transitionAnim.StartsWithIgnoreCase(animName))
			{
				return true;
			}
			if (m_currTurnAnim != null && animName != m_currTurnAnim && text != m_currTurnAnim)
			{
				m_currTurnAnim = null;
				m_playAfterTurnAnim = null;
			}
			bool flag = false;
			string text2 = m_spriteAnimator.ClipName;
			bool skipTransitionNextFrame = m_skipTransitionNextFrame;
			m_skipTransitionNextFrame = false;
			bool flip;
			AnimationClip animationClip = FindDirectionalAnimation(animName, out flip);
			if (animationClip == null && IsString.Set(GetData().AnimPrefix))
			{
				animName = text;
				animationClip = FindDirectionalAnimation(animName, out flip);
			}
			if (!skipTransitionNextFrame && IsString.Set(m_transitionAnim) && !m_transitionAnim.StartsWithIgnoreCase(animName) && animationClip != null && text2 != animationClip.name)
			{
				string transitionAnim = GetTransitionAnim(m_transitioningFromAnim, m_flippedLastUpdate, animationClip.name, flip);
				if (m_animChangeTime < 0.05f && IsString.Set(transitionAnim) && m_transitionAnim.StartsWithIgnoreCase(transitionAnim))
				{
					flag = true;
					text2 = m_transitioningFromAnim;
				}
				else if (m_data.LoopStartTime < 0f && m_data.LoopEndTime < 0f)
				{
					m_transitioningFromAnim = null;
					m_transitioningToAnim = null;
					m_transitionAnim = null;
				}
			}
			if (animName.Equals(m_transitioningToAnim))
			{
				m_transitioningFromAnim = null;
				m_transitioningToAnim = null;
				m_transitionAnim = null;
			}
			if (!skipTransitionNextFrame && animationClip != null && text2 != animationClip.name)
			{
				string transitionAnim2 = GetTransitionAnim(text2, m_flippedLastUpdate, animationClip.name, flip);
				if (IsString.Set(transitionAnim2))
				{
					animationClip = FindDirectionalAnimation(transitionAnim2, out flip);
					if (animationClip != null)
					{
						m_transitioningFromAnim = text2;
						m_transitioningToAnim = animName;
						animName = transitionAnim2;
						m_transitionAnim = animationClip.name;
					}
				}
				else if (m_data.LoopStartTime > 0f && m_data.LoopEndTime > 0f)
				{
					if (!(m_animChangeTime <= 0f))
					{
						if (!flag && m_data.LoopEndTime > m_data.LoopStartTime)
						{
							m_spriteAnimator.NormalizedTime = m_data.LoopEndTime;
						}
						m_transitioningFromAnim = text2;
						m_transitionAnim = text2;
						m_transitioningToAnim = animName;
						if (m_spriteAnimator.Paused)
						{
							m_spriteAnimator.Resume();
						}
						return true;
					}
					m_data.LoopStartTime = -1f;
					m_data.LoopEndTime = -1f;
				}
			}
			if (animationClip != null)
			{
				m_currAnimBaseName = animName;
				if (text2 != animationClip.name)
				{
					m_data.LoopStartTime = -1f;
					m_data.LoopEndTime = -1f;
					m_animChangeTime = 0f;
					if (fromStart || m_spriteAnimator.Clip == null)
					{
						m_spriteAnimator.Play(animationClip);
					}
					else
					{
						float normalizedTime = Mathf.Clamp01(m_spriteAnimator.NormalizedTime + Time.deltaTime);
						m_spriteAnimator.Play(animationClip);
						m_spriteAnimator.NormalizedTime = normalizedTime;
					}
					m_data.LoopStartTime = FindLoopStartEvent();
					m_data.LoopEndTime = FindLoopEndEvent();
				}
			}
			if (flip != Flipped())
			{
				base.transform.localScale = new Vector3(0f - base.transform.localScale.x, base.transform.localScale.y, base.transform.localScale.z);
			}
			if (animationClip == null && Singleton<PowerQuest>.Get.IsDebugBuild && animName != "Idle" && animName != "Talk" && animName != "Walk" && !string.IsNullOrEmpty(animName))
			{
				Debug.Log("Failed to find animation: " + animName, base.gameObject);
			}
			return animationClip != null;
		}

		private bool Flipped()
		{
			return Mathf.Sign(base.transform.localScale.x) < 0f;
		}

		private AnimationClip FindDirectionalAnimation(string name, out bool flip)
		{
			string finalName = FindDirectionalAnimationName(name, out flip);
			if (string.IsNullOrEmpty(finalName))
			{
				return null;
			}
			return GetAnimations().Find((AnimationClip item) => string.Equals(finalName, (item == null) ? null : item.name, StringComparison.OrdinalIgnoreCase));
		}

		private string FindDirectionalAnimationName(string name, out bool flip)
		{
			eFace animFacing = eFace.None;
			return FindDirectionalAnimationName(m_data.Facing, name, out flip, out animFacing);
		}

		private string FindDirectionalAnimationName(eFace facing, string name, out bool flip, out eFace animFacing)
		{
			if (name == null)
			{
				name = string.Empty;
			}
			animFacing = eFace.None;
			flip = false;
			BitMask availableDirections = default(BitMask);
			int length = name.Length;
			int num = name.Length + 1;
			int num2 = name.Length + 2;
			for (int i = 0; i < GetAnimations().Count; i++)
			{
				if (GetAnimations()[i] == null)
				{
					continue;
				}
				string text = GetAnimations()[i].name;
				bool flag = text.Length == length;
				bool flag2 = text.Length == num;
				bool flag3 = text.Length == num2;
				if (!(flag2 || flag3 || flag) || !text.StartsWith(name, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				if (flag2)
				{
					for (int j = 0; j <= 3; j++)
					{
						if (text[length] == DIRECTION_POSTFIX[j][0])
						{
							availableDirections.SetAt(j);
							break;
						}
					}
				}
				else if (flag3)
				{
					string text2 = text.Substring(length);
					for (int k = 4; k <= 7; k++)
					{
						if (text2 == DIRECTION_POSTFIX[k])
						{
							availableDirections.SetAt(k);
							break;
						}
					}
				}
				else
				{
					availableDirections.SetAt(DIRECTION_COUNT);
				}
			}
			bool flag4 = facing != eFace.Up && facing != eFace.Down;
			int num3;
			switch (facing)
			{
			default:
				num3 = ((facing == eFace.Down) ? 1 : 0);
				break;
			case eFace.Up:
				num3 = 1;
				break;
			case eFace.Left:
			case eFace.Right:
				num3 = 1;
				break;
			}
			bool flag5 = (byte)num3 != 0;
			if (availableDirections.Value == 0)
			{
				flip = (flag4 ? ToCardinal(facing) : m_data.GetFacingVerticalFallback()) == eFace.Left;
				return null;
			}
			if (availableDirections.Value == 1 << DIRECTION_COUNT)
			{
				return name;
			}
			if (TryGetAnimName(facing, ref name, ref flip, availableDirections, ref animFacing))
			{
				return name;
			}
			if (flag4)
			{
				if (flag5)
				{
					if (TryGetAnimName(ToDiagDown(facing), ref name, ref flip, availableDirections, ref animFacing))
					{
						return name;
					}
					if (TryGetAnimName(ToDiagUp(facing), ref name, ref flip, availableDirections, ref animFacing))
					{
						return name;
					}
				}
				if (!flag5)
				{
					if (TryGetAnimName(ToCardinal(facing), ref name, ref flip, availableDirections, ref animFacing))
					{
						return name;
					}
					if (TryGetAnimName(FlipV(facing), ref name, ref flip, availableDirections, ref animFacing))
					{
						return name;
					}
				}
			}
			else
			{
				switch (facing)
				{
				case eFace.Up:
					if (TryGetAnimName(ToDiagUp(m_data.GetFacingVerticalFallback()), ref name, ref flip, availableDirections, ref animFacing))
					{
						return name;
					}
					if (TryGetAnimName(ToCardinal(m_data.GetFacingVerticalFallback()), ref name, ref flip, availableDirections, ref animFacing))
					{
						return name;
					}
					if (TryGetAnimName(ToDiagDown(m_data.GetFacingVerticalFallback()), ref name, ref flip, availableDirections, ref animFacing))
					{
						return name;
					}
					break;
				case eFace.Down:
					if (TryGetAnimName(ToDiagDown(m_data.GetFacingVerticalFallback()), ref name, ref flip, availableDirections, ref animFacing))
					{
						return name;
					}
					if (TryGetAnimName(ToCardinal(m_data.GetFacingVerticalFallback()), ref name, ref flip, availableDirections, ref animFacing))
					{
						return name;
					}
					if (TryGetAnimName(ToDiagUp(m_data.GetFacingVerticalFallback()), ref name, ref flip, availableDirections, ref animFacing))
					{
						return name;
					}
					break;
				}
			}
			if (TryGetAnimName(eFace.Down, ref name, ref flip, availableDirections, ref animFacing))
			{
				return name;
			}
			if (TryGetAnimName(eFace.Up, ref name, ref flip, availableDirections, ref animFacing))
			{
				return name;
			}
			if (availableDirections.IsSet(DIRECTION_COUNT))
			{
				return name;
			}
			return null;
		}

		private static bool TryGetAnimName(eFace facing, ref string name, ref bool flip, BitMask availableDirections, ref eFace setFacingIfFound)
		{
			if (availableDirections.IsSet((int)facing))
			{
				name += DIRECTION_POSTFIX[(int)facing];
				setFacingIfFound = facing;
				return true;
			}
			if (availableDirections.IsSet((int)FlipH(facing)))
			{
				flip = true;
				name += DIRECTION_POSTFIX[(int)FlipH(facing)];
				setFacingIfFound = facing;
				return true;
			}
			return false;
		}

		private static eFace FlipH(eFace original)
		{
			return original switch
			{
				eFace.Right => eFace.Left, 
				eFace.Left => eFace.Right, 
				eFace.UpLeft => eFace.UpRight, 
				eFace.UpRight => eFace.UpLeft, 
				eFace.DownLeft => eFace.DownRight, 
				eFace.DownRight => eFace.DownLeft, 
				_ => original, 
			};
		}

		private static eFace FlipV(eFace original)
		{
			return original switch
			{
				eFace.Up => eFace.Down, 
				eFace.UpLeft => eFace.DownRight, 
				eFace.UpRight => eFace.DownRight, 
				eFace.Down => eFace.Down, 
				eFace.DownLeft => eFace.UpLeft, 
				eFace.DownRight => eFace.UpRight, 
				_ => original, 
			};
		}

		private static eFace ToDiagDown(eFace cardinalH)
		{
			return cardinalH switch
			{
				eFace.Right => eFace.DownRight, 
				eFace.Left => eFace.DownLeft, 
				eFace.UpLeft => eFace.DownLeft, 
				eFace.UpRight => eFace.DownRight, 
				_ => cardinalH, 
			};
		}

		private static eFace ToDiagUp(eFace cardinalH)
		{
			return cardinalH switch
			{
				eFace.Right => eFace.UpRight, 
				eFace.Left => eFace.UpLeft, 
				eFace.DownLeft => eFace.UpLeft, 
				eFace.DownRight => eFace.UpRight, 
				_ => cardinalH, 
			};
		}

		public static eFace ToCardinal(eFace diagonal)
		{
			return diagonal switch
			{
				eFace.UpLeft => eFace.Left, 
				eFace.UpRight => eFace.Right, 
				eFace.DownLeft => eFace.Left, 
				eFace.DownRight => eFace.Right, 
				_ => diagonal, 
			};
		}

		private void AnimSound(UnityEngine.Object obj)
		{
			if (!(obj == null) && !(obj as GameObject == null) && m_data != null && m_data.VisibleInRoom)
			{
				SystemAudio.Play((obj as GameObject).GetComponent<AudioCue>(), base.transform);
			}
		}

		private void AnimSound(string sound)
		{
			if (m_data != null && m_data.VisibleInRoom)
			{
				SystemAudio.Play(sound, base.transform);
			}
		}

		private void AnimFootstep()
		{
			if (m_data != null && m_data.VisibleInRoom)
			{
				SystemAudio.Play(m_data.FootstepSound, base.transform);
			}
		}

		private void AnimMouth(string animName)
		{
			m_data.AnimMouth = animName;
		}

		private float FindLoopStartEvent()
		{
			if (m_spriteAnimator.ClipName == m_transitionAnim)
			{
				return -1f;
			}
			AnimationEvent[] events = m_spriteAnimator.Clip.events;
			foreach (AnimationEvent animationEvent in events)
			{
				if (animationEvent.functionName.Contains("LoopStart") || animationEvent.stringParameter.Contains("LoopStart"))
				{
					return animationEvent.time / m_spriteAnimator.Clip.length + 0.001f;
				}
			}
			events = m_spriteAnimator.Clip.events;
			foreach (AnimationEvent animationEvent2 in events)
			{
				if (animationEvent2.functionName.Contains("Loop") || animationEvent2.stringParameter.Contains("Loop"))
				{
					return animationEvent2.time / m_spriteAnimator.Clip.length + 0.001f;
				}
			}
			return -1f;
		}

		private float FindLoopEndEvent()
		{
			if (m_spriteAnimator.ClipName == m_transitionAnim)
			{
				return -1f;
			}
			AnimationEvent[] events = m_spriteAnimator.Clip.events;
			foreach (AnimationEvent animationEvent in events)
			{
				if (animationEvent.functionName.Contains("LoopEnd") || animationEvent.stringParameter.Contains("LoopEnd"))
				{
					return animationEvent.time / m_spriteAnimator.Clip.length + 0.001f;
				}
			}
			events = m_spriteAnimator.Clip.events;
			foreach (AnimationEvent animationEvent2 in events)
			{
				if ((animationEvent2.functionName.Contains("Loop") || animationEvent2.stringParameter.Contains("Loop")) && !animationEvent2.functionName.Contains("Start") && !animationEvent2.stringParameter.Contains("Start"))
				{
					return animationEvent2.time / m_spriteAnimator.Clip.length + 0.001f;
				}
			}
			return -1f;
		}

		private void AnimLoopStart()
		{
		}

		private void AnimLoopEnd()
		{
			if (!(m_spriteAnimator.ClipName == m_transitionAnim))
			{
				if (m_data.LoopEndTime <= 0f)
				{
					m_data.LoopEndTime = m_spriteAnimator.NormalizedTime;
				}
				m_spriteAnimator.NormalizedTime = m_data.LoopStartTime;
			}
		}

		private void AnimLoop()
		{
			if (!(m_spriteAnimator.ClipName == m_transitionAnim))
			{
				m_spriteAnimator.NormalizedTime = m_data.LoopEndTime;
				m_spriteAnimator.Pause();
			}
		}

		private void AnimOffset()
		{
			SpriteAnimNodes component = m_data.Instance.GetComponent<SpriteAnimNodes>();
			Vector2 vector = component.GetPosition(1);
			Vector2 vector2 = component.GetPosition(2);
			m_data.SetPosition(m_data.GetPosition() + (vector - vector2));
		}

		private void AnimWalkSpeed(float speed)
		{
			AnimWalkSpeedX(speed);
			AnimWalkSpeedY(speed);
		}

		private void AnimWalkSpeedX(float speed)
		{
			m_walkSpeedOverride.x = ((speed > -1f && speed < 1f) ? (m_data.WalkSpeed.x * speed) : speed);
		}

		private void AnimWalkSpeedY(float speed)
		{
			m_walkSpeedOverride.y = ((speed > -1f && speed < 1f) ? (m_data.WalkSpeed.y * speed) : speed);
		}

		private void AnimWalkSpeedReset()
		{
			m_walkSpeedOverride = -Vector2.one;
		}

		private void AnimSpawn(GameObject obj)
		{
			if (obj != null)
			{
				UnityEngine.Object.Instantiate(obj, base.transform.position, Quaternion.identity);
			}
		}

		public void AnimShadowOff()
		{
			m_animShadowOff = true;
			UpdateShadow();
		}

		public void AnimShadowOn()
		{
			AnimShadowReset();
		}

		public void AnimShadowReset()
		{
			m_animShadowOff = false;
			UpdateShadow();
		}

		private void UpdateLipSync()
		{
			if (!m_data.LipSyncEnabled)
			{
				return;
			}
			bool flag = !string.IsNullOrEmpty(m_data.AnimMouth);
			if (flag && m_mouth == null)
			{
				GameObject gameObject = new GameObject("Mouth", typeof(PowerSprite), typeof(SpriteAnim));
				m_mouth = gameObject.GetComponent<SpriteAnim>();
				if (m_mouthNode == null)
				{
					m_mouthNode = base.gameObject.GetComponent<SpriteAnimNodes>();
				}
				if (m_mouthNode == null)
				{
					m_mouthNode = base.gameObject.AddComponent<SpriteAnimNodes>();
				}
				gameObject.transform.SetParent(base.transform, worldPositionStays: false);
				m_mouth.Play(FindDirectionalAnimation(m_data.AnimMouth, out var _));
				m_mouth.Pause();
				if (m_mouthNode == null)
				{
					flag = false;
				}
				if (!m_animating)
				{
					m_spriteAnimator.Play(m_spriteAnimator.GetCurrentAnimation());
				}
			}
			if (Talking)
			{
				if (flag && m_mouthNode.GetPositionRaw(0) == Vector2.zero)
				{
					m_mouth.gameObject.SetActive(value: false);
					return;
				}
				if (flag)
				{
					m_mouth.gameObject.SetActive(value: true);
					m_mouth.GetComponent<SpriteRenderer>().sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 1;
				}
				SpriteAnim spriteAnim = (flag ? m_mouth : m_spriteAnimator);
				TextData textData = SystemText.FindTextData(m_currLineId, m_data.ScriptName);
				float time = 0f;
				if (m_data.GetDialogAudioSource() != null)
				{
					time = m_data.GetDialogAudioSource().time - 0.1f;
				}
				int num = -1;
				if (textData != null)
				{
					num = Array.FindIndex(textData.m_phonesTime, (float item) => item > time);
				}
				num--;
				char c = (Singleton<SystemText>.Get.GetLipsyncUsesXShape() ? 'X' : 'A');
				if (num >= 0 && num < textData.m_phonesCharacter.Length)
				{
					c = textData.m_phonesCharacter[num];
				}
				int num2 = NUM_LIP_SYNC_FRAMES + Singleton<SystemText>.Get.GetLipsyncExtendedMouthShapes().Length;
				float normalizedTime = ((float)Mathf.Min(c - 65, num2 - 1) + 0.5f) / (float)num2;
				if (textData == null || textData.m_phonesTime.Length == 0)
				{
					normalizedTime = ((!Utils.GetTimeIncrementPassed(0.1f) || !(UnityEngine.Random.value > 0.2f)) ? spriteAnim.NormalizedTime : UnityEngine.Random.value);
				}
				spriteAnim.SetNormalizedTime(normalizedTime);
				spriteAnim.Pause();
				if (flag)
				{
					_ = m_mouth.transform;
					Vector2 vector = m_mouthNode.GetPosition(0);
					if (m_powerSprite != null)
					{
						Vector2 offset = m_powerSprite.Offset;
						offset.Scale(base.transform.localScale);
						vector += offset;
					}
					m_mouth.transform.position = vector;
					if (m_mouthNode.GetAngleRaw(0) > 90f)
					{
						m_mouth.transform.localScale = new Vector3(-1f, 1f, 1f);
					}
					else
					{
						m_mouth.transform.localScale = Vector3.one;
					}
				}
			}
			else if (m_mouth != null)
			{
				m_mouth.gameObject.SetActive(value: false);
			}
		}
	}
}
