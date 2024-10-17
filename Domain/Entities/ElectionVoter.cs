using System;
using System.Collections.Generic;

namespace E_Wybory.Domain.Entities;

public partial class ElectionVoter
{
    public int IdElectionVoter { get; set; }

    public int IdElection { get; set; }

    public int IdVoter { get; set; }

    public virtual Election IdElectionNavigation { get; set; } = null!;

    public virtual Voter IdVoterNavigation { get; set; } = null!;
}
