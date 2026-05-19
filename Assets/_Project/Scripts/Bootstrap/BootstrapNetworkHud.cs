using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;

namespace Reincarnate.Bootstrap
{
    public sealed class BootstrapNetworkHud : MonoBehaviour
    {
        private NetworkManager _networkManager;

        private void Awake()
        {
            _networkManager = GetComponent<NetworkManager>();

            if (_networkManager == null)
            {
                Debug.LogError(
                    "[BoostrapNetworkHud] Missing NetworkManager component. Put this script on the same GameObject as the FishNet NetworkManager.");
                enabled = false;
                return;
            }

            Debug.Log("[BootstrapNetworkHud] Ready.");
        }

        private void OnEnable()
        {
            if (_networkManager == null)
                _networkManager = GetComponent<NetworkManager>();

            if (_networkManager == null)
                return;

            _networkManager.ServerManager.OnServerConnectionState += HandleServerConnectionState;
            _networkManager.ServerManager.OnRemoteConnectionState += HandleRemoteConnectionState;
            _networkManager.ClientManager.OnClientConnectionState += HandleClientConnectionState;
            _networkManager.ClientManager.OnAuthenticated += HandleClientAuthenticated;
        }

        private void OnDisable()
        {
            if (_networkManager == null)
                return;

            _networkManager.ServerManager.OnServerConnectionState -= HandleServerConnectionState;
            _networkManager.ServerManager.OnRemoteConnectionState -= HandleRemoteConnectionState;
            _networkManager.ClientManager.OnClientConnectionState -= HandleClientConnectionState;
            _networkManager.ClientManager.OnAuthenticated -= HandleClientAuthenticated;
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 220, 170), "Network Controls", GUI.skin.window);

            if (GUILayout.Button("Start Host", GUILayout.Height(35)))
                StartHost();

            if (GUILayout.Button("Start Client", GUILayout.Height(35)))
                StartClient();

            if (GUILayout.Button("Stop All", GUILayout.Height(35)))
                StopAll();

            GUILayout.EndArea();
        }

        private void StartHost()
        {
            Debug.Log("[BootstrapNetworkHud] Start Host pressed.");

            if (!_networkManager.ServerManager.Started)
            {
                bool serverStarted = _networkManager.ServerManager.StartConnection();
                Debug.Log($"[BootstrapNetworkHud] Server start requested. Result: {serverStarted}");
            }

            if (!_networkManager.ClientManager.Started)
            {
                bool clientStarted = _networkManager.ClientManager.StartConnection();
                Debug.Log($"[BootstrapNetworkHud] Local client start requested. Result: {clientStarted}");
            }
        }

        private void StartClient()
        {
            {
                Debug.Log("[BootstrapNetworkHud] Start Client pressed.");

                if (_networkManager.ClientManager.Started)
                {
                    Debug.LogWarning("[BootstrapNetworkHud] Client is already started.");
                    return;
                }

                bool clientStarted = _networkManager.ClientManager.StartConnection();
                Debug.Log($"[BootstrapNetworkHud] Client start requested. Result: {clientStarted}");
            }
        }

        private void StopAll()
        {
            Debug.Log("[BootstrapNetworkHud] Stop All pressed.");

            if (_networkManager.ClientManager.Started)
            {
                bool clientStopped = _networkManager.ClientManager.StopConnection();
                Debug.Log($"[BootstrapNetworkHud] Client stop requested. Result: {clientStopped}");
            }

            if (_networkManager.ServerManager.Started)
            {
                bool serverStopped = _networkManager.ServerManager.StopConnection(true);
                Debug.Log($"[BootstrapNetworkHud] Server stop requested. Result: {serverStopped}");
            }
        }

        private void HandleServerConnectionState(ServerConnectionStateArgs args)
        {
            Debug.Log($"[Server] Local server state changed: {args.ConnectionState}");
        }

        private void HandleRemoteConnectionState(FishNet.Connection.NetworkConnection connection,
            RemoteConnectionStateArgs args)
        {
            Debug.Log(
                $"[Server] Remote client state changed. ClientId: {connection.ClientId}, State: {args.ConnectionState}");
        }

        private void HandleClientConnectionState(ClientConnectionStateArgs args)
        {
            Debug.Log($"[Client] Local client state changed: {args.ConnectionState}");
        }

        private void HandleClientAuthenticated()
        {
            Debug.Log("[Client] Local client authenticated.");
        }
    }
}