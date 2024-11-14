using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DoorScript : InteractableScript
{
    //new Collider collider;
    //NavMeshObstacle obstacle;
    Animator animator;

    public bool isClosed = true;
    bool isAnimating = false;

    public string anim_Closed = "Closed";
    public string anim_Open = "Open";
    public string anim_Closing = "Closing";
    public string anim_Opening = "Opening";

    public bool requiresKeycard;
    public new AudioCallerScript audio;
    PlayerScript player;

    private void Start()
    {
        //collider = GetComponent<Collider>();
        //obstacle = GetComponent<NavMeshObstacle>();
        //collider.isTrigger = !isClosed;
        //obstacle.enabled = isClosed;
        animator = GetComponent<Animator>();
        animator.Play(isClosed ? anim_Closed : anim_Open );
        if (requiresKeycard) player = GameStateManagerScript.instance.player;
        audio = GetComponent<AudioCallerScript>();
    }


    public override void Interact(MonoBehaviour interactor = null)
    {
        if (requiresKeycard) if (!player.hasKeycard || interactor.GetType() == typeof(StatueEnemyScript))
            {
                audio.PlaySoundOneShot(1);
                return;
            }

        if (isAnimating) return;

        animator.Play(isClosed ? anim_Opening : anim_Closing);
        isAnimating = true;
        isClosed = !isClosed;
        audio.PlaySoundOneShot(0);

    }

    public void ToggleCollision()
    {
        //collider.isTrigger = !isClosed;
        //obstacle.enabled = isClosed;
    }
    public void EndAnimation() => isAnimating = false;
}
