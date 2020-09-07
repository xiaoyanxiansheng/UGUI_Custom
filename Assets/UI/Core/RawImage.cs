using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace NEW_UI 
{
    [AddComponentMenu("NEWUI/Raw Image", 12)]
    public class RawImage : MaskableGraphic
    {
        [FormerlySerializedAs("m_Tex")]
        [SerializeField] Texture m_Texture;
        [SerializeField] Rect m_UVRect = new Rect(0f, 0f, 1f, 1f);

        ~RawImage()
        {
            // Debug.Log("RawImage Delete " + m_CacheInstanceId);
        }

        public override Texture mainTexture 
        {
            get 
            {
                if (m_Texture == null) 
                {
                    if (material != null && material.mainTexture != null)
                    {
                        return material.mainTexture;
                    }
                    return s_WhiteTexture;
                }

                return m_Texture;
            }
        }

        public Texture texture
        {
            get
            {
                return m_Texture;
            }
            set 
            {
                if (m_Texture == value) return;

                m_Texture = value;
                SetMaterialDirty();
            }
        }

        public Rect uvRect 
        {
            get 
            {
                return m_UVRect;
            }
            set 
            {
                if (m_UVRect == value) return;

                m_UVRect = value;
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh) 
        {
            Texture tex = mainTexture;
            vh.Clear();
            if (tex != null) 
            {
                var r = GetPixelAdjustedRect();
                var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
                vh.AddVert(new Vector3(v.x, v.y), color, new Vector2(m_UVRect.xMin, m_UVRect.yMin));
                vh.AddVert(new Vector3(v.x, v.w), color, new Vector2(m_UVRect.xMin, m_UVRect.yMax));
                vh.AddVert(new Vector3(v.z, v.w), color, new Vector2(m_UVRect.xMax, m_UVRect.yMax));
                vh.AddVert(new Vector3(v.z, v.y), color, new Vector2(m_UVRect.xMax, m_UVRect.yMin));

                vh.AddTriangle(0, 1, 2);
                vh.AddTriangle(2, 3, 0);
            }
        }
    }
}
