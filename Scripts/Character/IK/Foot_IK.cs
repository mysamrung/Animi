using UnityEngine;

/// <summary>
/// Handles foot IK (Inverse Kinematics) with ground detection for realistic foot placement on uneven surfaces.
/// Adjusts both foot position and rotation dynamically, along with pelvis height for natural animation.
/// </summary>
namespace FootIKSystem
{
    [RequireComponent(typeof(Animator))]
    public class FootIK : MonoBehaviour
    {
        [Header("Main")]
        [Range(0, 1)] public float Weight = 1f;

        [Header("Settings")]
        public float MaxStep = 0.5f;               // Max step height the foot can adapt to
        public float FootRadius = 0.15f;           // Radius for sphere cast (foot contact)
        public LayerMask Ground = 1;               // Ground layer for raycasting
        public float Offset = 0f;                  // Foot lift offset from ground

        [Header("Speed")]
        public float HipsPositionSpeed = 1f;       // Speed of pelvis adjustment
        public float FeetPositionSpeed = 2f;       // Speed of foot vertical position interpolation
        public float FeetRotationSpeed = 90f;      // Speed of foot rotation adjustment

        [Header("Weight")]
        [Range(0, 1)] public float HipsWeight = 0.75f;
        [Range(0, 1)] public float FootPositionWeight = 1f;
        [Range(0, 1)] public float FootRotationWeight = 1f;

        public bool ShowDebug = true;              // Toggle debug gizmos

        // Internal state variables
        Vector3 LIKPosition, RIKPosition, LNormal, RNormal;
        Quaternion LIKRotation, RIKRotation, LastLeftRotation, LastRightRotation;
        float LastRFootHeight, LastLFootHeight;

        Animator Anim;
        float Velocity;
        float FalloffWeight;
        float LastHeight;
        Vector3 LastPosition;
        bool LGrounded, RGrounded, IsGrounded;

        // Initialize references
        void Start()
        {
            Anim = GetComponent<Animator>();
        }

        // Called in fixed intervals (physics update)
        private void FixedUpdate()
        {
            if (Weight == 0 || !Anim) return;

            // Calculate movement velocity
            Vector3 Speed = (LastPosition - Anim.transform.position) / Time.fixedDeltaTime;
            Velocity = Mathf.Clamp(Speed.magnitude, 1, Speed.magnitude);
            LastPosition = Anim.transform.position;

            // Update IK target positions and rotations for both feet
            FeetSolver(HumanBodyBones.LeftFoot, ref LIKPosition, ref LNormal, ref LIKRotation, ref LGrounded);
            FeetSolver(HumanBodyBones.RightFoot, ref RIKPosition, ref RNormal, ref RIKRotation, ref RGrounded);

            // Update grounded state and IK weight blending
            GetGrounded();
        }

        // Called by Unity after animation is applied (ideal for IK logic)
        private void OnAnimatorIK(int layerIndex)
        {
            if (Weight == 0 || !Anim) return;

            // Adjust pelvis height based on foot contact
            MovePelvisHeight();

            // Apply IK to left foot
            MoveIK(AvatarIKGoal.LeftFoot, LIKPosition, LNormal, LIKRotation, ref LastLFootHeight, ref LastLeftRotation);

            // Apply IK to right foot
            MoveIK(AvatarIKGoal.RightFoot, RIKPosition, RNormal, RIKRotation, ref LastRFootHeight, ref LastRightRotation);
        }

        /// <summary>
        /// Adjust the pelvis height to match the lower foot for realistic grounding.
        /// </summary>
        private void MovePelvisHeight()
        {
            float LeftOffset = LIKPosition.y - Anim.transform.position.y;
            float RightOffset = RIKPosition.y - Anim.transform.position.y;
            float TotalOffset = Mathf.Min(LeftOffset, RightOffset);


            Vector3 NewPosition = Anim.bodyPosition;
            float NewHeight = TotalOffset * (HipsWeight * FalloffWeight);
            LastHeight = Mathf.MoveTowards(LastHeight, NewHeight, HipsPositionSpeed * Time.deltaTime);
            NewPosition.y += LastHeight + Offset;

            Anim.bodyPosition = NewPosition;
        }

