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
            gameObject.SetActive(false);
        }
    }

    void Start()
    {
        if (UDP == null)
        {
            Debug.LogError("[ClientManager] UDPService not assigned in the Inspector!");
            enabled = false;
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
            enabled = false;
            return;
        }

        UDP.OnMessageReceived += (string message, IPEndPoint sender) =>
        {
            Debug.Log("[CLIENT] Message received from " +
                      sender.Address.ToString() + ":" + sender.Port
                      + " =>" + message);

            if (message.StartsWith("ASSIGN_PADDLE_"))
            {
                string[] parts = message.Split('_');
                string paddleSide = parts[2];
                
                if (paddleSide == "LEFT")
                {
                    Globals.IsLeftPlayer = true;
                    Globals.IsRightPlayer = false;
                }
                else if (paddleSide == "RIGHT") 
                {
                    Globals.IsLeftPlayer = false;
                    Globals.IsRightPlayer = true;
                }
                
                if (parts.Length != 3)
                {
                    Debug.LogError("Invalid ASSIGN_PADDLE message format");
                    return;
                }

                if (paddleSide != "LEFT" && paddleSide != "RIGHT")
                {
                    Debug.LogError("Invalid paddle side: " + paddleSide);
                    return;
                }

                var paddles = FindObjectsOfType<PongPaddle>();
                foreach (var paddle in paddles)
                {
                    bool shouldBeEnabled = 
                        (paddleSide == "LEFT" && paddle.Player == PongPlayer.PlayerLeft) ||
                        (paddleSide == "RIGHT" && paddle.Player == PongPlayer.PlayerRight);

                    paddle.enabled = shouldBeEnabled;
                    var syncClient = paddle.GetComponent<PaddleSyncClient>();
                    if (syncClient) syncClient.enabled = shouldBeEnabled;
                }
            }
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
