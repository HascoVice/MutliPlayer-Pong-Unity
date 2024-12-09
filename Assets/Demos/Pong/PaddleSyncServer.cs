using UnityEngine;
using System.Net;

public class PaddleSyncServer : MonoBehaviour
{
    public string paddleSide; // "LEFT" ou "RIGHT"
    private ServerManager serverManager;
    private float nextUpdateTime = 0f;

    void Start()
    {
        serverManager = FindObjectOfType<ServerManager>();
        Debug.Log($"[SERVER] {paddleSide} PaddleSyncServer started");

        serverManager.UDP.OnMessageReceived += (string message, IPEndPoint sender) => {
            if (!message.StartsWith($"PADDLE_{paddleSide}_MOVE")) return;

            string[] tokens = message.Split('|');
            PaddleState state = JsonUtility.FromJson<PaddleState>(tokens[1]);

            transform.position = state.Position;
            Debug.Log($"[SERVER] Updated {paddleSide} paddle position to: {state.Position}");
        };
    }

    void Update()
    {
        if (Time.time > nextUpdateTime)
        {
            PaddleState state = new PaddleState { Position = transform.position };
            string json = JsonUtility.ToJson(state);
            string message = $"PADDLE_{paddleSide}_UPDATE|" + json;

            serverManager.BroadcastUDPMessage(message);
            nextUpdateTime = Time.time + 0.03f;
        }
    }
}

[System.Serializable]
public class PaddleState
{
    public Vector3 Position;
}
