using System;
using System.Collections.Generic;

namespace E_Wybory.Domain.Entities;

public partial class Party
{
    public int IdParty { get; set; }

    public string PartyName { get; set; } = null!;

    public string? Abbreviation { get; set; }

    public string? PartyAddress { get; set; }

    public string PartyType { get; set; } = null!;

    public bool IsCoalition { get; set; }

    public int? ListCommiteeNumber { get; set; }

    public string? Website { get; set; }

    public virtual ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();
}
