using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

// find better spawn pos (least colliders)
// snipping: trigger until sphere has no colliders then solid
// falling: leafy sound on trigger enter

public class BonsaiBranch
    : MonoBehaviour
{
    private GameObject branches;
    private GameObject stalk;
    private GameObject leaf;

    private Rigidbody stalkRigidbody;
    private Rigidbody leafRigidbody;

    private Collider stalkCollider;
    private Collider leafCollider;

    private Renderer stalkRenderer;
    private Renderer leafRenderer;

    private int depth = 0;

    public void Start()
    {
        OnScriptReload();
    }

    public void OnScriptReload()
    {
        branches = transform.FindChild("Branches").gameObject;
        stalk = transform.FindChild("Stalk").gameObject;
        leaf = transform.FindChild("Leaf").gameObject;

        stalkRigidbody = stalk.GetComponent<Rigidbody>();
        leafRigidbody = leaf.GetComponent<Rigidbody>();

        stalkCollider = stalk.GetComponent<Collider>();
        leafCollider = leaf.GetComponent<Collider>();

        stalkRenderer = stalk.GetComponent<Renderer>();
        leafRenderer = leaf.GetComponent<Renderer>();

        Physics.IgnoreCollision(stalkCollider, leafCollider);
        //Physics.IgnoreCollision(stalkCollider, transform.parent.parent.FindChild("Leaf").GetComponent<Collider>());

        //StartCoroutine(DoAutoGrow());
        StartCoroutine(DoDebugInput());
    }

    public Vector3 FindBestGrowDir()
    {
        var pos = transform.position + transform.up * 0.5f;
        var samples = 12;
        var bestDir = Vector3.up;
        var bestCount = 1000;

        for (int i = 0; i < samples; ++i)
        {
            var dir = Vector3.RotateTowards(Vector3.up, Random.onUnitSphere, 1.25f, 0.0f);
            var ofs = dir * 0.5f;
            var closest = leafCollider.ClosestPointOnBounds(ofs);
            var collisions = Physics.OverlapSphere(pos + ofs, 0.10f);
            var count = collisions.Length;

            //Debug.DrawLine(pos, pos + ofs, Color.Lerp(Color.white, Color.blue, Mathf.Clamp01(count * 0.2f)), 1.0f);

            if (count < bestCount)
            {
                //Debug.DrawLine(pos, pos + ofs, Color.red, 1.0f);
                bestDir = dir;
                bestCount = count;
            }
        }

        return bestDir;
    }

    public GameObject MakeBranch()
    {
        var dir = FindBestGrowDir();
        var src = leafCollider.ClosestPointOnBounds(transform.position + dir * (depth + 1));
        var dst = src + dir * (depth + 1);

        return MakeBranchFromTo(src, dst);
    }

    public GameObject MakeBranchFromTo(Vector3 src, Vector3 dst)
    {
        var depthChild = depth + 1;
        if (depthChild > 20)
            return null;

        var prefab = Resources.Load<GameObject>("BonsaiBranch");
        var obj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
        var ofs = dst - src;
        var dir = ofs.normalized;
        var len = ofs.magnitude;
        var scale = 1.0f - len * 0.125f;

        scale *= Random.Range(0.8f, 1.2f);

        obj.transform.SetParent(branches.transform);
        obj.transform.position = src;
        // what are we doing
        // we have a world space dir
        // so we can make a worldspace rot

        obj.transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        obj.transform.localScale = Vector3.one * scale;

        var bonsai = obj.GetComponent<BonsaiBranch>();

        bonsai.name = string.Format("Branch{0}", depthChild);
        bonsai.depth = depthChild;

        return obj;
    }

    public IEnumerator DoAutoGrow()
    {
        var targetScale = transform.localScale;

        transform.localScale = Vector3.zero;

        var shake = transform.DOShakePosition(1.5f, 0.015f);
        yield return transform.DOScale(targetScale, depth * 5.0f).WaitForCompletion();
        shake.Kill();

        transform.DOShakePosition(0.5f, 0.01f);

        if (depth > 0)
        {
            stalkRigidbody.isKinematic = false;
            leafRigidbody.isKinematic = false;
        }

        yield return new WaitForSeconds(Random.Range(1.0f, 5.0f) * depth);

        while (true)
        {
            var depthDelay = depth * 1.0f;
            var grownDelay = branches.transform.childCount * (depth + 1) * 5.0f;

            yield return new WaitForSeconds(depthDelay + grownDelay);

            MakeBranch();

            yield return null;
        }
    }

    public IEnumerator DoDebugInput()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.M))
                MakeBranch();

            yield return null;
        }
    }
}
