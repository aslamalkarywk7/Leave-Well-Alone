using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;

namespace PowerTools.Quest
{
	public class SystemText : Singleton<SystemText>
	{
		public enum ePlayerName
		{
			Character = 0,
			Plr = 1,
			Player = 2,
			Ego = 3
		}

		public enum eDefaultTextSource
		{
			Script = 0,
			ImportedText = 1
		}

		public class CharacterTextDataList : Dictionary<string, List<TextData>>
		{
		}

		[SerializeField]
		private LanguageData[] m_languages = new LanguageData[1]
		{
			new LanguageData()
		};

		[SerializeField]
		private eDefaultTextSource m_defaultTextSource;

		[Tooltip("Optional extended mouth shapes, eg: GHX")]
		[SerializeField]
		private string m_lipSyncExtendedShapes = "X";

		[SerializeField]
		[HideInInspector]
		private List<TextData> m_strings = new List<TextData>();

		[SerializeField]
		private Encoding m_csvEncoding = Encoding.Default;

		private CharacterTextDataList m_characterStrings;

		private List<TextData> m_stringsCopy;

		private CharacterTextDataList m_characterStringsCopy;

		private Dictionary<string, TextData> m_textOnlyStrings;

		private int m_currLanguage;

		private bool m_lipSyncUsesXShape;

		private ePlayerName m_lastPlayerName;

		public eDefaultTextSource EditorDefaultTextSource
		{
			get
			{
				return m_defaultTextSource;
			}
			set
			{
				m_defaultTextSource = value;
			}
		}

		public ePlayerName LastPlayerName
		{
			get
			{
				return m_lastPlayerName;
			}
			set
			{
				m_lastPlayerName = value;
			}
		}

		public int GetNumLanguages()
		{
			return m_languages.Length;
		}

		public int GetLanguage()
		{
			return m_currLanguage;
		}

		public int GetLanguageId(string languageCode)
		{
			return Array.FindIndex(GetLanguages(), (LanguageData item) => string.Equals(item.m_code, languageCode, StringComparison.OrdinalIgnoreCase));
		}

		public LanguageData GetLanguageData()
		{
			return m_languages[m_currLanguage];
		}

		public LanguageData GetLanguageData(int id)
		{
			return m_languages[id];
		}

		public LanguageData GetLanguageData(string languageCode)
		{
			return m_languages[GetLanguageId(languageCode)];
		}

		public void SetLanguage(int languageId)
		{
			m_currLanguage = languageId;
			Array.ForEach(UnityEngine.Object.FindObjectsOfType<QuestText>(includeInactive: true), delegate(QuestText item)
			{
				item.OnLanguageChange();
			});
		}

		public bool SetLanguage(string languageCode)
		{
			int languageId = GetLanguageId(languageCode);
			if (languageId < 0)
			{
				Debug.LogWarning("Couldn't find language code: " + languageCode + ", The code needs to be added to SystemText");
				return false;
			}
			Singleton<SystemText>.Get.SetLanguage(languageId);
			return true;
		}

		public LanguageData[] GetLanguages()
		{
			return m_languages;
		}

		public bool GetLipsyncUsesXShape()
		{
			return m_lipSyncUsesXShape;
		}

		public string GetLipsyncExtendedMouthShapes()
		{
			return m_lipSyncExtendedShapes;
		}

		public void SetLipsyncExtendedMouthShapes(string value)
		{
			m_lipSyncExtendedShapes = value;
		}

		public static string Localize(string defaultText, int id = -1, string characterName = null)
		{
			return GetDisplayText(defaultText, id, characterName);
		}

