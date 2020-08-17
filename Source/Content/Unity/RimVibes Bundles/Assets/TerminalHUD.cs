using System;
using UnityEngine;

namespace Assets
{
    class TerminalHUD : MonoBehaviour
    {
        public Rect DrawRect;
        [Range(0f, 1f)]
        public float VerticalSplit = 0.3f;

        public float SectionAWidth = 100;
        public float SectionBWidth = 100;

        public float MinVerticalSize = 100f;
        public float MinSectionWidth = 50f;

        private int currentlyDragging = -1;
        private int draggableIndex;
        private float sectionBStart;
        private float sectionAStart;

        private void OnGUI()
        {
            DrawUI(DrawRect);
        }

        void DrawUI(Rect rect)
        {
            draggableIndex = 0;
            float topHeight = rect.height * VerticalSplit;
            Rect topArea = new Rect(rect.x, rect.y, rect.width, topHeight);
            float bottomHeight = rect.height - topHeight;
            Rect bottomArea = new Rect(rect.x, rect.y + topHeight, rect.width, bottomHeight);

            // Title.
            AutoLabel(rect.position, "Power Control Terminal");

            // Vertical slide.
            float vertSize = Mathf.Clamp(Draggable(new Vector2(rect.x, rect.y + topHeight), rect.width, 0) - rect.y, MinVerticalSize, rect.height - MinVerticalSize);
            VerticalSplit = vertSize / rect.height;

            // First section slide.
            float maxSecAWidth = rect.width - SectionCWidth() - MinSectionWidth;
            SectionAWidth = Mathf.Clamp(Draggable(new Vector2(rect.x + SectionAWidth, rect.y + topHeight), 0f, bottomHeight, () =>
            {
                sectionBStart = SectionBWidth;
                sectionAStart = SectionAWidth;
            }) - rect.x, MinSectionWidth, maxSecAWidth);
            if (IsDragging())
            {
                float deltaA = SectionAWidth - sectionAStart;
                float adjustedB = sectionBStart - deltaA;
                SectionBWidth = adjustedB;
            }

            // Second section slide.
            float cWidth = rect.xMax - Draggable(new Vector2(rect.x + SectionAWidth + SectionBWidth, rect.y + topHeight), 0f, bottomHeight);
            float bWidth = rect.width - cWidth - SectionAWidth;
            SectionBWidth = Mathf.Clamp(bWidth, MinSectionWidth, rect.width - SectionAWidth - MinSectionWidth);

            Rect sectionA = new Rect(rect.x, rect.y + topHeight, SectionAWidth, bottomHeight);
            Rect sectionB = new Rect(rect.x + SectionAWidth, rect.y + topHeight, SectionBWidth, bottomHeight);
            Rect sectionC = new Rect(rect.x + SectionAWidth + SectionBWidth, rect.y + topHeight, SectionCWidth(), bottomHeight);

            //GUI.Box(rect, "");
            GUI.Box(topArea, "");
            GUI.Box(sectionA, "");
            GUI.Box(sectionB, "");
            GUI.Box(sectionC, "");

            float SectionCWidth()
            {
                return rect.width - SectionAWidth - SectionBWidth;
            }

            void AutoLabel(Vector2 pos, string text)
            {
                GUI.Label(new Rect(pos.x, pos.y, 250, 250), text);
            }
        }

        bool IsDragging()
        {
            return currentlyDragging == draggableIndex;
        }

        float Draggable(Vector2 start, float width, float height, Action startDrag = null)
        {
            if (width != 0 && height != 0)
                throw new System.Exception("Width and height cannot both be non-zero.");

            if (width == 0 && height == 0)
                throw new System.Exception("Width and height cannot both be zero.");

            int thisIndex = ++draggableIndex;

            var e = Event.current;
            bool isVertical = height != 0;

            const float THICC = 20f;
            const float HALF_THICC = THICC * 0.5f;

            Rect drawArea;
            if (isVertical)
                drawArea = new Rect(start.x - HALF_THICC, start.y, THICC, height);
            else
                drawArea = new Rect(start.x, start.y - HALF_THICC, width, THICC);

            bool isDraggingSelf = currentlyDragging == thisIndex;
            bool isDraggingAny = currentlyDragging > 0;

            // Update when dragging.
            if (isDraggingSelf)
            {
                // Detect stop drag.
                if (e.button == 0 && e.type == EventType.MouseUp)
                {
                    currentlyDragging = -1;
                }
                else
                {
                    if (isVertical)
                        start.x = e.mousePosition.x;
                    else
                        start.y = e.mousePosition.y;
                }

                var oldColor = GUI.color;
                GUI.color = new Color(0f, 0f, 0f, 0.8f);
                GUI.Box(drawArea, "");
                GUI.color = oldColor;
            }

            bool mouseInArea = drawArea.Contains(e.mousePosition);

            // Draw drag hint.
            if (!isDraggingAny && mouseInArea)
            {
                var oldColor = GUI.color;
                GUI.color = new Color(0f, 0f, 0f, 0.35f);
                GUI.Box(drawArea, "");
                GUI.color = oldColor;
            }

            // Detect start drag.
            if (!isDraggingAny && e.button == 0 && e.type == EventType.MouseDown && mouseInArea)
            {
                currentlyDragging = thisIndex;
                startDrag?.Invoke();
            }

            return isVertical ? start.x : start.y;
        }
    }
}
