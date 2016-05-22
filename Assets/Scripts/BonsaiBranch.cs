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

        stalkCollider = stalk.GetComponent<Collider>();
        leafCollider = leaf.GetComponent<Collider>();

        stalkRenderer = stalk.GetComponent<Renderer>();
        leafRenderer = leaf.GetComponent<Renderer>();

        StartCoroutine(DoGrow());
    }

    public GameObject MakeBranch()
    {
        var depthChild = depth + 1;
        if (depthChild > 20)
            return null;

        var prefab = Resources.Load<GameObject>("BonsaiBranch");
        var obj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
        var ofs = Vector3.RotateTowards(Vector3.up, Random.onUnitSphere, 1.25f, 0.0f);
        var closest = leafCollider.ClosestPointOnBounds(ofs);
        var scale = 1.0f - depth * 0.125f;

        scale *= Random.Range(0.8f, 1.2f);

        obj.transform.SetParent(branches.transform);
        obj.transform.position = closest;
        obj.transform.localRotation = Quaternion.LookRotation(ofs.normalized);
        obj.transform.localScale = Vector3.one * scale;

        var bonsai = obj.GetComponent<BonsaiBranch>();

        bonsai.name = string.Format("Branch{0}", depthChild);
        bonsai.depth = depthChild;

        return obj;
    }

    public IEnumerator DoGrow()
    {
        var targetScale = transform.localScale;

        transform.localScale = Vector3.zero;

        var shake = transform.DOShakePosition(1.5f, 0.015f);
        yield return transform.DOScale(targetScale, depth * 5.0f).WaitForCompletion();
        shake.Kill();

        transform.DOShakePosition(0.5f, 0.01f);

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
}
