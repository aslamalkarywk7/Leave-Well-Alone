using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace PowerTools.Quest
{
	public class PropComponent : MonoBehaviour
	{
		[SerializeField]
		private Prop m_data = new Prop();

		[Header("Parallax")]
		[Parallax]
		[SerializeField]
		private float m_parallaxDepth;

		[Tooltip("eg: (-1,0) means it's drawn clamped to left middle of screen")]
		[SerializeField]
		private Vector2 m_parallaxAlignment = Vector2.zero;

		private SpriteRenderer m_sprite;

		private SpriteAnim m_spriteAnimator;

		private VideoPlayer m_video;

		private bool m_moving;

		private bool m_overrideAnimPlaying;

		private int m_stopOverrideAnimDelay = -1;

		private Vector2 m_snapOffset = Vector2.zero;

		private ParticleSystem m_particle;

		private Renderer[] m_renderers;

		private SpriteRenderer[] m_sprites;

		private QuestText[] m_questTexts;

		private bool m_hasCollider;

		private QuestAnimationTriggers m_animTriggerComponent;

		public bool Moving => m_moving;

		public SpriteRenderer[] Sprites => m_sprites;

		public QuestText[] QuestTexts => m_questTexts;

		public Vector2 ParallaxOffset
		{
			get
			{
				return m_parallaxAlignment;
			}
			set
			{
				m_parallaxAlignment = value;
			}
		}

		public Prop GetData()
		{
			return m_data;
		}

		public void SetData(Prop data)
		{
			m_data = data;
			base.transform.position = m_data.Position.WithZ(base.transform.position.z);
			OnAnimationChanged();
			OnSetVisible();
			m_moving = false;
		}

		public SpriteRenderer GetSprite()
		{
			return m_sprite;
		}

		public SpriteAnim GetSpriteAnimator()
		{
			return m_spriteAnimator;
		}

		public bool GetAnimating()
		{
			if (m_overrideAnimPlaying)
			{
				return m_spriteAnimator.Playing;
			}
			return false;
		}

		public void PlayAnimation(string animName)
		{
			if (!PlayAnimInternal(animName) && Singleton<PowerQuest>.Get.IsDebugBuild)
			{
				Debug.LogWarning("Failed to find prop animation: " + animName);
			}
			m_overrideAnimPlaying = true;
		}

		public void PauseAnimation()
		{
			m_spriteAnimator.Pause();
		}

		public void ResumeAnimation()
		{
			m_spriteAnimator.Resume();
		}

		public void StopAnimation()
		{
			if (m_overrideAnimPlaying)
			{
				PlayAnimInternal(GetData().Animation);
			}
			m_overrideAnimPlaying = false;
		}

		public void OnAnimationChanged()
		{
			m_stopOverrideAnimDelay = 0;
			if (!m_overrideAnimPlaying && !string.IsNullOrEmpty(GetData().Animation))
			{
				PlayAnimInternal(GetData().Animation);
			}
		}

		public VideoPlayer GetVideoPlayer()
		{
			return m_video;
		}

		private void Awake()
		{
			SetupComponents();
		}

		public void SetupComponents()
		{
			if (m_sprites == null || m_renderers == null)
			{
				m_sprites = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
				m_questTexts = GetComponentsInChildren<QuestText>(includeInactive: true);
				m_renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
				m_sprite = GetComponentInChildren<SpriteRenderer>(includeInactive: true);
				m_particle = GetComponentInChildren<ParticleSystem>();
				m_hasCollider = GetComponentInChildren<Collider2D>(includeInactive: true) != null;
				if (m_sprite != null)
				{
					m_spriteAnimator = m_sprite.GetComponent<SpriteAnim>();
				}
				m_video = GetComponentInChildren<VideoPlayer>(includeInactive: true);
			}
		}

		private void Start()
		{
			if ((bool)m_video)
			{
				m_video.Prepare();
			}
			OnAnimationChanged();
			OnSetVisible();
		}

		public float GetParallax()
		{
			return m_parallaxDepth;
		}

		public void OnSetVisible()
		{
			if (!base.gameObject.activeSelf && GetData().Visible)
			{
				base.gameObject.SetActive(value: true);
			}
			SetupComponents();
			Renderer[] renderers = m_renderers;
			foreach (Renderer renderer in renderers)
			{
				if (renderer != null)
				{
					renderer.GetComponent<Renderer>().enabled = GetData().Visible;
				}
			}
			if (GetData().Visible)
			{
				UpdateBaseline();
				UpdateAlpha();
				ResumeAnimation();
			}
			else
			{
				PauseAnimation();
			}
			if (m_particle != null)
			{
				ParticleSystem.EmissionModule emission = m_particle.emission;
				emission.enabled = GetData().Visible;
				if (GetData().Visible && !m_particle.isPlaying)
				{
					m_particle.Play();
				}
				m_particle.GetComponent<Renderer>().sortingLayerName = "Default";
			}
		}

		public void OnSetPosition()
		{
			base.transform.position = m_data.Position.WithZ(base.transform.position.z);
			UpdateParallax();
			UpdateBaseline();
		}

		public void UpdateBaseline()
		{
			int sortOrder = GetData().SortOrder;
			if (m_sprites != null)
			{
				Array.ForEach(m_sprites, delegate(SpriteRenderer sprite)
				{
					if (sprite != null)
					{
						sprite.sortingOrder = sortOrder;
					}
				});
			}
			if (m_questTexts != null)
			{
				Array.ForEach(m_questTexts, delegate(QuestText text)
				{
					if (text != null)
					{
						text.OrderInLayer = sortOrder;
					}
				});
			}
			if (m_particle != null)
			{
				m_particle.GetComponent<Renderer>().sortingOrder = GetData().SortOrder;
			}
		}

		public void UpdateAlpha()
		{
			float alpha = GetData().Alpha;
			if (m_sprites != null)
			{
				Array.ForEach(m_sprites, delegate(SpriteRenderer sprite)
				{
					if (sprite != null)
					{
						sprite.color = sprite.color.WithAlpha(alpha);
					}
				});
			}
			if (m_questTexts == null)
			{
				return;
			}
			Array.ForEach(m_questTexts, delegate(QuestText text)
			{
				if (text != null)
				{
					text.color = text.color.WithAlpha(alpha);
				}
			});
		}

		public void OnLoadComplete()
		{
			m_hasCollider = GetComponentInChildren<Collider2D>(includeInactive: true) != null;
			if (m_data != null && !m_hasCollider && m_data.Clickable)
			{
				m_data.Clickable = false;
			}
			LateUpdate();
		}

		public bool GetHasCollider()
		{
			return m_hasCollider;
		}

		private void Update()
		{
			if (m_overrideAnimPlaying && Singleton<PowerQuest>.Get.GetSkippingCutscene() && !m_spriteAnimator.GetCurrentAnimation().isLooping)
			{
				StopAnimation();
			}
		}

		private void LateUpdate()
		{
			UpdateParallax();
			if (m_stopOverrideAnimDelay > 0)
			{
				m_stopOverrideAnimDelay--;
				if (m_stopOverrideAnimDelay == 0)
				{
					PlayAnimInternal(GetData().Animation);
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

		private void UpdateParallax()
		{
			if (m_parallaxDepth == 0f)
			{
				return;
			}
			float snapTo = 0f;
			RectCentered parallaxOffsetLimits = Singleton<PowerQuest>.Get.GetCamera().GetInstance().GetParallaxOffsetLimits();
			Vector2 vector = Utils.SnapRound(new Vector2(parallaxOffsetLimits.Center.x + m_parallaxAlignment.x * parallaxOffsetLimits.Width * 0.5f, parallaxOffsetLimits.Center.y + m_parallaxAlignment.y * parallaxOffsetLimits.Height * 0.5f), snapTo);
			QuestCamera camera = Singleton<PowerQuest>.Get.GetCamera();
			if (Singleton<PowerQuest>.Get.UseFancyParalaxSnapping && Singleton<PowerQuest>.Get.GetSnapToPixel() && !Singleton<PowerQuest>.Get.Camera.GetHasZoomOrTransition())
			{
				Vector2 vector2 = m_data.Position + (Singleton<PowerQuest>.Get.GetCamera().GetInstance().GetParallaxTargetPosition() - vector) * m_parallaxDepth;
				vector2 = Utils.SnapRound(vector2) - vector2;
				if (Singleton<PowerQuest>.Get.GetCamera().GetSnappedLastUpdate())
				{
					m_snapOffset = vector2;
				}
				else if (!camera.GetTargetPosChangedLastUpdate())
				{
					if (camera.GetTransitioning() && camera.GetTransitionTime() > 0f)
					{
						m_snapOffset = Vector2.MoveTowards(m_snapOffset, vector2, 1f / camera.GetTransitionTime() * Time.deltaTime);
					}
					else
					{
						m_snapOffset.x = Mathf.MoveTowards(m_snapOffset.x, vector2.x, Mathf.Max(1f, Mathf.Abs(camera.Velocity.x * 1.8f)) * Time.deltaTime);
						m_snapOffset.y = Mathf.MoveTowards(m_snapOffset.y, vector2.y, Mathf.Max(1f, Mathf.Abs(camera.Velocity.y * 1.8f)) * Time.deltaTime);
					}
				}
				else
				{
					m_snapOffset.x = Mathf.MoveTowards(m_snapOffset.x, 0f - Mathf.Sign(camera.Velocity.x), Mathf.Abs(camera.Velocity.x) * Time.deltaTime * 2.5f);
					m_snapOffset.y = Mathf.MoveTowards(m_snapOffset.y, 0f - Mathf.Sign(camera.Velocity.y), Mathf.Abs(camera.Velocity.y) * Time.deltaTime * 2.5f);
				}
			}
			else
			{
				m_snapOffset = Vector2.zero;
			}
			Vector2 vector3 = (Singleton<PowerQuest>.Get.GetCamera().GetInstance().GetPositionForParallax() - vector) * m_parallaxDepth;
			base.transform.position = (m_data.Position + vector3 + m_snapOffset).WithZ(base.transform.position.z);
		}

		private bool PlayAnimInternal(string animName, bool fromStart = true)
		{
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: true);
			}
			m_stopOverrideAnimDelay = 0;
			if (string.IsNullOrEmpty(animName))
			{
				return false;
			}
			if (Singleton<PowerQuest>.Get.GetCurrentRoom() == null || Singleton<PowerQuest>.Get.GetCurrentRoom().GetInstance() == null)
			{
				return false;
			}
			AnimationClip animation = Singleton<PowerQuest>.Get.GetCurrentRoom().GetInstance().GetAnimation(animName);
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
			Sprite sprite = Singleton<PowerQuest>.Get.GetCurrentRoom().GetInstance().GetSprite(animName);
			if (sprite != null && m_sprite != null)
			{
				m_spriteAnimator.Stop();
				m_sprite.sprite = sprite;
			}
			return false;
		}

		public IEnumerator CoroutineMoveTo(Vector2 toPos, float speed, eEaseCurve curve = eEaseCurve.None)
		{
			m_moving = true;
			Vector2 startPos = m_data.Position;
			float time = Vector2.Distance(startPos, toPos) / speed;
			float currTime = 0f;
			while (currTime < time && !Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				if (!SystemTime.Paused)
				{
					currTime += Time.deltaTime;
				}
				m_data.Position = Vector2.Lerp(startPos, toPos, QuestUtils.Ease(currTime / time, curve));
				yield return new WaitForEndOfFrame();
			}
			m_data.Position = toPos;
			m_moving = false;
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
