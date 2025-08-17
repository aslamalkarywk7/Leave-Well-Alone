using System;
using System.Collections.Generic;
using UnityEngine;

namespace PowerTools
{
	public class PowerSpriteImport : ScriptableObject
	{
		[Serializable]
		public class AnimImportData
		{
			public string m_name = string.Empty;

			public int m_firstFrame;

			public bool m_loop;

			public bool m_fullRect;

			public int m_length = 1;

			public int[] m_frameDurations;
		}

		public enum eTextureCompression
		{
			None = 0,
			Low = 1,
			Normal = 2,
			High = 3
		}

		public List<AnimImportData> m_animations = new List<AnimImportData>();

		public float m_pixelsPerUnit = 1f;

		public FilterMode m_filterMode;

		public eTextureCompression m_compression;

		public SpriteMeshType m_spriteMeshType = SpriteMeshType.Tight;

		public bool m_crunchedCompression;

		public string m_sourcePSD = string.Empty;

		public string m_sourceDirectory = string.Empty;

		public bool m_deleteImportedPngs = true;

		public string m_spriteDirectory = "Sprites";

		public bool m_gui;

		public bool m_isAseprite;

		public bool m_trimSprites;

		public bool m_createSingleSpriteAnims = true;

		public string[] m_importLayers;

		public string[] m_ignoreLayers = new string[2] { "Guide", "Ignore" };

		[Multiline]
		public string m_notes = string.Empty;
	}
}
