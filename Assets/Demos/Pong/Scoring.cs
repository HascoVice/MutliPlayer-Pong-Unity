using UnityEngine;
using UnityEngine.UI;

public class Scoring : MonoBehaviour
{
    public TMPro.TMP_Text Player1Score;
    public TMPro.TMP_Text Player2Score;
    public PongBall ball;
    public int maxScore = 5;

    private int leftScore = 0;
    private int rightScore = 0;
    private bool gameEnded = false;

    void Start()
    {
        Player1Score.text = "0";
        Player2Score.text = "0";
        gameEnded = false;
    }

    void Update()
    {
        if (gameEnded) return; 

        if (ball.State == PongBallState.PlayerLeftWin)
        {
            leftScore++;
            Player1Score.text = leftScore.ToString();
            if (CheckGameEnd())
            {
                EndGame();
            }
            else
            {
                ball.State = PongBallState.Playing;
            }
        }
        else if (ball.State == PongBallState.PlayerRightWin)
        {
            rightScore++;
            Player2Score.text = rightScore.ToString();
            if (CheckGameEnd())
            {
                EndGame();
            }
            else
            {
                ball.State = PongBallState.Playing;
            }
        }
    }

    bool CheckGameEnd()
    {
        return leftScore >= maxScore || rightScore >= maxScore;
    }

    void EndGame()
    {
        gameEnded = true;
        ball.SetGameEnded(true);

    }

    public void ResetScores()
    {
        leftScore = 0;
        rightScore = 0;
        Player1Score.text = "0";
        Player2Score.text = "0";
        gameEnded = false;
        ball.SetGameEnded(false);
        ball.State = PongBallState.Playing;
    }
}