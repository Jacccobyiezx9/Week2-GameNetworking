using UnityEngine;
using Fusion;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using System.Collections;

public class NetworkedGameManager : NetworkBehaviour
{
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new();
    private NetworkSessionManager networkSessionManager;
    [SerializeField] private NetworkPrefabRef playerPrefab;

    private int maxPlayers = 2;
    private int timerBeforeStart = 3;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI playerCountText;

    [Networked] public TickTimer RoundStartTimer { get; set; }

    private bool hasGameStarted = false;

    private void Awake()
    {
        networkSessionManager = GetComponent<NetworkSessionManager>();
    }

    public override void Spawned()
    {
        base.Spawned();
        NetworkSessionManager.Instance.OnPlayerJoinedEvent += OnPlayerJoined;
        NetworkSessionManager.Instance.OnPlayerLeftEvent += OnPlayerLeft;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        NetworkSessionManager.Instance.OnPlayerJoinedEvent -= OnPlayerJoined;
        NetworkSessionManager.Instance.OnPlayerLeftEvent -= OnPlayerLeft;
    }

    public override void FixedUpdateNetwork()
    {

        playerCountText.text = $"Players: {Object.Runner.ActivePlayers.Count()}/{maxPlayers}";

        if (RoundStartTimer.IsRunning)
        {
            timerText.text = RoundStartTimer.RemainingTime(Object.Runner).ToString();
        }
        else
        {
            timerText.text = " ";
        }

        if (RoundStartTimer.Expired(Object.Runner) && !hasGameStarted)
        {
            OnGameStarted();
            hasGameStarted = true;
        }
    }

    public override void Render()
    {
        base.Render();
    }

    private void OnPlayerJoined(PlayerRef player)
    {
        if (!HasStateAuthority) return;
        if (NetworkSessionManager.Instance.JoinedPlayers.Count >= maxPlayers)
        {
            // Start game countdown then spawn
            RoundStartTimer = TickTimer.CreateFromSeconds(Object.Runner, timerBeforeStart);
        }
        Debug.Log($"Player{player.PlayerId} joined");

    }
    private void OnPlayerLeft(PlayerRef player)
    {
        if (!HasStateAuthority) return;
        if (!_spawnedCharacters.TryGetValue(player, out var playerObject)) return;
        Object.Runner.Despawn(playerObject);
        _spawnedCharacters.Remove(player);
    }

    private void OnGameStarted()
    {
        Debug.Log("Game Started");
        StartCoroutine(SpawnInSequence());
    }

    private IEnumerator SpawnInSequence()
    {
        foreach (var player in NetworkSessionManager.Instance.JoinedPlayers)
        {
            yield return new WaitForSeconds(1f);
            var networkObj = Object.Runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);
            if (networkObj != null)
            {
                _spawnedCharacters.Add(player, networkObj);
            }
        }
    }
}



