using System;
using System.Collections.Generic;

namespace E_Wybory.Domain.Entities;

public partial class Voivodeship
{
    public int IdVoivodeship { get; set; }

    public string VoivodeshipName { get; set; } = null!;

    public virtual ICollection<County> Counties { get; set; } = new List<County>();
}
