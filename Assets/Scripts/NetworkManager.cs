using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    #region Public Variables
    [SerializeField] private NetworkPrefabRef playerPrefab;
    #endregion
    #region Private Variables
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = null;
    private NetworkRunner _networkRunner;
    #endregion

    public async void StartGame(GameMode gameMode)
    {
        _networkRunner = this.gameObject.AddComponent<NetworkRunner>();
        _networkRunner.ProvideInput = true;

        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();

        if(scene.IsValid)
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

        await _networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = gameMode,
            SessionName = "TestSession",
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });//Task<StartGameResult>
    }

    #region Unity Callbacks
    private void Start()
    {

       #if SERVER
       StartGame(GameMode.Host);
       #elif CLIENT
       StartGame(GameMode.Client);
       #endif
    }
       #endregion


    #region Used Fusion Callbacks
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();
        data.InputVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input.Set(data);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            var pos = Vector3.zero;
            var networkObj = runner.Spawn(playerPrefab, pos, Quaternion.identity, player);

            _spawnedCharacters.Add(player, networkObj);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if(!_spawnedCharacters.TryGetValue(player, out var playerObject)) return;

        runner.Despawn(playerObject);
        _spawnedCharacters.Remove(player);
    }
    #endregion

    #region Unsused Fusion Callbacks
    public void OnConnectedToServer(NetworkRunner runner)
    {
        
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        
    }

 
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }


    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        
    }
    #endregion
}
