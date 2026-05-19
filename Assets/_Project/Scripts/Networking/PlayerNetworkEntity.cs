using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace Reincarnate.Networking
{
    /// <summary>
    /// Main networked identity script for a player object.
    ///
    /// This does NOT handle movement yet.
    /// This does NOT read input yet.
    /// This only proves that the object exists on the server/client
    /// and that ownership is assigned correctly.
    /// </summary>
    public sealed class PlayerNetworkEntity : NetworkBehaviour
    {
        public override void OnStartServer()
        {
            base.OnStartServer();
            
            Debug.Log(
                $"[PlayerNetworkEntity] SERVER started player object." +
                $"ObjectId={ObjectId}, OwnerId={OwnerId}, OwnerValid={Owner.IsValid}"
                );
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            
            Debug.Log(
                $"[PlayerNetworkEntity] CLIENT started player object." +
                $"ObjectId={ObjectId}, OwnerId={OwnerId}, IsOwner={IsOwner}"
                );

            if (IsOwner)
            {
                Debug.Log(
                    $"[PlayerNetworkEntity] LOCAL CLIENT owns this player object." +
                    $"ObjectId={ObjectId}, OwnerId={OwnerId}"
                    );
            }
        }

        public override void OnOwnershipServer(NetworkConnection previousOwner)
        {
            base.OnOwnershipServer(previousOwner);
            
            Debug.Log(
                $"[PlayerNetworkEntity] SERVER ownership assigned/changed." +
                $"ObjectId={ObjectId}, PreviousOwner={GetConnectionDebugText(previousOwner)}, NewOwnerId={OwnerId}"
                );
        }

        public override void OnOwnershipClient(NetworkConnection previousOwner)
        {
            base.OnOwnershipClient(previousOwner);
            
            Debug.Log(
                $"[PlayerNetworkEntity] CLIENT ownership assigned/changed." +
                $"ObjectId={ObjectId}, PreviousOwner={GetConnectionDebugText(previousOwner)}, NewOwnerId={OwnerId}, IsOwner={IsOwner}"
                );
        }

        private static string GetConnectionDebugText(NetworkConnection connection)
        {
            if (!connection.IsValid)
            {
                return "None";
            }

            return connection.ClientId.ToString();
        }
    }
}