using Animi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.XR;

namespace Animi.Editor {
    public abstract class AnimiNodeBase : Node {

        [System.NonSerialized]
        protected List<AnimiComponent> components = new List<AnimiComponent>();

        [System.NonSerialized]
        public SerializedObject serializedObject;

        public AnimiNodeBase()
        {
            InitializeData(null);
            InitializeNode();
        }

        public AnimiNodeBase(AnimiNodeBaseBehaviour dataObject)
        {
            InitializeData(dataObject);
            InitializeNode();
        }

        protected T InitializeSerializedObject<T>()
        {
            AnimiCustomEditor attribute = GetType().GetCustomAttributes(typeof(AnimiCustomEditor)).FirstOrDefault() as AnimiCustomEditor;
            var dataObject = Activator.CreateInstance(typeof(T));
            serializedObject = new SerializedObject(dataObject as AnimiNodeBaseBehaviour);

            return (T)dataObject;
        }


        protected virtual void InitializeNode()
        {

        }

        protected virtual void InitializeData(AnimiNodeBaseBehaviour dataObject)
        {

        }

        public virtual void OnAnimiInspectorGUINode() {
            var iter = serializedObject.GetIterator();
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
            }
        }

        public virtual void OnAnimiInspectorGUIComponent() {
            foreach (AnimiComponent comp in components) {
                comp.OnAnimiInspectorGUI();
                GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
            }
        }

        public void AddComponent(AnimiComponent component) {
            components.Add(component);
        }

        public void AddComponent<T>() where T : AnimiComponent, new(){
            components.Add((AnimiComponent)new T());
        }

        public void RemoveComponent<T>() where T : AnimiComponent {
            components.RemoveAll(s => s is T);
        }

        public virtual ScriptableObject GetNodeData() {
            return null;
        }

    }
}