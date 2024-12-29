using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 5f; // Vitesse de déplacement
    public float speed_rotation = 100f; // Vitesse de rotation
    public float acceleration = 5f;  // vitesse du coup de pagaie
    public float max_speed = 10f;     // Vitesse maximale du kayak
    public float deceleration = 2f;  // lente réduction de la vitesse / courant
    private Rigidbody2D rb;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

     float horizontalInput = Input.GetAxis("Horizontal");

    // avancer
    transform.Translate(Vector3.up * speed * Time.deltaTime);

    // Rotation progressive
    float targetRotation = horizontalInput * speed_rotation;
    transform.Rotate(Vector3.forward, Mathf.Lerp(0, targetRotation, Time.deltaTime));
   
    }
}
