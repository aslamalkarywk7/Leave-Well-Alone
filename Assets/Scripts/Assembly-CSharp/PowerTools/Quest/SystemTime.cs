using System;
using System.Collections.Generic;
using UnityEngine;

namespace PowerTools.Quest
{
	public class SystemTime : Singleton<SystemTime>
	{
		private class tTimeScaleMultiplier
		{
			public float m_mult = 1f;

			public float m_time;
		}

		[Header("Layers and components that shouldn't pause")]
		[SerializeField]
		private string[] m_ignoredLayers = new string[3] { "UI", "NoPause", "PostProcessing" };

		[SerializeField]
		private string[] m_ignoredComponents = new string[1] { "PostProcessLayer" };

		private static int m_layersNoPause;

		private Dictionary<string, tTimeScaleMultiplier> m_timeScaleMultipliers = new Dictionary<string, tTimeScaleMultiplier>();

		private float m_timeSinceLastFrame;

		private float m_timeFixedOriginal = 0.1f;

		private float m_debugTimeMultiplier = 1f;

		private bool m_gamePaused;

		private List<string> m_pauseSources = new List<string>();

		private List<Behaviour> m_pausedComponents = new List<Behaviour>();

		private List<Rigidbody2D> m_pausedBodies = new List<Rigidbody2D>();

		private List<Vector2> m_pausedVelocities = new List<Vector2>();

		private List<float> m_pausedAngularVelocities = new List<float>();

		private List<ParticleSystem> m_particleSystems = new List<ParticleSystem>();

		public Action CallbackOnPause;

		public Action CallbackOnUnpause;

		public static bool Paused => Singleton<SystemTime>.m_instance.m_gamePaused;

		public static bool GetPausedBy(string source)
		{
			return Singleton<SystemTime>.m_instance.m_pauseSources.Contains(source);
		}

		public bool IsBehaviourPausable(Behaviour behaviour)
		{
			string behaviourName = behaviour.GetType().Name;
			if (Array.Exists(m_ignoredComponents, (string item) => item == behaviourName))
			{
				return false;
			}
			return true;
		}

		public void PauseGame(string source = null)
		{
			if (source == null)
			{
				source = string.Empty;
			}
			m_gamePaused = true;
			m_pauseSources.Add(source);
			GameObject[] array = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
			GameObject gameObject = null;
			Behaviour[] array2 = null;
			int num = 0;
			Behaviour behaviour = null;
			ParticleSystem[] array3 = null;
			ParticleSystem particleSystem = null;
			int num2 = array.Length;
			for (int i = 0; i < num2; i++)
			{
				gameObject = array[i];
				if (!gameObject.activeInHierarchy || !IsLayerPauseable(gameObject.layer))
				{
					continue;
				}
				Rigidbody2D component = gameObject.GetComponent<Rigidbody2D>();
				if (component != null && !component.isKinematic)
				{
					m_pausedVelocities.Add(component.velocity);
					m_pausedAngularVelocities.Add(component.angularVelocity);
					m_pausedBodies.Add(component);
					component.isKinematic = true;
					component.velocity = Vector2.zero;
					component.angularVelocity = 0f;
				}
				array2 = gameObject.GetComponents<Behaviour>();
				num = array2.Length;
				for (int j = 0; j < num; j++)
				{
					behaviour = array2[j];
					if (behaviour.enabled && (behaviour is MonoBehaviour || behaviour is Animator) && IsBehaviourPausable(behaviour))
					{
						m_pausedComponents.Add(behaviour);
						behaviour.enabled = false;
					}
				}
				array3 = gameObject.GetComponents<ParticleSystem>();
				num = array3.Length;
				for (int k = 0; k < num; k++)
				{
					particleSystem = array3[k];
					if (particleSystem.isPlaying && !particleSystem.isPaused)
					{
						particleSystem.Pause();
						m_particleSystems.Add(particleSystem);
					}
				}
			}
			if (CallbackOnPause != null)
			{
				CallbackOnPause();
			}
		}

		public void UnPauseGame(string source = null)
		{
			if (source == null)
			{
				source = string.Empty;
			}
			m_pauseSources.RemoveAll((string item) => item == source);
			if (m_pauseSources.Count == 0)
			{
				Behaviour behaviour = null;
				int count = m_pausedComponents.Count;
				for (int num = 0; num < count; num++)
				{
					behaviour = m_pausedComponents[num];
					if (behaviour != null)
					{
						behaviour.enabled = true;
					}
				}
				m_pausedComponents.Clear();
				Rigidbody2D rigidbody2D = null;
				count = m_pausedBodies.Count;
				for (int num2 = 0; num2 < count; num2++)
				{
					rigidbody2D = m_pausedBodies[num2];
					if (rigidbody2D != null)
					{
						rigidbody2D.isKinematic = false;
						rigidbody2D.velocity = m_pausedVelocities[num2];
						rigidbody2D.angularVelocity = m_pausedAngularVelocities[num2];
						rigidbody2D.WakeUp();
					}
				}
				m_pausedBodies.Clear();
				m_pausedVelocities.Clear();
				m_pausedAngularVelocities.Clear();
				count = m_particleSystems.Count;
				for (int num3 = 0; num3 < count; num3++)
				{
					if (m_particleSystems[num3] != null)
					{
						m_particleSystems[num3].Play();
					}
				}
				m_particleSystems.Clear();
				m_gamePaused = false;
			}
			if (CallbackOnUnpause != null)
			{
				CallbackOnUnpause();
			}
		}

