using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarScript : MonoBehaviour
{
    public Image progressBarImage; // Référence à l'image de la barre
    public float currentValue = 0f; // Valeur actuelle

    private float maxValue = 100f;  // Valeur maximale
    private Image image = null;
    private Color green = new Color32(135,192,149,255); 
    private Color red = new Color32(203,112,102,255);
    private bool isGreen = true;

    // Start is called before the first frame update
    void Start()
    {
        // Get the image component
        image = GetComponent<Image>();
        // Set the color of the image
        image.color = isGreen ? green : red;
    }
    
    void Update()
    {
        // Calculer le remplissage en fonction de la valeur actuelle
        float fillAmount = Mathf.Clamp01(currentValue / maxValue);
        progressBarImage.fillAmount = fillAmount;
        // Changer la couleur de la barre en fonction de la valeur actuelle
        image.color = isGreen ? green : red;
    }

    // Méthode pour mettre à jour la valeur actuelle
    public void UpdateProgress(float value, float maxValue, bool isGreen)
    {
        currentValue = Mathf.Clamp(value, 0, maxValue);
        this.maxValue = maxValue;
        this.isGreen = isGreen;
    }
}
