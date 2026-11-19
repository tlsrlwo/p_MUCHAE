using UnityEngine;

namespace muchae
{
    public class PlayerIdleState : PlayerStateBase
    {
        private const float InputThreshold = 0.1f;

        private float _idleTimer;
        private float _nextIdleTime;

        public override void EnterState(PlayerMovement movement)
        {
            Debug.Log("현재 상태 : 기본");
            movement.SetAnimationState(PlayerAnimationState.Idle);

            // Idle애니메이션 3 가지 중에서 랜덤으로 하나를 선택
            movement.SetIdleVariant(Random.Range(0, 3));

            movement.CurrentSpeed = 0f;

            // 타이머를 설정
            _idleTimer = 0f;
            _nextIdleTime = Random.Range(4f, 10f);            
        }

        public override void UpdateState(PlayerMovement movement)
        {
            // 이동 입력이 있을 때만 Walk 또는 Run으로 전환
            if (movement.MoveInput.magnitude >= InputThreshold)
            {
                if (movement.RunInput)
                    movement.ChangeState(movement.RunState);
                else
                    movement.ChangeState(movement.WalkState);

                return;
            }

             // 입력이 없으면 Idle 유지 및 애니메이션 변경 타이머 갱신
            movement.CurrentSpeed = 0f;
            _idleTimer += Time.deltaTime;

            if (_idleTimer >= _nextIdleTime)
            {
                movement.SetIdleVariant(Random.Range(0, 3));
                _idleTimer = 0f;
                _nextIdleTime = Random.Range(4f, 10f);
            }
        }

        public override void ExitState(PlayerMovement movement)
        {
        }
    }
}