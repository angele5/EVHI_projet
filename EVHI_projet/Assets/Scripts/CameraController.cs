using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public float speed_trans = 0.125f; // lissage
    public float decalageZ = -10f; // Décalage Z pour éloigner la caméra de la scène

    void LateUpdate()
    {
        if (player != null)
        {
            float new_y = player.transform.position.y;

            Vector3 new_pos = new Vector3(transform.position.x, new_y, decalageZ);

            Vector3 pos_lisse = Vector3.Lerp(transform.position, new_pos, speed_trans);

            transform.position = pos_lisse;
        }
    }
}
