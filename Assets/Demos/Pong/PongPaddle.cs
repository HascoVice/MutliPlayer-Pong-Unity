using UnityEngine;
using UnityEngine.InputSystem;

public enum PongPlayer
{
    PlayerLeft = 1,
    PlayerRight = 2
}

public class PongPaddle : MonoBehaviour
{
    public PongPlayer Player = PongPlayer.PlayerLeft;
    public float Speed = 5f;
    public float MinY = -4f;
    public float MaxY = 4f;

    private PongInput inputActions;
    private InputAction moveAction;

    void Awake()
    {
        if (Globals.IsServer)
        {
            enabled = false;
            return;
        }
        inputActions = new PongInput();
    }

    void OnEnable()
    {
        if (!Globals.IsServer && inputActions != null)
        {
            inputActions.Enable();
            moveAction = inputActions.Pong.Player1;
            
            bool isLocalPlayer = (Player == PongPlayer.PlayerLeft && Globals.IsLeftPlayer) ||
                               (Player == PongPlayer.PlayerRight && Globals.IsRightPlayer);
            
            enabled = isLocalPlayer;
            moveAction.Enable();
        }
    }

    void OnDisable()
    {
        if (moveAction != null)
            moveAction.Disable();
        if (inputActions != null)
            inputActions.Disable();
    }

    void Update()
    {
        if (moveAction == null) return;
        
        float direction = moveAction.ReadValue<float>();
        Vector3 newPosition = transform.position;
        newPosition.y += direction * Speed * Time.deltaTime;
        newPosition.y = Mathf.Clamp(newPosition.y, MinY, MaxY);
        transform.position = newPosition;
    }
}
