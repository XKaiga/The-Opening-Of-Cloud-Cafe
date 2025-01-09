using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effects : MonoBehaviour
{
    public EffectType type;

    public enum EffectType
{
    Pride,
    Joy,
    Anger,
    Shocked,
    Laughter
}


public static object DetermineEffect(string effectName)
    {
        if (System.Enum.TryParse(effectName, true, out EffectType effect))
            return effect;

        return null;
    }

}
