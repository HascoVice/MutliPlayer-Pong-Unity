using System;
using System.Net;
using UnityEngine;

public class PaddleSyncServer : MonoBehaviour
{
    public enum PaddleSide { LEFT, RIGHT }
    public PaddleSide paddleSide;

    private ServerManager serverManager;
    private float paddlePositionY;

    float NextUpdateTimeout = -1;
    
    private IPEndPoint lastMessageSender;

    void Awake()
    {
        if (!Globals.IsServer)
        {
            enabled = false;
        }
    }

    void Start()
    {   
        serverManager = FindObjectOfType<ServerManager>();
        if (serverManager == null)
        {
            Debug.LogError("ServerManager not found in scene!");
            enabled = false;
            return;
        }
        
        serverManager.UDP.OnMessageReceived += OnMessageReceived;
        paddlePositionY = transform.position.y;
    }
    
    void Update()
    {
        float currentPositionY = transform.position.y;
        if (Time.time > NextUpdateTimeout)
        {
            string message = $"UPDATE|PADDLE|{paddleSide}|Y:{currentPositionY.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            serverManager.BroadcastUDPMessage(message, lastMessageSender);            
            NextUpdateTimeout = Time.time + 0.03f;
        }
    }

    private void OnMessageReceived(string message, IPEndPoint sender)
    {
        if (!message.StartsWith("UPDATE|PADDLE|")) return;

        lastMessageSender = sender;

        string[] tokens = message.Split('|');
        if (tokens.Length < 4 || !Enum.TryParse(tokens[2], out PaddleSide receivedSide)) return;

        if (receivedSide == paddleSide)
        {
            string positionData = tokens[3].Replace("Y:", "");
            if (float.TryParse(positionData, System.Globalization.NumberStyles.Float, 
                    System.Globalization.CultureInfo.InvariantCulture, out float newPositionY))
            {
                paddlePositionY = newPositionY;
                UpdatePaddlePosition();
            }
        }
    }

    private void UpdatePaddlePosition()
    {
        Vector3 newPosition = transform.position;
        newPosition.y = paddlePositionY;
        transform.position = newPosition;

        Debug.Log($"Paddle {paddleSide} position updated to Y: {paddlePositionY}");
    }

    void OnDestroy()
    {
        if (serverManager?.UDP != null)
        {
            serverManager.UDP.OnMessageReceived -= OnMessageReceived;
        }
    }
}