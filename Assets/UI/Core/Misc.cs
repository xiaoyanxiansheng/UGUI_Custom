using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NEW_UI 
{
    public static class Misc
    {
        static public void DestroyImmediate(Object obj) 
        {
            if (obj != null) 
            {
                if (Application.isEditor) Object.DestroyImmediate(obj);
                else Object.Destroy(obj);
            }
        }
    }
}
