
using System;
using UnityEngine;

namespace NEW_UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class Mask : UIIBehaviour, ICanvasRaycastFilter, IMaterialModifier
    {
        [NonSerialized]
        private Graphic m_Graphic;
        [SerializeField]
        private bool m_ShowMaskGraphic = true;
        [NonSerialized]
        private RectTransform m_RectTransform;
        public RectTransform rectTransform
        {
            get { return m_RectTransform ?? (m_RectTransform = GetComponent<RectTransform>()); }
        }

        public bool showMaskGraphic 
        {
            get { return m_ShowMaskGraphic; }
            set 
            {
                if (m_ShowMaskGraphic == value) return;
                m_ShowMaskGraphic = value;
                if (graphic != null) graphic.SetMaterialDirty();
            }
        }

        public Graphic graphic
        {
            get
            {
                return m_Graphic ?? (m_Graphic = GetComponent<Graphic>());
            }
        }

        [NonSerialized]
        private Material m_MaskMaterial;

        [NonSerialized]
        private Material m_UnmaskMaterial;

        protected Mask()
        { }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (graphic != null) 
            {
                graphic.canvasRenderer.hasPopInstruction = true;
                // WG 这里会导致材质的创建
                graphic.SetMaterialDirty();
            }

            // WG 启用裁剪之后这个节点下面的字元素都需要重新计算材质的模板值
            // 由于所有的UI元素都继承与MaskableGraphic，所以这个重新计算写在里面
            MaskUtilities.NotifyStencilStateChanged(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (graphic != null) 
            {
                graphic.SetMaterialDirty();
                graphic.canvasRenderer.hasPopInstruction = false;
                graphic.canvasRenderer.popMaterialCount = 0;
            }

            StencilMaterial.Remove(m_MaskMaterial);
            m_MaskMaterial = null;
            StencilMaterial.Remove(m_UnmaskMaterial);
            m_UnmaskMaterial = null;

            MaskUtilities.NotifyStencilStateChanged(this);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (!IsActive())
                return;

            if (graphic != null)
                graphic.SetMaterialDirty();

            MaskUtilities.NotifyStencilStateChanged(this);
        }

#endif

        public Material GetModifiedMaterial(Material baseMaterial)
        {
            if (!MaskEnabled()) 
            {
                return baseMaterial;
            }

            // var rootSortCanvas = MaskUtilities.
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            // TODO
            return false;
        }

        public virtual bool MaskEnabled() 
        {
            return IsActive() && graphic != null;
        }
    }
}  
