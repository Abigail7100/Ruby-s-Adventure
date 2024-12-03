using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //==========Tracking Robots===========
    public int score;
    public GameObject gameOverText;
    public GameObject gameWonText;
    // Creating boolean for game over
    public bool gameOver;
    //new audio changes
    public AudioClip youLose;
    public AudioClip youWin;
    //=========Sounds=========
    AudioSource audioSource;
    // Variables related to player character movement
    public InputAction MoveAction;
    Rigidbody2D rigidbody2d;
    Vector2 move;
    public float speed = 3.0f;

    // Variables related to the health system
    public int maxHealth = 5;
    int currentHealth;
    public int health { get { return currentHealth; } }
    public ParticleSystem hitParticle;
    public ParticleSystem healthPickupParticles;

    // Variables related to temporary invincibility
    public float timeInvincible = 2.0f;
    bool isInvincible;
    float damageCooldown;
    // Variables related to animation
    Animator animator;
    Vector2 moveDirection = new Vector2(1, 0);


    // Variables related to projectiles
    public GameObject projectilePrefab;
    public InputAction Projectile;


    // Start is called before the first frame update
    void Start()
    {
        MoveAction.Enable();
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();


        currentHealth = maxHealth;

        //game starts as normal
        gameOver = false;
        //========Audio=======
        audioSource = GetComponent<AudioSource>();
    }
    // Update is called once per frame
    void Update()
    {
        move = MoveAction.ReadValue<Vector2>();


        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            moveDirection.Set(move.x, move.y);
            moveDirection.Normalize();
        }


        animator.SetFloat("Move X", moveDirection.x);
        animator.SetFloat("Move Y", moveDirection.y);
        animator.SetFloat("Speed", move.magnitude);


        if (isInvincible)
        {
            damageCooldown -= Time.deltaTime;
            if (damageCooldown < 0)
            {
                isInvincible = false;
            }
        }


        // Detect input for projectile launch
        if (Input.GetKeyDown(KeyCode.C))
        {
            Launch();
        }
        // Detect input for NPC interaction
        if (Input.GetKeyDown(KeyCode.X))
        {
            RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, moveDirection, 1.5f, LayerMask.GetMask("NPC"));
            if (hit.collider != null)
            {
                NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
            }
        }

    //=========Game Over==========
    if(currentHealth == 0)
        {
            speed = 0;
            gameOverText.SetActive(true);
            //boolean
            gameOver = true;
            //audio
            audioSource.PlayOneShot(youLose);
        }
    //============Game Won=========
    //Score is amount of robots in current scene
    if(score==3)
        {
            speed = 0;
            gameWonText.SetActive(true);
            //boolean
            gameOver = true;
            //audio
            audioSource.PlayOneShot(youWin);
        }
    //checking if R is pressed
    if (Input.GetKey(KeyCode.R))
        {
            //boolean
            if(gameOver==true)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

    }


    // FixedUpdate has the same call rate as the physics system
    void FixedUpdate()

    {
        Vector2 position = (Vector2)rigidbody2d.position + move * speed * Time.deltaTime;
        rigidbody2d.MovePosition(position);
    }


    public void ChangeHealth(int amount)
    {
        if (amount < 0)
        {
            if (isInvincible)
            {
                return;
            }
            isInvincible = true;
            damageCooldown = timeInvincible;

            animator.SetTrigger("Hit");
            //damage particle system
            Instantiate(hitParticle, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }


        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UIHandler.instance.SetHealthValue(currentHealth / (float)maxHealth);
        //Health pickup particle system
        Instantiate(healthPickupParticles, transform.position + Vector3.up * 0.5f, Quaternion.identity);
    }
    //========Speed Potion=========
    public void ChangeSpeed(int speedAmount)
    {
        speed = speed + speedAmount;
    }
    //========Score============
    public void ChangeScore(int scoreAmount)
    {
        score = score + scoreAmount;
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    void Launch()
    {
        GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);
        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.Launch(moveDirection, 300);


        animator.SetTrigger("Launch");
    }

}