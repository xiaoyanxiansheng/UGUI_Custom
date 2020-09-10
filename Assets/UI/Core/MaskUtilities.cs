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
    }
}
