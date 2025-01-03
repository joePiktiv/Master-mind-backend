using System.Text;

namespace master_mind_api.Models;

public class Game
{
    public string Id {get; set;}

    public int[] Combination { get; set;} // color index 0-7, there are 4 colors in the combination

    public List<User> Users {get; set;}

    public string Winner { get; set; }

    public int Round { get; set; }

    public GameStatus Status { get; set; } 

    public Game(string uniqueId, string userName)
    {
        Id = uniqueId;
        Users = new List<User>{new User(userName)};
        Users[0].SetRole(Role.Owner);
        Combination = GenerateColorCombination(4, 8);
        Winner = "";
        Round = 0;
        Status = GameStatus.Waiting;
    }

    internal void ResetGame()
    {
        Combination = GenerateColorCombination(4, 8);
        Winner = "";
        Round = 0;
        Users.ForEach(u => u.Sets = new List<int[]>());
        Status = GameStatus.Waiting;
    }

    public bool IsWinner(int[] set)
    {
        return set.Select((s, index) => Combination[index] == s).All(r => r);
    }

    private int[] GenerateColorCombination(int num, int colorRange)
    {
        var random = new Random();
        return Enumerable.Range(0, num).Select(i => random.Next(colorRange)).ToArray();
    }

    public static string GenerateId(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var stringBuilder = new StringBuilder();

        for (int i = 0; i < length; i++)
        {
            stringBuilder.Append(chars[random.Next(chars.Length)]);
        }

        return stringBuilder.ToString();
    }

    public string Join(string userName)
    {
        var user = new User("");
        if (ExistUser(userName)) {
            var last = userName[userName.Length - 1];
            var lastIsInt = int.TryParse(last.ToString(), out var value );
            userName = lastIsInt ? GetNewNameWithIndex(userName, value): (userName + "1");
        } 
        user.Name = userName;
        user.SetRole(Role.Player);
        Users.Add(user);
        return userName;
    }

    private string GetNewNameWithIndex(string userName, int value)
    {
        return userName.Substring(0, userName.Length - 1) + (value + 1).ToString();
    }

    bool ExistUser (string name)
    {
        return Users.Exists( u => u.Name == name);
    }

    internal string[] CheckEntrySet(int[] entrySet)
    {
        var checkBlack = Combination.Select((c, index) => c == entrySet[index]).ToList();
        var blacks = checkBlack.Count(c => c);
        var restCombo = checkBlack.Select((c, index) => c ? -1 : Combination[index]).Where(c => c != -1).ToList();
        var restEntry = checkBlack.Select((c, index) => c ? -1 : entrySet[index]).Where(c => c != -1).ToList();
        var reds = 0;
        for (int i = 0; i < restEntry.Count; i++)
        {
            var entry = restEntry[i];
            var index = restCombo.IndexOf(entry);
            if (index == -1) continue;
            restCombo.RemoveAt(index);
            reds++;
        }
        return Enumerable.Repeat("black", blacks)
                                     .Concat(Enumerable.Repeat("red", reds))
                                     .Concat(Enumerable.Repeat("white", 4 - blacks - reds))
                                     .ToArray();
    }


}

public enum GameStatus { Waiting, Started, Won}

