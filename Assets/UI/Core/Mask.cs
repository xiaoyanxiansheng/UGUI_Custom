
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace NEW_UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class Mask : UIIBehaviour, IMaterialModifier
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

            var rootSortCanvas = MaskUtilities.FindRootSortOverriedCanvas(transform);
            var stencilDepth = MaskUtilities.GetStencilDepth(transform,rootSortCanvas);
            if (stencilDepth > 8) 
            {
                Debug.LogWarning("Mask 层级不能超过8层，在同一个canvas下");
                return baseMaterial;
            }

            int desiredStencilBit = 1 << stencilDepth;
            // 第一个Mask
            if (desiredStencilBit == 0) 
            {
                // 重新创建一个裁剪材质
                var maskMaterial = StencilMaterial.Add(baseMaterial, 1, StencilOp.Replace, CompareFunction.Always, m_ShowMaskGraphic ? ColorWriteMask.All : 0);
                StencilMaterial.Remove(m_MaskMaterial);
                m_MaskMaterial = maskMaterial;
                // 重新创建一个清理模板值得材质
                var unmaskMaterial = StencilMaterial.Add(baseMaterial, 1, StencilOp.Zero, CompareFunction.Always, 0);
                StencilMaterial.Remove(m_UnmaskMaterial);
                m_UnmaskMaterial = unmaskMaterial;
                // TODO WG 不清楚具体现实逻辑
                graphic.canvasRenderer.popMaterialCount = 1;
                graphic.canvasRenderer.SetPopMaterial(m_UnmaskMaterial, 0);

                return m_MaskMaterial;
            }

            // 不是第一个Mask
            var maskMaterial2 = StencilMaterial.Add(baseMaterial, desiredStencilBit | (desiredStencilBit - 1), StencilOp.Replace, CompareFunction.Equal, m_ShowMaskGraphic ? ColorWriteMask.All : 0, desiredStencilBit - 1, desiredStencilBit | (desiredStencilBit - 1));
            StencilMaterial.Remove(m_MaskMaterial);
            m_MaskMaterial = maskMaterial2;

            // TODO WG 不清楚具体现实逻辑
            graphic.canvasRenderer.hasPopInstruction = true;
            var unmaskMaterial2 = StencilMaterial.Add(baseMaterial, desiredStencilBit - 1, StencilOp.Replace, CompareFunction.Equal, 0, desiredStencilBit - 1, desiredStencilBit | (desiredStencilBit - 1));
            StencilMaterial.Remove(m_UnmaskMaterial);
            m_UnmaskMaterial = unmaskMaterial2;
            graphic.canvasRenderer.popMaterialCount = 1;
            graphic.canvasRenderer.SetPopMaterial(m_UnmaskMaterial, 0);

            return m_MaskMaterial;
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
