using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

namespace PowerTools.Quest
{
	[Serializable]
	public class Room : IQuestScriptable, IRoom, IQuestSaveCachable
	{
		[Serializable]
		public class RoomPoint
		{
			public string m_name = "Point";

			public Vector2 m_position = Vector2.zero;

			public static implicit operator Vector2(RoomPoint self)
			{
				return self.m_position;
			}
		}

		[Tooltip("Currently not used")]
		[HideInInspector]
		[SerializeField]
		private string m_description = "New Room";

		[Tooltip("When false, the player is hidden and can't walk")]
		[SerializeField]
		private bool m_playerVisible = true;

		[Tooltip("The vertical resolution of this room. Set Non-zero to override the default set in PowerQuest. (How many pixels high the camera view should be)")]
		[SerializeField]
		private float m_verticalResolution;

		[Tooltip("The walkable area that's currently enabled")]
		[SerializeField]
		private int m_activeWalkableArea;

		[Tooltip("Defines the bounds of the room, the camera will not go outside these bounds")]
		[SerializeField]
		private RectCentered m_bounds = new RectCentered(0f, 0f, 0f, 0f);

		[Tooltip("Defines the area in which the camera will track the player (0 to disable)")]
		[SerializeField]
		private RectCentered m_scrollBounds = new RectCentered(0f, 0f, 0f, 0f);

		[ReadOnly]
		[SerializeField]
		private string m_shortName = "New";

		[ReadOnly]
		[SerializeField]
		private string m_scriptClass = "RoomNew";

		[ReadOnly]
		[SerializeField]
		private string m_sceneName = "SceneNew";

		[ReadOnly]
		[SerializeField]
		private List<RoomPoint> m_points = new List<RoomPoint>();

		private List<Hotspot> m_hotspots = new List<Hotspot>();

		private List<Prop> m_props = new List<Prop>();

		private List<Region> m_regions = new List<Region>();

		private RoomComponent m_instance;

		private QuestScript m_script;

		private GameObject m_prefab;

		private int m_timesVisited;

		private bool m_saveDirty = true;

		public RoomComponent Instance => m_instance;

		public string Description => m_description;

		public string ScriptName => m_shortName;

		public bool Active
		{
			get
			{
				return Singleton<PowerQuest>.Get.GetCurrentRoom() == this;
			}
			set
			{
				if (value)
				{
					Singleton<PowerQuest>.Get.ChangeRoomBG(this);
				}
				else
				{
					Debug.LogError("Can't set Room.Active to false, move to another room instead");
				}
			}
		}

		public bool Current
		{
			get
			{
				return Active;
			}
			set
			{
				Active = value;
			}
		}

		public bool Visited => m_timesVisited > 0;

		public bool FirstTimeVisited => m_timesVisited == 1;

		public int TimesVisited => m_timesVisited;

		public RectCentered Bounds
		{
			get
			{
				return m_bounds;
			}
			set
			{
				m_bounds = value;
			}
		}

		public RectCentered ScrollBounds
		{
			get
			{
				return m_scrollBounds;
			}
			set
			{
				m_scrollBounds = value;
			}
		}

		public int ActiveWalkableArea
		{
			get
			{
				return m_activeWalkableArea;
			}
			set
			{
				if (m_activeWalkableArea != value)
				{
					m_activeWalkableArea = value;
					if (m_instance != null)
					{
						m_instance.SetActiveWalkableArea(m_activeWalkableArea);
					}
				}
			}
		}

		public bool PlayerVisible
		{
			get
			{
				return m_playerVisible;
			}
			set
			{
				if (m_playerVisible != value)
				{
					m_playerVisible = value;
					Character player = Singleton<PowerQuest>.Get.GetPlayer();
					if (player != null && player.Instance != null)
					{
						(player.Instance as CharacterComponent).UpdateEnabled();
					}
				}
			}
		}

		public float VerticalResolution
		{
			get
			{
				return m_verticalResolution;
			}
			set
			{
				m_verticalResolution = value;
			}
		}

		public float Zoom
		{
			get
			{
				if (!(m_verticalResolution > 0f))
				{
					return 1f;
				}
				return Singleton<PowerQuest>.Get.DefaultVerticalResolution / m_verticalResolution;
			}
			set
			{
				m_verticalResolution = Singleton<PowerQuest>.Get.DefaultVerticalResolution * value;
			}
		}

