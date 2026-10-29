using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject cowboyPrefab;
    public Transform[] spawnPoints;
    public Shooting shooting;

    // Gameplay UI
    public TMP_Text roundText;
    public TMP_Text scoreText;

    // Results UI
    public GameObject resultsPanel;
    public TMP_Text resultsText;

    public int currentRound = 1;

    private int targetsRemaining;
    private int score = 0;

    // Timer
    private float gameTimer = 0f;
    private bool timerRunning = true;

    // Game ending
    private bool trainingOver = false;
    private bool endingTraining = false;

    void Start()
    {
        gameTimer = 0f;
        timerRunning = true;
        trainingOver = false;
        endingTraining = false;
        score = 0;
        currentRound = 1;

        // Hide results screen when game starts
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);
        }

        UpdateUI();
        StartRound();
    }

    void Update()
    {
        if (timerRunning)
        {
            gameTimer += Time.deltaTime;
        }
    }

    void StartRound()
    {
        if (trainingOver)
        {
            return;
        }

        Debug.Log("ROUND " + currentRound);

        // Reload back to 6 bullets
        shooting.Reload();

        // Number of targets equals round number
        targetsRemaining = currentRound;

        UpdateUI();

        // Make list of available spawn points
        List<int> availableSpawns = new List<int>();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            availableSpawns.Add(i);
        }

        // Spawn targets
        for (int i = 0; i < currentRound; i++)
        {
            int randomIndex =
                Random.Range(0, availableSpawns.Count);

            int spawnNumber =
                availableSpawns[randomIndex];

            Transform spawn =
                spawnPoints[spawnNumber];

            GameObject cowboy = Instantiate(
                cowboyPrefab,
                spawn.position,
                Quaternion.identity
            );

            // Match size to spawn point
            cowboy.transform.localScale =
                spawn.localScale;

            // Prevent two targets using same spawn
            availableSpawns.RemoveAt(randomIndex);
        }
    }

    public void TargetDestroyed(int points)
    {
        if (trainingOver)
        {
            return;
        }

        // Add score
        score += points;

        // One less target
        targetsRemaining--;

        UpdateUI();

        Debug.Log(
            "Targets Remaining: " + targetsRemaining +
            " | Score: " + score
        );

        // Round finished
        if (targetsRemaining <= 0)
        {
            endingTraining = false;

            CancelInvoke(nameof(EndTrainingFailed));

            // Go to next round
            if (currentRound < 6)
            {
                currentRound++;

                UpdateUI();

                Invoke(nameof(StartRound), 1.5f);
            }
            else
            {
                // Finished Round 6
                EndTrainingComplete();
            }
        }
    }

    public void OutOfBullets()
    {
        // If final bullet killed final target,
        // don't fail the player
        if (targetsRemaining <= 0)
        {
            return;
        }

        if (trainingOver || endingTraining)
        {
            return;
        }

        endingTraining = true;

        Debug.Log(
            "OUT OF BULLETS! Training ending in 3 seconds..."
        );

        Invoke(nameof(EndTrainingFailed), 3f);
    }

    void EndTrainingFailed()
    {
        if (trainingOver)
        {
            return;
        }

        trainingOver = true;
        timerRunning = false;

        ShowResults("OUT OF AMMO");
    }

    void EndTrainingComplete()
    {
        trainingOver = true;
        timerRunning = false;

        ShowResults("TRAINING COMPLETE");
    }

    void ShowResults(string title)
    {
        // Hide normal gameplay UI
        if (roundText != null)
        {
            roundText.gameObject.SetActive(false);
        }

        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(false);
        }

        // Show results panel
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);
        }

        // Display final results
        if (resultsText != null)
        {
            resultsText.text =
                title +
                "\n\nSCORE: " + score +
                "\nTIME: " + gameTimer.ToString("F2") + " SEC" +
                "\nROUND: " + currentRound + " / 6";
        }

        Debug.Log(
            title +
            " | Score: " + score +
            " | Time: " + gameTimer.ToString("F2") +
            " | Round: " + currentRound + "/6"
        );
    }

    void UpdateUI()
    {
        if (roundText != null)
        {
            roundText.text =
                "ROUND " + currentRound + " / 6";
        }

        if (scoreText != null)
        {
            scoreText.text =
                "SCORE: " + score;
        }
    }

    public bool IsTrainingOver()
    {
        return trainingOver;
    }

    public float GetGameTime()
    {
        return gameTimer;
    }

    public int GetScore()
    {
        return score;
    }

    // RETRY BUTTON
    public void RetryGame()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }
}