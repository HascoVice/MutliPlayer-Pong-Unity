using System.Net;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    public UDPService UDP;
    public string ServerIP = "127.0.0.1";
    public int ServerPort = 25000;

    public IPEndPoint ServerEndpoint;

    private float NextCoucouTimeout = -1;

    void Awake()
    {
        if (Globals.IsServer)
        {
            gameObject.SetActive(false); // Désactive si ce n'est pas un client
        }
    }

    void Start()
    {
        if (UDP == null)
        {
            Debug.LogError("[ClientManager] UDPService not assigned in the Inspector!");
            enabled = false; // Désactive le script
            return;
        }

        UDP.InitClient();

        try
        {
            ServerEndpoint = new IPEndPoint(IPAddress.Parse(ServerIP), ServerPort);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ClientManager] Error initializing ServerEndpoint: {ex.Message}");
            enabled = false; // Désactive le script si l'adresse ou le port sont invalides
            return;
        }

        UDP.OnMessageReceived += (string message, IPEndPoint sender) =>
        {
            Debug.Log("[CLIENT] Message received from " +
                      sender.Address.ToString() + ":" + sender.Port
                      + " =>" + message);
        };
    }

    void Update()
    {
        if (ServerEndpoint == null)
        {
            Debug.LogError("[ClientManager] ServerEndpoint is not set!");
            return;
        }

        if (Time.time > NextCoucouTimeout)
        {
            UDP.SendUDPMessage("coucou", ServerEndpoint);
            NextCoucouTimeout = Time.time + 0.5f;
        }
    }
}
