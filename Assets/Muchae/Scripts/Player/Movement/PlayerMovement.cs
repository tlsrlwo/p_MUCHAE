using UnityEngine;
using UnityEngine.Animations;

namespace muchae
{
    public enum PlayerAnimationState
    {
        Idle = 0,
        Walk = 1,
        Run = 2
    }
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputReader))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("플레이어 움직임")]
        [SerializeField] private float _walkSpeed = 3f;
        [SerializeField] private float _runSpeed = 8f;
        [SerializeField] private float _rotationSmoothTime = 0.12f;
        private float _rotationVelocity;

        [Header("중력 빛 바닥 검사")]
        [SerializeField] private float _groundCheckRadius = 0.2f;
        [SerializeField] private float _groundCheckYOffset = 0.1f;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _gravity = -9.81f;


        [Header("참조")]
        private CharacterController _cController;
        private PlayerInputReader _pInput;
        private PlayerCameraLook _playerCamera;
        private Animator _anim;

        [Header("애니메이션")]
        [SerializeField] private float _animDampTime = 0.1f;      // 값이 작으면 입력 변화에 빠르게 반응, 애니메이션 파라미터를 부드럽게 보간하기 위한 Damp 변수           
        [SerializeField] private float _idleTimer;                // idle 애니메이션 변환 타이머
        [SerializeField] private float _idleCrossFadeDuration = 0.25f; 

        public float CurrentSpeed { get; set; }
        public bool IsGrounded { get; private set; }


        // FSM 에서 사용할 수 있는 읽기전용 프로퍼티 선언
        public Vector2 MoveInput => _pInput.MoveInput;
        public bool RunInput => _pInput.RunInput;
        public float WalkSpeed => _walkSpeed;
        public float RunSpeed => _runSpeed;

        private float _verticalVelocity;

        // 바닥 체크
        private Vector3 GroundCheckPosition => transform.position + Vector3.down * _groundCheckYOffset;

        #region 플레이어 FSM
        private PlayerStateBase _currentState;
        

        public PlayerIdleState IdleState = new PlayerIdleState();
        public PlayerWalkState WalkState = new PlayerWalkState();
        public PlayerRunState RunState = new PlayerRunState();

        //  Animator에 등록된 Idle 상태들의 해시값(Hash)을 배열에 저장
        private static readonly int[] IdleStateHashes =
        {
            Animator.StringToHash("Base Layer.Locomotion.IdleVariants.Idle1"),
            Animator.StringToHash("Base Layer.Locomotion.IdleVariants.Idle2"),
            Animator.StringToHash("Base Layer.Locomotion.IdleVariants.Idle3")
        };

#endregion


        private void Awake()
        {
            _cController = GetComponent<CharacterController>();
            _pInput = GetComponent<PlayerInputReader>();
            _playerCamera = GetComponent<PlayerCameraLook>();
            _anim = GetComponentInChildren<Animator>();
            if(_anim == null)
            {
                Debug.LogWarning("[PlayerMovement] Animator 컴포넌트를 참조할 수 없습니다.", this);
                enabled = false;
                return;
            }
        }

        private void Start()
        {
            ChangeState(IdleState);
        }

        private void Update()
        {
            _currentState?.UpdateState(this);
            Move();

            _idleTimer = IdleState.IdleTimer;
        }

        public void ChangeState(PlayerStateBase newState)
        {
            // 오류 방지
            if (newState == null || newState == _currentState)
            {
                return;
            }

            // 플레이어 상태 변환
            _currentState?.ExitState(this);                  // _currentState 가 null 이 아닐 때 ExitState() 실행
            _currentState = newState;
            _currentState.EnterState(this);
        }

