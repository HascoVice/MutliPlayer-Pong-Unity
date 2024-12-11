using UnityEngine;
using UnityEngine.InputSystem;

public enum PongPlayer
{
    PlayerLeft = 1,
    PlayerRight = 2
}

public class PongPaddle : MonoBehaviour
{
    public PongPlayer Player = PongPlayer.PlayerLeft; // Définir si le paddle est à gauche ou à droite
    public float Speed = 5f; // Vitesse de mouvement
    public float MinY = -4f; // Limite inférieure
    public float MaxY = 4f; // Limite supérieure

    private InputSystem_Actions inputActions; // Référence à la classe générée
    private InputAction paddleMoveAction; // Action spécifique pour PaddleMove

    void Awake()
    {
        // Initialisation des Input Actions
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        // Récupérer l'action PaddleMove depuis l'Action Map Player
        paddleMoveAction = inputActions.Player.PaddleMove;

        // Activer l'action PaddleMove
        paddleMoveAction.Enable();
    }

    void OnDisable()
    {
        // Désactiver l'action PaddleMove
        paddleMoveAction.Disable();
    }

    void Update()
    {
        // Vérifier si ce joueur peut contrôler ce paddle
        bool canControl = (Globals.IsServer && Player == PongPlayer.PlayerLeft) || 
                         (!Globals.IsServer && Player == PongPlayer.PlayerRight);
        
        if (!canControl) return;

        // Lire la valeur d'entrée de PaddleMove
        float direction = paddleMoveAction.ReadValue<float>();

        // Calculer la nouvelle position
        Vector3 newPosition = transform.position;
        newPosition.y += direction * Speed * Time.deltaTime;
        newPosition.y = Mathf.Clamp(newPosition.y, MinY, MaxY);

        // Appliquer la nouvelle position
        transform.position = newPosition;
    }
}
