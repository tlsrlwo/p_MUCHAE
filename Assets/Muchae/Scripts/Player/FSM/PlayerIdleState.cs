using UnityEngine;

namespace muchae
{
    public class PlayerIdleState : PlayerStateBase
    {
        private const float InputThreshold = 0.1f;

        public override void EnterState(PlayerMovement movement)
        {
            Debug.Log("현재 상태 : 기본");
            movement.CurrentSpeed = 0f;
        }

        public override void UpdateState(PlayerMovement movement)
        {
            if (movement.MoveInput.magnitude < InputThreshold)
            {
                movement.CurrentSpeed = 0f;
                return;
            }

            if (movement.RunInput)
                movement.ChangeState(movement.RunState);
            else
                movement.ChangeState(movement.WalkState);
        }

        public override void ExitState(PlayerMovement movement)
        {
        }
    }
}