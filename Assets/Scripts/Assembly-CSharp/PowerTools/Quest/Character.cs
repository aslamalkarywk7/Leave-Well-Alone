using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

namespace PowerTools.Quest
{
	[Serializable]
	public class Character : IQuestClickable, ICharacter, IQuestClickableInterface, IQuestScriptable, IQuestSaveCachable
	{
		[Serializable]
		public class CollectedItem
		{
			public string m_name = string.Empty;

			public float m_quantity = 1f;
		}

		[Serializable]
		public class FaceCharacterData
		{
			public string m_character = string.Empty;

			public float m_minTime;

			public float m_maxTime;

			public float m_timer;
		}

		public enum eState
		{
			Idle = 0,
			Walk = 1,
			Talk = 2,
			Animate = 3,
			None = 4
		}

		[QuestSave]
		private class NonSavedData
		{
			public Room m_roomCached;
		}

		public static readonly Vector2[] FACE_DIRECTIONS = new Vector2[8]
		{
			Vector2.left,
			Vector2.right,
			Vector2.down,
			Vector2.up,
			new Vector2(-1f, -1f).normalized,
			new Vector2(1f, -1f).normalized,
			new Vector2(-1f, 1f).normalized,
			Vector2.one.normalized
		};

		[Header("Mouse-over Defaults")]
		[TextArea(1, 10)]
		[SerializeField]
		private string m_description = "New Character";

		[Tooltip("If set, changes the name of the cursor when moused over")]
		[SerializeField]
		private string m_cursor;

		[Header("Starting Room, Position, etc")]
		[SerializeField]
		private string m_room;

		[SerializeField]
		private Vector2 m_position = Vector2.zero;

		[SerializeField]
		private eFace m_faceDirection = eFace.Down;

		[Tooltip("Whether character is clickable/can be interacted with")]
		[SerializeField]
		private bool m_clickable = true;

		[Tooltip("Whether character sprites are visible")]
		[SerializeField]
		private bool m_visible = true;

		[SerializeField]
		private List<CollectedItem> m_inventory = new List<CollectedItem>();

		[Header("Movement Defaults")]
		[SerializeField]
		private Vector2 m_walkSpeed = new Vector2(50f, 50f);

		private Vector2 m_defaultWalkSpeed = -Vector2.one;

		[SerializeField]
		private bool m_moveable = true;

		[Tooltip("If true, this character will walk around other characters marked as solid (Using their Solid Size)")]
		[SerializeField]
		private bool m_solid;

		[Tooltip("Width & height of rectangle for other characters to pathfind around, centered on character pivot")]
		[SerializeField]
		private Vector2 m_solidSize = new Vector2(20f, 4f);

		[SerializeField]
		private bool m_turnBeforeWalking = true;

		[SerializeField]
		private bool m_turnBeforeFacing = true;

		[Tooltip("How fast character turns (Frames per second)")]
		[SerializeField]
		private float m_turnSpeedFPS = 12f;

		[SerializeField]
		private bool m_adjustSpeedWithScaling = true;

		[Header("Visuals Setup")]
		[SerializeField]
		private Color m_textColour = Color.white;

		[SerializeField]
		private string m_animIdle = "Idle";

		[SerializeField]
		private string m_animWalk = "Walk";

		[SerializeField]
		private string m_animTalk = "Talk";

		[SerializeField]
		private string m_animMouth = string.Empty;

		[SerializeField]
		private string m_animShadow = "";

		[SerializeField]
		private bool m_useRegionTinting = true;

		[SerializeField]
		private bool m_useRegionScaling = true;

		[SerializeField]
		[Tooltip("Dialog text offset from the top of the sprite, added to the global one set in PowerQuest settings")]
		private Vector2 m_textOffset = Vector2.zero;

		[SerializeField]
		[Tooltip("To use, talk anims should be frames ABCDEFX in that order from https://github.com/DanielSWolf/rhubarb-lip-sync. Rhubarb must be downloaded to Project/Rhubarb/Rhubarb.exe")]
		private bool m_LipSyncEnabled;

		[SerializeField]
		private bool m_antiGlide;

		[Header("Audio")]
		[Tooltip("Add Footstep event to animation to trigger the footstep sound")]
		[SerializeField]
		private string m_footstepSound = string.Empty;

		[Header("Other Settings")]
		[Tooltip("Whether clickable collider shape is taken from the sprite")]
		[SerializeField]
		private bool m_useSpriteAsHotspot;

		[SerializeField]
		private float m_baseline;

		[SerializeField]
		private Vector2 m_walkToPoint = Vector2.zero;

		[SerializeField]
		private Vector2 m_lookAtPoint = Vector2.zero;

		[ReadOnly]
		[SerializeField]
		private string m_scriptName = "New";

		[ReadOnly]
		[SerializeField]
		private string m_scriptClass = "CharacterNew";

		private QuestScript m_script;

		private GameObject m_prefab;

		private CharacterComponent m_instance;

		private string m_activeInventory;

		private List<string> m_inventoryAllTime = new List<string>();

		private eFace m_targetFaceDirection = eFace.Right;

		private eFace m_facingVerticalFallback = eFace.Right;

		private eFace m_faceAfterWalk = eFace.None;

		private QuestText m_dialogText;

		private AudioHandle m_dialogAudioSource;

		private string m_lastRoom;

		private Vector2 m_textPositionOverride = Vector2.zero;

		private int m_clickableColliderId;

		private IEnumerator m_coroutineSay;

		private int m_useCount;

		private int m_lookCount;

		private bool m_enabled = true;

		private string m_animPrefix;

		private string m_animOverride;

		private bool m_pauseAnimAtEnd;

		private float m_animationTime = -1f;

		private float m_loopStartTime = -1f;

		private float m_loopEndTime = -1f;

		private List<Vector2> m_waypoints = new List<Vector2>();

		private FaceCharacterData m_faceChar;

		private bool m_shadowOn = true;

		private NonSavedData m_nonSavedData = new NonSavedData();

		public Action<string, int> CallbackOnSay;

		public Action CallbackOnEndSay;

		private bool m_saveDirty = true;

		private bool m_startSayCalled;

		public eQuestClickableType ClickableType => eQuestClickableType.Character;

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

		public Character Data => this;

		public IQuestClickable IClickable => this;

		public IRoom Room
		{
			get
			{
				if (string.IsNullOrEmpty(m_room))
				{
					m_nonSavedData.m_roomCached = null;
				}
				else if (m_nonSavedData.m_roomCached == null || m_room != m_nonSavedData.m_roomCached.ScriptName)
				{
					m_nonSavedData.m_roomCached = Singleton<PowerQuest>.Get.GetRoom(m_room);
				}
				return m_nonSavedData.m_roomCached;
			}
			set
			{
				string room = m_room;
				m_room = value?.ScriptName ?? null;
				string scriptName = Singleton<PowerQuest>.Get.GetCurrentRoom().ScriptName;
				if (room == m_room && (!IsPlayer || m_room == scriptName))
				{
					return;
				}
				if (!Singleton<PowerQuest>.Get.GetRestoringGame())
				{
					m_lastRoom = room;
				}
				if (scriptName == room)
				{
					if (IsPlayer)
					{
						Singleton<PowerQuest>.Get.StartRoomTransition(value.Data);
					}
					else if (Instance != null)
					{
						Instance.gameObject.name = "deleted";
						UnityEngine.Object.Destroy(Instance.gameObject);
					}
				}
				else if (scriptName == m_room)
				{
					SpawnInstance();
				}
				else if (scriptName != m_room && IsPlayer && !Singleton<PowerQuest>.Get.GetRestoringGame())
				{
					Singleton<PowerQuest>.Get.StartRoomTransition(value.Data);
				}
			}
		}

