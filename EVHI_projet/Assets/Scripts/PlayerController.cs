using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float acceleration = 3f; // Force du coup de pagaie
    public float max_speed = 10f; // Vitesse maximale
    public float rotationForce = 0.4f; // Force de rotation par coup de pagaie
    public float resistance = 0.98f; // Résistance de l'eau, réduit la vitesse progressivement
    public float time_before_paddle = 0.5f; // Temps avant de pouvoir donner un autre
    public float time_redo_same = 0.3f; // Temps avant de pouvoir donner un autre coup de pagaie dans la même direction
    public bool can_paddle_left = true;
    public bool can_paddle_right = true;

    public BarScript leftBarScript;
    public BarScript rightBarScript;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private bool is_flip = false;
    private float time_since_left_true = 0f;
    private float time_since_right_true = 0f;
    private float time_since_left_false = 0f;
    private float time_since_right_false = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

    }

    void Update()
    {
        handle_input();
    }

    private void FixedUpdate()
    {
        apply_resistance();
    }

    void handle_input()
    {
        // Gestion des inputs
        // Si on appuie sur la touche droite
        if (Input.GetKey(KeyCode.RightArrow))
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
        if (Input.GetKey(KeyCode.LeftArrow))
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
}
