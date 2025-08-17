using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Video;

namespace PowerTools.Quest
{
	[Serializable]
	public class Prop : IQuestClickable, IProp, IQuestClickableInterface, IQuestScriptable
	{
		[Header("Mouse-over Defaults")]
		[TextArea(1, 10)]
		[SerializeField]
		private string m_description = "New Prop";

		[Tooltip("If set, changes the name of the cursor when moused over")]
		[SerializeField]
		private string m_cursor;

		[Header("Starting State")]
		[SerializeField]
		private bool m_visible = true;

		[Tooltip("Whether clicking on hotspot triggers an event")]
		[SerializeField]
		private bool m_clickable = true;

		[SerializeField]
		private string m_animation;

		[SerializeField]
		private float m_alpha = 1f;

		[Header("Editable in Scene")]
		[Tooltip("Move the transform around to change this (unlike characters!)")]
		[ReadOnly]
		[SerializeField]
		private Vector2 m_position = Vector2.zero;

		[SerializeField]
		private float m_baseline;

		[SerializeField]
		[Tooltip("If true, the baseline will be in world position, instead of local to the object. So y position of the sortable is ignored")]
		private bool m_baselineFixed;

		[SerializeField]
		private Vector2 m_walkToPoint = Vector2.zero;

		[SerializeField]
		private Vector2 m_lookAtPoint = Vector2.zero;

		[ReadOnly]
		[SerializeField]
		private string m_scriptName = "PropNew";

		private PropComponent m_instance;

		private int m_useCount;

		private int m_lookCount;

		public eQuestClickableType ClickableType => eQuestClickableType.Prop;

		public string Description
		{
			get
			{
				return m_description;
			}
			set
			{
				m_description = value;
			}
		}

		public string ScriptName => m_scriptName;

		public MonoBehaviour Instance => m_instance;

		public Prop Data => this;