		public IRoom LastRoom => Singleton<PowerQuest>.Get.GetRoom(m_lastRoom);

		public Vector2 Position
		{
			get
			{
				return m_position;
			}
			set
			{
				SetPosition(value);
			}
		}

		public Vector2 TargetPosition
		{
			get
			{
				if (m_instance != null)
				{
					return m_instance.GetTargetPosition();
				}
				return m_position;
			}
		}

		public List<Vector2> Waypoints => m_waypoints;

		public float Baseline
		{
			get
			{
				return m_baseline;
			}
			set
			{
				m_baseline = value;
			}
		}

		public Vector2 WalkSpeed
		{
			get
			{
				return m_walkSpeed;
			}
			set
			{
				if (m_defaultWalkSpeed.x < 0f)
				{
					m_defaultWalkSpeed = m_walkSpeed;
				}
				m_walkSpeed = value;
			}
		}

		public bool TurnBeforeWalking
		{
			get
			{
				return m_turnBeforeWalking;
			}
			set
			{
				m_turnBeforeWalking = value;
			}
		}

		public bool TurnBeforeFacing
		{
			get
			{
				return m_turnBeforeFacing;
			}
			set
			{
				m_turnBeforeFacing = value;
			}
		}

		public float TurnSpeedFPS
		{
			get
			{
				return m_turnSpeedFPS;
			}
			set
			{
				m_turnSpeedFPS = value;
			}
		}

		public bool AdjustSpeedWithScaling
		{
			get
			{
				return m_adjustSpeedWithScaling;
			}
			set
			{
				m_adjustSpeedWithScaling = value;
			}
		}

		public eFace Facing
		{
			get
			{
				return m_faceDirection;
			}
			set
			{
				m_faceAfterWalk = eFace.None;
				m_faceDirection = value;
				m_targetFaceDirection = m_faceDirection;
				if (m_faceDirection != eFace.Up && m_faceDirection != eFace.Down && m_faceDirection != eFace.None)
				{
					m_facingVerticalFallback = CharacterComponent.ToCardinal(m_faceDirection);
				}
				if (m_instance != null)
				{
					m_instance.UpdateFacingVisuals(value);
				}
			}
		}

		public bool Enabled
		{
			get
			{
				if (m_enabled)
				{
					if (IsPlayer)
					{
						return Singleton<PowerQuest>.Get.GetCurrentRoom().PlayerVisible;
					}
					return true;
				}
				return false;
			}
			set
			{
				if (m_enabled != value)
				{
					m_enabled = value;
					if (m_instance != null)
					{
						m_instance.UpdateEnabled();
					}
				}
			}
		}

		public bool Clickable
		{
			get
			{
				if (m_clickable)
				{
					return Enabled;
				}
				return false;
			}
			set
			{
				if (value && !m_enabled)
				{
					Debug.LogWarning("Character Clickable set when Character is not Enabled. Did you mean to call Show() or Enable() first?");
				}
				m_clickable = value;
			}
		}

		public bool Visible
		{
			get
			{
				if (m_visible)
				{
					return Enabled;
				}
				return false;
			}
			set
			{
				if (value && !m_enabled)
				{
					Debug.LogWarning("Character Visible set when Character is not Enabled. Did you mean to call Show() or Enable() first?");
				}
				bool flag = value != m_visible;
				m_visible = value;
				if (m_instance != null && flag)
				{
					m_instance.UpdateVisibility();
				}
			}
		}

		public bool VisibleInRoom
		{
			get
			{
				if (Visible && Room != null)
				{
					return Room.Current;
				}
				return false;
			}
		}

		public bool Solid
		{
			get
			{
				if (m_solid)
				{
					return Enabled;
				}
				return false;
			}
			set
			{
				if (m_solid != value)
				{
					m_solid = value;
					if (m_instance != null)
					{
						m_instance.UpdateSolid();
					}
				}
			}
		}

		public Vector2 SolidSize
		{
			get
			{
				return m_solidSize;
			}
			set
			{
				if ((m_solidSize - value).sqrMagnitude > float.Epsilon)
				{
					m_solidSize = value;
					if (m_instance != null)
					{
						m_instance.UpdateSolidSize();
					}
				}
			}
		}

		public bool UseSpriteAsHotspot
		{
			get
			{
				return m_useSpriteAsHotspot;
			}
			set
			{
				if (value != m_useSpriteAsHotspot)
				{
					m_useSpriteAsHotspot = value;
					if (m_instance != null)
					{
						m_instance.UpdateUseSpriteAsHotspot();
					}
				}
			}
		}

		public bool Moveable
		{
			get
			{
				if (m_moveable)
				{
					return Enabled;
				}
				return false;
			}
			set
			{
				m_moveable = value;
			}
		}

		public bool Walking
		{
			get
			{
				if (!(m_instance == null))
				{
					return m_instance.Walking;
				}
				return false;
			}
		}

		public bool Talking
		{
			get
			{
				if (!(m_dialogText != null) || !m_dialogText.gameObject.activeSelf)
				{
					if (m_dialogAudioSource != null)
					{
						return m_dialogAudioSource.isPlaying;
					}
					return false;
				}
				return true;
			}
		}

		public bool Animating
		{
			get
			{
				if (!(m_instance == null))
				{
					return m_instance.Animating;
				}
				return false;
			}
		}

		public bool IsPlayer => Singleton<PowerQuest>.Get.GetPlayer() == this;

		public Color TextColour
		{
			get
			{
				return m_textColour;
			}
			set
			{
				m_textColour = value;
				if (m_dialogText != null)
				{
					m_dialogText.GetComponent<TextMesh>().color = m_textColour;
				}
			}
		}

		public string AnimIdle
		{
			get
			{
				return m_animIdle;
			}
			set
			{
				bool flag = m_animIdle != value;
				m_animIdle = value;
				if (m_instance != null && flag)
				{
					m_instance.OnAnimationChanged(eState.Idle);
				}
			}
		}

		public string AnimWalk
		{
			get
			{
				return m_animWalk;
			}
			set
			{
				bool flag = m_animWalk != value;
				m_animWalk = value;
				if (m_instance != null && flag)
				{
					m_instance.OnAnimationChanged(eState.Walk);
				}
			}
		}

		public string AnimTalk
		{
			get
			{
				return m_animTalk;
			}
			set
			{
				bool flag = m_animTalk != value;
				m_animTalk = value;
				if (m_instance != null && flag)
				{
					m_instance.OnAnimationChanged(eState.Talk);
				}
			}
		}

		public string AnimMouth
		{
			get
			{
				return m_animMouth;
			}
			set
			{
				bool flag = m_animMouth != value;
				m_animMouth = value;
				if (m_instance != null && flag)
				{
					m_instance.UpdateMouthAnim();
				}
			}
		}

