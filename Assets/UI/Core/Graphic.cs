
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace NEW_UI 
{
    [RequireComponent(typeof(CanvasRenderer))]
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    public abstract class Graphic
        : UIIBehaviour,
        ICanvasElement
    {
        static protected Material s_DefaultUI = null;
        static protected Texture2D s_WhiteTexture = null;

        static public Material defaultGraphicMaterial
        {
            get
            {
                if (s_DefaultUI == null)
                {
                    s_DefaultUI = Canvas.GetDefaultCanvasMaterial();
                }
                return s_DefaultUI;
            }
        }

        [FormerlySerializedAs("m_Mat")]
        [SerializeField] protected Material m_Material;
        [SerializeField] private Color m_Color = Color.white;

        [NonSerialized] private CanvasRenderer m_CanvasRenderer;
        [NonSerialized] private RectTransform m_RectTransfrom;
        [NonSerialized] private bool m_VertsDirty;
        [NonSerialized] private bool m_MaterialDirty;
        [NonSerialized] protected static Mesh s_Mesh;
        [NonSerialized] private static readonly VertexHelper s_VertexHelper = new VertexHelper();

        public virtual Texture mainTexture
        {
            get
            {
                return s_WhiteTexture;
            }
        }

        public virtual Material material
        {
            get
            {
                return (m_Material != null) ? m_Material : defaultMaterial;
            }
            set
            {
                if (m_Material == value) return;

                m_Material = value;
                SetMaterialDirty();
            }
        }

        public virtual Material defaultMaterial
        {
            get
            {
                return defaultGraphicMaterial;
            }
        }

        protected override void OnEnable() 
        {
            base.OnEnable();
            SetAllDirty();
        }

        public virtual void SetMaterialDirty()
        {
            m_MaterialDirty = true;
            CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
        }

        public virtual void SetVerticesDirty()
        {
            m_VertsDirty = true;
            CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
        }

        protected virtual void OnPopulateMesh(VertexHelper vh)
        {
            var r = GetPixelAdjustedRect();
            var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);

            Color32 color32 = color;
            vh.Clear();
            vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(0f, 0f));
            vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(0f, 1f));
            vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(1f, 1f));
            vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(1f, 0f));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }

        protected virtual void UpdateMaterial()
        {
            if (!IsActive()) return;

            canvasRenderer.materialCount = 1;
            canvasRenderer.SetMaterial(materialForRendering, 0);
            canvasRenderer.SetTexture(mainTexture);
        }

        protected virtual void UpdateGeometry()
        {
            // TODO 简化
            DoMeshGeneration();
        }

        private void DoMeshGeneration()
        {
            if (rectTransform != null && rectTransform.rect.width >= 0 && rectTransform.rect.height >= 0)
            {
                OnPopulateMesh(s_VertexHelper);
            }
            else
            {
                s_VertexHelper.Clear();
            }

            s_VertexHelper.FillMesh(workMesh);
            canvasRenderer.SetMesh(workMesh);
        }

        public virtual Material materialForRendering
        {
            get
            {
                // TODO 简化
                return material;
            }
        }

        public virtual void Rebuild(CanvasUpdate update)
        {
            if (canvasRenderer == null || canvasRenderer.cull) return;

            switch (update)
            {
                // 记录 图形的设置实在渲染之前
                case CanvasUpdate.PreRender:
                    if (m_VertsDirty)
                    {
                        UpdateGeometry();
                        m_VertsDirty = false;
                    }
                    if (m_MaterialDirty)
                    {
                        UpdateMaterial();
                        m_MaterialDirty = false;
                    }
                    break;
            }
        }

        public virtual Color color { get { return m_Color; } set { if (SetPropertyUtility.SetColor(ref m_Color, value)) SetVerticesDirty(); } }

        public Rect GetPixelAdjustedRect()
        {
            // TODO 简化
            return rectTransform.rect;
        }

        public CanvasRenderer canvasRenderer
        {
            get
            {
                if (m_CanvasRenderer == null)
                {
                    m_CanvasRenderer = GetComponent<CanvasRenderer>();
                }
                return m_CanvasRenderer;
            }
        }

        public void LayoutComplete()
        {
            throw new NotImplementedException();
        }

        public void GraphicUpdateComplete()
        {
            
        }

        public virtual void SetAllDirty() 
        {
            SetMaterialDirty();
            SetVerticesDirty();
        }

        protected override void OnRectTransformDimensionsChange() 
        {
            SetVerticesDirty();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetAllDirty();
        }

#endif

        public RectTransform rectTransform
        {
            get
            {
                if (m_RectTransfrom == null)
                {
                    m_RectTransfrom = GetComponent<RectTransform>();
                }
                return m_RectTransfrom;
            }
        }

        protected static Mesh workMesh 
        {
            get 
            {
                if (s_Mesh == null) 
                {
                    s_Mesh = new Mesh();
                    s_Mesh.name = "Shared UI Mesh";
                    s_Mesh.hideFlags = HideFlags.HideAndDontSave;
                }
                return s_Mesh;
            }
        }
    }
}