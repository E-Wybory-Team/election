using System;
using System.Collections.Generic;

namespace E_Wybory.Domain.Entities;

public partial class ElectionType
{
    public int IdElectionType { get; set; }

    public string ElectionTypeName { get; set; } = null!;

    public string? ElectionTypeInfo { get; set; }

    public virtual ICollection<Election> Elections { get; set; } = new List<Election>();
}
