using UnityEngine;
using Mirror;

[DisallowMultipleComponent]
public class Targetable : NetworkBehaviour
{
    [SerializeField] private Transform aimAtPoint;

    public Transform GetAimAtPoint => aimAtPoint;
}
