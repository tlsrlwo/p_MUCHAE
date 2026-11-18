using UnityEngine;

namespace muchae
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance {get; private set;}

        private void Awake()
        {
            if(Instance == null)
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
        // 플레이어의 저장 관련 데이터들을 저장
        private void Start()
        {

        }
        private void Update()
        {

        }
    }
}