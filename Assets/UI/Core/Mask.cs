
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
