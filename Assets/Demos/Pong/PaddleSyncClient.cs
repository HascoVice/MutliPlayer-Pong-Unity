using UnityEngine;
using System.Net;

public class PaddleSyncClient : MonoBehaviour
{
    public string paddleSide;
    private UDPService UDP;
    private ClientManager clientManager;

    void Awake()
    {
        if (Globals.IsServer)
        {
            enabled = false;
            return;
        }
    }

    void Start()
    {
        UDP = FindObjectOfType<UDPService>();
        clientManager = FindObjectOfType<ClientManager>();

        UDP.OnMessageReceived += (string message, IPEndPoint sender) =>
        {
            if (!message.StartsWith($"PADDLE_{paddleSide}_UPDATE")) return;

            string[] tokens = message.Split('|');
            PaddleState state = JsonUtility.FromJson<PaddleState>(tokens[1]);
            transform.position = state.Position;
        };
    }

    void Update()
    {
        if (!Globals.IsServer && paddleSide == "RIGHT")
        {
            PaddleState state = new PaddleState { Position = transform.position };
            string json = JsonUtility.ToJson(state);
            string message = $"PADDLE_{paddleSide}_MOVE|{json}";
            UDP.SendUDPMessage(message, clientManager.ServerEndpoint);
        }
    }
}
