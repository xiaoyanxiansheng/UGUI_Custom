using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NEW_UI
{
    /// <summary>
    /// ²Ã¼ôÆ÷
    /// </summary>
    public class ClipperRegistry
    {
        static ClipperRegistry s_Instance;

        readonly IndexedSet<IClipper> m_Clipper = new IndexedSet<IClipper>();

        protected ClipperRegistry() { }

        public static ClipperRegistry instance 
        {
            get 
            {
                if (s_Instance == null) 
                {
                    s_Instance = new ClipperRegistry();
                }
                return s_Instance;
            }
        }

        public void Cull() 
        {
            for (int i = 0; i < m_Clipper.Count; i++) 
            {
                m_Clipper[i].PerformClipping();
            }
        }

        public static void Register(IClipper c) 
        {
            if (c == null) 
            {
                return;
            }
            instance.m_Clipper.Add(c);
        }

        public static void UnRegister(IClipper c) 
        {
            instance.m_Clipper.Remove(c);
        }
    }
}  
