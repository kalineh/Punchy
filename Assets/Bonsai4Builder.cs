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

            var ofs = Vector3.up * (0.2f + (1.0f - t) * 0.8f);
            var dir = Vector3.up;

            branch.StartCoroutine(branch.DoAttachment(parent, ofs, dir));

            parent = branch.gameObject;

            yield return new WaitForSeconds(0.25f);
        }
    }

    public static IEnumerator DoBuildOneUp(GameObject root, string name)
    {
        var settings = Bonsai4Settings.Get("Bonsai4SettingsSrc");
        var branch = Bonsai4.MakeBranch(settings);

        var ofs = Vector3.up;
        var dir = Vector3.up;

        branch.StartCoroutine(branch.DoAttachment(root, ofs, dir));

        yield return new WaitForSeconds(0.25f);
    }
}
