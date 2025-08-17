using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.U2D;

namespace PowerTools.Quest
{
	public class PowerQuest : Singleton<PowerQuest>, ISerializationCallbackReceiver, IQuestScriptable, IPowerQuest
	{
		[Serializable]
		private class Timer
		{
			public string n;

			public float t;
		}

		private class SavedVarCollection
		{
			public SourceList m_occurrences = new SourceList();

			public List<string> m_tempDisabledProps = new List<string>();

			public List<string> m_tempDisabledHotspots = new List<string>();

			public List<string> m_tempCursorNoneProps = new List<string>();

			public List<string> m_tempCursorNoneHotspots = new List<string>();

			public List<string> m_tempCursorNoneCursor = new List<string>();

			public List<string> m_currentInteractionOccurrences = new List<string>();

			public List<string> m_captureInputSources = new List<string>();

			public List<Timer> m_timers = new List<Timer>();

			public bool m_callEnterOnRestore;

			public bool m_useFancyParallaxSnapping = true;
		}

		private enum eInventoryClickStyle
		{
			SelectInventory = 0,
			UseInventory = 1,
			OnMouseClick = 2
		}

		private class ExtraSaveData
		{
			public string m_player = string.Empty;

			public string m_currentDialog = string.Empty;

			public string m_displayBoxGui;

			public string m_dialogTreeGui;

			public string m_customSpeechGui;

			public eSpeechStyle m_speechStyle;

			public eSpeechPortraitLocation m_speechPortraitLocation;

			public float m_transitionFadeTime;
		}

		public delegate IEnumerator DelegateWaitForFunction();

		public delegate void DelegateDelayedFunction();

		private static readonly Type TYPE_COMPILERGENERATED = typeof(CompilerGeneratedAttribute);

		private static readonly string FUNC_UPDATE = "Update";

		private static readonly string FUNC_UPDATE_NOPAUSE = "UpdateNoPause";

		public static readonly string SPRITE_NUM_POSTFIX_0 = "_0";

		private static readonly int MaxColliderInteractions = 256;

		private Collider2D[] m_tempPicked = new Collider2D[MaxColliderInteractions];

		[Header("Default In-Game Settings")]
		[Tooltip("Config Settings (like volume) that are game wide.")]
		[SerializeField]
		private QuestSettings m_settings = new QuestSettings();

		[Header("Screen Setup")]
		[Tooltip("The default vertical resolution of the game. How many pixels high the camera view will be.")]
		[SerializeField]
		private float m_verticalResolution = 180f;

		[Tooltip("The range of horizontal resolution your game supports. How many pixels wide the camera view will be. If the screen aspect ratio goes narrower or wider than this the game will be letterboxed. (Use to set what aspect ratios you support)")]
		[FormerlySerializedAs("m_horizontalResolution")]
		[SerializeField]
		private MinMaxRange m_letterboxWidth = new MinMaxRange(320f);

		[Tooltip("Whether camera and other things snap to pixel. For pixel art games")]
		[SerializeField]
		private bool m_snapToPixel = true;

		[Tooltip("Whether to set up a pixel camera that renderes sprites at pixel resolution. For pixel art games")]
		[SerializeField]
		private bool m_pixelCamEnabled;

		[Tooltip("Default pixels per unit that sprites are imported at")]
		[SerializeField]
		private float m_defaultPixelsPerUnit = 1f;

		[Header("Dialog Speech Display Setup")]
		[Tooltip("How is dialog displayed. Above head (lucasarts style), next to a portrait (sierra style), or as a caption not attached to character position")]
		[SerializeField]
		private eSpeechStyle m_speechStyle;

		[Tooltip("Which side is portrait located (currently only LEFT is implemented)")]
		[SerializeField]
		private eSpeechPortraitLocation m_speechPortraitLocation;

		[Tooltip("Prefab for displayed above character")]
		[SerializeField]
		private QuestText m_dialogTextPrefab;

		[Tooltip("Global offset of dialog text (above character sprite)")]
		[SerializeField]
		private Vector2 m_dialogTextOffset = Vector2.zero;

		[Tooltip("Set speech style to AboveCharacter to use, and implement ISpeechGui")]
		[SerializeField]
		private string m_customSpeechGui = "";

		[SerializeField]
		private string m_displayBoxGui = "DisplayBox";

		[SerializeField]
		private string m_dialogTreeGui = "DialogTree";

		[Header("Other Dialog Speech Setup")]
		[Tooltip("When clicking to skip text, ignore clicks until text has been shown for this time")]
		[SerializeField]
		private float m_textNoSkipTime = 0.25f;

		[Tooltip("Whether charaters stop walking automatically when they start talking")]
		[SerializeField]
		private bool m_stopWalkingToTalk = true;

		[Tooltip("Whether character dialog text requires a click to advance, or dismisses after dialog's spoken/after time")]
		[SerializeField]
		private bool m_sayTextAutoAdvance = true;

		[Tooltip("Whether display requires a click to advance, or dismisses after dialog's spoken/after time")]
		[SerializeField]
		private bool m_displayTextAutoAdvance;

		[Tooltip("After dialog's audio finishes, how long before going to next line (sec)")]
		[SerializeField]
		private float m_textAutoAdvanceDelay;

		[Tooltip("If true, display is shown even when subtitles off")]
		[SerializeField]
		private bool m_alwaysShowDisplayText = true;

		[Header("Project Verb Setup")]
		[SerializeField]
		private bool m_enableUse = true;

		[SerializeField]
		private bool m_enableLook = true;

		[SerializeField]
		private bool m_enableInventory = true;

		[Tooltip("Whether clicking inventory results in 'Selecting' it Broken Sword style, or 'Using' it Lucasarts style, or specified in GlobalScript's OnMouseClick")]
		[SerializeField]
		private eInventoryClickStyle m_inventoryClickStyle = eInventoryClickStyle.OnMouseClick;

		[Tooltip("When true, no editor keyboard shortcuts will be used")]
		[SerializeField]
		private bool m_customKbShortcuts;

		[Header("Screen-Fade Setup")]
		[SerializeField]
		private float m_transitionFadeTime = 0.3f;

		[SerializeField]
		private QuestMenuManager m_menuManager;

		[Header("Spawnables")]
		[Tooltip("Add objects here so you can spawn them by name in QuestScripts")]
		[SerializeField]
		private List<GameObject> m_spawnablePrefabs = new List<GameObject>();

		[Header("Project Character settings")]
		[Tooltip("Controls what angles the player is considered 'facing' a direction (right, up, down-left,etc). Increase to favour cardinal directions more than diagonals")]
		[Range(45f, 90f)]
		[SerializeField]
		private float m_facingSegmentAngle = 45f;

		[Header("Save Game Settings")]
		[Tooltip("Height in pixels of screenshot recorded in save-game slot data. Set to 0 to disable saving screenshot with save games")]
		[SerializeField]
		private int m_saveScreenshotHeight = 180;

		[Tooltip("Increase when the data you're saving changes, and you need to know if you're loading an old save game")]
		[SerializeField]
		private int m_saveVersion;

		[Tooltip("Increase when you can no longer save games of a specific version. After launch you should avoid increasing this if possible or player's save files get invalidated!")]
		[SerializeField]
		private int m_saveVersionRequired;

		[Header("Text Sprite Setup")]
		public Material m_textSpriteMaterial;

		[ReorderableArray]
		[NonReorderable]
		public QuestText.TextSpriteData[] m_textSprites;

		[Header("Other Systems To Create")]
		[SerializeField]
		private List<Component> m_systems;

		[Header("Prefab Lists (Read only, enable debug inspector to edit)")]
		[SerializeField]
		[ReadOnly]
		private QuestCursorComponent m_cursorPrefab;

		[SerializeField]
		[ReadOnly]
		private QuestCameraComponent m_cameraPrefab;

		[SerializeField]
		[ReadOnly]
		[NonReorderable]
		private List<RoomComponent> m_roomPrefabs = new List<RoomComponent>();

		[SerializeField]
		[ReadOnly]
		[NonReorderable]
		private List<CharacterComponent> m_characterPrefabs = new List<CharacterComponent>();

		[SerializeField]
		[ReadOnly]
		[NonReorderable]
		private List<GuiComponent> m_guiPrefabs = new List<GuiComponent>();

		[SerializeField]
		[ReadOnly]
		[NonReorderable]
		private List<DialogTreeComponent> m_dialogTreePrefabs = new List<DialogTreeComponent>();

		[SerializeField]
		[ReadOnly]
		[NonReorderable]
		private List<InventoryComponent> m_inventoryPrefabs = new List<InventoryComponent>();

		[SerializeField]
		[ReadOnly]
		[NonReorderable]
		private List<AnimationClip> m_inventoryAnimations = new List<AnimationClip>();

		[SerializeField]
		[ReadOnly]
		[NonReorderable]
		private List<Sprite> m_inventorySprites = new List<Sprite>();

		[SerializeField]
		[ReadOnly]
		[NonReorderable]
		private List<AnimationClip> m_guiAnimations = new List<AnimationClip>();

		[SerializeField]
		[ReadOnly]
		[NonReorderable]
		private List<Sprite> m_guiSprites = new List<Sprite>();

		[Header("Pre-loaded Shaders")]
		[SerializeField]
		private Shader[] m_preloadedShaders;

		[SerializeField]
		[HideInInspector]
		private int m_version = -1;

		[SerializeField]
		[HideInInspector]
		private int m_newVersion = -1;

		private QuestScript m_globalScript;

		private QuestCamera m_cameraData = new QuestCamera();

		private Camera m_cameraGui;

		private Canvas m_canvas;

		private QuestCursor m_cursor;

		private Character m_player;

		private List<Character> m_characters = new List<Character>();

		private List<Inventory> m_inventoryItems = new List<Inventory>();

		private List<DialogTree> m_dialogTrees = new List<DialogTree>();

		private List<Gui> m_guis = new List<Gui>();

		private List<Room> m_rooms = new List<Room>();

		private Room m_currentRoom;

		private DialogOption m_dialogOptionSelected;

		private DialogTree m_currentDialog;

		private DialogTree m_previousDialog;

		private bool m_skipCutscene;

		private bool m_interruptNextLine;

		private float m_interruptNextLineTime;

		private bool m_skipDialog;

		private bool m_blocking;

		private bool m_transitioning;

		private bool m_roomLoopStarted;

		private bool m_cutscene;

		private bool m_initialised;

		private bool m_walkClickDown;

		private bool m_guiConsumedClick;

		private bool m_sequenceIsCancelable;

		private bool m_allowEnableCancel;

		private bool m_serializeComplete;

		private bool m_restoring;

		private bool m_displayActive;

		private bool m_skipCutsceneButtonConsumed;

		private bool m_waitingForBGDialogSkip;

		private bool m_restartOnUpdate;

		private static bool s_hasRestarted = false;

		private static string s_restartScene = null;

		private static string s_restartPlayFromFunction = null;

		private static Assembly s_restartAssembly = null;

		private bool m_leftClickPrev;

		private bool m_rightClickPrev;

		private bool m_overrideMousePos;

		private Vector2 m_mousePos = Vector2.zero;

		private Vector2 m_mousePosGui = Vector2.zero;

		private float m_timeLastTextShown;

		private Coroutine m_currentSequence;

		private List<Coroutine> m_queuedScriptInteractions = new List<Coroutine>();

		private Coroutine m_coroutineMainLoop;

		private IEnumerator m_coroutineSay;

		private bool m_levelLoadedCalled;

		private bool m_overrideMouseOverClickable;

		private IQuestClickable m_mouseOverClickable;

		private IQuestClickable m_lastClickable;

		private string m_mouseOverDescriptionOverride;

		private AudioHandle m_dialogAudioSource;

		private SavedVarCollection m_savedVars = new SavedVarCollection();

		private Coroutine m_backgroundSequence;

		private List<Coroutine> m_currentSequences = new List<Coroutine>();

		private List<Coroutine> m_backgroundSequences = new List<Coroutine>();

		private List<IQuestClickable> m_currentInteractionClickables = new List<IQuestClickable>();

		private List<eQuestVerb> m_currentInteractionVerbs = new List<eQuestVerb>();

		private static Assembly m_hotLoadAssembly = null;

		private static readonly string STR_UNHANDLED = "Unhandled";

		private IQuestScriptable m_autoLoadScriptable;

		private string m_autoLoadFunction = string.Empty;

		private IQuestScriptable m_autoLoadUnhandledScriptable;

		private string m_autoLoadUnhandledFunction = string.Empty;

		private Coroutine m_consumedInteraction;

		private int m_inlineDialogResult = -1;

		private DialogTree m_inlineDialogPrevDialog;

		public Action CallbackOnDialogSkipped;

		public Action<ICharacter, IInventory> CallbackOnInventoryCollected;

		public Action CallbackOnEndCutscene;

		public Action<bool> CallbackOnProcessClick;

		public Action CallbackOnBlock;

		public Action CallbackOnUnblock;

		private bool m_lostFocus;

		private bool m_hasFocus;

		private static Dictionary<string, Action<SpriteAtlas>> s_roomAtlasCallbacks = new Dictionary<string, Action<SpriteAtlas>>();

		private bool m_ignoreAutoLoadFunc;

		private string m_autoLoadFunc = string.Empty;

		private SpriteAtlas m_lastAtlas;

		private SpriteAtlas m_atlasToUnload;

		private bool m_loadingAtlas;

		public static readonly string GLOBAL_SCRIPT_NAME = "GlobalScript";

		public static readonly string SCRIPT_FUNCTION_INTERACT = "OnInteract";

		public static readonly string SCRIPT_FUNCTION_LOOKAT = "OnLookAt";

		public static readonly string SCRIPT_FUNCTION_USEINV = "OnUseInv";

		public static readonly string SCRIPT_FUNCTION_DIALOG_START_OLD = "Start";

		public static readonly string SCRIPT_FUNCTION_DIALOG_START = "OnStart";

		public static readonly string SCRIPT_FUNCTION_DIALOG_STOP = "OnStop";

		public static readonly string SCRIPT_FUNCTION_DIALOG_OPTION = "Option";

		public static readonly string STR_HOTSPOT = "Hotspot";

		public static readonly string STR_PROP = "Prop";

		public static readonly string STR_REGION = "Region";

		public static readonly string STR_CHARACTER = "Character";

		public static readonly string STR_INVENTORY = "Inventory";

		public static readonly string SCRIPT_FUNCTION_INTERACT_PROP = SCRIPT_FUNCTION_INTERACT + STR_PROP;

		public static readonly string SCRIPT_FUNCTION_INTERACT_HOTSPOT = SCRIPT_FUNCTION_INTERACT + STR_HOTSPOT;

		public static readonly string SCRIPT_FUNCTION_INTERACT_INVENTORY = SCRIPT_FUNCTION_INTERACT + STR_INVENTORY;

		public static readonly string SCRIPT_FUNCTION_INTERACT_CHARACTER = SCRIPT_FUNCTION_INTERACT + STR_CHARACTER;

		public static readonly string SCRIPT_FUNCTION_LOOKAT_PROP = SCRIPT_FUNCTION_LOOKAT + STR_PROP;

		public static readonly string SCRIPT_FUNCTION_LOOKAT_HOTSPOT = SCRIPT_FUNCTION_LOOKAT + STR_HOTSPOT;

		public static readonly string SCRIPT_FUNCTION_LOOKAT_INVENTORY = SCRIPT_FUNCTION_LOOKAT + STR_INVENTORY;

		public static readonly string SCRIPT_FUNCTION_LOOKAT_CHARACTER = SCRIPT_FUNCTION_LOOKAT + STR_CHARACTER;

		public static readonly string SCRIPT_FUNCTION_USEINV_PROP = SCRIPT_FUNCTION_USEINV + STR_PROP;

		public static readonly string SCRIPT_FUNCTION_USEINV_HOTSPOT = SCRIPT_FUNCTION_USEINV + STR_HOTSPOT;

		public static readonly string SCRIPT_FUNCTION_USEINV_INVENTORY = SCRIPT_FUNCTION_USEINV + STR_INVENTORY;

		public static readonly string SCRIPT_FUNCTION_USEINV_CHARACTER = SCRIPT_FUNCTION_USEINV + STR_CHARACTER;

		public static readonly string SCRIPT_FUNCTION_ENTER_REGION = "OnEnterRegion";

		public static readonly string SCRIPT_FUNCTION_EXIT_REGION = "OnExitRegion";

		public static readonly string SCRIPT_FUNCTION_ENTER_REGION_BG = "OnEnterRegionBG";

		public static readonly string SCRIPT_FUNCTION_EXIT_REGION_BG = "OnExitRegionBG";

		public static readonly string SCRIPT_FUNCTION_GETCURSOR = "GetCursor";

		public static readonly string SCRIPT_FUNCTION_ONMOUSECLICK = "OnMouseClick";

		public static readonly string SCRIPT_FUNCTION_ONWALKTO = "OnWalkTo";

		public static readonly string SCRIPT_FUNCTION_ONANYCLICK = "OnAnyClick";

		public static readonly string SCRIPT_FUNCTION_AFTERANYCLICK = "AfterAnyClick";

		public static readonly string SCRIPT_FUNCTION_CLICKGUI = "OnClick";

		public static readonly string SCRIPT_FUNCTION_DRAGGUI = "OnDrag";

		public static readonly string SCRIPT_FUNCTION_ONKBFOCUS = "OnKeyboardFocus";

		public static readonly string SCRIPT_FUNCTION_ONKBDEFOCUS = "OnKeyboardDefocus";

		public static readonly string SCRIPT_FUNCTION_ONTEXTEDIT = "OnTextEdit";

		public static readonly string SCRIPT_FUNCTION_ONTEXTCONFIRM = "OnTextConfirm";

		private static readonly YieldInstruction EMPTY_YIELD_INSTRUCTION = new YieldInstruction();

		private static readonly YieldInstruction CONSUME_YIELD_INSTRUCTION = new YieldInstruction();

		private static readonly float TEXT_DISPLAY_TIME_MIN = 1f;

		private static readonly float TEXT_DISPLAY_TIME_CHARACTER = 0.1f;

		private static readonly string DEFAULT_FADE_SOURCE = "";

		private static int LAYER_UI = -1;

		private Gui m_focusedGui;

		private GuiControl m_focusedControl;

		private GuiControl m_keyboardFocusedControl;

		private IGui m_blockingGui;

		private GuiControl m_focusedControlLock;

