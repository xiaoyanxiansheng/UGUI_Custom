
using UnityEngine;

namespace NEW_UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class Mask : UIIBehaviour , ICanvasRaycastFilter , IMaterialModifier
    {
        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            // TODO
            return false;
        }

        
    }
}  