		public string AnimPrefix
		{
			get
			{
				return m_animPrefix;
			}
			set
			{
				bool flag = m_animPrefix != value;
				m_animPrefix = value;
				if (m_instance != null && flag)
				{
					m_instance.OnAnimationChanged();
				}
			}
		}

		public bool LipSyncEnabled
		{
			get
			{
				return m_LipSyncEnabled;
			}
			set
			{
				m_LipSyncEnabled = value;
			}
		}

		public bool AntiGlide
		{
			get
			{
				return m_antiGlide;
			}
			set
			{
				m_antiGlide = value;
			}
		}

		public string FootstepSound
		{
			get
			{
				return m_footstepSound;
			}
			set
			{
				m_footstepSound = value;
			}
		}

		public eState State
		{
			get
			{
				if (!(m_instance != null))
				{
					return eState.None;
				}
				return m_instance.GetState();
			}
		}

		public IInventory ActiveInventory
		{
			get
			{
				return Singleton<PowerQuest>.Get.GetInventory(m_activeInventory);
			}
			set
			{
				m_activeInventory = value?.ScriptName;
			}
		}

		public bool HasActiveInventory => !string.IsNullOrEmpty(m_activeInventory);

		public string ActiveInventoryName
		{
			get
			{
				return m_activeInventory;
			}
			set
			{
				m_activeInventory = value;
			}
		}

		public bool HasActiveInventoryName => string.IsNullOrEmpty(m_activeInventory);

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

		public bool UseRegionTinting
		{
			get
			{
				return m_useRegionTinting;
			}
			set
			{
				m_useRegionTinting = value;
			}
		}

		public bool UseRegionScaling
		{
			get
			{
				return m_useRegionScaling;
			}
			set
			{
				m_useRegionScaling = value;
			}
		}

		public bool FirstUse => UseCount == 0;

		public bool FirstLook => LookCount == 0;

		public int UseCount => m_useCount - (Singleton<PowerQuest>.Get.GetInteractionInProgress(this, eQuestVerb.Use) ? 1 : 0);

		public int LookCount => m_lookCount - (Singleton<PowerQuest>.Get.GetInteractionInProgress(this, eQuestVerb.Look) ? 1 : 0);

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

		public Vector2 TextPositionOffset
		{
			get
			{
				return m_textOffset;
			}
			set
			{
				m_textOffset = value;
			}
		}

		public Vector2 TextPositionOverride
		{
			get
			{
				return m_textPositionOverride;
			}
			set
			{
				m_textPositionOverride = value;
			}
		}

		public bool PauseAnimAtEnd
		{
			get
			{
				return m_pauseAnimAtEnd;
			}
			set
			{
				m_pauseAnimAtEnd = value;
			}
		}

		public float AnimationTime
		{
			get
			{
				return m_animationTime;
			}
			set
			{
				m_animationTime = value;
			}
		}

		public float LoopStartTime
		{
			get
			{
				return m_loopStartTime;
			}
			set
			{
				m_loopStartTime = value;
			}
		}

		public float LoopEndTime
		{
			get
			{
				return m_loopEndTime;
			}
			set
			{
				m_loopEndTime = value;
			}
		}

		public bool SaveDirty
		{
			get
			{
				return m_saveDirty;
			}
			set
			{
				m_saveDirty = value;
			}
		}

		public string Animation
		{
			get
			{
				return m_animOverride;
			}
			set
			{
				if (value == null)
				{
					StopAnimation();
				}
				else
				{
					PlayAnimationBG(value, pauseAtEnd: true);
				}
			}
		}

		public bool Idle
		{
			get
			{
				if (!Animating && !Walking && !Talking && (m_instance == null || !m_instance.GetPlayingTransition()))
				{
					return m_targetFaceDirection == m_faceDirection;
				}
				return false;
			}
		}

		public int ClickableColliderId
		{
			get
			{
				return m_clickableColliderId;
			}
			set
			{
				m_clickableColliderId = value;
				if (m_instance != null)
				{
					m_instance.OnClickableColliderIdChanged();
				}
			}
		}

		public string AnimShadow
		{
			get
			{
				return m_animShadow;
			}
			set
			{
				if (!(m_animShadow == value))
				{
					m_animShadow = value;
					if ((bool)m_instance)
					{
						m_instance.UpdateShadow();
					}
				}
			}
		}

		public bool ShadowEnabled
		{
			get
			{
				return m_shadowOn;
			}
			set
			{
				if (m_shadowOn != value)
				{
					m_shadowOn = value;
					if (m_instance != null)
					{
						m_instance.UpdateShadow();
					}
				}
			}
		}

		public void ChangeRoomBG(IRoom room)
		{
			Room = room;
		}

		public Coroutine ChangeRoom(IRoom room)
		{
			return Singleton<PowerQuest>.Get.StartCoroutine(CoroutineChangeRoom(room));
		}

		private IEnumerator CoroutineChangeRoom(IRoom room)
		{
			if (Singleton<PowerQuest>.Get.GetPlayer() == this)
			{
				yield return Singleton<PowerQuest>.Get.ChangeRoom(room);
			}
			else
			{
				Room = room;
			}
		}

		public void DebugSetLastRoom(IRoom room)
		{
			if (room == null)
			{
				m_lastRoom = null;
			}
			else
			{
				m_lastRoom = room.ScriptName;
			}
		}

		public void SetBaselineInFrontOf(IQuestClickableInterface clickable)
		{
			Baseline = clickable.IClickable.Baseline - 1f - Position.y;
		}

		public void ResetWalkSpeed()
		{
			if (m_defaultWalkSpeed.x > 0f)
			{
				WalkSpeed = m_defaultWalkSpeed;
			}
		}

		public eFace GetFaceAfterWalk()
		{
			return m_faceAfterWalk;
		}

		public void Show(float posX, float posy, eFace facing = eFace.None)
		{
			Show(new Vector2(posX, posy), facing);
		}

		public void Show(eFace facing)
		{
			Show(Vector2.zero, facing);
		}

		public void Show(IQuestClickableInterface atClickableWalkToPos, eFace face = eFace.None)
		{
			Show(atClickableWalkToPos.IClickable.Position + atClickableWalkToPos.IClickable.WalkToPoint, face);
		}

		public void Show(Vector2 pos = default(Vector2), eFace facing = eFace.None)
		{
			Enabled = true;
			Visible = true;
			if (pos != Vector2.zero)
			{
				Position = pos;
			}
			if (facing != eFace.None)
			{
				Facing = facing;
			}
			Room = Singleton<PowerQuest>.Get.GetCurrentRoom();
		}

		public void Show(bool clickable)
		{
			Enable(clickable);
		}

		public void Enable(bool clickable)
		{
			Enabled = true;
			Visible = true;
			Clickable = clickable;
			Room = Singleton<PowerQuest>.Get.GetCurrentRoom();
		}

		public void Hide()
		{
			Disable();
		}

		public void Enable()
		{
			Enabled = true;
		}

		public void Disable()
		{
			Enabled = false;
		}

		public void SetTextPosition(Vector2 worldPosition)
		{
			TextPositionOverride = worldPosition;
		}

