using System.Collections.Generic;
using UnityEngine;

namespace PowerTools.Quest
{
	public interface IRoom
	{
		RoomComponent Instance { get; }

		string Description { get; }

		string ScriptName { get; }

		bool Active { get; set; }

		bool Current { get; set; }

		bool Visited { get; }

		bool FirstTimeVisited { get; }

		int TimesVisited { get; }

		int ActiveWalkableArea { get; set; }

		bool PlayerVisible { get; set; }

		RectCentered Bounds { get; set; }

		RectCentered ScrollBounds { get; set; }

		float VerticalResolution { get; set; }

		float Zoom { get; set; }

		Room Data { get; }

		void EnterBG();

		Coroutine Enter();

		Hotspot GetHotspot(string name);

		Prop GetProp(string name);

		Region GetRegion(string name);

		Vector2 GetPoint(string name);

		void SetPoint(string name, Vector2 location);

		void SetPoint(string name, string fromPosition);

		List<Hotspot> GetHotspots();

		List<Prop> GetProps();

		T GetScript<T>() where T : RoomScript<T>;
	}
}
