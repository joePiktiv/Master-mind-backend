using Microsoft.AspNetCore.Identity;

namespace master_mind_api.Models;


public class User
{
    public string Name { get; set; }

    public List<int[]> Sets { get; set; }

    public Status Status { get; set; }

    public Role Role { get; set; }

    public int Wins { get; set; }

    public User(string name)
    {
        Name = name;
        Sets = new List<int[]>();
        Status = Status.Active;
        Wins = 0;
    }

    public void SetRole (Role role)
    { 
        Role = role; 
    }

    void UpdateSet(int[] set)
    {
        Sets.Add(set);
    }
}

public enum Status { Active, Inactive, Ready, NotReady };
public enum Role { Owner, Player };