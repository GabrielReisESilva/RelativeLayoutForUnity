using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RelativeLayout
{
    [CreateAssetMenu(fileName = "Settings", menuName = "Settings", order = 50)]
    public class RelativeLayout_Settings : ScriptableObject
    {
        [Header("Top Icons")]
        public Texture TopLockedImage;
        public Texture TopUnlockedImage;
        public Texture topTopAlignmentIcon;
        public Texture topBottomAlignmentIcon;
        public Texture topCenterAlignmentIcon;
        [Header("Bottom Icons")]
        public Texture BottomLockedImage;
        public Texture BottomUnlockedImage;
        public Texture bottomTopAlignmentIcon;
        public Texture bottomBottomAlignmentIcon;
        public Texture bottomCenterAlignmentIcon;
        [Header("Left Icons")]
        public Texture LeftLockedImage;
        public Texture LeftUnlockedImage;
        public Texture leftLeftAlignmentIcon;
        public Texture leftRightAlignmentIcon;
        public Texture leftCenterAlignmentIcon;
        [Header("Right Icons")]
        public Texture RightLockedImage;
        public Texture RightUnlockedImage;
        public Texture rightLeftAlignmentIcon;
        public Texture rightRightAlignmentIcon;
        public Texture rightCenterAlignmentIcon;
        [Header("Reference Buttons")]
        public Edge[] topReferenceButtons = new Edge[] { Edge.TOP, Edge.CENTER_VERTICAL, Edge.BOTTOM };
        public Edge[] bottomReferenceButtons = new Edge[] { Edge.BOTTOM, Edge.CENTER_VERTICAL, Edge.TOP };
        public Edge[] leftReferenceButtons = new Edge[] { Edge.LEFT, Edge.CENTER_HORIZONTAL, Edge.RIGHT };
        public Edge[] rightReferenceButtons = new Edge[] { Edge.RIGHT, Edge.CENTER_HORIZONTAL, Edge.LEFT };
        [Header("Scene View Handles Color")]
        public Color defaultHandleBackgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.5f);
        public Color defaultHandleOutlineColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        public Color selectedHandleColor = new Color(1f, 1f, 1f, 1f);
        public Color targetRectOutlineColor = new Color(1f, 1f, 0.5f, 1f);
        public Color targetEdgeOutlineColor = new Color(1f, 0.5f, 0.5f, 1f);
        [Header("Scene View Text")]
        [Range (6,24)]
        public int offsetTextFontSize = 12;
        public Color offsetTextColor = Color.gray;
        [Header("Free Movement Mode")]
        public KeyCode freeMovementKeyCode = KeyCode.LeftShift;
        [Header("Runtime Exection")]
        public bool roundOffsetToInt = true;
        public bool allowRuntimeExecution = true;
        public bool lockParameters = false;

        public int GetReferenceButtonsIndex(Edge edge, Edge targetEdge)
        {
            Edge[] selectionEdges = GetReferenceButtons(edge);
            for (int i = 0; i < selectionEdges.Length; i++)
            {
                if (targetEdge == selectionEdges[i]) return i;
            }
            return -1;
        }
        public Texture[] GetButtonIcons(Edge edge)
        {
            Edge[] edges = GetReferenceButtons(edge);
            Texture[] buttons = new Texture[edges.Length];
            for (int i = 0; i < edges.Length; i++)
            {
                buttons[i] = GetAlignmentIcon(edge, edges[i]);
            }
            return buttons;
        }
        private Texture GetAlignmentIcon(Edge from, Edge to)
        {
            if (from == Edge.TOP && to == Edge.TOP) return topTopAlignmentIcon;
            else if (from == Edge.TOP && to == Edge.BOTTOM) return topBottomAlignmentIcon;
            else if (from == Edge.TOP && to == Edge.CENTER_VERTICAL) return topCenterAlignmentIcon;
            else if (from == Edge.BOTTOM && to == Edge.TOP) return bottomTopAlignmentIcon;
            else if (from == Edge.BOTTOM && to == Edge.BOTTOM) return bottomBottomAlignmentIcon;
            else if (from == Edge.BOTTOM && to == Edge.CENTER_VERTICAL) return bottomCenterAlignmentIcon;
            else if (from == Edge.LEFT && to == Edge.LEFT) return leftLeftAlignmentIcon;
            else if (from == Edge.LEFT && to == Edge.RIGHT) return leftRightAlignmentIcon;
            else if (from == Edge.LEFT && to == Edge.CENTER_HORIZONTAL) return leftCenterAlignmentIcon;
            else if (from == Edge.RIGHT && to == Edge.LEFT) return rightLeftAlignmentIcon;
            else if (from == Edge.RIGHT && to == Edge.RIGHT) return rightRightAlignmentIcon;
            else if (from == Edge.RIGHT && to == Edge.CENTER_HORIZONTAL) return rightCenterAlignmentIcon;

            return null;
        }
        public Edge GetSelectedEdge(Edge edge, int selection)
        {
            Edge[] edges = GetReferenceButtons(edge);
            if (selection > -1 && selection < edges.Length)
            {
                return edges[selection];
            }
            return Edge.NONE;
        }
        private Edge[] GetReferenceButtons(Edge edge)
        {
            switch (edge)
            {
                case Edge.TOP:
                    return topReferenceButtons;
                case Edge.BOTTOM:
                    return bottomReferenceButtons;
                case Edge.LEFT:
                    return leftReferenceButtons;
                case Edge.RIGHT:
                    return rightReferenceButtons;
                default:
                    return new Edge[0];
            }
        }
        public GUIStyle GetButtonStyle(GUIStyle miniButtonStyle)
        {
            GUIStyle unselectedButtonStyle = new GUIStyle(miniButtonStyle);
            unselectedButtonStyle.padding = RelativeLayout_Utils.ALIGNMENT_BUTTON_PADDING;
            unselectedButtonStyle.fixedWidth = RelativeLayout_Utils.ALIGNMENT_BUTTON_WIDTH;
            unselectedButtonStyle.fixedHeight = RelativeLayout_Utils.ALIGNMENT_BUTTON_HEIGHT;
            return unselectedButtonStyle;
        }
    }
}