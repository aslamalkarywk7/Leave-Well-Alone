using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PowerTools.Quest
{
	public class AudioCueSource : MonoBehaviour
	{
		private static readonly int LAYER_UI = LayerMask.NameToLayer("UI");

		public AudioCue m_playOnSpawn;

		public bool m_stopOnDestroy;

		public List<AudioCue> m_cues = new List<AudioCue>();

		private List<AudioHandle> m_currSounds = new List<AudioHandle>();

		public void AnimSound(Object cueName)
		{
			GameObject gameObject = cueName as GameObject;
			if (!(gameObject == null))
			{
				AudioCue component = gameObject.GetComponent<AudioCue>();
				if (base.gameObject.layer == LAYER_UI)
				{
					SystemAudio.Play(component, ref m_currSounds);
				}
				else
				{
					SystemAudio.Play(component, ref m_currSounds, base.transform);
				}
			}
		}

		public void AnimSound(string cueName)
		{
			if (base.gameObject.layer == LAYER_UI)
			{
				SystemAudio.Play(SystemAudio.GetCue(cueName), ref m_currSounds);
			}
			else
			{
				SystemAudio.Play(SystemAudio.GetCue(cueName), ref m_currSounds, base.transform);
			}
		}

		public void AnimSoundStop()
		{
			StopSounds();
		}

		private void Start()
		{
			OnSpawn();
		}

		private void OnSpawn()
		{
			if ((bool)m_playOnSpawn)
			{
				if (base.gameObject.layer == LAYER_UI)
				{
					SystemAudio.Play(m_playOnSpawn, ref m_currSounds);
				}
				else
				{
					SystemAudio.Play(m_playOnSpawn, ref m_currSounds, base.transform);
				}
			}
			StartCoroutine(CoroutineClearFinishedSounds());
		}

		private void OnDestroy()
		{
			if (m_stopOnDestroy)
			{
				StopSounds();
			}
		}

		private void OnDisable()
		{
			if (m_stopOnDestroy)
			{
				StopSounds();
			}
		}

		private void StopSounds()
		{
			for (int i = 0; i < m_currSounds.Count; i++)
			{
				m_currSounds[i].Stop(0.2f);
			}
			m_currSounds.Clear();
		}

		private IEnumerator CoroutineClearFinishedSounds()
		{
			while (true)
			{
				yield return new WaitForSeconds(0.2f);
				for (int num = m_currSounds.Count - 1; num >= 0; num--)
				{
					AudioSource audioSource = m_currSounds[num];
					if (audioSource == null || !audioSource.isPlaying)
					{
						m_currSounds.RemoveAt(num);
					}
				}
			}
		}
	}
}
