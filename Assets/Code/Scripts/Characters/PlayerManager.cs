using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace GodlyGambit
{
    // TODO: Refactor and separate out the functionality of this class
    [RequireComponent(typeof(PlayerInputManager))]
    [RequireComponent(typeof(PlayerSpawnPointManager))]
    public class PlayerManager : Singleton<PlayerManager>
    {
        #region Serialized Fields

        [SerializeField] private GameObject _godPrefab = null;
        [SerializeField] private GameObject _fighterPrefab = null;
        [SerializeField] private float _godSpawnDelay = 2f;
        [SerializeField] private float _spawnEffectDuration = 4f;

        // Variable that holds all scenes
        [SerializeField] private string[] _gameScenes = null;

        [Header("Debug")]
        [SerializeField] private bool _spawnGodFirst = false;

        #endregion Serialized Fields

        #region Variants

        [Header("Player Variants: Order matters!")]
        [SerializeField, ColorUsage(true, true)] private Color[] _godFireColors = null;

        [SerializeField] private Color[] _godLightColors = null;
        [SerializeField] private Material[] _godVariants = null;
        [SerializeField] private GameObject[] _godSpawnEffectVariants = null;

        [SerializeField] private Material[] _circleVariants = null;
        [SerializeField] private Material[] _crossbowVariants = null;
        [SerializeField] private Material[] _fighterVariants = null;

        #endregion Variants

        #region Private Fields

        private bool _inGameScene = false;
        private bool _playersSetUp = false;
        private bool _restart = false;
        private GameObject _god = null;

        private int _godIndex = 0;
        private int _minPlayers = 2;
        private int _playerCount = 0;

        private PlayerSpawnPointManager _playerSpawnPointManager = null;
        private PlayerInputManager _playerInputManager = null;
        private readonly List<FirebowlColorSwitcher> _firebowls = new List<FirebowlColorSwitcher>();

        private readonly List<GameObject> _fighters = new List<GameObject>();
        private readonly List<GameObject> _aliveFighters = new List<GameObject>(); // Hacky workaround to make the Enemies not get a component every frame
        private readonly List<GameObject> _players = new List<GameObject>();
        private readonly List<int> _UsedPlayerVariantIndices = new List<int>();

        #endregion Private Fields

        #region Events

        public delegate void GameStarted();

        public static event GameStarted OnGameStarted;

        public delegate void PlayerJoined();

        public static event PlayerJoined OnNewPlayerJoined;

        public delegate void PlayerJoinEvent(int playerIndex);

        public static event PlayerJoinEvent OnPlayerJoin;

        public delegate void GameEndEvent();

        public static event GameEndEvent OnGameEnd;

        #endregion Events

        public GameObject God => _god;
        public List<GameObject> ActiveFighters => _fighters;

        public List<GameObject> AliveFighters => _aliveFighters;

        public bool CanGameStart { get; set; } = false;
        public int GodIndex => _godIndex;

        public int PlayerCount => _playerCount;

        public int MinPlayers => _minPlayers;

        public int[] MaterialVariantIndices => _UsedPlayerVariantIndices.ToArray();

        public bool GodRecentlySpawned { get; private set; } = false;

        public int GetPlayerIndex(GameObject player) => _players.IndexOf(player.transform.parent.gameObject);

        public void AddDeadFighter(GameObject deadFighter)
        {
            if (_aliveFighters.Contains(deadFighter)) _aliveFighters.Remove(deadFighter);
        }

        public GameObject GetPlayer(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex > _players.Count - 1) return null;
            return _players[playerIndex];
        }

        private void Awake()
        {
            _playerInputManager = GetComponent<PlayerInputManager>();

            if (_godPrefab == null)
            {
                Debug.LogError("<color=#ff6b6b>PlayerManager:</color> No god prefab assigned!");
            }

            if (_fighterVariants.Length == 0)
            {
                Debug.LogError("<color=#ff6b6b>PlayerManager:</color> No player variants assigned!");
            }

            // If the amount of player prefabs is less than the maximum amount of players, log a warning
            if (_fighterVariants.Length < _playerInputManager.maxPlayerCount - 1)
            {
                Debug.LogWarning("<color=#ff6b6b>PlayerManager:</color> The amount of player variants is less than the maximum amount of players!");
            }

            // Clamp the minimum number of players to 2
            _minPlayers = Mathf.Clamp(_minPlayers, 2, _playerInputManager.maxPlayerCount);

            _playerSpawnPointManager = GetComponent<PlayerSpawnPointManager>();

            _playerInputManager.EnableJoining();
        }

        private void OnEnable()
        {
            FighterController.OnFighterDied += OnFighterDied;
            HealthComponent.OnFighterDied += CheckGameEnd;

            _playerInputManager.onPlayerJoined += OnPlayerJoined;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            FighterController.OnFighterDied -= OnFighterDied;
            HealthComponent.OnFighterDied -= CheckGameEnd;

            _playerInputManager.onPlayerJoined -= OnPlayerJoined;

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _inGameScene = false;

            // If the scene is not a game scene, return
            // Check if the scene is in the list of game scenes
            foreach (string gameScene in _gameScenes)
            {
                if (scene.name != gameScene) continue;

                Initialize();

                return;
            }
        }

        private void Initialize()
        {
            _inGameScene = true;
            _playerSpawnPointManager.FindSpawnPoints();

            // Find all firebowls in the scene
            var firebowls = FindObjectsOfType<FirebowlColorSwitcher>();
            foreach (var firebowl in firebowls)
            {
                _firebowls.Add(firebowl);
            }

            if (_firebowls.Count < 1)
            {
                Debug.LogWarning("<color=#ff6b6b>PlayerManager:</color> No firebowls found in the scene!");
            }

            if (_playerCount < 2) return;
            SetupPlayers();
        }

        public void OnPlayerJoined(PlayerInput playerInput)
        {
            // Get a reference to the newly joined player's gameobject
            // If there is no player input gameobject create a new one
            _players.Add(playerInput.gameObject);
            ++_playerCount;

            CreateUnusedPlayerVariantIndex();

            SetupPlayers();

            OnPlayerJoin?.Invoke(_playerCount - 1);
        }

        public void UnjoinAllPlayers()
        {
            // Destroy every player
            foreach (var player in _players)
            {
                Destroy(player);
            }

            _aliveFighters.Clear();
            _fighters.Clear();
            _god = null;
            _godIndex = 0;
            _inGameScene = false;
            _playerCount = 0;
            _players.Clear();
            _playersSetUp = false;
            _restart = false;
            _UsedPlayerVariantIndices.Clear();
            CanGameStart = false;
        }

        private void SetupPlayers()
        {
            if (!_inGameScene) return;

            if (_restart || (CanGameStart && !_playersSetUp) || ((!CanGameStart && !_playersSetUp) && _playerCount == _minPlayers))
            {
                _godIndex = _spawnGodFirst ? 0 : Random.Range(0, _playerCount);

                // Loop through all the players and set them up
                for (int i = 0; i < _playerCount; ++i)
                {
                    if (i == _godIndex)
                    {
                        SetupGod(i);
                        continue;
                    }

                    SetupFighter(i);
                }

                //TimerComponent timerComponent = FindObjectOfType<TimerComponent>();
                //if (timerComponent != null)
                //{
                //    timerComponent.ResetTimer();
                //}

                CanGameStart = false;
                _playersSetUp = true;
                _restart = false;

                OnGameStarted?.Invoke();
                OnNewPlayerJoined?.Invoke();

                return;
            }

            if (_playersSetUp)
            {
                SetupFighter(_playerCount - 1);
                OnNewPlayerJoined?.Invoke();
            }
        }

        private void SetupGod(int playerIndex)
        {
            Transform spawnPoint = _playerSpawnPointManager.GetGodSpawnPoint();
            // Needs to be instantiated as a child of the PlayerBase, otherwise some getters the PlayerInput component will fail
            _god = Instantiate(_godPrefab, spawnPoint.position, spawnPoint.rotation, _players[playerIndex].transform);
            var playerInput = _players[playerIndex].GetComponent<PlayerInput>();
            playerInput.enabled = true;
            playerInput.SwitchCurrentActionMap("God");

            SkinnedMeshRenderer renderer = _players[playerIndex].GetComponentInChildren<SkinnedMeshRenderer>();

            if (renderer)
            {
                renderer.material = _godVariants[_UsedPlayerVariantIndices[playerIndex]];
            }

            // Set the _firebowl to the god's color
            foreach (var firebowl in _firebowls)
            {
                firebowl.SwitchColors(_godLightColors[_UsedPlayerVariantIndices[playerIndex]], _godFireColors[_UsedPlayerVariantIndices[playerIndex]]);
            }
        }

        private void SetupFighter(int playerIndex)
        {
            Transform spawnPoint = _playerSpawnPointManager.GetRandomPlayerSpawnPoint();

            // Needs to be instantiated as a child of the PlayerBase, otherwise some getting the PlayerInput component will fail
            var fighter = Instantiate(_fighterPrefab, spawnPoint.position, spawnPoint.rotation, _players[playerIndex].transform);
            _fighters.Add(fighter);
            _aliveFighters.Add(fighter);

            SkinnedMeshRenderer renderer = _players[playerIndex].GetComponentInChildren<SkinnedMeshRenderer>();

            int variantIndex = _UsedPlayerVariantIndices[playerIndex];

            // Set the player's crossbow variant
            if (renderer)
            {
                renderer.material = _fighterVariants[variantIndex];
            }

            if (fighter.TryGetComponent(out FighterController fighterController))
            {
                fighterController.SetCircleColor(_circleVariants[variantIndex]);
            }

            if (fighter.TryGetComponent(out ShootingBehaviour crossbow))
            {
                crossbow.SetWeaponMaterial(_crossbowVariants[variantIndex]);
            }

            var playerInput = _players[playerIndex].GetComponent<PlayerInput>();
            playerInput.enabled = true;
            playerInput.SwitchCurrentActionMap("Fighter");
        }

        public void SpawnGod(GameObject parent, HealthComponent healthComponent)
        {
            GodRecentlySpawned = true;
            Transform spawnPoint = _playerSpawnPointManager.GetGodSpawnPoint();

            // Needs to be instantiated as a child of the PlayerBase, otherwise some getting the PlayerInput component will fail
            _godIndex = _players.IndexOf(parent);
            Destroy(Instantiate(_godSpawnEffectVariants[_UsedPlayerVariantIndices[_godIndex]], spawnPoint.position, spawnPoint.rotation), _spawnEffectDuration);

            foreach (var firebowl in _firebowls)
            {
                firebowl.SwitchColors(_godLightColors[_UsedPlayerVariantIndices[_godIndex]], _godFireColors[_UsedPlayerVariantIndices[_godIndex]]);
            }

            TimerComponent timerComponent = FindObjectOfType<TimerComponent>();
            if (timerComponent != null)
            {
                timerComponent.StopTimer();
            }

            StartCoroutine(SpawnGodCoroutine(parent, spawnPoint, healthComponent));
        }

        private IEnumerator SpawnGodCoroutine(GameObject parent, Transform spawnPoint, HealthComponent healthComponent)
        {
            // Delay for a couple of seconds
            yield return new WaitForSeconds(_godSpawnDelay);

            if (!spawnPoint)
            {
                GodRecentlySpawned = false;
                yield break;
            }

            _god = Instantiate(_godPrefab, spawnPoint.position, spawnPoint.rotation, parent.transform);
            _god.AddComponent<HealthComponent>().CopyHealthValues(healthComponent);

            SkinnedMeshRenderer renderer = _god.GetComponentInChildren<SkinnedMeshRenderer>();
            if (renderer)
            {
                renderer.material = _fighterVariants[_UsedPlayerVariantIndices[_godIndex]];
            }

            var playerInput = parent.GetComponent<PlayerInput>();
            playerInput.enabled = true;
            playerInput.SwitchCurrentActionMap("God");

            TimerComponent timerComponent = FindObjectOfType<TimerComponent>();
            if (timerComponent != null)
            {
                timerComponent.StartTimer();
            }

            GodRecentlySpawned = false;
        }

        public void SpawnPlayer(GameObject parent, int playerIndex, out GameObject newPlayer)
        {
            Transform spawnPoint = _playerSpawnPointManager.GetRandomPlayerSpawnPoint();
            // Needs to be instantiated as a child of the PlayerBase, otherwise some getting the PlayerInput component will fail
            newPlayer = Instantiate(_fighterPrefab, spawnPoint.position, spawnPoint.rotation, parent.transform);
            _fighters.Add(newPlayer);
            _aliveFighters.Add(newPlayer);

            SkinnedMeshRenderer renderer = newPlayer.GetComponentInChildren<SkinnedMeshRenderer>();
            if (renderer)
            {
                renderer.material = _fighterVariants[_UsedPlayerVariantIndices[playerIndex]];
            }

            if (newPlayer.TryGetComponent(out FighterController fighterController))
            {
                fighterController.SetCircleColor(_circleVariants[_UsedPlayerVariantIndices[playerIndex]]);
            }

            if (newPlayer.TryGetComponent(out ShootingBehaviour crossbow))
            {
                crossbow.SetWeaponMaterial(_crossbowVariants[_UsedPlayerVariantIndices[playerIndex]]);
            }

            var playerInput = parent.GetComponent<PlayerInput>();
            playerInput.enabled = true;
            playerInput.SwitchCurrentActionMap("Fighter");
        }

        public void RespawnFighter(GameObject fighter)
        {
            Transform spawnPoint = _playerSpawnPointManager.GetRandomPlayerSpawnPoint();
            fighter.transform.position = spawnPoint.position;
            fighter.transform.rotation = spawnPoint.rotation;
            _fighters.Add(fighter);
            _aliveFighters.Add(fighter);

            if (fighter.TryGetComponent(out FighterController fighterController))
            {
                fighterController.ResetFighter();
            }
        }

        private void CreateUnusedPlayerVariantIndex()
        {
            int randomIndex = Random.Range(0, _fighterVariants.Length);

            while (_UsedPlayerVariantIndices.Contains(randomIndex))
            {
                randomIndex = Random.Range(0, _fighterVariants.Length);
            }
            _UsedPlayerVariantIndices.Add(randomIndex);
        }

        private void OnFighterDied(GameObject fighter)
        {
            _fighters.Remove(fighter);
            _aliveFighters.Remove(fighter);
            _playerSpawnPointManager.ResetUsedSpawnPoints();
        }

        public bool IsGod(int playerIndex)
        {
            return playerIndex == _godIndex;
        }

        public void RumbleAll(float lowFreq, float highFreq, float duration)
        {
            for (int i = 0; i < _players.Count; i++)
            {
                // Get the player's gamepad
                var devices = _players[i].GetComponentInChildren<PlayerInput>().devices;
                Gamepad gamepad = devices.Count > 0 ? devices[0] as Gamepad : null;
                if (gamepad == null) continue;

                // Rumble the gamepad
                gamepad.SetMotorSpeeds(lowFreq, highFreq);
                StartCoroutine(StopRumble(duration, gamepad));
            }
        }

        private IEnumerator StopRumble(float duration, Gamepad gamepad)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            gamepad.SetMotorSpeeds(.0f, .0f);
        }

        private void CheckGameEnd()
        {
            OnGameEnd?.Invoke();

            PurgeLevel();

            List<GameObject> fighterList = _fighters;

            for (int i = fighterList.Count - 1; i >= 0; i--)
            {
                if (!fighterList[i])
                {
                    fighterList.RemoveAt(i);
                    continue;
                }

                if (fighterList[i].TryGetComponent(out HealthComponent healthComponent) && healthComponent.IsDead)
                {
                    fighterList.RemoveAt(i);
                }
            }

            if (fighterList.Count < 1)
            {
                for (int i = 0; i < _players.Count; i++)
                {
                    if (!_players[i].TryGetComponent(out TimerScript timerScript))
                    {
                        Debug.LogError("Player " + i + " has no TimerScript attached!");
                        continue;
                    }

                    HealthComponent healthComponent = _players[i].GetComponentInChildren<HealthComponent>();
                    if (!healthComponent)
                    {
                        Debug.LogError("Player " + i + " has no HealthComponent attached!");
                        continue;
                    }

                    ScoreManager.AddPlayerTime(i, timerScript.GetAccumulatedTime);
                    ScoreManager.AddPlayerLives(i, healthComponent.CurrentLives);
                    ScoreManager.AddPlayerColor(i, _UsedPlayerVariantIndices[i]);
                }

                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }

        public void Resetart()
        {
            // Find and destroy all objects with a FighterController component
            FighterController[] fighterControllers = FindObjectsOfType<FighterController>();
            foreach (var fighterController in fighterControllers)
            {
                Destroy(fighterController.gameObject);
            }

            // Find and destroy all objects with a GodController component
            GodController[] godControllers = FindObjectsOfType<GodController>();
            foreach (var godController in godControllers)
            {
                Destroy(godController.gameObject);
            }

            _firebowls.Clear();
            _fighters.Clear();
            _aliveFighters.Clear();

            // Reset the spawn points
            _playerSpawnPointManager.ResetUsedSpawnPoints();

            GodRecentlySpawned = false;
            _restart = true;
        }

        public void PurgeLevel()
        {
            EnemyController[] enemies = FindObjectsOfType<EnemyController>();
            foreach (var enemy in enemies)
            {
                enemy.Kill();
            }
        }
    }
}