		private List<Gui> m_sortedGuis = new List<Gui>();

		private static readonly string STR_ROOM_START = "Ro";

		public static readonly string SAV_SETTINGS = "Settings";

		public static readonly string SAV_SETTINGS_FILE = "Settings.sav";

		public static readonly int SAV_SETTINGS_VER = 0;

		public static readonly int SAV_SETTINGS_VER_REQ = 0;

		private QuestSaveManager m_saveManager = new QuestSaveManager();

		private int m_restoredVersion = -1;

		private static readonly string STR_ON_POST_RESTORE = "OnPostRestore";

		public bool UseFancyParalaxSnapping
		{
			get
			{
				return SV.m_useFancyParallaxSnapping;
			}
			set
			{
				SV.m_useFancyParallaxSnapping = value;
			}
		}

		private SavedVarCollection SV => m_savedVars;

		public bool DialogInterruptRequested => m_interruptNextLine;

		public float DialogInterruptDuration => m_interruptNextLineTime;

		public float FacingSegmentAngle => m_facingSegmentAngle;

		public YieldInstruction Break => EMPTY_YIELD_INSTRUCTION;

		public YieldInstruction ConsumeEvent => CONSUME_YIELD_INSTRUCTION;

		public bool IsDebugBuild => Debug.isDebugBuild;

		public ICamera Camera => GetCamera();

		public ICursor Cursor => GetCursor();

		public Color FadeColor
		{
			get
			{
				return m_menuManager.FadeColor;
			}
			set
			{
				m_menuManager.FadeColor = value;
			}
		}

		public Color FadeColorDefault
		{
			get
			{
				return m_menuManager.FadeColorDefault;
			}
			set
			{
				m_menuManager.FadeColorDefault = value;
			}
		}

		public bool Paused
		{
			get
			{
				return SystemTime.Paused;
			}
			set
			{
				if (value)
				{
					Pause();
				}
				else
				{
					UnPause();
				}
			}
		}

		public ICharacter Player
		{
			get
			{
				return GetSavable(m_player);
			}
			set
			{
				SetPlayer(value, 0.6f);
			}
		}

		public IInventory ActiveInventory
		{
			get
			{
				return GetSavable(m_player.ActiveInventory as Inventory);
			}
			set
			{
				m_player.ActiveInventory = value;
			}
		}

		public bool GameHasKeyboardFocus => m_keyboardFocusedControl == null;

		public float VerticalResolution
		{
			get
			{
				if (m_currentRoom != null && m_currentRoom.VerticalResolution > 0f)
				{
					return m_currentRoom.VerticalResolution;
				}
				return m_verticalResolution;
			}
		}

		public float DefaultVerticalResolution => m_verticalResolution;

		public MinMaxRange HorizontalResolution => m_letterboxWidth;

		public QuestSettings Settings => m_settings;

		public string DisplayBoxGui
		{
			get
			{
				return m_displayBoxGui;
			}
			set
			{
				m_displayBoxGui = value;
			}
		}

		public string DialogTreeGui
		{
			get
			{
				return m_dialogTreeGui;
			}
			set
			{
				m_dialogTreeGui = value;
			}
		}

		public string CustomSpeechGui
		{
			get
			{
				return m_customSpeechGui;
			}
			set
			{
				m_customSpeechGui = value;
			}
		}

		public bool AlwaysShowDisplayText
		{
			get
			{
				return m_alwaysShowDisplayText;
			}
			set
			{
				m_alwaysShowDisplayText = value;
			}
		}

		public eSpeechStyle SpeechStyle
		{
			get
			{
				return m_speechStyle;
			}
			set
			{
				m_speechStyle = value;
			}
		}

		public eSpeechPortraitLocation SpeechPortraitLocation
		{
			get
			{
				return m_speechPortraitLocation;
			}
			set
			{
				m_speechPortraitLocation = value;
			}
		}

		public float TransitionFadeTime
		{
			get
			{
				return m_transitionFadeTime;
			}
			set
			{
				m_transitionFadeTime = value;
			}
		}

		public int InlineDialogResult => m_inlineDialogResult;

		public Pathfinder Pathfinder
		{
			get
			{
				if (GetCurrentRoom() == null)
				{
					return null;
				}
				return GetCurrentRoom().GetInstance().GetPathfinder();
			}
		}

		public bool UseCustomKBShortcuts => m_customKbShortcuts;

		public float SnapAmount
		{
			get
			{
				if (!m_snapToPixel)
				{
					return 0f;
				}
				return 1f;
			}
		}

		public int EditorNewVersion
		{
			get
			{
				return m_newVersion;
			}
			set
			{
				m_newVersion = value;
			}
		}

		public void Restart()
		{
			s_hasRestarted = true;
			s_restartScene = null;
			s_restartPlayFromFunction = null;
			s_restartAssembly = m_hotLoadAssembly;
			m_restartOnUpdate = true;
			StopAllCoroutines();
		}

		public void Restart(IRoom room, string playFromFunction = null)
		{
			s_hasRestarted = true;
			s_restartScene = (room as Room).GetSceneName();
			LoadAtlas(room.ScriptName);
			s_restartPlayFromFunction = playFromFunction;
			s_restartAssembly = m_hotLoadAssembly;
			m_restartOnUpdate = true;
			StopAllCoroutines();
		}

		public Coroutine Wait(float time = 0.5f)
		{
			return StartQuestCoroutine(CoroutineWaitForTime(time, skippable: false));
		}

		public Coroutine WaitSkip(float time = 0.5f)
		{
			return StartQuestCoroutine(CoroutineWaitForTime(time, skippable: true));
		}

		public Coroutine WaitForTimer(string timerName, bool skippable = false)
		{
			return StartQuestCoroutine(CoroutineWaitForTimer(timerName, skippable: true));
		}

		public void DelayedInvoke(float time, Action functionToInvoke)
		{
			StartQuestCoroutine(CoroutineDelayedInvoke(time, functionToInvoke));
		}

		public Coroutine WaitFor(DelegateWaitForFunction functionToWaitFor, bool autoLoadQuestScript = true)
		{
			if (Application.isEditor && functionToWaitFor != null && functionToWaitFor.Target != null && autoLoadQuestScript)
			{
				List<IQuestScriptable> allScriptables = Singleton<PowerQuest>.Get.GetAllScriptables();
				string classname = functionToWaitFor.Target.GetType().Name;
				IQuestScriptable questScriptable = allScriptables.Find((IQuestScriptable item) => item.GetScriptClassName() == classname);
				if (questScriptable != null && !Attribute.IsDefined(functionToWaitFor.Method, TYPE_COMPILERGENERATED))
				{
					SetAutoLoadScript(questScriptable, functionToWaitFor.Method.Name, functionBlocked: true, isWaitForFunction: true);
				}
			}
			return StartQuestCoroutine(functionToWaitFor(), cancelable: true);
		}

		public Coroutine WaitWhile(Func<bool> condition, bool skippable = false)
		{
			return StartQuestCoroutine(CoroutineWaitWhile(condition, skippable));
		}

		public Coroutine WaitUntil(Func<bool> condition, bool skippable = false)
		{
			return StartQuestCoroutine(CoroutineWaitUntil(condition, skippable));
		}

		public Coroutine WaitForDialog()
		{
			return StartQuestCoroutine(CoroutineWaitForDialog());
		}

		public Coroutine Display(string dialog, int id = -1)
		{
			if (m_coroutineSay != null)
			{
				StopCoroutine(m_coroutineSay);
				EndDisplay();
			}
			m_coroutineSay = CoroutineDisplay(dialog, id);
			return StartCoroutine(m_coroutineSay);
		}

		public Coroutine DisplayBG(string dialog, int id = -1)
		{
			if (m_coroutineSay != null)
			{
				StopCoroutine(m_coroutineSay);
				EndDisplay();
			}
			m_coroutineSay = CoroutineDisplayBG(dialog, id);
			StartCoroutine(m_coroutineSay);
			return StartCoroutine(CoroutineEmpty());
		}

		public void StartCutscene()
		{
			m_cutscene = true;
		}

		public void EndCutscene()
		{
			OnEndCutscene();
		}

		public Coroutine FadeIn(float time = 0.2f, bool skippable = true)
		{
			return StartCoroutine(CoroutineFadeIn(DEFAULT_FADE_SOURCE, time, skippable));
		}

		public Coroutine FadeOut(float time = 0.2f, bool skippable = true)
		{
			return StartCoroutine(CoroutineFadeOut(DEFAULT_FADE_SOURCE, time, skippable));
		}

		public Coroutine FadeIn(float time, string source, bool skippable = true)
		{
			return StartCoroutine(CoroutineFadeIn(source, time, skippable));
		}

		public Coroutine FadeOut(float time, string source, bool skippable = true)
		{
			return StartCoroutine(CoroutineFadeOut(source, time, skippable));
		}

		public void FadeInBG(float time = 0.2f)
		{
			m_menuManager.FadeIn(time, DEFAULT_FADE_SOURCE);
		}

		public void FadeOutBG(float time = 0.2f)
		{
			m_menuManager.FadeOut(time, DEFAULT_FADE_SOURCE);
		}

		public void FadeInBG(float time, string source)
		{
			m_menuManager.FadeIn(time, source);
		}

		public void FadeOutBG(float time, string source)
		{
			m_menuManager.FadeOut(time, source);
		}

		public bool GetFading()
		{
			return m_menuManager.GetFading();
		}

		public void FadeColorRestore()
		{
			m_menuManager.FadeColorRestore();
		}

		public float GetFadeRatio()
		{
			return m_menuManager.GetFadeRatio();
		}

		public Color GetFadeColor()
		{
			return m_menuManager.GetFadeColor();
		}

		public QuestMenuManager GetMenuManager()
		{
			return m_menuManager;
		}

		public void Pause(string source = null)
		{
			if (Singleton<SystemTime>.HasInstance())
			{
				Singleton<SystemTime>.Get.PauseGame(source);
			}
		}

		public void UnPause(string source = null)
		{
			if (Singleton<SystemTime>.HasInstance())
			{
				Singleton<SystemTime>.Get.UnPauseGame(source);
			}
		}

		private Timer FindTimer(string name)
		{
			foreach (Timer timer in SV.m_timers)
			{
				if (timer.n.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					return timer;
				}
			}
			return null;
		}

		public void SetTimer(string name, float time)
		{
			Timer timer = FindTimer(name);
			if (time <= 0f)
			{
				if (timer != null)
				{
					SV.m_timers.Remove(timer);
				}
				return;
			}
			if (timer == null)
			{
				timer = new Timer
				{
					n = name,
					t = time
				};
				SV.m_timers.Add(timer);
			}
			timer.t = time;
		}

		public bool GetTimerExpired(string name)
		{
			Timer timer = FindTimer(name);
			if (timer != null && timer.t <= 0f)
			{
				SV.m_timers.Remove(timer);
				return true;
			}
			return false;
		}

		public float GetTimer(string name)
		{
			Timer timer = FindTimer(name);
			if (timer == null || !(timer.t > 0f))
			{
				return 0f;
			}
			return timer.t;
		}

		public void ChangeRoomBG(IRoom room)
		{
			GetPlayer().Room = room;
		}

		public Coroutine ChangeRoom(IRoom room)
		{
			return StartCoroutine(CoroutineChangeRoom(room));
		}

		public T GetScript<T>() where T : QuestScript
		{
			string text = typeof(T).ToString();
			IQuestScriptable questScriptable = null;
			Type typeFromHandle = typeof(T);
			if (typeFromHandle.IsSubclassOf(typeof(RoomScript<T>)))
			{
				text = text.Substring(4);
				questScriptable = GetRoom(text);
			}
			else if (typeFromHandle.IsSubclassOf(typeof(CharacterScript<T>)))
			{
				text = text.Substring(9);
				questScriptable = GetCharacter(text);
			}
			else if (typeFromHandle.IsSubclassOf(typeof(DialogTreeScript<T>)))
			{
				text = text.Substring(6);
				questScriptable = GetDialogTree(text);
			}
			else if (typeFromHandle.IsSubclassOf(typeof(InventoryScript<T>)))
			{
				text = text.Substring(9);
				questScriptable = GetInventory(text);
			}
			else if (typeFromHandle.IsSubclassOf(typeof(GuiScript<T>)))
			{
				text = text.Substring(3);
				questScriptable = GetGui(text);
			}
			else if (typeFromHandle.ToString() == GLOBAL_SCRIPT_NAME)
			{
				text = GLOBAL_SCRIPT_NAME;
				questScriptable = this;
			}
			if (questScriptable != null && questScriptable.GetScript() != null)
			{
				return questScriptable.GetScript() as T;
			}
			return null;
		}

		public IRoom GetRestoringRoom()
		{
			return GetSavable(m_player.Room as Room);
		}

		public Room GetCurrentRoom()
		{
			return GetSavable(m_currentRoom);
		}

		public void DebugSetPreviousRoom(IRoom room)
		{
			GetPlayer().DebugSetLastRoom(room);
		}

		public Room GetRoom(string scriptName)
		{
			Room room = QuestUtils.FindScriptable(m_rooms, scriptName);
			if (room == null && !string.IsNullOrEmpty(scriptName))
			{
				Debug.LogError("Room doesn't exist: " + scriptName + ". Check for typos and that it's added to PowerQuest");
			}
			return GetSavable(room);
		}

		public Character GetPlayer()
		{
			return GetSavable(m_player);
		}

		public void SetPlayer(ICharacter character, float cameraTransitionTime = 0f)
		{
			bool flag = character != null && m_player != null && character.Room == m_player.Room;
			Character character2 = (m_player = GetCharacter(character.ScriptName));
			GetCamera().SetCharacterToFollow(character2, flag ? cameraTransitionTime : 0f);
			ChangeRoomBG(m_player.Room);
		}

		public Character GetCharacter(string scriptName)
		{
			Systems.Text.LastPlayerName = SystemText.ePlayerName.Character;
			return GetSavable(QuestUtils.FindScriptable(m_characters, scriptName));
		}

		public List<Inventory> GetInventoryItems()
		{
			return m_inventoryItems;
		}

		public Inventory GetInventory(string scriptName)
		{
			return GetSavable(QuestUtils.FindScriptable(m_inventoryItems, scriptName));
		}

		public static T GetSavable<T>(T savable) where T : IQuestSaveCachable
		{
			if (savable != null)
			{
				savable.SaveDirty = true;
			}
			return savable;
		}

		public DialogTree GetCurrentDialog()
		{
			return GetSavable(m_currentDialog);
		}

		public DialogTree GetPreviousDialog()
		{
			return GetSavable(m_previousDialog);
		}

		public DialogTree GetDialogTree(string scriptName)
		{
			return GetSavable(QuestUtils.FindScriptable(m_dialogTrees, scriptName));
		}

		public Gui GetGui(string scriptName)
		{
			return QuestUtils.FindScriptable(m_guis, scriptName);
		}

		public GameObject GetSpawnablePrefab(string name)
		{
			return QuestUtils.FindByName(m_spawnablePrefabs, name);
		}

		public Camera GetCameraGui()
		{
			return m_cameraGui;
		}

		public Canvas GetCanvas()
		{
			return m_canvas;
		}

		public Vector2 GetMousePosition()
		{
			return m_mousePos;
		}

		public Vector2 GetMousePositionGui()
		{
			return m_mousePosGui;
		}

		public bool GetHasMousePositionOverride()
		{
			return m_overrideMousePos;
		}

		public void SetMousePositionOverride(Vector2 mousePos)
		{
			m_overrideMousePos = true;
			m_mousePos = mousePos;
		}

		public void ResetMousePositionOverride()
		{
			m_overrideMousePos = false;
		}

		public IQuestClickable GetMouseOverClickable()
		{
			return m_mouseOverClickable;
		}

		public eQuestClickableType GetMouseOverType()
		{
			if (m_mouseOverClickable != null)
			{
				return m_mouseOverClickable.ClickableType;
			}
			return eQuestClickableType.None;
		}

		public void SetMouseOverClickableOverride(IQuestClickable clickable)
		{
			if (!m_focusedControlLock)
			{
				m_overrideMouseOverClickable = true;
				m_mouseOverClickable = clickable;
			}
		}

		public void ResetMouseOverClickableOverride()
		{
			m_overrideMouseOverClickable = false;
			if (!m_focusedControlLock)
			{
				m_mouseOverClickable = null;
			}
		}

		public string GetMouseOverDescription()
		{
			if (m_mouseOverDescriptionOverride != null)
			{
				return m_mouseOverDescriptionOverride;
			}
			if (m_mouseOverClickable == null)
			{
				return string.Empty;
			}
			return m_mouseOverClickable.Description;
		}

		public Vector2 GetLastLookAt()
		{
			if (m_lastClickable != null && !(m_lastClickable.Instance == null))
			{
				return m_lastClickable.LookAtPoint + (Vector2)m_lastClickable.Instance.transform.position;
			}
			return Vector2.zero;
		}

		public Vector2 GetLastWalkTo()
		{
			if (m_lastClickable != null && !(m_lastClickable.Instance == null))
			{
				return m_lastClickable.WalkToPoint + (Vector2)m_lastClickable.Instance.transform.position;
			}
			return Vector2.zero;
		}

		public IGui GetFocusedGui()
		{
			return m_focusedGui;
		}

		public IGuiControl GetFocusedGuiControl()
		{
			if (!Singleton<PowerQuest>.Get.GetBlocked() || (m_focusedGui != null && m_focusedGui == m_blockingGui))
			{
				return m_focusedControl;
			}
			return null;
		}

		public GuiControl GetKeyboardFocus()
		{
			return m_keyboardFocusedControl;
		}

		public void SetKeyboardFocus(GuiControl control)
		{
			if (!(control == m_keyboardFocusedControl))
			{
				if (m_keyboardFocusedControl != null)
				{
					m_keyboardFocusedControl.OnKeyboardDefocus();
				}
				m_keyboardFocusedControl = control;
				if (m_keyboardFocusedControl != null)
				{
					m_keyboardFocusedControl.OnKeyboardFocus();
				}
			}
		}

		public void EditorSetHorizontalResolution(MinMaxRange range)
		{
			m_letterboxWidth = range;
		}

