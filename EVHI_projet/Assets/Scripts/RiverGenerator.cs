using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverGenerator : MonoBehaviour
{
    public GameObject player;
    public GameObject riverPrefab; 
    public GameObject edgePrefab;
    public float length = 10f; // Longueur d'un segment
    public float river_width = 8f; // Largeur de la rivière
    public float edge_width = 2.5f; // Largeur de la berge
    public int segments_ahead = 5; // Nombre de segments à générer devant le joueur
    public int segments_behind = 2; // Nombre de segments à garder derrière le joueur

    private LinkedList<GameObject> river_segments = new LinkedList<GameObject>();
    private LinkedList<GameObject> left_edges = new LinkedList<GameObject>();
    private LinkedList<GameObject> right_edges = new LinkedList<GameObject>();

    private float lastGeneratedZ = 0f; // Position du dernier segment généré

    void Start()
    {
        // Génère les segments initiaux
        for (int i = -segments_behind; i <= segments_ahead; i++)
        {
            generate_segment(i *length);
        }
    }

    void Update()
    {
        float playerZ = player.transform.position.y;

        // Génère les segments à venir
        while (playerZ + segments_ahead * length > lastGeneratedZ)
        {
            generate_segment(lastGeneratedZ + length);
            lastGeneratedZ += length;
        }

        // Supprime les segments derrière le joueur
        while (river_segments.First != null && playerZ - segments_behind * length > river_segments.First.Value.transform.position.y + length)
        {
            destroy_segment();
        }
    }

    void generate_segment(float zPosition)
    {
        // Génère un segment de rivière
        GameObject riverSegment = Instantiate(riverPrefab);
        riverSegment.transform.position = new Vector3(0, zPosition, 0);
        riverSegment.transform.localScale = new Vector3(river_width, length, 1);
        river_segments.AddLast(riverSegment);

        // Génère une berge à gauche
        GameObject leftEdge = Instantiate(edgePrefab);
        leftEdge.transform.position = new Vector3(-river_width / 2 - edge_width / 2, zPosition, 0);
        leftEdge.transform.localScale = new Vector3(edge_width, length, 1);
        left_edges.AddLast(leftEdge);

        // Génère une berge à droite
        GameObject rightEdge = Instantiate(edgePrefab);
        rightEdge.transform.position = new Vector3(river_width / 2 + edge_width / 2, zPosition, 0);
        rightEdge.transform.localScale = new Vector3(edge_width, length, 1);
        right_edges.AddLast(rightEdge);
    }

    void destroy_segment()
    {
        // Supprime un segment de rivière
        if (river_segments.First != null)
        {
            Destroy(river_segments.First.Value);
            river_segments.RemoveFirst();
        }

        // Supprime une berge à gauche
        if (left_edges.First != null)
        {
            Destroy(left_edges.First.Value);
            left_edges.RemoveFirst();
        }

        // Supprime une berge à droite
        if (right_edges.First != null)
        {
            Destroy(right_edges.First.Value);
            right_edges.RemoveFirst();
        }
    }
}
