using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class Bonsai4Builder
    : MonoBehaviour
{
    public static IEnumerator DoBuildTower(GameObject root, string name)
    {
        var src = Bonsai4Settings.Get("Bonsai4SettingsSrc");
        var dst = Bonsai4Settings.Get("Bonsai4SettingsDst");
        var depth = 16;

        var parent = root;

        for (int i = 0; i < depth; ++i)
        {
            var t = 1.0f / (float)depth * (float)i;
            var settings = Bonsai4Settings.Lerp(src, dst, t);
            var branch = Bonsai4.MakeBranch(settings);

            var tip = parent.transform.FindChild("Cube/Tip").position;
            var branchSrc = tip;
            var branchDst = tip + Vector3.up * (0.2f + (1.0f - t) * 0.8f);

            branch.StartCoroutine(branch.DoAttachment(parent, branchSrc, branchDst));

            parent = branch.gameObject;

            yield return new WaitForSeconds(0.25f);
        }

        Destroy(src.gameObject);
        Destroy(dst.gameObject);
    }

    public static IEnumerator DoBuildOneUp(GameObject root, string name)
    {
        var settings = Bonsai4Settings.Get("Bonsai4SettingsSrc");
        var branch = Bonsai4.MakeBranch(settings);

        var tip = root.transform.FindChild("Cube/Tip").position;
        var branchSrc = tip;
        var branchDst = tip + Vector3.up * 1.0f;

        branch.StartCoroutine(branch.DoAttachment(root, branchSrc, branchDst));

        yield return new WaitForSeconds(0.25f);
    }

    public static IEnumerator DoBuildCross(GameObject root, string name)
    {
        var parent = root;

        for (int i = 0; i < 4; ++i)
        {
            var settings = Bonsai4Settings.Get("Bonsai4SettingsSrc");
            var branch = Bonsai4.MakeBranch(settings);

            var tip = root.transform.FindChild("Cube/Tip").position;
            var branchSrc = tip;
            var branchDst = tip + Quaternion.Euler(45.0f, i * 90.0f, 0.0f) * Vector3.up;

            branch.StartCoroutine(branch.DoAttachment(parent, branchSrc, branchDst));

            yield return new WaitForSeconds(0.05f);
        }
    }

    public static IEnumerator DoBuildTree(GameObject root, string name, int depth = 0, int maxDepth = 8)
    {
        if (depth >= maxDepth)
            yield break;

        yield return new WaitForSeconds(0.15f);

        var src = Bonsai4Settings.Get("Bonsai4SettingsSrc");
        var dst = Bonsai4Settings.Get("Bonsai4SettingsDst");

        var parent = root;
        var branches = Random.Range(1, 3) - depth / 3;
        var t = 1.0f / (float)maxDepth * (float)depth;

        for (int i = 0; i < branches; ++i)
        {
            var settings = Bonsai4Settings.Lerp(src, dst, t);
            var branch = Bonsai4.MakeBranch(settings);

            var tip = parent.transform.FindChild("Cube/Tip").position;
            var ofs = Vector3.RotateTowards(Vector3.up, Random.onUnitSphere, Random.Range(0.2f, 0.5f), 0.0f) * (0.6f + (t * 0.4f));
            var branchSrc = tip;
            var branchDst = tip + ofs;

            branch.StartCoroutine(branch.DoAttachment(parent, branchSrc, branchDst));
            branch.StartCoroutine(DoBuildTree(branch.gameObject, string.Format("branch{0}.{1}", depth.ToString(), i.ToString()), depth + 1, maxDepth));

            yield return new WaitForSeconds(0.05f);
        }

        Destroy(src.gameObject);
        Destroy(dst.gameObject);
    }

}
