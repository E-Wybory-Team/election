using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using E_Wybory.Client.Components.Pages;
using E_Wybory.Client.Services;
using Moq;
using E_Wybory.Client.FilterData;
using E_Wybory.Client.ViewModels;
using System.Collections.Generic;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class CandidateInfoTests : TestContext
    {
        private readonly Mock<IFilterWrapperManagementService> _filterWrapperServiceMock;
        private readonly Mock<IPartyManagementService> _partyManagementServiceMock;
        private readonly Mock<IElectionTypeManagementService> _electionTypeManagementServiceMock;
        private readonly Mock<IPersonManagementService> _personManagementServiceMock;

        public CandidateInfoTests()
        {
            _filterWrapperServiceMock = new Mock<IFilterWrapperManagementService>();
            _partyManagementServiceMock = new Mock<IPartyManagementService>();
            _electionTypeManagementServiceMock = new Mock<IElectionTypeManagementService>();
            _personManagementServiceMock = new Mock<IPersonManagementService>();

            _filterWrapperServiceMock.Setup(service => service.GetFilteredCandidates(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<CandidatePersonViewModel>
                {
                    new CandidatePersonViewModel
                    {
                        personViewModel = new PersonViewModel
                        {
                            Name = "Jan",
                            Surname = "Kowalski",
                            BirthDate = new System.DateTime(1985, 5, 10)
                        },
                        candidateViewModel = new CandidateViewModel
                        {
                            PositionNumber = 1,
                            JobType = "Inżynier",
                            Workplace = "Firma A",
                            PlaceOfResidence = "Warszawa",
                            EducationStatus = "Wyższe",
                            IdParty = 1
                        }
                    },
                    new CandidatePersonViewModel
                    {
                        personViewModel = new PersonViewModel
                        {
                            Name = "Anna",
                            Surname = "Nowak",
                            BirthDate = new System.DateTime(1990, 8, 15)
                        },
                        candidateViewModel = new CandidateViewModel
                        {
                            PositionNumber = 2,
                            JobType = "Lekarz",
                            Workplace = "Szpital B",
                            PlaceOfResidence = "Kraków",
                            EducationStatus = "Wyższe",
                            IdParty = 2
                        }
                    }
                });

            _filterWrapperServiceMock.Setup(service => service.GetFilteredLists(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new FilterListWrapperFull
                {
                    ElectionFilter = new List<ElectionTypeViewModel>
                    {
                        new ElectionTypeViewModel { IdElectionType = 1, ElectionTypeName = "Parlamentarne" },
                        new ElectionTypeViewModel { IdElectionType = 2, ElectionTypeName = "Samorządowe" }
                    },
                    FilterListWrapper = new FilterListWrapper
                    {
                        VoivodeshipFilter = new List<VoivodeshipViewModel>(),
                        CountyFilter = new List<CountyViewModel>(),
                        ProvinceFilter = new List<ProvinceViewModel>(),
                        DistrictFilter = new List<DistrictViewModel>()
                    }
                });

            _partyManagementServiceMock.Setup(service => service.Parties())
                .ReturnsAsync(new List<PartyViewModel>
                {
                    new PartyViewModel { IdParty = 1, PartyName = "Partia A" },
                    new PartyViewModel { IdParty = 2, PartyName = "Partia B" }
                });

            _partyManagementServiceMock.Setup(service => service.GetPartyNameById(It.Is<int>(id => id == 1), It.IsAny<List<PartyViewModel>>()))
                .Returns("Partia A");

            _partyManagementServiceMock.Setup(service => service.GetPartyNameById(It.Is<int>(id => id == 2), It.IsAny<List<PartyViewModel>>()))
                .Returns("Partia B");

            _electionTypeManagementServiceMock.Setup(service => service.ElectionTypes())
                .ReturnsAsync(new List<ElectionTypeViewModel>
                {
                    new ElectionTypeViewModel { IdElectionType = 1, ElectionTypeName = "Parlamentarne" },
                    new ElectionTypeViewModel { IdElectionType = 2, ElectionTypeName = "Samorządowe" }
                });

            _personManagementServiceMock.Setup(service => service.CountPersonAge(It.IsAny<System.DateTime>()))
                .Returns(36);

            Services.AddSingleton(_filterWrapperServiceMock.Object);
            Services.AddSingleton(_partyManagementServiceMock.Object);
            Services.AddSingleton(_electionTypeManagementServiceMock.Object);
            Services.AddSingleton(_personManagementServiceMock.Object);

           }

        [Fact]
        public void CandidateInfo_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<CandidateInfo>();



            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("SPIS KANDYDATÓW", cut.Markup);
                Assert.Contains("Rodzaj wyborów", cut.Markup);
                Assert.Contains("Województwo", cut.Markup);
                Assert.Contains("Powiat", cut.Markup);
                Assert.Contains("Gmina", cut.Markup);
                Assert.Contains("Numer obwodu", cut.Markup);
                Assert.Contains("Nazwisko i imiona", cut.Markup);
            });
        }

        [Fact]
        public async Task CandidateInfo_Should_Display_Filtered_Candidates()
        {
            // Act
            var cut = RenderComponent<CandidateInfo>();


            var electionSelect = cut.Find("select#rodzaj-wyborow");
            await cut.InvokeAsync(() => electionSelect.Change("1"));

            var voivodeshipSelect = cut.Find("select#wojewodztwo");
            await cut.InvokeAsync(() => voivodeshipSelect.Change("1"));

            var countySelect = cut.Find("select#powiat");
            await cut.InvokeAsync(() => countySelect.Change("1"));

            var provinceSelect = cut.Find("select#gmina");
            await cut.InvokeAsync(() => provinceSelect.Change("1"));

            var districtSelect = cut.Find("select#numer-obwodu");
            await cut.InvokeAsync(() => districtSelect.Change("1"));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Console.WriteLine(cut.Markup);
                Assert.Contains("Kowalski Jan", cut.Markup);
                Assert.Contains("Inżynier", cut.Markup);
                Assert.Contains("Firma A", cut.Markup);
                Assert.Contains("Warszawa", cut.Markup);
                Assert.Contains("Wyższe", cut.Markup);
                Assert.Contains("Partia A", cut.Markup);

                Assert.Contains("Nowak Anna", cut.Markup);
                Assert.Contains("Lekarz", cut.Markup);
                Assert.Contains("Szpital B", cut.Markup);
                Assert.Contains("Kraków", cut.Markup);
                Assert.Contains("Partia B", cut.Markup);
            });
        }

     
    }
}
