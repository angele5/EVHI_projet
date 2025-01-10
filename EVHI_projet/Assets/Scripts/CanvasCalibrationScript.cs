using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CanvasCalibrationScript : MonoBehaviour
{

    public GameObject askProfilePanel;
    public GameObject calibrationPanel;
    public GameObject checkProfilePanel;

    private TMP_InputField inputFieldProfil;

    // Start is called before the first frame update
    void Start()
    {
        inputFieldProfil = askProfilePanel.GetComponentInChildren<TMP_InputField>();
        PlayerPrefs.SetString("PROFILE_ACTUEL", "None");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnProfileButtonClicked()
    {
        askProfilePanel.SetActive(false);
        string profileName = inputFieldProfil.text;

        // Sauvegarde le nom du profil actuel
        PlayerPrefs.SetString("PROFILE_ACTUEL", profileName);
        PlayerPrefs.SetString(profileName, "");
        PlayerPrefs.Save();
        Debug.Log("PROFILE_ACTUEL : " + PlayerPrefs.GetString("PROFILE_ACTUEL"));
        if (PlayerPrefs.HasKey(profileName))
        {
            checkProfilePanel.SetActive(true);
        }
        else 
        {
            calibrationPanel.SetActive(true);
        }
    }

    public void OnGarderButtonClicked()
    {
        checkProfilePanel.SetActive(false);
        // Lancer la scene du jeu
        Debug.Log("Lancer la scene du jeu");
    }

    public void OnCalibrationButtonClicked()
    {
        checkProfilePanel.SetActive(false);
        // Lance la calibration
        Debug.Log("Lancer la calibration");
    }

}

