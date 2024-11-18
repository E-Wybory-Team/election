using System;
using System.Collections.Generic;
namespace E_Wybory.Domain.Entities;

public partial class Constituence
{
    public int IdConstituency { get; set; }

    public string ConstituencyName { get; set; } = null!;

    public virtual ICollection<District> Districts { get; set; } = new List<District>();
}