#region 애니메이션
       
        public void SetAnimationState(PlayerAnimationState state)
        {
            if(_anim == null) return;
            _anim.SetInteger("PlayerState", (int)state);     // 애니메이션 제어
        }

        public void SetIdleVariant(int index)
        {
            if (_anim == null) return;
            _anim.SetFloat("IdleVariant", index);            // IdleVariant 는 animator의 파라미터
        }

        // Animator에서 Idle 애니메이션을 자연스럽게 전환시키는 로직이 포함된 함수
        public void PlayIdleVariant(int index)
        {
            if (_anim == null) return;

            // index값이 들어올 때 최대 IdleStatehases.Length -1 을 넘지 못하도록 제한 (IdleStateHashes는 3개이므로 0,1,2 가 되게끔)
            index = Mathf.Clamp(index, 0, IdleStateHashes.Length - 1);

            // 지정된 Animator 상태로 초 단위 크로스페이드. idle 애니메이션 3 개를 돌아가면서 바뀌게 함
            _anim.CrossFadeInFixedTime(IdleStateHashes[index], _idleCrossFadeDuration, 0);
        }

        // 현재 재생 중인 Idle 애니메이션이 루프 끝에 가까워졌는지 확인
        public bool IsCurrentIdleNearEnd(float normalizedThreshold = 0.9f)
        {
            // Animator가 없거나 다른 애니메이션으로 전환 중이면 반환
            if (_anim == null || _anim.IsInTransition(0))
                return false;

            // 현재 재생 중인 상태 정보 가져오기
            AnimatorStateInfo stateInfo = _anim.GetCurrentAnimatorStateInfo(0);

            // 현재 상태가 Idle1, Idle2, Idle3 중 하나인지 확인
            bool isIdleState = false;

            // 현재 재생중인 애니메이션의 fullPathHash 가 idleStateHash 안에 있는 거면 현재 idle=true 인 상태. 루프 종료
            foreach (int idleStateHash in IdleStateHashes)
            {
                if (stateInfo.fullPathHash == idleStateHash)
                {
                    isIdleState = true;
                    break;
                }
            }

            // Idle 애니메이션이 아니면 전환 시점이 아니므로 false
            if (!isIdleState)
                return false;

            // normalizedTime은 애니메이션 진행도를 나타냄
            // 1로 나눠서 남은값, 즉 소수 부분만 사용해 현재 루프에서의 진행도를 계산 (1.3 % 1f 는 0.3 을 의미)
            float loopProgress = stateInfo.normalizedTime % 1f;

            // 진행도가 기준값(기본 0.9, 즉 루프의 90%) 이상이면 true
            return loopProgress >= normalizedThreshold;
        }
        #endregion

        private void Move()
        {
            // 바닥체크 
            IsGrounded = CheckGrounded();

            // 인풋 값을 가져오되, 1을 넘기지 않도록 설정
            Vector2 input = Vector2.ClampMagnitude(MoveInput, 1f);

            // 인풋값의 방향을 가져옴 PlayerCamearLook의 FlatCameraRight, FlatCameraForward 사용
            Vector3 moveDirection = _playerCamera.FlatCameraRight * input.x + _playerCamera.FlatCameraForward * input.y;

            // 각 Vector 범위의 최대값을 1로 제한
            moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

            // 입력값이 있을 경우
            if (moveDirection.sqrMagnitude > 0.01f)
            {
                // 플레이어의 목표 회전값을 도 단위로 변환
                float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;

                // SmoothDampAngle을 통해 현재 각도를 목표 각도까지 부드럽게 따라가게 하는 한다
                float angle = Mathf.SmoothDampAngle(
                    transform.eulerAngles.y,
                    targetAngle,
                    ref _rotationVelocity,
                    _rotationSmoothTime
                );

                // 플레이어 캐릭터를 회전시켜줌
                // SmoothDampAngle을 통해 현재 각도에서 목표 각도까지 매 프레임 부드럽게 보간하여 회전시킴
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }

            // 애니메이션 파라미터 연결
            // 카메라 기준으로 계산된 이동 방향을 플레이어 자신의 로컬 좌표계 기준으로 변환
            Vector3 localMoveDirection = transform.InverseTransformDirection(moveDirection);

            // Animator의 xInput, zInput 파라미터에 전달해서 캐릭터가 이동하는 방향에 맞는 애니메이션을 재생하도록
            if (_anim != null)
            {
                _anim.SetFloat("xInput", localMoveDirection.x, _animDampTime, Time.deltaTime);
                _anim.SetFloat("zInput", localMoveDirection.z, _animDampTime, Time.deltaTime);
            }


            // 수평 이동 계산
            Vector3 horizontalVelocity = moveDirection * CurrentSpeed;

            UpdateVerticalVelocity();

            // 이동 수치 계산
            Vector3 velocity = horizontalVelocity + Vector3.up * _verticalVelocity;

            _cController.Move(velocity * Time.deltaTime);
        }

        private void UpdateVerticalVelocity()
        {
            // 바닥에 있고 수직 이동이 없을 때
            if (IsGrounded && _verticalVelocity < 0f)
            {
                // 바닥에서 뜨는 것을 방지
                _verticalVelocity = -2f;
            }
            else
            {
                // 수직 이동 속도 적용
                _verticalVelocity += _gravity * Time.deltaTime;
            }
        }

        private bool CheckGrounded()
        {
            return Physics.CheckSphere(
                GroundCheckPosition,
                _groundCheckRadius,
                _groundLayer,
                QueryTriggerInteraction.Ignore);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GroundCheckPosition, _groundCheckRadius);
        }
    }
}