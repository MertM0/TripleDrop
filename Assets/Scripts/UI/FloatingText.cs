using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public AnimationCurve verticalCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float curveScale = 2f;
    public float destroyTime = 1.5f;

    private TextMeshPro textMesh;
    private Color originalColor;
    private float timer = 0f;
    private Camera localCam;
    private Vector3 startPosition;

    public void Initialize(string text, Color color)
    {
        textMesh = GetComponentInChildren<TextMeshPro>();
        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.color = color;
            originalColor = color;
        }

        FindLocalCamera();
        
        startPosition = transform.position;
        Destroy(gameObject, destroyTime);
    }

    private void FindLocalCamera()
    {
        if (Camera.main != null && Camera.main.isActiveAndEnabled)
        {
            localCam = Camera.main;
            return;
        }

        foreach (Camera cam in Camera.allCameras)
        {
            if (cam.isActiveAndEnabled)
            {
                localCam = cam;
                break;
            }
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float normalizedTime = timer / destroyTime;

        float yOffset = verticalCurve.Evaluate(normalizedTime) * curveScale;
        transform.position = startPosition + new Vector3(0, yOffset, 0);

        if (textMesh != null)
        {
            float alpha = Mathf.Lerp(1f, 0f, normalizedTime);
            textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }

        if (localCam != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - localCam.transform.position);
        }
        else
        {
            FindLocalCamera();
        }
    }
}