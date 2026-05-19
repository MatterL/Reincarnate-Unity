using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

namespace Reincarnate.Networking
{
    /// <summary>
    /// Server-side player spawning.
    ///
    /// This script creates one networked player object per connected client.
    ///
    /// Important rules:
    /// - Only the server spawns player objects.
    /// - Each client owns only its own player object.
    /// - Clients do not spawn themselves.
    /// - This does not handle movement.
    /// - This does not handle input.
    /// </summary>
    public sealed class PlayerSpawner : MonoBehaviour
    {
        [Header("Required")]
        [SerializeField]
        private NetworkObject playerPrefab;

        [Header("Debug")]
        [SerializeField]
        private float spawnSpacing = 2f;

        private readonly Dictionary<int, NetworkObject> spawnedPlayersByClientId = new();

        private NetworkManager networkManager;

        private void Awake()
        {
            networkManager = GetComponent<NetworkManager>();

            if (networkManager == null)
            {
                networkManager = FindFirstObjectByType<NetworkManager>();
            }

            if (networkManager == null)
            {
                Debug.LogError("[PlayerSpawner] No FishNet NetworkManager found in the scene.");
            }
        }

        private void OnEnable()
        {
            if (networkManager == null)
            {
                return;
            }

            networkManager.ServerManager.OnRemoteConnectionState += HandleRemoteConnectionState;
            networkManager.ClientManager.OnClientConnectionState += HandleLocalClientConnectionState;

            Debug.Log("[PlayerSpawner] Subscribed to FishNet connection events.");
        }

        private void OnDisable()
        {
            if (networkManager == null)
            {
                return;
            }

            networkManager.ServerManager.OnRemoteConnectionState -= HandleRemoteConnectionState;
            networkManager.ClientManager.OnClientConnectionState -= HandleLocalClientConnectionState;

            Debug.Log("[PlayerSpawner] Unsubscribed from FishNet connection events.");
        }

        private void HandleRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
        {
            Debug.Log(
                $"[PlayerSpawner] SERVER remote connection state changed. " +
                $"ClientId={args.ConnectionId}, State={args.ConnectionState}"
            );

            if (args.ConnectionState == RemoteConnectionState.Started)
            {
                SpawnPlayerForConnection(connection);
                return;
            }

            if (args.ConnectionState == RemoteConnectionState.Stopped)
            {
                ForgetPlayerForConnection(args.ConnectionId);
            }
        }

        private void HandleLocalClientConnectionState(ClientConnectionStateArgs args)
        {
            Debug.Log(
                $"[PlayerSpawner] LOCAL client connection state changed. " +
                $"State={args.ConnectionState}"
            );

            // This matters when running as Host.
            // A Host is both server and client in the same Unity instance.
            // We only spawn here if the local process is also the server.
            if (!networkManager.ServerManager.Started)
            {
                return;
            }

            if (args.ConnectionState != LocalConnectionState.Started)
            {
                return;
            }

            NetworkConnection localConnection = networkManager.ClientManager.Connection;

            Debug.Log(
                $"[PlayerSpawner] HOST local client connected. " +
                $"ClientId={localConnection.ClientId}"
            );

            SpawnPlayerForConnection(localConnection);
        }

        private void SpawnPlayerForConnection(NetworkConnection connection)
        {
            if (!connection.IsValid)
            {
                Debug.LogWarning("[PlayerSpawner] Tried to spawn player, but connection was not valid.");
                return;
            }

            if (playerPrefab == null)
            {
                Debug.LogError("[PlayerSpawner] Player prefab is not assigned.");
                return;
            }

            int clientId = connection.ClientId;

            if (spawnedPlayersByClientId.ContainsKey(clientId))
            {
                Debug.LogWarning(
                    $"[PlayerSpawner] Client already has a spawned player. " +
                    $"ClientId={clientId}"
                );

                return;
            }

            Vector3 spawnPosition = GetSpawnPositionForClient(clientId);

            NetworkObject playerInstance = Instantiate(
                playerPrefab,
                spawnPosition,
                Quaternion.identity
            );

            networkManager.ServerManager.Spawn(playerInstance, connection);

            spawnedPlayersByClientId.Add(clientId, playerInstance);

            Debug.Log(
                $"[PlayerSpawner] SERVER spawned player. " +
                $"ClientId={clientId}, ObjectId={playerInstance.ObjectId}, OwnerId={playerInstance.OwnerId}, Position={spawnPosition}"
            );
        }

        private void ForgetPlayerForConnection(int clientId)
        {
            if (!spawnedPlayersByClientId.Remove(clientId))
            {
                Debug.Log(
                    $"[PlayerSpawner] No tracked player to forget for disconnected client. " +
                    $"ClientId={clientId}"
                );

                return;
            }

            Debug.Log(
                $"[PlayerSpawner] Forgot tracked player for disconnected client. " +
                $"ClientId={clientId}"
            );
        }

        private Vector3 GetSpawnPositionForClient(int clientId)
        {
            // Temporary 2D spawn positions so players do not overlap visually.
            // This is not a movement system.
            return new Vector3(clientId * spawnSpacing, 0f, 0f);
        }
    }
}