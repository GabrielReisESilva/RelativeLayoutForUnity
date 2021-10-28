using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEditor.SceneManagement;

namespace RelativeLayout
{
    [CustomEditor(typeof(RelativeLayout_RelativePosition))]
    [CanEditMultipleObjects]
    public class RelativeLayout_RelativePositionSceneEditor : RelativeLayout_RelativePositionEditor
    {
        private const float HANDLE_SIZE = 0.1f;
        private const float HANDLE_OFFSET = 10f;
        private const float HANDLE_LINE_THICKNESS = 2f;
        private const int NoneId = 0;
        private const int UpId = 1;
        private const int DownId = 2;
        private const int LeftId = 3;
        private const int RightId = 4;

        private readonly Vector2 OFFSET = new Vector2(HANDLE_OFFSET, HANDLE_OFFSET);
        private readonly Quaternion HANDLE_ROTATION = Quaternion.identity;
        private readonly Vector3 TopLeft = new Vector3(-1, 1);

        private Event currentEvent;
        private int selectedId = NoneId;
        private bool isDragging = false;
        private Vector3 lineTarget;
        private Edge targetEdge;
        private RectTransform rectTransformTarget;
        private RectTransform canvasRect;
        private Canvas canvas;
        private Camera sceneCamera;
        private float handleSize;
        private Vector2 offset;
        private Vector3[] corners;

