using UnityEngine;
using System.Collections;
using DG.Tweening;

// when hit by fist, determine strike type

public class FistTarget
    : MonoBehaviour
{
    public new Renderer renderer;
    public Material material;
    public Rigidbody body;

    public void OnScriptReload()
    {
        renderer = GetComponentInChildren<Renderer>();
        material = renderer.material;
        body = GetComponent<Rigidbody>();
    }

    public void OnTriggerEnter(Collider collider)
    {
        var fist = collider.GetComponent<Fist>();
        if (!fist)
            return;

        material.DOKill();
        material.DOColor(Color.red, 0.2f);
        material.DOColor(Color.white, 0.2f).SetDelay(0.2f);
    }
}
