using UnityEngine;

namespace muchae
{
    public class PlayerJumpState : PlayerStateBase
    {
        public override void EnterState(PlayerMovement movement)
        {         
            Debug.Log("[PlayerJumpState] 진입");           
        }

        public override void UpdateState(PlayerMovement movement)
        {
            movement.ChangeState(movement.IdleState);
            return;
        }

        public override void ExitState(PlayerMovement movement)
        {
        }

    }
}