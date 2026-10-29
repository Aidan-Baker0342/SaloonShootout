using UnityEngine;

public class Shooting : MonoBehaviour
{
    public int maxBullets = 6;
    private int bulletsRemaining;

    // Ammo UI
    public GameObject[] bulletIcons;

    // Sound
    public AudioClip gunshotSound;
    public AudioClip hitSound;
    public AudioClip emptySound;

    private AudioSource audioSource;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();

        audioSource = GetComponent<AudioSource>();

        Reload();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // Don't allow shooting after training ends
        if (gameManager != null && gameManager.IsTrainingOver())
        {
            return;
        }

        // Gun is empty
        if (bulletsRemaining <= 0)
        {
            Debug.Log("EMPTY!");

            if (audioSource != null && emptySound != null)
            {
                audioSource.PlayOneShot(emptySound);
            }

            return;
        }

        // Use one bullet
        bulletsRemaining--;

        UpdateAmmoUI();

        // Play gunshot
        if (audioSource != null && gunshotSound != null)
        {
            audioSource.PlayOneShot(gunshotSound);
        }

        Vector2 mousePosition =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Collider2D[] hits =
            Physics2D.OverlapPointAll(mousePosition);

        TargetHit bestTarget = null;

        // Find highest-value target hitbox
        foreach (Collider2D hit in hits)
        {
            TargetHit target =
                hit.GetComponent<TargetHit>();

            if (target != null)
            {
                // Headshot takes priority over body
                if (bestTarget == null ||
                    target.points > bestTarget.points)
                {
                    bestTarget = target;
                }
            }
        }

        // Target was hit
        if (bestTarget != null)
        {
            Debug.Log(
                "Hit! +" + bestTarget.points +
                " points | Bullets: " +
                bulletsRemaining
            );

            // Play target hit sound
            if (audioSource != null && hitSound != null)
            {
                audioSource.PlayOneShot(hitSound);
            }

            if (gameManager != null)
            {
                gameManager.TargetDestroyed(
                    bestTarget.points
                );
            }

            Destroy(
                bestTarget.transform.parent.gameObject
            );
        }
        else
        {
            Debug.Log(
                "Miss! | Bullets: " +
                bulletsRemaining
            );
        }

        // Check if gun is empty
        if (bulletsRemaining <= 0)
        {
            if (gameManager != null)
            {
                gameManager.OutOfBullets();
            }
        }
    }

    public void Reload()
    {
        bulletsRemaining = maxBullets;

        UpdateAmmoUI();

        Debug.Log(
            "Reloaded! Bullets: " +
            bulletsRemaining
        );
    }

    void UpdateAmmoUI()
    {
        if (bulletIcons == null)
        {
            return;
        }

        for (int i = 0; i < bulletIcons.Length; i++)
        {
            if (bulletIcons[i] != null)
            {
                bulletIcons[i].SetActive(
                    i < bulletsRemaining
                );
            }
        }
    }

    public int GetBulletsRemaining()
    {
        return bulletsRemaining;
    }
}