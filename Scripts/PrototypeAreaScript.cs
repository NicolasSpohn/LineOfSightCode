using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrototypeAreaScript : MonoBehaviour
{
    public GameObject Enemy;
    Vector3 enemyInitPos;

    private void Start()
    {
        enemyInitPos = Enemy.transform.position;
        Enemy.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other)) Enemy.SetActive(true);
        Enemy.transform.position = enemyInitPos;
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other)) Enemy.SetActive(false);
    }

    bool IsPlayer(Collider other) => other.GetComponent<PlayerScript>() != null;
}
