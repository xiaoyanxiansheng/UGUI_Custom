
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace NEW_UI 
{
    public class MaskableGraphic : Graphic, IClippable, IMaskable, IMaterialModifier
    {
        [NonSerialized]
        protected bool m_ShouldRecalculateStencil = true;

        [NonSerialized]
        protected Material m_MaskMaterial;

        [NonSerialized]
        private RectMask2D m_ParentMask;

        [NonSerialized]
        protected int m_StencilValue;

        [Serializable] public class CullStateChangedEvent : UnityEvent<bool> { }
        [SerializeField]
        private CullStateChangedEvent m_OnCullStateChanged = new CullStateChangedEvent();
        public CullStateChangedEvent cullStateChangeEvent 
        {
            get { return m_OnCullStateChanged; }
            set { m_OnCullStateChanged = value; }
        }

        [NonSerialized]
        private bool m_Maskable = true;
        public bool maskable 
        {
            get { return m_Maskable; }
            set 
            {
                if (value == m_Maskable) 
                {
                    return;
                }

                m_Maskable = true;
                m_ShouldRecalculateStencil = true;
                SetMaterialDirty();
            }
        }

        readonly Vector3[] m_Corners = new Vector3[4];
        private Rect rootCanvasRect 
        {
            get 
            {
                rectTransform.GetWorldCorners(m_Corners);

                if (canvas != null)
                {
                    Canvas rootCanvas = canvas.rootCanvas;
                    for (int i = 0; i < 4; i++)
                    {
                        m_Corners[i] = rootCanvas.transform.InverseTransformPoint(m_Corners[i]);
                    }
                }

                return new Rect(m_Corners[0].x, m_Corners[0].y, m_Corners[2].x - m_Corners[0].x, m_Corners[2].y - m_Corners[0].y);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            m_ShouldRecalculateStencil = true;
            UpdateClipParent();
            SetMaterialDirty();

            if (GetComponent<Mask>() != null) 
            {
                MaskUtilities.NotifyStencilStateChanged(this);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            m_ShouldRecalculateStencil = true;
            SetMaterialDirty();
            UpdateClipParent();
            StencilMaterial.Remove(m_MaskMaterial);
            m_MaskMaterial = null;

            if (GetComponent<Mask>() != null) 
            {
                MaskUtilities.NotifyStencilStateChanged(this);
            }
        }

        private void UpdateClipParent()
        {
            var newParent = (maskable && IsActive()) ? MaskUtilities.GetRectMaskForClippable(this) : null;

            if (m_ParentMask != null && (newParent != m_Maskable || !newParent.IsActive())) 
            {
                m_ParentMask.RemoveClippable(this);
                UpdateCull(false);
            }

            if (newParent != null && newParent.IsActive()) 
            {
                newParent.AddClippable(this);
            }

            m_ParentMask = newParent;
        }

        public Material GetModifiedMaterial(Material baseMaterial)
        {
            var toUse = baseMaterial;

            if (m_ShouldRecalculateStencil) 
            {
                var rootCanvas = MaskUtilities.FindRootSortOverriedCanvas(transform);
                m_StencilValue = MaskUtilities.GetStencilDepth(transform, rootCanvas);
                m_ShouldRecalculateStencil = false;
            }

            Mask maskComponent = GetComponent<Mask>();
            if (m_StencilValue > 0 && (maskComponent == null || !maskComponent.IsActive())) 
            {
                var maskMat = StencilMaterial.Add(toUse, (1 << m_StencilValue) - 1, StencilOp.Keep, CompareFunction.Equal, ColorWriteMask.All, (1 << m_StencilValue) - 1, 0);
                StencilMaterial.Remove(m_MaskMaterial);
                m_MaskMaterial = maskMat;
                toUse = m_MaskMaterial;
            }
            return toUse;
        }

        public virtual void RecalculateMasking()
        {
            StencilMaterial.Remove(m_MaskMaterial);
            m_MaskMaterial = null;
            m_ShouldRecalculateStencil = true;
            SetMaterialDirty();
        }

        public virtual void SetClipRect(Rect clipRect, bool validRect) 
        {
            if (validRect)
            {
                canvasRenderer.EnableRectClipping(clipRect);
            }
            else 
            {
                canvasRenderer.DisableRectClipping();
            }
        }

        public virtual void Cull(Rect clipRect, bool validRect) 
        {
            var cull = !validRect || !clipRect.Overlaps(rootCanvasRect, true);
            UpdateCull(cull);
        }

        private void UpdateCull(bool cull)
        {
            if (canvasRenderer.cull != cull) 
            {
                canvasRenderer.cull = cull;
                // TODO ²»¶®
                UISystemProfilerApi.AddMarker("MaskableGraphic.cullingChanged", this);
                m_OnCullStateChanged.Invoke(cull);
                OnCullingChanged();
            }
        }

        public void RecalculateClipping()
        {
            UpdateClipParent();
        }
    }

}