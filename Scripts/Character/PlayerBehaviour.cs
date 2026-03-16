using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehaviour : MonoBehaviour
{
    private CharacterController characterController;
    private Animator animator;

    private InputAction moveAction;
    private InputAction combatAction;

    [SerializeField]
    private CharacterMovementStats moveStats;
    
    [SerializeField]
    private CharacterAttackStats attackStats;

    [SerializeField]
    private CharacterWeapon characterWeapon;

    private void Start() {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        PlayerInput playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions.FindAction("Move");
        combatAction = playerInput.actions.FindAction("Fire");

        var playerMovementStateMachines = animator.GetBehaviours<PlayerMovementStateMachine>();
        foreach(var playerMovementStateMachine in playerMovementStateMachines) {
            playerMovementStateMachine.Setup(moveAction, characterController, moveStats);
        }

        var playerCombatStateMachines = animator.GetBehaviours<PlayerCombatStateMachine>();
        foreach (var playerCombatStateMachine in playerCombatStateMachines) {
            playerCombatStateMachine.Setup(combatAction, moveAction, characterController);
        }
    }
}