        /// <summary>
        /// Move and rotate the foot IK goal to match ground position and slope.
        /// </summary>
        void MoveIK(AvatarIKGoal Foot, Vector3 IKPosition, Vector3 Normal, Quaternion IKRotation, ref float LastHeight, ref Quaternion LastRotation)
        {
            Vector3 Position = Anim.GetIKPosition(Foot);
            Quaternion Rotation = Anim.GetIKRotation(Foot);

            // Position adjustment (local to world)
            Position = Anim.transform.InverseTransformPoint(Position);
            IKPosition = Anim.transform.InverseTransformPoint(IKPosition);
            LastHeight = Mathf.MoveTowards(LastHeight, IKPosition.y, FeetPositionSpeed * Time.deltaTime);
            Position.y += LastHeight;
            Position = Anim.transform.TransformPoint(Position);
            Position += Normal * Offset;

            // Rotation adjustment based on ground normal
            Quaternion Relative = Quaternion.Inverse(IKRotation * Rotation) * Rotation;
            LastRotation = Quaternion.RotateTowards(LastRotation, Quaternion.Inverse(Relative), FeetRotationSpeed * Time.deltaTime);
            Rotation *= LastRotation;

            // Apply IK weights and final values
            Anim.SetIKPosition(Foot, Position);
            Anim.SetIKPositionWeight(Foot, FootPositionWeight * FalloffWeight);
            Anim.SetIKRotation(Foot, Rotation);
            Anim.SetIKRotationWeight(Foot, FootRotationWeight * FalloffWeight);
        }

        /// <summary>
        /// Check if either foot is grounded and adjust global weight blending.
        /// </summary>
        void GetGrounded()
        {
            IsGrounded = LGrounded || RGrounded;

            // Smoothly fade foot IK on/off based on grounded state
            FalloffWeight = LerpValue(FalloffWeight, IsGrounded ? 1f : 0f, 1f, 10f, Time.fixedDeltaTime) * Weight;
        }

        /// <summary>
        /// Smooth value interpolation based on separate speeds for increase/decrease.
        /// </summary>
        public float LerpValue(float Current, float Desired, float IncreaseSpeed, float DecreaseSpeed, float DeltaTime)
        {
            if (Current == Desired) return Desired;
            if (Current < Desired)
                return Mathf.MoveTowards(Current, Desired, (IncreaseSpeed * Velocity) * DeltaTime);
            else
                return Mathf.MoveTowards(Current, Desired, (DecreaseSpeed * Velocity) * DeltaTime);
        }

        /// <summary>
        /// Casts a sphere down from foot to detect the ground position and slope.
        /// </summary>
        private void FeetSolver(HumanBodyBones Foot, ref Vector3 IKPosition, ref Vector3 Normal, ref Quaternion IKRotation, ref bool Grounded)
        {
            Vector3 Position = Anim.GetBoneTransform(Foot).position;
            Position.y = Anim.transform.position.y + MaxStep;
            Position -= Normal * Offset;

            float FeetHeight = MaxStep;
            RaycastHit Hit;

            // Visual debug
            if (ShowDebug)
                Debug.DrawLine(Position, Position + Vector3.down * (MaxStep * 2), Color.yellow);

            // Detect ground using sphere cast
            if (Physics.SphereCast(Position, FootRadius, Vector3.down, out Hit, MaxStep * 2, Ground))
            {
                FeetHeight = Anim.transform.position.y - Hit.point.y;
                IKPosition = Hit.point;
                Normal = Hit.normal;

                if (ShowDebug)
                    Debug.DrawRay(Hit.point, Hit.normal, Color.blue);

                // Create rotation that aligns foot to slope
                Vector3 Axis = Vector3.Cross(Vector3.up, Hit.normal);
                float Angle = Vector3.Angle(Vector3.up, Hit.normal);
                IKRotation = Quaternion.AngleAxis(Angle, Axis);
            }

            Grounded = FeetHeight < MaxStep;

            if (!Grounded)
            {
                IKPosition.y = Anim.transform.position.y - MaxStep;
                IKRotation = Quaternion.identity;
            }
        }
    }
}