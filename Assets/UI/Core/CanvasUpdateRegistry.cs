
using UnityEngine;

namespace NEW_UI
{

    /// <summary>
    /// Values of 'update' called on a Canvas update.
    /// </summary>
    public enum CanvasUpdate
    {
        /// <summary>
        /// Called before layout.
        /// </summary>
        Prelayout = 0,
        /// <summary>
        /// Called for layout.
        /// </summary>
        Layout = 1,
        /// <summary>
        /// Called after layout.
        /// </summary>
        PostLayout = 2,
        /// <summary>
        /// Called before rendering.
        /// </summary>
        PreRender = 3,
        /// <summary>
        /// Called late, before render.
        /// </summary>
        LatePreRender = 4,
        /// <summary>
        /// Max enum value. Always last.
        /// </summary>
        MaxUpdateValue = 5
    }

    public class CanvasUpdateRegistry
    {
        private static CanvasUpdateRegistry s_Instance;

        private bool m_PerformingGraphicUpdate;

        private readonly IndexedSet<ICanvasElement> m_GraphicRebuildQueue = new IndexedSet<ICanvasElement>();

        public CanvasUpdateRegistry() 
        {
            Canvas.willRenderCanvases += PerformUpdate;
        }

        private void CleanInvalidItems() 
        {
            for (int i = m_GraphicRebuildQueue.Count - 1; i >= 0; --i)
            {
                var item = m_GraphicRebuildQueue[i];
                if (item == null)
                {
                    m_GraphicRebuildQueue.RemoveAt(i);
                    continue;
                }

                if (item.IsDestroyed())
                {
                    m_GraphicRebuildQueue.RemoveAt(i);
                    item.GraphicUpdateComplete();
                }
            }
        }

        private void PerformUpdate() 
        {
            CleanInvalidItems();

            m_PerformingGraphicUpdate = true;

            for (var i = (int)CanvasUpdate.PreRender; i < (int)(CanvasUpdate.MaxUpdateValue); i++) 
            {
                for (var k = 0; k < instance.m_GraphicRebuildQueue.Count; k++) 
                {
                    var element = instance.m_GraphicRebuildQueue[k];
                    if(ObjectValidForUpdate(element)) element.Rebuild((CanvasUpdate)i);
                }   
            }

            for (int i = 0; i < m_GraphicRebuildQueue.Count; i++) 
            {
                m_GraphicRebuildQueue[i].GraphicUpdateComplete();
            }

            m_GraphicRebuildQueue.Clear();
            m_PerformingGraphicUpdate = false;
        } 

        public static CanvasUpdateRegistry instance 
        {
            get 
            {
                if (s_Instance == null)
                    s_Instance = new CanvasUpdateRegistry();
                return s_Instance;
            }
        }

        public static void RegisterCanvasElementForGraphicRebuild(ICanvasElement element) 
        {
            instance.InternalRegisterCanvasElementForGraphicRebuild(element);
        }

        private bool InternalRegisterCanvasElementForGraphicRebuild(ICanvasElement element)
        {
            if (m_PerformingGraphicUpdate)
            {
                Debug.LogError(string.Format("Trying to add {0} for graphic rebuild while we are already inside a graphic rebuild loop. This is not supported.", element));
                return false;
            }

            return m_GraphicRebuildQueue.AddUnique(element);
        }

        private bool ObjectValidForUpdate(ICanvasElement element)
        {
            var valid = element != null;

            var isUnityObject = element is Object;
            if (isUnityObject)
                valid = (element as Object) != null; //Here we make use of the overloaded UnityEngine.Object == null, that checks if the native object is alive.

            return valid;
        }
    }

    public interface ICanvasElement 
    {
        void Rebuild(CanvasUpdate executing);

        Transform transform { get; }

        void LayoutComplete();

        void GraphicUpdateComplete();

        bool IsDestroyed();
    }
}  