		public void SetTextPosition(float worldPosX, float worldPosY)
		{
			TextPositionOverride = new Vector2(worldPosX, worldPosY);
		}

		public void LockTextPosition()
		{
			if (m_instance != null)
			{
				ResetTextPosition();
				TextPositionOverride = m_instance.GetTextPosition();
			}
			else
			{
				TextPositionOverride = Position;
			}
		}

		public void ResetTextPosition()
		{
			TextPositionOverride = Vector2.zero;
		}

		public void StartFacingCharacter(ICharacter character, float minWaitTime = 0.2f, float maxWaitTime = 0.4f)
		{
			m_faceChar = new FaceCharacterData
			{
				m_character = character.ScriptName,
				m_minTime = minWaitTime,
				m_maxTime = maxWaitTime,
				m_timer = UnityEngine.Random.Range(minWaitTime, maxWaitTime)
			};
		}

		public void StopFacingCharacter()
		{
			m_faceChar = null;
		}

		public QuestScript GetScript()
		{
			return m_script;
		}

		public IQuestScriptable GetScriptable()
		{
			return this;
		}

		public T GetScript<T>() where T : CharacterScript<T>
		{
			if (m_script == null)
			{
				return null;
			}
			return m_script as T;
		}

		public string GetScriptName()
		{
			return m_scriptName;
		}

		public string GetScriptClassName()
		{
			return m_scriptClass;
		}

		public void HotLoadScript(Assembly assembly)
		{
			QuestUtils.HotSwapScript(ref m_script, m_scriptClass, assembly);
		}

		public GameObject GetPrefab()
		{
			return m_prefab;
		}

		public GameObject SpawnInstance()
		{
			GameObject gameObject = GameObject.Find(GetPrefab().name);
			if (gameObject == null)
			{
				gameObject = UnityEngine.Object.Instantiate(GetPrefab());
			}
			SetInstance(gameObject.GetComponent<CharacterComponent>());
			SetPosition(GetPosition());
			Facing = GetFaceDirection();
			return gameObject;
		}

		public GameObject GetInstance()
		{
			if (!(m_instance != null))
			{
				return null;
			}
			return m_instance.gameObject;
		}

		public void SetInstance(CharacterComponent instance)
		{
			m_instance = instance;
			m_instance.SetData(this);
			m_instance.name = m_prefab.name;
		}

		public void SetPosition(float x, float y, eFace face = eFace.None)
		{
			SetPosition(new Vector2(x, y), face);
		}

		public void SetPosition(IQuestClickableInterface clickable, eFace face = eFace.None)
		{
			SetPosition(clickable.IClickable.Position + clickable.IClickable.WalkToPoint, face);
		}

		public void SetPosition(Vector2 position, eFace face = eFace.None)
		{
			m_position = position;
			if (m_instance != null)
			{
				m_instance.transform.position = Utils.SnapRound(m_position, Singleton<PowerQuest>.Get.SnapAmount);
			}
			if (face != eFace.None)
			{
				Facing = face;
			}
		}

		public Vector2 GetPosition()
		{
			return m_position;
		}

		public eFace GetFacingVerticalFallback()
		{
			return m_facingVerticalFallback;
		}

		public void SetFacingVerticalFallback(eFace value)
		{
			m_facingVerticalFallback = value;
		}

		public eFace GetTargetFaceDirection()
		{
			return m_targetFaceDirection;
		}

		public eFace GetFaceDirection()
		{
			return m_faceDirection;
		}

		public void SetFaceDirection(eFace direction)
		{
			m_faceDirection = direction;
		}

		public List<CollectedItem> GetInventory()
		{
			return m_inventory;
		}

		public float GetInventoryItemCount()
		{
			return m_inventory.Count;
		}

		public float GetInventoryQuantity(string itemName)
		{
			float num = 0f;
			foreach (CollectedItem item in m_inventory)
			{
				if (item.m_name == itemName)
				{
					num += item.m_quantity;
				}
			}
			return num;
		}

		public bool HasInventory(string itemName)
		{
			return m_inventory.Exists((CollectedItem inv) => inv.m_name == itemName);
		}

		public bool GetEverHadInventory(string itemName)
		{
			return m_inventoryAllTime.Contains(itemName);
		}

		public void AddInventory(string itemName, float quantity = 1f)
		{
			Inventory inventory = Singleton<PowerQuest>.Get.GetInventory(itemName);
			if (inventory == null)
			{
				return;
			}
			if (inventory.Stack)
			{
				CollectedItem collectedItem = m_inventory.Find((CollectedItem item) => string.Equals(itemName, item.m_name, StringComparison.OrdinalIgnoreCase));
				if (collectedItem == null)
				{
					m_inventory.Add(new CollectedItem
					{
						m_name = itemName,
						m_quantity = quantity
					});
					if (!m_inventoryAllTime.Contains(itemName))
					{
						m_inventoryAllTime.Add(itemName);
					}
				}
				else
				{
					collectedItem.m_quantity += quantity;
				}
			}
			else
			{
				for (int num = 0; (float)num < quantity; num++)
				{
					m_inventory.Add(new CollectedItem
					{
						m_name = itemName,
						m_quantity = quantity
					});
				}
				if (!m_inventoryAllTime.Contains(itemName))
				{
					m_inventoryAllTime.Add(itemName);
				}
			}
			inventory.OnCollected();
			if (Singleton<PowerQuest>.Get.CallbackOnInventoryCollected != null)
			{
				Singleton<PowerQuest>.Get.CallbackOnInventoryCollected(Data, inventory);
			}
		}

		public void RemoveInventory(string itemName, float quantity = 1f)
		{
			Inventory inventory = Singleton<PowerQuest>.Get.GetInventory(itemName);
			if (inventory == null)
			{
				return;
			}
			if (inventory.Stack)
			{
				CollectedItem collectedItem = m_inventory.Find((CollectedItem item) => string.Equals(itemName, item.m_name, StringComparison.OrdinalIgnoreCase));
				if (collectedItem != null)
				{
					collectedItem.m_quantity -= quantity;
					if (collectedItem.m_quantity <= 0f)
					{
						m_inventory.Remove(collectedItem);
					}
				}
			}
			else
			{
				CollectedItem collectedItem2 = m_inventory.Find((CollectedItem item) => string.Equals(itemName, item.m_name, StringComparison.OrdinalIgnoreCase));
				for (int num = 0; (float)num < quantity; num++)
				{
					if (collectedItem2 == null)
					{
						break;
					}
					m_inventory.Remove(collectedItem2);
					collectedItem2 = m_inventory.Find((CollectedItem item) => string.Equals(itemName, item.m_name, StringComparison.OrdinalIgnoreCase));
				}
			}
			if (itemName == m_activeInventory && !HasInventory(itemName))
			{
				ActiveInventory = null;
			}
		}

		public void ClearInventory()
		{
			m_inventory.Clear();
			ActiveInventory = null;
		}

		public float GetInventoryQuantity(IInventory item)
		{
			return GetInventoryQuantity(item?.ScriptName);
		}

		public bool HasInventory(IInventory item)
		{
			return HasInventory(item?.ScriptName);
		}

