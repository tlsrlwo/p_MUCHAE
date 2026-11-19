using UnityEngine;

namespace muchae
{
    public class PlayerRunState : PlayerStateBase
    {
        private const float InputThreshold = 0.1f;

        public override void EnterState(PlayerMovement movement)
        {
            Debug.Log("현재 상태 : 달리기");
            movement.SetAnimationState(PlayerAnimationState.Run);
            movement.CurrentSpeed = movement.RunSpeed;
        }

        public override void UpdateState(PlayerMovement movement)
        {
            if (movement.MoveInput.magnitude < InputThreshold)
            {
                movement.ChangeState(movement.IdleState);
                return;
            }

            if (!movement.RunInput)
            {
                movement.ChangeState(movement.WalkState);
                return;
            }

            movement.CurrentSpeed = movement.RunSpeed;
        }

        public override void ExitState(PlayerMovement movement)
        {
        }
    }
}