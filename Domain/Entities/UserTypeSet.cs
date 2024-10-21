using System;
using System.Collections.Generic;

namespace E_Wybory.Domain.Entities;

public partial class UserTypeSet
{
    public int IdUserTypeSet { get; set; }

    public int IdElectionUser { get; set; }

    public int IdUserType { get; set; }

    public virtual ElectionUser IdElectionUserNavigation { get; set; } = null!;

    public virtual UserType IdUserTypeNavigation { get; set; } = null!;
}
