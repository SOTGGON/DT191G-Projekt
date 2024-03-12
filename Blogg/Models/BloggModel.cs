using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blogg.Models;

public class BloggModel
{
    // Properties

    public int Id { get; set; }

    [Required]
    [Display(Name = "Titel")]
    public string? Title { get; set; }

    [Required]
    [Display(Name = "Innehåll")]
    public string? Content { get; set; }

    [Display(Name = "Skapad av")]
    public string? CreateBy { get; set; }

    public string? ImageName { get; set; }

    [NotMapped]
    [Display(Name = "Bild")]
    public IFormFile? ImageFile { get; set; }

    [Display(Name = "Datum")]
    public DateTime PublishDate { get; set; } = DateTime.Now;

}