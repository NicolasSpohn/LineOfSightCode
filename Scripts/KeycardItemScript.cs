using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeycardItemScript : InteractableScript
{
    public float spinSpeed;
    PlayerScript player;

    
    private void Start() => player = GameStateManagerScript.instance.player;

    private void Update() => transform.eulerAngles += Vector3.up * spinSpeed;

    public override void Interact(MonoBehaviour interactor = null)
    {
        player.hasKeycard = true;
        Destroy(gameObject);
    }
}
