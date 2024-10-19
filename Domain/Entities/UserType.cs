using System;
using System.Collections.Generic;

namespace E_Wybory.Domain.Entities;

public partial class UserType
{
    public int IdUserType { get; set; }

    public string UserTypeName { get; set; } = null!;

    public string? UserTypeInfo { get; set; }

    public int IdUserTypesGroup { get; set; }

    public virtual UserTypesGroup IdUserTypesGroupNavigation { get; set; } = null!;

    public virtual ICollection<UserTypeSet> UserTypeSets { get; set; } = new List<UserTypeSet>();
}
