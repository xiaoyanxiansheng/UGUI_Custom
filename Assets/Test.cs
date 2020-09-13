using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NEW_UI
{

    [ExecuteAlways]
    public class Test : MonoBehaviour
    {
        public bool cull = false;

        public Rect rect;

        protected void OnRectTransformDimensionsChange()
        {
            Debug.LogError("-----------");
        }
        // Start is called before the first frame update
        void Start()
        {
            rect = GetComponent<RectTransform>().rect;
        }

        // Update is called once per frame
        void Update()
        {
            GetComponent<CanvasRenderer>().cull = cull;
            GetComponent<CanvasRenderer>().EnableRectClipping(rect);
        }
    }
}  
