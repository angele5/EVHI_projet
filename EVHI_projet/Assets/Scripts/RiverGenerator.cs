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

    // gestion obstacles
    public GameObject obstaclePrefab;
    public int maxObstacles = 5;
    public float proba0bs = 0.3f;
    private float obstacleMinOffset;
    private float obstacleMaxOffset;
    List<GameObject> obstacles = new List<GameObject>();

    //fleurs
    public GameObject flowerPrefab;  // Prefab pour la fleur
    public float flowerOffset = 1.5f; // Distance fleurs apparaissent
    public int maxFlowersPerSegment = 5; 
    List<GameObject> flowers = new List<GameObject>(); // Liste fleurs


    // Paramètres de génération de la rivière
    public float segmentLength = 2f;  // Longueur entre segments
    public float segemntWidth = 6f;
    public float curveIntensity = 5f;  // intensite courbure
    public int initialSegments = 10;  // Nombre de segments init
    public float distanceBeforeDestroy = 20f; // Distance avant destruction
    public float distanceBeforeGenerate = 10f; // Distance avant generation
    private List<Vector2> leftPoints = new List<Vector2>(); // Liste points rive gauche
    private List<Vector2> rightPoints = new List<Vector2>(); // Liste points rive droite
    private Mesh riverMesh; // surface de la riviere
    private float lastPlayerYPos; // derniere position du joueur
    public PlayerModel playerModel;

    void Start()
    {
        lastPlayerYPos = player.transform.position.y; // Save position du joueur au start
        obstacleMinOffset = - segemntWidth /2 + 0.5f;
        obstacleMaxOffset =  segemntWidth /2 - 0.5f;
        maxObstacles = 0;
        proba0bs = 0.3f;

        InitializeRiver(); // init riviere
        GenerateRiverMesh(); // creation surface riviere

    }

    void Update()
    {
        if (player.transform.position.y > lastPlayerYPos){ // Si joueur  avance
        
            UpdateDiff();
            RemoveOldSections(); // Supp anciens segments
            GenerateRiverMesh(); // MAJ riviere
        }

        lastPlayerYPos = player.transform.position.y;
    }

    void UpdateDiff(){

        if (playerModel.fatigue){
            maxObstacles = 0;
            return;
        }
        float dodgeFactor = Mathf.Clamp01(playerModel.dodgeLevel / 20f); // Normalisation
        float coordFactor = playerModel.coordinationLevel;

        // Nombre maximal augmente avec la coord et dodge
        maxObstacles = Mathf.RoundToInt(25 * coordFactor * dodgeFactor); // Entre 0 et 20

        // Proba apparition des obstacles
        proba0bs = Mathf.Lerp(0.3f, 0.9f, coordFactor * dodgeFactor);
    }

    void InitializeRiver()
    {
        float y = player.transform.position.y - 10;

        for (int i = 0; i < initialSegments; i++){
            float xOffset = Mathf.Sin(i * 0.1f) * curveIntensity;
            leftPoints.Add(new Vector2(-segemntWidth + xOffset, y));
            rightPoints.Add(new Vector2(xOffset, y));

            y += segmentLength;
        }

        // MaJ LineRenderers et EdgeColliders
        UpdateLineRenderers();
        UpdateEdgeColliders();
    }

    void ExtendRiver()
    {
        float y = leftPoints[leftPoints.Count - 1].y + segmentLength;
        float xOffset = Mathf.Sin(y * 0.1f) * curveIntensity;

        leftPoints.Add(new Vector2(-segemntWidth + xOffset, y));
        rightPoints.Add(new Vector2(xOffset, y));

        if (obstacles.Count < maxObstacles && Random.value < proba0bs){
            CreateObstacle(y);
        }

        CreateFlowers(y);

        UpdateLineRenderers();
        UpdateEdgeColliders();
    }

    void CreateObstacle(float yPosition)
    {
        int index = leftPoints.FindIndex(p => Mathf.Approximately(p.y, yPosition));
        if (index == -1) return; // si index pas existe

        // positions rives niveau creation
        float leftX = leftPoints[index].x;
        float rightX = rightPoints[index].x;

        //  X entre rives gauche et droite
        float xPosition = Random.Range(leftX + 3f, rightX +2f);        
            
        GameObject newObstacle = Instantiate(obstaclePrefab, new Vector3(xPosition, yPosition, -2), Quaternion.identity);
        
        obstacles.Add(newObstacle); // liste obstacles
    }
    void CreateFlowers(float yPosition)
    {
        // Choix random nombre de fleurs à cree
        float leftX = leftPoints[leftPoints.Count - 1].x;
        float rightX = rightPoints[rightPoints.Count - 1].x;

        if (Random.value < 0.5f)
        {
            float flowerXPosition = Random.Range(leftX +0.5f, leftX +1.5f); // ext de la berge gauche
            GameObject flower = Instantiate(flowerPrefab, new Vector3(flowerXPosition, yPosition, -2), Quaternion.identity);
            flowers.Add(flower);
        }

        if (Random.value < 0.5f)
        {
            float flowerXPosition = Random.Range(rightX + 3.5f, rightX + 4.5f); //ext de la berge droite
            GameObject flower = Instantiate(flowerPrefab, new Vector3(flowerXPosition, yPosition, -2), Quaternion.identity);
            flowers.Add(flower);
        }
    }

    void RemoveOldSections()
    {
        // Supp sections trop loin
        if (leftPoints.Count > 0 && leftPoints[0].y < player.transform.position.y - distanceBeforeDestroy){
            leftPoints.RemoveAt(0); 
            rightPoints.RemoveAt(0); 

            // MaJ LineRenderers et EdgeColliders
            UpdateLineRenderers();
            UpdateEdgeColliders();
        }

        for (int i = obstacles.Count - 1; i >= 0; i--){  //parcours obstacles pour supp si trop bas
            if (obstacles[i].transform.position.y < player.transform.position.y - distanceBeforeDestroy){
                Destroy(obstacles[i]);
                obstacles.RemoveAt(i);
            }
        }

        for (int i = flowers.Count - 1; i >= 0; i--){  // Parcours fleurs pour supprimer si loin
            if (flowers[i].transform.position.y < player.transform.position.y - distanceBeforeDestroy){
                Destroy(flowers[i]);
                flowers.RemoveAt(i);
            }
        }

        // creation nouveaux segments
        if (leftPoints[leftPoints.Count - 1].y < player.transform.position.y + distanceBeforeGenerate){
            ExtendRiver();
        }
    }

    void UpdateLineRenderers()
    {
        // MaJ  LineRenderer pour  gauche
        leftLineRenderer.positionCount = leftPoints.Count;
        for (int i = 0; i < leftPoints.Count; i++){
            leftLineRenderer.SetPosition(i, leftPoints[i]);
        }

        // MaJ  LineRenderer pour droite
        rightLineRenderer.positionCount = rightPoints.Count;
        for (int i = 0; i < rightPoints.Count; i++){
            rightLineRenderer.SetPosition(i, rightPoints[i]);
        }
    }

    void UpdateEdgeColliders()
    {
        leftEdgeCollider.points = leftPoints.ToArray();
        rightEdgeCollider.points = rightPoints.ToArray();
    }

    void GenerateRiverMesh()
    {
        if (riverMesh == null){
            riverMesh = new Mesh();
            riverMeshFilter.mesh = riverMesh;
        }

        riverMesh.Clear();

        List<Vector3> vertices = new List<Vector3>();
        for (int i = 0; i < leftPoints.Count; i++){
            vertices.Add(leftPoints[i]);  // add point gauche
            vertices.Add(rightPoints[i]); // add point droite
        }

        List<int> triangles = new List<int>();
        for (int i = 0; i < leftPoints.Count - 1; i++){
            int startIndex = i * 2;

            triangles.Add(startIndex);
            triangles.Add(startIndex + 2);
            triangles.Add(startIndex + 1);

            triangles.Add(startIndex + 1);
            triangles.Add(startIndex + 2);
            triangles.Add(startIndex + 3);
        }

        List<Vector2> uvs = new List<Vector2>();
        for (int i = 0; i < vertices.Count; i++){
            uvs.Add(new Vector2(vertices[i].x, vertices[i].y)); 
        }



        // Applique les données au maillage
        riverMesh.vertices = vertices.ToArray();
        riverMesh.triangles = triangles.ToArray();
        riverMesh.uv = uvs.ToArray();

        // Recalcule les normales du maillage pour une meilleure gestion des ombres
        riverMesh.RecalculateNormals();
    }
}
