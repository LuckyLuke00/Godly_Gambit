using UnityEngine;

namespace GodlyGambit
{
    public class PlayerSwapper : Singleton<PlayerSwapper>
    {
        [SerializeField] private bool _godLosesLiveOnSwap = true;
        [SerializeField] private bool _purgeLevelOnSwap = true;
        [SerializeField] private bool _resetTimerOnSwap = true;
        [SerializeField] private AudioSource _swapSound;

        private bool _shouldGodLoseLife = false;

        // Event that is triggered when a player is swapped
        public delegate void PlayerSwapped();

        public static event PlayerSwapped OnPlayerSwapped;

        private void OnEnable()
        {
            HealthComponent.OnFighterLostLive += Swap;
            TimerComponent.OnTimeIsUp += HandleTimeUp;
        }

        private void OnDisable()
        {
            HealthComponent.OnFighterLostLive -= Swap;
            TimerComponent.OnTimeIsUp -= HandleTimeUp;
        }

        private void HandleTimeUp()
        {
            _shouldGodLoseLife = _godLosesLiveOnSwap;

            RandomSwap();

            _shouldGodLoseLife = false;
        }

        private void RandomSwap()
        {
            var fighters = PlayerManager.Instance.ActiveFighters;
            int randomIndex = Random.Range(0, fighters.Count);
            Swap(fighters[randomIndex], fighters[randomIndex].GetComponent<HealthComponent>());
        }

        private void Swap(GameObject fighter, HealthComponent healthComponent)
        {
            PlayerManager playerManager = PlayerManager.Instance;

            if (playerManager.GodRecentlySpawned)
            {
                // Just respawn, don't swap
                playerManager.RespawnFighter(fighter);
                return;
            }

            if (!fighter.TryGetComponent(out FighterController fighterController))
            {
                Debug.LogError("PlayerSwapper: Fighter lost live but no FighterController found");
                return;
            }

            GameObject god = playerManager.God;
            if (!god.TryGetComponent(out GodController godController))
            {
                Debug.LogError("PlayerSwapper: Fighter lost live but no GodController found");
                return;
            }

            if (_resetTimerOnSwap)
            {
                TimerComponent timerComponent = FindObjectOfType<TimerComponent>();
                if (timerComponent != null)
                {
                    timerComponent.ResetTimer();
                }
            }

            if (_swapSound)
            {
                _swapSound.Play();
            }

            if (_purgeLevelOnSwap) playerManager.PurgeLevel();

            fighterController.Kill();
            godController.Kill();

            playerManager.SpawnPlayer(GetBase(ref god), playerManager.GodIndex, out GameObject newPlayer);
            HealthComponent newPlayerHealth = newPlayer.GetComponent<HealthComponent>();
            if (god.TryGetComponent(out HealthComponent godHealth))
            {
                newPlayerHealth.CurrentLives = godHealth.CurrentLives;
            }

            // Needs to be spawned last, beause the GodIndex is changed
            // And we need the previous GodIndex, to set the correct material
            playerManager.SpawnGod(GetBase(ref fighter), healthComponent);

            if (_shouldGodLoseLife)
            {
                --newPlayerHealth.CurrentLives;
                if (newPlayerHealth.CurrentLives < 0)
                {
                    newPlayerHealth.SimpleKill();
                    //PlayerManager.Instance.AddDeadFighter(gameObject);
                    newPlayerHealth.SimpleKillEvents();
                }
            }

            OnPlayerSwapped?.Invoke();
        }

        private GameObject GetBase(ref GameObject child) => child.transform.parent.gameObject;

        private Material GetMaterial(ref GameObject gameObject) => gameObject.GetComponentInChildren<MeshRenderer>().material;
    }
}