		public bool GetEverHadInventory(IInventory item)
		{
			return GetEverHadInventory(item?.ScriptName);
		}

		public void AddInventory(IInventory item, float quantity = 1f)
		{
			AddInventory(item?.ScriptName, quantity);
		}

		public void RemoveInventory(IInventory item, float quantity = 1f)
		{
			RemoveInventory(item?.ScriptName, quantity);
		}

		public void ReplaceInventory(IInventory oldItem, IInventory newItem)
		{
			AddInventory(newItem);
			int num = GetInventory().FindIndex((CollectedItem item) => item.m_name == oldItem.ScriptName);
			if (num >= 0)
			{
				GetInventory().Swap(num, GetInventory().Count - 1);
				RemoveInventory(oldItem);
			}
		}

		public AudioSource GetDialogAudioSource()
		{
			return m_dialogAudioSource;
		}

		public void EditorInitialise(string name)
		{
			m_description = name;
			m_scriptName = name;
			m_scriptClass = PowerQuest.STR_CHARACTER + name;
		}

		public void EditorRename(string name)
		{
			m_scriptName = name;
			m_scriptClass = PowerQuest.STR_CHARACTER + name;
		}

		public string EditorGetRoom()
		{
			return m_room;
		}

		public void EditorSetRoom(string roomName)
		{
			m_room = roomName;
		}

		public bool EditorGetSolid()
		{
			return m_solid;
		}

		public void OnPostRestore(int version, GameObject prefab)
		{
			m_prefab = prefab;
			if (m_script == null)
			{
				m_script = QuestUtils.ConstructByName<QuestScript>(m_scriptClass);
			}
			SaveDirty = false;
		}