		public static string GetDisplayText(string defaultText, int id = -1, string characterName = null, bool isPlayer = false)
		{
			if (Singleton<SystemText>.m_instance == null || defaultText == null)
			{
				return defaultText;
			}
			Singleton<SystemText>.m_instance.UpdateTextDataLists();
			TextData value = null;
			if (id < 0)
			{
				id = Singleton<SystemText>.m_instance.ParseIdFromText(ref defaultText);
			}
			if (id < 0)
			{
				Singleton<SystemText>.m_instance.m_textOnlyStrings.TryGetValue(defaultText, out value);
			}
			else
			{
				if (isPlayer && Singleton<SystemText>.m_instance.LastPlayerName != ePlayerName.Character)
				{
					characterName = Singleton<SystemText>.m_instance.LastPlayerName.ToString();
				}
				value = Singleton<SystemText>.m_instance.FindTextDataInternal(id, characterName);
			}
			if (value == null)
			{
				return defaultText;
			}
			int num = Singleton<SystemText>.m_instance.m_currLanguage - 1;
			if (num >= 0 && Singleton<SystemText>.m_instance.m_currLanguage < Singleton<SystemText>.m_instance.m_languages.Length && num < value.m_translations.Length && !string.IsNullOrEmpty(value.m_translations[num]))
			{
				return value.m_translations[num];
			}
			if (Singleton<SystemText>.m_instance.m_defaultTextSource == eDefaultTextSource.ImportedText && !string.IsNullOrEmpty(value.m_string))
			{
				return value.m_string;
			}
			return defaultText;
		}

		public static AudioHandle PlayAudio(int id, string characterName, Transform emitter = null, AudioMixerGroup mixerGroupOverride = null)
		{
			if (Singleton<SystemText>.m_instance.FindTextDataInternal(id, characterName) == null)
			{
				if (Debug.isDebugBuild && id >= 0)
				{
					Debug.LogWarning("Text id " + characterName + id + " is missing. You need to run 'Process Text From Scripts' to add ids!");
				}
				return null;
			}
			return SystemAudio.Play(Singleton<SystemText>.m_instance.GetVoiceAudioClip(id, characterName), 4, emitter, 1f, 1f, loop: false, mixerGroupOverride);
		}

		public static TextData FindTextData(int id, string characterName = null)
		{
			if (Singleton<SystemText>.m_instance == null)
			{
				return null;
			}
			return Singleton<SystemText>.m_instance.FindTextDataInternal(id, characterName);
		}

		public int ParseIdFromText(ref string text)
		{
			if (string.IsNullOrEmpty(text) || text[0] != '&')
			{
				return -1;
			}
			int num = text.IndexOf(' ', 1);
			if (num < 1)
			{
				return -1;
			}
			if (!int.TryParse(text.Substring(1, num), out var result))
			{
				return -1;
			}
			text = text.Substring(num + 1);
			return result;
		}

		public void EditorOnBeginAddText()
		{
			UpdateTextDataLists();
			m_stringsCopy = m_strings;
			m_strings = new List<TextData>(m_stringsCopy.Count);
			m_characterStringsCopy = m_characterStrings;
			m_characterStrings = new CharacterTextDataList();
		}

