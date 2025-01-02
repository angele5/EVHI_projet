using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void StartGame()
    {
        // Charger la scène du jeu
       // SceneManager.LoadScene("Calibration");
       SceneManager.LoadScene("Game");
 
    }
}
