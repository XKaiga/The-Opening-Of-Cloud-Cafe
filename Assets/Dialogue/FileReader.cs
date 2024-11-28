using System.IO;
using UnityEngine;

public class FileReader : MonoBehaviour
{

    private static bool isDrinkFileRead = false;
    public static void ReadDrinkFile()
    {
        if (isDrinkFileRead)
            return;

        string filePath = Application.dataPath + "/Dialogue/InfoLoadFiles/drinksRecipes.txt";

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            isDrinkFileRead = true;

            int dayNumber = 100;
            int index = -1;
            while (index < lines.Length - 1)
            {
                index++;

                if (lines[index].StartsWith("DAY", System.StringComparison.OrdinalIgnoreCase))
                {
                    int.TryParse(lines[index].Substring(3), out dayNumber);
                    continue;
                }

                if (dayNumber < GameManager.startDayNum)
                {
                    index++;
                    continue;
                }

                if (lines[index].Contains("(DRINK") && Dialogue.ExtractClientNameAndDrinkNumber(lines[index], out string name, out int drinkNumber))
                {
                    index++;

                    string[] recipeIgredients = lines[index].Split('_');

                    Drink drink = new(client: name, drinkNumberOfClient: drinkNumber, dayOfTheDrink: dayNumber);
                    foreach (string igredient in recipeIgredients)
                        DrinkManager.AddFlavour(igredient, drink);

                    DrinkManager.mainDrinks.Add(drink);
                }
            }
        }
    }

    private static bool isMusicFileRead = false;
    public static void ReadMusicFile()
    {
        if (isMusicFileRead)
            return;

        string filePath = Application.dataPath + "/Dialogue/InfoLoadFiles/Musics.txt";

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            isMusicFileRead = true;

            int index = -1;
            while (index < lines.Length - 1)
            {
                index++;

                string[] musicCodeParts = Dialogue.TrimSplitDialogueCode(lines[index]);

                AudioClip newAudioClip = Resources.Load<AudioClip>("Musics/" + musicCodeParts[0]);

                var musicEmoDetermined = Music.DetermineEmotion(musicCodeParts[1]);
                if (musicEmoDetermined == null)
                    continue;

                if (musicEmoDetermined is EmotionEnum emotion)
                {
                    Music music = new(musicCodeParts[0], emotion, false, newAudioClip);
                    Music.musicList.Add(music);
                }
            }
        }
    }

    private static bool isFeedbacksFileRead = false;
    public static void ReadFeedbackFile()
    {
        if (isFeedbacksFileRead)
            return;

        string filePath = Application.dataPath + "/Dialogue/InfoLoadFiles/Feedbacks.txt";

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            isFeedbacksFileRead = true;

            int index = -1;
            Feedback temp = new();
            temp.reactionsTxt = new();
            while (index < lines.Length - 1)
            {
                index++;

                string line = lines[index];
                if (lines[index].StartsWith('('))
                {
                    if (temp.reactionsTxt.Count != 0)
                    {
                        Feedback.feedbacksList.Add(temp);
                        temp = new();
                        temp.reactionsTxt = new();
                    }

                    string[] feedbackCodeParts = Dialogue.TrimSplitDialogueCode(lines[index]);

                    string name = feedbackCodeParts[0];
                    string type = feedbackCodeParts[1];

                    var feedbackTypeDetermined = Feedback.DetermineType(feedbackCodeParts[1]);
                    if (feedbackTypeDetermined == null)
                        continue;


                    if (feedbackTypeDetermined is FeedbackType feedbackType)
                    {

                        temp.clientName = name;
                        temp.reactionType = feedbackType;
                    }
                }
                else
                    temp.reactionsTxt.Add(temp.clientName.ToUpper() + ": " + lines[index]);
            }
            Feedback.feedbacksList.Add(temp);
        }
    }

    private static bool isFinalsFileRead = false;
    public static void ReadFinalsFile()
    {
        if (isFinalsFileRead)
            return;

        string filePath = Application.dataPath + "/Dialogue/InfoLoadFiles/finais.txt";

        Finals temp = new();
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            isFinalsFileRead = true;

            int index = -1;
            while (index < lines.Length - 1)
            {
                index++;

                string line = lines[index];
                if (lines[index].StartsWith('('))
                {
                    if (temp.finalTxt != "")
                    {
                        Finals.finalsList.Add(temp);
                        temp = new();
                    }

                    string[] finalCodeParts = Dialogue.TrimSplitDialogueCode(lines[index]);

                    string name = finalCodeParts[0];
                    string type = finalCodeParts[1];

                    var finalTypeDetermined = Finals.DetermineType(finalCodeParts[1]);
                    if (finalTypeDetermined == null)
                        continue;

                    if (finalTypeDetermined is FinalType finalType)
                    {
                        temp.clientName = name;
                        temp.finalType = finalType;
                    }
                }
                else
                    temp.finalTxt += lines[index];
            }
            Finals.finalsList.Add(temp);
        }
    }
}
