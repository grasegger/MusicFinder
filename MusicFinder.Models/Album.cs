using System.ComponentModel.DataAnnotations.Schema;

namespace MusicFinder.Models;

[Table("Albums")]
public class Album
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
}