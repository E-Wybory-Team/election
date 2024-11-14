using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using E_Wybory.Domain.Entities;

namespace E_Wybory.Infrastructure.DbContext;

public partial class ElectionDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public ElectionDbContext()
    {
    }

    public ElectionDbContext(DbContextOptions<ElectionDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Candidate> Candidates { get; set; }

    public virtual DbSet<Constituence> Constituences { get; set; }

    public virtual DbSet<County> Counties { get; set; }

    public virtual DbSet<District> Districts { get; set; }

    public virtual DbSet<Election> Elections { get; set; }

    public virtual DbSet<ElectionType> ElectionTypes { get; set; }

    public virtual DbSet<ElectionUser> ElectionUsers { get; set; }

    public virtual DbSet<ElectionVoter> ElectionVoters { get; set; }

    public virtual DbSet<Party> Parties { get; set; }

    public virtual DbSet<Person> People { get; set; }

    public virtual DbSet<Province> Provinces { get; set; }

    public virtual DbSet<UserType> UserTypes { get; set; }

    public virtual DbSet<UserTypeSet> UserTypeSets { get; set; }

    public virtual DbSet<UserTypesGroup> UserTypesGroups { get; set; }

    public virtual DbSet<Voivodeship> Voivodeships { get; set; }

    public virtual DbSet<Vote> Votes { get; set; }

    public virtual DbSet<Voter> Voters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.HasKey(e => e.IdCandidate).HasName("PRIMARY");

            entity.HasIndex(e => e.IdDistrict, "FkDistrictsCandidates");

            entity.HasIndex(e => e.IdElection, "FkElectionsCandidates");

            entity.HasIndex(e => e.IdParty, "FkPartiesCandidates");

            entity.HasIndex(e => e.IdPerson, "FkPeopleCandidates");

            entity.Property(e => e.IdCandidate).HasColumnName("idCandidate");
            entity.Property(e => e.CampaignDescription)
                .HasMaxLength(255)
                .HasColumnName("campaignDescription");
            entity.Property(e => e.EducationStatus)
                .HasMaxLength(50)
                .HasColumnName("educationStatus");
            entity.Property(e => e.IdDistrict).HasColumnName("idDistrict");
            entity.Property(e => e.IdElection).HasColumnName("idElection");
            entity.Property(e => e.IdParty).HasColumnName("idParty");
            entity.Property(e => e.IdPerson).HasColumnName("idPerson");
            entity.Property(e => e.JobType)
                .HasMaxLength(50)
                .HasColumnName("jobType");
            entity.Property(e => e.PlaceOfResidence)
                .HasMaxLength(50)
                .HasColumnName("placeOfResidence");
            entity.Property(e => e.PositionNumber).HasColumnName("positionNumber");
            entity.Property(e => e.Workplace)
                .HasMaxLength(100)
                .HasColumnName("workplace");

            entity.HasOne(d => d.IdDistrictNavigation).WithMany(p => p.Candidates)
                .HasForeignKey(d => d.IdDistrict)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FkDistrictsCandidates");

            entity.HasOne(d => d.IdElectionNavigation).WithMany(p => p.Candidates)
                .HasForeignKey(d => d.IdElection)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FkElectionsCandidates");

            entity.HasOne(d => d.IdPartyNavigation).WithMany(p => p.Candidates)
                .HasForeignKey(d => d.IdParty)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FkPartiesCandidates");

            entity.HasOne(d => d.IdPersonNavigation).WithMany(p => p.Candidates)
                .HasForeignKey(d => d.IdPerson)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FkPeopleCandidates");
        });

        modelBuilder.Entity<Constituence>(entity =>
        {
            entity.HasKey(e => e.IdConstituency).HasName("PRIMARY");

            entity.Property(e => e.IdConstituency).HasColumnName("idConstituency");
            entity.Property(e => e.ConstituencyName)
                .HasMaxLength(50)
                .HasColumnName("constituencyName");
        });

        modelBuilder.Entity<County>(entity =>
        {
            entity.HasKey(e => e.IdCounty).HasName("PRIMARY");

            entity.HasIndex(e => e.IdVoivodeship, "FkVoivodeshipsCounties");

            entity.HasIndex(e => new { e.CountyName, e.IdVoivodeship }, "uniqueCountyNameInVoivodeship").IsUnique();

            entity.Property(e => e.IdCounty).HasColumnName("idCounty");
            entity.Property(e => e.CountyName)
                .HasMaxLength(50)
                .HasColumnName("countyName");
            entity.Property(e => e.IdVoivodeship).HasColumnName("idVoivodeship");

            entity.HasOne(d => d.IdVoivodeshipNavigation).WithMany(p => p.Counties)
                .HasForeignKey(d => d.IdVoivodeship)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FkVoivodeshipsCounties");
        });

        modelBuilder.Entity<District>(entity =>
        {
            entity.HasKey(e => e.IdDistrict).HasName("PRIMARY");

            entity.HasIndex(e => e.IdConstituency, "FkConstituencyDistrict");

            entity.HasIndex(e => e.IdProvince, "FkProvincesDistrict");

            entity.HasIndex(e => e.DistrictHeadquarters, "UniqueDistrictHeadquarters").IsUnique();

            entity.HasIndex(e => new { e.DistrictName, e.IdConstituency }, "uniqueDistrictNameInConstituency").IsUnique();

            entity.Property(e => e.IdDistrict).HasColumnName("idDistrict");
            entity.Property(e => e.DisabledFacilities).HasColumnName("disabledFacilities");
            entity.Property(e => e.DistrictHeadquarters)
                .HasMaxLength(150)
                .HasColumnName("districtHeadquarters");
            entity.Property(e => e.DistrictName)
                .HasMaxLength(50)
                .HasColumnName("districtName");
            entity.Property(e => e.IdConstituency).HasColumnName("idConstituency");
            entity.Property(e => e.IdProvince).HasColumnName("idProvince");

            entity.HasOne(d => d.IdConstituencyNavigation).WithMany(p => p.Districts)
                .HasForeignKey(d => d.IdConstituency)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FkConstituencyDistrict");

            entity.HasOne(d => d.IdProvinceNavigation).WithMany(p => p.Districts)
                .HasForeignKey(d => d.IdProvince)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FkProvincesDistrict");
        });

        modelBuilder.Entity<Election>(entity =>
        {
            entity.HasKey(e => e.IdElection).HasName("PRIMARY");

            entity.HasIndex(e => e.IdElectionType, "fkElectionTypeElections");

            entity.Property(e => e.IdElection).HasColumnName("idElection");
            entity.Property(e => e.ElectionEndDate)
                .HasColumnType("datetime")
                .HasColumnName("electionEndDate");
            entity.Property(e => e.ElectionStartDate)
                .HasColumnType("datetime")
                .HasColumnName("electionStartDate");
            entity.Property(e => e.ElectionTour).HasColumnName("electionTour");
            entity.Property(e => e.IdElectionType).HasColumnName("idElectionType");

            entity.HasOne(d => d.IdElectionTypeNavigation).WithMany(p => p.Elections)
                .HasForeignKey(d => d.IdElectionType)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fkElectionTypeElections");
        });

        modelBuilder.Entity<ElectionType>(entity =>
        {
            entity.HasKey(e => e.IdElectionType).HasName("PRIMARY");

            entity.HasIndex(e => e.ElectionTypeName, "uniqueElectionTypeName").IsUnique();

            entity.Property(e => e.IdElectionType).HasColumnName("idElectionType");
            entity.Property(e => e.ElectionTypeInfo)
                .HasMaxLength(255)
                .HasColumnName("electionTypeInfo");
            entity.Property(e => e.ElectionTypeName)
                .HasMaxLength(50)
                .HasColumnName("electionTypeName");
        });

        modelBuilder.Entity<ElectionUser>(entity =>
        {
            entity.HasKey(e => e.IdElectionUser).HasName("PRIMARY");

            entity.HasIndex(e => e.IdDistrict, "FkDistrictELectionUsers");

            entity.HasIndex(e => e.Email, "uniqueEmail").IsUnique();

            entity.HasIndex(e => e.IdPerson, "uniquePerson").IsUnique();

            entity.Property(e => e.IdElectionUser).HasColumnName("idElectionUser");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.IdDistrict).HasColumnName("idDistrict");
            entity.Property(e => e.IdPerson).HasColumnName("idPerson");
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .HasColumnName("password");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .HasColumnName("phoneNumber");
            entity.Property(e => e.UserSecret)
                .HasMaxLength(100)
                .HasColumnName("userSecret");

            entity.HasOne(d => d.IdDistrictNavigation).WithMany(p => p.ElectionUsers)
                .HasForeignKey(d => d.IdDistrict)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FkDistrictELectionUsers");

            entity.HasOne(d => d.IdPersonNavigation).WithOne(p => p.ElectionUser)
                .HasForeignKey<ElectionUser>(d => d.IdPerson)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FkPeopleElectionUsers");
        });

        modelBuilder.Entity<ElectionVoter>(entity =>
        {
            entity.HasKey(e => e.IdElectionVoter).HasName("PRIMARY");

            entity.HasIndex(e => e.IdElection, "FkElectionElectionVoters");

            entity.HasIndex(e => e.IdVoter, "FkVoterElectionVoters");

            entity.Property(e => e.IdElectionVoter).HasColumnName("idElectionVoter");
            entity.Property(e => e.IdElection).HasColumnName("idElection");
            entity.Property(e => e.IdVoter).HasColumnName("idVoter");
            entity.Property(e => e.VoteTime)
                .HasColumnType("datetime")
                .HasColumnName("voteTime");

            entity.HasOne(d => d.IdElectionNavigation).WithMany(p => p.ElectionVoters)
                .HasForeignKey(d => d.IdElection)
                .HasConstraintName("FkElectionElectionVoters");

            entity.HasOne(d => d.IdVoterNavigation).WithMany(p => p.ElectionVoters)
                .HasForeignKey(d => d.IdVoter)
                .HasConstraintName("FkVoterElectionVoters");
        });

        modelBuilder.Entity<Party>(entity =>
        {
            entity.HasKey(e => e.IdParty).HasName("PRIMARY");

            entity.HasIndex(e => e.PartyName, "uniquePartyName").IsUnique();

            entity.Property(e => e.IdParty).HasColumnName("idParty");
            entity.Property(e => e.Abbreviation)
                .HasMaxLength(10)
                .HasColumnName("abbreviation");
            entity.Property(e => e.IsCoalition).HasColumnName("isCoalition");
            entity.Property(e => e.ListCommiteeNumber).HasColumnName("listCommiteeNumber");
            entity.Property(e => e.PartyAddress)
                .HasMaxLength(100)
                .HasColumnName("partyAddress");
            entity.Property(e => e.PartyName)
                .HasMaxLength(50)
                .HasColumnName("partyName");
            entity.Property(e => e.PartyType)
                .HasMaxLength(30)
                .HasColumnName("partyType");
            entity.Property(e => e.Website)
                .HasMaxLength(2000)
                .HasColumnName("website");
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.IdPerson).HasName("PRIMARY");

            entity.HasIndex(e => e.Pesel, "uniquePesel").IsUnique();

            entity.Property(e => e.IdPerson).HasColumnName("idPerson");
            entity.Property(e => e.BirthDate)
                .HasColumnType("date")
                .HasColumnName("birthDate");
            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .HasColumnName("name");
            entity.Property(e => e.Pesel)
                .HasMaxLength(11)
                .IsFixedLength()
                .HasColumnName("pesel");
            entity.Property(e => e.Surname)
                .HasMaxLength(50)
                .HasColumnName("surname");
        });

        modelBuilder.Entity<Province>(entity =>
        {
            entity.HasKey(e => e.IdProvince).HasName("PRIMARY");

            entity.HasIndex(e => e.IdCounty, "FkCountyProvinces");

            entity.HasIndex(e => new { e.ProvinceName, e.IdCounty }, "uniqueProvinceNameInCounty").IsUnique();

            entity.Property(e => e.IdProvince).HasColumnName("idProvince");
            entity.Property(e => e.IdCounty).HasColumnName("idCounty");
            entity.Property(e => e.ProvinceName)
                .HasMaxLength(50)
                .HasColumnName("provinceName");

            entity.HasOne(d => d.IdCountyNavigation).WithMany(p => p.Provinces)
                .HasForeignKey(d => d.IdCounty)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FkCountyProvinces");
        });

        modelBuilder.Entity<UserType>(entity =>
        {
            entity.HasKey(e => e.IdUserType).HasName("PRIMARY");

            entity.HasIndex(e => e.IdUserTypesGroup, "FkUserTypesGroupsUserTypes");

            entity.Property(e => e.IdUserType).HasColumnName("idUserType");
            entity.Property(e => e.IdUserTypesGroup).HasColumnName("idUserTypesGroup");
            entity.Property(e => e.UserTypeInfo)
                .HasMaxLength(255)
                .HasColumnName("userTypeInfo");
            entity.Property(e => e.UserTypeName)
                .HasMaxLength(30)
                .HasColumnName("userTypeName");

            entity.HasOne(d => d.IdUserTypesGroupNavigation).WithMany(p => p.UserTypes)
                .HasForeignKey(d => d.IdUserTypesGroup)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FkUserTypesGroupsUserTypes");
        });

        modelBuilder.Entity<UserTypeSet>(entity =>
        {
            entity.HasKey(e => e.IdUserTypeSet).HasName("PRIMARY");

            entity.HasIndex(e => e.IdUserType, "UserTypesUserTypeSets");

            entity.HasIndex(e => new { e.IdElectionUser, e.IdUserType }, "uniqueElectionUserUserType").IsUnique();

            entity.Property(e => e.IdUserTypeSet).HasColumnName("idUserTypeSet");
            entity.Property(e => e.IdElectionUser).HasColumnName("idElectionUser");
            entity.Property(e => e.IdUserType).HasColumnName("idUserType");

            entity.HasOne(d => d.IdElectionUserNavigation).WithMany(p => p.UserTypeSets)
                .HasForeignKey(d => d.IdElectionUser)
                .HasConstraintName("FkElectionUsersUserTypeSets");

            entity.HasOne(d => d.IdUserTypeNavigation).WithMany(p => p.UserTypeSets)
                .HasForeignKey(d => d.IdUserType)
                .HasConstraintName("UserTypesUserTypeSets");
        });

        modelBuilder.Entity<UserTypesGroup>(entity =>
        {
            entity.HasKey(e => e.IdUserTypesGroup).HasName("PRIMARY");

            entity.HasIndex(e => e.UserTypesGroupName, "uniqueUserTypeName").IsUnique();

            entity.Property(e => e.IdUserTypesGroup).HasColumnName("idUserTypesGroup");
            entity.Property(e => e.UserTypesGroupInfo)
                .HasMaxLength(255)
                .HasColumnName("userTypesGroupInfo");
            entity.Property(e => e.UserTypesGroupName)
                .HasMaxLength(30)
                .HasColumnName("userTypesGroupName");
        });

        modelBuilder.Entity<Voivodeship>(entity =>
        {
            entity.HasKey(e => e.IdVoivodeship).HasName("PRIMARY");

            entity.Property(e => e.IdVoivodeship).HasColumnName("idVoivodeship");
            entity.Property(e => e.VoivodeshipName)
                .HasMaxLength(50)
                .HasColumnName("voivodeshipName");
        });

        modelBuilder.Entity<Vote>(entity =>
        {
            entity.HasKey(e => e.IdVote).HasName("PRIMARY");

            entity.HasIndex(e => e.IdDistrict, "FK_Votes_Districts");

            entity.HasIndex(e => e.IdCandidate, "FkCandidatesVotes");

            entity.HasIndex(e => e.IdElection, "FkElectionsVotes");

            entity.Property(e => e.IdVote).HasColumnName("idVote");
            entity.Property(e => e.IdCandidate).HasColumnName("idCandidate");
            entity.Property(e => e.IdDistrict).HasColumnName("idDistrict");
            entity.Property(e => e.IdElection).HasColumnName("idElection");
            entity.Property(e => e.IsValid).HasColumnName("isValid");

            entity.HasOne(d => d.IdCandidateNavigation).WithMany(p => p.Votes)
                .HasForeignKey(d => d.IdCandidate)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FkCandidatesVotes");

            entity.HasOne(d => d.IdDistrictNavigation).WithMany(p => p.Votes)
                .HasForeignKey(d => d.IdDistrict)
                .HasConstraintName("FK_Votes_Districts");

            entity.HasOne(d => d.IdElectionNavigation).WithMany(p => p.Votes)
                .HasForeignKey(d => d.IdElection)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FkElectionsVotes");
        });

        modelBuilder.Entity<Voter>(entity =>
        {
            entity.HasKey(e => e.IdVoter).HasName("PRIMARY");

            entity.HasIndex(e => e.IdDistrict, "idDistrict");

            entity.HasIndex(e => e.IdElectionUser, "uniqueElectionUser").IsUnique();

            entity.Property(e => e.IdVoter).HasColumnName("idVoter");
            entity.Property(e => e.IdDistrict).HasColumnName("idDistrict");
            entity.Property(e => e.IdElectionUser).HasColumnName("idElectionUser");

            entity.HasOne(d => d.IdDistrictNavigation).WithMany(p => p.Voters)
                .HasForeignKey(d => d.IdDistrict)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("idDistrict");

            entity.HasOne(d => d.IdElectionUserNavigation).WithOne(p => p.Voter)
                .HasForeignKey<Voter>(d => d.IdElectionUser)
                .HasConstraintName("FkElectionUsersVoters");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
