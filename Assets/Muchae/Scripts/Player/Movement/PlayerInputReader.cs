using UnityEngine;
using UnityEngine.InputSystem;

namespace muchae
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputReader : MonoBehaviour
    {
        // 이동 입력
        private Vector2 _moveInput;

        // 마우스 입력
        private Vector2 _lookInput;

        // 달리기 입력
        private bool _runInput;

        public Vector2 MoveInput => _moveInput;
        public Vector2 LookInput => _lookInput;
        public bool RunInput => _runInput;

        public void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>();
             Debug.Log($"Move 입력: {_moveInput}");
        }

        // public void OnLook(InputValue value) 추후에 추가할 것 

        public void OnRun(InputValue value)
        {
            _runInput = value.isPressed;
        }

        public void OnLook(InputValue value)
        {
            _lookInput = value.Get<Vector2>();
        }

        private void OnDisable()
        {
            // 비활성화될 때 이전 입력이 남지 않도록 초기화
            _moveInput = Vector2.zero;
            _lookInput = Vector2.zero;
            _runInput = false;
        }
    }        
}
