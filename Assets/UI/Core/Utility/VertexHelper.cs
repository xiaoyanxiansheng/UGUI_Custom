using System;
using System.Collections.Generic;
using UnityEngine;

namespace NEW_UI
{
    public class VertexHelper : IDisposable
    {
        private List<Vector3> m_Positions;
        private List<Color32> m_Colors;
        private List<Vector2> m_Uv0S;
        private List<int> m_Indices;

        private bool m_ListsInitalized = false;

        public VertexHelper() { }
        public VertexHelper(Mesh m)
        {
            InitializeListIfRequired();

            m_Positions.AddRange(m.vertices);
            m_Colors.AddRange(m.colors32);
            m_Uv0S.AddRange(m.uv);
            m_Indices.AddRange(m.GetIndices(0));
        }

        private void InitializeListIfRequired() 
        {
            if (!m_ListsInitalized) 
            {
                m_Positions = ListPool<Vector3>.Get();
                m_Colors = ListPool<Color32>.Get();
                m_Uv0S = ListPool<Vector2>.Get();
                m_Indices = ListPool<int>.Get();
                m_ListsInitalized = true;
            }
        }

        public void Dispose()
        {
            if (m_ListsInitalized)
            {
                ListPool<Vector3>.Release(m_Positions);
                ListPool<Color32>.Release(m_Colors);
                ListPool<Vector2>.Release(m_Uv0S);
                ListPool<int>.Release(m_Indices);

                m_Positions = null;
                m_Colors = null;
                m_Uv0S = null;
                m_Indices = null;

                m_ListsInitalized = false;
            }
        }

        public void AddVert(Vector3 position, Color32 color, Vector2 uv0) 
        {
            InitializeListIfRequired();

            m_Positions.Add(position);
            m_Colors.Add(color);
            m_Uv0S.Add(uv0);
        }

        public void AddTriangle(int idx0, int idx1, int idx2) 
        {
            InitializeListIfRequired();

            m_Indices.Add(idx0);
            m_Indices.Add(idx1);
            m_Indices.Add(idx2);
        }

        public void FillMesh(Mesh mesh) 
        {
            InitializeListIfRequired();

            mesh.Clear();

            if(m_Positions.Count >= 65000) 
                throw new ArgumentException("Mesh can not have more than 65000 vertices");

            mesh.SetVertices(m_Positions);
            mesh.SetColors(m_Colors);
            mesh.SetUVs(0, m_Uv0S);
            mesh.SetTriangles(m_Indices, 0);
        }

        public void Clear() 
        {
            if (!m_ListsInitalized) return;

            m_Positions.Clear();
            m_Colors.Clear();
            m_Uv0S.Clear();
            m_Indices.Clear();
        }
    }
}  
