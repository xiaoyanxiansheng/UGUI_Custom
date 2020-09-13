using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NEW_UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class RectMask2D : UIIBehaviour, IClipper
    {
        [NonSerialized]
        private readonly RectangularVertexClipper m_VertexClipper = new RectangularVertexClipper();
        [NonSerialized]
        private RectTransform m_RectTransform;
        [NonSerialized]
        private HashSet<MaskableGraphic> m_MaskableTargets = new HashSet<MaskableGraphic>();
        [NonSerialized]
        private bool m_ShouldRecalculateClipRects;
        [NonSerialized]
        private List<RectMask2D> m_Clippers = new List<RectMask2D>();

        [NonSerialized] private Canvas m_Canvas;

        [NonSerialized]
        private Rect m_LastClipRectCanvasSpace;
        [NonSerialized]
        private bool m_ForceClip;
        private Canvas Canvas
        {
            get
            {
                if (m_Canvas == null)
                {
                    var list = ListPool<Canvas>.Get();
                    gameObject.GetComponentsInParent<Canvas>(false, list);
                    if (list.Count > 0)
                    {
                        m_Canvas = list[list.Count - 1];
                    }
                    else
                    {
                        m_Canvas = null;
                    }
                    ListPool<Canvas>.Release(list);
                }
                return m_Canvas;
            }
        }

        public RectTransform rectTransform
        {
            get
            {
                return m_RectTransform ?? (m_RectTransform = GetComponent<RectTransform>());
            }
        }

        private Vector3[] m_Corners = new Vector3[4];
        private Rect rootCanvasRect 
        {
            get 
            {
                rectTransform.GetWorldCorners(m_Corners);

                if (Canvas != null) 
                {
                    Canvas rootCanvas = Canvas.rootCanvas;
                    for (int i = 0; i < 4; i++) 
                    {
                        m_Corners[i] = rootCanvas.transform.InverseTransformPoint(m_Corners[i]);
                    }
                }

                return new Rect(m_Corners[0].x, m_Corners[0].y, m_Corners[2].x - m_Corners[0].x, m_Corners[2].y - m_Corners[0].y);
            }
        }

        public Rect canvasRect
        {
            get
            {
                return m_VertexClipper.GetCanvasRect(rectTransform, Canvas);
            }
        }

        protected RectMask2D(){}

        protected override void OnEnable()
        {
            base.OnEnable();

            m_ShouldRecalculateClipRects = true;
            ClipperRegistry.Register(this);
            MaskUtilities.Notify2DMaskStateChanged(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            m_MaskableTargets.Clear();
            m_Clippers.Clear();
            ClipperRegistry.UnRegister(this);
            MaskUtilities.Notify2DMaskStateChanged(this);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            m_ShouldRecalculateClipRects = true;

            if (!IsActive())
                return;

            MaskUtilities.Notify2DMaskStateChanged(this);
        }

#endif

        /// <summary>
        /// Update中计算
        /// </summary>
        // TODO 简单实现
        public void PerformClipping()
        {
            if (ReferenceEquals(Canvas, null))
            {
                return;
            }

            if (m_ShouldRecalculateClipRects) 
            {
                MaskUtilities.GetRectMasksForClip(this, m_Clippers);
                m_ShouldRecalculateClipRects = false;
            }

            bool validRect = true;
            // 相交区域
            Rect clipRect = Clipping.FindCullAndClipWorldRect(m_Clippers, out validRect);

            RenderMode renderMode = Canvas.rootCanvas.renderMode;
            // 已经被裁剪
            bool maskIsCulled = (renderMode == RenderMode.ScreenSpaceCamera || renderMode == RenderMode.ScreenSpaceOverlay) && !clipRect.Overlaps(rootCanvasRect);
            if (maskIsCulled) 
            {
                clipRect = Rect.zero;
                validRect = false;
            }
            if (clipRect != m_LastClipRectCanvasSpace)
            {
                foreach (var maskTarget in m_MaskableTargets)
                {
                    maskTarget.SetClipRect(clipRect, validRect);
                    maskTarget.Cull(clipRect, validRect);
                }
            }
            else if (m_ForceClip) 
            {
                foreach (var maskTarget in m_MaskableTargets)
                {
                    maskTarget.SetClipRect(clipRect, validRect);

                    if (maskTarget.canvasRenderer.hasMoved)
                        maskTarget.Cull(clipRect, validRect);
                }
            }

            m_LastClipRectCanvasSpace = clipRect;
            m_ForceClip = false;
        }

        public void AddClippable(IClippable clippable) 
        {
            if (clippable == null) return;

            m_ShouldRecalculateClipRects = true;
            MaskableGraphic maskable = clippable as MaskableGraphic;
            m_MaskableTargets.Add(maskable);
            m_ForceClip = true;
        }

        public void RemoveClippable(IClippable clippable) 
        {
            if (clippable == null) return;

            m_ShouldRecalculateClipRects = true;
            clippable.SetClipRect(new Rect(), false);
            MaskableGraphic maskable = clippable as MaskableGraphic;
            m_MaskableTargets.Remove(maskable);
            m_ForceClip = true;
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            m_ShouldRecalculateClipRects = true;
        }

        protected override void OnCanvasHierarchyChanged()
        {
            m_Canvas = null;
            base.OnCanvasHierarchyChanged();
            m_ShouldRecalculateClipRects = true;
        }
    }
}  
