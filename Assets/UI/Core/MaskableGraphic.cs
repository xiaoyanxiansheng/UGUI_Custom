
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace NEW_UI 
{
    public class MaskableGraphic : Graphic, IClipRegion, IMaskable, IMaterialModifier
    {
        [NonSerialized]
        protected bool m_ShouldRecalculateStencil = true;

        [NonSerialized]
        protected Material m_MaskMaterial;

        [NonSerialized]
        private RectMask2D m_ParentMask;

        [NonSerialized]
        protected int m_StencilValue;

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
            // TODO
            // throw new NotImplementedException();
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
    }

}