		public float GetTimeScale()
		{
			return Time.timeScale / m_debugTimeMultiplier;
		}

		public float GetUnscaledDeltaTime()
		{
			return Time.unscaledDeltaTime * m_debugTimeMultiplier;
		}

		public void SlowMoPause(string source, float time)
		{
			tTimeScaleMultiplier tTimeScaleMultiplier = new tTimeScaleMultiplier();
			tTimeScaleMultiplier.m_time = time;
			tTimeScaleMultiplier.m_mult = 0.001f;
			m_timeScaleMultipliers[source] = tTimeScaleMultiplier;
		}

		public void SlowMoBegin(string source, float scale, float time)
		{
			m_timeScaleMultipliers[source] = new tTimeScaleMultiplier
			{
				m_mult = scale,
				m_time = time
			};
		}

		public void SlowMoBegin(string source, float scale)
		{
			m_timeScaleMultipliers[source] = new tTimeScaleMultiplier
			{
				m_mult = scale,
				m_time = -1f
			};
		}

		public void SlowMoEnd(string source)
		{
			m_timeScaleMultipliers.Remove(source);
		}

		public void SlowMoClear()
		{
			ClearTimeMultipliers();
		}

		public void SetDebugTimeMultiplier(float multiplier)
		{
			m_debugTimeMultiplier = multiplier;
		}

		public float GetDebugTimeMultiplier()
		{
			return m_debugTimeMultiplier;
		}

		public static bool TimePassed(float period)
		{
			return Time.timeSinceLevelLoad % period < (Time.timeSinceLevelLoad - Time.deltaTime) % period;
		}

		public static bool TimePassedFixed(float period)
		{
			return Time.timeSinceLevelLoad % period < (Time.timeSinceLevelLoad - Time.fixedDeltaTime) % period;
		}

		public static bool TimePassedFixed(float period, float offsetRatio)
		{
			float num = period * offsetRatio + Time.timeSinceLevelLoad;
			return num % period < (num - Time.fixedDeltaTime) % period;
		}

		private void Awake()
		{
			SetSingleton();
			UnityEngine.Object.DontDestroyOnLoad(this);
			m_timeFixedOriginal = Time.fixedDeltaTime;
		}

		private void Update()
		{
			UpdateTimeMultipliers();
			if (!Singleton<PowerQuest>.Get.UseCustomKBShortcuts)
			{
				if (PowerQuest.GetDebugKeyHeld() && Input.GetKeyDown(KeyCode.PageDown))
				{
					SetDebugTimeMultiplier(GetDebugTimeMultiplier() * 0.8f);
				}
				if (PowerQuest.GetDebugKeyHeld() && Input.GetKeyDown(KeyCode.PageUp))
				{
					SetDebugTimeMultiplier(GetDebugTimeMultiplier() + 0.2f);
				}
				if (PowerQuest.GetDebugKeyHeld() && Input.GetKeyDown(KeyCode.End))
				{
					SetDebugTimeMultiplier(1f);
				}
			}
		}

		private void ClearTimeMultipliers()
		{
			m_timeScaleMultipliers.Clear();
			Time.timeScale = 1f;
			Time.fixedDeltaTime = m_timeFixedOriginal * 1f;
		}

		private void UpdateTimeMultipliers()
		{
			float num = 1f;
			if (!m_gamePaused)
			{
				float num2 = Time.realtimeSinceStartup - m_timeSinceLastFrame;
				m_timeSinceLastFrame = Time.realtimeSinceStartup;
				if (m_timeScaleMultipliers.Count > 0)
				{
					foreach (string item in new List<string>(m_timeScaleMultipliers.Keys))
					{
						float time = m_timeScaleMultipliers[item].m_time;
						if (time > 0f)
						{
							time -= num2;
							if (time <= 0f)
							{
								m_timeScaleMultipliers.Remove(item);
								continue;
							}
							m_timeScaleMultipliers[item].m_time = time;
							num = Mathf.Min(num, m_timeScaleMultipliers[item].m_mult);
						}
						else
						{
							num = Mathf.Min(num, m_timeScaleMultipliers[item].m_mult);
						}
					}
				}
			}
			num = (Time.timeScale = num * m_debugTimeMultiplier);
			Time.fixedDeltaTime = m_timeFixedOriginal * num;
		}

		public bool IsLayerPauseable(int layer)
		{
			if (m_layersNoPause == 0)
			{
				m_layersNoPause = LayerMask.GetMask(m_ignoredLayers);
			}
			return (m_layersNoPause & (1 << layer)) == 0;
		}
	}
}
