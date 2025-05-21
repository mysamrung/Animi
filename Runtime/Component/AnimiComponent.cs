using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Animi.Core {
    public abstract class AnimiComponent : ScriptableObject {
        private static float ContentHeaderOffset = 5;

        protected SerializedObject SerializedObject {
            get {
                if (serializedObject == null) {
                    serializedObject = new SerializedObject(this);
                }
                return serializedObject;
            }
        }

        [System.NonSerialized]
        private SerializedObject serializedObject;

#if UNITY_EDITOR
        private bool showDetail = true;
#endif
        public virtual void OnAnimiInspectorGUI() 
        {
#if UNITY_EDITOR
            showDetail = EditorGUILayout.BeginFoldoutHeaderGroup(showDetail, this.GetType().Name);

            Rect headerRect = GUILayoutUtility.GetLastRect();
            // Define the button size
            float buttonWidth = 60f;
            float buttonHeight = 16f;

            // Position the button on the right side of the header rect
            Rect buttonRect = new Rect(headerRect.xMax - buttonWidth - 6f, headerRect.y + 2f, buttonWidth, buttonHeight);

            // Draw the button
            if (GUI.Button(buttonRect, "Remove")) {
            }

            EditorGUILayout.Space(ContentHeaderOffset);

            if (showDetail) { 
                var iter = SerializedObject.GetIterator();
                iter.NextVisible(true);
                while (iter.NextVisible(false)) {
                    bool hideInspector = false;

                    var targetType = serializedObject.targetObject.GetType();
                    var fieldInfo = targetType.GetField(iter.name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (fieldInfo != null)
                    {
                        var attributes = fieldInfo.GetCustomAttributes(false);
                        foreach (var attr in attributes)
                        {
                            if (attr is HideInInspector)
                            {
                                hideInspector = true;
                                continue;
                            }
                        }
                    }
                    if (!hideInspector)
                        EditorGUILayout.PropertyField(iter, true);

                    EditorGUILayout.PropertyField(iter, true);
                }
            }


            EditorGUILayout.EndFoldoutHeaderGroup();
#endif

        }
    }
}