		public bool ProcessGuiClick(Gui gui, GuiControl control = null)
		{
			_ = control == null;
			if (control != null)
			{
				gui = control.GuiData;
			}
			bool result = false;
			if (gui == null)
			{
				return result;
			}
			if ((Paused || gui.Modal) && m_inventoryClickStyle == eInventoryClickStyle.OnMouseClick)
			{
				MethodInfo methodInfo = null;
				if (m_globalScript != null)
				{
					methodInfo = m_globalScript.GetType().GetMethod(SCRIPT_FUNCTION_ONMOUSECLICK, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (methodInfo != null)
					{
						methodInfo.Invoke(m_globalScript, new object[2]
						{
							Input.GetMouseButton(0),
							Input.GetMouseButton(1)
						});
					}
					if (m_queuedScriptInteractions.Count > 0)
					{
						result = true;
					}
				}
			}
			if (StartScriptInteraction(gui.GetScriptable(), SCRIPT_FUNCTION_ONANYCLICK, new object[1] { control }, stopPlayerMoving: false, cancelCurrentInteraction: true))
			{
				m_queuedScriptInteractions.Add(m_currentSequence);
				result = true;
			}
			if (!Input.GetMouseButton(1) && control != null && StartScriptInteraction(gui.GetScriptable(), SCRIPT_FUNCTION_CLICKGUI + control.ScriptName, new object[1] { control }, stopPlayerMoving: false, cancelCurrentInteraction: true))
			{
				m_queuedScriptInteractions.Add(m_currentSequence);
				result = true;
			}
			return result;
		}

		public bool ProcessGuiEvent(string eventName, Gui gui, GuiControl control = null)
		{
			_ = control == null;
			if (control != null)
			{
				gui = control.GuiData;
			}
			bool result = false;
			if (StartScriptInteraction(gui.GetScriptable(), eventName, new object[1] { control }, stopPlayerMoving: false, cancelCurrentInteraction: true))
			{
				m_queuedScriptInteractions.Add(m_currentSequence);
				result = true;
			}
			if (control != null && StartScriptInteraction(gui.GetScriptable(), eventName + control.ScriptName, new object[1] { control }, stopPlayerMoving: false, cancelCurrentInteraction: true))
			{
				m_queuedScriptInteractions.Add(m_currentSequence);
				result = true;
			}
			return result;
		}

		public bool ProcessClick(eQuestVerb verb)
		{
			return ProcessClick(verb, m_mouseOverClickable, m_mousePos);
		}

		public bool ProcessClick(eQuestVerb verb, IQuestClickable clickable, Vector2 mousePosition)
		{
			bool flag = false;
			bool flag2 = false;
			GameObject gameObject = ((clickable == null || clickable.Instance == null) ? null : clickable.Instance.gameObject);
			if (!flag2)
			{
				if (!flag2)
				{
					flag2 = StartScriptInteraction(m_currentRoom, SCRIPT_FUNCTION_ONANYCLICK);
				}
				if (!flag2)
				{
					flag2 = StartScriptInteraction(this, SCRIPT_FUNCTION_ONANYCLICK);
				}
				if (flag2)
				{
					m_queuedScriptInteractions.Add(m_currentSequence);
					flag = true;
				}
			}
			if (!flag2 && verb == eQuestVerb.None)
			{
				flag2 = true;
			}
			if (!flag2 && verb == eQuestVerb.Walk)
			{
				OnInteraction(null, eQuestVerb.Walk);
				if (!flag2)
				{
					flag2 = StartScriptInteraction(m_currentRoom, SCRIPT_FUNCTION_ONWALKTO);
				}
				if (!flag2)
				{
					flag2 = StartScriptInteraction(this, SCRIPT_FUNCTION_ONWALKTO);
				}
				if (flag2)
				{
					m_queuedScriptInteractions.Add(m_currentSequence);
					flag = true;
				}
				if (!flag2)
				{
					m_player.WalkToBG(mousePosition);
					if (mousePosition == m_mousePos)
					{
						m_walkClickDown = true;
					}
					flag2 = true;
					flag = true;
				}
			}
			if (gameObject != null && !flag2)
			{
				CharacterComponent component = gameObject.GetComponent<CharacterComponent>();
				if (component != null)
				{
					m_lastClickable = clickable;
					OnInteraction(m_lastClickable, verb);
					switch (verb)
					{
					case eQuestVerb.Inventory:
						flag2 = StartScriptInteraction(m_currentRoom.GetScriptable(), SCRIPT_FUNCTION_USEINV_CHARACTER + m_lastClickable.ScriptName, new object[2]
						{
							component.GetData(),
							m_player.ActiveInventory
						}, stopPlayerMoving: true);
						break;
					case eQuestVerb.Look:
						flag2 = StartScriptInteraction(m_currentRoom.GetScriptable(), SCRIPT_FUNCTION_LOOKAT_CHARACTER + m_lastClickable.ScriptName, new object[1] { component.GetData() }, stopPlayerMoving: true);
						break;
					case eQuestVerb.Use:
						flag2 = StartScriptInteraction(m_currentRoom.GetScriptable(), SCRIPT_FUNCTION_INTERACT_CHARACTER + m_lastClickable.ScriptName, new object[1] { component.GetData() }, stopPlayerMoving: true);
						break;
					}
					if (!flag2)
					{
						switch (verb)
						{
						case eQuestVerb.Inventory:
							flag2 = StartScriptInteraction(m_lastClickable.GetScriptable(), SCRIPT_FUNCTION_USEINV, new object[1] { m_player.ActiveInventory }, stopPlayerMoving: true);
							break;
						case eQuestVerb.Look:
							flag2 = StartScriptInteraction(m_lastClickable.GetScriptable(), SCRIPT_FUNCTION_LOOKAT, null, stopPlayerMoving: true);
							break;
						case eQuestVerb.Use:
							flag2 = StartScriptInteraction(m_lastClickable.GetScriptable(), SCRIPT_FUNCTION_INTERACT, null, stopPlayerMoving: true);
							break;
						}
					}
					if (flag2)
					{
						m_queuedScriptInteractions.Add(m_currentSequence);
						flag = true;
					}
				}
			}
			if (gameObject != null && !flag2)
			{
				HotspotComponent component2 = gameObject.GetComponent<HotspotComponent>();
				if (component2 != null)
				{
					m_lastClickable = clickable;
					OnInteraction(m_lastClickable, verb);
					flag2 = ((verb != eQuestVerb.Inventory || !m_player.HasActiveInventory) ? StartScriptInteraction(m_lastClickable.GetScriptable(), ((verb != eQuestVerb.Look) ? SCRIPT_FUNCTION_INTERACT_HOTSPOT : SCRIPT_FUNCTION_LOOKAT_HOTSPOT) + m_lastClickable.ScriptName, new object[1] { component2.GetData() }, stopPlayerMoving: true) : StartScriptInteraction(m_lastClickable.GetScriptable(), SCRIPT_FUNCTION_USEINV_HOTSPOT + m_lastClickable.ScriptName, new object[2]
					{
						component2.GetData(),
						m_player.ActiveInventory
					}, stopPlayerMoving: true));
					if (flag2)
					{
						m_queuedScriptInteractions.Add(m_currentSequence);
						flag = true;
					}
				}
			}
			if (gameObject != null && !flag2)
			{
				PropComponent component3 = gameObject.GetComponent<PropComponent>();
				if (component3 != null)
				{
					m_lastClickable = clickable;
					OnInteraction(m_lastClickable, verb);
					flag2 = ((verb != eQuestVerb.Inventory || !m_player.HasActiveInventory) ? StartScriptInteraction(m_lastClickable.GetScriptable(), ((verb != eQuestVerb.Look) ? SCRIPT_FUNCTION_INTERACT_PROP : SCRIPT_FUNCTION_LOOKAT_PROP) + m_lastClickable.ScriptName, new object[1] { component3.GetData() }, stopPlayerMoving: true) : StartScriptInteraction(m_lastClickable.GetScriptable(), SCRIPT_FUNCTION_USEINV_PROP + m_lastClickable.ScriptName, new object[2]
					{
						component3.GetData(),
						m_player.ActiveInventory
					}, stopPlayerMoving: true));
					if (flag2)
					{
						m_queuedScriptInteractions.Add(m_currentSequence);
						flag = true;
					}
				}
			}
			if (!flag2 && m_inventoryClickStyle == eInventoryClickStyle.OnMouseClick && clickable.ClickableType == eQuestClickableType.Inventory)
			{
				m_lastClickable = clickable;
				OnInteraction(m_lastClickable, verb);
				if (verb == eQuestVerb.Inventory && m_player.HasActiveInventory)
				{
					flag2 = StartScriptInteraction(m_lastClickable.GetScriptable(), SCRIPT_FUNCTION_USEINV_INVENTORY, new object[2]
					{
						clickable as IInventory,
						m_player.ActiveInventory
					}, stopPlayerMoving: true);
					if (!flag2 && m_player.HasActiveInventory)
					{
						flag2 = StartScriptInteraction(m_player.ActiveInventory as IQuestScriptable, SCRIPT_FUNCTION_USEINV_INVENTORY, new object[2]
						{
							m_player.ActiveInventory,
							clickable as IInventory
						}, stopPlayerMoving: true);
					}
				}
				else
				{
					flag2 = StartScriptInteraction(m_lastClickable.GetScriptable(), (verb != eQuestVerb.Look) ? SCRIPT_FUNCTION_INTERACT_INVENTORY : SCRIPT_FUNCTION_LOOKAT_INVENTORY, new object[1] { clickable as IInventory }, stopPlayerMoving: true);
				}
				if (flag2)
				{
					m_queuedScriptInteractions.Add(m_currentSequence);
					flag = true;
				}
			}
			if (clickable != null && !flag2 && m_globalScript != null)
			{
				string text = "";
				object[] array = null;
				bool stopPlayerMoving = true;
				if (verb == eQuestVerb.Inventory && m_player.HasActiveInventory)
				{
					if (clickable.ClickableType == eQuestClickableType.Inventory)
					{
						text = "UnhandledUseInvInv";
						IInventory inventory = clickable as IInventory;
						IInventory activeInventory = m_player.ActiveInventory;
						if (!flag2)
						{
							flag2 = StartScriptInteraction(this, text, new object[2] { inventory, activeInventory }, stopPlayerMoving);
						}
						if (!flag2)
						{
							flag2 = StartScriptInteraction(this, text, new object[2] { activeInventory, inventory }, stopPlayerMoving);
						}
						if (!flag2)
						{
							text = "UnhandledUseInv";
						}
					}
					else
					{
						text = "UnhandledUseInv";
					}
					array = new object[2] { clickable, m_player.ActiveInventory };
				}
				else
				{
					text = ((verb != eQuestVerb.Look) ? "UnhandledInteract" : "UnhandledLookAt");
					array = new object[1] { clickable };
					if (clickable.ClickableType == eQuestClickableType.Inventory)
					{
						stopPlayerMoving = false;
					}
				}
				if (!flag2)
				{
					flag2 = StartScriptInteraction(m_currentRoom.GetScriptable(), text, array, stopPlayerMoving);
				}
				if (!flag2)
				{
					flag2 = StartScriptInteraction(this, text, array, stopPlayerMoving);
				}
				if (flag2)
				{
					m_queuedScriptInteractions.Add(m_currentSequence);
					flag = true;
				}
			}
			if (CallbackOnProcessClick != null)
			{
				CallbackOnProcessClick(flag);
			}
			return flag;
		}

		public Coroutine HandleInteract(IHotspot target)
		{
			OnHandleInteraction(target.IClickable, eQuestVerb.Use);
			return StartScriptInteractionCoroutine(target.IClickable.GetScript(), SCRIPT_FUNCTION_INTERACT_HOTSPOT + target.ScriptName, new object[1] { target }, stopPlayerMoving: true);
		}

		public Coroutine HandleLookAt(IHotspot target)
		{
			OnHandleInteraction(target.IClickable, eQuestVerb.Look);
			return StartScriptInteractionCoroutine(target.IClickable.GetScript(), SCRIPT_FUNCTION_LOOKAT_HOTSPOT + target.ScriptName, new object[1] { target }, stopPlayerMoving: true);
		}

		public Coroutine HandleInventory(IHotspot target, IInventory item)
		{
			OnHandleInteraction(target.IClickable, eQuestVerb.Inventory);
			return StartScriptInteractionCoroutine(target.IClickable.GetScript(), SCRIPT_FUNCTION_USEINV_HOTSPOT + target.ScriptName, new object[2] { target, item }, stopPlayerMoving: true);
		}

		public Coroutine HandleInteract(IProp target)
		{
			OnHandleInteraction(target.IClickable, eQuestVerb.Use);
			return StartScriptInteractionCoroutine(target.IClickable.GetScript(), SCRIPT_FUNCTION_INTERACT_PROP + target.ScriptName, new object[1] { target }, stopPlayerMoving: true);
		}

		public Coroutine HandleLookAt(IProp target)
		{
			OnHandleInteraction(target.IClickable, eQuestVerb.Look);
			return StartScriptInteractionCoroutine(target.IClickable.GetScript(), SCRIPT_FUNCTION_LOOKAT_PROP + target.ScriptName, new object[1] { target }, stopPlayerMoving: true);
		}

		public Coroutine HandleInventory(IProp target, IInventory item)
		{
			OnHandleInteraction(target.IClickable, eQuestVerb.Inventory);
			return StartScriptInteractionCoroutine(target.IClickable.GetScript(), SCRIPT_FUNCTION_USEINV_PROP + target.ScriptName, new object[2] { target, item }, stopPlayerMoving: true);
		}

		public Coroutine HandleInteract(ICharacter target)
		{
			OnHandleInteraction(target.IClickable, eQuestVerb.Use);
			Coroutine coroutine = StartScriptInteractionCoroutine(m_currentRoom.GetScriptable().GetScript(), SCRIPT_FUNCTION_INTERACT_CHARACTER + target.ScriptName, new object[1] { target }, stopPlayerMoving: true);
			if (coroutine != null)
			{
				return coroutine;
			}
			return StartScriptInteractionCoroutine(target.IClickable.GetScript(), SCRIPT_FUNCTION_INTERACT, null, stopPlayerMoving: true);
		}

		public Coroutine HandleLookAt(ICharacter target)
		{
			OnHandleInteraction(target.IClickable, eQuestVerb.Look);
			Coroutine coroutine = StartScriptInteractionCoroutine(m_currentRoom.GetScriptable().GetScript(), SCRIPT_FUNCTION_LOOKAT_CHARACTER + target.ScriptName, new object[1] { target }, stopPlayerMoving: true);
			if (coroutine != null)
			{
				return coroutine;
			}
			return StartScriptInteractionCoroutine(target.IClickable.GetScript(), SCRIPT_FUNCTION_LOOKAT, null, stopPlayerMoving: true);
		}

		public Coroutine HandleInventory(ICharacter target, IInventory item)
		{
			OnHandleInteraction(target.IClickable, eQuestVerb.Inventory);
			Coroutine coroutine = StartScriptInteractionCoroutine(m_currentRoom.GetScriptable().GetScript(), SCRIPT_FUNCTION_USEINV_CHARACTER + target.ScriptName, new object[2] { target, item }, stopPlayerMoving: true);
			if (coroutine != null)
			{
				return coroutine;
			}
			return StartScriptInteractionCoroutine(target.IClickable.GetScript(), SCRIPT_FUNCTION_USEINV, new object[1] { item }, stopPlayerMoving: true);
		}

		public Coroutine HandleInteract(IInventory target)
		{
			return StartScriptInteractionCoroutine(target.Data.GetScript(), SCRIPT_FUNCTION_INTERACT_INVENTORY, new object[1] { target }, stopPlayerMoving: true);
		}

		public Coroutine HandleLookAt(IInventory target)
		{
			return StartScriptInteractionCoroutine(target.Data.GetScript(), SCRIPT_FUNCTION_LOOKAT_INVENTORY, new object[1] { target }, stopPlayerMoving: true);
		}

		public Coroutine HandleInventory(IInventory target, IInventory item)
		{
			return StartScriptInteractionCoroutine(target.Data.GetScript(), SCRIPT_FUNCTION_USEINV_INVENTORY, new object[2] { target, item }, stopPlayerMoving: true);
		}

		public Coroutine HandleOption(IDialogTree dialog, string optionName)
		{
			DialogOption dialogOption = (DialogOption)dialog.GetOption(optionName);
			bool used = dialogOption.Used;
			dialogOption.Used = true;
			dialogOption.TimesUsed++;
			Coroutine coroutine = StartScriptInteractionCoroutine(dialog.Data.GetScript(), SCRIPT_FUNCTION_DIALOG_OPTION + dialogOption.Name, new object[1] { dialogOption });
			if (coroutine == null)
			{
				coroutine = StartScriptInteractionCoroutine(dialog.Data.GetScript(), SCRIPT_FUNCTION_DIALOG_OPTION + dialogOption.Name);
			}
			if (coroutine != null)
			{
				GetGui(DialogTreeGui).Visible = false;
			}
			else
			{
				dialogOption.Used = used;
				dialogOption.TimesUsed--;
			}
			return coroutine;
		}

		public Vector2 WorldPositionToGui(Vector2 position)
		{
			return m_cameraGui.ViewportToWorldPoint(m_cameraData.Camera.WorldToViewportPoint(position));
		}

		public Vector2 GuiPositionToWorld(Vector2 position)
		{
			return m_cameraData.Camera.ViewportToWorldPoint(m_cameraGui.WorldToViewportPoint(position));
		}

		public bool GetCanCancel()
		{
			return m_sequenceIsCancelable;
		}

		public void EnableCancel()
		{
			if (!m_sequenceIsCancelable && m_allowEnableCancel)
			{
				EnableCancelInternal();
				StopCoroutine(m_coroutineMainLoop);
				m_coroutineMainLoop = StartCoroutine(MainLoop());
			}
		}

		public void DisableCancel()
		{
			if (m_sequenceIsCancelable && m_backgroundSequence != null)
			{
				m_currentSequence = m_backgroundSequence;
				m_backgroundSequence = null;
				m_currentSequences = m_backgroundSequences;
				m_backgroundSequences.Clear();
			}
			m_sequenceIsCancelable = false;
			m_allowEnableCancel = false;
		}

		public void CancelCurrentInteraction()
		{
			if (!m_sequenceIsCancelable || m_backgroundSequence == null)
			{
				return;
			}
			for (int i = 0; i < m_currentInteractionClickables.Count; i++)
			{
				m_currentInteractionClickables[i]?.OnCancelInteraction(m_currentInteractionVerbs[i]);
			}
			m_currentInteractionClickables.Clear();
			m_currentInteractionVerbs.Clear();
			SV.m_currentInteractionOccurrences.ForEach(delegate(string occurrence)
			{
				SV.m_occurrences.Remove(occurrence);
			});
			SV.m_currentInteractionOccurrences.Clear();
			StopCoroutine(m_backgroundSequence);
			m_backgroundSequence = null;
			foreach (Coroutine backgroundSequence in m_backgroundSequences)
			{
				if (backgroundSequence != null)
				{
					StopCoroutine(backgroundSequence);
				}
			}
			m_backgroundSequences.Clear();
			m_currentSequence = null;
			m_currentSequences.Clear();
			m_sequenceIsCancelable = false;
		}

		public bool FirstOccurrence(string uniqueString)
		{
			if (m_allowEnableCancel)
			{
				SV.m_currentInteractionOccurrences.Add(uniqueString);
			}
			return SV.m_occurrences.Add(uniqueString) <= 1;
		}

		public int GetOccurrenceCount(string thing)
		{
			return SV.m_occurrences.Count(thing);
		}

		public int Occurrence(string thing)
		{
			if (m_allowEnableCancel)
			{
				SV.m_currentInteractionOccurrences.Add(thing);
			}
			return SV.m_occurrences.Add(thing) - 1;
		}

		public void DisableAllClickablesExcept()
		{
			RestoreAllClickables();
			foreach (Prop prop in Singleton<PowerQuest>.Get.GetCurrentRoom().GetProps())
			{
				if (prop.Clickable)
				{
					SV.m_tempDisabledProps.Add(prop.ScriptName);
					prop.Clickable = false;
				}
			}
			foreach (Hotspot hotspot in Singleton<PowerQuest>.Get.GetCurrentRoom().GetHotspots())
			{
				if (hotspot.Clickable)
				{
					SV.m_tempDisabledHotspots.Add(hotspot.ScriptName);
					hotspot.Clickable = false;
				}
			}
		}

		public void DisableAllClickablesExcept(params string[] exceptions)
		{
			RestoreAllClickables();
			foreach (Prop prop in Singleton<PowerQuest>.Get.GetCurrentRoom().GetProps())
			{
				if (prop.Clickable && !Array.Exists(exceptions, (string item) => string.Equals(prop.ScriptName, item, StringComparison.OrdinalIgnoreCase)))
				{
					SV.m_tempDisabledProps.Add(prop.ScriptName);
					prop.Clickable = false;
				}
			}
			foreach (Hotspot hotspot in Singleton<PowerQuest>.Get.GetCurrentRoom().GetHotspots())
			{
				if (hotspot.Clickable && !Array.Exists(exceptions, (string item) => string.Equals(hotspot.ScriptName, item, StringComparison.OrdinalIgnoreCase)))
				{
					SV.m_tempDisabledHotspots.Add(hotspot.ScriptName);
					hotspot.Clickable = false;
				}
			}
		}

		public void DisableAllClickablesExcept(params IQuestClickableInterface[] exceptions)
		{
			RestoreAllClickables();
			foreach (Prop prop in Singleton<PowerQuest>.Get.GetCurrentRoom().GetProps())
			{
				if (prop.Clickable && !Array.Exists(exceptions, (IQuestClickableInterface item) => item == prop))
				{
					SV.m_tempDisabledProps.Add(prop.ScriptName);
					prop.Clickable = false;
				}
			}
			foreach (Hotspot hotspot in Singleton<PowerQuest>.Get.GetCurrentRoom().GetHotspots())
			{
				if (hotspot.Clickable && !Array.Exists(exceptions, (IQuestClickableInterface item) => item == hotspot))
				{
					SV.m_tempDisabledHotspots.Add(hotspot.ScriptName);
					hotspot.Clickable = false;
				}
			}
		}

		public void RestoreAllClickables()
		{
			foreach (string tempDisabledProp in SV.m_tempDisabledProps)
			{
				Prop prop = Singleton<PowerQuest>.Get.GetCurrentRoom().GetProp(tempDisabledProp);
				if (prop != null)
				{
					prop.Clickable = true;
				}
			}
			SV.m_tempDisabledProps.Clear();
			foreach (string tempDisabledHotspot in SV.m_tempDisabledHotspots)
			{
				Hotspot hotspot = Singleton<PowerQuest>.Get.GetCurrentRoom().GetHotspot(tempDisabledHotspot);
				if (hotspot != null)
				{
					hotspot.Clickable = true;
				}
			}
			SV.m_tempDisabledHotspots.Clear();
		}

		public void SetAllClickableCursors(string cursor, params string[] exceptions)
		{
			foreach (Prop prop in Singleton<PowerQuest>.Get.GetCurrentRoom().GetProps())
			{
				if (prop.Clickable && prop.Cursor != cursor && !Array.Exists(exceptions, (string item) => item == prop.ScriptName))
				{
					SV.m_tempCursorNoneCursor.Add(prop.Cursor);
					SV.m_tempCursorNoneProps.Add(prop.ScriptName);
					prop.Cursor = cursor;
				}
			}
			foreach (Hotspot hotspot in Singleton<PowerQuest>.Get.GetCurrentRoom().GetHotspots())
			{
				if (hotspot.Clickable && hotspot.Cursor != cursor && !Array.Exists(exceptions, (string item) => item == hotspot.ScriptName))
				{
					SV.m_tempCursorNoneCursor.Add(hotspot.Cursor);
					SV.m_tempCursorNoneHotspots.Add(hotspot.ScriptName);
					hotspot.Cursor = cursor;
				}
			}
		}

		public void RestoreAllClickableCursors()
		{
			int num = 0;
			foreach (string tempCursorNoneProp in SV.m_tempCursorNoneProps)
			{
				Prop prop = Singleton<PowerQuest>.Get.GetCurrentRoom().GetProp(tempCursorNoneProp);
				if (prop != null)
				{
					prop.Cursor = SV.m_tempCursorNoneCursor[num];
				}
				num++;
			}
			SV.m_tempCursorNoneProps.Clear();
			foreach (string tempCursorNoneHotspot in SV.m_tempCursorNoneHotspots)
			{
				Hotspot hotspot = Singleton<PowerQuest>.Get.GetCurrentRoom().GetHotspot(tempCursorNoneHotspot);
				if (hotspot != null)
				{
					hotspot.Cursor = SV.m_tempCursorNoneCursor[num];
				}
				num++;
			}
			SV.m_tempCursorNoneHotspots.Clear();
			SV.m_tempCursorNoneCursor.Clear();
		}

		public Coroutine WaitForInlineDialog(params string[] options)
		{
			if (options != null && options.Length != 0)
			{
				return StartQuestCoroutine(CoroutineWaitForInlineDialog(options));
			}
			return null;
		}

		private IEnumerator CoroutineWaitForInlineDialog(string[] options)
		{
			bool wasCutscene = m_cutscene;
			if (wasCutscene)
			{
				EndCutscene();
			}
			m_inlineDialogResult = -1;
			m_inlineDialogPrevDialog = m_currentDialog;
			DialogTree dialogTree = new DialogTree();
			int num = 0;
			foreach (string text in options)
			{
				DialogOption dialogOption = new DialogOption();
				dialogOption.InlineId = num;
				dialogOption.Text = text;
				dialogTree.Options.Add(dialogOption);
				num++;
			}
			m_currentDialog = dialogTree;
			GetGui(DialogTreeGui).Visible = true;
			bool hideCursor = GetCursor().HideWhenBlocking;
			GetCursor().HideWhenBlocking = false;
			GetCursor().Visible = true;
			yield return WaitWhile(() => m_inlineDialogResult < 0);
			GetCursor().HideWhenBlocking = hideCursor;
			m_currentDialog = m_inlineDialogPrevDialog;
			if (wasCutscene)
			{
				StartCutscene();
			}
			yield return Break;
		}

		public string GetScriptName()
		{
			return "PowerQuest";
		}

		public string GetScriptClassName()
		{
			return GLOBAL_SCRIPT_NAME;
		}

		public QuestScript GetScript()
		{
			return m_globalScript;
		}

		public IQuestScriptable GetScriptable()
		{
			return this;
		}

		public void HotLoadScript(Assembly assembly)
		{
			QuestUtils.HotSwapScript(ref m_globalScript, GLOBAL_SCRIPT_NAME, assembly);
		}

		public void EditorRename(string name)
		{
		}

		public bool GetSerializationComplete()
		{
			return m_serializeComplete;
		}

		public void OnBeforeSerialize()
		{
			m_serializeComplete = false;
		}

		public void OnAfterDeserialize()
		{
			m_serializeComplete = true;
		}

		public bool GetShouldSayTextAutoAdvance()
		{
			return m_sayTextAutoAdvance;
		}

		public bool GetStopWalkingToTalk()
		{
			return m_stopWalkingToTalk;
		}

		public Coroutine WaitForDialog(float time, AudioHandle audioSource, bool autoAdvance, bool skippable, QuestText textComponent = null)
		{
			return StartCoroutine(CoroutineWaitForDialog(time, skippable, autoAdvance, audioSource, textComponent));
		}

		public void CancelDisplayBG()
		{
			EndDisplay();
		}

		public bool GetBlocked()
		{
			if (!m_blocking || m_sequenceIsCancelable)
			{
				return m_transitioning;
			}
			return true;
		}

		public void InterruptNextLine(float bySeconds)
		{
			m_interruptNextLine = true;
			m_interruptNextLineTime = bySeconds;
		}

		public void ResetInterruptNextLine()
		{
			m_interruptNextLine = false;
		}

		public void SkipDialog(bool useNoSkipTime = true)
		{
			if (!useNoSkipTime || Time.timeSinceLevelLoad - m_timeLastTextShown > m_textNoSkipTime)
			{
				m_skipDialog = true;
			}
		}

		public bool SkipCutscene()
		{
			if (m_skipCutscene)
			{
				return true;
			}
			if (m_cutscene)
			{
				SystemAudio.Play("SkipCutscene");
				FadeOutBG(0f, "CUTSCENE");
				foreach (Character character in m_characters)
				{
					if (character.Instance != null)
					{
						(character.Instance as CharacterComponent).OnSkipCutscene();
					}
				}
				m_skipCutscene = true;
			}
			return m_cutscene;
		}

		public bool GetSkippingCutscene()
		{
			return m_skipCutscene;
		}

		public bool HandleSkipDialogKeyPressed()
		{
			bool skipDialog = m_skipDialog;
			m_skipDialog = false;
			m_leftClickPrev = true;
			m_rightClickPrev = true;
			if (CallbackOnDialogSkipped != null)
			{
				CallbackOnDialogSkipped();
			}
			return skipDialog;
		}

		public QuestCamera GetCamera()
		{
			return m_cameraData;
		}

		public QuestCursor GetCursor()
		{
			return m_cursor;
		}

		public List<Character> GetCharacters()
		{
			return m_characters;
		}

		public Character GetCharacter(int id)
		{
			List<Character> characters = m_characters;
			if (id >= 0 && id < characters.Count)
			{
				return GetSavable(characters[id]);
			}
			return null;
		}

		public int GetCharacterId(Character character)
		{
			return m_characters.FindIndex((Character ch) => ch == character);
		}

		public Inventory GetInventory(int id)
		{
			List<Inventory> inventoryItems = m_inventoryItems;
			if (id >= 0 && id < inventoryItems.Count)
			{
				return GetSavable(inventoryItems[id]);
			}
			return null;
		}

		public List<DialogTree> GetDialogTrees()
		{
			return m_dialogTrees;
		}

		public DialogTree GetDialogTree(int id)
		{
			List<DialogTree> dialogTrees = m_dialogTrees;
			if (id >= 0 && id < dialogTrees.Count)
			{
				return GetSavable(dialogTrees[id]);
			}
			return null;
		}

		public Gui GetGui(int id)
		{
			List<Gui> guis = m_guis;
			if (id >= 0 && id < guis.Count)
			{
				return guis[id];
			}
			return null;
		}

		public QuestText GetDialogTextPrefab()
		{
			return m_dialogTextPrefab;
		}

		public Vector2 GetDialogTextOffset()
		{
			return m_dialogTextOffset;
		}

		public void SetMouseOverDescriptionOverride(string description)
		{
			m_mouseOverDescriptionOverride = description;
		}

		public void ResetMouseOverDescriptionOverride()
		{
			m_mouseOverDescriptionOverride = null;
		}

		public float GetTextDisplayTime(string text)
		{
			return Mathf.Max(TEXT_DISPLAY_TIME_MIN, (float)text.Length * TEXT_DISPLAY_TIME_CHARACTER) * Settings.TextSpeedMultiplier;
		}

		public QuestScript GetGlobalScript()
		{
			return m_globalScript;
		}

		public bool GetRoomLoading()
		{
			return !m_levelLoadedCalled;
		}

		public void StartRoomTransition(Room room, bool force = false)
		{
			if (!m_levelLoadedCalled)
			{
				Debug.LogError("Attempted to change rooms while already changing rooms!");
			}
			else if (m_initialised)
			{
				m_levelLoadedCalled = false;
				CancelCurrentInteraction();
				m_backgroundSequence = null;
				if (room != null && (SceneManager.GetActiveScene().name != room.GetSceneName() || force))
				{
					StartCoroutine(CoroutineRoomTransition(room, force));
				}
			}
		}

		public void StartDialog(string dialogName)
		{
			DialogTree dialogTree = GetDialogTree(dialogName);
			if (dialogTree == null)
			{
				Debug.LogWarning("Couldn't start Dialog: " + dialogName + ", it doesn't exist!");
				return;
			}
			dialogTree.OnStart();
			m_previousDialog = m_currentDialog;
			m_currentDialog = dialogTree;
			if (!StartScriptInteraction(dialogTree, SCRIPT_FUNCTION_DIALOG_START))
			{
				StartScriptInteraction(dialogTree, SCRIPT_FUNCTION_DIALOG_START_OLD);
			}
		}

		public void StopDialog()
		{
			if (m_currentDialog != null)
			{
				StartScriptInteraction(m_currentDialog, SCRIPT_FUNCTION_DIALOG_STOP);
				m_currentDialog = null;
			}
			GetGui(DialogTreeGui).Visible = false;
		}

		public void SetDialogOptionSelected(DialogOption option)
		{
			m_dialogOptionSelected = option;
		}

		public bool OnDialogOptionClick(DialogOption option)
		{
			if (m_currentDialog == null)
			{
				return false;
			}
			m_guiConsumedClick = true;
			if (option == null)
			{
				return false;
			}
			if (option.InlineId >= 0)
			{
				m_inlineDialogResult = option.InlineId;
				option.Used = true;
				GetGui(DialogTreeGui).Visible = false;
				StopDialog();
				return true;
			}
			bool used = option.Used;
			option.Used = true;
			option.TimesUsed++;
			if (StartScriptInteraction(m_currentDialog, SCRIPT_FUNCTION_DIALOG_OPTION + option.Name, new object[1] { option }) || StartScriptInteraction(m_currentDialog, SCRIPT_FUNCTION_DIALOG_OPTION + option.Name))
			{
				GetGui(DialogTreeGui).Visible = false;
				return true;
			}
			option.Used = used;
			option.TimesUsed--;
			return false;
		}

		public bool OnInventoryClick()
		{
			if (Paused || GetModalGuiActive())
			{
				if (m_inventoryClickStyle != eInventoryClickStyle.OnMouseClick)
				{
					Debug.LogWarning("InventoryClickStyle should be set to OnMouseClick, other modes are no longer in use");
				}
				MethodInfo methodInfo = null;
				if (m_globalScript != null)
				{
					methodInfo = m_globalScript.GetType().GetMethod("OnMouseClick", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (methodInfo != null)
					{
						methodInfo.Invoke(m_globalScript, new object[2]
						{
							Input.GetMouseButton(0),
							Input.GetMouseButton(1)
						});
					}
					if (m_queuedScriptInteractions.Count > 0)
					{
						m_leftClickPrev = Input.GetMouseButton(0);
						m_rightClickPrev = Input.GetMouseButton(1);
						return true;
					}
				}
			}
			return false;
		}

		public bool OnInventoryClick(string item, PointerEventData.InputButton button)
		{
			if (m_inventoryClickStyle == eInventoryClickStyle.OnMouseClick)
			{
				if (Paused)
				{
					MethodInfo methodInfo = null;
					if (m_globalScript != null)
					{
						methodInfo = m_globalScript.GetType().GetMethod(SCRIPT_FUNCTION_ONMOUSECLICK, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (methodInfo != null)
						{
							methodInfo.Invoke(m_globalScript, new object[2]
							{
								button == PointerEventData.InputButton.Left,
								button == PointerEventData.InputButton.Right
							});
						}
						if (m_queuedScriptInteractions.Count > 0)
						{
							return true;
						}
					}
				}
				return false;
			}
			bool flag = false;
			bool result = false;
			m_guiConsumedClick = true;
			Inventory inventory = GetInventory(item);
			if (inventory == null)
			{
				return false;
			}
			if (inventory == m_player.ActiveInventory)
			{
				m_player.ActiveInventory = null;
				return false;
			}
			switch (button)
			{
			case PointerEventData.InputButton.Left:
				if (m_player.HasActiveInventory)
				{
					Inventory data = m_player.ActiveInventory.Data;
					flag = StartScriptInteraction(inventory, SCRIPT_FUNCTION_USEINV_INVENTORY, new object[2] { inventory, data }, stopPlayerMoving: true);
					if (!flag)
					{
						flag = StartScriptInteraction(data, SCRIPT_FUNCTION_USEINV_INVENTORY, new object[2] { data, inventory }, stopPlayerMoving: true);
					}
					bool flag2 = flag;
					if (!flag)
					{
						flag = StartScriptInteraction(this, "UnhandledUseInvInv", new object[2] { inventory, data }, stopPlayerMoving: true);
						if (!flag)
						{
							flag = StartScriptInteraction(this, "UnhandledUseInvInv", new object[2] { data, inventory }, stopPlayerMoving: true);
						}
					}
					if (flag)
					{
						result = true;
						if (flag2 && !m_player.HasInventory(m_player.ActiveInventory))
						{
							m_player.ActiveInventory = null;
						}
					}
				}
				else if (m_inventoryClickStyle == eInventoryClickStyle.UseInventory)
				{
					flag = StartScriptInteraction(inventory, SCRIPT_FUNCTION_INTERACT_INVENTORY, new object[1] { inventory }, stopPlayerMoving: true);
					if (!flag)
					{
						flag = StartScriptInteraction(this, "UnhandledInteractInventory", new object[1] { inventory }, stopPlayerMoving: true);
					}
					if (flag)
					{
						result = true;
					}
				}
				else
				{
					m_player.ActiveInventory = inventory;
					SystemAudio.Play("InventoryCursorSet");
				}
				break;
			case PointerEventData.InputButton.Right:
				if (m_player.HasActiveInventory)
				{
					m_player.ActiveInventory = null;
					break;
				}
				flag = StartScriptInteraction(inventory, SCRIPT_FUNCTION_LOOKAT_INVENTORY, new object[1] { inventory }, stopPlayerMoving: true);
				if (!flag)
				{
					flag = StartScriptInteraction(this, "UnhandledLookAtInv", new object[1] { inventory }, stopPlayerMoving: true);
				}
				if (flag)
				{
					result = true;
				}
				break;
			}
			return result;
		}

		public bool GetRestoringGame()
		{
			return m_restoring;
		}

		public Coroutine StartQuestCoroutine(IEnumerator routine, bool cancelable = false)
		{
			Coroutine coroutine = StartCoroutine(routine);
			if (cancelable)
			{
				if (m_sequenceIsCancelable)
				{
					m_backgroundSequences.Add(coroutine);
				}
				else
				{
					m_currentSequences.Add(coroutine);
				}
			}
			return coroutine;
		}

		public Coroutine QueueCoroutine(IEnumerator routine)
		{
			Coroutine coroutine = StartQuestCoroutine(routine);
			m_queuedScriptInteractions.Add(coroutine);
			return coroutine;
		}

		public static bool GetDebugKeyHeld()
		{
			if (Singleton<PowerQuest>.Get.IsDebugBuild)
			{
				if (!Input.GetKey(KeyCode.BackQuote))
				{
					return Input.GetKey(KeyCode.Backslash);
				}
				return true;
			}
			return false;
		}

		public bool GetActionEnabled(eQuestVerb action)
		{
			return action switch
			{
				eQuestVerb.Use => m_enableUse, 
				eQuestVerb.Look => m_enableLook, 
				eQuestVerb.Inventory => m_enableInventory, 
				_ => true, 
			};
		}

		public bool GetSnapToPixel()
		{
			return m_snapToPixel;
		}

		public bool GetPixelCamEnabled()
		{
			if (m_snapToPixel)
			{
				return m_pixelCamEnabled;
			}
			return false;
		}

		public float EditorGetDefaultPixelsPerUnit()
		{
			return m_defaultPixelsPerUnit;
		}

		public List<RoomComponent> GetRoomPrefabs()
		{
			return m_roomPrefabs;
		}

		public List<CharacterComponent> GetCharacterPrefabs()
		{
			return m_characterPrefabs;
		}

		public List<InventoryComponent> GetInventoryPrefabs()
		{
			return m_inventoryPrefabs;
		}

		public List<DialogTreeComponent> GetDialogTreePrefabs()
		{
			return m_dialogTreePrefabs;
		}

		public List<GuiComponent> GetGuiPrefabs()
		{
			return m_guiPrefabs;
		}

		public List<Gui> GetGuis()
		{
			return m_guis;
		}

		public QuestCursorComponent GetCursorPrefab()
		{
			return m_cursorPrefab;
		}

		public QuestCameraComponent GetCameraPrefab()
		{
			return m_cameraPrefab;
		}

		public QuestText GetDialogTextPrefabEditor()
		{
			return m_dialogTextPrefab;
		}

		public List<AnimationClip> GetInventoryAnimations()
		{
			return m_inventoryAnimations;
		}

		public AnimationClip GetInventoryAnimation(string animName)
		{
			return QuestUtils.FindByName(m_inventoryAnimations, animName);
		}

		public List<Sprite> GetInventorySprites()
		{
			return m_inventorySprites;
		}

		public Sprite GetInventorySprite(string animName)
		{
			return FindSpriteInList(m_inventorySprites, animName);
		}

		public List<AnimationClip> GetGuiAnimations()
		{
			return m_guiAnimations;
		}

		public AnimationClip GetGuiAnimation(string animName)
		{
			return QuestUtils.FindByName(m_guiAnimations, animName);
		}

		public List<Sprite> GetGuiSprites()
		{
			return m_guiSprites;
		}

		public Sprite GetGuiSprite(string animName)
		{
			return FindSpriteInList(m_guiSprites, animName);
		}

		public static Sprite FindSpriteInList(List<Sprite> list, string animName)
		{
			if (list == null)
			{
				return null;
			}
			string animName_0 = animName + SPRITE_NUM_POSTFIX_0;
			return list.Find((Sprite item) => item != null && (string.Equals(animName, item.name, StringComparison.OrdinalIgnoreCase) || string.Equals(animName_0, item.name, StringComparison.OrdinalIgnoreCase)));
		}

		public List<IQuestScriptable> GetAllScriptables()
		{
			List<IQuestScriptable> scriptables = new List<IQuestScriptable>();
			scriptables.Add(this);
			m_characters.ForEach(delegate(Character item)
			{
				scriptables.Add(item);
			});
			m_rooms.ForEach(delegate(Room item)
			{
				scriptables.Add(item);
			});
			m_dialogTrees.ForEach(delegate(DialogTree item)
			{
				scriptables.Add(item);
			});
			m_inventoryItems.ForEach(delegate(Inventory item)
			{
				scriptables.Add(item);
			});
			m_guis.ForEach(delegate(Gui item)
			{
				scriptables.Add(item);
			});
			return scriptables;
		}

		public bool GetModalGuiActive()
		{
			if (!m_guis.Exists((Gui item) => item.Modal && item.Visible))
			{
				return m_guiConsumedClick;
			}
			return true;
		}

		public Gui GetTopModalGui()
		{
			Gui gui = null;
			foreach (Gui gui2 in m_guis)
			{
				if (gui2.Modal && gui2.Visible && (gui == null || gui2.Baseline < gui.Baseline))
				{
					gui = gui2;
				}
			}
			return gui;
		}

		public void CaptureInputOn(string source)
		{
			SV.m_captureInputSources.Add(source);
			Gui gui = GetGuis().Find((Gui item) => item.ScriptName == "Source");
			if (gui != null)
			{
				m_mouseOverClickable = gui;
			}
		}

		public void CaptureInputOff(string source)
		{
			SV.m_captureInputSources.RemoveAll((string item) => string.Equals(item, source, StringComparison.OrdinalIgnoreCase));
		}

		public int EditorGetVersion()
		{
			return m_version;
		}

		public void EditorSetVersion(int version)
		{
			m_version = version;
		}

		public IQuestScriptable EditorGetAutoLoadScriptable()
		{
			return m_autoLoadScriptable;
		}

		public void EditorSetHotLoadAssembly(Assembly assembly)
		{
			m_hotLoadAssembly = assembly;
		}

		public Assembly EditorGetHotLoadAssembly()
		{
			return m_hotLoadAssembly;
		}

		public string EditorGetAutoLoadFunction()
		{
			return m_autoLoadFunction;
		}

		public bool GetRegainedFocus()
		{
			int num;
			if (m_lostFocus)
			{
				num = (m_hasFocus ? 1 : 0);
				if (num != 0)
				{
					m_lostFocus = false;
				}
			}
			else
			{
				num = 0;
			}
			return (byte)num != 0;
		}

		public bool GetLostFocus()
		{
			return m_lostFocus;
		}

		public void SetLostFocus(bool value)
		{
			m_lostFocus = value;
		}

		private void OnApplicationFocus(bool hasfocus)
		{
			m_hasFocus = hasfocus;
			if (hasfocus)
			{
				if (!Application.isEditor)
				{
					UnityEngine.Cursor.visible = false;
				}
			}
			else
			{
				m_lostFocus = true;
			}
		}

		private void OnEnable()
		{
			SpriteAtlasManager.atlasRequested += RequestAtlas;
		}

		private void OnDisable()
		{
			SpriteAtlasManager.atlasRequested -= RequestAtlas;
		}

		private void RequestAtlas(string tag, Action<SpriteAtlas> callback)
		{
			s_roomAtlasCallbacks.Add(tag, callback);
			if (m_currentRoom != null && tag.Equals("Room" + m_currentRoom.ScriptName + "Atlas"))
			{
				LoadAtlas(m_currentRoom.ScriptName);
			}
		}

		private void Awake()
		{
			if (Singleton<PowerQuest>.HasInstance())
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			SetSingleton();
			UnityEngine.Object.DontDestroyOnLoad(this);
			if (LAYER_UI < 0)
			{
				LAYER_UI = LayerMask.NameToLayer("UI");
			}
			m_menuManager.Awake();
			m_characterPrefabs.RemoveAll((CharacterComponent item) => item == null);
			m_roomPrefabs.RemoveAll((RoomComponent item) => item == null);
			m_dialogTreePrefabs.RemoveAll((DialogTreeComponent item) => item == null);
			m_guiPrefabs.RemoveAll((GuiComponent item) => item == null);
			m_inventoryPrefabs.RemoveAll((InventoryComponent item) => item == null);
			if (SingletonAuto<SystemAudio>.HasInstance())
			{
				UnityEngine.Object.Destroy(SingletonAuto<SystemAudio>.Get.gameObject);
			}
			foreach (Component system in m_systems)
			{
				Transform obj = UnityEngine.Object.Instantiate(system.gameObject).transform;
				obj.name = system.name;
				obj.parent = base.transform;
			}
			if (!string.IsNullOrEmpty(Settings.Language))
			{
				Settings.Language = Settings.Language;
			}
			m_globalScript = QuestUtils.ConstructByName<QuestScript>(GLOBAL_SCRIPT_NAME);
			if (m_cursorPrefab != null)
			{
				m_cursor = new QuestCursor();
				QuestUtils.CopyFields(m_cursor, m_cursorPrefab.GetData());
				m_cursor.Initialise(m_cursorPrefab.gameObject);
				UnityEngine.Cursor.visible = false;
				if (!Application.isEditor)
				{
					UnityEngine.Cursor.lockState = CursorLockMode.Confined;
				}
			}
			if (m_cameraPrefab != null)
			{
				QuestUtils.CopyFields(m_cameraData, m_cameraPrefab.GetData());
			}
			foreach (RoomComponent roomPrefab in m_roomPrefabs)
			{
				Room room = new Room();
				QuestUtils.CopyFields(room, roomPrefab.GetData());
				m_rooms.Add(room);
				room.Initialise(roomPrefab.gameObject);
			}
			foreach (InventoryComponent inventoryPrefab in m_inventoryPrefabs)
			{
				Inventory inventory = new Inventory();
				QuestUtils.CopyFields(inventory, inventoryPrefab.GetData());
				m_inventoryItems.Add(inventory);
				inventory.Initialise(inventoryPrefab.gameObject);
			}
			foreach (CharacterComponent characterPrefab in m_characterPrefabs)
			{
				Character character = new Character();
				QuestUtils.CopyFields(character, characterPrefab.GetData());
				m_characters.Add(character);
				character.Initialise(characterPrefab.gameObject);
			}
			foreach (DialogTreeComponent dialogTreePrefab in m_dialogTreePrefabs)
			{
				DialogTree dialogTree = new DialogTree();
				QuestUtils.CopyFields(dialogTree, dialogTreePrefab.GetData());
				m_dialogTrees.Add(dialogTree);
				dialogTree.Initialise(dialogTreePrefab.gameObject);
			}
			m_player = m_characters[0];
			foreach (GuiComponent guiPrefab in m_guiPrefabs)
			{
				Gui gui = new Gui();
				QuestUtils.CopyFields(gui, guiPrefab.GetData());
				m_guis.Add(gui);
				gui.Initialise(guiPrefab.gameObject);
			}
		}

		private void Start()
		{
			RestoreSettings();
			m_settings.OnInitialise();
			OnSceneLoaded();
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
			OnSceneLoaded();
		}

		private void OnSceneLoaded()
		{
			if (m_levelLoadedCalled)
			{
				return;
			}
			m_transitioning = true;
			m_levelLoadedCalled = true;
			string sceneName = SceneManager.GetActiveScene().name;
			FadeOutBG(0f, "RoomChange");
			if (Application.isEditor && s_restartAssembly != null)
			{
				m_hotLoadAssembly = s_restartAssembly;
				s_restartAssembly = null;
				foreach (IQuestScriptable allScriptable in GetAllScriptables())
				{
					allScriptable.HotLoadScript(m_hotLoadAssembly);
				}
			}
			m_coroutineMainLoop = StartCoroutine(LoadRoomSequence(sceneName));
		}

		private void Update()
		{
			UpdateCameraLetterboxing();
			if (m_restoring)
			{
				m_menuManager.Update();
				return;
			}
			UpdateRegions();
			UpdateGuiVisibility();
			Camera main = UnityEngine.Camera.main;
			if (!m_overrideMousePos)
			{
				m_mousePos = Vector2.zero;
				if (main != null)
				{
					m_mousePos = main.ScreenToWorldPoint(Input.mousePosition.WithZ(0f));
				}
			}
			if (main != null)
			{
				m_mousePosGui = m_cameraGui.ScreenToWorldPoint(main.WorldToScreenPoint(m_mousePos));
			}
			UpdateGuiFocus();
			UpdateDebugKeys();
			if (Application.isEditor && main != null)
			{
				Vector2 vector = main.ScreenToViewportPoint(Input.mousePosition);
				UnityEngine.Cursor.visible = vector.x < 0f || vector.x > 1f || vector.y < 0f || vector.y > 1f || !Cursor.Visible;
			}
			if (!m_customKbShortcuts)
			{
				if (Input.GetMouseButtonDown(0))
				{
					SkipDialog();
				}
				else if (Input.GetMouseButtonDown(1) || (GameHasKeyboardFocus && Input.GetKeyDown(KeyCode.Space)))
				{
					SkipDialog(useNoSkipTime: false);
				}
				else if (GameHasKeyboardFocus && Input.GetKey(KeyCode.Escape) && !m_skipCutsceneButtonConsumed)
				{
					SkipDialog(useNoSkipTime: false);
				}
				if (!GameHasKeyboardFocus || !Input.GetKey(KeyCode.Escape))
				{
					m_skipCutsceneButtonConsumed = false;
				}
			}
			if (m_globalScript != null)
			{
				MethodInfo method = m_globalScript.GetType().GetMethod(FUNC_UPDATE_NOPAUSE, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (method != null)
				{
					method.Invoke(m_globalScript, null);
				}
			}
			if (!SystemTime.Paused)
			{
				for (int i = 0; i < SV.m_timers.Count; i++)
				{
					SV.m_timers[i].t -= Time.deltaTime;
				}
				if (!m_customKbShortcuts && m_cutscene && GameHasKeyboardFocus && Input.GetKeyDown(KeyCode.Escape))
				{
					SkipCutscene();
					m_skipCutsceneButtonConsumed = true;
				}
				if (m_roomLoopStarted || !m_transitioning)
				{
					if (m_globalScript != null)
					{
						MethodInfo method2 = m_globalScript.GetType().GetMethod(FUNC_UPDATE, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (method2 != null)
						{
							method2.Invoke(m_globalScript, null);
						}
					}
					if (m_currentRoom != null && m_currentRoom.GetScript() != null)
					{
						MethodInfo method3 = m_currentRoom.GetScript().GetType().GetMethod(FUNC_UPDATE, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (method3 != null)
						{
							method3.Invoke(m_currentRoom.GetScript(), null);
						}
					}
				}
			}
			foreach (Gui gui in m_guis)
			{
				if (gui.Instance != null && gui.Instance.isActiveAndEnabled && gui.GetScript() != null)
				{
					MethodInfo method4 = gui.GetScript().GetType().GetMethod(FUNC_UPDATE, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (method4 != null)
					{
						method4.Invoke(gui.GetScript(), null);
					}
				}
			}
			m_menuManager.Update();
		}

		private void LateUpdate()
		{
			if (m_restartOnUpdate)
			{
				StopAllCoroutines();
				m_restartOnUpdate = false;
				SceneManager.MoveGameObjectToScene(base.gameObject, SceneManager.GetActiveScene());
				SceneManager.MoveGameObjectToScene(m_cameraGui.gameObject, SceneManager.GetActiveScene());
				if (!string.IsNullOrEmpty(s_restartScene))
				{
					SceneManager.LoadScene(s_restartScene);
					s_restartScene = null;
				}
				else
				{
					SceneManager.LoadScene(0);
				}
			}
		}

		private void UpdateDebugKeys()
		{
			if (m_customKbShortcuts)
			{
				return;
			}
			if (!Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F5) && !GetBlocked())
			{
				Save(1, "QuickSave");
			}
			if (Input.GetKeyDown(KeyCode.F7) && !GetBlocked())
			{
				RestoreSave(1);
			}
			if (Input.GetKeyDown(KeyCode.F9) && !GetBlocked())
			{
				if (GetDebugKeyHeld())
				{
					Restart(m_currentRoom, m_currentRoom.Instance.m_debugStartFunction);
				}
				else
				{
					Restart();
				}
			}
			if (!GetDebugKeyHeld())
			{
				return;
			}
			if (Input.GetKeyDown(KeyCode.F10) && !GetBlocked())
			{
				string text = "";
				foreach (QuestSaveSlotData saveSlotDatum in m_saveManager.GetSaveSlotData())
				{
					text += $"{saveSlotDatum.m_slotId}: {saveSlotDatum.m_description}\n";
				}
				Debug.Log(text);
			}
			if (!Input.GetKeyDown(KeyCode.I))
			{
				return;
			}
			foreach (Inventory inventoryItem in m_inventoryItems)
			{
				if (!m_player.HasInventory(inventoryItem))
				{
					m_player.AddInventory(inventoryItem);
				}
			}
		}

		private void Block()
		{
			m_blocking = true;
			CallbackOnBlock?.Invoke();
		}

		private void Unblock()
		{
			m_blocking = false;
			CallbackOnUnblock?.Invoke();
		}

		private void StartDisplay(string text, int id, out QuestText textComponent)
		{
			textComponent = null;
			Gui gui = GetGui(DisplayBoxGui);
			if (gui == null || gui.GetInstance() == null)
			{
				return;
			}
			m_displayActive = true;
			text = SystemText.GetDisplayText(text, id, "Narr");
			SystemAudio.Stop(m_dialogAudioSource);
			m_dialogAudioSource = null;
			if (Settings.DialogDisplay != QuestSettings.eDialogDisplay.TextOnly)
			{
				m_dialogAudioSource = SystemText.PlayAudio(id, "Narr", null, SingletonAuto<SystemAudio>.Get.NarratorMixerGroup);
			}
			if (Settings.DialogDisplay != QuestSettings.eDialogDisplay.SpeechOnly || Singleton<PowerQuest>.Get.AlwaysShowDisplayText)
			{
				QuestText questText = (textComponent = gui.GetInstance().GetComponentInChildren<QuestText>(includeInactive: true));
				if (questText != null)
				{
					questText.SetText(text);
					gui.Visible = true;
				}
			}
		}

		private void EndDisplay()
		{
			SystemAudio.Stop(m_dialogAudioSource);
			Gui gui = GetGui(DisplayBoxGui);
			if (gui != null && !(gui.GetInstance() == null))
			{
				gui.Visible = false;
				m_displayActive = false;
			}
		}

		public void UpdateCameraLetterboxing()
		{
			if (!(Camera.GetInstance() == null))
			{
				RectCentered rectCentered = new RectCentered(Camera.GetInstance().Camera.rect);
				float defaultVerticalResolution = DefaultVerticalResolution;
				float num = (float)Screen.width / (float)Screen.height * defaultVerticalResolution;
				float num2 = Mathf.Clamp(num, HorizontalResolution.Min, HorizontalResolution.Max);
				if (num2 < num)
				{
					rectCentered.Width = num2 / num;
					rectCentered.Height = 1f;
				}
				else if (num2 > num)
				{
					rectCentered.Width = 1f;
					rectCentered.Height = num / num2;
				}
				else
				{
					rectCentered.Width = 1f;
					rectCentered.Height = 1f;
				}
				Camera.GetInstance().Camera.rect = rectCentered;
				GetCameraGui().rect = rectCentered;
			}
		}

		private void UpdateRegions()
		{
			List<RegionComponent> regionComponents = m_currentRoom.GetInstance().GetRegionComponents();
			int count = regionComponents.Count;
			for (int i = 0; i < count; i++)
			{
				regionComponents[i].GetData().GetCharacterOnRegionMask().SetAll(value: false);
			}
			for (int j = 0; j < m_characters.Count; j++)
			{
				Character character = m_characters[j];
				bool flag = character.Enabled && (character.Room == m_currentRoom || character.IsPlayer);
				if (!flag)
				{
					continue;
				}
				Vector2 position = character.Position;
				Color color = new Color(1f, 1f, 1f, 0f);
				float num = 1f;
				for (int k = 0; k < count; k++)
				{
					RegionComponent regionComponent = regionComponents[k];
					Region data = regionComponent.GetData();
					if (regionComponent.UpdateCharactersOnRegion(j, flag, position))
					{
						if (character.UseRegionScaling)
						{
							float scaleAt = regionComponent.GetScaleAt(position);
							if (scaleAt != 1f)
							{
								num = scaleAt;
							}
						}
						if (character.UseRegionTinting && data.Tint.a > 0f)
						{
							float fadeRatio = regionComponent.GetFadeRatio(position);
							if (color.a <= 0f)
							{
								color = data.Tint;
								color.a *= fadeRatio;
							}
							else
							{
								Color tint = data.Tint;
								color = Color.Lerp(color, tint, fadeRatio);
							}
						}
					}
					RegionComponent.eTriggerResult eTriggerResult = regionComponent.UpdateCharacterOnRegionState(j, background: true);
					if (!data.Enabled || m_currentRoom == null || m_currentRoom.GetScript() == null || (data.PlayerOnly && character != m_player))
					{
						continue;
					}
					switch (eTriggerResult)
					{
					case RegionComponent.eTriggerResult.Enter:
					{
						MethodInfo method2 = m_currentRoom.GetScript().GetType().GetMethod(SCRIPT_FUNCTION_ENTER_REGION_BG + data.ScriptName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (method2 != null)
						{
							method2.Invoke(m_currentRoom.GetScript(), new object[2] { data, character });
						}
						break;
					}
					case RegionComponent.eTriggerResult.Exit:
					{
						MethodInfo method = m_currentRoom.GetScript().GetType().GetMethod(SCRIPT_FUNCTION_EXIT_REGION_BG + data.ScriptName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (method != null)
						{
							method.Invoke(m_currentRoom.GetScript(), new object[2] { data, character });
						}
						break;
					}
					}
				}
				if (!(character.GetInstance() != null))
				{
					continue;
				}
				CharacterComponent component = character.GetInstance().GetComponent<CharacterComponent>();
				component.transform.localScale = new Vector3(num * Mathf.Sign(component.transform.localScale.x), num, num);
				PowerSprite[] componentsInChildren = character.GetInstance().GetComponentsInChildren<PowerSprite>(includeInactive: true);
				foreach (PowerSprite powerSprite in componentsInChildren)
				{
					if (powerSprite != null)
					{
						powerSprite.Tint = color;
					}
				}
			}
		}

		private void OnEndCutscene()
		{
			if (m_cutscene && m_skipCutscene)
			{
				if (m_cameraData.GetInstance() != null)
				{
					m_cameraData.GetInstance().Snap();
				}
				if (CallbackOnEndCutscene != null)
				{
					CallbackOnEndCutscene();
				}
				m_menuManager.FadeSkip();
				FadeInBG(0.15f, "CUTSCENE");
			}
			m_cutscene = false;
			m_skipCutscene = false;
		}

		public void OnPlayerWalkComplete()
		{
			EndCancelableSection();
		}

		public void EndCancelableSection()
		{
			if (m_sequenceIsCancelable && m_backgroundSequence != null)
			{
				m_currentSequence = m_backgroundSequence;
				m_backgroundSequence = null;
			}
			m_sequenceIsCancelable = false;
		}

		public void OnSay()
		{
			if (m_coroutineSay != null)
			{
				StopCoroutine(m_coroutineSay);
				EndDisplay();
			}
		}

		public void CancelSay()
		{
			if (m_coroutineSay != null)
			{
				StopCoroutine(m_coroutineSay);
				EndDisplay();
			}
			foreach (Character character in m_characters)
			{
				character.CancelSay();
			}
		}

		private IQuestClickable GetObjectAt(Vector2 pos, int layerMask, out GameObject pickedGameObject)
		{
			IQuestClickable result = null;
			int num = Physics2D.OverlapPointNonAlloc(pos, m_tempPicked, layerMask);
			pickedGameObject = null;
			if (m_tempPicked != null && num > 0)
			{
				float num2 = float.MaxValue;
				for (int i = 0; i < num; i++)
				{
					IQuestClickable questClickable = null;
					GameObject gameObject = m_tempPicked[i].gameObject;
					GuiDialogOption component = gameObject.GetComponent<GuiDialogOption>();
					if (component != null)
					{
						questClickable = component.Clickable;
					}
					if (questClickable == null && m_inventoryClickStyle == eInventoryClickStyle.OnMouseClick)
					{
						InventoryComponent component2 = gameObject.GetComponent<InventoryComponent>();
						if (component2 != null)
						{
							questClickable = component2.GetData();
						}
					}
					if (questClickable == null)
					{
						GuiControl component3 = gameObject.GetComponent<GuiControl>();
						if (component3 != null)
						{
							questClickable = component3;
						}
					}
					if (questClickable == null)
					{
						GuiComponent component4 = gameObject.GetComponent<GuiComponent>();
						if (component4 != null)
						{
							questClickable = component4.GetData();
						}
					}
					if (questClickable == null)
					{
						HotspotComponent component5 = gameObject.GetComponent<HotspotComponent>();
						if (component5 != null)
						{
							questClickable = component5.GetData();
						}
					}
					if (questClickable == null)
					{
						PropComponent component6 = gameObject.GetComponent<PropComponent>();
						if (component6 != null)
						{
							questClickable = component6.GetData();
						}
					}
					if (questClickable == null)
					{
						CharacterComponent component7 = gameObject.GetComponent<CharacterComponent>();
						if (component7 != null)
						{
							questClickable = component7.GetData();
						}
					}
					if (questClickable == null || !questClickable.Clickable)
					{
						continue;
					}
					float num3 = questClickable.Baseline + gameObject.transform.position.y;
					if (questClickable.ClickableType == eQuestClickableType.Gui || questClickable.ClickableType == eQuestClickableType.Inventory)
					{
						if (gameObject.GetComponent<GuiComponent>() != null)
						{
							num3 = questClickable.Baseline;
						}
						else
						{
							GuiComponent componentInParent = gameObject.GetComponentInParent<GuiComponent>();
							if (componentInParent != null)
							{
								num3 = componentInParent.GetData().Baseline - 0.5f + questClickable.Baseline / 1000f;
							}
						}
					}
					if (num3 < num2)
					{
						pickedGameObject = gameObject;
						num2 = num3;
						result = questClickable;
					}
				}
			}
			return result;
		}

		private void EnableCancelInternal()
		{
			if (m_allowEnableCancel)
			{
				m_sequenceIsCancelable = true;
				m_backgroundSequences.Clear();
				m_backgroundSequences = m_currentSequences;
				m_currentSequences = new List<Coroutine>();
				m_backgroundSequence = m_currentSequence;
				m_currentSequence = StartCoroutine(CoroutineEmpty());
			}
		}

		public bool GetInteractionInProgress(IQuestClickable clickable, eQuestVerb verb)
		{
			if (clickable == null || (m_backgroundSequence == null && !m_blocking && m_currentSequence == null))
			{
				return false;
			}
			for (int i = 0; i < m_currentInteractionClickables.Count; i++)
			{
				if (m_currentInteractionClickables[i] == clickable && m_currentInteractionVerbs[i] == verb)
				{
					return true;
				}
			}
			return false;
		}

		private void OnInteraction(IQuestClickable clickable, eQuestVerb verb)
		{
			CancelCurrentInteraction();
			SV.m_currentInteractionOccurrences.Clear();
			m_currentInteractionClickables.Clear();
			m_currentInteractionVerbs.Clear();
			if (clickable != null)
			{
				m_currentInteractionClickables.Add(clickable);
				m_currentInteractionVerbs.Add(verb);
				clickable.OnInteraction(verb);
			}
		}

		private void OnHandleInteraction(IQuestClickable clickable, eQuestVerb verb)
		{
			m_currentInteractionClickables.Add(clickable);
			m_currentInteractionVerbs.Add(verb);
			clickable.OnInteraction(verb);
		}

		private bool StartScriptInteraction(IQuestScriptable scriptable, string methodName, object[] parameters = null, bool stopPlayerMoving = false, bool cancelCurrentInteraction = false)
		{
			QuestScript script = scriptable.GetScript();
			if (stopPlayerMoving)
			{
				CancelCurrentInteraction();
			}
			m_allowEnableCancel = true;
			Coroutine coroutine = null;
			try
			{
				coroutine = StartScriptInteractionCoroutine(script, methodName, parameters, stopPlayerMoving, cancelCurrentInteraction);
				if (coroutine != null && coroutine != m_consumedInteraction)
				{
					if (m_currentSequence == null)
					{
						m_currentSequence = coroutine;
						if (m_player.Walking && stopPlayerMoving && m_currentDialog == null && !cancelCurrentInteraction)
						{
							EnableCancelInternal();
						}
					}
					else
					{
						m_queuedScriptInteractions.Add(coroutine);
					}
				}
				SetAutoLoadScript(scriptable, methodName, coroutine != null, isWaitForFunction: false);
			}
			catch
			{
			}
			return coroutine != null;
		}

		private Coroutine StartScriptInteractionCoroutine(QuestScript scriptClass, string methodName, object[] parameters = null, bool stopPlayerMoving = false, bool cancelCurrentInteraction = false)
		{
			Coroutine coroutine = null;
			if (scriptClass != null)
			{
				MethodInfo method = scriptClass.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (method != null && ((parameters == null && method.GetParameters().Length == 0) || method.GetParameters().Length == parameters.Length))
				{
					if (stopPlayerMoving)
					{
						m_player.StopWalking();
					}
					m_autoLoadFunc = methodName;
					if (method.Invoke(scriptClass, parameters) is IEnumerator enumerator)
					{
						bool transitioning = m_transitioning;
						DialogTree currentDialog = m_currentDialog;
						if (cancelCurrentInteraction && m_sequenceIsCancelable)
						{
							m_sequenceIsCancelable = false;
							coroutine = StartCoroutine(enumerator);
							m_sequenceIsCancelable = true;
						}
						else
						{
							coroutine = StartCoroutine(enumerator);
						}
						if (enumerator.Current == EMPTY_YIELD_INSTRUCTION || coroutine == null)
						{
							if ((m_transitioning && !transitioning) || currentDialog != m_currentDialog)
							{
								m_consumedInteraction = StartCoroutine(CoroutineEmpty());
								coroutine = m_consumedInteraction;
							}
							else
							{
								coroutine = null;
							}
						}
						else if (enumerator.Current == CONSUME_YIELD_INSTRUCTION)
						{
							m_consumedInteraction = StartCoroutine(CoroutineEmpty());
							coroutine = m_consumedInteraction;
						}
						else if (coroutine != null)
						{
							if (cancelCurrentInteraction)
							{
								CancelCurrentInteraction();
							}
							m_currentSequences.Add(coroutine);
						}
					}
				}
			}
			return coroutine;
		}

		private void SetAutoLoadScript(IQuestScriptable questScriptable, string functionName, bool functionBlocked, bool isWaitForFunction)
		{
			if (!Application.isEditor)
			{
				return;
			}
			if (isWaitForFunction)
			{
				m_ignoreAutoLoadFunc = true;
			}
			else
			{
				if (m_autoLoadFunc == functionName && m_ignoreAutoLoadFunc)
				{
					m_autoLoadFunc = string.Empty;
					m_ignoreAutoLoadFunc = false;
					return;
				}
				m_autoLoadFunc = string.Empty;
				m_ignoreAutoLoadFunc = false;
			}
			bool flag = functionName.StartsWith(STR_UNHANDLED);
			if (functionBlocked)
			{
				if (flag && m_autoLoadUnhandledScriptable != null)
				{
					m_autoLoadScriptable = m_autoLoadUnhandledScriptable;
					m_autoLoadFunction = m_autoLoadUnhandledFunction;
				}
				else
				{
					m_autoLoadScriptable = questScriptable;
					m_autoLoadFunction = functionName;
				}
			}
			else if (!flag)
			{
				m_autoLoadUnhandledScriptable = questScriptable;
				m_autoLoadUnhandledFunction = functionName;
			}
		}

		private static IEnumerator CoroutineEmpty()
		{
			if (!Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				yield return null;
			}
		}

		private static IEnumerator CoroutineWaitForTime(float time, bool skippable)
		{
			bool first = true;
			while ((time > 0f || first) && !Singleton<PowerQuest>.Get.GetSkippingCutscene() && (!skippable || !Singleton<PowerQuest>.Get.HandleSkipDialogKeyPressed() || first))
			{
				first = false;
				yield return new WaitForEndOfFrame();
				if (!SystemTime.Paused)
				{
					time -= Time.deltaTime;
				}
			}
		}

		private static IEnumerator CoroutineWaitForTimer(string timerName, bool skippable)
		{
			bool first = true;
			while ((Singleton<PowerQuest>.Get.GetTimer(timerName) > 0f || first) && !Singleton<PowerQuest>.Get.GetSkippingCutscene() && (!skippable || !Singleton<PowerQuest>.Get.HandleSkipDialogKeyPressed() || first))
			{
				first = false;
				yield return new WaitForEndOfFrame();
			}
			Singleton<PowerQuest>.Get.SetTimer(timerName, -1f);
		}

		private IEnumerator CoroutineDelayedInvoke(float time, Action functionToInvoke)
		{
			yield return Wait(time);
			functionToInvoke?.Invoke();
		}

		private IEnumerator CoroutineDisplay(string text, int id = -1)
		{
			if (!GetSkippingCutscene())
			{
				StartDisplay(text, id, out var textComponent);
				yield return WaitForDialog(Singleton<PowerQuest>.Get.GetTextDisplayTime(text), m_dialogAudioSource, m_displayTextAutoAdvance, skippable: true, textComponent);
				EndDisplay();
			}
		}

		private IEnumerator CoroutineDisplayBG(string text, int id = -1)
		{
			if (!Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				StartDisplay(text, id, out var textComponent);
				yield return WaitForDialog(Singleton<PowerQuest>.Get.GetTextDisplayTime(text), m_dialogAudioSource, autoAdvance: true, skippable: false, textComponent);
				EndDisplay();
			}
		}

		private IEnumerator CoroutineWaitForDialog(float time, bool skippable, bool autoAdvance, AudioHandle audioSource, QuestText textComponent = null)
		{
			m_timeLastTextShown = Time.timeSinceLevelLoad;
			bool first = true;
			while (ShouldContinueDialog(first, ref time, skippable || m_waitingForBGDialogSkip, autoAdvance, audioSource, textComponent))
			{
				first = false;
				yield return new WaitForEndOfFrame();
				if (!SystemTime.Paused)
				{
					time -= Time.deltaTime;
				}
			}
		}

		public bool ShouldContinueDialogOld(bool firstCall, ref float time, bool skippable, bool autoAdvance, AudioHandle audioSource, QuestText textComponent = null, float endTime = 0f)
		{
			bool flag = true;
			if (autoAdvance && audioSource == null)
			{
				flag &= time > endTime;
			}
			flag &= !Singleton<PowerQuest>.Get.GetSkippingCutscene();
			if (textComponent != null && textComponent.GetTyping() && Singleton<PowerQuest>.Get.HandleSkipDialogKeyPressed() && !firstCall)
			{
				textComponent.SkipTyping();
				return true;
			}
			flag &= !skippable || !Singleton<PowerQuest>.Get.HandleSkipDialogKeyPressed() || firstCall;
			if (audioSource != null)
			{
				if ((1u & ((audioSource == null || audioSource.isPlaying || !Application.isFocused || Singleton<PowerQuest>.Get.Paused) ? 1u : 0u)) != 0)
				{
					time = 0f;
				}
				else
				{
					flag &= time > 0f - m_textAutoAdvanceDelay;
				}
				flag &= endTime <= 0f || audioSource == null || audioSource.clip == null || audioSource.time < audioSource.clip.length - endTime;
			}
			return flag;
		}

		public bool ShouldContinueDialog(bool firstCall, ref float time, bool skippable, bool autoAdvance, AudioHandle audioSource, QuestText textComponent = null, float endTime = 0f)
		{
			if (autoAdvance && audioSource == null && time <= endTime)
			{
				return false;
			}
			if (Singleton<PowerQuest>.Get.GetSkippingCutscene())
			{
				return false;
			}
			if (textComponent != null && textComponent.GetTyping() && Singleton<PowerQuest>.Get.HandleSkipDialogKeyPressed() && !firstCall)
			{
				textComponent.SkipTyping();
				return true;
			}
			if (skippable && Singleton<PowerQuest>.Get.HandleSkipDialogKeyPressed() && !firstCall)
			{
				return false;
			}
			if (audioSource != null)
			{
				bool flag = audioSource.isPlaying || !Application.isFocused || Singleton<PowerQuest>.Get.Paused;
				if (m_textAutoAdvanceDelay > 0f)
				{
					if (flag)
					{
						time = 0f;
					}
					else if (time <= 0f - m_textAutoAdvanceDelay)
					{
						return false;
					}
				}
				else if (!flag)
				{
					return false;
				}
				if (endTime > 0f && audioSource.clip != null && audioSource.time >= audioSource.clip.length - endTime)
				{
					return false;
				}
			}
			return true;
		}

		private IEnumerator CoroutineFadeIn(string source, float time, bool skippable = false)
		{
			m_menuManager.FadeIn(time, source);
			yield return skippable ? WaitSkip(time) : Wait(time);
			if (skippable)
			{
				m_menuManager.FadeSkip();
			}
		}

		private IEnumerator CoroutineFadeOut(string source, float time, bool skippable = false)
		{
			m_menuManager.FadeOut(time, source);
			yield return skippable ? WaitSkip(time) : Wait(time);
			m_menuManager.FadeSkip();
			yield return null;
		}

		private IEnumerator CoroutineRoomTransition(Room room, bool instant)
		{
			bool wasBlocking = m_blocking;
			if (!wasBlocking)
			{
				Block();
			}
			string sceneName = room.GetSceneName();
			m_transitioning = true;
			if (!m_restoring)
			{
				SV.m_callEnterOnRestore = true;
			}
			StartCoroutine(LoadAtlasAsync(room.ScriptName));
			AsyncOperation operation = null;
			if (instant)
			{
				FadeOutBG(0f, "RoomChange");
			}
			else
			{
				operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
				operation.allowSceneActivation = false;
				yield return FadeOut(TransitionFadeTime / 2f, "RoomChange");
			}
			if (m_coroutineMainLoop != null)
			{
				StopCoroutine(m_coroutineMainLoop);
				m_coroutineMainLoop = null;
				m_roomLoopStarted = false;
			}
			if (!m_restoring)
			{
				Coroutine coroutine = StartScriptInteractionCoroutine(GetScript(), "OnExitRoom", new object[2] { m_currentRoom, room });
				SetAutoLoadScript(this, "OnExitRoom", coroutine != null, isWaitForFunction: false);
				if (coroutine != null)
				{
					yield return coroutine;
				}
				if (m_currentRoom != null && m_currentRoom.GetScript() != null)
				{
					coroutine = StartScriptInteractionCoroutine(m_currentRoom.GetScript(), "OnExitRoom", new object[2] { m_currentRoom, room });
					SetAutoLoadScript(m_currentRoom, "OnExitRoom", coroutine != null, isWaitForFunction: false);
					if (coroutine != null)
					{
						yield return coroutine;
					}
				}
				RestoreAllClickables();
				RestoreAllClickableCursors();
			}
			if (!instant)
			{
				while (m_loadingAtlas)
				{
					yield return null;
				}
				operation.allowSceneActivation = true;
				while (!operation.isDone)
				{
					yield return null;
				}
				yield break;
			}
			yield return new WaitForEndOfFrame();
			while (m_loadingAtlas)
			{
				yield return null;
			}
			if (!wasBlocking)
			{
				Unblock();
			}
			SceneManager.LoadScene(sceneName);
		}

		private IEnumerator CoroutineWaitUntil(Func<bool> condition, bool skippable = false)
		{
			bool first = true;
			while (condition != null && !condition() && !Singleton<PowerQuest>.Get.GetSkippingCutscene() && (!skippable || !Singleton<PowerQuest>.Get.HandleSkipDialogKeyPressed() || first))
			{
				first = false;
				yield return null;
			}
		}

		private IEnumerator CoroutineWaitWhile(Func<bool> condition, bool skippable = false)
		{
			bool first = true;
			while (condition != null && condition() && !Singleton<PowerQuest>.Get.GetSkippingCutscene() && (!skippable || !Singleton<PowerQuest>.Get.HandleSkipDialogKeyPressed() || first))
			{
				first = false;
				yield return null;
			}
		}

		private IEnumerator CoroutineWaitForDialog()
		{
			yield return WaitWhile(() => m_displayActive || m_characters.Exists((Character item) => item.Talking));
		}

		private IEnumerator CoroutineChangeRoom(IRoom room)
		{
			GetPlayer().Room = room;
			while (!m_levelLoadedCalled || !m_roomLoopStarted)
			{
				yield return null;
			}
		}

		private void LoadAtlas(string roomName)
		{
			string text = "Room" + roomName + "Atlas";
			if (s_roomAtlasCallbacks.TryGetValue(text, out var value))
			{
				SpriteAtlas atlas = Resources.Load<SpriteAtlas>(text);
				OnAtlasLoadComplete(text, atlas, value);
			}
		}

		private IEnumerator LoadAtlasAsync(string roomName)
		{
			m_loadingAtlas = true;
			string atlasName = "Room" + roomName + "Atlas";
			if (s_roomAtlasCallbacks.TryGetValue(atlasName, out var callback))
			{
				ResourceRequest req = Resources.LoadAsync<SpriteAtlas>(atlasName);
				while (!req.isDone)
				{
					yield return null;
				}
				SpriteAtlas atlas = req.asset as SpriteAtlas;
				OnAtlasLoadComplete(atlasName, atlas, callback);
			}
			m_loadingAtlas = false;
		}

		private void OnAtlasLoadComplete(string atlasName, SpriteAtlas atlas, Action<SpriteAtlas> callback)
		{
			if ((bool)atlas)
			{
				m_atlasToUnload = m_lastAtlas;
				m_lastAtlas = atlas;
				callback(atlas);
			}
			else
			{
				Debug.Log("Failed to find atlas " + atlasName);
			}
		}

		public void LockFocusedControl()
		{
			m_focusedControlLock = m_focusedControl;
		}

		public void UnlockFocusedControl()
		{
			m_focusedControlLock = null;
		}

		public bool NavigateGui(eGuiNav input = eGuiNav.Ok)
		{
			if (!GetBlocked() && m_focusedGui != null)
			{
				return m_focusedGui.Navigate(input);
			}
			return false;
		}

		private void UpdateGuiFocus()
		{
			if (m_focusedControlLock != null && !m_focusedControlLock.gameObject.activeInHierarchy)
			{
				UnlockFocusedControl();
			}
			if (m_focusedControlLock != null)
			{
				m_mouseOverClickable = m_focusedControl;
				return;
			}
			Gui focusedGui = m_focusedGui;
			GuiControl focusedControl = m_focusedControl;
			GameObject pickedGameObject = null;
			m_focusedControl = null;
			m_focusedGui = null;
			if (!m_overrideMouseOverClickable)
			{
				m_mouseOverClickable = null;
				if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
				{
					List<RaycastResult> list = new List<RaycastResult>();
					EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current)
					{
						position = Input.mousePosition
					}, list);
					foreach (RaycastResult item in list)
					{
						if (m_inventoryClickStyle == eInventoryClickStyle.OnMouseClick)
						{
							InventoryComponent componentInParent = item.gameObject.GetComponentInParent<InventoryComponent>();
							if (componentInParent != null && componentInParent.GetData().Clickable)
							{
								m_mouseOverClickable = componentInParent.GetData();
								break;
							}
						}
						GuiComponent componentInParent2 = item.gameObject.GetComponentInParent<GuiComponent>();
						if (componentInParent2?.GetData()?.Clickable == true)
						{
							m_mouseOverClickable = componentInParent2.GetData();
							break;
						}
					}
				}
				if (m_mouseOverClickable == null && SV.m_captureInputSources.Count <= 0)
				{
					if (m_cameraGui != null)
					{
						m_mouseOverClickable = GetObjectAt(m_mousePosGui, 1 << LAYER_UI, out pickedGameObject);
					}
					if (m_mouseOverClickable == null && !GetModalGuiActive())
					{
						m_mouseOverClickable = GetObjectAt(m_mousePos, ~(1 << LAYER_UI), out pickedGameObject);
					}
				}
			}
			if (m_mouseOverClickable != null && (m_mouseOverClickable.ClickableType == eQuestClickableType.Gui || m_mouseOverClickable.ClickableType == eQuestClickableType.Inventory))
			{
				if (m_mouseOverClickable.ClickableType == eQuestClickableType.Inventory)
				{
					if (pickedGameObject != null)
					{
						m_focusedControl = pickedGameObject.GetComponent<GuiControl>();
						if (m_focusedControl != null)
						{
							m_focusedGui = m_focusedControl.GuiData;
						}
						else
						{
							m_focusedGui = null;
						}
					}
				}
				else if (m_mouseOverClickable is GuiControl)
				{
					m_focusedControl = m_mouseOverClickable as GuiControl;
					if (m_focusedControl != null)
					{
						m_focusedGui = m_focusedControl.GuiData;
					}
					else
					{
						m_focusedGui = null;
					}
				}
				else
				{
					m_focusedGui = m_mouseOverClickable as Gui;
				}
			}
			Gui topModalGui = GetTopModalGui();
			if (topModalGui != null && topModalGui != m_focusedGui && (m_focusedGui == null || topModalGui.Baseline < m_focusedGui.Baseline))
			{
				if (topModalGui.Clickable)
				{
					m_mouseOverClickable = topModalGui;
					m_focusedGui = topModalGui;
				}
				else
				{
					m_mouseOverClickable = null;
					m_focusedGui = null;
				}
				m_focusedControl = null;
			}
			if (focusedGui != m_focusedGui)
			{
				focusedGui?.OnDefocus();
				if (m_focusedGui != null && m_focusedGui.Instance != null)
				{
					m_focusedGui.OnFocus();
				}
			}
			if (focusedControl != m_focusedControl)
			{
				if (focusedControl != null)
				{
					focusedControl.OnDefocus();
				}
				if (m_focusedControl != null)
				{
					m_focusedControl.OnFocus();
				}
			}
		}

		private void UpdateGuiVisibility()
		{
			m_sortedGuis.Clear();
			m_sortedGuis.AddRange(m_guis);
			m_sortedGuis.Sort((Gui a, Gui b) => a.Baseline.CompareTo(b.Baseline));
			bool flag = false;
			foreach (Gui sortedGui in m_sortedGuis)
			{
				if (flag)
				{
					sortedGui.HiddenBySystem = true;
				}
				else if (!sortedGui.VisibleInCutscenes && (GetBlocked() || m_queuedScriptInteractions.Count > 0))
				{
					sortedGui.HiddenBySystem = true;
				}
				else
				{
					sortedGui.HiddenBySystem = false;
				}
				if (sortedGui.Visible && sortedGui.HideObscuredGuis)
				{
					flag = true;
				}
			}
			foreach (Gui sortedGui2 in m_sortedGuis)
			{
				if (!sortedGui2.Visible)
				{
					continue;
				}
				string[] hideSpecificGuis = sortedGui2.HideSpecificGuis;
				foreach (string scriptName in hideSpecificGuis)
				{
					Gui gui = GetGui(scriptName);
					if (gui != null)
					{
						gui.HiddenBySystem = true;
					}
				}
			}
		}

		public IGui GetBlockingGui()
		{
			return m_blockingGui;
		}

		public Coroutine WaitForGui(IGui gui)
		{
			return StartQuestCoroutine(CoroutineWaitForGui(gui));
		}

		public void OnGuiShown(Gui gui)
		{
			if (gui == null)
			{
				throw new Exception("gui is null");
			}
			if (gui.GetScript() != null)
			{
				MethodInfo method = gui.GetScript().GetType().GetMethod("OnShow", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (method != null)
				{
					method.Invoke(gui.GetScript(), null);
				}
			}
		}

		private IEnumerator CoroutineWaitForGui(IGui gui)
		{
			bool wasCutscene = m_cutscene;
			if (wasCutscene)
			{
				EndCutscene();
			}
			m_blockingGui = gui;
			gui.Show();
			bool hideCursor = GetCursor().HideWhenBlocking;
			GetCursor().HideWhenBlocking = false;
			GetCursor().Visible = true;
			yield return WaitWhile(() => m_blockingGui.Visible);
			GetCursor().HideWhenBlocking = hideCursor;
			m_blockingGui = null;
			if (wasCutscene)
			{
				StartCutscene();
			}
			yield return Break;
		}

		public bool GetIsGuiObscuredByModal(Gui gui)
		{
			for (int i = 0; i < m_guis.Count; i++)
			{
				Gui gui2 = m_guis[i];
				if (gui2.Visible && !gui2.HiddenBySystem && gui2.Baseline <= gui.Baseline && gui2.Modal)
				{
					return true;
				}
			}
			return false;
		}

		private IEnumerator LoadRoomSequence(string sceneName)
		{
			bool flag = !m_initialised;
			Camera[] array = UnityEngine.Object.FindObjectsOfType<Camera>(includeInactive: false);
			foreach (Camera camera in array)
			{
				if (camera != null && camera.gameObject != null && camera.gameObject.name == "QuestGuiCamera")
				{
					if (m_cameraGui == null)
					{
						m_cameraGui = camera;
						m_canvas = m_cameraGui.GetComponentInChildren<Canvas>();
						UnityEngine.Object.DontDestroyOnLoad(m_cameraGui.gameObject);
					}
					else if (camera != m_cameraGui)
					{
						UnityEngine.Object.DestroyImmediate(camera.gameObject);
					}
				}
			}
			QuestCameraComponent instance = UnityEngine.Object.FindObjectOfType<QuestCameraComponent>();
			m_cameraData.SetInstance(instance);
			UpdateCameraLetterboxing();
			foreach (Gui gui in m_guis)
			{
				if (gui.Instance != null)
				{
					continue;
				}
				GameObject gameObject = GameObject.Find(gui.GetPrefab().name);
				if (gameObject == null)
				{
					gameObject = UnityEngine.Object.Instantiate(gui.GetPrefab());
				}
				gui.SetInstance(gameObject.GetComponent<GuiComponent>());
				gameObject.SetActive(gui.Visible && gui.VisibleInCutscenes);
				if ((bool)gameObject.GetComponent<RectTransform>())
				{
					if (m_canvas != null)
					{
						gameObject.transform.SetParent(m_canvas.transform, worldPositionStays: false);
					}
				}
				else if (m_cameraGui != null)
				{
					gameObject.transform.SetParent(m_cameraGui.transform, worldPositionStays: false);
					gameObject.transform.position = gameObject.transform.position.WithZ(0f);
				}
			}
			if (flag)
			{
				MethodInfo method = m_globalScript.GetType().GetMethod("OnGameStart", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (method != null)
				{
					method.Invoke(m_globalScript, null);
				}
			}
			if (m_cursorPrefab != null)
			{
				QuestCursorComponent component = UnityEngine.Object.Instantiate(m_cursor.GetPrefab()).GetComponent<QuestCursorComponent>();
				m_cursor.SetInstance(component);
			}
			RoomComponent roomComponent = UnityEngine.Object.FindObjectOfType<RoomComponent>();
			string scriptName = roomComponent.GetData().ScriptName;
			Room room = (m_currentRoom = QuestUtils.FindScriptable(m_rooms, scriptName));
			room.SetInstance(roomComponent);
			m_player.Room = GetRoom(scriptName);
			foreach (Character character in m_characters)
			{
				if (character.Room == GetRoom(scriptName))
				{
					character.SpawnInstance();
					continue;
				}
				GameObject gameObject2 = GameObject.Find(character.GetPrefab().name);
				if (gameObject2 != null)
				{
					gameObject2.name = "deleted";
					UnityEngine.Object.Destroy(gameObject2);
				}
			}
			m_cameraData.SetCharacterToFollow(GetPlayer());
			m_initialised = true;
			if (room.GetInstance() != null)
			{
				room.GetInstance().OnLoadComplete();
			}
			if (m_restoring)
			{
				object[] onPostRestoreParams = new object[1] { m_restoredVersion };
				foreach (IQuestScriptable allScriptable in GetAllScriptables())
				{
					if (allScriptable != null && allScriptable.GetScript() != null && (allScriptable.GetScript() == room.GetScript() || !allScriptable.GetScriptClassName().StartsWithIgnoreCase(STR_ROOM_START)))
					{
						CallScriptPostRestore(allScriptable, onPostRestoreParams);
					}
				}
			}
			if (m_restoring && !SV.m_callEnterOnRestore)
			{
				yield return null;
				FadeInBG(TransitionFadeTime / 2f, "RoomChange");
				m_transitioning = false;
				m_restoring = false;
			}
			else
			{
				bool debugSkipEnter = false;
				if (flag && Singleton<PowerQuest>.Get.IsDebugBuild && room != null && room.GetScript() != null)
				{
					string value = ((!s_hasRestarted) ? room.GetInstance().m_debugStartFunction : s_restartPlayFromFunction);
					if (!string.IsNullOrEmpty(value))
					{
						MethodInfo method2 = room.GetScript().GetType().GetMethod(value, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (method2 != null)
						{
							object obj = method2.Invoke(room.GetScript(), null);
							if (obj != null && obj.Equals(true))
							{
								debugSkipEnter = true;
							}
						}
					}
				}
				Block();
				m_cameraData.ResetPositionOverride();
				yield return new WaitForEndOfFrame();
				if (m_globalScript != null)
				{
					MethodInfo method3 = m_globalScript.GetType().GetMethod("OnEnterRoom", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (method3 != null)
					{
						method3.Invoke(m_globalScript, null);
					}
				}
				if (room != null && room.GetScript() != null && !debugSkipEnter)
				{
					MethodInfo method3 = room.GetScript().GetType().GetMethod("OnEnterRoom", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (method3 != null)
					{
						method3.Invoke(room.GetScript(), null);
					}
				}
				FadeInBG(TransitionFadeTime / 2f, "RoomChange");
				Coroutine coroutine = StartScriptInteractionCoroutine(GetScript(), "OnEnterRoomAfterFade");
				Coroutine onEnterRoom = null;
				if (room != null && room.GetScript() != null && !debugSkipEnter)
				{
					onEnterRoom = StartScriptInteractionCoroutine(room.GetScript(), "OnEnterRoomAfterFade");
				}
				UpdateRegions();
				room.GetInstance().GetRegionComponents().ForEach(delegate(RegionComponent item)
				{
					item.OnRoomLoaded();
				});
				m_cameraData.GetInstance().OnEnterRoom();
				SV.m_callEnterOnRestore = false;
				m_transitioning = false;
				m_restoring = false;
				SetAutoLoadScript(this, "OnEnterRoomAfterFade", coroutine != null, isWaitForFunction: false);
				if (coroutine != null)
				{
					yield return coroutine;
				}
				if (room != null && room.GetScript() != null)
				{
					SetAutoLoadScript(room, "OnEnterRoomAfterFade", onEnterRoom != null, isWaitForFunction: false);
					if (onEnterRoom != null)
					{
						yield return onEnterRoom;
					}
				}
				Unblock();
			}
			m_roomLoopStarted = true;
			Block();
			while (m_currentSequence != null)
			{
				yield return null;
			}
			Unblock();
			m_coroutineMainLoop = StartCoroutine(MainLoop());
		}

		private IEnumerator MainLoop()
		{
			while (true)
			{
				Block();
				bool yielded = false;
				if (!SystemTime.Paused)
				{
					if (m_currentSequence != null)
					{
						yield return CoroutineWaitForCurrentSequence();
					}
					while (m_queuedScriptInteractions.Count > 0)
					{
						m_currentSequence = m_queuedScriptInteractions[0];
						m_queuedScriptInteractions.RemoveAt(0);
						if (m_currentSequence != null)
						{
							yielded = true;
							yield return CoroutineWaitForCurrentSequence();
						}
					}
					m_queuedScriptInteractions.Clear();
					bool flag = !m_leftClickPrev && Input.GetMouseButton(0);
					bool flag2 = !m_rightClickPrev && Input.GetMouseButton(1);
					m_leftClickPrev = Input.GetMouseButton(0);
					m_rightClickPrev = Input.GetMouseButton(1);
					bool flag3 = false;
					if (GetModalGuiActive())
					{
						flag3 = true;
					}
					if (SV.m_captureInputSources.Count > 0)
					{
						flag3 = true;
					}
					if (m_walkClickDown)
					{
						if (!Input.GetMouseButton(0) || flag3)
						{
							m_walkClickDown = false;
						}
						else if ((m_player.Position - m_mousePos).magnitude > 10f)
						{
							m_player.WalkToBG(m_mousePos);
						}
						flag3 = true;
					}
					if (!flag3 && (flag || flag2) && m_globalScript != null)
					{
						MethodInfo method = m_globalScript.GetType().GetMethod(SCRIPT_FUNCTION_ONMOUSECLICK, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (method != null)
						{
							method.Invoke(m_globalScript, new object[2] { flag, flag2 });
						}
						else
						{
							OnMouseClick(flag, flag2);
						}
					}
					while (m_queuedScriptInteractions.Count > 0)
					{
						m_currentSequence = m_queuedScriptInteractions[0];
						m_queuedScriptInteractions.RemoveAt(0);
						if (m_currentSequence != null)
						{
							yielded = true;
							yield return CoroutineWaitForCurrentSequence();
						}
					}
					m_queuedScriptInteractions.Clear();
					if (StartScriptInteraction(this, "UpdateBlocking", null, stopPlayerMoving: false, cancelCurrentInteraction: true))
					{
						yielded = true;
						yield return CoroutineWaitForCurrentSequence();
					}
					if (m_currentRoom != null && StartScriptInteraction(m_currentRoom, "UpdateBlocking", null, stopPlayerMoving: false, cancelCurrentInteraction: true))
					{
						yielded = true;
						yield return CoroutineWaitForCurrentSequence();
					}
					while (m_queuedScriptInteractions.Count > 0)
					{
						m_currentSequence = m_queuedScriptInteractions[0];
						m_queuedScriptInteractions.RemoveAt(0);
						if (m_currentSequence != null)
						{
							yielded = true;
							yield return CoroutineWaitForCurrentSequence();
						}
					}
					m_queuedScriptInteractions.Clear();
					if (yielded && StartScriptInteraction(m_currentRoom, SCRIPT_FUNCTION_AFTERANYCLICK))
					{
						yielded = true;
						yield return CoroutineWaitForCurrentSequence();
					}
					if (m_currentRoom != null)
					{
						List<RegionComponent> regionComponents = m_currentRoom.GetInstance().GetRegionComponents();
						int regionCount = regionComponents.Count;
						int charId = 0;
						while (charId < m_characters.Count)
						{
							Character character = m_characters[charId];
							int num;
							for (int regionId = 0; regionId < regionCount; regionId = num)
							{
								RegionComponent regionComponent = regionComponents[regionId];
								Region data = regionComponent.GetData();
								RegionComponent.eTriggerResult eTriggerResult = regionComponent.UpdateCharacterOnRegionState(charId, background: false);
								if (data.Enabled && (!data.PlayerOnly || character == m_player))
								{
									switch (eTriggerResult)
									{
									case RegionComponent.eTriggerResult.Enter:
										if (StartScriptInteraction(m_currentRoom, SCRIPT_FUNCTION_ENTER_REGION + data.ScriptName, new object[2] { data, character }, stopPlayerMoving: false, cancelCurrentInteraction: true))
										{
											yielded = true;
											yield return CoroutineWaitForCurrentSequence();
										}
										break;
									case RegionComponent.eTriggerResult.Exit:
										if (StartScriptInteraction(m_currentRoom, SCRIPT_FUNCTION_EXIT_REGION + data.ScriptName, new object[2] { data, character }, stopPlayerMoving: false, cancelCurrentInteraction: true))
										{
											yielded = true;
											yield return CoroutineWaitForCurrentSequence();
										}
										break;
									}
								}
								num = regionId + 1;
							}
							num = charId + 1;
							charId = num;
						}
					}
				}
				Unblock();
				m_guiConsumedClick = false;
				if (m_skipCutscene)
				{
					OnEndCutscene();
				}
				if (!yielded && m_currentDialog != null)
				{
					GetGui(DialogTreeGui).Visible = true;
				}
				if (!yielded)
				{
					yield return new WaitForEndOfFrame();
				}
			}
		}

		private IEnumerator CoroutineWaitForCurrentSequence()
		{
			yield return m_currentSequence;
			m_currentSequence = null;
		}

		private void OnMouseClick(bool leftClick, bool rightClick)
		{
			if (m_player.HasActiveInventory && (rightClick || (GetMouseOverClickable() == null && leftClick) || Cursor.NoneCursorActive))
			{
				SystemAudio.Play("InventoryCursorClear");
				m_player.ActiveInventory = null;
			}
			else
			{
				if (m_cursor.NoneCursorActive)
				{
					return;
				}
				if (leftClick)
				{
					if (GetMouseOverClickable() != null)
					{
						if (m_player.HasActiveInventory && !m_cursor.InventoryCursorOverridden)
						{
							ProcessClick(eQuestVerb.Inventory);
						}
						else
						{
							ProcessClick(eQuestVerb.Use);
						}
					}
					else
					{
						ProcessClick(eQuestVerb.Walk);
					}
				}
				else if (rightClick && GetActionEnabled(eQuestVerb.Look) && GetMouseOverClickable() != null)
				{
					ProcessClick(eQuestVerb.Look);
				}
			}
		}

		public bool SaveSettings()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add(SAV_SETTINGS, m_settings);
			return m_saveManager.Save(SAV_SETTINGS_FILE, SAV_SETTINGS, SAV_SETTINGS_VER, dictionary);
		}

		public bool RestoreSettings()
		{
			Dictionary<string, object> data = null;
			int version = -1;
			bool num = m_saveManager.RestoreSave(SAV_SETTINGS_FILE, SAV_SETTINGS_VER_REQ, out version, out data);
			if (num && data != null)
			{
				if (data.ContainsKey(SAV_SETTINGS))
				{
					m_settings = data[SAV_SETTINGS] as QuestSettings;
				}
				m_settings.OnPostRestore(version);
			}
			return num;
		}

		public List<QuestSaveSlotData> GetSaveSlotData()
		{
			return m_saveManager.GetSaveSlotData();
		}

		public QuestSaveSlotData GetSaveSlotData(int slot)
		{
			return m_saveManager.GetSaveSlot(slot);
		}

		public QuestSaveSlotData GetLastSaveSlotData()
		{
			QuestSaveSlotData questSaveSlotData = null;
			foreach (QuestSaveSlotData saveSlotDatum in GetSaveSlotData())
			{
				if (questSaveSlotData == null || saveSlotDatum.m_timestamp > questSaveSlotData.m_timestamp)
				{
					questSaveSlotData = saveSlotDatum;
				}
			}
			return questSaveSlotData;
		}

		public bool Save(int slot, string description, Texture2D imageOverride = null)
		{
			if (m_restoring)
			{
				return false;
			}
			SaveSettings();
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			foreach (Character character in m_characters)
			{
				dictionary.Add("Char" + character.ScriptName, character);
			}
			foreach (Room room in m_rooms)
			{
				dictionary.Add("Room" + room.GetScriptName(), room);
			}
			foreach (Gui gui in m_guis)
			{
				dictionary.Add("Gui" + gui.GetScriptName(), gui);
			}
			foreach (Inventory inventoryItem in m_inventoryItems)
			{
				dictionary.Add("Inv" + inventoryItem.GetScriptName(), inventoryItem);
			}
			foreach (DialogTree dialogTree in m_dialogTrees)
			{
				dictionary.Add("Dlg" + dialogTree.GetScriptName(), dialogTree);
			}
			dictionary.Add("Global", m_globalScript);
			dictionary.Add("Camera", m_cameraData);
			dictionary.Add("Cursor", m_cursor);
			dictionary.Add("Audio", SingletonAuto<SystemAudio>.Get.GetSaveData());
			dictionary.Add("SV", m_savedVars);
			dictionary.Add("Extra", new ExtraSaveData
			{
				m_player = m_player.ScriptName,
				m_currentDialog = ((m_currentDialog != null) ? m_currentDialog.ScriptName : string.Empty),
				m_displayBoxGui = m_displayBoxGui,
				m_dialogTreeGui = m_dialogTreeGui,
				m_customSpeechGui = m_customSpeechGui,
				m_speechStyle = m_speechStyle,
				m_speechPortraitLocation = m_speechPortraitLocation,
				m_transitionFadeTime = m_transitionFadeTime
			});
			Texture2D texture2D = imageOverride;
			Camera camera = m_cameraData?.Camera;
			if (texture2D == null && camera != null && m_saveScreenshotHeight > 0)
			{
				int saveScreenshotHeight = m_saveScreenshotHeight;
				int width = Mathf.CeilToInt((float)saveScreenshotHeight * camera.aspect);
				RenderTexture active = RenderTexture.active;
				RenderTexture.active = new RenderTexture(width, saveScreenshotHeight, 16, RenderTextureFormat.ARGB32, 0);
				RenderTexture targetTexture = camera.targetTexture;
				camera.targetTexture = RenderTexture.active;
				new Texture2D(width, saveScreenshotHeight, TextureFormat.ARGB32, mipChain: false);
				camera.Render();
				texture2D = new Texture2D(camera.targetTexture.width, camera.targetTexture.height);
				texture2D.ReadPixels(new Rect(0f, 0f, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
				texture2D.Apply();
				RenderTexture.active = active;
				camera.targetTexture = targetTexture;
			}
			return m_saveManager.Save(slot, description, m_saveVersion, dictionary, texture2D);
		}

		public bool RestoreLastSave()
		{
			QuestSaveSlotData lastSaveSlotData = GetLastSaveSlotData();
			if (lastSaveSlotData == null)
			{
				return false;
			}
			return RestoreSave(lastSaveSlotData.m_slotId);
		}

		public bool RestoreSave(int slot)
		{
			if (GetSaveSlotData(slot) == null)
			{
				return false;
			}
			Dictionary<string, object> data = null;
			int version = -1;
			StopAllCoroutines();
			GetMenuManager().ResetFade();
			m_consumedInteraction = null;
			m_coroutineMainLoop = null;
			m_backgroundSequence = null;
			m_backgroundSequences.Clear();
			m_currentSequence = null;
			m_currentSequences.Clear();
			bool flag = m_saveManager.RestoreSave(slot, m_saveVersionRequired, out version, out data);
			m_restoredVersion = version;
			(new object[1])[0] = version;
			if (flag)
			{
				m_restoring = true;
				for (int i = 0; i < m_characters.Count; i++)
				{
					Character character = m_characters[i];
					string key = "Char" + character.GetScriptName();
					if (data.ContainsKey(key))
					{
						m_characters[i] = data[key] as Character;
					}
				}
				m_player = m_characters[0];
				for (int j = 0; j < m_rooms.Count; j++)
				{
					Room room = m_rooms[j];
					string key2 = "Room" + room.GetScriptName();
					if (data.ContainsKey(key2))
					{
						m_rooms[j] = data[key2] as Room;
					}
				}
				for (int k = 0; k < m_guis.Count; k++)
				{
					Gui gui = m_guis[k];
					MonoBehaviour instance = gui.Instance;
					string key3 = "Gui" + gui.GetScriptName();
					if (data.ContainsKey(key3))
					{
						m_guis[k] = data[key3] as Gui;
						if (instance != null)
						{
							m_guis[k].SetInstance(instance as GuiComponent);
						}
					}
				}
				for (int l = 0; l < m_inventoryItems.Count; l++)
				{
					Inventory inventory = m_inventoryItems[l];
					string key4 = "Inv" + inventory.GetScriptName();
					if (data.ContainsKey(key4))
					{
						m_inventoryItems[l] = data[key4] as Inventory;
					}
				}
				for (int m = 0; m < m_dialogTrees.Count; m++)
				{
					DialogTree dialogTree = m_dialogTrees[m];
					string key5 = "Dlg" + dialogTree.GetScriptName();
					if (data.ContainsKey(key5))
					{
						m_dialogTrees[m] = data[key5] as DialogTree;
					}
				}
				string key6 = "Global";
				if (data.ContainsKey(key6))
				{
					m_globalScript = data[key6] as GlobalScript;
				}
				string key7 = "Camera";
				if (data.ContainsKey(key7))
				{
					m_cameraData = data[key7] as QuestCamera;
				}
				string key8 = "Cursor";
				if (data.ContainsKey(key8))
				{
					m_cursor = data[key8] as QuestCursor;
				}
				string key9 = "Audio";
				if (data.ContainsKey(key9))
				{
					SingletonAuto<SystemAudio>.Get.RestoreSaveData(data[key9]);
				}
				string key10 = "SV";
				if (data.ContainsKey(key10))
				{
					m_savedVars = data[key10] as SavedVarCollection;
				}
				string key11 = "Extra";
				if (data.ContainsKey(key11))
				{
					ExtraSaveData extraSaveData = data[key11] as ExtraSaveData;
					SetPlayer(GetCharacter(extraSaveData.m_player));
					m_currentDialog = GetDialogTree(extraSaveData.m_currentDialog);
					m_displayBoxGui = extraSaveData.m_displayBoxGui;
					m_dialogTreeGui = extraSaveData.m_dialogTreeGui;
					m_customSpeechGui = extraSaveData.m_customSpeechGui;
					m_speechStyle = extraSaveData.m_speechStyle;
					m_speechPortraitLocation = extraSaveData.m_speechPortraitLocation;
					m_transitionFadeTime = extraSaveData.m_transitionFadeTime;
				}
				if (Application.isEditor && m_hotLoadAssembly != null)
				{
					foreach (IQuestScriptable allScriptable in GetAllScriptables())
					{
						allScriptable.HotLoadScript(m_hotLoadAssembly);
					}
				}
				for (int n = 0; n < m_characters.Count; n++)
				{
					m_characters[n].OnPostRestore(version, m_characterPrefabs[n].gameObject);
				}
				for (int num = 0; num < m_rooms.Count; num++)
				{
					m_rooms[num].OnPostRestore(version, m_roomPrefabs[num].gameObject);
				}
				for (int num2 = 0; num2 < m_guis.Count; num2++)
				{
					m_guis[num2].OnPostRestore(version, m_guiPrefabs[num2].gameObject);
				}
				for (int num3 = 0; num3 < m_inventoryItems.Count; num3++)
				{
					m_inventoryItems[num3].OnPostRestore(version, m_inventoryPrefabs[num3].gameObject);
				}
				for (int num4 = 0; num4 < m_dialogTrees.Count; num4++)
				{
					m_dialogTrees[num4].OnPostRestore(version, m_dialogTreePrefabs[num4].gameObject);
				}
				QuestCameraComponent questCameraComponent = UnityEngine.Object.FindObjectOfType<QuestCameraComponent>();
				if (questCameraComponent != null)
				{
					m_cameraData.SetInstance(questCameraComponent);
				}
				m_cursor.OnPostRestore(version, m_cursorPrefab.gameObject);
				UpdateRegions();
				if (m_currentRoom != null)
				{
					m_currentRoom.GetInstance().GetRegionComponents().ForEach(delegate(RegionComponent item)
					{
						item.OnRoomLoaded();
					});
				}
				m_saveManager.OnPostRestore();
				Unblock();
				StartRoomTransition((Room)GetPlayer().Room, force: true);
			}
			Unblock();
			return flag;
		}

		public bool DeleteSave(int slot)
		{
			return m_saveManager.DeleteSave(slot);
		}

		public void AddSaveData(string name, object data, Action OnPostRestore = null)
		{
			m_saveManager.AddSaveData(name, data, OnPostRestore);
		}

		public void RemoveSaveData(string name)
		{
			m_saveManager.RemoveSaveData(name);
		}

		private void CallScriptPostRestore(IQuestScriptable scriptable, object[] onPostRestoreParams)
		{
			if (scriptable.GetScript() != null)
			{
				MethodInfo method = scriptable.GetScript().GetType().GetMethod(STR_ON_POST_RESTORE, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (method != null)
				{
					method.Invoke(scriptable.GetScript(), onPostRestoreParams);
				}
			}
		}
	}
}
