using UnityEngine;

namespace muchae
{
    public class PlayerCameraLook : MonoBehaviour
    {
        [SerializeField] private Transform _cameraTarget;
        private float _sensitivity = 0.1f;
        private float _minPitch = -30f;
        private float _maxPitch = 70f;

        private PlayerInputReader _pInput;
        private float _yaw;
        private float _pitch;

        // 카메라가 보는 앞 방향에서 위아래 기울기를 빼고, 바닥에 투영한 수평 앞 방향을 구함
        public Vector3 FlatCameraForward
        {
            get
            {
                // 플레이어의 현재 forward방향을 가져옴
                Vector3 forward = _cameraTarget.forward;
                // 위아래 방향을 제거
                forward.y = 0f;

                return forward.sqrMagnitude > 0.001f ? forward.normalized : transform.forward;
            }
        }

        public Vector3 FlatCameraRight => Vector3.Cross(Vector3.up, FlatCameraForward).normalized;

        private void Awake()
        {
            // 플레이어 인풋 가져오기
            _pInput = GetComponent<PlayerInputReader>();

            // 카메라의 타겟으로 플레이어 설정해주기 
            if (_cameraTarget == null)
            {
                _cameraTarget = transform.Find("CameraTarget");
            }

            if (_cameraTarget == null)
            {
                Debug.LogWarning("[PlayerCameraLook] cameraTarget 을 찾을 수 없음", this);
                enabled = false;
                return;
            }

            // 현재 플레이어의 회전값으로 초기화해줌
            Vector3 initialRotations = _cameraTarget.eulerAngles;
            _yaw = initialRotations.y;
            _pitch = initialRotations.x;

            // 
            if (_pitch > 180f)
                _pitch -= 360f;
        }

        private void LateUpdate()
        {
            Vector2 lookInput = _pInput.LookInput;

            // 카메라 회전
            _yaw += lookInput.x * _sensitivity;
            _pitch -= lookInput.y * _sensitivity;
            // 상하 최대값 설정
            _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

            _cameraTarget.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

    }
}
