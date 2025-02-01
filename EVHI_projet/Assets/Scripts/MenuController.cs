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

    public void QuitGame()
    {
        Application.Quit(); //fonctionne quand le jeu exporte
        Debug.Log("Game is over");
    }
}
