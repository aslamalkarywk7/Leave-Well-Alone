using UnityEngine;

namespace PowerTools
{
	[RequireComponent(typeof(SpriteRenderer))]
	[AddComponentMenu("PowerTools/PowerSprite")]
	public class PowerSprite : MonoBehaviour
	{
		public static readonly Color COLOR_DISABLED = new Color(1f, 1f, 1f, 0f);

		private static readonly string STR_SHADER = "Sprites/PowerSprite";

		private static readonly string STR_SHADER_OUTLINE = "Sprites/PowerSpriteOutline";

		private static readonly string STR_SPROP_TINT = "_Tint";

		private static readonly string STR_SPROP_OUTLINE = "_Outline";

		private static readonly string STR_SPROP_OFFSET = "_Offset";

		[SerializeField]
		private Color m_tint = COLOR_DISABLED;

		[SerializeField]
		private Color m_outline = COLOR_DISABLED;

		[SerializeField]
		private Vector2 m_offset = Vector2.zero;

		[SerializeField]
		private Shader m_shaderOverride;

		private SpriteRenderer m_renderer;

		private static Shader s_shader = null;

		private static Shader s_shaderOutline = null;

		private Material m_materialCached;

		private bool m_outlineEnabled;

		private SpriteAnimNodes m_nodes;

		private MaterialPropertyBlock m_block;

		public Color Tint
		{
			get
			{
				return m_tint;
			}
			set
			{
				if (m_tint != value)
				{
					m_tint = value;
					UpdateOutline();
					UpdateTint();
				}
			}
		}

		public Color Outline
		{
			get
			{
				return m_outline;
			}
			set
			{
				if (m_outline != value)
				{
					m_outline = value;
					UpdateOutline();
					UpdateTint();
				}
			}
		}

		public Vector2 Offset
		{
			get
			{
				return m_offset;
			}
			set
			{
				if (m_offset != value)
				{
					m_offset = value;
					UpdateOffset();
				}
			}
		}

		private SpriteRenderer Renderer
		{
			get
			{
				if (m_renderer == null)
				{
					m_renderer = GetComponent<SpriteRenderer>();
				}
				return m_renderer;
			}
		}

		public void AlignLeft()
		{
			CheckMaterial();
			m_offset.x = Renderer.sprite.bounds.size.x * 0.5f;
			UpdateOffset();
		}

		public void AlignRight()
		{
			CheckMaterial();
			m_offset.x = (0f - Renderer.sprite.bounds.size.x) * 0.5f;
			UpdateOffset();
		}

		public void AlignTop()
		{
			CheckMaterial();
			m_offset.y = (0f - Renderer.sprite.bounds.size.y) * 0.5f;
			UpdateOffset();
		}

		public void AlignBottom()
		{
			CheckMaterial();
			m_offset.y = Renderer.sprite.bounds.size.y * 0.5f;
			UpdateOffset();
		}

		public void AlignCenter()
		{
			CheckMaterial();
			m_offset.x = 0f;
			UpdateOffset();
		}

		public void AlignMiddle()
		{
			CheckMaterial();
			m_offset.y = 0f;
			UpdateOffset();
		}

		public void Snap()
		{
			CheckMaterial();
			if (Renderer.sprite != null)
			{
				m_offset = Snap(m_offset, 1f / Renderer.sprite.pixelsPerUnit);
			}
			UpdateOffset();
		}

		public Vector2 GetNodePosition(int node)
		{
			if (m_nodes == null)
			{
				m_nodes = GetComponent<SpriteAnimNodes>();
			}
			Vector2 offset = Offset;
			offset.Scale(base.transform.localScale);
			Vector2 vector = offset;
			if (m_nodes == null)
			{
				return vector + (Vector2)base.transform.position;
			}
			return vector + (Vector2)m_nodes.GetPosition(node);
		}

		public float GetNodeAngle(int node)
		{
			if (m_nodes == null)
			{
				m_nodes = GetComponent<SpriteAnimNodes>();
			}
			return m_nodes.GetAngle(node);
		}

		public Shader GetShaderOverride()
		{
			return m_shaderOverride;
		}

		public void SetShaderOverride(Shader shader)
		{
			m_shaderOverride = shader;
			m_materialCached = null;
			CheckMaterial();
		}

		private void Reset()
		{
			CheckMaterial();
		}

		public void OnValidate()
		{
			if (!Application.isPlaying)
			{
				CheckMaterial(onValidate: true);
				UpdateTint();
				UpdateOutline();
				UpdateOffset();
			}
		}

		private void Start()
		{
			UpdateAll();
		}

		private bool CheckMaterial(bool onValidate = false)
		{
			bool flag = !m_outlineEnabled && m_outline.a > 0f;
			if (s_shader == null)
			{
				s_shader = Shader.Find(STR_SHADER);
			}
			if (s_shaderOutline == null)
			{
				s_shaderOutline = Shader.Find(STR_SHADER_OUTLINE);
			}
			if (s_shader == null)
			{
				return false;
			}
			if (m_materialCached != null && !flag && m_shaderOverride == null)
			{
				return true;
			}
			Shader shader = ((m_shaderOverride != null) ? m_shaderOverride : (flag ? s_shaderOutline : s_shader));
			if (Renderer != null && shader != null)
			{
				if (!Application.isPlaying || (Application.isEditor && onValidate))
				{
					m_materialCached = Renderer.sharedMaterial;
				}
				else
				{
					m_materialCached = Renderer.material;
				}
				if (m_materialCached == null || m_materialCached.shader != shader)
				{
					m_materialCached = new Material(shader);
					if (!Application.isPlaying)
					{
						Renderer.sharedMaterial = m_materialCached;
					}
					else
					{
						Renderer.material = m_materialCached;
					}
				}
			}
			if (flag && m_materialCached != null)
			{
				m_outlineEnabled = true;
			}
			return m_materialCached != null;
		}

		private void UpdateAll()
		{
			CheckMaterial();
			MaterialPropertyBlock materialPropertyBlock = StartPropertyBlock();
			materialPropertyBlock.SetColor(STR_SPROP_TINT, m_tint);
			materialPropertyBlock.SetColor(STR_SPROP_OUTLINE, m_outline);
			materialPropertyBlock.SetVector(STR_SPROP_OFFSET, m_offset);
			EndPropertyBlock();
		}

		private void UpdateTint()
		{
			StartPropertyBlock().SetColor(STR_SPROP_TINT, m_tint);
			EndPropertyBlock();
		}

		private void UpdateOutline()
		{
			if (CheckMaterial())
			{
				StartPropertyBlock().SetColor(STR_SPROP_OUTLINE, m_outline);
				EndPropertyBlock();
			}
		}

		private void UpdateOffset()
		{
			StartPropertyBlock().SetVector(STR_SPROP_OFFSET, m_offset);
			EndPropertyBlock();
		}

		private MaterialPropertyBlock StartPropertyBlock()
		{
			if (m_block == null)
			{
				m_block = new MaterialPropertyBlock();
			}
			Renderer.GetPropertyBlock(m_block);
			return m_block;
		}

		private void EndPropertyBlock()
		{
			Renderer.SetPropertyBlock(m_block);
		}

		private static Vector2 Snap(Vector2 pos, float snapTo)
		{
			return new Vector2(Snap(pos.x, snapTo), Snap(pos.y, snapTo));
		}

		private static float Snap(float pos, float snapTo = 1f)
		{
			if (snapTo < 0.001f)
			{
				return pos;
			}
			return Mathf.Round(pos / snapTo) * snapTo;
		}
	}
}
