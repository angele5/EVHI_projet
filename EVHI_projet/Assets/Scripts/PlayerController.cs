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

    //adaptation
    public int tps_adapt = 10;
    private float tps_since_adapt =0f;
    private int obstacleCount =0;
    private int len_time_buffer_paddle = 10;
    private List<double> time_buffer_paddle = new List<double>();
    private bool just_paddled = false;
    private float optimal_time;
    private bool isFatigueCoroutineRunning =false;

    //fin du jeu
    private float gameTime = 0f;  // Temps écoulé
    public float maxGameTime = 120f; 
    public GameObject gameOverScreen;
    private bool ended = false;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI EndscoreText;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        playerModel.score = 0;
        playerModel.dodgeLevel = 0f;
        gameTime = 0f;
        previousPosition = rb.position;
        tps_since_adapt =0f; 
        obstacleCount =0;
        resistance = 0.98f;
        Time.timeScale = 1f;
        gameOverScreen.SetActive(false);
        ended=false;
        optimal_time = time_before_paddle*len_time_buffer_paddle;
    }

    void Update()
    {
        if(!ended){

            if (playerModel.fatigue && !isFatigueCoroutineRunning)
            {
                StartCoroutine(HandleFatigue());
                isFatigueCoroutineRunning = true;
            }
            handle_input();
            gameTime += Time.deltaTime;
            tps_since_adapt += Time.deltaTime;

            float timeLeft = Mathf.Max(maxGameTime - gameTime, 0);
            int minutes = Mathf.FloorToInt(timeLeft / 60);
            int seconds = Mathf.FloorToInt(timeLeft % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            
            if (gameTime >= maxGameTime)
            {
                EndGame();
            }

        }
        
    }

    void EndGame()
    {
        Debug.Log("Fin de la partie !");
        ended = true;
        Time.timeScale = 0f;
        EndscoreText.text = "Ton score est "+Mathf.CeilToInt(playerModel.score);
        gameOverScreen.SetActive(true); // Affiche l'écran de fin

    }

    private void FixedUpdate()
    {
        apply_resistance();
        update_score();
        if (tps_since_adapt > tps_adapt){
            update_difficulty();
            tps_since_adapt =0f;
            obstacleCount = 0;
        }
        
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
            // Debug.Log("Paddle right " + time_since_right_true);
            time_since_right_true = 0;
            can_paddle_left = true;
            can_paddle_right = false;

            // Recuperer le temps machine de ce coup
            time_buffer_paddle.Add(Time.time);
            just_paddled = true;
        }

        // Coup de pagaie à gauche
        if (time_since_left_true > time_before_paddle && can_paddle_left)
        {
            if (!is_flip)
            {
                Flip(true);
            }
            paddle(Vector2.right);
            // Debug.Log("Paddle left " + time_since_left_true);
            time_since_left_true = 0;
            can_paddle_left = false;
            can_paddle_right = true;

            // Recuperer le temps machine de ce coup
            time_buffer_paddle.Add(Time.time);
            just_paddled = true;
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

        // MAJ coordination
        if (just_paddled){
            just_paddled = false;
            // Si il y a au moins 10 coups de pagaie dans le buffer
            if (time_buffer_paddle.Count < 10) return;

            // Si le buffer contient trop de coups de pagaie on retire le plus ancien
            if (time_buffer_paddle.Count > 10) time_buffer_paddle.RemoveAt(0);

            double oldestTime = time_buffer_paddle[0];
            double newestTime = time_buffer_paddle[^1];
            double timeDiff = newestTime - oldestTime;
            double ratio = optimal_time / timeDiff;
            playerModel.coordinationLevel = Mathf.Min(Mathf.Max((float)ratio, 0.0f), 1.0f);
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

    void update_difficulty(){
        if (obstacleCount > 1){
            playerModel.dodgeLevel = Mathf.Max(playerModel.dodgeLevel - obstacleCount, playerModel.dodgeLevel - 4,0f); // Le dodgeLevel diminue
        }
        else{
            playerModel.dodgeLevel = Mathf.Min(playerModel.dodgeLevel + 3, 20);
        }
        
        resistance = Mathf.Max(Mathf.Min(resistance - playerModel.dodgeLevel*0.00002f - playerModel.coordinationLevel * 0.001f, 1.5f),0.95f); //
        Debug.Log("resistance"+resistance);
        Debug.Log("dodgeLevel"+playerModel.dodgeLevel);
        Debug.Log("coord"+playerModel.coordinationLevel);
    }

    IEnumerator HandleFatigue()
    {
        float initialCoordination = playerModel.coordinationLevel;
        float initialDodge = playerModel.dodgeLevel;
        float initialResistance = resistance;

        playerModel.dodgeLevel = 0; // Évite toute esquive pendant la fatigue
        playerModel.coordinationLevel =0;
        resistance = 0.98f;

        yield return new WaitForSeconds(20f); // Attente de 20 secondes

        playerModel.fatigue = false;
        playerModel.coordinationLevel = initialCoordination / 2f;
        playerModel.dodgeLevel = initialDodge / 2f;
        resistance = (resistance + initialResistance)/2f;
        isFatigueCoroutineRunning = false;
    }
    void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.collider != null) //si collision
        {
            playerModel.score -= 15;
            isTouchingObstacle = true;
            obstacleCount +=1;
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
