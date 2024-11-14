using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageItemScript : InteractableScript
{

    public Animator animator;
    public AnimationClip clip;
    bool up = false;

    private void Start()
    {
        
    }

    public override void Interact(MonoBehaviour interactor = null)
    {
        up = !up;

        float currentTime = 1 - Mathf.Clamp(animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 0, 1);

        animator.Play(up?"PageUp": "PageDown", 0, currentTime);
        
    }


}
