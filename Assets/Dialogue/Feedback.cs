using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feedback
{
    public static List<Feedback> feedbacksList = new();

    public string clientName;
    public FeedbackType reactionType;
    public List<string> reactionsTxt;

    public Feedback(string clientName = "", FeedbackType reactionType = FeedbackType.None, List<string> reactionsTxt = null)
    {
        this.clientName = clientName;
        this.reactionType = reactionType;
        this.reactionsTxt = reactionsTxt;
    }

    public static object DetermineType(string feedbackType)
    {
        if (Enum.TryParse(feedbackType, true, out FeedbackType feedback))
            return feedback;

        return null;
    }
}


public enum FeedbackType
{
    None,
    Bad,
    Average,
    Good
}