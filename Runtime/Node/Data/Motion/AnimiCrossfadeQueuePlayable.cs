using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Animi.Core
{
    public class AnimiCrossfadeQueuePlayable : PlayableBehaviour
    {
        private Playable mixer;
        private float timer;

        private bool isBlending;
        private bool isDone;

        private AnimiEdgeMotionBehaviour transition;

        public void CrossFade(AnimiEdgeMotionBehaviour transition, Playable owner, PlayableGraph graph)
        {
            owner.SetInputCount(1);

            mixer = AnimationMixerPlayable.Create(graph, 2);

            graph.Connect(mixer, 0, owner, 0);

            owner.SetInputWeight(0, 1);

            graph.Connect(AnimationClipPlayable.Create(graph, transition.from.animationClip), 0, mixer, 0);
            mixer.SetInputWeight(0, 1.0f);

            graph.Connect(AnimationClipPlayable.Create(graph, transition.to.animationClip), 0, mixer, 1);
            mixer.SetInputWeight(1, 1.0f);

            this.transition = transition;
            isBlending = false;
            isDone = false;
        }

        public override void PrepareFrame(Playable owner, FrameData info)
        {
            if (mixer.GetInputCount() == 0)
                return;

            timer += (float)info.deltaTime;

            if (!isBlending)
            {
                if (!transition.hasExitTime || (transition.hasExitTime && timer >= transition.exitTime))
                {
                    var currentClip = (AnimationClipPlayable)mixer.GetInput(1);
                    currentClip.SetTime(0);

                    transition.from.OnLeave();
                    transition.to.OnEntry();

                    isBlending = true;
                    timer = 0;
                }
                else
                {
                    transition.from.OnUpdate();
                }
            }

            if (isBlending && !isDone)
            {
                var currentClip = (AnimationClipPlayable)mixer.GetInput(1);
                var nextClip = (AnimationClipPlayable)mixer.GetInput(0);
                if (transition.blendType == AnimiEdgeMotionBehaviour.BlendType.Linear)
                {
                    float weight = Mathf.Clamp01(timer / transition.blendDuration);
                    mixer.SetInputWeight(0, weight);
                    mixer.SetInputWeight(1, 1.0f - weight);
                }
                else if (transition.blendType == AnimiEdgeMotionBehaviour.BlendType.Cubic)
                {
                    float weight = Mathf.Clamp01(timer / transition.blendDuration);
                    weight = weight * weight * (3f - 2f * weight);
                    mixer.SetInputWeight(0, weight);
                    mixer.SetInputWeight(1, 1.0f - weight);
                }

                if (timer >= transition.blendDuration)
                {
                    isBlending = false;
                    isDone = true;

                    transition.to.OnLeave();
                    timer = 0;
                }
            }
        }
    }
}