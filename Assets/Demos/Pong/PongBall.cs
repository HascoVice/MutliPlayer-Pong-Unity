using UnityEngine;
using System.Collections;

public enum PongBallState {
  Playing = 0,
  PlayerLeftWin = 1,
  PlayerRightWin = 2,
}

public class PongBall : MonoBehaviour
{
    public float Speed = 1;
    public float ScoreDelay = 0.5f; 
    private Vector3 StartPosition;
    private Vector3 Direction;
    public PongBallState State { get; set; }
    private bool gameEnded = false;
    private bool isWaitingToStart = false;

    void Start()
    {
        StartPosition = transform.position;
        ResetBall();
    }

    void ResetBall()
    {
        transform.position = StartPosition;
        isWaitingToStart = true;
        Direction = new Vector3(
            Random.Range(0.5f, 1),
            Random.Range(-0.5f, 0.5f),
            0
        );
        Direction.x *= Mathf.Sign(Random.Range(-100, 100));
        Direction.Normalize();

        StartCoroutine(StartAfterDelay());
    }

    IEnumerator StartAfterDelay()
    {
        yield return new WaitForSeconds(ScoreDelay);
        isWaitingToStart = false;
    }

    void Awake()
    {
        if (!Globals.IsServer)
        {
            enabled = false;
        }
    }

    void OnCollisionEnter(Collision c)
    {
        if (gameEnded) return;  // Skip collision handling if game has ended

        switch (c.collider.name)
        {
            case "BoundTop":
            case "BoundBottom":
                Direction.y = -Direction.y;
                break;

            case "PaddleLeft":
            case "PaddleRight":
                Direction.x = -Direction.x;
                break;

            case "BoundLeft":
                State = PongBallState.PlayerRightWin;
                ResetBall();
                break;

            case "BoundRight":
                State = PongBallState.PlayerLeftWin;
                ResetBall();
                break;
        }
    }

    void Update()
    {
        if (State != PongBallState.Playing || gameEnded || isWaitingToStart)
        {
            return;
        }

        transform.position = transform.position + (Direction * Speed * Time.deltaTime);
    }

    public void SetGameEnded(bool ended)
    {
        gameEnded = ended;
        if (ended)
        {
            transform.position = StartPosition;
        }
    }
}