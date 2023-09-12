using UnityEngine;
using UnityEngine.UI;

namespace GodlyGambit
{
    public class PlayerHUD : MonoBehaviour
    {
        [Tooltip("Decides in what order the HUD gets assigned to a player")]
        [SerializeField, Range(0, 3)] private int _priority = 0;

        [SerializeField] private Image _playerIcon = null;

        [Header("Fire")]
        [SerializeField] private GameObject _fireParent = null;

        [SerializeField] private GameObject[] _fireVariants = null;

        [Header("Health and Lives")]
        [SerializeField] private GameObject _HealthBar = null;

        [SerializeField] private Sprite _livesGray = null;
        [SerializeField] private Image[] _livesFull = null;

        [Header("Player Icons: Same order as in PlayerManager")]
        [SerializeField] private Sprite[] _fighterVariantIcons = null;

        [SerializeField] private Sprite[] _godVariantIcons = null;

        private FighterController _fighterController = null;
        private int _playerVariantIndex = 0;

        private void Awake()
        {
            // Disable all children of the HUD
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            PlayerManager.OnNewPlayerJoined += AssignHUD;
            PlayerSwapper.OnPlayerSwapped += AssignHUD;

            HealthComponent.OnHealthChanged += UpdateHealthBar;
            HealthComponent.OnLivesChanged += UpdateLives;
        }

        private void OnDisable()
        {
            PlayerManager.OnNewPlayerJoined -= AssignHUD;
            PlayerSwapper.OnPlayerSwapped -= AssignHUD;

            HealthComponent.OnHealthChanged -= UpdateHealthBar;
            HealthComponent.OnLivesChanged -= UpdateLives;
        }

        private void AssignHUD()
        {
            int playerCount = PlayerManager.Instance.PlayerCount;

            if (_priority > playerCount - 1) return;

            if (playerCount < PlayerManager.Instance.MinPlayers) return;

            _playerVariantIndex = PlayerManager.Instance.MaterialVariantIndices[_priority];

            AssignFire();

            // Assign the HUD to the player
            if (PlayerManager.Instance.IsGod(_priority))
            {
                SetupGodHUD();
            }
            else
            {
                SetupFighterHUD();
            }

            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
        }

        private void SetupGodHUD()
        {
            // Replace the player icon with the god icon
            _playerIcon.sprite = _godVariantIcons[_playerVariantIndex];

            // Disable the health bar
            _HealthBar.SetActive(false);

            // Enable the fire parent
            _fireParent.SetActive(true);
        }

        private void SetupFighterHUD()
        {
            // Replace the player icon with the fighter icon
            _playerIcon.sprite = _fighterVariantIcons[_playerVariantIndex];

            // Get the fighter controller
            GameObject player = PlayerManager.Instance.GetPlayer(_priority);
            if (!player) return;

            _fighterController = player.GetComponentInChildren<FighterController>();
            if (!_fighterController) return;
            UpdateLives(_fighterController.gameObject, player.GetComponentInChildren<HealthComponent>());

            // Enable the health bar
            _HealthBar.SetActive(true);
        }

        private void UpdateLives(GameObject sender, HealthComponent healthComponent)
        {
            if (!IsSenderAssignedPlayer(sender)) return;

            int lives = healthComponent.VisualLives;

            // Replace the full lives with empty lives of the current player
            for (int i = 0; i < _livesFull.Length; ++i)
            {
                if (i >= lives)
                {
                    _livesFull[i].sprite = _livesGray;
                }
            }
        }

        private void UpdateHealthBar(GameObject sender, HealthComponent healthComponent)
        {
            if (!IsSenderAssignedPlayer(sender)) return;

            // Update the health bar
            _HealthBar.GetComponent<Image>().fillAmount = healthComponent.HealthPercentage;
        }

        private void AssignFire()
        {
            // Disable the fire parent
            _fireParent.SetActive(false);

            // Destroy all children of the fire parent
            foreach (Transform child in _fireParent.transform)
            {
                Destroy(child.gameObject);
            }

            // Instantiate the fire variant
            Instantiate(_fireVariants[_playerVariantIndex], _fireParent.transform);
        }

        private bool IsSenderAssignedPlayer(GameObject sender)
        {
            // Check if the sender is a fighter
            if (!sender.CompareTag("Fighter")) return false;

            // Get the index of the sender
            int index = PlayerManager.Instance.GetPlayerIndex(sender);

            // Check if the sender is assigned to this HUD
            if (index < 0 || index != _priority) return false;

            return true;
        }
    }
}
