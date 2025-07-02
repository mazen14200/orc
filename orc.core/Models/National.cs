using System;
using System.Collections.Generic;

namespace orc.core.Models;
public partial class National
{
    public string Id { get; set; } = null!;

    public string? Name { get; set; }

    public string NationalNumber { get; set; } = null!;

    public string? Address { get; set; }

    public string? Government { get; set; }
}
