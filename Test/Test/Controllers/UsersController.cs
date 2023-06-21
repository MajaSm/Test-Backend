using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Globalization;
using System.Text.RegularExpressions;
using Test.Models;

namespace Test.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class UsersController : ControllerBase
  {
    private readonly MySqlConnection _sqlConnection;
    public UsersController(MySqlConnection sqlConnection)
    {
      _sqlConnection = sqlConnection;

    }

    private bool IsValidInput(User userObj, out List<string> validationErrors)
    {
      validationErrors = new List<string>();

      var emailRegex = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
      bool isValidEmail = emailRegex.IsMatch(userObj.Email);
      
      bool isValidDate = DateTime.TryParseExact(userObj.DateOfBirth, "d.M.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    

      var numberRegex = new Regex(@"^\+?\d+$");
      bool isValidNumber = numberRegex.IsMatch(userObj.Number);

     if (isValidDate)
      {
        DateTime date = DateTime.ParseExact(userObj.DateOfBirth, "d.M.yyyy", CultureInfo.InvariantCulture);
        int year = date.Year;
        isValidDate = year <= DateTime.Now.Year;
      }

      if (!isValidEmail)
      {
        validationErrors.Add("Email je krivo unesen.");
      }

      if (!isValidDate)
      {
        validationErrors.Add("Datum je krivo unesen.");
      }

      if (!isValidNumber)
      {
        validationErrors.Add("Broj je krivo unesen.");
      }
      return isValidEmail && isValidDate && isValidNumber ;
    }

    [HttpPost("add_user")]
    public async Task<IActionResult> AddUser([FromBody] User userObj)
    {
      if (userObj == null)
      {
        return BadRequest(new
        {
          Message = "Došlo je do pogreške."
        }); 
      }

        List<string> validationErrors;
        bool isValid = IsValidInput(userObj, out validationErrors);

      if (isValid)
        {
          _sqlConnection.Open();
          var command = new MySqlCommand("INSERT INTO users( id, name, surname, dateOfBirth, number, email, gender)" +
                                          " VALUES('" + userObj.Id + "','" + userObj.Name + "','" + userObj.Surname + "','" + userObj.DateOfBirth + "','" + userObj.Number + "','" + userObj.Email + "','" + userObj.Gender + "');", _sqlConnection);
          await command.ExecuteNonQueryAsync();

          await _sqlConnection.CloseAsync();
        return Ok(new
        {
          Message = "Korisk je dodan."
        });

      }
        else
        {
        return BadRequest(new
        {
          Message = validationErrors
        });
      }

       
      
    }
    [HttpPut("update_user")]
    public async Task<IActionResult> UpdateUser([FromBody] User userObj)
    {

      if (userObj == null)
      {
        return BadRequest();
      }

      List<string> validationErrors;
      bool isValid = IsValidInput(userObj, out validationErrors);

      if (isValid)
      {
          _sqlConnection.Open();
          var command = new MySqlCommand("update users set id = '" + userObj.Id + "', Name = '" + userObj.Name + "', Surname = '" + userObj.Surname + "', dateOfBirth = '" + userObj.DateOfBirth + "', number = '" + userObj.Number + "', email ='" + userObj.Email + "', gender = '" + userObj.Gender + "'  where id = '" + userObj.Id + "'; ", _sqlConnection);
          await command.ExecuteNonQueryAsync();
          await _sqlConnection.CloseAsync();
        return Ok(new
        {
          Message = "Korisk je azuriran."
        });

      }
        else
        {
          return BadRequest(new
          {
            Message = validationErrors
          });
        }

       
      

    }
    [HttpDelete("delete_user/{userId}")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
      if (userId == null)
      {
        return BadRequest(new
        {
          Message = "Korisnik ne postoji."
        });
      }
      else
      {
        _sqlConnection.Open();
        var command = new MySqlCommand("delete from users where id='" + userId + "';", _sqlConnection);
        await command.ExecuteNonQueryAsync();
        await _sqlConnection.CloseAsync();
        return Ok(new
        {
          Message = "Korisnik je obrisan"
        });
      }
    }
    [HttpGet("get_all_users")]
    public async Task<IActionResult> GetAllUser()
    {

      try
      {
        _sqlConnection.Open();
        var command = new MySqlCommand("SELECT * FROM users;", _sqlConnection);
        var reader = await command.ExecuteReaderAsync();

        var users = new List<User>(); 

        while (reader.Read())
        {
          var user = new User
          {
            Id = Convert.ToInt32(reader["id"]),
            Name = reader["name"].ToString(),
            Surname = reader["surname"].ToString(),
            DateOfBirth = reader["dateOfBirth"].ToString(),
            Number = reader["number"].ToString(),
            Email = reader["email"].ToString(),
            Gender = reader["gender"].ToString(),

          };

          users.Add(user);
        }

        await reader.CloseAsync();
        await _sqlConnection.CloseAsync();

        return Ok(users);
      }
      catch (Exception ex)
      {
        // Handle any exceptions that occur during the database query
        return StatusCode(500, ex.Message);
      }

    }
  
  }
}
