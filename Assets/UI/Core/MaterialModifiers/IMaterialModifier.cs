
using UnityEngine;

namespace NEW_UI 
{
    public interface IMaterialModifier
    {
        Material GetModifiedMaterial(Material baseMaterial);
    }
}
