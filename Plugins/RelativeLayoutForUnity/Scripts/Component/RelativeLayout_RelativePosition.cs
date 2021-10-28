using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RelativeLayout
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("RelativeLayout/Relative Position", 0)]
    public class RelativeLayout_RelativePosition : MonoBehaviour
    {
        public ReferenceTarget topReference;
        public ReferenceTarget bottomReference;
        public ReferenceTarget leftReference;
        public ReferenceTarget rightReference;
        public bool allowChangeSize = true;
        public bool disableOnStart = true;
        [SerializeField] RelativeLayout_Settings settings;

        private RectTransform rect;
        private RectTransform canvasRect;
        private Vector3 position;
        private DrivenRectTransformTracker rcTracker;
        private Vector3[] corners;

        private bool lockTop = false;
        private bool lockBottom = false;
        private bool lockLeft = false;
        private bool lockRight = false;

        public bool FreeMovementMode { get; set; }
        public RectTransform Rect { get { if (rect == null) rect = GetComponent<RectTransform>(); return rect; } }
        public RectTransform CanvasRect { get { return canvasRect; } }
        public int OffsetTextSize { get { if (settings != null) return settings.offsetTextFontSize; return 12; } }
        public Color OffsetTextColor { get { if (settings != null) return settings.offsetTextColor; return Color.gray; } }
        public KeyCode FreeMovementKey { get { if (settings != null) return settings.freeMovementKeyCode; return KeyCode.LeftShift; } }
        public bool DoLockParameters { get { if (settings != null) return settings.lockParameters; return false; } }
        public bool RoundOffsetToInt { get { if (settings != null) return settings.roundOffsetToInt; return false; } }

        private void Start()
        {
            Initialize();
            ValidateTargets();
            OnReferenceChange();

            if (Application.isPlaying && disableOnStart)
            {
                enabled = false;
            }
#if UNITY_EDITOR
            else
            {
                EditorApplication.update -= OnReferenceChange;
                EditorApplication.update += OnReferenceChange;
            }
#endif
        }
#if UNITY_EDITOR
        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                EditorApplication.update -= OnReferenceChange;
                EditorApplication.update += OnReferenceChange;
            }
        }
