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
        // 여기서 구한 _cameraTarget의 회전값을 기반으로 PlayerMovement 에서 카메라 시점 방향으로 이동이 가능해짐
        public Vector3 FlatCameraForward
        {
            get
            {
                // 플레이어의 현재 forward방향을 가져옴
                Vector3 forward = _cameraTarget.forward;
                // 위아래 기울기값을 제거
                forward.y = 0f;

                return forward.sqrMagnitude > 0.001f ? forward.normalized : transform.forward;
            }
        }


        // FlatCameraForward 와 FlatCameraRight 는 카메라의 각도에 상관없이 현재 플레이어의 정면과 우측이 어디인지 검사하는 함수
        // Vector3.Cross 는 두 방향 벡터를 이용해서 새로운 수진 방향 벡터를 구하는 함수 				
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
            Vector3 initialRotations = _cameraTarget.eulerAngles; // eulerAngles는 현재 Transform의 회전값을 오일러 각도로 반환
            _yaw = initialRotations.y;                            // 좌우 회전각도
            _pitch = initialRotations.x;                          // 상하 회전각도

            // _pitch의 각도를 조절하기 위한 코드
            if (_pitch > 180f)
                _pitch -= 360f;
        }

        private void LateUpdate()
        {
            // 마우스의 인풋값
            Vector2 lookInput = _pInput.LookInput;

            // 카메라 회전
            _yaw += lookInput.x * _sensitivity;                   // 카메라 좌우 회전
            _pitch -= lookInput.y * _sensitivity;                 // 카메라 상하 회전
            // 상하 최대값 설정
            _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

            // 계산한 오일러 각도를 Quaternion으로 변환해줌
            // Unity의 Transform은 내부적으로 회전을 Quaternion으로 관리하기 때문
            // 여기서 구한 _cameraTarget.rotation 이 PlayerMovement에서 사용된다 -> 플레이어가 카메라의 시점 방향으로 걸어가게끔
            _cameraTarget.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

    }
}
