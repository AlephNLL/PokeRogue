using UnityEngine;
using UnityEngine.UI;

public enum HandPose
{
    Idle,
    Point
}

public class TutorialHandActor : MonoBehaviour
{
    public static TutorialHandActor instance;

    [Header("Sprites")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite pointSprite;

    private Image handImage;

    private void Awake()
    {
        instance = this;
        handImage = GetComponent<Image>();
        gameObject.SetActive(false); 
    }

    public void ChangePose(HandPose newPose)
    {
        switch (newPose)
        {
            case HandPose.Idle: 
                handImage.sprite = idleSprite;
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case HandPose.Point: handImage.sprite = pointSprite; break;
        }
    }

    /// <summary>
    /// Calcula la posición global del elemento de UI destino y mueve la mano ahí.
    /// </summary>
    public void PointAt(RectTransform targetUI, Vector3 offset, HandPose pose)
    {
        if (targetUI == null) return;

        gameObject.SetActive(true);
        ChangePose(pose);

        transform.rotation = Quaternion.Euler(0, 0, 90);
        // 1. Obtenemos el centro del botón en el espacio de su propio Canvas
        Vector3[] corners = new Vector3[4];
        targetUI.GetWorldCorners(corners);
        Vector3 targetWorldCenter = (corners[0] + corners[2]) / 2f;

        // 2. Averiguamos qué cámara está renderizando al monstruo
        Canvas targetCanvas = targetUI.GetComponentInParent<Canvas>();
        Camera targetCamera = null;

        // Si el canvas del bicho no es Overlay (ej. World Space), necesitamos la cámara principal
        if (targetCanvas != null && targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            targetCamera = targetCanvas.worldCamera != null ? targetCanvas.worldCamera : Camera.main;
        }

        // 3. Traducimos ese punto del mundo a un punto en la pantalla (píxeles 2D)
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(targetCamera, targetWorldCenter);

        // 4. Averiguamos cómo funciona el Canvas de nuestra Mano Flotante
        RectTransform handCanvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        Canvas handCanvas = GetComponentInParent<Canvas>();
        Camera handCamera = (handCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : handCanvas.worldCamera;

        // 5. Traducimos el punto de la pantalla al sistema de coordenadas local del Canvas del Tutorial
        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(handCanvasRect, screenPoint, handCamera, out localPosition);

        // 6. Aplicamos la posición final + tu offset manual
        GetComponent<RectTransform>().localPosition = new Vector3(localPosition.x, localPosition.y, 0) + offset;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}