using master_mind_api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace master_mind_api.Controllers;

[ApiController]
[Route("[controller]")]
public class GameController : ControllerBase
{

    public static List<Game> Games = new List<Game>();

    private readonly ILogger<GameController> _logger;

    public GameController(ILogger<GameController> logger)
    {
        _logger = logger;
    }

    [HttpGet("CreateGame", Name = "CreateGame")]
    public IActionResult CreateGame(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > 10)
            {
                return BadRequest("Name cannot be empty or more than 10 charactors.");
            }
            var id = "";
            do
            {
                id = Game.GenerateId(5);

            } while (Games.Exists(g => g.Id == id));
            Games.Add(new Game(id, name));
            return Created(string.Empty, new JoinGameDTO(id, name));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error creating game: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while creating the game.");
        }

    }

    [HttpGet("JoinGame", Name = "JoinGame")]
    public IActionResult JoinGame(string id, string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > 10)
            {
                return BadRequest("Name cannot be empty or more than 10 charactors.");
            }
            var found = Games.Find(g => g.Id == id.Substring(0, 5));
            if (found == null)
            {
                //return NotFound("Game id does not exist");
                Games.Add(new Game(id.Length == 0 ? Game.GenerateId(5) : id.Substring(0,5) , name));
                // found = Games.Find(g => g.Id == id.Substring(0, 5));
            } else
            {
                name = found.Join(name);
            }
            return Ok(new JoinGameDTO(id, name));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error join game: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while join the game.");
        }
    }

    [HttpGet("Status", Name = "Status")]
    public IActionResult Status(string id, int ri)
    {
        try
        {
            var found = Games.Find(g => g.Id == id);
            if (found == null)
            {
                return NotFound("Game id does not exist");
            }
            var winner = found.Winner;
            var users = found.Users.Where(u => u.Status == Models.Status.Active).ToList();
            var names = users.Select(u => u.Name).ToArray();
            var sets = users.Select(u => u.Sets.Count == 0 ? new string[] { "white", "white", "white", "white" } : (found.CheckEntrySet(u.Sets.Last()) )).ToArray();
            var roundIndexes = users.Select(u => u.Sets.Count == 0 ? 0 : u.Sets.Count ).ToArray();
            var wins = found.Users.Select(u => u.Wins).ToArray();
            var status = found.Status;
            var winSet = winner.Length == 0 ? new[] {-1, -1, -1, -1}  : found.Combination;
            return Ok(new StatusDTO(winner, names, sets, roundIndexes, wins, status, winSet));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error join game: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while join the game.");
        }
    }

    [HttpGet("Reset", Name = "Reset")]
    public IActionResult Reset(string id, string name)
    {
        try
        {
            var found = Games.Find(g => g.Id == id);
            if (found == null)
            {
                return NotFound("Game id does not exist");
            }
            found.Winner = "";
            //found.Status = GameStatus.Waiting;
            var user = found.Users.Find(u => u.Name == name);
            user.Status = Models.Status.Active;
            user.Sets = new List<int[]>();
            if (user.Role == Role.Owner) {
                found.ResetGame();
            }
            return Ok();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error join game: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while join the game.");
        }
    }

    [HttpPost("UpdateGame", Name = "UpdateGame")]
    public IActionResult UpdateGame(UpdateDTO update)
    {
        try
        {
            Game found = Games.Find(g => g.Id == update.gameId);
            if (found == null)
            {
                return NotFound("Game id does not exist");
            }
            User user = found.Users.Find(u => u.Name == update.userName);
            if (user == null)
            {
                return NotFound("User name does not exist");
            }
            if (user.Sets.Count() != update.roundIndex - 1  )
            {
                return BadRequest("Failed to upload this round");
            }
            user.Sets.Add(update.entrySet);
            if (found.IsWinner(update.entrySet)) 
            { 
                found.Winner = update.userName;
                found.Status = GameStatus.Won;
                found.Users.ForEach(u => u.Status = Models.Status.Inactive);
                user.Wins++;

            }
            return Ok(found.CheckEntrySet(update.entrySet));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error join game: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred while join the game.");
        }

    }

    public record JoinGameDTO (string id, string name);
    public record UpdateDTO (string gameId, string userName, int roundIndex, int[] entrySet);
    public record StatusDTO(string winner, string[] userNames, string[][] sets, int[] roundIndex, int[] wins, GameStatus gameStatus, int[] winSet);
}