        protected override void OnEnable()
        {
            base.OnEnable();
            canvas = referencePosition.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    Debug.LogError("Please add a Canvas on the scene");
                    return;
                }
            }
            canvasRect = canvas.GetComponent<RectTransform>();
            corners = new Vector3[4];
            //EditorSceneManager.
        }
        private void OnSceneGUI()
        {
            if (!referencePosition.enabled)
            {
                return;
            }
            if(canvasRect == null)
            {
                GetCanvas();
                return;
            }
            currentEvent = Event.current;

            //DRAW HANDLERS
            if (currentEvent.type == EventType.Repaint || currentEvent.type == EventType.Layout)
            {
                Vector3 center = referencePosition.transform.position + canvasRect.TransformVector(referencePosition.Rect.rect.size * (Vector2.one * 0.5f - referencePosition.Rect.pivot));
                handleSize = HANDLE_SIZE * HandleUtility.GetHandleSize(referencePosition.transform.position);
                offset = Vector2.one * 1.5f * handleSize + (referencePosition.Rect.rect.size + OFFSET) * canvasRect.localScale * 0.5f;

                DrawHandleRepaint(referencePosition.topReference, UpId, GetPositionById(UpId, center, offset), handleSize, currentEvent.type);
                DrawHandleRepaint(referencePosition.bottomReference, DownId, GetPositionById(DownId, center, offset), handleSize, currentEvent.type);
                DrawHandleRepaint(referencePosition.leftReference, LeftId, GetPositionById(LeftId, center, offset), handleSize, currentEvent.type);
                DrawHandleRepaint(referencePosition.rightReference, RightId, GetPositionById(RightId, center, offset), handleSize, currentEvent.type);

                if (isDragging && currentEvent.type == EventType.Repaint)
                {
                    if (rectTransformTarget != null)
                    {
                        DrawTargetOutline(rectTransformTarget);
                        DrawTargetEdgeLine(rectTransformTarget, targetEdge);
                    }
                    DrawLine(GetPositionById(selectedId, center, offset), lineTarget);
                    HandleUtility.Repaint();
                }
            }
            //ON MOUSE DOWN: CLICKING ON HANDLERS
            else if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
            {
                selectedId = HandleUtility.nearestControl;
                HandleUtility.Repaint();
            }
            //ON MOUSE DRAG: IF CLICKED ON A HANDLER, LOOK FOR TARGETS ON DRAG POSITION
            else if (currentEvent.type == EventType.MouseDrag && currentEvent.button == 0 && !referencePosition.FreeMovementMode)
            {
                if (selectedId < 1 || selectedId > 4) return;
                isDragging = true;
                rectTransformTarget = GetTarget(GetMousePosition(currentEvent.mousePosition), out lineTarget);
                HandleUtility.Repaint();
            }
            //ON MOUSE UP: IF FOUND A TARGET, THEN SET AS NEW REFERENCE FOR THAT EDGE
            else if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
            {
                if (referencePosition.FreeMovementMode)
                {
                    if(selectedId == HandleUtility.nearestControl)
                    {
                        Undo.RecordObject(target, "Remove Reference");
                        referencePosition.RemoveReference(IdToEdge(selectedId));
                    }
                }
                else if (rectTransformTarget != null)
                {
                    Undo.RecordObject(target, "Set Reference");
                    referencePosition.SetReference(IdToEdge(selectedId), rectTransformTarget, targetEdge);
                }
                selectedId = NoneId;
                isDragging = false;
            }
            //ON LEAVE WINDOW: CANCEL EVERYTHING IF MOUSE IS OUT OF THE SCENE VIEW
            else if (currentEvent.type == EventType.MouseLeaveWindow)
            {
                selectedId = NoneId;
                isDragging = false;
            }
            //ON PRESS FREE MOVEMENT KEY: ENABLE FREE MOVEMENT MODE
            else if (currentEvent.isKey && currentEvent.keyCode != KeyCode.None)
            {
                if (currentEvent.keyCode == settings.freeMovementKeyCode)
                {
                    if(currentEvent.type == EventType.KeyUp)
                    {
                        referencePosition.FreeMovementMode = false;
                    }
                    else
                    {
                        referencePosition.FreeMovementMode = true;
                    }
                }
            }
        }
        private void GetCanvas()
        {
            if(referencePosition.CanvasRect != null)
            {
                canvasRect = referencePosition.CanvasRect;
                canvas = canvasRect.GetComponent<Canvas>();
            }
        }
        private void DrawHandleRepaint(ReferenceTarget reference, int id, Vector3 position, float size, EventType currentEvent)
        {
            if(reference.target == null && !referencePosition.FreeMovementMode || reference.target != null && referencePosition.FreeMovementMode)
            {
                if (selectedId == id)
                {
                    Handles.color = settings.selectedHandleColor;
                    Handles.DotHandleCap(id, position, HANDLE_ROTATION, size, currentEvent);
                }
                else
                {
                    Handles.DrawSolidRectangleWithOutline(new Rect(position - Vector3.one * size, Vector2.one * 2f * size), settings.defaultHandleBackgroundColor, settings.defaultHandleOutlineColor);
                    Handles.color = settings.defaultHandleOutlineColor;
                    Handles.RectangleHandleCap(id, position, HANDLE_ROTATION, size, currentEvent);
                }
                if (referencePosition.FreeMovementMode)
                {
                    Handles.color = Color.red;
                    Handles.DrawLine(position + TopLeft * size, position - TopLeft * size, 2);
                    Handles.DrawLine(position + Vector3.one * size, position - Vector3.one * size, 2);
                }
            }
        }

        private Vector3 GetPositionById(int id, Vector3 center, Vector2 offset)
        {
            switch (id)
            {
                case UpId:
                    return center + Vector3.up * offset.y;
                case DownId:
                    return center + Vector3.down * offset.y;
                case LeftId:
                    return center + Vector3.left * offset.x;
                case RightId:
                    return center + Vector3.right * offset.x;
            }
            return center;
        }

        private void DrawLine(Vector3 origin, Vector3 target)
        {
            Handles.color = rectTransformTarget == null ? settings.targetRectOutlineColor : settings.targetEdgeOutlineColor;
            Handles.DrawLine(origin, target, HANDLE_LINE_THICKNESS);
        }

        private Vector3 GetMousePosition(Vector3 eventMousePosition)
        {
            return HandleUtility.GUIPointToWorldRay(eventMousePosition).origin;
        }

        private RectTransform GetTarget(Vector3 mousePosition, out Vector3 target)
        {
            if(sceneCamera == null)
            {
                sceneCamera = SceneView.lastActiveSceneView.camera;
            }

            target = mousePosition;
            Graphic frontGraphic = null;
            Vector3 mouseScreenPosition = sceneCamera.WorldToScreenPoint(mousePosition);
            IList<Graphic> graphics = GraphicRegistry.GetRaycastableGraphicsForCanvas(canvas);
            for (int i = 0; i < graphics.Count; i++)
            {
                Graphic graphic = graphics[i];

                if (!graphic.raycastTarget || graphic.canvasRenderer.cull || graphic.depth == -1)
                    continue;

                if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, mouseScreenPosition, sceneCamera, graphic.raycastPadding))
                    continue;

                if (sceneCamera != null && sceneCamera.WorldToScreenPoint(graphic.rectTransform.position).z > sceneCamera.farClipPlane)
                    continue;

                if (graphic.Raycast(mouseScreenPosition, sceneCamera))
                {
                    if (frontGraphic == null || frontGraphic.depth < graphic.depth)
                    {
                        frontGraphic = graphic;
                    }
                }
            }
            if (frontGraphic != null)
            {
                RectTransform targetRect = frontGraphic.rectTransform;
                if (selectedId == UpId || selectedId == DownId)
                {
                    targetEdge = RelativeLayout_Utils.GetClosestVerticalEdge(canvasRect.InverseTransformPoint(mousePosition).y, targetRect, canvasRect);
                    //Debug.Log($"Top {targetRect.GetEdgePointInCanvas(Edge.TOP, canvasRect)}, Bottom {targetRect.GetEdgePointInCanvas(Edge.BOTTOM, canvasRect)}, Mouse: {canvasRect.InverseTransformPoint(mousePosition).y}");
                }
                else if (selectedId == LeftId || selectedId == RightId)
                {
                    targetEdge = RelativeLayout_Utils.GetClosestHorizontalEdge(canvasRect.InverseTransformPoint(mousePosition).x, targetRect, canvasRect);
                }
                //Debug.Log($"Edge: {targetEdge}");
                target = frontGraphic.transform.position;
                return targetRect;
            }
            return null;
        }
        private void DrawTargetOutline(RectTransform rect)
        {
            Handles.color = settings.targetRectOutlineColor;
            rect.GetWorldCorners(corners);
            int previousIndex = corners.Length - 1;
            for (int i = 0; i < corners.Length; i++)
            {
                Handles.DrawLine(corners[previousIndex], corners[i], HANDLE_LINE_THICKNESS);
                previousIndex = i;
            }
        }
        private void DrawTargetEdgeLine(RectTransform rect, Edge edge)
        {
            Handles.color = settings.targetEdgeOutlineColor;
            Vector3[] edgePoints = rect.GetEdgeWorldCorners(edge, corners);
            Handles.DrawLine(edgePoints[0], edgePoints[1], 2f * HANDLE_LINE_THICKNESS);
        }
        private Edge IdToEdge(int id)
        {
            switch (id)
            {
                case UpId:
                    return Edge.TOP;
                case DownId:
                    return Edge.BOTTOM;
                case LeftId:
                    return Edge.LEFT;
                case RightId:
                    return Edge.RIGHT;
                default:
                    return Edge.NONE;
            }
        }
    }
}