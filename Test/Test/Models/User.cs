using System.ComponentModel.DataAnnotations;


namespace Test.Models
{
  public class User
  {
    [Key]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Surname { get; set; }
    [Required]
    public string DateOfBirth { get; set; }
    [Required]

    public string Number { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string Gender { get; set; }
  
        
  }
}
