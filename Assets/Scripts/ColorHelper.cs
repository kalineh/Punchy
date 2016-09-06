using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public static class ColorExtensions
{
    public static Color Blink(this Color self)
    {
        return self * (float)((int)(Time.time * 10.0f) % 2);
    }

    public static Color RandomColor()
    {
        return new Color(
            Random.Range(0.0f, 1.0f),
            Random.Range(0.0f, 1.0f),
            Random.Range(0.0f, 1.0f),
            1.0f
        );
    }
}
