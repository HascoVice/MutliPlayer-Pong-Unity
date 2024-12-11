using UnityEngine;
using System.Net;

public class PaddleSyncClient : MonoBehaviour
{
    public string paddleSide;
    private UDPService UDP;
    private ClientManager clientManager;
    private float nextUpdateTime = 0f;
    private const float UPDATE_RATE = 0.02f;

    void Start()
    {
        if (Globals.IsServer)
        {
            enabled = false;
            return;
        }

        UDP = FindObjectOfType<UDPService>();
        clientManager = FindObjectOfType<ClientManager>();
        
        if (UDP == null || clientManager == null)
        {
            Debug.LogError("Required components not found!");
            enabled = false;
            return;
        }

        UDP.OnMessageReceived += (string message, IPEndPoint sender) =>
        {
            Debug.Log($"[CLIENT] Received message: {message}");

            if (!message.StartsWith($"PADDLE_{paddleSide}_UPDATE")) return;

            string[] tokens = message.Split('|');
            if (tokens.Length != 2) return;

            var paddle = GetComponent<PongPaddle>();
            Debug.Log($"[CLIENT] Paddle enabled: {paddle.enabled}");
            if (paddle && !paddle.enabled)
            {
                PaddleState state = JsonUtility.FromJson<PaddleState>(tokens[1]);
                transform.position = state.Position;
            }
        };
    }

    void Update()
    {
        if (Time.time < nextUpdateTime) return;
        
        var paddle = GetComponent<PongPaddle>();
        Debug.Log($"[CLIENT] Paddle {paddleSide} enabled status: {paddle.enabled}");
        if (paddle && paddle.enabled)
        {
            PaddleState state = new PaddleState { Position = transform.position };
            string json = JsonUtility.ToJson(state);
            string message = $"PADDLE_{paddleSide}_MOVE|{json}";
            UDP.SendUDPMessage(message, clientManager.ServerEndpoint);
        }

        nextUpdateTime = Time.time + UPDATE_RATE;
    }
}
