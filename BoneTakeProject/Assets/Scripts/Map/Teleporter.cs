using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public Transform tpPosition;
    private void OnTriggerEnter2D(Collider2D other)
    {
        other.gameObject.transform.position = tpPosition.position;
    }
}