		public void Initialise(GameObject prefab)
		{
			m_prefab = prefab;
			m_script = QuestUtils.ConstructByName<QuestScript>(m_scriptClass);
			m_inventory = QuestUtils.CopyListFields(m_inventory);
			m_inventoryAllTime.Clear();
			foreach (CollectedItem item in m_inventory)
			{
				if (!m_inventoryAllTime.Contains(item.m_name))
				{
					m_inventoryAllTime.Add(item.m_name);
				}
			}
			m_nonSavedData = new NonSavedData();
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

		public void WalkToBG(float x, float y, bool anywhere = false, eFace thenFace = eFace.None)
		{
			WalkToBG(new Vector2(x, y), anywhere, thenFace);
		}

		public void WalkToBG(Vector2 pos, bool anywhere = false, eFace thenFace = eFace.None)
		{
			m_faceAfterWalk = thenFace;
			m_waypoints.Clear();
			Singleton<PowerQuest>.Get.DisableCancel();
			if (!Moveable)
			{
				return;
			}
			if (m_instance != null && !Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				m_instance.WalkTo(pos, anywhere, playWalkAnim: true);
				if (!Walking && m_faceAfterWalk != eFace.None)
				{
					FaceBG(thenFace);
				}
				return;
			}
			SetPosition(pos);
			if (m_faceAfterWalk != eFace.None)
			{
				Facing = m_faceAfterWalk;
			}
			StopWalking();
		}

		public void WalkToBG(IQuestClickableInterface clickable, bool anywhere = false, eFace thenFace = eFace.None)
		{
			m_faceAfterWalk = thenFace;
			m_waypoints.Clear();
			if (clickable != null)
			{
				if (clickable.IClickable.Instance != null)
				{
					WalkToBG((Vector2)clickable.IClickable.Instance.transform.position + clickable.IClickable.WalkToPoint, anywhere, thenFace);
				}
				else
				{
					WalkToBG(clickable.IClickable.WalkToPoint, anywhere, thenFace);
				}
			}
		}

		public Coroutine WalkTo(float x, float y, bool anywhere = false)
		{
			return Singleton<PowerQuest>.Get.StartQuestCoroutine(CoroutineWalkTo(new Vector2(x, y), anywhere));
		}

		public Coroutine WalkTo(Vector2 pos, bool anywhere = false)
		{
			return Singleton<PowerQuest>.Get.StartQuestCoroutine(CoroutineWalkTo(pos, anywhere));
		}

		public Coroutine WalkTo(IQuestClickableInterface clickable, bool anywhere = false)
		{
			m_faceAfterWalk = eFace.None;
			m_waypoints.Clear();
			if (clickable != null)
			{
				if (clickable.IClickable.Instance != null)
				{
					return Singleton<PowerQuest>.Get.StartQuestCoroutine(CoroutineWalkTo((Vector2)clickable.IClickable.Instance.transform.position + clickable.IClickable.WalkToPoint, anywhere));
				}
				return Singleton<PowerQuest>.Get.StartQuestCoroutine(CoroutineWalkTo(clickable.IClickable.WalkToPoint, anywhere));
			}
			return null;
		}

		public Coroutine WalkToClicked(bool anywhere = false)
		{
			return Singleton<PowerQuest>.Get.StartCoroutine(CoroutineWalkTo(Singleton<PowerQuest>.Get.GetLastWalkTo(), anywhere));
		}

		public Coroutine MoveTo(float x, float y, bool anywhere = false)
		{
			return Singleton<PowerQuest>.Get.StartQuestCoroutine(CoroutineWalkTo(new Vector2(x, y), anywhere, playWalkAnim: false));
		}

		public Coroutine MoveTo(Vector2 pos, bool anywhere = false)
		{
			return Singleton<PowerQuest>.Get.StartQuestCoroutine(CoroutineWalkTo(pos, anywhere, playWalkAnim: false));
		}

		public Coroutine MoveTo(IQuestClickableInterface clickable, bool anywhere = false)
		{
			m_faceAfterWalk = eFace.None;
			m_waypoints.Clear();
			if (clickable != null)
			{
				if (clickable.IClickable.Instance != null)
				{
					return Singleton<PowerQuest>.Get.StartQuestCoroutine(CoroutineWalkTo((Vector2)clickable.IClickable.Instance.transform.position + clickable.IClickable.WalkToPoint, anywhere, playWalkAnim: false));
				}
				return Singleton<PowerQuest>.Get.StartQuestCoroutine(CoroutineWalkTo(clickable.IClickable.WalkToPoint, anywhere, playWalkAnim: false));
			}
			return null;
		}

		public void MoveToBG(float x, float y, bool anywhere = false)
		{
			MoveToBG(new Vector2(x, y), anywhere);
		}

		public void MoveToBG(Vector2 pos, bool anywhere = false)
		{
			m_faceAfterWalk = eFace.None;
			m_waypoints.Clear();
			Singleton<PowerQuest>.Get.DisableCancel();
			if (Moveable)
			{
				if (m_instance != null && !Singleton<PowerQuest>.Get.GetSkippingCutscene())
				{
					m_instance.WalkTo(pos, anywhere, playWalkAnim: false);
				}
				else
				{
					SetPosition(pos);
				}
			}
		}

		public void MoveToBG(IQuestClickableInterface clickable, bool anywhere = false)
		{
			m_faceAfterWalk = eFace.None;
			m_waypoints.Clear();
			if (clickable != null)
			{
				if (clickable.IClickable.Instance != null)
				{
					MoveToBG((Vector2)clickable.IClickable.Instance.transform.position + clickable.IClickable.WalkToPoint, anywhere);
				}
				else
				{
					MoveToBG(clickable.IClickable.WalkToPoint, anywhere);
				}
			}
		}

		public void StopWalking()
		{
			m_faceAfterWalk = eFace.None;
			m_waypoints.Clear();
			if (m_instance != null)
			{
				m_instance.StopWalk();
			}
		}

		public void AddWaypoint(float x, float y, eFace thenFace = eFace.None)
		{
			AddWaypoint(new Vector2(x, y), thenFace);
		}

		public void AddWaypoint(Vector2 pos, eFace thenFace = eFace.None)
		{
			m_faceAfterWalk = thenFace;
			Singleton<PowerQuest>.Get.DisableCancel();
			if (!Moveable)
			{
				return;
			}
			if (m_instance != null && !Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				m_waypoints.Add(pos);
				if (!m_instance.Walking)
				{
					m_instance.WalkTo(pos, anywhere: true, playWalkAnim: true);
				}
			}
			else
			{
				SetPosition(pos);
			}
		}

		public void FaceDownBG(bool instant = false)
		{
			Face(eFace.Down, instant);
		}

		public void FaceUpBG(bool instant = false)
		{
			Face(eFace.Up, instant);
		}

		public void FaceLeftBG(bool instant = false)
		{
			Face(eFace.Left, instant);
		}

		public void FaceRightBG(bool instant = false)
		{
			Face(eFace.Right, instant);
		}

		public void FaceUpRightBG(bool instant = false)
		{
			Face(eFace.UpRight, instant);
		}

		public void FaceUpLeftBG(bool instant = false)
		{
			Face(eFace.UpLeft, instant);
		}

		public void FaceDownRightBG(bool instant = false)
		{
			Face(eFace.DownRight, instant);
		}

		public void FaceDownLeftBG(bool instant = false)
		{
			Face(eFace.DownLeft, instant);
		}

		public void FaceBG(eFace direction, bool instant = false)
		{
			Face(direction, instant);
		}

		public void FaceBG(IQuestClickableInterface clickable, bool instant = false)
		{
			FaceBG(clickable.IClickable, instant);
		}

		public void FaceBG(IQuestClickable clickable, bool instant = false)
		{
			Face(clickable, instant);
		}

		public void FaceBG(float x, float y, bool instant = false)
		{
			Face(x, y, instant);
		}

		public void FaceBG(Vector2 location, bool instant = false)
		{
			Face(location, instant);
		}

		public void FaceClickedBG(bool instant = false)
		{
			FaceClicked(instant);
		}

		public void FaceAwayBG(bool instant = false)
		{
			FaceAway(instant);
		}

		public void FaceDirectionBG(Vector2 directionV2, bool instant = false)
		{
			FaceDirection(directionV2, instant);
		}

		public Coroutine Face(eFace direction, bool instant = false)
		{
			eFace fallback = eFace.None;
			if (direction != eFace.Up && direction != eFace.Down)
			{
				fallback = CharacterComponent.ToCardinal(direction);
			}
			return FaceInternal(direction, instant, fallback);
		}

		private Coroutine FaceInternal(eFace direction, bool instant, eFace fallback)
		{
			if (direction == eFace.None)
			{
				return null;
			}
			if (Singleton<PowerQuest>.Get.GetRoomLoading() || Singleton<PowerQuest>.Get.GetSkippingCutscene() || m_instance == null || !Visible)
			{
				instant = true;
			}
			if (Walking && !m_turnBeforeWalking)
			{
				instant = true;
			}
			if (!Walking && !m_turnBeforeFacing)
			{
				instant = true;
			}
			if (instant)
			{
				_ = m_facingVerticalFallback;
				m_targetFaceDirection = direction;
				if (fallback != eFace.None)
				{
					m_facingVerticalFallback = fallback;
				}
				m_faceDirection = direction;
				if (m_instance != null)
				{
					m_instance.UpdateFacingVisuals(direction);
				}
				return null;
			}
			return Singleton<PowerQuest>.Get.StartQuestCoroutine(CoroutineFace(direction, fallback));
		}

		public Coroutine FaceDown(bool instant = false)
		{
			return Face(eFace.Down, instant);
		}

		public Coroutine FaceUp(bool instant = false)
		{
			return Face(eFace.Up, instant);
		}

		public Coroutine FaceLeft(bool instant = false)
		{
			return Face(eFace.Left, instant);
		}

		public Coroutine FaceRight(bool instant = false)
		{
			return Face(eFace.Right, instant);
		}

		public Coroutine FaceUpRight(bool instant = false)
		{
			return Face(eFace.UpRight, instant);
		}

		public Coroutine FaceUpLeft(bool instant = false)
		{
			return Face(eFace.UpLeft, instant);
		}

		public Coroutine FaceDownRight(bool instant = false)
		{
			return Face(eFace.DownRight, instant);
		}

		public Coroutine FaceDownLeft(bool instant = false)
		{
			return Face(eFace.DownLeft, instant);
		}

		public Coroutine Face(IQuestClickableInterface clickable, bool instant = false)
		{
			return Face(clickable.IClickable, instant);
		}

		public Coroutine Face(IQuestClickable clickable, bool instant = false)
		{
			if (clickable == IClickable)
			{
				Debug.LogWarning("Character " + clickable.ScriptName + " tried to Face() themselves");
				return null;
			}
			if (clickable != null)
			{
				return Face(clickable.Position + clickable.LookAtPoint, instant);
			}
			return null;
		}

		public Coroutine Face(float x, float y, bool instant = false)
		{
			return Face(new Vector2(x, y), instant);
		}

		public Coroutine Face(Vector2 location, bool instant = false)
		{
			return FaceDirection((location - m_position).normalized, instant);
		}

		public Coroutine FaceClicked(bool instant = false)
		{
			return Face(Singleton<PowerQuest>.Get.GetLastLookAt(), instant);
		}

		public Coroutine FaceAway(bool instant = false)
		{
			return FaceDirection(-FACE_DIRECTIONS[(int)GetTargetFaceDirection()], instant);
		}

		public Coroutine FaceDirection(Vector2 directionV2, bool instant = false)
		{
			if (directionV2.sqrMagnitude <= 0f)
			{
				Debug.LogWarning("FaceDirection called with zero direction passed. Ignoring");
				return null;
			}
			int num = 8;
			float num2 = Mathf.Cos((float)Math.PI / 180f * Singleton<PowerQuest>.Get.FacingSegmentAngle * 0.5f);
			directionV2.Normalize();
			for (int i = 0; i < num; i++)
			{
				if (Vector2.Dot(FACE_DIRECTIONS[i], directionV2) >= num2)
				{
					eFace direction = (eFace)i;
					eFace fallback = eFace.None;
					if (directionV2.x > 0f)
					{
						fallback = eFace.Right;
					}
					else if (directionV2.x < 0f)
					{
						fallback = eFace.Left;
					}
					return FaceInternal(direction, instant, fallback);
				}
			}
			return null;
		}

		public Coroutine Say(string dialog, int id = -1)
		{
			PowerQuest get = Singleton<PowerQuest>.Get;
			if (m_coroutineSay != null)
			{
				get.StopCoroutine(m_coroutineSay);
				EndSay();
				get.OnSay();
			}
			if (CallbackOnSay != null)
			{
				CallbackOnSay(dialog, id);
			}
			if (get.DialogInterruptRequested)
			{
				get.ResetInterruptNextLine();
				return get.StartCoroutine(CoroutineSayEndEarly(dialog, get.DialogInterruptDuration, id));
			}
			m_coroutineSay = CoroutineSay(dialog, id);
			return get.StartCoroutine(m_coroutineSay);
		}

		public Coroutine SayBG(string dialog, int id = -1)
		{
			if (m_coroutineSay != null)
			{
				Singleton<PowerQuest>.Get.StopCoroutine(m_coroutineSay);
				EndSay();
			}
			m_coroutineSay = CoroutineSayBG(dialog, id);
			return Singleton<PowerQuest>.Get.StartCoroutine(m_coroutineSay);
		}

		public void CancelSay()
		{
			if (m_coroutineSay != null)
			{
				Singleton<PowerQuest>.Get.StopCoroutine(m_coroutineSay);
				EndSay();
			}
		}

		public Coroutine PlayAnimation(string animName)
		{
			ResetAnimationData();
			if (m_instance != null)
			{
				return Singleton<PowerQuest>.Get.StartCoroutine(CoroutinePlayAnimation(animName));
			}
			return null;
		}

		public Coroutine WaitForAnimation()
		{
			if (m_instance != null)
			{
				return Singleton<PowerQuest>.Get.StartCoroutine(CoroutineWaitForAnimation());
			}
			return null;
		}

		public void PlayAnimationBG(string animName, bool pauseAtEnd = false)
		{
			ResetAnimationData();
			m_animOverride = animName;
			m_pauseAnimAtEnd = pauseAtEnd;
			if ((!Singleton<PowerQuest>.Get.GetSkippingCutscene() || pauseAtEnd) && m_instance != null)
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

		public void StopAnimation()
		{
			ResetAnimationData();
			if (m_instance != null)
			{
				m_instance.StopAnimation();
			}
		}

		public void SkipTransition()
		{
			if (m_instance != null)
			{
				m_instance.SkipTransition();
			}
		}

		public Coroutine WaitForTransition(bool skippable = false)
		{
			return Singleton<PowerQuest>.Get.StartCoroutine(CoroutineWaitForTransition(skippable));
		}

		public IEnumerator CoroutineWaitForTransition(bool skippable)
		{
			if (m_instance != null)
			{
				yield return Singleton<PowerQuest>.Get.WaitWhile(() => m_instance != null && m_instance.GetPlayingTransition(), skippable);
				if (skippable)
				{
					SkipTransition();
					yield return null;
				}
			}
		}

		public Coroutine WaitForIdle(bool skippable = false)
		{
			return Singleton<PowerQuest>.Get.StartCoroutine(CoroutineWaitForIdle(skippable));
		}

		public IEnumerator CoroutineWaitForIdle(bool skippable)
		{
			yield return null;
			bool skipped = false;
			bool first = true;
			while (!Idle && !Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				if (skippable)
				{
					skipped = Singleton<PowerQuest>.Get.HandleSkipDialogKeyPressed();
				}
				if (skipped && !first)
				{
					break;
				}
				first = false;
				yield return null;
			}
			if (skipped || Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				if (m_instance != null)
				{
					m_instance.SkipWalk();
				}
				CancelSay();
				StopAnimation();
				Facing = m_targetFaceDirection;
				SkipTransition();
				yield return null;
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

		public void UpdateFacingCharacter()
		{
			if (m_faceChar != null && Singleton<PowerQuest>.Get.GetCharacter(m_faceChar.m_character) != null && !Walking && m_targetFaceDirection == m_faceDirection && Utils.GetTimeIncrementPassed(m_faceChar.m_minTime, m_faceChar.m_maxTime, ref m_faceChar.m_timer))
			{
				FaceBG((IQuestClickable)Singleton<PowerQuest>.Get.GetCharacter(m_faceChar.m_character), instant: false);
			}
		}

		public void ShadowOn()
		{
			ShadowEnabled = true;
		}

		public void ShadowOff()
		{
			ShadowEnabled = false;
		}

		private IEnumerator CoroutineWalkTo(Vector2 position, bool anywhere, bool playWalkAnim = true)
		{
			m_faceAfterWalk = eFace.None;
			m_waypoints.Clear();
			if (!Moveable)
			{
				yield break;
			}
			if (m_instance != null && !Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				m_instance.WalkTo(position, anywhere, playWalkAnim);
			}
			else
			{
				SetPosition(position);
			}
			if (Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				m_instance.SkipWalk();
				yield break;
			}
			bool skip = false;
			while (m_instance != null && !skip && Moveable && m_instance.Walking)
			{
				if (Singleton<PowerQuest>.Get.GetSkippingCutscene())
				{
					skip = true;
					m_instance.SkipWalk();
				}
				else
				{
					yield return new WaitForEndOfFrame();
				}
			}
			Singleton<PowerQuest>.Get.OnPlayerWalkComplete();
		}

		private IEnumerator CoroutineSay(string text, int id = -1)
		{
			if (Singleton<PowerQuest>.Get.GetStopWalkingToTalk())
			{
				StopWalking();
			}
			if (!Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				StartSay(text, id);
				yield return Singleton<PowerQuest>.Get.WaitForDialog(Singleton<PowerQuest>.Get.GetTextDisplayTime(text), m_dialogAudioSource, Singleton<PowerQuest>.Get.GetShouldSayTextAutoAdvance(), skippable: true, m_dialogText);
				EndSay();
			}
		}

		private IEnumerator CoroutineSayBG(string text, int id = -1)
		{
			StartSay(text, id, background: true);
			yield return Singleton<PowerQuest>.Get.WaitForDialog(Singleton<PowerQuest>.Get.GetTextDisplayTime(text), m_dialogAudioSource, autoAdvance: true, skippable: false, m_dialogText);
			EndSay();
		}

		private IEnumerator CoroutineSayEndEarly(string text, float endTime, int id = -1)
		{
			if (Singleton<PowerQuest>.Get.GetStopWalkingToTalk())
			{
				StopWalking();
			}
			if (Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				yield break;
			}
			SayBG(text, id);
			IEnumerator sayBGCoroutine = m_coroutineSay;
			float time = Singleton<PowerQuest>.Get.GetTextDisplayTime(text);
			bool first = true;
			bool skipped = false;
			bool stillPlaying = true;
			while (stillPlaying)
			{
				stillPlaying = Singleton<PowerQuest>.Get.ShouldContinueDialog(first, ref time, skippable: true, Singleton<PowerQuest>.Get.GetShouldSayTextAutoAdvance(), m_dialogAudioSource, m_dialogText, endTime);
				if (stillPlaying)
				{
					first = false;
					yield return new WaitForEndOfFrame();
					if (!SystemTime.Paused)
					{
						time -= Time.deltaTime;
					}
				}
				else
				{
					skipped = Singleton<PowerQuest>.Get.ShouldContinueDialog(first, ref time, skippable: false, Singleton<PowerQuest>.Get.GetShouldSayTextAutoAdvance(), m_dialogAudioSource, m_dialogText, endTime);
				}
			}
			if (skipped && m_coroutineSay != null && m_coroutineSay == sayBGCoroutine)
			{
				Singleton<PowerQuest>.Get.StopCoroutine(sayBGCoroutine);
				EndSay();
			}
		}

		private IEnumerator CoroutinePlayAnimation(string animName)
		{
			StopWalking();
			if (!Singleton<PowerQuest>.Get.GetSkippingCutscene() && !(m_instance == null))
			{
				m_instance.PlayAnimation(animName);
				while (Animating && !Singleton<PowerQuest>.Get.GetSkippingCutscene())
				{
					yield return new WaitForEndOfFrame();
				}
				if (m_instance.Animating)
				{
					m_instance.StopAnimation();
				}
			}
		}

		private IEnumerator CoroutineWaitForAnimation()
		{
			if (!Singleton<PowerQuest>.Get.GetSkippingCutscene() && !(m_instance == null))
			{
				while (m_instance.Animating && !Singleton<PowerQuest>.Get.GetSkippingCutscene())
				{
					yield return new WaitForEndOfFrame();
				}
				if (m_instance.Animating)
				{
					m_instance.StopAnimation();
				}
			}
		}

		private IEnumerator CoroutineFace(eFace direction, eFace verticalFallback)
		{
			if (m_instance.GetPlayingTransition())
			{
				yield return WaitForTransition();
			}
			eFace facingVerticalFallback = m_facingVerticalFallback;
			m_targetFaceDirection = direction;
			if (verticalFallback != eFace.None)
			{
				m_facingVerticalFallback = verticalFallback;
			}
			if (m_instance.StartTurnAnimation(facingVerticalFallback))
			{
				m_faceDirection = direction;
				if (m_instance != null)
				{
					m_instance.UpdateFacingVisuals(direction);
				}
			}
			if (Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				m_targetFaceDirection = direction;
				m_instance.UpdateFacingVisuals(direction);
				yield break;
			}
			bool skip = false;
			if (m_instance != null && !skip && Visible && !Singleton<PowerQuest>.Get.GetSkippingCutscene() && !Singleton<PowerQuest>.Get.GetRoomLoading())
			{
				while (m_instance != null && !skip && Visible && !Singleton<PowerQuest>.Get.GetSkippingCutscene() && !Singleton<PowerQuest>.Get.GetRoomLoading() && (m_targetFaceDirection != m_faceDirection || m_instance.GetPlayingTurnAnimation()))
				{
					yield return new WaitForEndOfFrame();
				}
			}
			m_targetFaceDirection = m_faceDirection;
			m_instance.UpdateFacingVisuals(m_faceDirection);
		}

		private void StartSay(string line, int id = -1, bool background = false)
		{
			m_startSayCalled = true;
			PowerQuest get = Singleton<PowerQuest>.Get;
			line = SystemText.GetDisplayText(line, id, m_scriptName, IsPlayer);
			SystemAudio.Stop(m_dialogAudioSource);
			m_dialogAudioSource = null;
			m_dialogAudioSource = SystemText.PlayAudio(id, m_scriptName, (m_instance != null) ? m_instance.transform : null);
			if (get.Settings.DialogDisplay == QuestSettings.eDialogDisplay.TextOnly && m_dialogAudioSource != null)
			{
				m_dialogAudioSource.volume = 0f;
			}
			GameObject gameObject = null;
			eSpeechStyle speechStyle = get.SpeechStyle;
			int num;
			if (speechStyle == eSpeechStyle.AboveCharacter || speechStyle == eSpeechStyle.Caption || background)
			{
				if (m_dialogAudioSource != null)
				{
					num = ((get.Settings.DialogDisplay != QuestSettings.eDialogDisplay.SpeechOnly) ? 1 : 0);
					if (num == 0)
					{
						goto IL_0204;
					}
				}
				else
				{
					num = 1;
				}
				if (m_dialogText == null)
				{
					GameObject gameObject2 = UnityEngine.Object.Instantiate(get.GetDialogTextPrefab().gameObject);
					m_dialogText = gameObject2.GetComponent<QuestText>();
					gameObject2.GetComponent<TextMesh>().color = m_textColour;
				}
				else
				{
					m_dialogText.gameObject.SetActive(value: true);
				}
				gameObject = m_dialogText.gameObject;
				if (speechStyle != eSpeechStyle.Caption)
				{
					m_dialogText.OrderInLayer = (background ? (-15) : (-10));
					if (m_instance == null)
					{
						Vector3 vector = m_position;
						if (TextPositionOverride != Vector2.zero)
						{
							vector = TextPositionOverride;
						}
						m_dialogText.AttachTo(vector);
					}
					else
					{
						Vector3 vector2 = m_instance.GetTextPosition();
						m_dialogText.AttachTo(m_instance.transform, vector2);
					}
				}
				goto IL_0204;
			}
			string scriptName = ((speechStyle == eSpeechStyle.Portrait) ? "SpeechBox" : get.CustomSpeechGui);
			Gui gui = get.GetGui(scriptName);
			if (gui != null && gui.Instance != null)
			{
				gameObject = gui.Instance.gameObject;
			}
			goto IL_027b;
			IL_0204:
			if (m_instance != null)
			{
				m_instance.StartSay(line, id);
			}
			if (num != 0)
			{
				m_dialogText.SetText(line);
			}
			goto IL_027b;
			IL_027b:
			if (gameObject != null)
			{
				Array.ForEach(gameObject.GetComponents<ISpeechGui>(), delegate(ISpeechGui iSpeechGui)
				{
					iSpeechGui.StartSay(this, line, id, background);
				});
			}
		}

		private void EndSay()
		{
			if (m_startSayCalled && CallbackOnEndSay != null)
			{
				CallbackOnEndSay();
			}
			SystemAudio.Stop(m_dialogAudioSource);
			if (m_dialogText != null)
			{
				m_dialogText.gameObject.SetActive(value: false);
			}
			if (m_instance != null)
			{
				m_instance.EndSay();
			}
			GameObject gameObject = null;
			PowerQuest get = Singleton<PowerQuest>.Get;
			eSpeechStyle speechStyle = get.SpeechStyle;
			if (speechStyle == eSpeechStyle.AboveCharacter || speechStyle == eSpeechStyle.Caption)
			{
				if (m_dialogText != null)
				{
					gameObject = m_dialogText.gameObject;
				}
			}
			else
			{
				string scriptName = ((speechStyle == eSpeechStyle.Portrait) ? "SpeechBox" : get.CustomSpeechGui);
				Gui gui = get.GetGui(scriptName);
				if (gui != null && gui.Instance != null)
				{
					gameObject = gui.Instance.gameObject;
				}
			}
			if (gameObject != null)
			{
				Array.ForEach(gameObject.GetComponents<ISpeechGui>(), delegate(ISpeechGui iSpeechGui)
				{
					iSpeechGui.EndSay(this);
				});
			}
			m_startSayCalled = false;
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

		private void ResetAnimationData()
		{
			m_animOverride = null;
			m_animationTime = -1f;
			m_pauseAnimAtEnd = false;
		}

		[OnDeserializing]
		private void CopyDefaults(StreamingContext sc)
		{
			QuestUtils.InitWithDefaults(this);
		}
	}
}
