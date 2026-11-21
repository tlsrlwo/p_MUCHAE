using UnityEngine;

namespace muchae
{
    public class PlayerIdleState : PlayerStateBase
    {
        private const float InputThreshold = 0.1f;
        private const int IdleAnimCount = 3;                // 애니메이션 개수

        // 현재 재생 중인 Idle과 다음에 재생할 Idle의 인덱스
        private int _currentIdleIndex;
        private int _pendingIdleIndex = -1;

        private float _idleTimer;
        private float _nextIdleTime;

        public override void EnterState(PlayerMovement movement)
        {
            Debug.Log("현재 상태 : 기본");

            // Animator의 플레이어 상태를 Idle로 설정
            movement.SetAnimationState(PlayerAnimationState.Idle);

            // Idle 상태에서는 이동 속도를 0으로 설정
            movement.CurrentSpeed = 0f;

            // Idle에 진입할 때 세 애니메이션 중 하나를 무작위로 선택
            _currentIdleIndex = 0;
            // movement.PlayIdleVariant(_currentIdleIndex);

            // 대기 상태를 초기화하고 다음 전환까지의 시간을 설정
            _pendingIdleIndex = -1;
            ResetIdleTimer();
        }

        public override void UpdateState(PlayerMovement movement)
        {
            // 이동 입력이 들어오면 Walk 또는 Run 상태로 전환
            if (movement.MoveInput.sqrMagnitude > InputThreshold * InputThreshold)
            {
                if (movement.RunInput)
                    movement.ChangeState(movement.RunState);
                else
                    movement.ChangeState(movement.WalkState);

                return;
            }

            // 이동 입력이 없으면 계속 Idle 상태를 유지
            movement.CurrentSpeed = 0f;
            _idleTimer += Time.deltaTime;

            // 대기 시간이 끝났고 아직 다음 Idle을 예약하지 않았다면
            // 현재 애니메이션과 다른 Idle을 선택해 예약
            if (_pendingIdleIndex < 0 && _idleTimer >= _nextIdleTime)
            {
                _pendingIdleIndex = GetDifferentIdleIndex(_currentIdleIndex);
            }

            // 다음 Idle이 예약되어 있고 현재 클립이 루프 끝에 가까우면 전환
            if (_pendingIdleIndex >= 0 && movement.IsCurrentIdleNearEnd())
            {
                movement.PlayIdleVariant(_pendingIdleIndex);

                // 방금 선택한 Idle을 현재 Idle로 기록
                _currentIdleIndex = _pendingIdleIndex;
                _pendingIdleIndex = -1;

                // 다음 랜덤 전환을 위한 타이머 초기화
                ResetIdleTimer();
            }
        }

        public override void ExitState(PlayerMovement movement)
        {
            // Idle을 벗어나면 예약되어 있던 다음 Idle을 취소
            _pendingIdleIndex = -1;
        }

        // 현재 Idle과 다른 인덱스를 무작위로 선택
        private int GetDifferentIdleIndex(int currentIndex)
        {
            // 0부터 1까지 선택한 뒤 현재 인덱스를 건너뛰어
            // 결과적으로 0, 1, 2 중 현재와 다른 값을 반환
            int nextIndex = Random.Range(0, IdleAnimCount - 1);

            if (nextIndex >= currentIndex)
                nextIndex++;

            return nextIndex;
        }

        // 타이머를 초기화하고 다음 Idle까지 기다릴 시간을 설정
        private void ResetIdleTimer()
        {
            _idleTimer = 0f;
            _nextIdleTime = Random.Range(4f, 8f);
        }
    }
}