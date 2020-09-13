
using UnityEngine;

namespace NEW_UI 
{
    public interface IClipper 
    {

        void PerformClipping();

    }

    public interface IClippable 
    {
        GameObject gameObject { get; }

        RectTransform rectTransform { get; }

        void RecalculateClipping();

        void SetClipRect(Rect value, bool validRect);
    }
}
