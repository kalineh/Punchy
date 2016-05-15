using UnityEngine;
using System.Collections;

public class Fist
    : MonoBehaviour
{
    // strike type: 
    // - forward
    // - forward charge
    // - upper
    // - hook
    // - fist vs palm/open with trigger

    public enum StrikeState
    {
        Idle,
        Jab,
        Uppercut,
        Hook,
    };

    StrikeState state;

    public GameObject owner;

    public float detectJab;
    public float detectUpper;
    public float detectHook;

    public float strikeForce;

    public void OnScriptReload()
    {
        StartCoroutine(DoDetectJab());
    }

    public void Update()
    {
    }

    public IEnumerator DoDetectJab()
    {
        var curr = Vector3.zero;
        var prev = Vector3.zero;
        var average = Vector3.zero;
        var jab = 0.0f;

        while (true)
        {
            yield return null;

            Debug.DrawLine(transform.position, transform.position + average, Color.Lerp(Color.white, Color.red, Mathf.Clamp01(jab)));

            prev = curr;
            curr = transform.position;

            var moved = prev - curr;
            var force = moved.magnitude;
            var dir = moved.normalized;

            if (force < 0.01f)
                continue;

            average = Vector3.Lerp(average, dir, 0.1f).normalized;

            var alignment = Vector3.Dot(dir, average);
            var alignedForce = force * alignment;

            jab += alignedForce;
        }
    }
}
