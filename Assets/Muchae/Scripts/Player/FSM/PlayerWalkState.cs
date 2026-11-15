using UnityEngine;

namespace muchae
{
    public class PlayerWalkState : PlayerStateBase
    {
        private const float InputThreshold = 0.1f;

        public override void EnterState(PlayerMovement movement)
        {
            Debug.Log("현재 상태 : 걷기");
            movement.CurrentSpeed = movement.WalkSpeed;
        }

        public override void UpdateState(PlayerMovement movement)
        {
            if (movement.MoveInput.magnitude < InputThreshold)
            {
                movement.ChangeState(movement.IdleState);
                return;
            }

            if (movement.RunInput)
            {
                movement.ChangeState(movement.RunState);
                return;
            }

            movement.CurrentSpeed = movement.WalkSpeed;
        }

        public override void ExitState(PlayerMovement movement)
        {
        }
    }
}