using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicPanelResizer : MonoBehaviour
{
    public RectTransform panel; // Référence au RectTransform du Panel
    public Canvas canvas;       // Référence au Canvas (optionnel si nécessaire)
    public bool isLeft = true;  // Coller le bord gauche ou droit

    void Start()
    {
        ResizePanel();
    }

    void Update()
    {
        // Optionnel : Redimensionner en temps réel si l'aspect ratio change
        ResizePanel();
    }

    void ResizePanel()
    {
        if (panel != null)
        {
            // Obtenir l'aspect ratio actuel
            float canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
            float canvasHeight = canvas.GetComponent<RectTransform>().rect.height;
            float xscaleratio = canvasWidth / 1347; // 1347 est la largeur de référence
            float yscaleratio = canvasHeight / 758;  // 758 est la hauteur de référence

            // Redimensionner le panel
            panel.localScale = new Vector3(xscaleratio, yscaleratio, 1);

            // Déplacer le panel
            if (isLeft)
            {
                float virtualWidth = panel.rect.width * xscaleratio;
                float panelX = virtualWidth/2;
                panel.transform.position = new Vector3(panelX, canvasHeight/2, 0);
            }
            else
            {   
                float virtualWidth = panel.rect.width * xscaleratio;
                float panelX = virtualWidth/2;
                panelX -= canvasWidth;
                panel.transform.position = new Vector3(-panelX, canvasHeight/2, 0);
            }

        }
    }
}