#endif
        private void OnDisable()
        {
            if (rect != null && canvasRect != null)
            {
                AdjustPosition();
                AdjustSize();
            }
#if UNITY_EDITOR
            rcTracker.Clear();
            EditorApplication.update -= OnReferenceChange;
#endif
        }
        private void LateUpdate()
        {
            if (!disableOnStart)
            {
                OnReferenceChange();
            }
        }
        private void OnReferenceChange()
        {
            if (rect == null || canvasRect == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (FreeMovementMode)
            {
                UpdateOffsets();
            }
            else
#endif
            {
                UpdateValues();
                AdjustPosition();
                AdjustSize();
                if (DoLockParameters) LockParameters();
            }
        }
        private void Initialize()
        {
            if (canvasRect != null) return;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                if (canvas == null)
                {
                    Debug.LogWarning("[REF. LAYOUT] Canvas is not parent of this rect");
                    enabled = false;
                    return;
                }
            }
            canvasRect = canvas.GetComponent<RectTransform>();
            rect = GetComponent<RectTransform>();
            corners = new Vector3[4];
            position = rect.anchoredPosition;
        }
        public void ValidateTargets()
        {
            if (rect == null) rect = GetComponent<RectTransform>();

            topReference.ValidateReference(Edge.TOP, rect);
            bottomReference.ValidateReference(Edge.BOTTOM, rect);
            leftReference.ValidateReference(Edge.LEFT, rect);
            rightReference.ValidateReference(Edge.RIGHT, rect);
        }
        public ReferenceTarget[] GetReferenceTargets(bool vertical)
        {
            if (vertical)
            {
                return new ReferenceTarget[] { topReference, bottomReference };
            }
            else
            {
                return new ReferenceTarget[] { leftReference, rightReference };
            }
        }
        public void SetReference(Edge edge, RectTransform target, Edge targetEdge, bool moveToEdge = false)
        {
            if (target == null || edge == Edge.NONE || targetEdge == Edge.NONE) return;

            ref ReferenceTarget reference = ref GetReference(edge);
            reference.target = target;
            reference.edge = targetEdge;
            if (moveToEdge == false)
            {
                SetOffset(edge, ref reference);
            }
            ValidateTargets();
            OnReferenceChange();
        }
        public void RemoveReference(Edge edge)
        {
            if (edge != Edge.TOP && edge != Edge.BOTTOM && edge != Edge.LEFT && edge != Edge.RIGHT) return;

            ref ReferenceTarget reference = ref GetReference(edge);
            reference.target = null;
            OnReferenceChange();
        }
        private void SetOffset(Edge edge, ref ReferenceTarget reference)
        {
            if (reference.target == null) return;

            if (edge == Edge.TOP || edge == Edge.RIGHT)
            {
                reference.offset = -(rect.GetEdgePointInCanvas(edge, canvasRect) - reference.GetAlignPoint(canvasRect));
            }
            else
            {
                reference.offset = rect.GetEdgePointInCanvas(edge, canvasRect) - reference.GetAlignPoint(canvasRect);
            }
            if (RoundOffsetToInt)
            {
                reference.offset = Mathf.RoundToInt(reference.offset);
            }
        }
        private void UpdateOffsets()
        {
            SetOffset(Edge.TOP, ref topReference);
            SetOffset(Edge.BOTTOM, ref bottomReference);
            SetOffset(Edge.LEFT, ref leftReference);
            SetOffset(Edge.RIGHT, ref rightReference);
        }
        private void UpdateValues()
        {
            if (topReference.HasEmptyReference)
            {
                topReference.edge = Edge.NONE;
                topReference.offset = 0f;
            }
            if (bottomReference.HasEmptyReference)
            {
                bottomReference.edge = Edge.NONE;
                bottomReference.offset = 0f;
            }
            if (leftReference.HasEmptyReference)
            {
                leftReference.edge = Edge.NONE;
                leftReference.offset = 0f;
            }
            if (rightReference.HasEmptyReference)
            {
                rightReference.edge = Edge.NONE;
                rightReference.offset = 0f;
            }

            lockTop = topReference.edge != Edge.NONE;
            lockBottom = bottomReference.edge != Edge.NONE;
            lockLeft = leftReference.edge != Edge.NONE;
            lockRight = rightReference.edge != Edge.NONE;
        }
        private void AdjustPosition(bool updateVetical = true, bool updateHorizontal = true)
        {
            position = GetPositionOnCanvas(transform.position);
            if (updateVetical && topReference.IsUsed)
            {
                position.y = topReference.target.GetEdgePointInCanvas(topReference.edge, canvasRect) - rect.rect.height * (1f - rect.pivot.y) - topReference.offset;
            }
            if (updateVetical && bottomReference.IsUsed)
            {
                position.y = bottomReference.target.GetEdgePointInCanvas(bottomReference.edge, canvasRect) - rect.rect.height * (0f - rect.pivot.y) + bottomReference.offset;
            }
            if (updateHorizontal && leftReference.IsUsed)
            {
                position.x = leftReference.target.GetEdgePointInCanvas(leftReference.edge, canvasRect) - rect.rect.width * (0f - rect.pivot.x) + leftReference.offset;
            }
            if (updateHorizontal && rightReference.IsUsed)
            {
                position.x = rightReference.target.GetEdgePointInCanvas(rightReference.edge, canvasRect) - rect.rect.width * (1f - rect.pivot.x) - rightReference.offset;
            }
            transform.position = canvasRect.TransformPoint(position);
        }
        private void AdjustSize(bool updateVetical = true, bool updateHorizontal = true)
        {
            Vector2 anchorMin = rect.anchorMin;
            Vector2 anchorMax = rect.anchorMax;
            Vector2 size = rect.sizeDelta;

            //ADJUST HEIGHT
            if (updateVetical)
            {
                if (lockTop && lockBottom)
                {
                    anchorMin.y = 0.5f;
                    anchorMax.y = 0.5f;

                    size.y = topReference.GetAlignPoint(canvasRect) - topReference.offset - bottomReference.GetAlignPoint(canvasRect) - bottomReference.offset;
                }
                else if (lockTop)
                {
                    anchorMin.y = 0.5f;
                    anchorMax.y = 0.5f;
                    //anchorMax.y = Mathf.Max(anchorMax.y, anchorMin.y);
                }
                else if (lockBottom)
                {
                    anchorMin.y = 0.5f;
                    anchorMax.y = 0.5f;
                    //anchorMin.y = Mathf.Min(anchorMax.y, anchorMin.y);
                }
            }

            //ADJUST WIDTH
            if (updateHorizontal)
            {
                if (lockLeft && lockRight)
                {
                    anchorMin.x = 0.5f;
                    anchorMax.x = 0.5f;
                    size.x = rightReference.GetAlignPoint(canvasRect) - rightReference.offset - leftReference.GetAlignPoint(canvasRect) - leftReference.offset;
                }
                else if (lockLeft || lockRight)
                {
                    anchorMin.x = 0.5f;
                    anchorMax.x = 0.5f;
                    //anchorMin.x = Mathf.Min(anchorMax.x, anchorMin.x);
                }
                else if (lockRight)
                {
                    anchorMin.x = 0.5f;
                    anchorMax.x = 0.5f;
                    //anchorMax.x = Mathf.Max(anchorMax.x, anchorMin.x);
                }
            }
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = size;
        }
        //The original designe intented to lock RectTransform parameters on inspector. However, this method causes the elements to move around when you Start the application, so shouldn't be used until fixed.
        private void LockParameters()
        {
            rcTracker.Clear();

            if (lockTop || lockBottom)
            {
                rcTracker.Add(this, rect, DrivenTransformProperties.AnchoredPositionY);
                rcTracker.Add(this, rect, DrivenTransformProperties.AnchorMaxY);
                rcTracker.Add(this, rect, DrivenTransformProperties.AnchorMinY);
            }
            if (lockTop && lockBottom) rcTracker.Add(this, rect, DrivenTransformProperties.SizeDeltaY);

            if (lockLeft || lockRight)
            {
                rcTracker.Add(this, rect, DrivenTransformProperties.AnchoredPositionX);
                rcTracker.Add(this, rect, DrivenTransformProperties.AnchorMinX);
                rcTracker.Add(this, rect, DrivenTransformProperties.AnchorMaxX);
            }
            if (lockLeft && lockRight) rcTracker.Add(this, rect, DrivenTransformProperties.SizeDeltaX);
        }
        private Vector2 GetPositionOnCanvas(Vector3 position)
        {
            return canvasRect.InverseTransformPoint(position);
        }
        private ref ReferenceTarget GetReference(Edge edge)
        {
            switch (edge)
            {
                case Edge.TOP:
                    return ref topReference;
                case Edge.BOTTOM:
                    return ref bottomReference;
                case Edge.LEFT:
                    return ref leftReference;
                case Edge.RIGHT:
                    return ref rightReference;
            }
            return ref topReference;
        }
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!enabled)
                return;
            if (rect == null) rect = GetComponent<RectTransform>();

            if (lockTop)
            {
                DrawAlignmentLines(Edge.TOP, topReference, RelativeLayout_Utils.GetOffsetTextStyle(Edge.TOP, OffsetTextColor, OffsetTextSize));
            }
            if (lockBottom)
            {
                DrawAlignmentLines(Edge.BOTTOM, bottomReference, RelativeLayout_Utils.GetOffsetTextStyle(Edge.BOTTOM, OffsetTextColor, OffsetTextSize));
            }
            if (lockLeft)
            {
                DrawAlignmentLines(Edge.LEFT, leftReference, RelativeLayout_Utils.GetOffsetTextStyle(Edge.LEFT, OffsetTextColor, OffsetTextSize));
            }
            if (lockRight)
            {
                DrawAlignmentLines(Edge.RIGHT, rightReference, RelativeLayout_Utils.GetOffsetTextStyle(Edge.RIGHT, OffsetTextColor, OffsetTextSize));
            }
        }
        private void DrawAlignmentLines(Edge edge, ReferenceTarget target, GUIStyle textStyle)
        {
            if (target.target == null || target.edge == Edge.NONE)
            {
                return;
            }
            //CALCULATE POINTS
            Vector3[] myEdge = rect.GetEdgeWorldCorners(edge, corners);
            Vector3[] targetEdge = target.target.GetEdgeWorldCorners(target.edge, corners);
            Vector3 offsetLineOrigin = Vector3.Lerp(myEdge[0], myEdge[1], 0.5f);
            Vector3 offsetLineTarget = offsetLineOrigin;

            if (edge.IsVertical())
            {
                targetEdge[0].x = Mathf.Min(targetEdge[0].x, myEdge[0].x);
                targetEdge[1].x = Mathf.Max(targetEdge[1].x, myEdge[1].x);
                offsetLineTarget.y = targetEdge[0].y;
            }
            else if (edge.IsHorizontal())
            {
                targetEdge[0].y = Mathf.Min(targetEdge[0].y, myEdge[0].y);
                targetEdge[1].y = Mathf.Max(targetEdge[1].y, myEdge[1].y);
                offsetLineTarget.x = targetEdge[0].x;
            }

            //DRAW REFERENCE LINES
            Gizmos.color = RelativeLayout_Utils.REFERENCE_EDGE_COLOR;
            Gizmos.DrawLine(myEdge[0], myEdge[1]);
            Gizmos.color = RelativeLayout_Utils.LOCK_EDGE_COLOR;
            Gizmos.DrawLine(targetEdge[0], targetEdge[1]);

            //DRAW DOTTED LINE
            if(Mathf.Abs(target.offset) > 0.001f)
            {
                DrawGizmoDotedLine(offsetLineOrigin, offsetLineTarget);
                DrawGizmoArrow(offsetLineOrigin, offsetLineTarget);
            }

            //DRAW OFFSET LABLE
            Handles.BeginGUI();
            Handles.Label(Vector3.Lerp(offsetLineOrigin, offsetLineTarget, 0.5f), target.offset.ToString(), textStyle);
            Handles.EndGUI();
        }
        private void DrawGizmoDotedLine(Vector3 origin, Vector3 target)
        {
            float dotLength = canvasRect.TransformVector(RelativeLayout_Utils.DOT_LENGTH, 0f, 0f).x;
            Gizmos.color = RelativeLayout_Utils.DOTTED_LINE_COLOR;
            Vector3 dotOrigin = origin;
            Vector3 dotTarget = Vector3.MoveTowards(dotOrigin, target, dotLength);
            float maxLength = (target - origin).sqrMagnitude;
            while ((dotTarget - origin).sqrMagnitude < maxLength)
            {
                Gizmos.DrawLine(dotOrigin, dotTarget);
                dotOrigin = Vector3.MoveTowards(dotTarget, target, dotLength);
                dotTarget = Vector3.MoveTowards(dotOrigin, target, dotLength);
            }
            Gizmos.DrawLine(dotOrigin, target);
        }
        private void DrawGizmoArrow(Vector3 origin, Vector3 target)
        {
            float arrowLength = canvasRect.TransformVector(RelativeLayout_Utils.ARROW_LENGTH, 0f, 0f).x;
            Gizmos.color = RelativeLayout_Utils.ARROW_COLOR;
            Vector3 arrowEnd = target;
            Vector3 arrowLeft = Quaternion.Euler(0, 0, -RelativeLayout_Utils.ARROW_ANGLE) * (Vector3.MoveTowards(target, origin, arrowLength) - target) + target;
            Vector3 arrowRight = Quaternion.Euler(0, 0, RelativeLayout_Utils.ARROW_ANGLE) * (Vector3.MoveTowards(target, origin, arrowLength) - target) + target;
            Gizmos.DrawLine(arrowEnd, arrowLeft);
            Gizmos.DrawLine(arrowEnd, arrowRight);
            Gizmos.DrawLine(arrowRight, arrowLeft);
        }
#endif
    }
}