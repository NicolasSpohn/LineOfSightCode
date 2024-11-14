using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIntroHelper : MonoBehaviour
{
    public DoorScript door;

    public void OpenDoor()
    {
        door.Interact();
    }
}
