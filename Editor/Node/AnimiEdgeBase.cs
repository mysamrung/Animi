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
    [AnimiCustomEditor(typeof(AnimiEdgeBaseBehaviour))]
    public class AnimiEdgeBase : Edge
    {
        [System.NonSerialized]
        public SerializedObject serializedObject;

        private AnimiEdgeBaseBehaviour animiEdgeBaseBehaviour;

        public AnimiEdgeBase()
        {
            animiEdgeBaseBehaviour = InitalizeSerializedObject<AnimiEdgeBaseBehaviour>();
        }

        public AnimiEdgeBase(AnimiEdgeBaseBehaviour dataObject)
        {
            serializedObject = new SerializedObject(dataObject);
            animiEdgeBaseBehaviour = dataObject;
        }

        public override void OnPortChanged(bool isInput)
        {
            base.OnPortChanged(isInput);
            PortRefresh();
        }

        public virtual void PortRefresh()
        {
            if (input != null && input.node is AnimiNodeBase inputNode)
            {
                animiEdgeBaseBehaviour.from = inputNode.serializedObject.targetObject as AnimiNodeBaseBehaviour;
            }

            if (output != null && output.node is AnimiNodeBase outputNode)
            {
                animiEdgeBaseBehaviour.to = outputNode.serializedObject.targetObject as AnimiNodeBaseBehaviour;
            }
        }

        protected T InitalizeSerializedObject<T>()
        {
            AnimiCustomEditor attribute = GetType().GetCustomAttributes(typeof(AnimiCustomEditor)).FirstOrDefault() as AnimiCustomEditor;
            var dataObject = Activator.CreateInstance(typeof(T));
            serializedObject = new SerializedObject(dataObject as AnimiEdgeBaseBehaviour);

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
