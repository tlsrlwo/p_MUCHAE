using UnityEngine;

namespace muchae
{
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerController : MonoBehaviour
    {
        private PlayerInputReader _inputReader;
        private PlayerMovement _playerMovement;

        private void Awake()
        {
            _inputReader = GetComponent<PlayerInputReader>();
            _playerMovement = GetComponent<PlayerMovement>();
        }
    }
}