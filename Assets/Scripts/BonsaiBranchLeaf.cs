using UnityEngine;
using System.Collections;

public class BonsaiBranchLeaf
    : MonoBehaviour
{
    public void OnTriggerEnter(Collider collider)
    {
        var parent = transform.parent;
        var bonsai = parent.GetComponent<BonsaiBranch>();
        var force = collider.GetComponent<Rigidbody>();

        //bonsai.OnHit

        Debug.Log("ENTER");
    }
}
