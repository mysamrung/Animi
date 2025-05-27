using Animi.Core;
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

        public override void PortRefresh()
        {
            if (animiEdgeMotionBehaviour.from != null)
            {
                (animiEdgeMotionBehaviour.from as AnimiMotionBehaviour).transition.Remove(animiEdgeMotionBehaviour);
            }

            if (input != null && input.node is AnimiMotionNode inputNode)
            {
                animiEdgeMotionBehaviour.from = inputNode.animiMotionBehaviour;
                inputNode.animiMotionBehaviour.transition.Add(animiEdgeMotionBehaviour);
            }

            if (output != null && output.node is AnimiMotionNode outputNode)
            {
                animiEdgeMotionBehaviour.to = outputNode.animiMotionBehaviour;
            }
        }
    }
}