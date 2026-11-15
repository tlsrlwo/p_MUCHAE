using UnityEngine;

namespace muchae
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputReader))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("플레이어 움직임")]
        [SerializeField] private float _walkSpeed = 3f;
        [SerializeField] private float _runSpeed = 8f;

        [Header("중력 빛 바닥 검사")]
        [SerializeField] private float _groundCheckRadius = 0.2f;
        [SerializeField] private float _groundCheckYOffset = 0.1f;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _gravity = -9.81f;


        [Header("참조")]
        private CharacterController _cController;
        private PlayerInputReader _pInput;

        public float CurrentSpeed { get; set; }
        public bool IsGrounded { get; private set; }


        // 
        public Vector2 MoveInput => _pInput.MoveInput;
        public bool RunInput => _pInput.RunInput;
        public float WalkSpeed => _walkSpeed;
        public float RunSpeed => _runSpeed;


        private float _verticalVelocity;

        // 바닥 체크
        private Vector3 GroundCheckPosition =>
                        transform.position + Vector3.down * _groundCheckYOffset;



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



        private void Move()
        {
            // 바닥체크 
            IsGrounded = CheckGrounded();

            Vector2 input = Vector2.ClampMagnitude(MoveInput, 1f);

            // 수평 이동 계산
            Vector3 horizontalVelocity = new Vector3(input.x, 0f, input.y) * CurrentSpeed;

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
        } */

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