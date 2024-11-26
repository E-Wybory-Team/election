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
            var candidate = new Candidate { IdCandidate = 1, JobType = "Job1" };
            var candidateModel = new CandidateViewModel { IdCandidate = 1, JobType = "UpdatedJob" };

            var mockSet = new Mock<DbSet<Candidate>>();
            mockSet.Setup(m => m.FindAsync(1)).ReturnsAsync(candidate);

            _mockContext.Setup(c => c.Candidates).Returns(mockSet.Object);

            // Act
            var result = await _controller.PutCandidate(1, candidateModel);

            // Assert
            var actionResult = Assert.IsType<IActionResult>(result);
            Assert.IsType<NoContentResult>(actionResult);
        }

        [Fact]
        public async Task PostCandidate_AddsNewCandidate()
        {
            // Arrange
            var candidateModel = new CandidateViewModel { IdCandidate = 1, JobType = "NewJob" };

            var mockSet = new Mock<DbSet<Candidate>>();
            _mockContext.Setup(c => c.Candidates).Returns(mockSet.Object);

            // Act
            var result = await _controller.PostCandidate(candidateModel);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Candidate>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsType<Candidate>(createdAtActionResult.Value);
            Assert.Equal(1, returnValue.IdCandidate);
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
            var actionResult = Assert.IsType<IActionResult>(result);
            Assert.IsType<NoContentResult>(actionResult);
        }

        [Fact]
        public async Task IfCandidateExists_ReturnsCandidateExistence()
        {
            // Arrange
            var candidate = new Candidate { IdCandidate = 1, JobType = "Job1" };
            var mockSet = new Mock<DbSet<Candidate>>();
            mockSet.Setup(m => m.FindAsync(1)).ReturnsAsync(candidate);

            _mockContext.Setup(c => c.Candidates).Returns(mockSet.Object);

            // Act
            var result = await _controller.IfCandidateExists(1);

            // Assert
            var actionResult = Assert.IsType<IActionResult>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.True((bool)okResult.Value);
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
