using UnityEngine;

namespace muchae
{
    public class GameManager : MonoBehaviour
    {
        // 게임 전체의 상태/시스템을 관리하는 최상위 관리자
        // Playing, Paused, GameOver 등
        public static GameManager Instance {get; private set;}

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
                return;
            }
        }

        private void Start()
        {
            SetCursorLocked(true);
        }
        
        public void SetCursorLocked(bool isLocked)
        {
            Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;

            Cursor.visible = !isLocked;
        }
    }
}