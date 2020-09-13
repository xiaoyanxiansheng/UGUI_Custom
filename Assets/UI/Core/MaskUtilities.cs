using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

namespace NEW_UI 
{
    public class MaskUtilities
    {

        public static void Notify2DMaskStateChanged(Component mask) 
        {
            var components = ListPool<Component>.Get();
            mask.GetComponentsInChildren(components);
            for (int i = 0; i < components.Count; i++) 
            {
                if (components[i] == null || components[i].gameObject == mask.gameObject) 
                {
                    continue;
                }
                var toNotify = components[i] as IClippable;
                if (toNotify != null) 
                {
                    toNotify.RecalculateClipping();
                }
            }
            ListPool<Component>.Release(components);
        }

        public static void NotifyStencilStateChanged(Component mask) 
        {
            var components = ListPool<Component>.Get();
            mask.GetComponentsInChildren(components);
            for (var i = 0; i < components.Count; i++) 
            {
                if (components[i] == null || components[i].gameObject == mask.gameObject) 
                {
                    continue;
                }
                var toNotify = components[i] as IMaskable;
                if (toNotify != null)
                    toNotify.RecalculateMasking();
            }
            ListPool<Component>.Release(components);
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

        public static void GetRectMasksForClip(RectMask2D clipper, List<RectMask2D> masks) 
        {
            masks.Clear();

            List<Canvas> canvasComponents = ListPool<Canvas>.Get();
            List<RectMask2D> rectMaskComponents = ListPool<RectMask2D>.Get();

            clipper.transform.GetComponentsInParent(false, rectMaskComponents);
            if (rectMaskComponents.Count > 0) 
            {
                clipper.transform.GetComponentsInParent(false, canvasComponents);

                for (int i = 0; i < rectMaskComponents.Count; i++) 
                {
                    if (!rectMaskComponents[i].IsActive()) 
                    {
                        continue;
                    }
                    bool shouldAdd = true;
                    for (int j = canvasComponents.Count - 1; j >= 0; j--)
                    {
                        // TODO 不懂
                        if (!IsDescendantOrSelf(canvasComponents[j].transform, rectMaskComponents[i].transform) && canvasComponents[j].overrideSorting)
                        {
                            shouldAdd = false;
                            break;
                        }
                    }
                    if (shouldAdd) 
                    {
                        masks.Add(rectMaskComponents[i]);
                    }
                }
            }
        }

        public static RectMask2D GetRectMaskForClippable(IClippable clippable)
        {
            List<RectMask2D> rectMaskComponents = ListPool<RectMask2D>.Get();
            List<Canvas> canvasComponents = ListPool<Canvas>.Get();
            RectMask2D componentToReturn = null;

            clippable.gameObject.GetComponentsInParent(false, rectMaskComponents);

            if (rectMaskComponents.Count > 0)
            {
                for (int rmi = 0; rmi < rectMaskComponents.Count; rmi++)
                {
                    componentToReturn = rectMaskComponents[rmi];
                    if (componentToReturn.gameObject == clippable.gameObject)
                    {
                        componentToReturn = null;
                        continue;
                    }
                    if (!componentToReturn.isActiveAndEnabled)
                    {
                        componentToReturn = null;
                        continue;
                    }
                    clippable.gameObject.GetComponentsInParent(false, canvasComponents);
                    for (int i = canvasComponents.Count - 1; i >= 0; i--)
                    {
                        if (!IsDescendantOrSelf(canvasComponents[i].transform, componentToReturn.transform) && canvasComponents[i].overrideSorting)
                        {
                            componentToReturn = null;
                            break;
                        }
                    }
                    break;
                }
            }

            ListPool<RectMask2D>.Release(rectMaskComponents);
            ListPool<Canvas>.Release(canvasComponents);

            return componentToReturn;
        }

        public static bool IsDescendantOrSelf(Transform father, Transform child)
        {
            if (father == null || child == null)
                return false;

            if (father == child)
                return true;

            while (child.parent != null)
            {
                if (child.parent == father)
                    return true;

                child = child.parent;
            }

            return false;
        }
    }
}
