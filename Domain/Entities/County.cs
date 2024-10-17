using System;
using System.Collections.Generic;

namespace E_Wybory.Domain.Entities;

public partial class County
{
    public int IdCounty { get; set; }

    public string CountyName { get; set; } = null!;

    public int IdVoivodeship { get; set; }

    public virtual Voivodeship IdVoivodeshipNavigation { get; set; } = null!;

    public virtual ICollection<Province> Provinces { get; set; } = new List<Province>();
}
