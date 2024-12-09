using System;
using UnityEditor;
using UnityEngine;

namespace Animi.Core {
    public abstract class AnimiComponent : ScriptableObject {

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

            if(showDetail) { 
                var iter = SerializedObject.GetIterator();
                iter.NextVisible(true);
                while (iter.NextVisible(false)) {
                    EditorGUILayout.PropertyField(iter, true);
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
#endif

        }
    }
}