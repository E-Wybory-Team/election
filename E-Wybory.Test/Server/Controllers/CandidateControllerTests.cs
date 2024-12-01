using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Wybory.Test.Server.Controllers
{
    using Xunit;
    using Moq;
    using Microsoft.EntityFrameworkCore;
    using E_Wybory.Domain.Entities;
    using E_Wybory.Infrastructure.DbContext;
    using E_Wybory.Client.ViewModels;
    using E_Wybory.Controllers;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;
    using E_Wybory.Test.Server.Utils;

    public class CandidateControllerTests
    {
        private readonly Mock<ElectionDbContext> _mockContext;
        private readonly CandidateController _controller;

        public CandidateControllerTests()
        {
            _mockContext = new Mock<ElectionDbContext>();
            _controller = new CandidateController(_mockContext.Object);
        }

        [Fact]
        public async Task GetCandidates_ReturnsAllCandidates()
        {
            // Arrange
            var candidates = new List<Candidate>
            {
                new Candidate { IdCandidate = 1, JobType = "Job1" },
                new Candidate { IdCandidate = 2, JobType = "Job2" }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Candidate>>();
            mockSet.As<IQueryable<Candidate>>().Setup(m => m.Provider).Returns(candidates.Provider);
            mockSet.As<IQueryable<Candidate>>().Setup(m => m.Expression).Returns(candidates.Expression);
            mockSet.As<IQueryable<Candidate>>().Setup(m => m.ElementType).Returns(candidates.ElementType);
            mockSet.As<IQueryable<Candidate>>().Setup(m => m.GetEnumerator()).Returns(candidates.GetEnumerator());

            mockSet.As<IAsyncEnumerable<Candidate>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<System.Threading.CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Candidate>(candidates.GetEnumerator()));

            mockSet.As<IQueryable<Candidate>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Candidate>(candidates.Provider));

            _mockContext.Setup(c => c.Candidates).Returns(mockSet.Object);

            // Act
            var result = await _controller.GetCandidates();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Candidate>>>(result);
            var returnValue = Assert.IsType<List<Candidate>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetCandidate_ReturnsCandidateById()
        {
            // Arrange
            var candidate = new Candidate { IdCandidate = 1, JobType = "Job1" };
            var mockSet = new Mock<DbSet<Candidate>>();
            mockSet.Setup(m => m.FindAsync(1)).ReturnsAsync(candidate);

            _mockContext.Setup(c => c.Candidates).Returns(mockSet.Object);

            // Act
            var result = await _controller.GetCandidate(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Candidate>>(result);
            var returnValue = Assert.IsType<Candidate>(actionResult.Value);
            Assert.Equal(1, returnValue.IdCandidate);
        }

        [Fact]
        public async Task PutCandidate_UpdatesCandidate()
        {
            // Arrange
            var candidate = new Candidate
            {
                IdCandidate = 1,
                JobType = "Job1",
                PlaceOfResidence = "City A",
                PositionNumber = 1,
                IdPerson = 1,
                IdElection = 1
            };

            var candidateModel = new CandidateViewModel
            {
                IdCandidate = 1,
                JobType = "UpdatedJob",
                PlaceOfResidence = "City B",
                PositionNumber = 2,
                IdPerson = 1,
                IdElection = 1
            };

            var candidates = new List<Candidate> { candidate }.AsQueryable();

            var mockSet = new Mock<DbSet<Candidate>>();
            mockSet.As<IQueryable<Candidate>>().Setup(m => m.Provider).Returns(candidates.Provider);
            mockSet.As<IQueryable<Candidate>>().Setup(m => m.Expression).Returns(candidates.Expression);
            mockSet.As<IQueryable<Candidate>>().Setup(m => m.ElementType).Returns(candidates.ElementType);
            mockSet.As<IQueryable<Candidate>>().Setup(m => m.GetEnumerator()).Returns(candidates.GetEnumerator());

            mockSet.As<IAsyncEnumerable<Candidate>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<System.Threading.CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Candidate>(candidates.GetEnumerator()));

            mockSet.As<IQueryable<Candidate>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Candidate>(candidates.Provider));

            mockSet.Setup(m => m.FindAsync(1)).ReturnsAsync(candidate);

            _mockContext.Setup(c => c.Candidates).Returns(mockSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _controller.PutCandidate(1, candidateModel);

            // Assert
            var actionResult = Assert.IsAssignableFrom<IActionResult>(result);
            Assert.IsType<OkResult>(actionResult);

            // Verify that the candidate was updated correctly
            Assert.Equal("UpdatedJob", candidate.JobType);
            Assert.Equal("City B", candidate.PlaceOfResidence);
            Assert.Equal(2, candidate.PositionNumber);
            Assert.Equal(1, candidate.IdPerson);
            Assert.Equal(1, candidate.IdElection);

            // Verify that SaveChangesAsync was called once
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task PostCandidate_AddsNewCandidate()
        {
            // Arrange
            var candidateModel = new CandidateViewModel
            {
                IdCandidate = 1,
                JobType = "Engineer",
                PlaceOfResidence = "City A",
                PositionNumber = 1,
                IdPerson = 1,
                IdElection = 1
            };

            var candidates = new List<Candidate>
            {
                new Candidate { IdCandidate = 1, JobType = "Job1" }
            };

            var mockSet = new Mock<DbSet<Candidate>>();
            mockSet.As<IQueryable<Candidate>>().Setup(m => m.Provider).Returns(candidates.AsQueryable().Provider);
            mockSet.As<IQueryable<Candidate>>().Setup(m => m.Expression).Returns(candidates.AsQueryable().Expression);
            mockSet.As<IQueryable<Candidate>>().Setup(m => m.ElementType).Returns(candidates.AsQueryable().ElementType);
            mockSet.As<IQueryable<Candidate>>().Setup(m => m.GetEnumerator()).Returns(candidates.AsQueryable().GetEnumerator());

            mockSet.As<IAsyncEnumerable<Candidate>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<System.Threading.CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Candidate>(candidates.AsQueryable().GetEnumerator()));

            mockSet.As<IQueryable<Candidate>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Candidate>(candidates.AsQueryable().Provider));

            mockSet.Setup(m => m.Add(It.IsAny<Candidate>())).Callback<Candidate>((candidate) =>
            {
                candidate.IdCandidate = candidates.Max(c => c.IdCandidate) + 1;
                candidates.Add(candidate);
            });

            _mockContext.Setup(c => c.Candidates).Returns(mockSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _controller.PostCandidate(candidateModel);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Candidate>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsType<Candidate>(createdAtActionResult.Value);
            Assert.Equal(2, returnValue.IdCandidate);
        }

        [Fact]
        public async Task DeleteCandidate_DeletesCandidateById()
        {
            // Arrange
            var candidate = new Candidate { IdCandidate = 1, JobType = "Job1" };
            var mockSet = new Mock<DbSet<Candidate>>();
            mockSet.Setup(m => m.FindAsync(1)).ReturnsAsync(candidate);

            _mockContext.Setup(c => c.Candidates).Returns(mockSet.Object);

            // Act
            var result = await _controller.DeleteCandidate(1);

            // Assert
            var actionResult = Assert.IsAssignableFrom<IActionResult>(result);
            Assert.IsType<NoContentResult>(actionResult);
        }

        [Fact]
        public async Task IfCandidateExists_ReturnsCandidateExistence()
        {
            // Arrange
            var candidates = new List<Candidate>
            {
                new Candidate { IdCandidate = 1, IdElection = 1, IdDistrict = 1, JobType = "Job1" }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Candidate>>();
            mockSet.As<IQueryable<Candidate>>().Setup(m => m.Provider).Returns(candidates.Provider);
            mockSet.As<IQueryable<Candidate>>().Setup(m => m.Expression).Returns(candidates.Expression);
            mockSet.As<IQueryable<Candidate>>().Setup(m => m.ElementType).Returns(candidates.ElementType);
            mockSet.As<IQueryable<Candidate>>().Setup(m => m.GetEnumerator()).Returns(candidates.GetEnumerator());

            mockSet.As<IAsyncEnumerable<Candidate>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<System.Threading.CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Candidate>(candidates.GetEnumerator()));

            mockSet.As<IQueryable<Candidate>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Candidate>(candidates.Provider));

            _mockContext.Setup(c => c.Candidates).Returns(mockSet.Object);

            // Act
            var result = await _controller.IfCandidateExists(1);

            // Assert
            var actionResult = Assert.IsType<OkResult>(result);
            Assert.NotNull(actionResult);
        }

        [Fact]
        public async Task GetCandidatesByElectionIdAndDistrict_ReturnsFilteredCandidates()
        {
            // Arrange
            var candidates = new List<Candidate>
            {
                new Candidate { IdCandidate = 1, IdElection = 1, IdDistrict = 1, JobType = "Job1" },
                new Candidate { IdCandidate = 2, IdElection = 1, IdDistrict = 2, JobType = "Job2" }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Candidate>>();
            mockSet.As<IQueryable<Candidate>>().Setup(m => m.Provider).Returns(candidates.Provider);
            mockSet.As<IQueryable<Candidate>>().Setup(m => m.Expression).Returns(candidates.Expression);
            mockSet.As<IQueryable<Candidate>>().Setup(m => m.ElementType).Returns(candidates.ElementType);
            mockSet.As<IQueryable<Candidate>>().Setup(m => m.GetEnumerator()).Returns(candidates.GetEnumerator());

            mockSet.As<IAsyncEnumerable<Candidate>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<System.Threading.CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Candidate>(candidates.GetEnumerator()));

            mockSet.As<IQueryable<Candidate>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Candidate>(candidates.Provider));

            _mockContext.Setup(c => c.Candidates).Returns(mockSet.Object);

            // Act
            var result = await _controller.GetCandidatesByElectionIdAndDistrict(1, 1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<List<CandidateViewModel>>>(result);
            var returnValue = Assert.IsType<List<CandidateViewModel>>(actionResult.Value);
            Assert.Single(returnValue);
        }
    }
}
