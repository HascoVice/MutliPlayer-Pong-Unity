using UnityEngine;
using UnityEngine.SceneManagement;

public class PongWinUI : MonoBehaviour
{
    public GameObject Panel;
    public GameObject PlayerLeft;
    public GameObject PlayerRight;

    private PongBall Ball;
    private Scoring ScoringSystem;

    void Start()
    {
        Panel.SetActive(false);
        PlayerLeft.SetActive(false);
        PlayerRight.SetActive(false);
        Ball = GameObject.FindFirstObjectByType<PongBall>();
        ScoringSystem = GameObject.FindFirstObjectByType<Scoring>();
    }

    void Update()
    {
        switch (Ball.State) 
        {
            case PongBallState.Playing:
                Panel.SetActive(false);
                PlayerLeft.SetActive(false);
                PlayerRight.SetActive(false);
                break;
            case PongBallState.PlayerLeftWin:
                Panel.SetActive(true);
                PlayerLeft.SetActive(true);
                PlayerRight.SetActive(false);
                break;
            case PongBallState.PlayerRightWin:
                Panel.SetActive(true);
                PlayerLeft.SetActive(false);
                PlayerRight.SetActive(true);
                break;
        }
    }

    public void OnReplay() 
    {
        ScoringSystem.ResetScores();
        Ball.State = PongBallState.Playing;
        Panel.SetActive(false);
        PlayerLeft.SetActive(false);
        PlayerRight.SetActive(false);
    }
}