		public Room Data => this;

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

		public void EnterBG()
		{
			Singleton<PowerQuest>.Get.ChangeRoomBG(this);
		}

		public Coroutine Enter()
		{
			return Singleton<PowerQuest>.Get.ChangeRoom(this);
		}

		public void DebugSetVisited(int times)
		{
			m_timesVisited = times;
		}

		public GameObject GetPrefab()
		{
			return m_prefab;
		}

		public string GetScriptName()
		{
			return m_shortName;
		}

		public string GetScriptClassName()
		{
			return m_scriptClass;
		}

		public QuestScript GetScript()
		{
			return m_script;
		}

		public IQuestScriptable GetScriptable()
		{
			return this;
		}

		public T GetScript<T>() where T : RoomScript<T>
		{
			if (m_script == null)
			{
				return null;
			}
			return m_script as T;
		}

		public void HotLoadScript(Assembly assembly)
		{
			QuestUtils.HotSwapScript(ref m_script, m_scriptClass, assembly);
		}

		public RoomComponent GetInstance()
		{
			return m_instance;
		}

		public void SetInstance(RoomComponent roomInstance)
		{
			m_instance = roomInstance;
			m_instance.SetData(this);
			m_timesVisited++;
			HotspotComponent[] componentsInChildren = m_instance.GetComponentsInChildren<HotspotComponent>(includeInactive: true);
			foreach (HotspotComponent hotspotInstance in componentsInChildren)
			{
				m_hotspots.Find((Hotspot item) => item.ScriptName == hotspotInstance.GetData().ScriptName).SetInstance(hotspotInstance);
			}
			PropComponent[] componentsInChildren2 = m_instance.GetComponentsInChildren<PropComponent>(includeInactive: true);
			foreach (PropComponent propInstance in componentsInChildren2)
			{
				m_props.Find((Prop item) => item.ScriptName == propInstance.GetData().ScriptName).SetInstance(propInstance);
			}
			RegionComponent[] componentsInChildren3 = m_instance.GetComponentsInChildren<RegionComponent>(includeInactive: true);
			foreach (RegionComponent regionInstance in componentsInChildren3)
			{
				m_regions.Find((Region item) => item.ScriptName == regionInstance.GetData().ScriptName).SetInstance(regionInstance);
			}
			m_instance.SetActiveWalkableArea(m_activeWalkableArea);
		}

		public Hotspot GetHotspot(string name)
		{
			Hotspot hotspot = QuestUtils.FindScriptable(m_hotspots, name);
			if (hotspot == null)
			{
				Debug.LogError("Hotspot '" + name + "' doesn't exist in " + ScriptName);
			}
			return hotspot;
		}

		public Prop GetProp(string name)
		{
			Prop prop = QuestUtils.FindScriptable(m_props, name);
			if (prop == null)
			{
				Debug.LogError("Prop '" + name + "' doesn't exist in " + ScriptName);
			}
			return prop;
		}

		public Region GetRegion(string name)
		{
			Region region = QuestUtils.FindScriptable(m_regions, name);
			if (region == null)
			{
				Debug.LogError("Region '" + name + "' doesn't exist in " + ScriptName);
			}
			return region;
		}

		public Vector2 GetPoint(string name)
		{
			return FindPoint(name).m_position;
		}

		public void SetPoint(string name, Vector2 position)
		{
			FindPoint(name).m_position = position;
		}

		public void SetPoint(string name, string fromPoint)
		{
			FindPoint(name).m_position = GetPoint(fromPoint);
		}

		public void SetSize(RectCentered size)
		{
			m_bounds = size;
		}

		public void SetScrollSize(RectCentered size)
		{
			m_scrollBounds = size;
		}

		public string GetSceneName()
		{
			return m_sceneName;
		}

		public List<Hotspot> GetHotspots()
		{
			return m_hotspots;
		}

		public List<Prop> GetProps()
		{
			return m_props;
		}

		public List<RoomPoint> GetPoints()
		{
			if (m_points == null)
			{
				m_points = new List<RoomPoint>();
			}
			return m_points;
		}

		public List<Region> GetRegions()
		{
			return m_regions;
		}

		public static implicit operator string(Room room)
		{
			return room.m_shortName;
		}

