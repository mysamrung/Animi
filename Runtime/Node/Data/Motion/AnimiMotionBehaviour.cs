using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Animi.Core {
    [System.Serializable]
    public class AnimiMotionBehaviour : AnimiNodeBaseBehaviour {

#if UNITY_EDITOR
        public string nodeName = "Motion Node";
#endif

        public AnimationClip animationClip;

        [HideInInspector]
        public List<AnimiEdgeMotionBehaviour> transition = new List<AnimiEdgeMotionBehaviour>();

        private AnimiMotionController playQueueSample;

        public override void OnInitialize()
        {
            playQueueSample = gameObject.GetComponent<AnimiMotionController>();
        }

        public override void OnEntry() {
            Debug.Log("Motion Entry");
            
            if(transition.Count > 0)
                playQueueSample.CrossFade(transition[0]);
        }
        public override void OnUpdate() {
            Debug.Log("Motion Update");
        }
        public override void OnLeave() {
            Debug.Log("Motion Leave");
        }
    }
}