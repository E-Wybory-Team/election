using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using E_Wybory.Client.Services;
using E_Wybory.Client.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using E_Wybory.Client.Components.Pages;
using E_Wybory.Client.FilterData;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class CandidateInfoTests : TestContext
    {
        private readonly Mock<IPartyManagementService> _partyManagementServiceMock;
        private readonly Mock<IPersonManagementService> _personManagementServiceMock;
        private readonly Mock<IElectionTypeManagementService> _electionTypeManagementServiceMock;
        private readonly Mock<IFilterWrapperManagementService> _filterWrapperServiceMock;

        public CandidateInfoTests()
        {
            _partyManagementServiceMock = new Mock<IPartyManagementService>();
            _personManagementServiceMock = new Mock<IPersonManagementService>();
            _electionTypeManagementServiceMock = new Mock<IElectionTypeManagementService>();
            _filterWrapperServiceMock = new Mock<IFilterWrapperManagementService>();

            Services.AddSingleton(_partyManagementServiceMock.Object);
            Services.AddSingleton(_personManagementServiceMock.Object);
            Services.AddSingleton(_electionTypeManagementServiceMock.Object);
            Services.AddSingleton(_filterWrapperServiceMock.Object);
        }

        [Fact]
        public void CandidateInfo_Should_Render_Correctly()
        {
            // Arrange
            var electionTypes = new List<ElectionTypeViewModel>
            {
                new ElectionTypeViewModel { IdElectionType = 1, ElectionTypeName = "Presidential" },
                new ElectionTypeViewModel { IdElectionType = 2, ElectionTypeName = "Parliamentary" }
            };
            _electionTypeManagementServiceMock.Setup(s => s.ElectionTypes()).ReturnsAsync(electionTypes);

            var parties = new List<PartyViewModel>
            {
                new PartyViewModel { IdParty = 1, PartyName = "Party A" },
                new PartyViewModel { IdParty = 2, PartyName = "Party B" }
            };
            _partyManagementServiceMock.Setup(s => s.Parties()).ReturnsAsync(parties);
            _partyManagementServiceMock.Setup(s => s.GetPartyNameById(It.IsAny<int>(), It.IsAny<List<PartyViewModel>>()))
                .Returns((int id, List<PartyViewModel> partyList) => partyList.Find(p => p.IdParty == id)?.PartyName);

            var candidates = new List<CandidatePersonViewModel>
            {
                new CandidatePersonViewModel
                {
                    candidateViewModel = new CandidateViewModel
                    {
                        PositionNumber = 1,
                        JobType = "Engineer",
                        Workplace = "Company A",
                        PlaceOfResidence = "City A",
                        EducationStatus = "Bachelor",
                        IdParty = 1
                    },
                    personViewModel = new PersonViewModel
                    {
                        Name = "John",
                        Surname = "Doe",
                        BirthDate = new DateTime(1985, 5, 10)
                    }
                },
                new CandidatePersonViewModel
                {
                    candidateViewModel = new CandidateViewModel
                    {
                        PositionNumber = 2,
                        JobType = "Doctor",
                        Workplace = "Hospital B",
                        PlaceOfResidence = "City B",
                        EducationStatus = "Master",
                        IdParty = 2
                    },
                    personViewModel = new PersonViewModel
                    {
                        Name = "Jane",
                        Surname = "Smith",
                        BirthDate = new DateTime(1990, 7, 20)
                    }
                }
            };
            _filterWrapperServiceMock.Setup(s => s.GetFilteredCandidates(null, null, null, null, null)).ReturnsAsync(candidates);

            var filterWrapperFull = new FilterListWrapperFull
            {
                FilterListWrapper = new FilterListWrapper
                {
                    VoivodeshipFilter = new List<VoivodeshipViewModel>
                    {
                        new VoivodeshipViewModel { idVoivodeship = 1, voivodeshipName = "Voivodeship A" },
                        new VoivodeshipViewModel { idVoivodeship = 2, voivodeshipName = "Voivodeship B" }
                    },
                    CountyFilter = new List<CountyViewModel>
                    {
                        new CountyViewModel { IdCounty = 1, IdVoivodeship = 1, CountyName = "County A1" },
                        new CountyViewModel { IdCounty = 2, IdVoivodeship = 2, CountyName = "County B1" }
                    },
                    ProvinceFilter = new List<ProvinceViewModel>
                    {
                        new ProvinceViewModel { IdProvince = 1, IdCounty = 1, ProvinceName = "Province A1" },
                        new ProvinceViewModel { IdProvince = 2, IdCounty = 2, ProvinceName = "Province B1" }
                    },
                    DistrictFilter = new List<DistrictViewModel>
                    {
                        new DistrictViewModel { IdDistrict = 1, IdProvince = 1, DistrictName = "District A1", DistrictHeadquarters = "HQ A1" },
                        new DistrictViewModel { IdDistrict = 2, IdProvince = 2, DistrictName = "District B1", DistrictHeadquarters = "HQ B1" }
                    }
                },
                ElectionFilter = electionTypes
            };
            _filterWrapperServiceMock.Setup(s => s.GetFilteredLists(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(filterWrapperFull);

            _personManagementServiceMock.Setup(s => s.CountPersonAge(It.IsAny<DateTime>()))
                .Returns((DateTime birthDate) =>
                {
                    var today = DateTime.Today;
                    var age = today.Year - birthDate.Year;
                    if (birthDate.Date > today.AddYears(-age)) age--;
                    return age;
                });

            // Act
            var cut = RenderComponent<CandidateInfo>();

            // Wait for any asynchronous tasks to complete
            cut.WaitForAssertion(() => Assert.Equal(candidates.Count, cut.FindAll("table.candidate-table tbody tr").Count));

            // Assert
            // Verify table rendering
            var rows = cut.FindAll("table.candidate-table tbody tr");
            Assert.Equal(2, rows.Count);

            // Verify first candidate's data
            var firstRowCells = rows[0].QuerySelectorAll("td");

            Assert.Equal("1", firstRowCells[0].InnerHtml.Trim()); 
            Assert.Equal("Doe John", firstRowCells[1].InnerHtml.Trim()); 
            Assert.Equal("Engineer", firstRowCells[2].InnerHtml.Trim()); 
            Assert.Equal("Company A", firstRowCells[3].InnerHtml.Trim());
            Assert.Equal("City A", firstRowCells[4].InnerHtml.Trim()); 
            Assert.Equal("Bachelor", firstRowCells[5].InnerHtml.Trim());
            var expectedAge1 = _personManagementServiceMock.Object.CountPersonAge(candidates[0].personViewModel.BirthDate).ToString();
            Assert.Equal(expectedAge1, firstRowCells[6].InnerHtml.Trim()); 
            Assert.Equal("Party A", firstRowCells[7].InnerHtml.Trim()); 

            // Verify second candidate's data
            var secondRowCells = rows[1].QuerySelectorAll("td");
            Assert.Equal("2", secondRowCells[0].InnerHtml.Trim()); 
            Assert.Equal("Smith Jane", secondRowCells[1].InnerHtml.Trim()); 
            Assert.Equal("Doctor", secondRowCells[2].InnerHtml.Trim()); 
            Assert.Equal("Hospital B", secondRowCells[3].InnerHtml.Trim()); 
            Assert.Equal("City B", secondRowCells[4].InnerHtml.Trim()); 
            Assert.Equal("Master", secondRowCells[5].InnerHtml.Trim()); 
            var expectedAge2 = _personManagementServiceMock.Object.CountPersonAge(candidates[1].personViewModel.BirthDate).ToString();
            Assert.Equal(expectedAge2, secondRowCells[6].InnerHtml.Trim()); 
            Assert.Equal("Party B", secondRowCells[7].InnerHtml.Trim()); 

            // Verify filters rendering
            Assert.NotNull(cut.Find("div.filters"));
            var filterGroups = cut.Find("div.filters").QuerySelectorAll("div.filter-group");
            Assert.Equal(5, filterGroups.Count());

            // Verify election type filter options
            var selects = cut.FindAll("select");
            var electionTypeSelect = selects[0];
            var electionTypeOptions = electionTypeSelect.QuerySelectorAll("option");
            Assert.Contains(electionTypeOptions, option => option.InnerHtml.Contains("Wybierz rodzaj wyborów"));
            Assert.Contains(electionTypeOptions, option => option.InnerHtml.Contains("Presidential"));
            Assert.Contains(electionTypeOptions, option => option.InnerHtml.Contains("Parliamentary"));

        }
    }
}
