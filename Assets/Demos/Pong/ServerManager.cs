using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    [SerializeField] private UDPService networkService;
    public UDPService UDP => networkService;
    [SerializeField] private int networkPort = 25000;

    private readonly Dictionary<string, IPEndPoint> connectedClients = new Dictionary<string, IPEndPoint>();
    private readonly Dictionary<IPEndPoint, string> paddleAssignments = new Dictionary<IPEndPoint, string>();

    private void Awake()
    {
        if (!Globals.IsServer)
        {
            DisableComponent();
        }
    }

    private void Start()
    {
        InitializeNetworking();
    }

    private void DisableComponent()
    {
        gameObject.SetActive(false);
    }

    private void InitializeNetworking()
    {
        networkService.Listen(networkPort);
        networkService.OnMessageReceived += ProcessIncomingMessage;
    }

    private void ProcessIncomingMessage(string data, IPEndPoint source)
    {
        Debug.Log($"Received: {data}");
        
        if (data.StartsWith("CONNECT"))
        {
            HandleConnectionRequest(source);
        }
        else if (data.StartsWith("UPDATE|PADDLE|"))
        {
            ProcessPaddleUpdate(data, source);
        }
    }

    private void HandleConnectionRequest(IPEndPoint clientEndpoint)
    {
        var clientId = FormatClientId(clientEndpoint);
        
        if (IsNewClient(clientId))
        {
            RegisterClient(clientId, clientEndpoint);
            var assignedPaddle = DeterminePaddleAssignment(clientEndpoint);
            NotifyClientOfAssignment(clientEndpoint, assignedPaddle);
        }

        LogClientCount();
        CheckAndStartGame();
    }

    private string FormatClientId(IPEndPoint endpoint)
    {
        return $"{endpoint.Address}:{endpoint.Port}";
    }

    private bool IsNewClient(string clientId)
    {
        return !connectedClients.ContainsKey(clientId);
    }

    private void RegisterClient(string clientId, IPEndPoint endpoint)
    {
        connectedClients.Add(clientId, endpoint);
    }

    private string DeterminePaddleAssignment(IPEndPoint clientEndpoint)
    {
        Debug.Log($"Assigning paddle for client: {clientEndpoint}");
        
        var paddleCounts = CountExistingPaddles();
        var assignedPaddle = DetermineOptimalPaddleAssignment(paddleCounts);
        
        RegisterPaddleAssignment(clientEndpoint, assignedPaddle);
        
        return assignedPaddle;
    }

    private Dictionary<string, int> CountExistingPaddles()
    {
        var counts = new Dictionary<string, int>
        {
            { "PaddleLeft", 0 },
            { "PaddleRight", 0 }
        };

        foreach (var assignment in paddleAssignments.Values)
        {
            if (counts.ContainsKey(assignment))
            {
                counts[assignment]++;
            }
        }

        return counts;
    }

    private string DetermineOptimalPaddleAssignment(Dictionary<string, int> paddleCounts)
    {
        return paddleCounts["PaddleLeft"] <= paddleCounts["PaddleRight"] 
            ? "PaddleLeft" 
            : "PaddleRight";
    }

    private void RegisterPaddleAssignment(IPEndPoint clientEndpoint, string paddleId)
    {
        paddleAssignments[clientEndpoint] = paddleId;
        Debug.Log($"Registered {paddleId} for client {clientEndpoint}");
    }

    private void NotifyClientOfAssignment(IPEndPoint clientEndpoint, string paddleId)
    {
        var message = $"ASSIGN|PADDLE|{paddleId}";
        networkService.SendUDPMessage(message, clientEndpoint);
    }

    private void LogClientCount()
    {
        Debug.Log($"Active clients: {connectedClients.Count}");
    }

    private void CheckAndStartGame()
    {
        if (connectedClients.Count >= 2)
        {
            BroadcastGameStart();
        }
    }

    private void BroadcastGameStart()
    {
        foreach (var client in connectedClients.Values)
        {
            networkService.SendUDPMessage("UPDATE|GAMESTART", client);
        }
    }

    private void ProcessPaddleUpdate(string message, IPEndPoint source)
    {
        Debug.Log($"[SERVER] Update from {source.Address}:{source.Port} => {message}");
        
        var segments = message.Split('|');
        if (!ValidatePaddleUpdate(segments)) return;

        var paddleSide = segments[2];
        var positionData = segments[3];

        if (!ValidatePositionData(positionData)) return;
        
        UpdatePaddlePosition(paddleSide, positionData, source);
    }

    private bool ValidatePaddleUpdate(string[] segments)
    {
        return segments.Length >= 4;
    }

    private bool ValidatePositionData(string data)
    {
        return data.StartsWith("Y:");
    }

    private void UpdatePaddlePosition(string paddleSide, string positionData, IPEndPoint source)
    {
        var yPosition = float.Parse(positionData.Substring(2), 
            System.Globalization.CultureInfo.InvariantCulture);

        foreach (var assignment in paddleAssignments)
        {
            if (IsValidPaddleUpdate(assignment, source, paddleSide))
            {
                SynchronizePaddlePosition(paddleSide, yPosition);
                break;
            }
        }
    }

    private bool IsValidPaddleUpdate(KeyValuePair<IPEndPoint, string> assignment, 
        IPEndPoint source, string paddleSide)
    {
        return assignment.Key.Equals(source) && assignment.Value == paddleSide;
    }

    private void SynchronizePaddlePosition(string paddleId, float yPosition)
    {
        var paddle = GameObject.Find(paddleId);
        if (paddle != null)
        {
            var position = paddle.transform.position;
            paddle.transform.position = new Vector3(position.x, yPosition, position.z);
        }
    }

    public void BroadcastUDPMessage(string message, IPEndPoint excludedClient = null)
    {
        foreach (var client in connectedClients)
        {
            if (client.Value.Equals(excludedClient)) continue;
            networkService.SendUDPMessage(message, client.Value);
        }
    }
}