using System;
using UnityEngine;

namespace PowerTools.Quest
{
	[Serializable]
	public class QuestSettings
	{
		public enum eDialogDisplay
		{
			TextAndSpeech = 0,
			SpeechOnly = 1,
			TextOnly = 2
		}

		public CursorLockMode m_lockCursor = CursorLockMode.Confined;

		[SerializeField]
		private float m_masterVolume = 1f;

		[SerializeField]
		private float m_musicVolume = 1f;

		[SerializeField]
		private float m_sfxVolume = 1f;

		[SerializeField]
		private float m_dialogVolume = 1f;

		[SerializeField]
		private eDialogDisplay m_dialogDisplay;

		[Tooltip("The Default Language. Should match codes set in SystemText")]
		[SerializeField]
		private string m_languageCode = "EN";

		[SerializeField]
		private float m_textSpeedMultiplier = 1f;

		public float Volume
		{
			get
			{
				return m_masterVolume;
			}
			set
			{
				m_masterVolume = value;
				AudioListener.volume = value;
			}
		}

		public float VolumeMusic
		{
			get
			{
				return m_musicVolume;
			}
			set
			{
				m_musicVolume = value;
				SystemAudio.SetVolume(AudioCue.eAudioType.Music, value);
			}
		}

		public float VolumeSFX
		{
			get
			{
				return m_sfxVolume;
			}
			set
			{
				m_sfxVolume = value;
				SystemAudio.SetVolume(AudioCue.eAudioType.Sound, value);
			}
		}

		public float VolumeDialog
		{
			get
			{
				return m_dialogVolume;
			}
			set
			{
				m_dialogVolume = value;
				SystemAudio.SetVolume(AudioCue.eAudioType.Dialog, value);
			}
		}

		public float TextSpeedMultiplier
		{
			get
			{
				return m_textSpeedMultiplier;
			}
			set
			{
				m_textSpeedMultiplier = value;
			}
		}

		public eDialogDisplay DialogDisplay
		{
			get
			{
				return m_dialogDisplay;
			}
			set
			{
				m_dialogDisplay = value;
			}
		}

		public CursorLockMode LockCursor
		{
			get
			{
				return m_lockCursor;
			}
			set
			{
				m_lockCursor = value;
				if (!Application.isEditor)
				{
					Cursor.lockState = m_lockCursor;
				}
			}
		}

		public string Language
		{
			get
			{
				return m_languageCode;
			}
			set
			{
				if (Singleton<SystemText>.Get.SetLanguage(value))
				{
					m_languageCode = value;
				}
			}
		}

		public LanguageData LanguageData => Systems.Text.GetLanguageData();

		public int LanguageId
		{
			get
			{
				return Singleton<SystemText>.Get.GetLanguage();
			}
			set
			{
				if (!Systems.Text.GetLanguages().IsIndexValid(value))
				{
					Debug.LogWarning("Couldn't find language id: " + value);
					return;
				}
				m_languageCode = Systems.Text.GetLanguages()[value].m_code;
				Singleton<SystemText>.Get.SetLanguage(value);
			}
		}

		public void OnInitialise()
		{
			AudioListener.volume = m_masterVolume;
			SystemAudio.SetVolume(AudioCue.eAudioType.Music, m_musicVolume);
			SystemAudio.SetVolume(AudioCue.eAudioType.Sound, m_sfxVolume);
			SystemAudio.SetVolume(AudioCue.eAudioType.Dialog, m_dialogVolume);
			LockCursor = m_lockCursor;
			Language = m_languageCode;
		}

		public void OnPostRestore(int version)
		{
			OnInitialise();
		}

		public LanguageData[] GetLanguages()
		{
			return Singleton<SystemText>.Get.GetLanguages();
		}
	}
}
