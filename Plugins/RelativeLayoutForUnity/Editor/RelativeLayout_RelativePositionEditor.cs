using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RelativeLayout
{
    //[CustomEditor(typeof(RelativeLayout_RelativePosition))]
    public class RelativeLayout_RelativePositionEditor : Editor
    {
        public RelativeLayout_Settings settings;

        protected RelativeLayout_RelativePosition referencePosition;
        private SerializedProperty topReference;
        private SerializedProperty bottomReference;
        private SerializedProperty leftReference;
        private SerializedProperty rightReference;
        private SerializedProperty allowChangeSize;
        private SerializedProperty disableOnStart;

        protected virtual void OnEnable()
        {
            referencePosition = (RelativeLayout_RelativePosition)target;
            topReference = serializedObject.FindProperty("topReference");
            bottomReference = serializedObject.FindProperty("bottomReference");
            leftReference = serializedObject.FindProperty("leftReference");
            rightReference = serializedObject.FindProperty("rightReference");
            allowChangeSize = serializedObject.FindProperty("allowChangeSize");
            disableOnStart = serializedObject.FindProperty("disableOnStart");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            ShowAlignmentEditor(Edge.TOP, "Top Reference", settings.TopLockedImage, settings.TopUnlockedImage, ref referencePosition.topReference, topReference, referencePosition.bottomReference.IsUsed == false || referencePosition.allowChangeSize);
            GUILayout.Space(10f);
            ShowAlignmentEditor(Edge.BOTTOM, "Bottom Reference", settings.BottomLockedImage, settings.BottomUnlockedImage, ref referencePosition.bottomReference, bottomReference, referencePosition.topReference.IsUsed == false || referencePosition.allowChangeSize);
            GUILayout.Space(10f);
            ShowAlignmentEditor(Edge.LEFT, "Left Reference", settings.LeftLockedImage, settings.LeftUnlockedImage, ref referencePosition.leftReference, leftReference, referencePosition.rightReference.IsUsed == false || referencePosition.allowChangeSize);
            GUILayout.Space(10f);
            ShowAlignmentEditor(Edge.RIGHT, "Right Reference", settings.RightLockedImage, settings.RightUnlockedImage, ref referencePosition.rightReference, rightReference, referencePosition.leftReference.IsUsed == false || referencePosition.allowChangeSize);
            GUILayout.Space(10f);
            EditorGUILayout.PropertyField(allowChangeSize, new GUIContent("Control Size"));
            if (settings.allowRuntimeExecution)
            {
                EditorGUILayout.PropertyField(disableOnStart, new GUIContent("Disable On Start"));
                if (!disableOnStart.boolValue)
                {
                    EditorGUILayout.HelpBox("Executing on Runtime may cause an unexpected behaviour on UI elements that have a position relative to this one.", MessageType.Warning);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                referencePosition.ValidateTargets();
                EditorUtility.SetDirty(referencePosition);
            }
        }

        private void ShowAlignmentEditor(Edge edge, string alignmentLabel, Texture locked, Texture unlocked, ref ReferenceTarget alignment, SerializedProperty serializedProperty, bool canChange)
        {
            SerializedProperty target = serializedProperty.FindPropertyRelative("target");
            SerializedProperty offset = serializedProperty.FindPropertyRelative("offset");
            //HEADER AND TARGET FIELD
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(alignmentLabel, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.PropertyField(target, GUIContent.none);
            if (target.objectReferenceValue == null)
            {
                if (GUILayout.Button("P", EditorStyles.miniButton))
                {
                    referencePosition.SetReference(edge, referencePosition.Rect.parent.GetComponent<RectTransform>(), edge);
                }
            }
            else
            {
                if (GUILayout.Button("X", EditorStyles.miniButton))
                {
                    target.objectReferenceValue = null;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            //LEFT ICON
            GUILayout.Box((target.objectReferenceValue == null) ? unlocked : locked, GUILayout.Height(55f), GUILayout.Width(55f));
            EditorGUILayout.BeginVertical();
            GUILayout.Space(10f);
            if (target.objectReferenceValue != null)
            {
                //DISPLAY A WARNING TO AVOID UNINTERNATIONAL SIZE ADJUSTMENT
                if (!canChange)
                {
                    alignment.edge = Edge.NONE;
                    EditorGUILayout.HelpBox("Allow Control Size to add alignments on both sides", MessageType.Info);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    return;
                }
                //REFENCE POSITION BUTTONS
                EditorGUILayout.BeginHorizontal();
                int selection = settings.GetReferenceButtonsIndex(edge, alignment.edge);
                selection = GUILayout.Toolbar(selection, settings.GetButtonIcons(edge), settings.GetButtonStyle(GUI.skin.button));
                alignment.edge = settings.GetSelectedEdge(edge, selection);

                //REFERENCE DESCRIPTION
                EditorGUILayout.BeginVertical();
                GUILayout.Label(string.Format("{0} to {1}", edge, alignment.edge));
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("+");
                EditorGUILayout.PropertyField(offset, GUIContent.none);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
}