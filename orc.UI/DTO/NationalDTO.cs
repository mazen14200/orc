using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace orc.UI.DTO;

public partial class NationalDTO
{
    [Required]
    public string? FirstName { get; set; }
    [Required]
    public string? LastName { get; set; }
    public IFormFile? file { get; set; }
    public string? TextValue_View { get; set; }
    public string? Message_View { get; set; }
}
