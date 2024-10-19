using System;
using System.Collections.Generic;

namespace E_Wybory.Domain.Entities;

public partial class Candidate
{
    public int IdCandidate { get; set; }

    public string? CampaignDescription { get; set; }

    public string JobType { get; set; } = null!;

    public string PlaceOfResidence { get; set; } = null!;

    public int PositionNumber { get; set; }

    public string? EducationStatus { get; set; }

    public string? Workplace { get; set; }

    public int IdPerson { get; set; }

    public int IdDistrict { get; set; }

    public int? IdParty { get; set; }

    public int IdElection { get; set; }

    public virtual District IdDistrictNavigation { get; set; } = null!;

    public virtual Election IdElectionNavigation { get; set; } = null!;

    public virtual Party? IdPartyNavigation { get; set; }

    public virtual Person IdPersonNavigation { get; set; } = null!;

    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
