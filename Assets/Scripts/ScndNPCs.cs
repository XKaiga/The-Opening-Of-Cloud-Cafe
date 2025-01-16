using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScndNPCs
{
    //public static List<Drink> secondariesDrinks = new();
    public static List<Drink> secondariesDrinksToServe = new();
    public static bool secndClientWaiting => secondariesDrinksToServe.Count != 0;

    public static List<Feedback> feedbacksList = new(){
            new Feedback("Client", FeedbackType.Good, new() {
                "“Wow, this is exactly what I needed. Perfectly balanced, and just the right temperature. You’ve got a real gift for this!”",
                "“Mmm, you nailed it. I can tell you put real care into this one. Thank you, it’s incredible.”",
                "“This is hands down the best coffee I’ve had in ages. You sure this isn’t magic?”",
                "“I didn’t think it’d be this good! Seriously, it’s even better than I imagined.”",
                "“You really outdid yourself! I’ll be back here for sure!.”"
            }),
            new Feedback("Client", FeedbackType.Average, new() {
                "“Not bad! Maybe a bit off from what I’m used to, but I like it. Thanks.”",
                "“Pretty good, though I think it’s missing… something. But I’m enjoying it anyway!”",
                "“This is nice. I wouldn’t say amazing, but hey, I’d come back for another.”",
                "“Hmm, this is alright.Not exactly what I expected, but I think it’ll grow on me.”",
                "“It’s decent! Maybe next time I’ll try something a little different.”"
            }),
            new Feedback("Client", FeedbackType.Bad, new() {
                "“Hmm… it’s, um… interesting. Maybe just a bit too much of something. Not sure it’s for me.”",
                "“Oh, I think there might’ve been a mix-up. This isn’t quite what I ordered.”",
                "“I appreciate the effort, but this just doesn’t taste right. Maybe next time?”",
                "“This is… definitely not what I expected.”",
                "“Yikes, did something go wrong with the recipe? Sorry, but this isn’t really my thing.”"
            })
    };

    //private void Awake()
    //{
    //    LoadSecondariesDrinks();
    //}

    //private void LoadSecondariesDrinks()
    //{
    //    for (int i = 0; i < 20; i++)
    //    {
    //        SyrupFlavour rndSyrp = (SyrupFlavour)Random.Range(1, 7);
    //        BaseFlavour rndBase = (BaseFlavour)Random.Range(1, 7);
    //        TopFlavour rndTopg = (TopFlavour)Random.Range(1, 7);
    //        secondariesDrinks.Add(new(rndBase, rndTopg, rndSyrp, scndOrder: $"Client: \"I want {rndTopg} on top of {rndBase}, and with a little bit of {rndSyrp}\""));
    //    }
    //}

    public static Drink GenerateRandomScndDrink()
    {
        SyrupFlavour rndSyrp = (SyrupFlavour)Random.Range(1, 7);
        BaseFlavour rndBase = (BaseFlavour)Random.Range(1, 7);
        TopFlavour rndTopg = (TopFlavour)Random.Range(1, 7);

        int newDrinkNumber = 1;
        if (secondariesDrinksToServe.Count > 0)
            newDrinkNumber = secondariesDrinksToServe[secondariesDrinksToServe.Count - 1].drinkNumberOfClient + 1;
        if (newDrinkNumber > 99)
            newDrinkNumber = 1;

        return new Drink(rndBase, rndTopg, rndSyrp, drinkNumberOfClient: newDrinkNumber, scndOrder: $"Client: \"I want {rndTopg} on top of {rndBase}, and with a little bit of {rndSyrp}\"");
    }
}
