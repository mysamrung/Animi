using Animi.Core;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;


namespace Animi.Editor
{
    public class AnimiEdgeBase : Edge
    {
        [System.NonSerialized]
        public SerializedObject serializedObject;

        [AnimiHideInspector]
        public long hashId;

        protected T InitalizeSerializedObject<T>()
        {
            AnimiCustomEditor attribute = GetType().GetCustomAttributes(typeof(AnimiCustomEditor)).FirstOrDefault() as AnimiCustomEditor;
            var dataObject = Activator.CreateInstance(typeof(T));
            serializedObject = new SerializedObject(dataObject as AnimiEdgeMotionBehaviour);

            return (T)dataObject;
        }

        public virtual void OnAnimiInspectorGUIEdge()
        {
            var iter = serializedObject.GetIterator();
            iter.NextVisible(true);
            while (iter.NextVisible(false))
            {
                bool hideInspector = false;

                var targetType = serializedObject.targetObject.GetType();
                var fieldInfo = targetType.GetField(iter.name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    var attributes = fieldInfo.GetCustomAttributes(false);
                    foreach (var attr in attributes)
                    {
                        if(attr is HideInInspector)
                        {
                            hideInspector = true;
                            continue;
                        }
                    }
                }
                if(!hideInspector)
                    EditorGUILayout.PropertyField(iter, true);
            }
        }
    }
}
