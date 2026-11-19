using UnityEngine;

namespace muchae
{
    public class PlayerWalkState : PlayerStateBase
    {
        private const float InputThreshold = 0.1f;

        public override void EnterState(PlayerMovement movement)
        {
            Debug.Log("현재 상태 : 걷기");
            movement.SetAnimationState(PlayerAnimationState.Walk);
            movement.CurrentSpeed = movement.WalkSpeed;
        }

        public override void UpdateState(PlayerMovement movement)
        {
            if (movement.MoveInput.magnitude < InputThreshold)
            {
                // 인풋값이 0.1보다 낮을 경우 Idle 로 전환
                movement.ChangeState(movement.IdleState);
                return;
            }

            if (movement.RunInput)
            {
                // 달리기 버튼이 눌렸을 경우 달리기로 전환
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