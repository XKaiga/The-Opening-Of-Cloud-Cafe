using System;
using System.Collections.Generic;

public class Finals
{
    //FinalTxt
    public static List<Finals> finalsList = new();

    public string clientName;
    public FinalType finalType;
    public string finalTxt;

    public Finals(string clientName = "", FinalType finalType = FinalType.None, string finalTxt = "")
    {
        this.clientName = clientName;
        this.finalType = finalType;
        this.finalTxt = finalTxt;
    }

    public static object DetermineType(string finalType)
    {
        if (Enum.TryParse(finalType, true, out FinalType final))
            return final;

        return null;
    }
}


public enum FinalType
{
    None,
    GOOD,
    BAD,
    NEUTRAL
}
