using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatStateMachine : StateMachineBehaviour { 

    [System.Serializable]
    public class CombatProperty {
        public Vector2 movementDuration = new Vector2(0, 1);
        public Vector2 hitDuration = new Vector2(0, 1);
        public Vector2 inputDuration = new Vector2(0, 1);

        public float moveSpeed;
        public float rotateSpeed;
    }

    private static readonly int AttackStateID = Animator.StringToHash("Attack");
    private InputAction combatAction;
    private InputAction moveAction;

    public CombatProperty combatProperty;

    private CharacterAttackStats characterAttackStats;
    private CharacterController characterController;
    private CharacterWeapon characterWeapon;

    public void Setup(InputAction combatAction, InputAction moveAction, CharacterController characterController, CharacterAttackStats characterAttackStats, CharacterWeapon characterWeapon) { 
        this.combatAction = combatAction;
        this.moveAction = moveAction;
        this.characterAttackStats = characterAttackStats;
        this.characterController = characterController;
        this.characterWeapon = characterWeapon;
    }

    public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (combatAction == null || animator.IsInTransition(0))
            return;

        Movement(animator.transform, animator, stateInfo);
    }

    private void Movement(Transform transform, Animator animator, AnimatorStateInfo stateInfo) {

        bool isPressed = combatAction.IsPressed();
        if(isPressed && stateInfo.normalizedTime >= combatProperty.inputDuration.x && stateInfo.normalizedTime <= combatProperty.inputDuration.y) {
            animator.SetTrigger(AttackStateID);
        }

        if(stateInfo.normalizedTime >= combatProperty.movementDuration.x && stateInfo.normalizedTime <= combatProperty.movementDuration.y) {
            Vector2 moveValue = moveAction.ReadValue<Vector2>();

            Vector3 direction = new Vector3(moveValue.x, 0, moveValue.y);
            direction = Camera.main.transform.TransformDirection(direction);
            direction.y = 0;
            direction.Normalize();

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), combatProperty.rotateSpeed * Time.deltaTime);

            characterController.Move(transform.forward * combatProperty.moveSpeed * Time.deltaTime);
        }
    }
}
