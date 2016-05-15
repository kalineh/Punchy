using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Fist
    : MonoBehaviour
{
    public GameObject owner;

    public float recharge = 1.0f;

    public void OnScriptReload()
    {
        StartCoroutine(DoRecharge());
        StartCoroutine(DoDetectJab());
    }

    public void Update()
    {
    }

    public void OnHit()
    {
        recharge = 0.0f;
    }

    public IEnumerator DoRecharge()
    {
        var scale = transform.localScale;

        while (true)
        {
            var selfxz = new Vector3(transform.position.x, 0.0f, transform.position.z);
            var ownerxz = new Vector3(owner.transform.position.x, 0.0f, owner.transform.position.z);
            var ofs = ownerxz - selfxz;
            var len = ofs.magnitude;

            var chargedPrev = (recharge >= 1.0f);

            if (len < 0.30f)
                recharge += 7.5f * Time.deltaTime;

            var chargedCurr = (recharge >= 1.0f);

            if (chargedCurr && !chargedPrev)
            {
                transform.DOShakePosition(0.01f);
            }

            recharge = Mathf.Clamp01(recharge);

            var s = recharge * recharge;
            transform.localScale = scale * s;

            yield return null;
        }
    }

    public IEnumerator DoDetectJab()
    {
        var curr = Vector3.zero;
        var prev = Vector3.zero;
        var average = Vector3.zero;
        var jab = 0.0f;

        while (true)
        {
            jab -= 1.0f * Time.deltaTime;
            jab = Mathf.Clamp01(jab);

            yield return null;

            Debug.DrawLine(transform.position, transform.position + average, Color.Lerp(Color.white, Color.red, Mathf.Clamp01(jab)));

            prev = curr;
            curr = transform.position;

            var moved = curr - prev;
            var force = moved.magnitude;
            var dir = moved.normalized;

            if (force < 0.01f)
                continue;

            average = Vector3.Lerp(average, dir, 0.25f).normalized;

            var alignment = Vector3.Dot(dir, average);
            var alignedForce = force * alignment;

            jab += alignedForce;
        }
    }
}
