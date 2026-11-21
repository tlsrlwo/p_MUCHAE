using UnityEngine;

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

        [Header("애니메이션")]
        private Animator _anim;
        [SerializeField] private float _animDampTime = 0.1f;      // 값이 작으면 입력 변화에 빠르게 반응, 애니메이션 파라미터를 부드럽게 보간하기 위한 Damp 변수           

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
        }

        public void ChangeState(PlayerStateBase newState)
        {
            // 오류 방지
            if (newState == null || newState == _currentState)
            {
                return;
            }

            // 플레이어 상태 변환
            _currentState?.ExitState(this);            // _currentState 가 null 이 아닐 때 ExitState() 실행
            _currentState = newState;
            _currentState.EnterState(this);
        }

#region 애니메이션
        // 애니메이션 제어
        public void SetAnimationState(PlayerAnimationState state)
        {
            if(_anim == null) return;
            _anim.SetInteger("PlayerState", (int)state);
        }

        public void SetIdleVariant(int index)
        {
            if(_anim == null) return;
            _anim.SetFloat("IdleVariant", index);   
        }
        #endregion

        private void Move()
        {
            // 바닥체크 
            IsGrounded = CheckGrounded();

            // 인풋 값을 가져오되, 1을 넘기지 않도록 설정
            Vector2 input = Vector2.ClampMagnitude(MoveInput, 1f);

            // 인풋값의 방향을 가져옴
            Vector3 moveDirection = _playerCamera.FlatCameraRight * input.x + _playerCamera.FlatCameraForward * input.y;

            moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

            // 입력값이 있을 경우
            if (moveDirection.sqrMagnitude > 0.01f)
            {
                // 플레이어의 목표 회전값
                float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;

                // SmoothDampAngle을 통해 현재 각도를 목표 각도까지 부드럽게 따라가게 하는 한다
                float angle = Mathf.SmoothDampAngle(
                    transform.eulerAngles.y,
                    targetAngle,
                    ref _rotationVelocity,
                    _rotationSmoothTime
                );

                // 플레이어 캐릭터를 회전시켜줌 
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }

            // 애니메이션 파라미터 연결
            Vector3 localMoveDirection = transform.InverseTransformDirection(moveDirection);

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

        /*  수평 이동 계산기
                private Vector3 CalculateHorizontalVelocity()
                {
                    // 인풋 값 가져와서 최대값을 1로 설정
                    Vector2 input = Vector2.ClampMagnitude(_pInput.MoveInput, 1f);

                    // 현재 속도 지정하기
                    float speed = _pInput.RunInput ? _runSpeed : _walkSpeed;

                    // 수평 이동속도 지정하기 
                    CurrentSpeed = input.sqrMagnitude > 0f ? speed : 0f;

                    return new Vector3(input.x, 0, input.y) * speed;
                } 
        */

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