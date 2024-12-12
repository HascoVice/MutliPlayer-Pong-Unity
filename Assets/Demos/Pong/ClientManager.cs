using System;
using System.Net;
using UnityEngine;
using System.Globalization;

public class ClientManager : MonoBehaviour
{
    public UDPService UDP;
    public string ServerIP = "127.0.0.1";
    
    public int ServerPort = 25000;
    public IPEndPoint ServerEndpoint;

    private string assignedPaddle = "";

    public PongPaddle paddleLeftPlayer;
    public PongPaddle paddleRightPlayer;

    public event Action<PaddleSyncClient.PaddleSide, float> OnPaddlePositionUpdated;

    void Awake()
    {
        if (Globals.IsServer)
        {
            gameObject.SetActive(false);
        }
    }

    void Start()
    {
        UDP.InitClient();
    
        ServerEndpoint = new IPEndPoint(IPAddress.Parse(ServerIP), ServerPort);
    
        SendConnectMessage();

        UDP.OnMessageReceived += HandleMessage;
    }

    private void SendConnectMessage()
    {
        Debug.Log("Sending CONNECT message to server: " + ServerEndpoint.ToString());
        UDP.SendUDPMessage("CONNECT", ServerEndpoint);
    }

    private void HandleMessage(string message, IPEndPoint sender)
    {
        Debug.Log($"Message : {message}");

        if (message.StartsWith("ASSIGN|PADDLE|"))
        {
            HandlePaddleAssignment(message);
        }
        else if (message.StartsWith("UPDATE|PADDLE|"))
        {
            HandlePaddleUpdate(message);
        }

    }

    private void HandlePaddleAssignment(string message)
    {
        string[] tokens = message.Split('|');
        if (tokens.Length == 3)
        {
            assignedPaddle = tokens[2];
            Debug.Log($"Assigned paddle value: {assignedPaddle}");

            if (assignedPaddle == "PaddleRight")
            {
                Debug.Log("Disabling paddleRightPlayer, Enabling paddleLeftPlayer");
                paddleLeftPlayer.enabled = true;
                paddleRightPlayer.enabled = false;
            }
            else
            {
                Debug.Log("Disabling paddleLeftPlayer, Enabling paddleRightPlayer");
                paddleLeftPlayer.enabled = false;
                paddleRightPlayer.enabled = true;
            }
        }
    }

    private void HandlePaddleUpdate(string message)
    {
        try
        {
            string[] tokens = message.Split('|');
            if (tokens.Length < 4)
            {
                Debug.LogWarning("Message mal formé : parties insuffisantes.");
                return;
            }

            string receivedPaddleSide = tokens[2];
            if (!Enum.TryParse(receivedPaddleSide, out PaddleSyncClient.PaddleSide paddleSide))
            {
                Debug.LogWarning("PaddleSide invalide.");
                return;
            }

            string data = tokens[3];
            string[] positionData = data.Split(':');

            if (positionData.Length != 2 || positionData[0] != "Y")
            {
                Debug.LogWarning($"Données de position mal formées : {data}");
                return;
            }

            float y = float.Parse(positionData[1], CultureInfo.InvariantCulture);
            OnPaddlePositionUpdated?.Invoke(paddleSide, y);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Erreur lors du parsing des données de position : {ex.Message}");
        }
    }


    public void SendPaddleUpdate(PaddleSyncClient.PaddleSide paddleSide, float positionY)
    {
        string message = $"UPDATE|PADDLE|{paddleSide}|Y:{positionY.ToString(CultureInfo.InvariantCulture)}";
        UDP.SendUDPMessage(message, ServerEndpoint);
        Debug.Log($"Message envoyé par le client {ServerEndpoint.Address}:{ServerEndpoint.Port} => {message}");
    }
}