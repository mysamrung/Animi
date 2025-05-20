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

        [HideInInspector]
        public long hashId;

        protected void InitalizedSerializedObject<T>() {
            AnimiCustomEditor attribute = typeof(T).GetCustomAttributes(typeof(AnimiCustomEditor)).FirstOrDefault() as AnimiCustomEditor;
            var dataObject = Activator.CreateInstance(attribute.GetType()) as AnimiNodeBaseBehaviour;
            serializedObject = new SerializedObject(dataObject);
        }

        public virtual void OnAnimiInspectorGUINode() {
            var iter = serializedObject.GetIterator();
            iter.NextVisible(true);
            while (iter.NextVisible(false)) {
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