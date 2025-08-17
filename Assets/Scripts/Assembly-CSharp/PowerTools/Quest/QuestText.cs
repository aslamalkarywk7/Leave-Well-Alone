using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PowerTools.Quest.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerTools.Quest
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(TextMesh))]
	public class QuestText : MonoBehaviour
	{
		[Serializable]
		public class TextSpriteData
		{
			public string m_tag = string.Empty;

			public float m_offsetY;

			public Sprite m_sprite;

			public string m_platform = string.Empty;

			public override string ToString()
			{
				return m_tag;
			}
		}

		private static readonly string STR_SHADER_PIXEL = "Powerhoof/Pixel Text Shader";

		private static readonly string STR_SHADER = "GUI/Text Shader";

		private static readonly Vector3[] SHADOW_OFFSETS = new Vector3[8]
		{
			Vector3.up,
			Vector3.down,
			Vector3.left,
			Vector3.right,
			Vector3.up + Vector3.left,
			Vector3.up + Vector3.right,
			Vector3.down + Vector3.left,
			Vector3.down + Vector3.right
		};

		private static readonly string STR_COLOR_START = "<color=#fff0>";

		private static readonly string STR_COLOR_END = "</color>";

		[Multiline]
		[SerializeField]
		private string m_text = "";

		[SerializeField]
		private bool m_localize;

		[SerializeField]
		private string m_sortingLayer = "Default";

		[SerializeField]
		private int m_orderInLayer;

		[Tooltip("Width in game units to wrap text (0 = disabled)")]
		[SerializeField]
		[Delayed]
		private float m_wrapWidth;

		[Tooltip("Whether to cut the text (adding ellipsis) instead of wrapping it")]
		[SerializeField]
		private bool m_truncate;

		[Tooltip("Whether to use more expensive word wrap that tries to keep uniform line width")]
		[SerializeField]
		private bool m_wrapUniformLineWidth;

		[Tooltip("Min line width when uniform line width is used, or Keep On Screen is set and the width is shortened. (0 = disabled)")]
		[SerializeField]
		[Delayed]
		private float m_wrapWidthMin;

		[Tooltip("Ensures the dialog is on screen when created (eg. offscreen character dialog)")]
		[SerializeField]
		private bool m_keepOnScreen;

		[SerializeField]
		private Padding m_screenPadding = new Padding(8f, 8f, 8f, 8f);

		[SerializeField]
		private Shader m_shaderOverride;

		[Tooltip("If true, pixel filtering is applied for pixel art games, otherwise bilinear.")]
		[SerializeField]
		private bool m_setFiltering = true;

		[SerializeField]
		private TextOutline m_outline;

		[Tooltip("Set for a typewriter effect, in characters per second. 0.05 is a good starting value.")]
		[SerializeField]
		private float m_typeSpeed;

		private static Shader s_shader = null;

		private bool m_materialSet;

		private TextMesh m_mesh;

		private TextWrapper m_textWrapper;

		private MeshRenderer m_meshRenderer;

		private Transform m_attachObject;

		private Vector2 m_attachObjOffset = Vector2.zero;

		private Vector2 m_attachWorldPos = Vector2.zero;

		private Vector2 m_attachOffset = Vector2.zero;

		private bool m_editorRefresh;

		[SerializeField]
		[HideInInspector]
		private string m_unlocalizedText;

		private string m_wrappedText;

		[SerializeField]
		[HideInInspector]
		private List<TextMesh> m_outlineMeshes;

		private bool m_wasRendererEnabled = true;

		private Vector2 m_rectSize = Vector2.zero;

		private int m_typeChar;

		private static string s_textSpritePlatform = string.Empty;

		public Action CallbackOnTypeCharacter;

		private List<Material> m_tempMaterials = new List<Material>();

		private static readonly Regex REGEX_TAG = new Regex("\\[(\\w*)\\]");

		private static readonly string TAG_QUAD = "<quad material={0} size={1} x={2} y={3} width={4} height={5} />";

		public string text
		{
			get
			{
				return m_text;
			}
			set
			{
				SetText(value);
			}
		}

		public Color color
		{
			get
			{
				if (!CheckTextMesh())
				{
					return Color.white;
				}
				return m_mesh.color;
			}
			set
			{
				if (!CheckTextMesh())
				{
					return;
				}
				m_mesh.color = value;
				foreach (TextMesh outlineMesh in m_outlineMeshes)
				{
					if (outlineMesh != null)
					{
						outlineMesh.color = outlineMesh.color.WithAlpha(color.a * m_outline.m_color.a);
					}
				}
			}
		}

		public TextAlignment alignment
		{
			get
			{
				return m_mesh?.alignment ?? TextAlignment.Center;
			}
			set
			{
				if (m_mesh != null)
				{
					m_mesh.alignment = value;
				}
				RefreshText();
			}
		}

		public TextAnchor anchor
		{
			get
			{
				return m_mesh?.anchor ?? TextAnchor.MiddleCenter;
			}
			set
			{
				if (m_mesh != null)
				{
					m_mesh.anchor = value;
				}
				RefreshText();
			}
		}

		public float WrapWidth
		{
			get
			{
				return m_wrapWidth;
			}
			set
			{
				m_wrapWidth = value;
			}
		}

		public bool Truncate
		{
			get
			{
				return m_truncate;
			}
			set
			{
				m_truncate = value;
			}
		}

		public bool WrapUniformLineWidth
		{
			get
			{
				return m_wrapUniformLineWidth;
			}
			set
			{
				m_wrapUniformLineWidth = value;
			}
		}

		public string SortingLayer
		{
			get
			{
				return m_sortingLayer;
			}
			set
			{
				m_sortingLayer = value;
			}
		}

		public int OrderInLayer
		{
			get
			{
				return m_orderInLayer;
			}
			set
			{
				if (m_orderInLayer == value)
				{
					return;
				}
				m_orderInLayer = value;
				if ((bool)m_meshRenderer)
				{
					m_meshRenderer.sortingOrder = m_orderInLayer;
				}
				if (m_outlineMeshes == null)
				{
					return;
				}
				foreach (TextMesh outlineMesh in m_outlineMeshes)
				{
					if (outlineMesh != null)
					{
						outlineMesh.GetComponent<Renderer>().sortingOrder = m_orderInLayer;
					}
				}
			}
		}

		public bool GetShouldLocalize()
		{
			return m_localize;
		}

		public float GetWrapWidth()
		{
			return m_wrapWidth;
		}

		public void SetWrapWidth(float width)
		{
			m_wrapWidth = width;
			RefreshText();
		}

		public TextOutline GetOutline()
		{
			return m_outline;
		}

		public void SetOutline(TextOutline outline)
		{
			if (m_outline != null && (outline == null || m_outline.m_directions != outline.m_directions) && m_outlineMeshes != null)
			{
				for (int i = 0; i < m_outlineMeshes.Count; i++)
				{
					if (m_outlineMeshes[i] != null)
					{
						if (Application.isPlaying)
						{
							UnityEngine.Object.Destroy(m_outlineMeshes[i].gameObject);
						}
						else
						{
							UnityEngine.Object.DestroyImmediate(m_outlineMeshes[i].gameObject);
						}
					}
				}
				m_outlineMeshes.Clear();
			}
			m_outline = outline;
			RefreshText();
		}

		public string GetUnlocalizedText()
		{
			if (m_localize && !string.IsNullOrEmpty(m_unlocalizedText))
			{
				return m_unlocalizedText;
			}
			return m_text;
		}

		public static void SetTextSpritePlatform(string platform)
		{
			s_textSpritePlatform = platform;
		}

		private void Start()
		{
			if (CheckTextMesh())
			{
				CheckMaterial();
				RefreshText();
			}
		}

		public void OnLanguageChange()
		{
			RefreshText();
		}

		public void SetText(string text)
		{
			if (text == null)
			{
				text = string.Empty;
			}
			m_unlocalizedText = text;
			text = SystemText.Localize(text);
			text = ParseImages(text);
			m_text = text;
			m_typeChar = 0;
			if (!CheckMaterial())
			{
				return;
			}
			m_meshRenderer.sortingOrder = m_orderInLayer;
			int num = UnityEngine.SortingLayer.NameToID(m_sortingLayer);
			if (UnityEngine.SortingLayer.IsValid(num))
			{
				m_meshRenderer.sortingLayerID = num;
			}
			if (m_wrapWidth > 0f)
			{
				if (m_textWrapper == null)
				{
					m_textWrapper = new TextWrapper(m_mesh);
				}
				if (!m_keepOnScreen)
				{
					text = (m_truncate ? m_textWrapper.Truncate(m_text, m_wrapWidth) : ((!m_wrapUniformLineWidth) ? m_textWrapper.WrapText(m_text, m_wrapWidth) : m_textWrapper.WrapTextMinimiseWidth(m_text, m_wrapWidth, m_wrapWidthMin)));
				}
				else
				{
					float num2 = m_wrapWidth;
					RectCentered rectCentered = new RectCentered(base.transform.position, base.transform.position);
					RectCentered rectCentered2 = default(RectCentered);
					if (Application.isPlaying && Singleton<PowerQuest>.Exists)
					{
						Camera cameraGui = Singleton<PowerQuest>.Get.GetCameraGui();
						rectCentered.Width = m_wrapWidth;
						if (m_mesh.anchor == TextAnchor.LowerLeft || m_mesh.anchor == TextAnchor.MiddleLeft || m_mesh.anchor == TextAnchor.UpperLeft)
						{
							rectCentered.CenterX = rectCentered.MaxX;
						}
						else if (m_mesh.anchor == TextAnchor.LowerRight || m_mesh.anchor == TextAnchor.MiddleRight || m_mesh.anchor == TextAnchor.UpperRight)
						{
							rectCentered.CenterX = rectCentered.MaxX;
						}
						rectCentered2 = new RectCentered(cameraGui.transform.position.x, cameraGui.transform.position.y, cameraGui.orthographicSize * 2f * cameraGui.aspect, cameraGui.orthographicSize * 2f);
						rectCentered2.RemovePadding(m_screenPadding);
						if (m_wrapWidthMin > 0f)
						{
							if (rectCentered.MinX < rectCentered2.MinX)
							{
								num2 -= rectCentered2.MinX - rectCentered.MinX;
							}
							if (rectCentered.MaxX > rectCentered2.MaxX)
							{
								num2 -= rectCentered.MaxX - rectCentered2.MaxX;
							}
							num2 = Mathf.Max(m_wrapWidthMin + 1f, num2);
						}
					}
					text = (m_truncate ? m_textWrapper.Truncate(m_text, num2) : ((!m_wrapUniformLineWidth) ? m_textWrapper.WrapText(m_text, num2) : m_textWrapper.WrapTextMinimiseWidth(m_text, num2, m_wrapWidthMin)));
					m_mesh.text = text;
					if (Application.isPlaying)
					{
						rectCentered = new RectCentered(m_textWrapper.Bounds);
						Vector2 zero = Vector2.zero;
						if (rectCentered.MinX < rectCentered2.MinX)
						{
							zero.x += rectCentered2.MinX - rectCentered.MinX;
						}
						if (rectCentered.MaxX > rectCentered2.MaxX)
						{
							zero.x += rectCentered2.MaxX - rectCentered.MaxX;
						}
						if (rectCentered.MinY < rectCentered2.MinY)
						{
							zero.y += rectCentered2.MinY - rectCentered.MinY;
						}
						if (rectCentered.MaxY > rectCentered2.MaxY)
						{
							zero.y += rectCentered2.MaxY - rectCentered.MaxY;
						}
						if (m_attachWorldPos != Vector2.zero && Singleton<PowerQuest>.Exists)
						{
							m_attachOffset = Utils.SnapRound(zero, Singleton<PowerQuest>.Get.SnapAmount);
							LateUpdate();
						}
						else
						{
							base.transform.Translate(zero.WithZ(0f));
						}
					}
					m_rectSize = rectCentered.Size;
				}
			}
			m_wrappedText = text;
			text = ProcessTypedText(text);
			UpdateOutline(text);
			m_mesh.text = text;
		}

		private void UpdateOutline(string text)
		{
			if (m_outline == null || m_outline.m_directions == 0 || !base.gameObject.scene.IsValid())
			{
				return;
			}
			for (int num = m_outlineMeshes.Count - 1; num >= 0; num--)
			{
				if (m_outlineMeshes[num] == null)
				{
					m_outlineMeshes.RemoveAt(num);
				}
			}
			int num2 = 0;
			for (int i = 0; i < 8; i++)
			{
				if (PowerTools.Quest.Text.BitMask.IsSet(m_outline.m_directions, i))
				{
					if (m_outlineMeshes == null)
					{
						m_outlineMeshes = new List<TextMesh>();
					}
					if (num2 >= m_outlineMeshes.Count)
					{
						GameObject obj = new GameObject(((TextOutline.eDirection)(1 << i)/*cast due to .constrained prefix*/).ToString(), m_mesh.GetType());
						obj.hideFlags = HideFlags.HideAndDontSave;
						obj.transform.parent = base.transform;
						obj.transform.localScale = Vector3.one;
						obj.transform.localPosition = SHADOW_OFFSETS[i] * m_outline.m_width;
						obj.layer = base.gameObject.layer;
						MeshRenderer component = obj.GetComponent<MeshRenderer>();
						component.sharedMaterial = m_meshRenderer.sharedMaterial;
						component.shadowCastingMode = ShadowCastingMode.Off;
						component.receiveShadows = false;
						component.sortingLayerID = m_meshRenderer.sortingLayerID;
						component.sortingLayerName = m_meshRenderer.sortingLayerName;
						component.sortingOrder = m_meshRenderer.sortingOrder;
						TextMesh component2 = obj.GetComponent<TextMesh>();
						component2.offsetZ = m_mesh.offsetZ + 0.1f;
						m_outlineMeshes.Add(component2);
					}
					TextMesh textMesh = m_outlineMeshes[num2];
					textMesh.text = text;
					textMesh.color = m_outline.m_color.WithAlpha(m_outline.m_color.a * color.a);
					textMesh.alignment = m_mesh.alignment;
					textMesh.anchor = m_mesh.anchor;
					textMesh.characterSize = m_mesh.characterSize;
					textMesh.font = m_mesh.font;
					textMesh.fontSize = m_mesh.fontSize;
					textMesh.fontStyle = m_mesh.fontStyle;
					textMesh.richText = m_mesh.richText;
					textMesh.lineSpacing = m_mesh.lineSpacing;
					textMesh.tabSize = m_mesh.tabSize;
					num2++;
				}
			}
		}

		public void AttachTo(Vector2 worldPosition)
		{
			m_attachOffset = Vector2.zero;
			m_attachWorldPos = worldPosition;
			m_attachObject = null;
			m_attachObjOffset = Vector2.zero;
			LateUpdate();
		}

		public void AttachTo(Transform obj, Vector2 worldPosition)
		{
			if (obj == null)
			{
				AttachTo(worldPosition);
				return;
			}
			m_attachOffset = Vector2.zero;
			m_attachWorldPos = worldPosition;
			m_attachObject = obj;
			m_attachObjOffset = worldPosition - (Vector2)obj.transform.position;
			LateUpdate();
		}

		private void OnValidate()
		{
			if (base.gameObject.activeInHierarchy)
			{
				m_editorRefresh = true;
			}
		}

		private void RefreshText()
		{
			SetText(GetUnlocalizedText());
		}

		private void EditorUpdate()
		{
			SetText(m_text);
		}

		private void Update()
		{
			UpdateTyping();
			if (!m_editorRefresh || !base.gameObject.activeInHierarchy || !Application.isEditor || Application.isPlaying)
			{
				return;
			}
			if (m_outline != null && m_outlineMeshes != null)
			{
				for (int i = 0; i < m_outlineMeshes.Count; i++)
				{
					if (m_outlineMeshes[i] != null)
					{
						UnityEngine.Object.DestroyImmediate(m_outlineMeshes[i].gameObject);
					}
				}
				m_outlineMeshes.Clear();
			}
			EditorUpdate();
			m_editorRefresh = false;
		}

		private void LateUpdate()
		{
			if (!CheckMaterial())
			{
				return;
			}
			if (m_attachObject != null || m_attachWorldPos != Vector2.zero)
			{
				if (!Singleton<PowerQuest>.Exists || Singleton<PowerQuest>.Get.GetCamera() == null || Singleton<PowerQuest>.Get.GetCameraGui() == null)
				{
					return;
				}
				QuestCamera camera = Singleton<PowerQuest>.Get.GetCamera();
				Camera component = camera.GetInstance().GetComponent<Camera>();
				Camera cameraGui = Singleton<PowerQuest>.Get.GetCameraGui();
				if (m_attachObject != null)
				{
					m_attachWorldPos = m_attachObjOffset + Utils.SnapRound(m_attachObject.position, Singleton<PowerQuest>.Get.SnapAmount);
				}
				Vector2 vector = (Vector2)component.transform.position - camera.GetPosition();
				Vector2 position = (Vector2)cameraGui.ViewportToWorldPoint(component.WorldToViewportPoint(m_attachWorldPos)) + m_attachOffset + vector;
				position = GetOnScreenPosition(position);
				if (Singleton<PowerQuest>.Get.GetSnapToPixel())
				{
					float snapTo = Singleton<PowerQuest>.Get.SnapAmount * Mathf.Max(camera.GetZoom(), 1f);
					base.transform.position = Utils.SnapRound(position, snapTo) + new Vector2(0.001f, 0.001f);
					Vector2 vector2 = position - (Vector2)base.transform.position;
					m_meshRenderer.material.SetVector("_Offset", vector2);
				}
				else
				{
					base.transform.position = position;
				}
			}
			if (m_wasRendererEnabled == m_meshRenderer.enabled)
			{
				return;
			}
			foreach (TextMesh outlineMesh in m_outlineMeshes)
			{
				if (outlineMesh != null)
				{
					outlineMesh.GetComponent<Renderer>().enabled = m_meshRenderer.enabled;
				}
			}
			m_wasRendererEnabled = m_meshRenderer.enabled;
		}

		public void StartTyping(float speedOverride = -1f)
		{
			m_typeChar = 0;
			if (speedOverride > 0f)
			{
				m_typeSpeed = speedOverride;
			}
		}

		public bool GetTyping()
		{
			if (m_typeSpeed > 0f)
			{
				return m_typeChar < m_wrappedText.Length;
			}
			return false;
		}

		private string ProcessTypedText(string text)
		{
			if (m_typeSpeed <= 0f || m_typeChar >= text.Length || !Application.isPlaying)
			{
				return text;
			}
			return text.Insert(m_typeChar, STR_COLOR_START) + STR_COLOR_END;
		}

		public void SkipTyping()
		{
			m_typeChar = int.MaxValue;
			string text = ProcessTypedText(m_wrappedText);
			UpdateOutline(text);
			m_mesh.text = text;
		}

		private void UpdateTyping()
		{
			if (m_typeSpeed <= 0f || m_typeChar >= m_wrappedText.Length)
			{
				return;
			}
			float num = m_typeSpeed;
			char c = m_wrappedText[m_typeChar];
			if (c == '.' || c == ',' || c == '?' || c == '!')
			{
				num *= 2f;
			}
			if (Utils.GetTimeIncrementPassed(num) && m_typeChar < m_wrappedText.Length)
			{
				m_typeChar++;
				while (m_typeChar < m_wrappedText.Length && (c == ' ' || c == '\r' || c == '\n'))
				{
					m_typeChar++;
					c = m_wrappedText[m_typeChar];
				}
				CallbackOnTypeCharacter?.Invoke();
				string text = ProcessTypedText(m_wrappedText);
				UpdateOutline(text);
				m_mesh.text = text;
			}
		}

		private Vector2 GetOnScreenPosition(Vector2 position)
		{
			if (!Application.isPlaying || m_rectSize.x <= 0f || !Singleton<PowerQuest>.Exists)
			{
				return position;
			}
			Camera cameraGui = Singleton<PowerQuest>.Get.GetCameraGui();
			RectCentered rectCentered = new RectCentered(position.x, position.y, m_rectSize.x, m_rectSize.y);
			RectCentered rectCentered2 = new RectCentered(cameraGui.transform.position.x, cameraGui.transform.position.y, cameraGui.orthographicSize * 2f * cameraGui.aspect, cameraGui.orthographicSize * 2f);
			rectCentered2.RemovePadding(m_screenPadding);
			Vector2 zero = Vector2.zero;
			if (rectCentered.MinX < rectCentered2.MinX)
			{
				zero.x += rectCentered2.MinX - rectCentered.MinX;
			}
			if (rectCentered.MaxX > rectCentered2.MaxX)
			{
				zero.x += rectCentered2.MaxX - rectCentered.MaxX;
			}
			if (rectCentered.MinY < rectCentered2.MinY)
			{
				zero.y += rectCentered2.MinY - rectCentered.MinY;
			}
			if (rectCentered.MaxY > rectCentered2.MaxY)
			{
				zero.y += rectCentered2.MaxY - rectCentered.MaxY;
			}
			return position + zero;
		}

		private bool CheckTextMesh()
		{
			if (m_mesh == null)
			{
				m_mesh = GetComponent<TextMesh>();
			}
			return m_mesh != null;
		}

		private bool CheckMaterial()
		{
			if (m_materialSet && Application.isPlaying)
			{
				return true;
			}
			if (m_mesh == null)
			{
				m_mesh = GetComponent<TextMesh>();
			}
			if (m_meshRenderer == null)
			{
				m_meshRenderer = GetComponent<MeshRenderer>();
			}
			if (m_mesh == null || m_meshRenderer == null)
			{
				return false;
			}
			bool flag = true;
			if (Application.isPlaying && Singleton<PowerQuest>.Exists)
			{
				flag = Singleton<PowerQuest>.Get.GetSnapToPixel();
			}
			if (s_shader == null)
			{
				s_shader = Shader.Find(flag ? STR_SHADER_PIXEL : STR_SHADER);
			}
			if (s_shader == null)
			{
				return false;
			}
			Material material = (Application.isPlaying ? m_meshRenderer.material : m_meshRenderer.sharedMaterial);
			if (m_shaderOverride != null && material != null && material.shader != m_shaderOverride)
			{
				if (material.shader != m_shaderOverride)
				{
					material = new Material(material);
					material.shader = m_shaderOverride;
					material.mainTexture.filterMode = ((!flag || !m_setFiltering) ? FilterMode.Bilinear : FilterMode.Point);
					material.mainTexture.anisoLevel = ((!flag || !m_setFiltering) ? 1 : 0);
					if (Application.isPlaying)
					{
						m_meshRenderer.material = material;
					}
				}
				if (!Application.isPlaying)
				{
					m_materialSet = material != null;
				}
				return m_materialSet;
			}
			if (m_mesh.font.material.shader != s_shader || !Application.isPlaying)
			{
				m_mesh.font.material.shader = s_shader;
				m_mesh.font.material.mainTexture.filterMode = ((!flag || !m_setFiltering) ? FilterMode.Bilinear : FilterMode.Point);
				m_mesh.font.material.mainTexture.anisoLevel = ((!flag || !m_setFiltering) ? 1 : 0);
			}
			if (!Application.isPlaying)
			{
				m_materialSet = true;
			}
			return true;
		}

		private string ParseImages(string text)
		{
			if (!Application.isPlaying)
			{
				return text;
			}
			MeshRenderer component = GetComponent<MeshRenderer>();
			component.GetMaterials(m_tempMaterials);
			if (m_tempMaterials.Count > 1)
			{
				for (int i = 1; i < component.materials.Length; i++)
				{
					UnityEngine.Object.Destroy(component.materials[i]);
				}
				component.materials = new Material[1] { component.materials[0] };
			}
			return REGEX_TAG.Replace(text, EvaluateImageTagMatch);
		}

		private static TextSpriteData FindByTag(TextSpriteData[] textSpriteData, string tag)
		{
			TextSpriteData result = null;
			foreach (TextSpriteData textSpriteData2 in textSpriteData)
			{
				if (string.Equals(textSpriteData2.m_tag, tag, StringComparison.OrdinalIgnoreCase))
				{
					if (textSpriteData2.m_platform.EqualsIgnoreCase(s_textSpritePlatform))
					{
						return textSpriteData2;
					}
					if (IsString.Empty(textSpriteData2.m_platform))
					{
						result = textSpriteData2;
					}
				}
			}
			return result;
		}

		private string EvaluateImageTagMatch(Match match)
		{
			string empty = string.Empty;
			if (match.Groups == null || match.Groups.Count < 2 || !Singleton<PowerQuest>.Exists)
			{
				return empty;
			}
			string value = match.Groups[1].Value;
			value += "PS";
			TextSpriteData textSpriteData = FindByTag(Singleton<PowerQuest>.Get.m_textSprites, value);
			if (textSpriteData == null)
			{
				value = match.Groups[1].Value;
				textSpriteData = FindByTag(Singleton<PowerQuest>.Get.m_textSprites, value);
			}
			if (textSpriteData == null || textSpriteData.m_sprite == null)
			{
				return empty;
			}
			Sprite sprite = textSpriteData.m_sprite;
			Material material = null;
			MeshRenderer component = GetComponent<MeshRenderer>();
			int num = -1;
			for (int i = 0; i < component.materials.Length; i++)
			{
				if (component.materials[i].mainTexture == sprite.texture)
				{
					num = i;
					break;
				}
			}
			if (num < 0)
			{
				num = component.materials.Length;
				Material[] array = new Material[num + 1];
				Array.Copy(component.materials, array, num);
				material = UnityEngine.Object.Instantiate(Singleton<PowerQuest>.Get.m_textSpriteMaterial);
				material.mainTexture = sprite.texture;
				array[num] = material;
				component.materials = array;
			}
			material = component.materials[num];
			material.SetVector("_Offset", new Vector2(0f, textSpriteData.m_offsetY));
			return string.Format(TAG_QUAD, num, sprite.textureRect.height, sprite.textureRect.x / (float)sprite.texture.width, sprite.textureRect.y / (float)sprite.texture.height, sprite.textureRect.width / (float)sprite.texture.width, sprite.textureRect.height / (float)sprite.texture.height);
		}
	}
}
