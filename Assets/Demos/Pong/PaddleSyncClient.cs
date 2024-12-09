using UnityEngine;
using System.Net;

public class PaddleSyncClient : MonoBehaviour
{
    public string paddleSide; // "LEFT" ou "RIGHT"
    private UDPService udpService;
    private ClientManager clientManager;
    private float moveSpeed = 5f;

    void Start()
    {
        // Trouver les services dans la scène
        udpService = FindObjectOfType<UDPService>();
        clientManager = FindObjectOfType<ClientManager>();

        // Vérifications des dépendances
        if (udpService == null)
        {
            Debug.LogError("[PaddleSyncClient] UDPService not found in the scene! Please add it.");
            enabled = false; // Désactive le script
            return;
        }

        if (clientManager == null)
        {
            Debug.LogError("[PaddleSyncClient] ClientManager not found in the scene! Please add it.");
            enabled = false; // Désactive le script
            return;
        }

        if (string.IsNullOrEmpty(paddleSide))
        {
            Debug.LogError("[PaddleSyncClient] PaddleSide is not set! Please assign 'LEFT' or 'RIGHT' in the Inspector.");
            enabled = false; // Désactive le script
            return;
        }

        udpService.OnMessageReceived += (string message, IPEndPoint sender) =>
        {
            if (!message.StartsWith($"PADDLE_{paddleSide}_UPDATE")) return;

            string[] tokens = message.Split('|');
            PaddleState state = JsonUtility.FromJson<PaddleState>(tokens[1]);

            transform.position = state.Position;
        };
    }

    void Update()
    {
        if (udpService == null || clientManager == null || clientManager.ServerEndpoint == null)
        {
            Debug.LogError("[PaddleSyncClient] Missing dependencies in Update. Ensure everything is properly set.");
            return;
        }

        float movement = Input.GetAxis($"Vertical_{paddleSide}"); // Par exemple, "Vertical_LEFT" ou "Vertical_RIGHT"

        if (movement != 0)
        {
            Vector3 newPosition = transform.position;
            newPosition.y += movement * moveSpeed * Time.deltaTime;
            transform.position = newPosition;

            PaddleState state = new PaddleState { Position = newPosition };
            string json = JsonUtility.ToJson(state);
            string message = $"PADDLE_{paddleSide}_MOVE|" + json;

            udpService.SendUDPMessage(message, clientManager.ServerEndpoint);
        }
    }
}