		public void EditorInitialise(string name)
		{
			m_shortName = name;
			m_description = name;
			m_scriptClass = "Room" + name;
			m_sceneName = "SceneRoom" + name;
		}

		public void EditorRename(string name)
		{
			m_shortName = name;
			m_scriptClass = "Room" + name;
			m_sceneName = "SceneRoom" + name;
		}

		public void OnPostRestore(int version, GameObject prefab)
		{
			m_prefab = prefab;
			if (m_script == null)
			{
				m_script = QuestUtils.ConstructByName<QuestScript>(m_scriptClass);
			}
			List<Hotspot> hotspots = m_hotspots;
			m_hotspots = new List<Hotspot>();
			HotspotComponent[] componentsInChildren = prefab.GetComponentsInChildren<HotspotComponent>(includeInactive: true);
			foreach (HotspotComponent prefabComponent in componentsInChildren)
			{
				Hotspot hotspot = hotspots.Find((Hotspot item) => item.ScriptName == prefabComponent.GetData().ScriptName);
				Hotspot hotspot2 = new Hotspot();
				QuestUtils.CopyFields(hotspot2, (hotspot != null) ? hotspot : prefabComponent.GetData());
				m_hotspots.Add(hotspot2);
			}
			List<Prop> props = m_props;
			m_props = new List<Prop>();
			PropComponent[] componentsInChildren2 = prefab.GetComponentsInChildren<PropComponent>(includeInactive: true);
			foreach (PropComponent prefabComponent2 in componentsInChildren2)
			{
				Prop prop = props.Find((Prop item) => item.ScriptName == prefabComponent2.GetData().ScriptName);
				Prop prop2 = new Prop();
				QuestUtils.CopyFields(prop2, (prop != null) ? prop : prefabComponent2.GetData());
				m_props.Add(prop2);
			}
			List<Region> regions = m_regions;
			m_regions = new List<Region>();
			RegionComponent[] componentsInChildren3 = prefab.GetComponentsInChildren<RegionComponent>(includeInactive: true);
			foreach (RegionComponent prefabComponent3 in componentsInChildren3)
			{
				Region region = regions.Find((Region item) => item.ScriptName == prefabComponent3.GetData().ScriptName);
				Region region2 = new Region();
				QuestUtils.CopyFields(region2, (region != null) ? region : prefabComponent3.GetData());
				m_regions.Add(region2);
			}
			SaveDirty = Active;
		}

		public void Initialise(GameObject prefab)
		{
			m_prefab = prefab;
			m_script = QuestUtils.ConstructByName<QuestScript>(m_scriptClass);
			m_hotspots.Clear();
			HotspotComponent[] componentsInChildren = prefab.GetComponentsInChildren<HotspotComponent>(includeInactive: true);
			foreach (HotspotComponent hotspotComponent in componentsInChildren)
			{
				Hotspot hotspot = new Hotspot();
				QuestUtils.CopyFields(hotspot, hotspotComponent.GetData());
				m_hotspots.Add(hotspot);
			}
			m_props.Clear();
			PropComponent[] componentsInChildren2 = prefab.GetComponentsInChildren<PropComponent>(includeInactive: true);
			foreach (PropComponent propComponent in componentsInChildren2)
			{
				Prop prop = new Prop();
				QuestUtils.CopyFields(prop, propComponent.GetData());
				m_props.Add(prop);
				prop.Position = propComponent.transform.position;
			}
			m_regions.Clear();
			RegionComponent[] componentsInChildren3 = prefab.GetComponentsInChildren<RegionComponent>(includeInactive: true);
			foreach (RegionComponent regionComponent in componentsInChildren3)
			{
				Region region = new Region();
				QuestUtils.CopyFields(region, regionComponent.GetData());
				m_regions.Add(region);
			}
			m_points = QuestUtils.CopyListFields(m_points);
		}

		private RoomPoint FindPoint(string name)
		{
			RoomPoint roomPoint = m_points.Find((RoomPoint pos) => string.Equals(pos.m_name, name, StringComparison.OrdinalIgnoreCase));
			if (roomPoint == null)
			{
				Debug.LogError("Position '" + name + "' doesn't exist in " + ScriptName);
				return new RoomPoint();
			}
			return roomPoint;
		}

		[OnDeserializing]
		private void CopyDefaults(StreamingContext sc)
		{
			QuestUtils.InitWithDefaults(this);
		}
	}
}
