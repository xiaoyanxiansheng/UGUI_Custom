
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
                // WG ����ᵼ�²��ʵĴ���
                graphic.SetMaterialDirty();
            }

            // WG ���òü�֮������ڵ��������Ԫ�ض���Ҫ���¼�����ʵ�ģ��ֵ
            // �������е�UIԪ�ض��̳���MaskableGraphic������������¼���д������
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
                Debug.LogWarning("Mask �㼶���ܳ���8�㣬��ͬһ��canvas��");
                return baseMaterial;
            }

            int desiredStencilBit = 1 << stencilDepth;
            // ��һ��Mask
            if (desiredStencilBit == 0) 
            {
                // ���´���һ���ü�����
                var maskMaterial = StencilMaterial.Add(baseMaterial, 1, StencilOp.Replace, CompareFunction.Always, m_ShowMaskGraphic ? ColorWriteMask.All : 0);
                StencilMaterial.Remove(m_MaskMaterial);
                m_MaskMaterial = maskMaterial;
                // ���´���һ������ģ��ֵ�ò���
                var unmaskMaterial = StencilMaterial.Add(baseMaterial, 1, StencilOp.Zero, CompareFunction.Always, 0);
                StencilMaterial.Remove(m_UnmaskMaterial);
                m_UnmaskMaterial = unmaskMaterial;
                // TODO WG �����������ʵ�߼�
                graphic.canvasRenderer.popMaterialCount = 1;
                graphic.canvasRenderer.SetPopMaterial(m_UnmaskMaterial, 0);

                return m_MaskMaterial;
            }

            // ���ǵ�һ��Mask
            var maskMaterial2 = StencilMaterial.Add(baseMaterial, desiredStencilBit | (desiredStencilBit - 1), StencilOp.Replace, CompareFunction.Equal, m_ShowMaskGraphic ? ColorWriteMask.All : 0, desiredStencilBit - 1, desiredStencilBit | (desiredStencilBit - 1));
            StencilMaterial.Remove(m_MaskMaterial);
            m_MaskMaterial = maskMaterial2;

            // TODO WG �����������ʵ�߼�
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
