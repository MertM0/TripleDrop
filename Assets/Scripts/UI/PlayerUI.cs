using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public GameObject powerBarContainer;
    public Image powerBarFill;

    private void Start()
    {
        if (powerBarContainer == null)
        {
            Transform bg = transform.Find("PowerBarBg");
            if (bg != null)
            {
                powerBarContainer = bg.gameObject;
            }
        }

        if (powerBarFill == null) 
        {
            Transform bg = powerBarContainer != null ? powerBarContainer.transform : transform.Find("PowerBarBg");
            if (bg != null)
            {
                Transform fill = bg.Find("PowerBarFill");
                if (fill != null) powerBarFill = fill.GetComponent<Image>();
            }
        }
    }

    private void Update()
    {
        PlayerController localPlayer = PlayerController.Local;

        if (localPlayer == null)
        {
            if (powerBarContainer != null && powerBarContainer.activeSelf)
                powerBarContainer.SetActive(false);
            else if (powerBarFill != null && powerBarFill.gameObject.activeSelf)
                powerBarFill.gameObject.SetActive(false);
            return;
        }

        if (powerBarFill != null)
        {
            powerBarFill.fillAmount = localPlayer.chargePower / localPlayer.maxChargePower;
        }
        
        bool shouldShow = localPlayer.hasBall && localPlayer.isCharging;
        
        if (powerBarContainer != null)
        {
            if (powerBarContainer.activeSelf != shouldShow)
                powerBarContainer.SetActive(shouldShow);
        }
        else if (powerBarFill != null)
        {
            if (powerBarFill.gameObject.activeSelf != shouldShow)
                powerBarFill.gameObject.SetActive(shouldShow);
        }
    }
}