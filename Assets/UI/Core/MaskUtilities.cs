using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

namespace NEW_UI 
{
    public class MaskUtilities
    {
        public static void NotifyStencilStateChanged(Component mask) 
        {
            var components = ListPool<Component>.Get();
            mask.GetComponentsInChildren(components);
            for (var i = 0; i < components.Count; i++) 
            {
                if (components[i] == null || components[i].gameObject == mask.gameObject) 
                {
                    continue;

                    var toNotify = components[i] as IMaskable;
                    if (toNotify != null)
                        toNotify.RecalculateMasking();
                }
                ListPool<Component>.Release(components);
            }
        }

        public static Transform FindRootSortOverriedCanvas(Transform start) 
        {
            var canvasList = ListPool<Canvas>.Get();
            start.GetComponentsInChildren(false, canvasList);
            Canvas canvas = null;
            for (int i = 0; i < canvasList.Count; ++i)
            {
                canvas = canvasList[i];
                // 如果有重新排序过就不需要再往上找了 因为显示层的父子结构已经不对了
                if (canvas.overrideSorting)
                    break;
            }
            ListPool<Canvas>.Release(canvasList);

            return canvas != null ? canvas.transform : null;
        }

        public static int GetStencilDepth(Transform transform, Transform stopAfter) 
        {
            int depth = 0;
            if (transform == stopAfter) return depth;

            var t = transform.parent;
            var compenents = ListPool<Mask>.Get();
            while (t != null) 
            {
                t.GetComponents<Mask>(compenents);
                for (var i = 0; i < compenents.Count; i++) 
                {
                    if (compenents[i] != null && compenents[i].MaskEnabled() && compenents[i].graphic.IsActive()) 
                    {
                        depth++;
                        break;
                    }
                }

                if (t == stopAfter)
                    break;

                t = t.parent;
            }

            ListPool<Mask>.Release(compenents);

            return depth;
        }
    }
}
