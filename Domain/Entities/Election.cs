using System;
using System.Collections.Generic;

namespace E_Wybory.Domain.Entities;

public partial class Election
{
    public int IdElection { get; set; }

    public DateTime ElectionStartDate { get; set; }

    public DateTime ElectionEndDate { get; set; }

    public int? ElectionTour { get; set; }

    public int IdElectionType { get; set; }

    public virtual ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();

    public virtual ICollection<ElectionVoter> ElectionVoters { get; set; } = new List<ElectionVoter>();

    public virtual ElectionType IdElectionTypeNavigation { get; set; } = null!;

    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
