using UnityEngine;

namespace PowerTools.Quest
{
	public class AudioHandle
	{
		private AudioSource m_source;

		private string m_cueName;

		public AudioSource source => m_source;

		public string cueName => m_cueName;

		public bool isPlaying
		{
			get
			{
				if (m_source != null)
				{
					return m_source.isPlaying;
				}
				return false;
			}
		}

		public float volume
		{
			get
			{
				if (!(m_source == null))
				{
					return SingletonAuto<SystemAudio>.Get.GetVolume(this);
				}
				return 0f;
			}
			set
			{
				if (m_source != null)
				{
					SingletonAuto<SystemAudio>.Get.SetVolume(this, value);
				}
			}
		}

		public float pitch
		{
			get
			{
				if (!(m_source == null))
				{
					return m_source.pitch;
				}
				return 0f;
			}
			set
			{
				if (m_source != null)
				{
					m_source.pitch = value;
				}
			}
		}

		public float panStereo
		{
			get
			{
				if (!(m_source == null))
				{
					return SingletonAuto<SystemAudio>.Get.GetPan(this);
				}
				return 0f;
			}
			set
			{
				if (m_source != null)
				{
					SingletonAuto<SystemAudio>.Get.SetPan(this, value);
				}
			}
		}

		public float time
		{
			get
			{
				if (!(m_source == null))
				{
					return m_source.time;
				}
				return 0f;
			}
			set
			{
				if (m_source != null)
				{
					m_source.time = Mathf.Min(value, (m_source.clip.length - 0.01f) * 0.9f);
				}
			}
		}

		public bool loop
		{
			get
			{
				if (!(m_source == null))
				{
					return m_source.loop;
				}
				return false;
			}
		}

		public AudioClip clip
		{
			get
			{
				if (!(m_source == null))
				{
					return m_source.clip;
				}
				return null;
			}
		}

		public AudioHandle(AudioSource source, string fromCue = null)
		{
			m_source = source;
			m_cueName = fromCue;
		}

		public static bool IsPlaying(AudioHandle handle)
		{
			return handle?.isPlaying ?? false;
		}

		public static bool IsNullOrStopped(AudioHandle handle)
		{
			if (handle != null)
			{
				return !handle.isPlaying;
			}
			return true;
		}

		public static implicit operator AudioSource(AudioHandle handle)
		{
			return handle?.m_source;
		}

		public void Pause()
		{
			SystemAudio.Pause(this);
		}

		public void UnPause()
		{
			SystemAudio.UnPause(this);
		}

		public void Stop(float overTime = 0f, float afterDelay = 0f)
		{
			if (SingletonAuto<SystemAudio>.GetValid() && SingletonAuto<SystemAudio>.Get.StopHandleInternal(this, overTime, afterDelay))
			{
				m_source = null;
			}
		}

		public AudioHandle FadeIn(float overTime)
		{
			SingletonAuto<SystemAudio>.Get.StartFadeIn(this, overTime);
			return this;
		}

		public AudioHandle Fade(float targetVolume, float overTime)
		{
			SingletonAuto<SystemAudio>.Get.StartFade(this, targetVolume, overTime);
			return this;
		}
	}
}
