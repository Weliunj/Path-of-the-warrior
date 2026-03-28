using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using TMPro;

public class RoomManager : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    public TMP_InputField PlayerNameInput;
    public TMP_InputField RoomNameInput;
    public GameObject menuPanel;

    // Biến static để script PlayerInfo có thể lấy tên để hiển thị trên đầu nhân vật
    public static string LocalPlayerName;

    public void OnJoinButtonClicked()
    {
        // 1. Lấy tên người chơi (nếu trống thì đặt tên ngẫu nhiên)
        LocalPlayerName = PlayerNameInput.text;
        if (string.IsNullOrEmpty(LocalPlayerName)) 
        {
            LocalPlayerName = "Guest_" + UnityEngine.Random.Range(1000, 9999);
        }

        // 2. Lấy tên phòng
        string roomName = RoomNameInput.text;
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogError("Bạn phải nhập tên phòng!");
            return;
        }

        // 3. Bắt đầu kết nối Shared Mode
        StartSharedGame(roomName);
    }

    public async void StartSharedGame(string roomName)
    {
        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
        }

        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = roomName,
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
        // menuPanel.SetActive(false);
    }

    // --- CÁC CALLBACK ĐÃ CẬP NHẬT THEO FUSION 2 ---

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
    {
        if (player == runner.LocalPlayer)
        {
            runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity, player);
        }
    }

    #region Blaa

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    // --- CÁC HÀM CÒN LẠI (GIỮ NGUYÊN) ---

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    #endregion
}