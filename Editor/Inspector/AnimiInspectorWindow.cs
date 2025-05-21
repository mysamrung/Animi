using Animi.Core;
using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Animi.Editor
{
    public class AnimiInspectorWindow : EditorWindow
    {
        public static GUIStyle HeaderStyle
        {
            get
            {
                if (headerStyle == null)
                {
                    headerStyle = new GUIStyle();
                    headerStyle.normal.textColor = Color.white;
                    headerStyle.fontSize = 24;
                }

                return headerStyle;
            }
        }
        private static GUIStyle headerStyle;


        private AnimiEditor animiEditor;

        private Vector2 scrollPosition;

        private bool showDetail = true;

        [MenuItem("Animi/Animi Inspector")]
        public static void Open()
        {
            AnimiInspectorWindow animiWindow = (AnimiInspectorWindow)EditorWindow.GetWindow(typeof(AnimiInspectorWindow));
            animiWindow.Show();
        }

        public void OnGUI()
        {
            if (animiEditor == null)
                animiEditor = (AnimiEditor)EditorWindow.GetWindow(typeof(AnimiEditor));

            if (animiEditor == null || animiEditor.graphView == null)
                return;

            if (animiEditor.graphView.selection == null || animiEditor.graphView.selection.Count <= 0)
                return;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            AnimiNodeBase nodeBase = animiEditor.graphView.selection[0] as AnimiNodeBase;
            if (nodeBase != null)
            {

                showDetail = EditorGUILayout.BeginFoldoutHeaderGroup(showDetail, this.GetType().Name);
                if (showDetail)
                    nodeBase.OnAnimiInspectorGUINode();

                EditorGUILayout.EndFoldoutHeaderGroup();

                GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
                nodeBase.OnAnimiInspectorGUIComponent();

                Rect buttonRect = GUILayoutUtility.GetLastRect();
                buttonRect.x = (this.position.width / 2) - (125);
                buttonRect.y += 25;
                buttonRect.width = 250;
                buttonRect.height = 25;

                if (GUI.Button(buttonRect, "Add Component"))
                {
                    var dropdown = new AnimiComponentDropdown(new AdvancedDropdownState());
                    dropdown.onSelectedCallBack += (Type type) =>
                    {
                        AnimiComponent comp = (AnimiComponent)Activator.CreateInstance(type);
                        nodeBase.AddComponent(comp);
                    };

                    dropdown.Show(buttonRect);
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
