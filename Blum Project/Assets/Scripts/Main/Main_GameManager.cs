using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Main_GameManager : MonoBehaviour
{
    public static Main_GameManager instance;
    [SerializeField] private GameObject ui_DamagePopup;
    [SerializeField] private Canvas canvas_World;
    private void Awake()
    {
        instance = this;
    }
    public void SpawnDamagePopup(Vector3 _position, float _damageDealt)
    {
        var spawnedDamagePopup = Instantiate(ui_DamagePopup, canvas_World.transform);
        float positionRandomRange = 0.3f;
        spawnedDamagePopup.transform.position = _position + new Vector3(Random.Range(-positionRandomRange, positionRandomRange), Random.Range(-positionRandomRange, positionRandomRange), 0f);
        var damagePopupText = spawnedDamagePopup.GetComponent<TextMeshProUGUI>();
        damagePopupText.text = _damageDealt.ToString();
    }
}
