using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace PowerTools.Quest
{
	[ExecuteInEditMode]
	public class SystemAudio : SingletonAuto<SystemAudio>
	{
		[Serializable]
		private class AudioTypeVolume
		{
			public AudioCue.eAudioType m_type = AudioCue.eAudioType.Sound;

			public float m_volume = 1f;

			public AudioMixerGroup m_mixerGroup;
		}

		private class ClipInfo
		{
			public AudioCue cue { get; set; }

			public AudioHandle handle { get; set; }

			public int type { get; set; }

			public float defaultVolume { get; set; }

			public float defaultPitch { get; set; }

			public float defaultPan { get; set; }

			public float targetVolume { get; set; }

			public float fadeDelta { get; set; }

			public float startFromTime { get; set; }

			public float stopAfterTime { get; set; }

			public bool stopAfterFade { get; set; }

			public Transform emmitter { get; set; }

			public bool paused { get; set; }
		}

		private class SaveData
		{
			public class ActiveAudioSaveData
			{
				public string cueName;

				public bool paused;

				public float time;

				public float pan;

				public float pitch = 1f;

				public float volume = 1f;

				public float volumeTarget = 1f;

				public float fadeDelta;

				public bool stopAfterFade;

				public ActiveAudioSaveData()
				{
				}

				public ActiveAudioSaveData(ClipInfo clipInfo)
				{
					cueName = clipInfo.cue.name;
					paused = clipInfo.paused;
					time = clipInfo.handle.time;
					pitch = clipInfo.defaultPitch;
					pan = clipInfo.defaultPan;
					volume = clipInfo.defaultVolume;
					volumeTarget = clipInfo.targetVolume;
					fadeDelta = clipInfo.fadeDelta;
					stopAfterFade = clipInfo.stopAfterFade;
				}

				public void InitClipInfo(ClipInfo clipInfo)
				{
					if (clipInfo != null)
					{
						clipInfo.defaultVolume = volume;
						clipInfo.targetVolume = volumeTarget;
						clipInfo.defaultPitch = pitch;
						clipInfo.defaultPan = pan;
						clipInfo.fadeDelta = fadeDelta;
						clipInfo.stopAfterFade = stopAfterFade;
					}
				}
			}

			public string m_musicCueName;

			public float m_musicVolOverride = -1f;

			public float m_musicTime;

			public string m_ambientCueName;

			public bool m_restartMusicIfAlreadyPlaying;

			public float m_falloffDistanceMultiplier = 1f;

			public ActiveAudioSaveData[] activeAudio;
		}

		private static List<AudioHandle> s_defaultAudioHandleList = new List<AudioHandle>();

		private static readonly string STRING_NAME_PREFIX = "Audio: ";

		private static int LAYER_NOPAUSE = -1;

		private static readonly int AUDIO_SOURCE_POOL_SIZE = 16;

		[Header("Default volume levels(and/or mixer groups), by type (Music, SFX, Dialog)")]
		[SerializeField]
		[ReorderableArray]
		[NonReorderable]
		private List<AudioTypeVolume> m_volumeByType = new List<AudioTypeVolume>();

		[SerializeField]
		private float m_musicDuckingMaxVolume = 0.5f;

		[Header("Default falloff values (radios of screen width)")]
		[SerializeField]
		private float m_falloffMinVol = 0.2f;

		[SerializeField]
		private float m_falloffPanMax = 0.8f;

		[SerializeField]
		private float m_falloffStart = 1f;

		[SerializeField]
		private float m_falloffEnd = 2f;

		[SerializeField]
		private float m_falloffPanStart = 0.5f;

		[SerializeField]
		private float m_falloffPanEnd = 2f;

		[Header("Misc settings")]
		[Tooltip("Default minimum time between the same sound playing again")]
		[SerializeField]
		private float m_noDuplicateTime = 0.05f;

		[Tooltip("If the narrator has its own mixer group, set it here. Otherwise it will use the default one for Dialog")]
		[SerializeField]
		private AudioMixerGroup m_narratorMixerGroup;

		[Header("List of audio cues that can be played by name")]
		[Tooltip("If set, any cues in the Audio folder will be automatically added (you don't have to click the button in the cue)")]
		[SerializeField]
		private bool m_autoAddCues = true;

		[Tooltip("Audio cues that can be played by name")]
		[SerializeField]
		private List<AudioCue> m_audioCues = new List<AudioCue>();

		private List<ClipInfo> m_activeAudio = new List<ClipInfo>();

		private AudioHandle m_activeMusic;

		private AudioHandle m_activeAmbientLoop;

		private Transform m_audioListener;

		private bool m_hasFocus = true;

		private List<AudioSource> m_audioSources = new List<AudioSource>();

		private Camera m_cameraGame;

		private string m_musicCueName = string.Empty;

		private float m_musicVolOverride = 1f;

		private string m_ambientCueName;

		private bool m_restartMusicIfAlreadyPlaying;

		private float m_falloffDistanceMultiplier = 1f;

		public static AudioHandle MusicHandle => SingletonAuto<SystemAudio>.Get.GetActiveMusicHandle();

		public static float FalloffDistanceMultiplier
		{
			get
			{
				return SingletonAuto<SystemAudio>.m_instance.m_falloffDistanceMultiplier;
			}
			set
			{
				SingletonAuto<SystemAudio>.m_instance.m_falloffDistanceMultiplier = value;
			}
		}

		public static bool ShouldRestartMusicIfAlreadyPlaying => SingletonAuto<SystemAudio>.m_instance.m_restartMusicIfAlreadyPlaying;

		public AudioMixerGroup NarratorMixerGroup => m_narratorMixerGroup;

		public static void SetVolume(AudioCue.eAudioType type, float volume)
		{
			SystemAudio get = SingletonAuto<SystemAudio>.Get;
			if (get == null)
			{
				Debug.LogWarning("Failed to set AudioSystem volume. It hasn't been initialised");
				return;
			}
			float num = 1f;
			bool flag = false;
			if (volume <= 0f)
			{
				volume = -1f;
			}
			for (int i = 0; i < get.m_volumeByType.Count; i++)
			{
				if (get.m_volumeByType[i].m_type == type)
				{
					num = get.m_volumeByType[i].m_volume;
					get.m_volumeByType[i].m_volume = volume;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				get.m_volumeByType.Add(new AudioTypeVolume
				{
					m_type = type,
					m_volume = volume
				});
			}
			float num2 = volume / num;
			for (int j = 0; j < get.m_activeAudio.Count; j++)
			{
				ClipInfo clipInfo = get.m_activeAudio[j];
				if (clipInfo != null && ((uint)clipInfo.type & (uint)type) != 0)
				{
					if (clipInfo.handle.source != null)
					{
						clipInfo.handle.source.volume *= num2;
					}
					clipInfo.defaultVolume *= num2;
					clipInfo.targetVolume *= num2;
				}
			}
		}

		public static float GetVolume(AudioCue.eAudioType type)
		{
			SystemAudio get = SingletonAuto<SystemAudio>.Get;
			for (int i = 0; i < get.m_volumeByType.Count; i++)
			{
				if (get.m_volumeByType[i].m_type == type)
				{
					return Mathf.Clamp01(get.m_volumeByType[i].m_volume);
				}
			}
			return 1f;
		}

		public float GetVolume(AudioHandle source)
		{
			return SingletonAuto<SystemAudio>.m_instance.m_activeAudio.Find((ClipInfo item) => item.handle == source)?.defaultVolume ?? 0f;
		}

		public void SetVolume(AudioHandle source, float volume)
		{
			ClipInfo clipInfo = SingletonAuto<SystemAudio>.m_instance.m_activeAudio.Find((ClipInfo item) => item.handle == source);
			if (clipInfo != null && !clipInfo.stopAfterFade)
			{
				clipInfo.defaultVolume = volume;
				clipInfo.targetVolume = volume;
			}
		}

		public float GetPan(AudioHandle source)
		{
			return SingletonAuto<SystemAudio>.m_instance.m_activeAudio.Find((ClipInfo item) => item.handle == source)?.defaultPan ?? 0f;
		}

		public void SetPan(AudioHandle source, float pan)
		{
			ClipInfo clipInfo = SingletonAuto<SystemAudio>.m_instance.m_activeAudio.Find((ClipInfo item) => item.handle == source);
			if (clipInfo != null)
			{
				clipInfo.defaultPan = pan;
				source.source.panStereo = pan;
			}
		}

		public static AudioCue GetCue(string cueName)
		{
			return SingletonAuto<SystemAudio>.m_instance.m_audioCues.Find((AudioCue item) => string.Equals(cueName, item.name, StringComparison.OrdinalIgnoreCase));
		}

		public static bool IsPlaying(string cueName)
		{
			return AudioHandle.IsPlaying(GetHandle(cueName));
		}

		public static AudioHandle Play(string cueName, Transform emmitter = null)
		{
			AudioCue audioCue = SingletonAuto<SystemAudio>.Get.m_audioCues.Find((AudioCue item) => item != null && string.Equals(cueName, item.name, StringComparison.OrdinalIgnoreCase));
			if (audioCue == null && !string.IsNullOrEmpty(cueName) && (Application.isEditor || Singleton<PowerQuest>.Get.IsDebugBuild))
			{
				Debug.LogWarning("Sound cue not found: " + cueName);
			}
			return Play(audioCue, emmitter);
		}

		public static AudioHandle Play(AudioCue cue, Transform emitter = null, float volumeMult = 1f, float pitchMult = 1f, float fromTime = 0f, float withStartDelay = 0f)
		{
			return Play(cue, ref s_defaultAudioHandleList, emitter, volumeMult, pitchMult, fromTime, withStartDelay);
		}

		public static AudioHandle Play(AudioCue cue, ref List<AudioHandle> handles, Transform emmitter = null, float volumeMult = 1f, float pitchMult = 1f, float fromTime = 0f, float withStartDelay = 0f)
		{
			if (cue == null)
			{
				return new AudioHandle(null);
			}
			SystemAudio get = SingletonAuto<SystemAudio>.Get;
			AudioCue.Clip clip = cue.GetClip();
			if (clip == null)
			{
				return new AudioHandle(null);
			}
			if (Application.isPlaying && Singleton<PowerQuest>.Get.GetSkippingCutscene() && !cue.m_loop && (2 & cue.m_type) == 0)
			{
				return new AudioHandle(null);
			}
			if (UnityEngine.Random.value > cue.m_chance)
			{
				return new AudioHandle(null);
			}
			AudioClip sound = clip.m_sound;
			if (sound == null)
			{
				return new AudioHandle(null);
			}
			for (int i = 0; i < get.m_activeAudio.Count; i++)
			{
				ClipInfo clipInfo = get.m_activeAudio[i];
				if (clipInfo.handle != null && clipInfo.handle.clip == sound && Application.isPlaying && withStartDelay <= 0f && fromTime <= 0f && cue.m_loopSection.m_endTime <= 0f && (int)cue.m_startDelay <= 0 && clipInfo.handle.time < ((cue.m_noDuplicateTime >= 0f) ? cue.m_noDuplicateTime : get.m_noDuplicateTime))
				{
					return new AudioHandle(null);
				}
			}
			float num = clip.m_volume.GetRandom() * volumeMult;
			num *= cue.m_volume.GetRandom();
			AudioMixerGroup outputAudioMixerGroup = null;
			for (int j = 0; j < get.m_volumeByType.Count; j++)
			{
				if (((uint)get.m_volumeByType[j].m_type & (uint)cue.m_type) != 0)
				{
					num *= get.m_volumeByType[j].m_volume;
					outputAudioMixerGroup = get.m_volumeByType[j].m_mixerGroup;
				}
			}
			float num2 = cue.m_pitch.GetRandom() * clip.m_pitch.GetRandom() * pitchMult;
			float random = cue.m_pan.GetRandom();
			AudioSource source = get.SpawnAudioSource(Debug.isDebugBuild ? (STRING_NAME_PREFIX + cue.name) : STRING_NAME_PREFIX, get.transform.position);
			AudioReverbFilter component = source.gameObject.GetComponent<AudioReverbFilter>();
			if (component == null)
			{
				get.AddSourceFilters(source.gameObject);
				component = source.gameObject.GetComponent<AudioReverbFilter>();
			}
			component.enabled = cue.m_reverbPreset != AudioReverbPreset.Off;
			if (component.enabled)
			{
				component.reverbPreset = cue.m_reverbPreset;
			}
			AudioEchoFilter component2 = source.gameObject.GetComponent<AudioEchoFilter>();
			component2.enabled = cue.m_echoFilter != null;
			if (component2.enabled)
			{
				component2.delay = cue.m_echoFilter.delay;
				component2.decayRatio = cue.m_echoFilter.decayRatio;
				component2.dryMix = cue.m_echoFilter.dryMix;
			}
			AudioDistortionFilter component3 = source.gameObject.GetComponent<AudioDistortionFilter>();
			component3.enabled = cue.m_distortionLevel > 0f;
			if (component3.enabled)
			{
				component3.distortionLevel = cue.m_distortionLevel;
			}
			AudioHighPassFilter component4 = source.gameObject.GetComponent<AudioHighPassFilter>();
			component4.enabled = cue.m_highPassFilter != null || cue.m_highPass > 10f;
			if (component4.enabled)
			{
				if (cue.m_highPass > 10f)
				{
					component4.cutoffFrequency = cue.m_highPass;
					component4.highpassResonanceQ = cue.m_highPassQ;
				}
				else
				{
					component4.cutoffFrequency = cue.m_highPassFilter.cutoffFrequency;
					component4.highpassResonanceQ = cue.m_highPassFilter.highpassResonanceQ;
				}
			}
			AudioLowPassFilter component5 = source.gameObject.GetComponent<AudioLowPassFilter>();
			component5.enabled = cue.m_lowPassFilter != null || cue.m_lowPass > 10;
			if (component5.enabled)
			{
				if (cue.m_lowPass > 10)
				{
					component5.cutoffFrequency = cue.m_lowPass;
					component5.lowpassResonanceQ = cue.m_lowPassQ;
				}
				else
				{
					component5.cutoffFrequency = cue.m_lowPassFilter.cutoffFrequency;
					component5.lowpassResonanceQ = cue.m_lowPassFilter.lowpassResonanceQ;
				}
			}
			AudioChorusFilter component6 = source.gameObject.GetComponent<AudioChorusFilter>();
			component6.enabled = cue.m_chorusFilter != null;
			if (component6.enabled)
			{
				component6.delay = cue.m_chorusFilter.delay;
				component6.depth = cue.m_chorusFilter.depth;
				component6.dryMix = cue.m_chorusFilter.dryMix;
				component6.wetMix1 = cue.m_chorusFilter.wetMix1;
				component6.wetMix2 = cue.m_chorusFilter.wetMix2;
				component6.wetMix3 = cue.m_chorusFilter.wetMix3;
				component6.rate = cue.m_chorusFilter.rate;
			}
			if (cue.m_mixerGroup != null)
			{
				source.outputAudioMixerGroup = cue.m_mixerGroup;
			}
			else
			{
				source.outputAudioMixerGroup = outputAudioMixerGroup;
			}
			source.transform.parent = get.transform;
			source.transform.localPosition = Vector3.zero;
			get.SetSource(ref source, sound, num, num2, random, 128, emmitter);
			source.loop = cue.m_loop;
			if (fromTime <= 0f && clip.m_startTime > 0f)
			{
				fromTime = clip.m_startTime;
			}
			if (fromTime > 0f)
			{
				source.time = Mathf.Clamp(fromTime, 0f, Mathf.Max(source.clip.length - 1f, source.clip.length * 0.9f));
			}
			else
			{
				source.time = 0f;
			}
			if (withStartDelay <= 0f)
			{
				withStartDelay = cue.m_startDelay.GetRandom();
			}
			if (withStartDelay > 0f)
			{
				source.PlayDelayed(withStartDelay);
			}
			else
			{
				source.Play();
				if (!source.isPlaying && source.time > 0f)
				{
					if (Debug.isDebugBuild)
					{
						Debug.LogWarning("Failed to play sound from specific time. Retrying from beginning");
					}
					source.time = 0f;
					source.Play();
				}
			}
			AudioHandle audioHandle = new AudioHandle(source, cue.name);
			if (handles != s_defaultAudioHandleList)
			{
				if (handles == null)
				{
					handles = new List<AudioHandle>();
				}
				handles.Add(audioHandle);
			}
			if (clip.m_endTime > 0f)
			{
				float num3 = clip.m_endTime;
				if (clip.m_startTime > 0f)
				{
					num3 -= clip.m_startTime;
				}
				num3 /= num2;
				if (withStartDelay > 0f)
				{
					num3 += withStartDelay;
				}
				audioHandle.source.SetScheduledEndTime(AudioSettings.dspTime + (double)num3);
			}
			get.AddActiveAudio(new ClipInfo
			{
				handle = audioHandle,
				cue = cue,
				type = cue.m_type,
				defaultVolume = num,
				targetVolume = num,
				defaultPitch = num2,
				defaultPan = random,
				startFromTime = fromTime,
				emmitter = ((emmitter == null) ? get.transform : emmitter)
			});
			if (cue.m_alsoPlay != null)
			{
				Play(cue.m_alsoPlay, ref handles, emmitter);
			}
			return audioHandle;
		}

		public static AudioHandle Play(AudioClip clip, int type = 1, Transform emmitter = null, float volume = 1f, float pitch = 1f, bool loop = false, AudioMixerGroup mixerGroup = null)
		{
			if (clip == null)
			{
				return null;
			}
			SystemAudio get = SingletonAuto<SystemAudio>.Get;
			for (int i = 0; i < get.m_activeAudio.Count; i++)
			{
				ClipInfo clipInfo = get.m_activeAudio[i];
				if (clipInfo.handle != null && clipInfo.handle.clip == clip && clipInfo.handle.time < get.m_noDuplicateTime)
				{
					return null;
				}
			}
			AudioMixerGroup outputAudioMixerGroup = null;
			for (int j = 0; j < get.m_volumeByType.Count; j++)
			{
				if (((uint)get.m_volumeByType[j].m_type & (uint)type) != 0)
				{
					volume *= get.m_volumeByType[j].m_volume;
					outputAudioMixerGroup = get.m_volumeByType[j].m_mixerGroup;
				}
			}
			AudioSource source = get.SpawnAudioSource(Debug.isDebugBuild ? (STRING_NAME_PREFIX + clip.name) : STRING_NAME_PREFIX, get.transform.position);
			AudioReverbFilter component = source.gameObject.GetComponent<AudioReverbFilter>();
			if (component != null)
			{
				component.enabled = false;
			}
			AudioEchoFilter component2 = source.gameObject.GetComponent<AudioEchoFilter>();
			if (component2 != null)
			{
				component2.enabled = false;
			}
			AudioDistortionFilter component3 = source.gameObject.GetComponent<AudioDistortionFilter>();
			if (component3 != null)
			{
				component3.enabled = false;
			}
			AudioHighPassFilter component4 = source.gameObject.GetComponent<AudioHighPassFilter>();
			if (component4 != null)
			{
				component4.enabled = false;
			}
			AudioLowPassFilter component5 = source.gameObject.GetComponent<AudioLowPassFilter>();
			if (component5 != null)
			{
				component5.enabled = false;
			}
			AudioChorusFilter component6 = source.gameObject.GetComponent<AudioChorusFilter>();
			if (component6 != null)
			{
				component6.enabled = false;
			}
			if (mixerGroup != null)
			{
				source.outputAudioMixerGroup = mixerGroup;
			}
			else
			{
				source.outputAudioMixerGroup = outputAudioMixerGroup;
			}
			source.transform.parent = get.transform;
			source.transform.localPosition = Vector3.zero;
			get.SetSource(ref source, clip, volume, pitch, 0f, 0, emmitter);
			source.loop = loop;
			source.time = 0f;
			source.Play();
			if (emmitter == null)
			{
				emmitter = get.transform;
			}
			AudioHandle audioHandle = new AudioHandle(source);
			get.AddActiveAudio(new ClipInfo
			{
				handle = audioHandle,
				cue = null,
				type = type,
				defaultVolume = volume,
				targetVolume = volume,
				defaultPitch = pitch,
				defaultPan = 0f,
				emmitter = ((emmitter == null) ? get.transform : emmitter)
			});
			return audioHandle;
		}

		public static void Pause(AudioHandle handle)
		{
			if (handle != null)
			{
				ClipInfo clipInfo = SingletonAuto<SystemAudio>.m_instance.m_activeAudio.Find((ClipInfo clip) => clip.handle == handle);
				if (clipInfo != null && !clipInfo.paused)
				{
					clipInfo.paused = true;
					handle.source.Pause();
				}
			}
		}

		public static void UnPause(AudioHandle handle)
		{
			if (handle != null)
			{
				ClipInfo clipInfo = SingletonAuto<SystemAudio>.m_instance.m_activeAudio.Find((ClipInfo clip) => clip.handle == handle);
				if (clipInfo != null && clipInfo.paused)
				{
					clipInfo.paused = false;
					handle.source.Play();
				}
			}
		}

		public static void Pause(string cueName)
		{
			foreach (ClipInfo item in SingletonAuto<SystemAudio>.m_instance.m_activeAudio)
			{
				if (item.cue != null && string.Equals(item.cue.name, cueName, StringComparison.OrdinalIgnoreCase))
				{
					Pause(item.handle);
				}
			}
		}

		public static void UnPause(string cueName)
		{
			foreach (ClipInfo item in SingletonAuto<SystemAudio>.m_instance.m_activeAudio)
			{
				if (item.cue != null && string.Equals(item.cue.name, cueName, StringComparison.OrdinalIgnoreCase))
				{
					UnPause(item.handle);
				}
			}
		}

		public static AudioHandle GetHandle(string cueName)
		{
			return SingletonAuto<SystemAudio>.Get.m_activeAudio.Find((ClipInfo item) => item.cue != null && string.Equals(item.cue.name, cueName, StringComparison.OrdinalIgnoreCase))?.handle;
		}

		public static AudioHandle[] GetHandles(string cueName)
		{
			List<ClipInfo> list = SingletonAuto<SystemAudio>.m_instance.m_activeAudio.FindAll((ClipInfo item) => item.cue != null && string.Equals(item.cue.name, cueName, StringComparison.OrdinalIgnoreCase));
			if (list != null && list.Count > 0)
			{
				AudioHandle[] array = new AudioHandle[list.Count];
				for (int num = 0; num < list.Count; num++)
				{
					array[num] = list[num].handle;
				}
				return array;
			}
			return null;
		}

		public static void Stop(string cueName, float overTime = 0f)
		{
			foreach (ClipInfo item in SingletonAuto<SystemAudio>.m_instance.m_activeAudio)
			{
				if (item != null && item.cue != null && string.Equals(item.cue.name, cueName, StringComparison.OrdinalIgnoreCase))
				{
					Stop(item.handle, overTime);
				}
			}
		}

		public static void Stop(AudioHandle handle, float overTime = 0f, float afterDelay = 0f)
		{
			handle?.Stop(overTime, afterDelay);
		}

		public bool StopHandleInternal(AudioHandle handle, float overTime = 0f, float afterDelay = 0f)
		{
			if (handle == null || handle.source == null)
			{
				return false;
			}
			ClipInfo clipInfo = m_activeAudio.Find((ClipInfo clip) => clip.handle == handle);
			if (overTime > 0f || afterDelay > 0f)
			{
				if (clipInfo != null)
				{
					clipInfo.stopAfterTime = afterDelay;
				}
				if (afterDelay > 0f && overTime <= 0f)
				{
					overTime = 0.0001f;
				}
				StartFade(handle, 0f, overTime, stopOnFinish: true);
				return false;
			}
			if (clipInfo != null)
			{
				clipInfo.stopAfterFade = true;
			}
			handle.source.Stop();
			handle.source.gameObject.SetActive(value: false);
			return true;
		}

		public static AudioHandle PlayMusic(string cueName, float fadeTime = 0f)
		{
			return PlayMusic(cueName, fadeTime, fadeTime);
		}

		public static AudioHandle PlayMusic(string cueName, float fadeOutTime, float fadeInTime)
		{
			AudioCue audioCue = SingletonAuto<SystemAudio>.m_instance.m_audioCues.Find((AudioCue item) => string.Equals(cueName, item.name, StringComparison.OrdinalIgnoreCase));
			if (audioCue == null && Debug.isDebugBuild && !string.IsNullOrEmpty(cueName))
			{
				Debug.LogWarning("Music sound cue not found: " + cueName);
			}
			return PlayMusic(audioCue, fadeOutTime, fadeInTime);
		}

		public static AudioHandle PlayMusic(AudioCue cue, float fadeTime = 0f)
		{
			return PlayMusic(cue, fadeTime, fadeTime);
		}

		public static AudioHandle PlayMusic(AudioCue cue, float fadeOutTime, float fadeInTime)
		{
			if (!SingletonAuto<SystemAudio>.m_instance.m_restartMusicIfAlreadyPlaying && SingletonAuto<SystemAudio>.m_instance.GetIsActiveMusic(cue))
			{
				SingletonAuto<SystemAudio>.m_instance.UpdateCurrentMusicVolumeFromCue(cue, fadeInTime);
				return SingletonAuto<SystemAudio>.m_instance.m_activeMusic;
			}
			StopMusic(fadeOutTime);
			SingletonAuto<SystemAudio>.m_instance.m_musicCueName = ((cue == null) ? null : cue.name);
			SingletonAuto<SystemAudio>.m_instance.m_musicVolOverride = 0f;
			SingletonAuto<SystemAudio>.m_instance.m_activeMusic = Play(cue);
			if (fadeInTime > 0f && cue != null)
			{
				SingletonAuto<SystemAudio>.m_instance.StartFadeIn(SingletonAuto<SystemAudio>.m_instance.m_activeMusic, fadeInTime);
			}
			return SingletonAuto<SystemAudio>.m_instance.m_activeMusic;
		}

		public static AudioSource PlayMusicSynced(string name, float fadeTime, float volumeOverride = 0f)
		{
			return PlayMusicSynced(SingletonAuto<SystemAudio>.m_instance.m_audioCues.Find((AudioCue item) => string.Equals(name, item.name, StringComparison.OrdinalIgnoreCase)), fadeTime, volumeOverride);
		}

		public static AudioSource PlayMusicSynced(AudioCue cue, float fadeTime, float volumeOverride = 0f)
		{
			if (SingletonAuto<SystemAudio>.m_instance.m_activeMusic == null)
			{
				return PlayMusic(cue);
			}
			if (!SingletonAuto<SystemAudio>.m_instance.m_restartMusicIfAlreadyPlaying && SingletonAuto<SystemAudio>.m_instance.GetIsActiveMusic(cue))
			{
				SingletonAuto<SystemAudio>.m_instance.UpdateCurrentMusicVolumeFromCue(cue, fadeTime, volumeOverride);
				return SingletonAuto<SystemAudio>.m_instance.m_activeMusic;
			}
			float fromTime = SingletonAuto<SystemAudio>.m_instance.m_activeMusic.time + 0.25f;
			StopMusic(fadeTime * 1.5f, 0.25f);
			SingletonAuto<SystemAudio>.m_instance.m_musicCueName = cue.name;
			SingletonAuto<SystemAudio>.m_instance.m_activeMusic = Play(cue, null, 1f, 1f, fromTime, 0.25f);
			if (volumeOverride > 0f)
			{
				SingletonAuto<SystemAudio>.m_instance.SetVolume(SingletonAuto<SystemAudio>.m_instance.m_activeMusic, volumeOverride);
				SingletonAuto<SystemAudio>.m_instance.m_musicVolOverride = volumeOverride;
			}
			SingletonAuto<SystemAudio>.m_instance.StartFadeIn(SingletonAuto<SystemAudio>.m_instance.m_activeMusic, fadeTime);
			return SingletonAuto<SystemAudio>.m_instance.m_activeMusic;
		}

		public static void StopMusic(float fadeTime = 0f, float afterDelay = 0f)
		{
			Stop(SingletonAuto<SystemAudio>.Get.m_activeMusic, fadeTime, afterDelay);
			SingletonAuto<SystemAudio>.m_instance.m_activeMusic = null;
			SingletonAuto<SystemAudio>.m_instance.m_musicCueName = null;
		}

		public static void PlayAmbientSound(string name, float fadeTime = 0.4f)
		{
			PlayAmbientSound(name, fadeTime, fadeTime);
		}

		public static void PlayAmbientSound(string name, float fadeoutTime, float fadeInTime)
		{
			StopAmbientSound(fadeoutTime);
			SingletonAuto<SystemAudio>.m_instance.m_activeAmbientLoop = Play(name);
			if (fadeInTime > 0f)
			{
				SingletonAuto<SystemAudio>.m_instance.m_activeAmbientLoop.FadeIn(fadeInTime);
			}
			SingletonAuto<SystemAudio>.m_instance.m_ambientCueName = name;
		}

		public static void StopAmbientSound(float overTime = 0.4f)
		{
			Stop(SingletonAuto<SystemAudio>.m_instance.m_activeAmbientLoop, overTime);
			SingletonAuto<SystemAudio>.m_instance.m_ambientCueName = string.Empty;
		}

		public static void UpdateCustomFalloff(string cueName, Vector2 soundPos, Vector2 listenerPos, float closeDist, float farDist, float farVol = 0f, float closeVol = 1f, float farPan = 0.7f)
		{
			AudioHandle fireHandle = GetHandle(cueName);
			if (fireHandle == null)
			{
				return;
			}
			ClipInfo clipInfo = SingletonAuto<SystemAudio>.m_instance.m_activeAudio.Find((ClipInfo item) => item.handle == fireHandle);
			if (clipInfo != null && !clipInfo.stopAfterFade)
			{
				float f = soundPos.x - listenerPos.x;
				float num = Mathf.Lerp(closeVol, farVol, Utils.EaseCubic(Mathf.InverseLerp(closeDist, farDist, Vector2.Distance(soundPos, listenerPos))));
				float num2 = Mathf.Lerp(0f, farPan, Utils.EaseCubic(Mathf.InverseLerp(closeDist, farDist, Mathf.Abs(f))));
				num2 *= Mathf.Sign(f);
				if (clipInfo.targetVolume != clipInfo.defaultVolume)
				{
					clipInfo.targetVolume = num;
				}
				else
				{
					fireHandle.volume = num;
				}
				fireHandle.panStereo = num2;
			}
		}

		public bool GetAnyMusicPlaying()
		{
			return m_activeAudio.Exists((ClipInfo item) => (item.type & 2) > 0);
		}

		public float GetCueVolume(AudioCue cue, AudioClip specificClip = null)
		{
			if (cue == null)
			{
				return 0f;
			}
			SystemAudio get = SingletonAuto<SystemAudio>.Get;
			AudioCue.Clip clip = cue.GetClipData(specificClip);
			if (clip == null)
			{
				clip = cue.GetClip();
			}
			float num = cue.m_volume.GetRandom() * clip.m_volume.GetRandom() * 1f;
			for (int i = 0; i < get.m_volumeByType.Count; i++)
			{
				if (((uint)get.m_volumeByType[i].m_type & (uint)cue.m_type) != 0)
				{
					num *= get.m_volumeByType[i].m_volume;
				}
			}
			return num;
		}

		public void PauseAllSounds(bool alsoPauseMusic = false)
		{
			foreach (ClipInfo item in m_activeAudio)
			{
				try
				{
					if (alsoPauseMusic || item.handle != m_activeMusic)
					{
						Pause(item.handle);
					}
				}
				catch
				{
				}
			}
		}

		public void ResumeAllSounds()
		{
			foreach (ClipInfo item in m_activeAudio)
			{
				try
				{
					if (!item.handle.isPlaying)
					{
						UnPause(item.handle);
					}
				}
				catch
				{
				}
			}
		}

		public AudioHandle GetActiveMusicHandle()
		{
			return m_activeMusic;
		}

		public bool GetIsActiveMusic(AudioCue cue)
		{
			if (m_activeMusic == null)
			{
				return cue == null;
			}
			if (cue == null)
			{
				return false;
			}
			return cue.GetClipData(m_activeMusic.clip) != null;
		}

		private void UpdateCurrentMusicVolumeFromCue(AudioCue cue, float fadeTime, float volumeOverride = 0f)
		{
			if (m_activeMusic != null && !(cue == null))
			{
				float random = cue.GetClipData(m_activeMusic.clip).m_volume.GetRandom();
				random *= cue.m_volume.GetRandom();
				if (volumeOverride > 0f)
				{
					random = volumeOverride;
					m_musicVolOverride = volumeOverride;
				}
				m_activeMusic.Fade(random, fadeTime);
			}
		}

		public bool EditorAddCue(AudioCue cue)
		{
			if (!m_audioCues.Contains(cue))
			{
				m_audioCues.Add(cue);
				return true;
			}
			return false;
		}

		public List<AudioCue> EditorGetAudioCues()
		{
			return m_audioCues;
		}

		public bool EditorGetAutoAddCues()
		{
			return m_autoAddCues;
		}

		public void StartFadeIn(AudioHandle handle, float time)
		{
			float volume = GetVolume(handle);
			SetVolume(handle, 0f);
			StartFade(handle, volume, time);
		}

		public void StartFadeOut(AudioHandle handle, float time, bool stopOnFinish = false)
		{
			StartFade(handle, 0f, time, stopOnFinish: true);
		}

		public void StartFade(AudioHandle handle, float targetVolume, float time, bool stopOnFinish = false)
		{
			ClipInfo clipInfo = m_activeAudio.Find((ClipInfo clip) => clip.handle == handle);
			if (clipInfo == null)
			{
				return;
			}
			float targetVolume2 = clipInfo.targetVolume;
			clipInfo.targetVolume = targetVolume;
			clipInfo.stopAfterFade = stopOnFinish;
			if (time <= 0f)
			{
				clipInfo.defaultVolume = targetVolume;
				if (stopOnFinish)
				{
					handle.Stop();
				}
				return;
			}
			float num = Mathf.Abs(targetVolume - targetVolume2);
			if (num <= 0f)
			{
				if (stopOnFinish)
				{
					handle.Stop();
				}
			}
			else
			{
				clipInfo.fadeDelta = num / time;
			}
		}

		public object GetSaveData()
		{
			SaveData saveData = new SaveData();
			saveData.m_musicCueName = m_musicCueName;
			saveData.m_musicVolOverride = m_musicVolOverride;
			saveData.m_ambientCueName = m_ambientCueName;
			saveData.m_restartMusicIfAlreadyPlaying = m_restartMusicIfAlreadyPlaying;
			saveData.m_falloffDistanceMultiplier = m_falloffDistanceMultiplier;
			saveData.m_musicTime = ((m_activeMusic == null) ? 0f : m_activeMusic.time);
			saveData.activeAudio = new SaveData.ActiveAudioSaveData[m_activeAudio.Count];
			for (int i = 0; i < m_activeAudio.Count; i++)
			{
				ClipInfo clipInfo = m_activeAudio[i];
				if (clipInfo != null && clipInfo.cue != null && clipInfo.handle.isPlaying && clipInfo.handle != m_activeMusic && clipInfo.handle != m_activeAmbientLoop)
				{
					saveData.activeAudio[i] = new SaveData.ActiveAudioSaveData(clipInfo);
				}
			}
			return saveData;
		}

		public void RestoreSaveData(object obj)
		{
			if (obj == null || !(obj is SaveData))
			{
				return;
			}
			SaveData saveData = obj as SaveData;
			m_restartMusicIfAlreadyPlaying = saveData.m_restartMusicIfAlreadyPlaying;
			m_falloffDistanceMultiplier = saveData.m_falloffDistanceMultiplier;
			foreach (ClipInfo item in m_activeAudio)
			{
				if (item != null && item.handle != null)
				{
					Stop(item.handle, 0.1f);
				}
			}
			if (saveData.activeAudio != null)
			{
				SaveData.ActiveAudioSaveData[] activeAudio = saveData.activeAudio;
				foreach (SaveData.ActiveAudioSaveData activeAudioSaveData in activeAudio)
				{
					if (activeAudioSaveData != null)
					{
						AudioHandle handle = Play(activeAudioSaveData.cueName);
						ClipInfo clipInfo = SingletonAuto<SystemAudio>.m_instance.m_activeAudio.Find((ClipInfo item) => item.handle == handle);
						activeAudioSaveData.InitClipInfo(clipInfo);
						handle.time = activeAudioSaveData.time;
						handle.source.panStereo = activeAudioSaveData.pan;
						if (activeAudioSaveData.paused)
						{
							Pause(handle);
						}
					}
				}
			}
			if (string.IsNullOrEmpty(saveData.m_musicCueName))
			{
				StopMusic(0.1f);
				m_musicVolOverride = -1f;
			}
			else if (PlayMusic(saveData.m_musicCueName) != null)
			{
				if (saveData.m_musicVolOverride > 0f)
				{
					m_musicVolOverride = saveData.m_musicVolOverride;
					SetVolume(SingletonAuto<SystemAudio>.m_instance.m_activeMusic, m_musicVolOverride);
				}
				if (m_activeMusic != null && m_activeMusic.clip != null)
				{
					m_activeMusic.time = saveData.m_musicTime % Mathf.Max(m_activeMusic.clip.length - 1f, m_activeMusic.clip.length * 0.9f);
				}
				SingletonAuto<SystemAudio>.m_instance.StartFadeIn(SingletonAuto<SystemAudio>.m_instance.m_activeMusic, 0.1f);
			}
			if (string.IsNullOrEmpty(saveData.m_ambientCueName))
			{
				StopAmbientSound(0.1f);
			}
			else
			{
				PlayAmbientSound(saveData.m_ambientCueName);
			}
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
			AudioListener audioListener = Array.Find(UnityEngine.Object.FindObjectsOfType<AudioListener>(), (AudioListener item) => item.enabled);
			if (audioListener != null)
			{
				m_audioListener = audioListener.transform;
			}
			if (m_audioListener == null && Debug.isDebugBuild)
			{
				Debug.Log("Unable to find audio listener in scene");
			}
		}

		private void Awake()
		{
			SceneManager.sceneLoaded += OnSceneLoaded;
			SetSingleton();
			if (Application.isPlaying)
			{
				UnityEngine.Object.DontDestroyOnLoad(this);
			}
			if (LAYER_NOPAUSE == -1)
			{
				LAYER_NOPAUSE = LayerMask.NameToLayer("NoPause");
			}
			if (Application.isPlaying)
			{
				for (int i = 0; i < AUDIO_SOURCE_POOL_SIZE; i++)
				{
					GameObject gameObject = new GameObject(STRING_NAME_PREFIX);
					m_audioSources.Add(gameObject.AddComponent<AudioSource>());
					gameObject.layer = LAYER_NOPAUSE;
					gameObject.SetActive(value: false);
					gameObject.transform.parent = base.transform;
				}
			}
			m_audioCues.RemoveAll((AudioCue item) => item == null);
			if (m_audioListener == null)
			{
				AudioListener audioListener = (AudioListener)UnityEngine.Object.FindObjectOfType(typeof(AudioListener));
				if (audioListener != null)
				{
					m_audioListener = audioListener.transform;
				}
			}
			if (m_audioListener == null && Debug.isDebugBuild)
			{
				Debug.LogWarning("Unable to find audio listener in scene");
			}
			m_activeMusic = null;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			m_hasFocus = hasFocus;
		}

		private void Update()
		{
			if (m_audioListener == null)
			{
				AudioListener audioListener = (AudioListener)UnityEngine.Object.FindObjectOfType(typeof(AudioListener));
				if (audioListener != null)
				{
					m_audioListener = audioListener.transform;
				}
			}
			if (!(m_audioListener == null))
			{
				base.transform.position = m_audioListener.position;
				UpdateActiveAudio();
			}
		}

		private float GetFalloff(Vector2 soundPos)
		{
			if (m_cameraGame == null)
			{
				return 1f;
			}
			float num = m_cameraGame.orthographicSize * m_cameraGame.aspect * m_falloffStart * m_falloffDistanceMultiplier;
			float num2 = num * m_falloffEnd * m_falloffDistanceMultiplier;
			return Mathf.Lerp(1f, m_falloffMinVol, Utils.EaseCubic((Mathf.Abs(soundPos.x - m_cameraGame.transform.position.x) - num) / num2));
		}

		private float GetPanPos(Vector2 soundPos)
		{
			if (m_cameraGame == null)
			{
				return 0f;
			}
			float f = soundPos.x - m_cameraGame.transform.position.x;
			float num = m_cameraGame.orthographicSize * m_cameraGame.aspect * m_falloffDistanceMultiplier;
			float ratio = Mathf.InverseLerp(m_falloffPanStart * num, m_falloffPanEnd * num, Mathf.Abs(f));
			return Mathf.Lerp(0f, m_falloffPanMax, Utils.EaseCubic(ratio)) * Mathf.Sign(f);
		}

		private void SetSource(ref AudioSource source, AudioClip clip, float volume, float pitch, float panStereo, int priority, Transform emmitter)
		{
			source.spatialize = false;
			source.priority = priority;
			source.pitch = pitch;
			source.clip = clip;
			source.playOnAwake = false;
			if ((bool)emmitter)
			{
				source.volume = volume * GetFalloff(emmitter.position);
				source.panStereo = Mathf.Clamp(panStereo + GetPanPos(emmitter.position), -1f, 1f);
			}
			else
			{
				source.volume = volume;
				source.panStereo = panStereo;
			}
		}

		private void AddActiveAudio(ClipInfo info)
		{
			m_activeAudio.Add(info);
			UpdateActiveAudioClip(info);
		}

		private void UpdateActiveAudioClipLoopPoints(ClipInfo audioClip)
		{
			if (audioClip.paused)
			{
				return;
			}
			if (audioClip.stopAfterTime > 0f)
			{
				audioClip.stopAfterTime -= Time.deltaTime;
			}
			if (audioClip.handle == null || audioClip.cue == null)
			{
				return;
			}
			AudioCue.LoopSection loopSection = audioClip.cue.m_loopSection;
			float time = audioClip.handle.time;
			float endTime = loopSection.m_endTime;
			if (!(endTime > 0f))
			{
				return;
			}
			if (loopSection.m_startTime >= endTime)
			{
				Debug.LogError("Loop point start time after end time");
				return;
			}
			endTime = Mathf.Max(loopSection.m_startTime + 0.5f, loopSection.m_endTime);
			if (audioClip.handle.source != null && audioClip.handle.source.clip != null)
			{
				endTime = Mathf.Min(endTime, audioClip.handle.source.clip.length);
			}
			if (endTime > 0f && (!audioClip.handle.isPlaying || endTime - time < 1f) && !audioClip.stopAfterFade)
			{
				float num = Mathf.Max(0f, endTime - time);
				if (!audioClip.handle.isPlaying)
				{
					num = 0f;
				}
				Stop(audioClip.handle, Mathf.Max(0.067f, loopSection.m_fadeOut), num);
				AudioHandle audioHandle = Play(audioClip.cue, null, 1f, 1f, loopSection.m_startTime, num).FadeIn(Mathf.Max(0.033f, loopSection.m_fadeIn));
				if (m_activeMusic == audioClip.handle)
				{
					m_activeMusic = audioHandle;
				}
				if (m_activeAmbientLoop == audioClip.handle)
				{
					m_activeAmbientLoop = audioHandle;
				}
			}
		}

		private void UpdateActiveAudio()
		{
			if (!m_hasFocus)
			{
				return;
			}
			if (m_cameraGame == null)
			{
				m_cameraGame = Camera.main;
			}
			bool num = GetVolume(AudioCue.eAudioType.Dialog) > 0.5f && m_activeAudio.Exists((ClipInfo item) => item != null && (item.type & 4) != 0);
			float duckingVolume = 1f;
			if (num)
			{
				duckingVolume = m_musicDuckingMaxVolume;
			}
			List<ClipInfo> list = new List<ClipInfo>();
			for (int num2 = 0; num2 < m_activeAudio.Count; num2++)
			{
				ClipInfo clipInfo = m_activeAudio[num2];
				UpdateActiveAudioClipLoopPoints(clipInfo);
				if (clipInfo.handle == null || (!clipInfo.handle.isPlaying && !clipInfo.paused))
				{
					list.Add(clipInfo);
				}
				else
				{
					UpdateActiveAudioClip(clipInfo, duckingVolume);
				}
			}
			foreach (ClipInfo item in list)
			{
				m_activeAudio.Remove(item);
				if (item.handle != null)
				{
					item.handle.Stop();
				}
			}
		}

		private void UpdateActiveAudioClip(ClipInfo audioClip, float duckingVolume = 1f)
		{
			float num = 1f;
			float num2 = 0f;
			if (audioClip.emmitter != null && audioClip.emmitter != base.transform)
			{
				num *= GetFalloff(audioClip.emmitter.position);
				num2 = GetPanPos(audioClip.emmitter.position);
			}
			if (!Utils.ApproximatelyZero(audioClip.fadeDelta))
			{
				bool num3 = audioClip.defaultVolume <= 0f && audioClip.targetVolume > 0f && audioClip.handle.source.time <= audioClip.startFromTime;
				bool flag = audioClip.stopAfterTime > 0f && audioClip.stopAfterFade;
				if (!num3 && !flag)
				{
					audioClip.defaultVolume = Mathf.MoveTowards(audioClip.defaultVolume, audioClip.targetVolume, audioClip.fadeDelta * Time.deltaTime);
					if (audioClip.stopAfterFade && Mathf.Approximately(audioClip.targetVolume, audioClip.defaultVolume) && audioClip.handle.isPlaying)
					{
						audioClip.handle.source.Stop();
					}
				}
			}
			if (duckingVolume < 1f && (audioClip.type & 2) != 0 && duckingVolume < audioClip.defaultVolume)
			{
				num = duckingVolume / audioClip.defaultVolume;
			}
			audioClip.handle.source.volume = audioClip.defaultVolume * num;
			audioClip.handle.source.panStereo = Mathf.Clamp(audioClip.defaultPan + num2, -1f, 1f);
		}

		private AudioSource SpawnAudioSource(string name, Vector2 position)
		{
			AudioSource audioSource = null;
			for (int i = 0; i < m_audioSources.Count; i++)
			{
				audioSource = m_audioSources[i];
				if (audioSource == null)
				{
					GameObject obj = new GameObject(STRING_NAME_PREFIX);
					audioSource = obj.AddComponent<AudioSource>();
					obj.layer = LAYER_NOPAUSE;
					m_audioSources[i] = audioSource;
					obj.SetActive(value: false);
					obj.transform.parent = base.transform;
					break;
				}
				if (!audioSource.gameObject.activeSelf)
				{
					break;
				}
				audioSource = null;
			}
			if (audioSource == null)
			{
				GameObject obj2 = new GameObject(STRING_NAME_PREFIX);
				audioSource = obj2.AddComponent<AudioSource>();
				obj2.layer = LAYER_NOPAUSE;
				m_audioSources.Add(audioSource);
				obj2.SetActive(value: false);
				obj2.transform.parent = base.transform;
			}
			if (audioSource != null)
			{
				if (audioSource.isPlaying)
				{
					Debug.Log("Reusing audio Source that's playing: " + audioSource.clip.name);
				}
				audioSource.gameObject.SetActive(value: true);
				if (Debug.isDebugBuild)
				{
					audioSource.gameObject.name = name;
				}
				audioSource.gameObject.transform.position = position;
			}
			else if (Debug.isDebugBuild)
			{
				Debug.Log("Failed to spawn audio source");
			}
			return audioSource;
		}

		private void AddSourceFilters(GameObject source)
		{
			source.AddComponent<AudioLowPassFilter>().enabled = false;
			source.AddComponent<AudioHighPassFilter>().enabled = false;
			source.AddComponent<AudioEchoFilter>().enabled = false;
			source.AddComponent<AudioChorusFilter>().enabled = false;
			source.AddComponent<AudioDistortionFilter>().enabled = false;
			source.AddComponent<AudioReverbFilter>().enabled = false;
		}

		public static float ToDecibel(float linear)
		{
			linear = Mathf.Clamp01(linear);
			if (linear == 0f)
			{
				return -144f;
			}
			return 20f * Mathf.Log10(linear);
		}

		public static float FromDecibel(float dB)
		{
			return Mathf.Pow(10f, dB * 0.05f);
		}
	}
}
