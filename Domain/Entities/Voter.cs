using System;
using System.Collections.Generic;

namespace E_Wybory.Domain.Entities;

public partial class Voter
{
    public int IdVoter { get; set; }

    public int IdElectionUser { get; set; }

    public int? IdDistrict { get; set; }

    public virtual ICollection<ElectionVoter> ElectionVoters { get; set; } = new List<ElectionVoter>();

    public virtual District? IdDistrictNavigation { get; set; }

    public virtual ElectionUser IdElectionUserNavigation { get; set; } = null!;
}
