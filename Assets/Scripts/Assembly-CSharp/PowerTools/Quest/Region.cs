using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

namespace PowerTools.Quest
{
	[Serializable]
	public class Region : IRegion, IQuestScriptable
	{
		[Tooltip("Whether walking on region triggers events/tints characters, etc")]
		[FormerlySerializedAs("m_triggerEnabled")]
		[SerializeField]
		private bool m_enabled = true;

		[Tooltip("Whether character can walk over region, if false they'll path around it")]
		[SerializeField]
		private bool m_walkable = true;

		[Tooltip("Whether OnEnter and similar scripts affect the player only")]
		[SerializeField]
		private bool m_playerOnly;

		[Tooltip("Colour to tint the player when in this area. Alpha controls the amount of tint.")]
		[SerializeField]
		private Color m_tint = new Color(1f, 1f, 1f, 0f);

		[Tooltip("Distance a character has to move into a region before tint is fully faded in")]
		[SerializeField]
		private float m_fadeDistance;

		[Tooltip("Amount to scale the player while in the region (at the top)")]
		[SerializeField]
		private float m_scaleTop = 1f;

		[Tooltip("Amount to scale the player while in the region (at the bottom)")]
		[SerializeField]
		private float m_scaleBottom = 1f;

		[ReadOnly]
		[SerializeField]
		private string m_scriptName = "RegionNew";

		private RegionComponent m_instance;

		private BitArray m_characterOnRegionMask = new BitArray(64);

		private BitArray m_characterOnRegionMaskOld = new BitArray(64);

		private BitArray m_characterOnRegionMaskBGOld = new BitArray(64);

		public string ScriptName => m_scriptName;

		public MonoBehaviour Instance => m_instance;

		public Region Data => this;

		public bool Enabled
		{
			get
			{
				return m_enabled;
			}
			set
			{
				m_enabled = value;
			}
		}

		public bool Walkable
		{
			get
			{
				return m_walkable;
			}
			set
			{
				m_walkable = value;
				if ((bool)m_instance)
				{
					m_instance.OnSetWalkable(m_walkable);
				}
			}
		}

		public bool PlayerOnly
		{
			get
			{
				return m_playerOnly;
			}
			set
			{
				m_playerOnly = value;
			}
		}

		public Color Tint
		{
			get
			{
				return m_tint;
			}
			set
			{
				m_tint = value;
			}
		}

		public float FadeDistance
		{
			get
			{
				return m_fadeDistance;
			}
			set
			{
				m_fadeDistance = value;
			}
		}

		public float ScaleTop
		{
			get
			{
				return m_scaleTop;
			}
			set
			{
				m_scaleTop = value;
			}
		}

		public float ScaleBottom
		{
			get
			{
				return m_scaleBottom;
			}
			set
			{
				m_scaleBottom = value;
			}
		}

		public bool ContainsCharacter(ICharacter character = null)
		{
			return GetCharacterOnRegion(character);
		}

		public bool GetCharacterOnRegion(ICharacter character = null)
		{
			if (m_instance == null || m_characterOnRegionMask == null)
			{
				return false;
			}
			if (character == null)
			{
				foreach (bool item in m_characterOnRegionMask)
				{
					if (item)
					{
						return true;
					}
				}
			}
			else if (character.Data != null)
			{
				int characterId = Singleton<PowerQuest>.Get.GetCharacterId(character.Data);
				if (characterId < 0)
				{
					return false;
				}
				return m_characterOnRegionMask.Get(characterId);
			}
			return false;
		}

		public bool ContainsPoint(Vector2 position)
		{
			if (m_instance == null || m_instance.GetPolygonCollider() == null)
			{
				return false;
			}
			return m_instance.GetPolygonCollider().OverlapPoint(position);
		}

		public RegionComponent GetInstance()
		{
			return m_instance;
		}

		public void SetInstance(RegionComponent instance)
		{
			m_instance = instance;
			instance.SetData(this);
			instance.OnSetWalkable(m_walkable);
		}

		public QuestScript GetScript()
		{
			if (Singleton<PowerQuest>.Get.GetCurrentRoom() != null)
			{
				return Singleton<PowerQuest>.Get.GetCurrentRoom().GetScript();
			}
			return null;
		}

		public void EditorInitialise(string name)
		{
			m_scriptName = name;
		}

		public void EditorRename(string name)
		{
			m_scriptName = name;
		}

		public BitArray GetCharacterOnRegionMask()
		{
			return m_characterOnRegionMask;
		}

		public BitArray GetCharacterOnRegionMaskOld(bool background)
		{
			if (!background)
			{
				return m_characterOnRegionMaskOld;
			}
			return m_characterOnRegionMaskBGOld;
		}

		public string GetScriptName()
		{
			return m_scriptName;
		}

		public string GetScriptClassName()
		{
			return PowerQuest.STR_REGION + m_scriptName;
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
