using System;
using System.Collections.Generic;

namespace E_Wybory.Domain.Entities;

public partial class Person
{
    public int IdPerson { get; set; }

    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string Pesel { get; set; } = null!;

    public DateTime BirthDate { get; set; }

    public virtual ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();

    public virtual ElectionUser? ElectionUser { get; set; }
}
