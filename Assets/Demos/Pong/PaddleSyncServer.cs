using UnityEngine;
using System.Net;

public class PaddleSyncServer : MonoBehaviour
{
    public string paddleSide;
    private ServerManager serverManager;

    void Start()
    {
        if (!Globals.IsServer)
        {
            enabled = false;
            return;
        }

        if (string.IsNullOrEmpty(paddleSide) || (paddleSide != "LEFT" && paddleSide != "RIGHT"))
        {
            Debug.LogError("Invalid paddleSide configuration");
            enabled = false;
            return;
        }

        serverManager = FindObjectOfType<ServerManager>();
        if (serverManager == null)
        {
            Debug.LogError("ServerManager not found!");
            enabled = false;
            return;
        }

        serverManager.UDP.OnMessageReceived += (string message, IPEndPoint sender) => {
            Debug.Log($"[SERVER] Received message: {message}");
            
            if (!message.StartsWith($"PADDLE_{paddleSide}_MOVE")) return;

            string[] parts = message.Split('|');
            if (parts.Length != 2)
            {
                Debug.LogError("Invalid paddle move message format");
                return;
            }

            // Appliquer la position sur le serveur
            PaddleState state = JsonUtility.FromJson<PaddleState>(parts[1]);
            transform.position = state.Position;

            // Broadcaster aux clients
            string broadcastMessage = $"PADDLE_{paddleSide}_UPDATE|" + parts[1];
            serverManager.BroadcastUDPMessage(broadcastMessage);
            Debug.Log($"[SERVER] Broadcasting: {broadcastMessage}");
        };
    }
}
