using Animi.Core;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Animi.Editor
{
    [AnimiCustomEditor(typeof(AnimiEdgeMotionBehaviour))]
    public class AnimiMotionEdge : AnimiEdgeBase
    {
        public AnimiEdgeMotionBehaviour animiEdgeMotionBehaviour;
        public AnimiMotionEdge()
        {
            animiEdgeMotionBehaviour = InitalizeSerializedObject<AnimiEdgeMotionBehaviour>();
        }
        public AnimiMotionEdge(AnimiEdgeBaseBehaviour dataObject)
        {
            serializedObject = new SerializedObject(dataObject);
            animiEdgeMotionBehaviour = dataObject as AnimiEdgeMotionBehaviour;
        }

        public override void PortRefresh()
        {
            if (animiEdgeMotionBehaviour.from != null)
            {
                (animiEdgeMotionBehaviour.from as AnimiMotionBehaviour).transition.Remove(animiEdgeMotionBehaviour);
            }

            if (input != null && input.node is AnimiMotionNode inputNode)
            {
                animiEdgeMotionBehaviour.from = inputNode.animiMotionBehaviour;
                animiEdgeMotionBehaviour.fromPortName = input.portName;
                inputNode.animiMotionBehaviour.transition.Add(animiEdgeMotionBehaviour);
            }

            if (output != null && output.node is AnimiMotionNode outputNode)
            {
                animiEdgeMotionBehaviour.to = outputNode.animiMotionBehaviour;
                animiEdgeMotionBehaviour.toPortName = output.portName;
            }
        }
    }
}