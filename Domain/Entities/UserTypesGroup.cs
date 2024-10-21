using System;
using System.Collections.Generic;

namespace E_Wybory.Domain.Entities;

public partial class UserTypesGroup
{
    public int IdUserTypesGroup { get; set; }

    public string UserTypesGroupName { get; set; } = null!;

    public string? UserTypesGroupInfo { get; set; }

    public virtual ICollection<UserType> UserTypes { get; set; } = new List<UserType>();
}
