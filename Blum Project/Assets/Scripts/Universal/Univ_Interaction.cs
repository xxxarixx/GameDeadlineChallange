using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Univ_Interaction : MonoBehaviour
{
    public Color hilightColor = Color.green;
    public Color defaultColor = Color.white;
    public List<SpriteRenderer> effectInteractionColorOn = new List<SpriteRenderer>();
    public UnityEvent OnInteractionPressed;
    private float _clickDuration = .1f;
    private float _processClickDuration;
    private void Update()
    {
        if (_processClickDuration > 0f) _processClickDuration -= Time.deltaTime;
    }
    //just hilight interaction
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        foreach (var interaction in effectInteractionColorOn)
        {
            interaction.color = hilightColor;
        }
    }
    //if player press interaction button then interact
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (_processClickDuration > 0f) return;
        if (Player_References.instance.input.interactionPressed)
        {
            OnInteractionPressed?.Invoke();
            _processClickDuration = _clickDuration;
        }
    }
    //leave color to deafault Color
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        foreach (var interaction in effectInteractionColorOn)
        {
            interaction.color = defaultColor;
        }
    }
}
