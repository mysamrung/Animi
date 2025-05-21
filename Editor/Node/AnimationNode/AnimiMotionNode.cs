using Animi.Core;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Animi.Editor {
    public class AnimiMotionNode : AnimiNodeBase {

        public AnimiMotionBehaviour animiMotionBehaviour;

        public AnimiMotionNode() {
            this.title = "Motion Node";

            var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(AnimiNodeBase));
            inputPort.portName = "In";
            inputContainer.Add(inputPort);

            var outputPort = Port.Create<AnimiMotionEdge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(AnimiNodeBase));
            outputPort.portName = "Out";
            outputContainer.Add(outputPort);

            animiMotionBehaviour = InitializeSerializedObject<AnimiMotionBehaviour>();
        }

        public override void OnAnimiInspectorGUINode() {
            base.OnAnimiInspectorGUINode();

            if(serializedObject.hasModifiedProperties) {
                serializedObject.ApplyModifiedProperties();
                this.title = serializedObject.FindProperty("nodeName").stringValue;
            }
        }
    }

}