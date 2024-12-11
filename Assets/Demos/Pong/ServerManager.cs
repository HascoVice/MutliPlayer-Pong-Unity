using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    public UDPService UDP;
    public int ListenPort = 25000;
    private List<IPEndPoint> connectedClients = new List<IPEndPoint>();

    void Awake()
    {
        if (!Globals.IsServer)
        {
            gameObject.SetActive(false);
        }
    }

    void Start()
    {
        if (!Globals.IsServer) return;

        UDP.Listen(ListenPort);

        UDP.OnMessageReceived += (string message, IPEndPoint sender) =>
        {
            if (message == "coucou")  // Message de connexion du client
            {
                Debug.Log($"[SERVER] New client connected from {sender}");
                
                if (!connectedClients.Contains(sender))
                {
                    connectedClients.Add(sender);
                    
                    // Assigne LEFT au premier client, RIGHT au second
                    string paddleSide = connectedClients.Count == 1 ? "LEFT" : "RIGHT";
                    string assignMessage = $"ASSIGN_PADDLE_{paddleSide}";
                    
                    UDP.SendUDPMessage(assignMessage, sender);
                    Debug.Log($"[SERVER] Assigned {paddleSide} paddle to {sender}");
                }
            }
        };
    }

    public void BroadcastUDPMessage(string message)
    {
        foreach (var client in connectedClients)
        {
            UDP.SendUDPMessage(message, client);
        }
    }
}