		public TextData EditorAddText(string line, string sourceFile = null, string sourceFunction = null, string characterName = null, int existingId = -1, bool preserveExistingIds = false)
		{
			List<TextData> value = null;
			if (characterName == null)
			{
				characterName = string.Empty;
			}
			if (!m_characterStrings.TryGetValue(characterName, out value))
			{
				value = new List<TextData>();
				m_characterStrings.Add(characterName, value);
			}
			int newId = value.Count;
			if (existingId == -1)
			{
				existingId = ParseIdFromText(ref line);
			}
			if (preserveExistingIds)
			{
				if (existingId != -1)
				{
					newId = existingId;
				}
				else
				{
					List<TextData> value2 = null;
					List<TextData> value3 = null;
					if (m_characterStringsCopy != null)
					{
						m_characterStringsCopy.TryGetValue(characterName, out value2);
					}
					m_characterStrings.TryGetValue(characterName, out value3);
					while ((value2 != null && value2.Exists((TextData item) => item.m_id == newId)) || (value3 != null && value3.Exists((TextData item) => item.m_id == newId)))
					{
						int num = newId + 1;
						newId = num;
					}
				}
			}
			TextData textData = new TextData
			{
				m_id = newId,
				m_character = characterName,
				m_orderId = m_strings.Count,
				m_string = line,
				m_sourceFile = sourceFile,
				m_sourceFunction = sourceFunction
			};
			if (existingId >= 0)
			{
				TextData textData2 = FindTextDataCopy(existingId, characterName);
				if (textData2 != null)
				{
					textData.m_translations = textData2.m_translations;
					textData.m_phonesCharacter = textData2.m_phonesCharacter;
					textData.m_phonesTime = textData2.m_phonesTime;
					if (textData.m_string != textData2.m_string)
					{
						textData.m_changedSinceImport = true;
					}
				}
			}
			m_strings.Add(textData);
			value.Add(textData);
			return textData;
		}

		public bool EditorGetShouldImportDefaultStringFromCSV()
		{
			return true;
		}

		public List<TextData> EditorGetTextDataOrdered()
		{
			return m_strings;
		}

		public TextData EditorFindText(string defaultText, int id = -1, string characterName = null)
		{
			TextData value = null;
			UpdateTextDataLists();
			if (id < 0)
			{
				id = ParseIdFromText(ref defaultText);
			}
			if (id < 0)
			{
				m_textOnlyStrings.TryGetValue(defaultText, out value);
			}
			else
			{
				value = FindTextDataInternal(id, characterName);
			}
			return value;
		}

		public bool EditorHasAudio(int id, string characterName)
		{
			return GetVoiceAudioClip(id, characterName) != null;
		}

		private AudioClip GetVoiceAudioClip(int id, string characterName)
		{
			string text = characterName + id;
			UnityEngine.Object obj = Resources.Load(string.Concat("Voice/" + GetLanguageData().m_code + "/", text));
			if (obj == null)
			{
				obj = Resources.Load("Voice/" + text);
			}
			return obj as AudioClip;
		}

		private TextData FindTextDataInternal(int id, string characterName = null)
		{
			UpdateTextDataLists();
			if (characterName == null)
			{
				characterName = string.Empty;
			}
			if (m_characterStrings.TryGetValue(characterName, out var value))
			{
				foreach (TextData item in value)
				{
					if (item.m_id == id)
					{
						return item;
					}
				}
			}
			return null;
		}

		private TextData FindTextDataCopy(int id, string characterName = null)
		{
			if (characterName == null)
			{
				characterName = string.Empty;
			}
			List<TextData> value = null;
			if (m_characterStringsCopy != null)
			{
				m_characterStringsCopy.TryGetValue(characterName, out value);
			}
			return value?.Find((TextData item) => item.m_id == id);
		}

		private void Awake()
		{
			SetSingleton();
			UnityEngine.Object.DontDestroyOnLoad(this);
			m_lipSyncUsesXShape = m_lipSyncExtendedShapes.Contains("X");
		}

		private void UpdateTextDataLists()
		{
			if (m_characterStrings != null && m_textOnlyStrings != null)
			{
				return;
			}
			m_characterStrings = new CharacterTextDataList();
			m_textOnlyStrings = new Dictionary<string, TextData>();
			int count = m_strings.Count;
			for (int i = 0; i < count; i++)
			{
				TextData textData = m_strings[i];
				if (textData.m_id < 0)
				{
					m_textOnlyStrings.Add(textData.m_string, textData);
					continue;
				}
				List<TextData> value = null;
				string key = ((textData.m_character == null) ? string.Empty : textData.m_character);
				if (!m_characterStrings.TryGetValue(key, out value))
				{
					value = new List<TextData>();
					m_characterStrings.Add(key, value);
				}
				value.Add(textData);
			}
		}
	}
}
