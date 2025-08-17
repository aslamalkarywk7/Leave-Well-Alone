using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace PowerTools.Quest
{
	public class AudioCue : MonoBehaviour
	{
		public enum eAudioType
		{
			Sound = 1,
			Music = 2,
			Dialog = 4,
			User1 = 8,
			User2 = 0x10,
			User3 = 0x20,
			User4 = 0x40,
			User5 = 0x80
		}

		[Serializable]
		public class Clip
		{
			[HideInInspector]
			public AudioClip m_sound;

			[HideInInspector]
			public float m_weight = 100f;

			[HideInInspector]
			public bool m_loop;

			[Tooltip("Volume. Multiplier on the base cue volume")]
			public MinMaxRange m_volume = new MinMaxRange(1f);

			[Tooltip("Volume. Multiplier on the base cue pitch")]
			public MinMaxRange m_pitch = new MinMaxRange(1f);

			[Tooltip("At what point in the clip should play end (crops the sound)")]
			public float m_startTime;

			[Tooltip("At what point in the clip should play end (crops the sound)")]
			public float m_endTime;
		}

		[Serializable]
		public struct LoopSection
		{
			public float m_startTime;

			public float m_endTime;

			public float m_fadeIn;

			public float m_fadeOut;
		}

		[Header("Type of Sound")]
		[Tooltip("Type of sound. Used to allow adjusting volume of different types of sounds")]
		[BitMask(typeof(eAudioType))]
		public int m_type = 1;

		[Tooltip("Whether it's a looping sound")]
		public bool m_loop;

		[Tooltip("If set, the sound will loop back to the start time when it reaches the end time. Useful for music that has an intro/outro section")]
		public LoopSection m_loopSection;

		[Header("Basic settings")]
		[Tooltip("Volume. Usually Randomly chosen within the specified range. But if Camera Size Range is set then it's the min/max for volume as it goes out of camera range")]
		[MinMaxRange(0f, 1f)]
		public MinMaxRange m_volume = new MinMaxRange(1f);

		[Tooltip("Pitch. Randomly chosen within the specified range")]
		[MinMaxRange(0.01f, 4f)]
		public MinMaxRange m_pitch = new MinMaxRange(1f);

		[Tooltip("Stereo Pan. 0 is center, -1  is left, 1 is right. Randomly chosen within the specified range")]
		[MinMaxRange(-1f, 1f)]
		public MinMaxRange m_pan = new MinMaxRange(0f);

		[Header("Playback Settings")]
		[Tooltip("Trigger another cue when this is played")]
		public AudioCue m_alsoPlay;

		[Tooltip("The random chance that this sound will play at all")]
		[Range(0f, 1f)]
		public float m_chance = 1f;

		[Tooltip("Delay before playing sound, after cue is played")]
		public MinMaxRange m_startDelay = new MinMaxRange(0f);

		[Tooltip("If >= 0 this overrides the default delay before the same sound can be repeated")]
		public float m_noDuplicateTime = -1f;

		[Header("Mixer Group")]
		[Tooltip("For more advanced enviroment effects, play using a mixer group")]
		public AudioMixerGroup m_mixerGroup;

		[Header("Effects")]
		[Tooltip("Reverb setting")]
		public AudioReverbPreset m_reverbPreset;

		[Range(0f, 0.9f)]
		[Tooltip("Distortion amount")]
		public float m_distortionLevel;

		[Tooltip("Low pass filter (remove treble) cutoff frequency in Hz (eg: 5000. Or Zero to disable)")]
		[Range(0f, 22000f)]
		public int m_lowPass;

		[Tooltip("Determines how much of the filter's self resonance is dampened (1 by default)")]
		public float m_lowPassQ = 1f;

		[Tooltip("High pass filter (remove bass) cutoff frequency in Hz (eg: 5000. Or Zero to disable)")]
		[Range(0f, 22000f)]
		public float m_highPass;

		[Tooltip("Determines how much of the filter's self resonance is dampened (1 by default)")]
		public float m_highPassQ = 1f;

		[Tooltip("Drag an audio filter on to here. (Add an AudioSource and filter component to this cue and drag it here)")]
		public AudioEchoFilter m_echoFilter;

		[HideInInspector]
		[Tooltip("Drag an audio filter on to here. (Add an AudioSource and filter component to this cue and drag it here)")]
		public AudioLowPassFilter m_lowPassFilter;

		[HideInInspector]
		[Tooltip("Drag an audio filter on to here. (Add an AudioSource and filter component to this cue and drag it here)")]
		public AudioHighPassFilter m_highPassFilter;

		[Tooltip("Drag an audio filter on to here. (Add an AudioSource and filter component to this cue and drag it here)")]
		public AudioChorusFilter m_chorusFilter;

		public List<Clip> m_sounds = new List<Clip>(1);

		private WeightedShuffledIndex m_shuffledIndex;

		public void SetTypeToMusic()
		{
			m_type = 2;
		}

		public void SetTypeToSFX()
		{
			m_type = 1;
		}

		public void SetTypeToDialog()
		{
			m_type = 4;
		}

		public void Play()
		{
			SystemAudio.Play(this);
		}

		public void Play(Transform emmitter)
		{
			SystemAudio.Play(this, emmitter);
		}

		public Clip GetClip()
		{
			if (m_sounds.Count == 0)
			{
				return null;
			}
			ValidateShuffledList();
			return m_sounds[m_shuffledIndex];
		}

		public int GetClipIndex(AudioClip clip)
		{
			return m_sounds.FindIndex((Clip item) => item.m_sound == clip);
		}

		public AudioClip GetClip(int index)
		{
			if (!m_sounds.IsIndexValid(index))
			{
				return null;
			}
			return m_sounds[index].m_sound;
		}

		public Clip GetClipData(int index)
		{
			if (!m_sounds.IsIndexValid(index))
			{
				return null;
			}
			return m_sounds[index];
		}

		public Clip GetClipData(AudioClip clip)
		{
			return m_sounds.Find((Clip item) => item.m_sound == clip);
		}

		public int GetClipCount()
		{
			return m_sounds.Count;
		}

		public WeightedShuffledIndex GetShuffledIndex()
		{
			ValidateShuffledList();
			return m_shuffledIndex;
		}

		private void ValidateShuffledList()
		{
			if (m_shuffledIndex == null || m_shuffledIndex.Count != m_sounds.Count)
			{
				m_shuffledIndex = new WeightedShuffledIndex();
				m_shuffledIndex.SetWeights(m_sounds, (Clip item) => item.m_weight);
			}
		}
	}
}
