using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverGenerator : MonoBehaviour
{
    // Références aux composants
    public LineRenderer leftLineRenderer;  // Pour la berge gauche
    public LineRenderer rightLineRenderer; // Pour la berge droite
    public EdgeCollider2D leftEdgeCollider; // Collider de la berge gauche
    public EdgeCollider2D rightEdgeCollider; // Collider de la berge droite
    public MeshFilter riverMeshFilter; // MeshFilter pour la surface de la rivière
    public GameObject player; // Référence au GameObject du joueur

    // Paramètres de génération de la rivière
    public float segmentLength = 2f;  // Longueur entre chaque segment

    public float segemntWidth = 6f;
    public float curveIntensity = 5f;  // Intensité de la courbure
    public int initialSegments = 10;  // Nombre de segments au départ
    public float distanceBeforeDestroy = 20f; // Distance avant qu'une section de rivière soit détruite
    public float distanceBeforeGenerate = 10f; // Distance avant de générer une nouvelle section de rivière

    private List<Vector2> leftPoints = new List<Vector2>(); // Liste des points de la berge gauche
    private List<Vector2> rightPoints = new List<Vector2>(); // Liste des points de la berge droite
    private Mesh riverMesh; // Le maillage pour la surface de la rivière
    private float lastPlayerYPos; // Dernière position Y du joueur

    void Start()
    {
        lastPlayerYPos = player.transform.position.y; // Sauvegarde la position Y du joueur au démarrage
        InitializeRiver(); // Initialise la rivière avec quelques segments
        GenerateRiverMesh(); // Crée la surface de la rivière
    }

    void Update()
    {
        // La rivière ne se déplace que lorsque le joueur se déplace
        if (player.transform.position.y > lastPlayerYPos) // Si le joueur se déplace vers le bas (ou avance)
        {
           // ExtendRiver(); // Ajoute de nouveaux segments
            RemoveOldSections(); // Supprime les anciens segments
            GenerateRiverMesh(); // Met à jour le maillage de la rivière
        }

        // Sauvegarde la nouvelle position Y du joueur
        lastPlayerYPos = player.transform.position.y;
    }

    void InitializeRiver()
{
    float y = player.transform.position.y - 10; // Position de départ au niveau du joueur

    for (int i = 0; i < initialSegments; i++)
    {
        // Calcule un offset symétrique pour les bords gauche et droit
        float xOffset = Mathf.Sin(i * 0.5f) * curveIntensity;
        Debug.Log("xoffset "+xOffset);


        // Définit les points gauche et droit symétriques autour de x = 0
        leftPoints.Add(new Vector2(-segemntWidth + xOffset, y));
        rightPoints.Add(new Vector2(xOffset, y));

        y += segmentLength; // Incrément de la position verticale
    }

    // Met à jour les LineRenderers et EdgeColliders
    UpdateLineRenderers();
    UpdateEdgeColliders();
}


    void ExtendRiver()
    {
        float y = leftPoints[leftPoints.Count - 1].y + segmentLength;
        float xOffset = Mathf.Sin(y * 0.1f) * curveIntensity;

        // Ajout de points symétriques autour de x = 0
        leftPoints.Add(new Vector2(-segemntWidth + xOffset, y));
        rightPoints.Add(new Vector2(xOffset, y));

        // Met à jour les LineRenderers et EdgeColliders
        UpdateLineRenderers();
        UpdateEdgeColliders();
    }


    void RemoveOldSections()
    {
        // Supprime les sections qui sont trop éloignées du joueur (hors de la vue de la caméra)
        if (leftPoints.Count > 0 && leftPoints[0].y < player.transform.position.y - distanceBeforeDestroy)
        {
            leftPoints.RemoveAt(0); // Retirer le premier point (le plus ancien)
            rightPoints.RemoveAt(0); // Retirer le premier point (le plus ancien)

            // Met à jour les LineRenderers et EdgeColliders
            UpdateLineRenderers();
            UpdateEdgeColliders();
        }

        // Générer de nouveaux segments si nécessaire
        if (leftPoints[leftPoints.Count - 1].y < player.transform.position.y + distanceBeforeGenerate)
        {
            ExtendRiver();
        }
    }

    void UpdateLineRenderers()
    {
        // Mise à jour du LineRenderer pour la berge gauche
        leftLineRenderer.positionCount = leftPoints.Count;
        for (int i = 0; i < leftPoints.Count; i++)
        {
            leftLineRenderer.SetPosition(i, leftPoints[i]);
        }

        // Mise à jour du LineRenderer pour la berge droite
        rightLineRenderer.positionCount = rightPoints.Count;
        for (int i = 0; i < rightPoints.Count; i++)
        {
            rightLineRenderer.SetPosition(i, rightPoints[i]);
        }
    }

    void UpdateEdgeColliders()
    {
        // Mise à jour des colliders avec les points des berges
        leftEdgeCollider.points = leftPoints.ToArray();
        rightEdgeCollider.points = rightPoints.ToArray();
    }

    void GenerateRiverMesh()
    {
        // Si le maillage n'existe pas, créez-le
        if (riverMesh == null)
        {
            riverMesh = new Mesh();
            riverMeshFilter.mesh = riverMesh;
        }

        // Efface le maillage actuel pour éviter les problèmes de superposition
        riverMesh.Clear();

        // Crée une liste de vertices (points) pour la surface de la rivière
        List<Vector3> vertices = new List<Vector3>();
        for (int i = 0; i < leftPoints.Count; i++)
        {
            vertices.Add(leftPoints[i]);  // Ajoute un point pour la berge gauche
            vertices.Add(rightPoints[i]); // Ajoute un point pour la berge droite
        }

        // Crée une liste d'indices pour définir les triangles (faces) du maillage
        List<int> triangles = new List<int>();
        for (int i = 0; i < leftPoints.Count - 1; i++)
        {
            int startIndex = i * 2;

            // Crée des triangles pour la surface entre les deux bords
            triangles.Add(startIndex);
            triangles.Add(startIndex + 2);
            triangles.Add(startIndex + 1);

            triangles.Add(startIndex + 1);
            triangles.Add(startIndex + 2);
            triangles.Add(startIndex + 3);
        }

        // Crée des UVs (optionnel, pour appliquer des textures)
        List<Vector2> uvs = new List<Vector2>();
        for (int i = 0; i < vertices.Count; i++)
        {
            uvs.Add(new Vector2(vertices[i].x, vertices[i].y)); // Définir les coordonnées UV
        }

        // Applique les données au maillage
        riverMesh.vertices = vertices.ToArray();
        riverMesh.triangles = triangles.ToArray();
        riverMesh.uv = uvs.ToArray();

        // Recalcule les normales du maillage pour une meilleure gestion des ombres
        riverMesh.RecalculateNormals();
    }
}
