using System;
using System.Collections.Generic;

namespace E_Wybory.Domain.Entities;
public partial class Province
{
    public int IdProvince { get; set; }

    public string ProvinceName { get; set; } = null!;

    public int IdCounty { get; set; }

    public virtual ICollection<District> Districts { get; set; } = new List<District>();

    public virtual County IdCountyNavigation { get; set; } = null!;
}
