using UnityEngine;
using UnityEngine.SceneManagement;

namespace muchae
{
    public class MuchaeSceneManager : MonoBehaviour
    {
        // 어떤 씬을 로드할 지, 씬 로드가 완료되었는지 (Async 사용), 씬 전환 연출을 담당

        public static MuchaeSceneManager Instance {get; private set;}

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

        // 씬을 로드함
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}