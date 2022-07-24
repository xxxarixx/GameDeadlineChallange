using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
/// <summary>
/// all ui elements centrelizesion
/// </summary>
public class Main_UiController : MonoBehaviour
{
    public static Main_UiController instance { get; private set; }
    public Transform healthContainerHolder;
    public TextMeshProUGUI coinCountText;
    public GameObject ui_DamagePopup;
    public GameObject deadScreen;
    public Canvas canvas_World;
    private void Awake()
    {
        instance = this;
    }
    public void ChangeScene(int ScneneIndex)
    {
        SceneManager.LoadScene(ScneneIndex);
    }
}
