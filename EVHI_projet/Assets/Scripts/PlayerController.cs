using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public PlayerModel playerModel;
    public float acceleration = 3f; // Force du coup de pagaie
    public float max_speed = 10f; // Vitesse maximale
    public float rotationForce = 0.4f; // Force de rotation par coup de pagaie
    public float resistance = 0.98f; // Résistance de l'eau, réduit la vitesse progressivement
    public float time_before_paddle = 0.5f; // Temps avant de pouvoir donner un autre
    public float time_redo_same = 0.3f; // Temps avant de pouvoir donner un autre coup de pagaie dans la même direction
    public bool can_paddle_left = true;
    public bool can_paddle_right = true;

    public float knockbackForce =2f;  //Recule collision

    public BarScript leftBarScript;
    public BarScript rightBarScript;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private bool is_flip = false;
    private float time_since_left_true = 0f;
    private float time_since_right_true = 0f;
    private float time_since_left_false = 0f;
    private float time_since_right_false = 0f;

    private bool isTouchingObstacle = false;
    private Vector2 obstaclePosition;

    // score
    private Vector2 previousPosition;
    public TextMeshProUGUI scoreText;



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        playerModel.score = 0;
        previousPosition = rb.position;

    }

    void Update()
    {
        handle_input();
    }

    private void FixedUpdate()
    {
        apply_resistance();
        update_score();
    }

    void handle_input()
    {
        // Gestion des inputs
        // Si on appuie sur la touche droite
        if (Input.GetKey(KeyCode.RightArrow) || playerModel.isRight)
        {
            time_since_right_true += Time.deltaTime;
            time_since_left_true = 0;
            time_since_right_false = 0;
        }
        else
        {
            time_since_right_true = 0;
            time_since_right_false += Time.deltaTime;
        }

        // Si on appuie sur la touche gauche
        if (Input.GetKey(KeyCode.LeftArrow) || playerModel.isLeft)
        {
            time_since_left_true += Time.deltaTime;
            time_since_right_true = 0;
            time_since_left_false = 0;
        }
        else
        {
            time_since_left_true = 0;
            time_since_left_false += Time.deltaTime;
        }

        // Si ca fait assez longtemps qu'on a pas appuyé sur une touche, on peut donner un autre coup de pagaie
        if (time_since_left_false > time_redo_same)
        {
            can_paddle_left = true;
        }

        if (time_since_right_false > time_redo_same)
        {
            can_paddle_right = true;
        }


        // Coup de pagaie droite
        if (time_since_right_true > time_before_paddle && can_paddle_right)
        {
            if (is_flip)
            {
                Flip(false);
            }
            paddle(Vector2.left);
            Debug.Log("Paddle right " + time_since_right_true);
            time_since_right_true = 0;
            can_paddle_left = true;
            can_paddle_right = false;
        }

        // Coup de pagaie à gauche
        if (time_since_left_true > time_before_paddle && can_paddle_left)
        {
            if (!is_flip)
            {
                Flip(true);
            }
            paddle(Vector2.right);
            Debug.Log("Paddle left " + time_since_left_true);
            time_since_left_true = 0;
            can_paddle_left = false;
            can_paddle_right = true;
        }

        // Met à jour les barres de progression
        if (can_paddle_left)
        {
            leftBarScript.UpdateProgress(time_since_left_true, time_before_paddle, true);
        }else
        {
            leftBarScript.UpdateProgress(time_redo_same-time_since_left_false, time_redo_same, false);
        }

        if (can_paddle_right)
        {
            rightBarScript.UpdateProgress(time_since_right_true, time_before_paddle, true);
        }else
        {
            rightBarScript.UpdateProgress(time_redo_same-time_since_right_false, time_redo_same, false);
        }

    }

    void Flip(bool flip)
    {
        sr.flipX = flip;
        is_flip = flip;
    }

    void paddle(Vector2 direction)
    {
        float rotation;

        if (isTouchingObstacle && Vector2.Dot(transform.up, Vector2.up) > 0.7f) // Kayak vers le haut
        {
            Vector2 obstacleDir = ((Vector2)transform.position - obstaclePosition).normalized;

            // Détermine si le coup de pagaie est du côté de l'obstacle
            if ((direction == Vector2.left && obstacleDir.x < 0) || (direction == Vector2.right && obstacleDir.x > 0))
            {
                // Pousse légèrement vers l'opposé de l'obstacle
                rb.AddForce(obstacleDir * knockbackForce, ForceMode2D.Impulse);
                return; // On empêche le mouvement normal pour ce coup
            }
        }
        // Applique avance
        if (rb.velocity.magnitude < max_speed)
        {
            rb.AddForce(transform.up * acceleration, ForceMode2D.Impulse);
        }

        // Applique rotation
        if (direction == Vector2.right){
            rotation =  -rotationForce;
        }
        else{
            rotation = rotationForce;
        }
        rb.AddTorque(rotation, ForceMode2D.Impulse);
    }

    void apply_resistance()
    {
        // reduit vitesse lineaire et angulaire pour simuler la résistance de l'eau
        rb.velocity *= resistance;
        rb.angularVelocity *= resistance;
    }

    void update_score()
    {
        float distanceTravelled = Vector2.Distance(previousPosition, rb.position);
        float speedFactor = rb.velocity.magnitude; // Plus la vitesse est élevée, plus le score augmente rapidement
        
        float scoreIncrement = distanceTravelled * speedFactor * 5f;

        playerModel.score += Mathf.RoundToInt(scoreIncrement);
        previousPosition = rb.position; // Mettre à jour la position précédente

        scoreText.text = ""+Mathf.RoundToInt(playerModel.score);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.collider != null) //si collision
        {
            playerModel.score -= 15;
            isTouchingObstacle = true;
            obstaclePosition = collision.contacts[0].point; 

            Vector2 river_center_dir = (new Vector2(0, 10) - (Vector2)transform.position).normalized;

            // Vérifier si le kayak est à l'envers (tête en bas)
            float angleWithUp = Vector2.SignedAngle(transform.up, Vector2.up);
            
            if (Mathf.Abs(angleWithUp) > 120f) // Si le kayak est quasiment à l'envers
            {
                // Réinitialisation du kayak au centre avec la tête en haut
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                transform.rotation = Quaternion.Euler(0, 0, 0); // Aligné vers le haut
                return; // Sortir de la fonction pour éviter d'appliquer d'autres forces
            }


            Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
            rb.velocity = Vector2.zero;
            // Applique une force de recul au Rigidbody2D du joueur
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        isTouchingObstacle = false; // On n'est plus en contact avec l'obstacle
    }

}
