using System;
using System.Collections.Generic;

namespace E_Wybory.Domain.Entities;

public partial class Vote
{
    public int IdVote { get; set; }

    public bool IsValid { get; set; }

    public int IdCandidate { get; set; }

    public int IdElection { get; set; }

    public virtual Candidate IdCandidateNavigation { get; set; } = null!;

    public virtual Election IdElectionNavigation { get; set; } = null!;
}
