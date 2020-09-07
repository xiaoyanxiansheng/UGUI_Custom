using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NEW_UI
{
    public class UIIBehaviour : MonoBehaviour
    {

        protected virtual void Awake()
        { }

        protected virtual void OnEnable()
        { }

        protected virtual void Start()
        { }

        protected virtual void OnDisable()
        { }

        protected virtual void OnDestroy()
        { }

        protected virtual void OnRectTransformDimensionsChange() 
        { }

        public virtual bool IsActive()
        {
            return isActiveAndEnabled;
        }
        /// <summary>
        /// Returns true if the native representation of the behaviour has been destroyed.
        /// </summary>
        /// <remarks>
        /// When a parent canvas is either enabled, disabled or a nested canvas's OverrideSorting is changed this function is called. You can for example use this to modify objects below a canvas that may depend on a parent canvas - for example, if a canvas is disabled you may want to halt some processing of a UI element.
        /// </remarks>
        public bool IsDestroyed()
        {
            // Workaround for Unity native side of the object
            // having been destroyed but accessing via interface
            // won't call the overloaded ==
            return this == null;
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        { }

        protected virtual void Reset()
        { }
#endif
    }
}  
