﻿using System;
using System.Collections.Generic;

namespace E_Wybory.Domain.Entities;

public partial class ElectionUser
{
    public int IdElectionUser { get; set; }

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int IdPerson { get; set; }

    public int IdDistrict { get; set; }

    public virtual District IdDistrictNavigation { get; set; } = null!;

    public virtual Person IdPersonNavigation { get; set; } = null!;

    public virtual ICollection<UserTypeSet> UserTypeSets { get; set; } = new List<UserTypeSet>();

    public virtual Voter? Voter { get; set; }
}
