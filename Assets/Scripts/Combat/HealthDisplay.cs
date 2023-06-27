using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private GameObject healthBarParent;
    [SerializeField] private Image healthBarImage;

    private void Awake() 
    {
        health.ClientOnHealthUpdated += OnHealthUpdate;
    }

    private void OnDestroy() 
    {
        health.ClientOnHealthUpdated -= OnHealthUpdate;
    }

    private void OnMouseEnter() 
    {
        healthBarParent.SetActive(true);
    }

    private void OnMouseExit() 
    {
        healthBarParent.SetActive(false);
    }

    private void OnHealthUpdate(int currentHp, int maxHp)
    {
        healthBarImage.fillAmount = (float)currentHp / maxHp;
    }
}
