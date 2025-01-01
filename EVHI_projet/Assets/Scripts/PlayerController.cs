using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float acceleration = 3f; // Force du coup de pagaie
    public float max_speed = 10f; // Vitesse maximale
    public float rotationForce = 0.4f; // Force de rotation par coup de pagaie
    public float resistance = 0.98f; // Résistance de l'eau, réduit la vitesse progressivement

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
        // Coup de pagaie droite
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            paddle(Vector2.left);
        }

        // Coup de pagaie à gauche
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            paddle(Vector2.right);
        }
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
