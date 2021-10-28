using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RelativeLayout
{
    public enum Edge
    {
        NONE = 0,
        TOP = 1,
        BOTTOM = 2,
        CENTER_VERTICAL = 3,
        LEFT = 4, 
        RIGHT = 5,
        CENTER_HORIZONTAL = 6,
    };
    public class RelativeLayout_Utils
    {
        public static readonly Color LOCK_EDGE_COLOR = new Color(1f, 0.67f, 0f);
        public static readonly Color REFERENCE_EDGE_COLOR = LOCK_EDGE_COLOR;
        public static readonly Color DOTTED_LINE_COLOR = new Color(0.5f, 0.5f, 0.5f);
        public static readonly Color ARROW_COLOR = DOTTED_LINE_COLOR;
        public static readonly Color OFFSET_TEXT_COLOR = Color.gray;

        public const float DOT_LENGTH = 4f;
        public const float ARROW_LENGTH = 8f;
        public const float ARROW_ANGLE = 20f;
        public const float ALIGNMENT_BUTTON_WIDTH = 40f;
        public const float ALIGNMENT_BUTTON_HEIGHT = 40f;

        private static GUIStyle verticalOffsetTextStyle;
        private static GUIStyle horizontalOffsetTextStyle;

        public static readonly RectOffset ALIGNMENT_BUTTON_PADDING = new RectOffset(3, 3, 3, 3);
        //private static GUIStyle unselectedButtonStyle;
        //private static GUIStyle selectedButtonStyle;

        public static GUIStyle GetOffsetTextStyle(Edge edge, Color textColor, int fontSize)
        {
            if (edge.IsHorizontal()) return GetHorizontalOffsetTextStyle(textColor, fontSize);

            return GetVerticalOffsetTextStyle(textColor, fontSize);
        }
        private static GUIStyle GetVerticalOffsetTextStyle(Color textColor, int fontSize)
        {
            if (verticalOffsetTextStyle == null)
            {
                verticalOffsetTextStyle = new GUIStyle();
                verticalOffsetTextStyle.contentOffset = new Vector2(5f , -10f);
                verticalOffsetTextStyle.alignment = TextAnchor.LowerLeft;
                verticalOffsetTextStyle.normal.textColor = textColor;
                verticalOffsetTextStyle.fontSize = fontSize;
            }
            return verticalOffsetTextStyle;
        }
        private static GUIStyle GetHorizontalOffsetTextStyle(Color textColor, int fontSize)
        {
            if (horizontalOffsetTextStyle == null)
            {
                horizontalOffsetTextStyle = new GUIStyle();
                horizontalOffsetTextStyle.contentOffset = new Vector2(0f, 0f);
                horizontalOffsetTextStyle.alignment = TextAnchor.UpperCenter;
                horizontalOffsetTextStyle.normal.textColor = textColor;
                horizontalOffsetTextStyle.fontSize = fontSize;
            }
            return horizontalOffsetTextStyle;
        }
        public static Edge GetClosestVerticalEdge(float value, RectTransform rect, RectTransform canvasRect)
        {
            return GetClosestEdge(value, rect.GetEdgePointInCanvas(Edge.BOTTOM, canvasRect), rect.GetEdgePointInCanvas(Edge.TOP, canvasRect), new Edge[] { Edge.BOTTOM, Edge.CENTER_VERTICAL, Edge.TOP });
        }
        public static Edge GetClosestHorizontalEdge(float value, RectTransform rect, RectTransform canvasRect)
        {
            return GetClosestEdge(value, rect.GetEdgePointInCanvas(Edge.LEFT, canvasRect), rect.GetEdgePointInCanvas(Edge.RIGHT, canvasRect), new Edge[] { Edge.LEFT, Edge.CENTER_HORIZONTAL, Edge.RIGHT });
        }
        private static Edge GetClosestEdge(float value, float min, float max, Edge[] edges)
        {
            float center = (max - min) * 0.5f;
            if (value < Mathf.Lerp(min, max, 0.35f)) return edges[0];
            else if (value > Mathf.Lerp(min, max, 0.65f)) return edges[2];
            else return edges[1];
        }
    }
    [System.Serializable]
    public struct ReferenceTarget
    {
        public RectTransform target;
        public Edge edge;
        public float offset;

        public bool IsUsed { get { return target != null && edge != Edge.NONE; } }
        public bool HasEmptyReference { get { return target == null && edge != Edge.NONE; } }

        public float GetAlignPoint(RectTransform canvasRect)
        {
            if(target != null && edge != Edge.NONE)
            {
                return target.GetEdgePointInCanvas(edge, canvasRect);
            }
            Debug.LogError("[REF. LAYOUT] GetAlignPoint: Target is null");
            return 0;
        }
        public void ValidateReference(Edge edge, RectTransform rect)
        {
            if(IsValidReference(edge, rect, new List<RelativeLayout_RelativePosition>()) == false)
            {
                Debug.LogWarning("Position cannot be relative to itself or to another Rect that depends on it in the same axis.\nMake sure " + target.name + " or its parents are not positioning relative to " + rect.name);
                target = null;
            }
        }
        public bool IsValidReference(Edge edge, RectTransform rect, List<RelativeLayout_RelativePosition> references = null)
        {
            // IS THERE A TARGET?
            if (target == null) return true;
            // IS IT TARGETING ITSELF
            if (target == rect) return false;
            // DOES THE TARGET IS RELATIVE TO SOMETHING ELSE?
            RelativeLayout_RelativePosition targetReferencePosition = target.GetComponent<RelativeLayout_RelativePosition>();
            if (targetReferencePosition == null)
            {
                // IS THE TARGET'S PARENT RELATIVE TO SOMETHING ELSE?
                targetReferencePosition = target.GetComponentInParent<RelativeLayout_RelativePosition>();
                if (targetReferencePosition == null) return true;
            }
            // HAVE WE ALREADY VERIFIED THIS TARGET?
            if (references.Contains(targetReferencePosition)) return true;
            references.Add(targetReferencePosition);
            // DOES THE TARGET POSITION IS RELATIVE TO ME?
            ReferenceTarget[] referenceTargets = targetReferencePosition.GetReferenceTargets(edge.IsVertical());
            if (referenceTargets[0].IsValidReference(edge, rect, references) == false)
            {
                return false;
            }
            if (referenceTargets[0].target != referenceTargets[1].target)
            {
                if (referenceTargets[1].IsValidReference(edge, rect, references) == false)
                {
                    return false;
                }
            }
            return true;
        }
        /*
        public void ValidateReference(Edge edge, RectTransform rect)
        {
            if (target == null) return;
            if (target == rect)
            {
                Debug.LogError($"[REF. LAYOUT] Invalid Target: {rect.name} cannot reference itself");
                target = null;
                return;
            }

            RelativeLayout_RelativePosition targetReferencePosition = target.GetComponent<RelativeLayout_RelativePosition>();
            if (targetReferencePosition == null) return;

            RectTransform[] referenceTargets = targetReferencePosition.GetTargets(edge.IsVertical());
            for (int i = 0; i < referenceTargets.Length; i++)
            {
                if (referenceTargets[i] == rect)
                {
                    Debug.LogError($"[REF. LAYOUT] Invalid Target: {rect.name} and {target.name} cannot reference each other on the same axis");
                    target = null;
                }
            }
        }
        */
    }
    public static class UtilsExtension
    {
        public static bool IsVertical(this Edge edge)
        {
            return edge == Edge.TOP || edge == Edge.BOTTOM || edge == Edge.CENTER_VERTICAL;
        }
        public static bool IsHorizontal(this Edge edge)
        {
            return edge == Edge.LEFT || edge == Edge.RIGHT || edge == Edge.CENTER_HORIZONTAL;
        }
        public static float GetEdgePointInCanvas(this RectTransform rect, Edge edge, RectTransform canvasRect)
        {
            Vector3 pointInCanvas = canvasRect.InverseTransformPoint(rect.position);
            switch (edge)
            {
                case Edge.TOP:
                    return pointInCanvas.y + rect.rect.height * (1.0f - rect.pivot.y);
                case Edge.BOTTOM:
                    return pointInCanvas.y + rect.rect.height * (0.0f - rect.pivot.y);
                case Edge.RIGHT:
                    return pointInCanvas.x + rect.rect.width * (1.0f - rect.pivot.x);
                case Edge.LEFT:
                    return pointInCanvas.x + rect.rect.width * (0.0f - rect.pivot.x);
                case Edge.CENTER_VERTICAL:
                    return pointInCanvas.y + rect.rect.height * (0.5f - rect.pivot.y);
                case Edge.CENTER_HORIZONTAL:
                    return pointInCanvas.x + rect.rect.width * (0.5f - rect.pivot.x);
            }
            Debug.LogWarning("[REF. LAYOUT] Cannot find edge point for edge " + edge);
            return pointInCanvas.y;
        }
        public static Vector3[] GetEdgeWorldCorners(this RectTransform rect, Edge edge, Vector3[] corners)
        {
            rect.GetWorldCorners(corners);
            Vector3[] edgeCorner = new Vector3[2];
            switch (edge)
            {
                case Edge.TOP:
                    edgeCorner[0] = corners[1];
                    edgeCorner[1] = corners[2];
                    break;
                case Edge.BOTTOM:
                    edgeCorner[0] = corners[0];
                    edgeCorner[1] = corners[3];
                    break;
                case Edge.CENTER_VERTICAL:
                    edgeCorner[0] = Vector3.Lerp(corners[0], corners[1], 0.5f);
                    edgeCorner[1] = Vector3.Lerp(corners[2], corners[3], 0.5f);
                    break;
                case Edge.LEFT:
                    edgeCorner[0] = corners[0];
                    edgeCorner[1] = corners[1];
                    break;
                case Edge.RIGHT:
                    edgeCorner[0] = corners[3];
                    edgeCorner[1] = corners[2];
                    break;
                case Edge.CENTER_HORIZONTAL:
                    edgeCorner[0] = Vector3.Lerp(corners[0], corners[3], 0.5f);
                    edgeCorner[1] = Vector3.Lerp(corners[1], corners[2], 0.5f);
                    break;
            }
            return edgeCorner;
        }
    }
}