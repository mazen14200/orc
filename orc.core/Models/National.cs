using System;
using System.Collections.Generic;

namespace orc.core.Models;

public partial class National
{
    public string Id { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string NationalNumber { get; set; } = null!;
}
