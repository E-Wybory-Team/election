using System;
using System.Collections.Generic;

namespace E_Wybory.Domain.Entities;

public partial class District
{
    public int IdDistrict { get; set; }

    public string DistrictName { get; set; } = null!;

    public bool DisabledFacilities { get; set; }

    public string DistrictHeadquarters { get; set; } = null!;

    public int? IdConstituency { get; set; }

    public int? IdProvince { get; set; }

    public virtual ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();

    public virtual ICollection<ElectionUser> ElectionUsers { get; set; } = new List<ElectionUser>();

    public virtual Constituence? IdConstituencyNavigation { get; set; } = null!;

    public virtual Province? IdProvinceNavigation { get; set; }

    public virtual ICollection<Voter> Voters { get; set; } = new List<Voter>();

    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