		public IQuestClickable IClickable => this;

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
					m_instance.OnSetVisible();
				}
			}
		}

		public bool Clickable
		{
			get
			{
				return m_clickable;
			}
			set
			{
				if (!value || !(Instance != null) || m_instance.GetHasCollider())
				{
					m_clickable = value;
				}
			}
		}

		public Vector2 Position
		{
			get
			{
				return m_position;
			}
			set
			{
				float y = m_position.y;
				m_position = value;
				if (m_instance != null)
				{
					m_instance.OnSetPosition();
					if (!m_baselineFixed && y != value.y)
					{
						m_instance.UpdateBaseline();
					}
				}
			}
		}

		public float Baseline
		{
			get
			{
				return m_baseline;
			}
			set
			{
				m_baseline = value;
				if (m_instance != null)
				{
					m_instance.UpdateBaseline();
				}
			}
		}

		public bool BaselineFixed
		{
			get
			{
				return m_baselineFixed;
			}
			set
			{
				m_baselineFixed = value;
				if (m_instance != null)
				{
					m_instance.UpdateBaseline();
				}
			}
		}

		public int SortOrder => -Mathf.RoundToInt(((m_baselineFixed ? 0f : Position.y) + Baseline) * 10f);

		public Vector2 WalkToPoint
		{
			get
			{
				return m_walkToPoint;
			}
			set
			{
				m_walkToPoint = value;
			}
		}

		public Vector2 LookAtPoint
		{
			get
			{
				return m_lookAtPoint;
			}
			set
			{
				m_lookAtPoint = value;
			}
		}

		public string Cursor
		{
			get
			{
				return m_cursor;
			}
			set
			{
				m_cursor = value;
			}
		}

		public bool FirstUse => UseCount == 0;

		public bool FirstLook => LookCount == 0;

		public int UseCount => m_useCount - (Singleton<PowerQuest>.Get.GetInteractionInProgress(this, eQuestVerb.Use) ? 1 : 0);

		public int LookCount => m_lookCount - (Singleton<PowerQuest>.Get.GetInteractionInProgress(this, eQuestVerb.Look) ? 1 : 0);

		public string Animation
		{
			get
			{
				return m_animation;
			}
			set
			{
				m_animation = value;
				if (m_instance != null)
				{
					m_instance.OnAnimationChanged();
				}
			}
		}

		public bool Animating
		{
			get
			{
				if (m_instance != null)
				{
					return m_instance.GetAnimating();
				}
				return false;
			}
		}

		public bool Moving
		{
			get
			{
				if (!(Instance == null))
				{
					return m_instance.Moving;
				}
				return false;
			}
		}

		public VideoPlayer VideoPlayer
		{
			get
			{
				if (m_instance == null)
				{
					return null;
				}
				return m_instance.GetVideoPlayer();
			}
		}

		public float Alpha
		{
			get
			{
				return m_alpha;
			}
			set
			{
				m_alpha = value;
				if (m_instance != null)
				{
					m_instance.UpdateAlpha();
				}
			}
		}

		public void Show(bool clickable = true)
		{
			Enable(clickable);
		}

		public void Hide()
		{
			Disable();
		}

		public void Enable(bool clickable = true)
		{
			Visible = true;
			if (clickable)
			{
				Clickable = true;
			}
		}

		public void Disable()
		{
			Visible = false;
			Clickable = false;
		}

		public void SetPosition(float x, float y)
		{
			Position = new Vector2(x, y);
		}

		public PropComponent GetInstance()
		{
			return m_instance;
		}

		public void SetInstance(PropComponent instance)
		{
			m_instance = instance;
			instance.SetData(this);
		}

		public QuestScript GetScript()
		{
			if (Singleton<PowerQuest>.Get.GetCurrentRoom() != null)
			{
				return Singleton<PowerQuest>.Get.GetCurrentRoom().GetScript();
			}
			return null;
		}

		public IQuestScriptable GetScriptable()
		{
			return this;
		}

		public void OnInteraction(eQuestVerb verb)
		{
			switch (verb)
			{
			case eQuestVerb.Look:
				m_lookCount++;
				break;
			case eQuestVerb.Use:
				m_useCount++;
				break;
			}
		}

		public void OnCancelInteraction(eQuestVerb verb)
		{
			switch (verb)
			{
			case eQuestVerb.Look:
				m_lookCount--;
				break;
			case eQuestVerb.Use:
				m_useCount--;
				break;
			}
		}

		public void IsCollidingWith()
		{
			throw new NotImplementedException();
		}

		public Coroutine PlayAnimation(string animName)
		{
			if (m_instance != null)
			{
				return Singleton<PowerQuest>.Get.StartCoroutine(CoroutinePlayAnimation(animName));
			}
			return null;
		}

		public void PlayAnimationBG(string animName)
		{
			if (m_instance != null && !Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				m_instance.PlayAnimation(animName);
			}
		}

		public void PauseAnimation()
		{
			if (m_instance != null)
			{
				m_instance.PauseAnimation();
			}
		}

		public void ResumeAnimation()
		{
			if (m_instance != null)
			{
				m_instance.ResumeAnimation();
			}
		}

		public void AddAnimationTrigger(string triggerName, bool removeAfterTriggering, Action action)
		{
			if (m_instance != null)
			{
				QuestAnimationTriggers questAnimationTriggers = m_instance.GetComponent<QuestAnimationTriggers>();
				if (questAnimationTriggers == null)
				{
					questAnimationTriggers = m_instance.gameObject.AddComponent<QuestAnimationTriggers>();
				}
				if (questAnimationTriggers != null)
				{
					questAnimationTriggers.AddTrigger(triggerName, action, removeAfterTriggering);
				}
			}
		}

		public void RemoveAnimationTrigger(string triggerName)
		{
			if (m_instance != null)
			{
				QuestAnimationTriggers component = m_instance.GetComponent<QuestAnimationTriggers>();
				if (component != null)
				{
					component.RemoveTrigger(triggerName);
				}
			}
		}

		public Coroutine WaitForAnimTrigger(string triggerName)
		{
			return Singleton<PowerQuest>.Get.StartCoroutine(CoroutineWaitForAnimTrigger(triggerName));
		}

		public Coroutine MoveTo(float x, float y, float speed, eEaseCurve curve = eEaseCurve.None)
		{
			return MoveTo(new Vector2(x, y), speed, curve);
		}

		public Coroutine MoveTo(Vector2 toPos, float speed, eEaseCurve curve = eEaseCurve.None)
		{
			if (m_instance == null)
			{
				return null;
			}
			return Singleton<PowerQuest>.Get.StartQuestCoroutine(m_instance.CoroutineMoveTo(toPos, speed, curve));
		}

		public void MoveToBG(Vector2 toPos, float speed, eEaseCurve curve = eEaseCurve.None)
		{
			MoveTo(toPos, speed, curve);
		}

		public Coroutine PlayVideo(float skippableAfterTime = -1f)
		{
			if (m_instance != null)
			{
				return Singleton<PowerQuest>.Get.StartCoroutine(CoroutinePlayVideo(skippableAfterTime));
			}
			return null;
		}

		public void PlayVideoBG()
		{
			if (!(m_instance == null))
			{
				VideoPlayer videoPlayer = m_instance.GetVideoPlayer();
				if (videoPlayer == null)
				{
					Debug.LogWarning("Video Playback failed- No VideoPlayer component added to prop " + ScriptName);
					return;
				}
				videoPlayer.enabled = true;
				videoPlayer.Play();
			}
		}

		public void EditorInitialise(string name)
		{
			m_description = name;
			m_scriptName = name;
			m_animation = name;
		}

		public void EditorRename(string name)
		{
			m_scriptName = name;
		}

		public Coroutine Fade(float start, float end, float duration, eEaseCurve curve = eEaseCurve.InOutSmooth)
		{
			return Singleton<PowerQuest>.Get.StartCoroutine(CoroutineFade(start, end, duration, curve));
		}

		public void FadeBG(float start, float end, float duration, eEaseCurve curve = eEaseCurve.InOutSmooth)
		{
			Singleton<PowerQuest>.Get.StartCoroutine(CoroutineFade(start, end, duration, curve));
		}

		private IEnumerator CoroutinePlayAnimation(string animName)
		{
			if (m_instance == null)
			{
				yield break;
			}
			m_instance.PlayAnimation(animName);
			while (m_instance != null && m_instance.GetAnimating() && !Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				yield return new WaitForEndOfFrame();
			}
			if (Singleton<PowerQuest>.Get.GetSkippingCutscene() && m_instance != null)
			{
				SpriteAnim component = m_instance.GetComponent<SpriteAnim>();
				if (component != null)
				{
					component.NormalizedTime = 1f;
					m_instance.GetComponent<Animator>().Update(0f);
				}
				m_instance.StopAnimation();
			}
		}

		private IEnumerator CoroutinePlayVideo(float skippableAfterTime = -1f)
		{
			if (Singleton<PowerQuest>.Get.GetSkippingCutscene() || m_instance == null)
			{
				yield break;
			}
			VideoPlayer video = m_instance.GetVideoPlayer();
			if (video == null)
			{
				Debug.LogWarning("Video Playback failed- No VideoPlayer component added to prop " + ScriptName);
				yield break;
			}
			bool wasEnabled = video.enabled;
			video.enabled = true;
			video.Play();
			yield return Singleton<PowerQuest>.Get.WaitUntil(() => video.isPlaying);
			if (skippableAfterTime >= 0f)
			{
				yield return Singleton<PowerQuest>.Get.Wait(skippableAfterTime);
			}
			yield return Singleton<PowerQuest>.Get.WaitWhile(() => video.isPlaying || !Application.isFocused, skippableAfterTime >= 0f);
			video.Stop();
			if (!wasEnabled)
			{
				video.enabled = false;
			}
		}

		private IEnumerator CoroutineFade(float start, float end, float duration, eEaseCurve curve = eEaseCurve.InOutSmooth)
		{
			if (Instance == null)
			{
				yield break;
			}
			float time = 0f;
			Alpha = start;
			while (time < duration && !Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				yield return new WaitForEndOfFrame();
				if (!SystemTime.Paused)
				{
					time += Time.deltaTime;
				}
				Alpha = Mathf.Lerp(start, end, QuestUtils.Ease(time / duration, curve));
			}
			Alpha = end;
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
				yield return Singleton<PowerQuest>.Get.WaitUntil(() => hit || m_instance == null || !m_instance.GetSpriteAnimator().Playing);
			}
		}

		public string GetScriptName()
		{
			return m_scriptName;
		}

		public string GetScriptClassName()
		{
			return PowerQuest.STR_PROP + m_scriptName;
		}

		public void HotLoadScript(Assembly assembly)
		{
		}

		[OnDeserializing]
		private void CopyDefaults(StreamingContext sc)
		{
			QuestUtils.InitWithDefaults(this);
		}
	}
}
