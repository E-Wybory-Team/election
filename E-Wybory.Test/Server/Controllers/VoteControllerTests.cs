using E_Wybory.Client.ViewModels;
using E_Wybory.Controllers;
using E_Wybory.Domain.Entities;
using E_Wybory.Infrastructure.DbContext;
using E_Wybory.Test.Server.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace E_Wybory.Test.Server.Controllers
{
    public class VoteControllerTests
    {
        private readonly Mock<ElectionDbContext> _mockContext;
        private readonly VoteController _controller;

        public VoteControllerTests()
        {
            _mockContext = new Mock<ElectionDbContext>();
            _controller = new VoteController(_mockContext.Object);
        }

        [Fact]
        public async Task GetVotes_ReturnsAllVotes()
        {
            // Arrange
            var votes = new List<Vote>
            {
                new Vote { IdVote = 1, IsValid = true, IdCandidate = 1, IdElection = 1, IdDistrict = 1 },
                new Vote { IdVote = 2, IsValid = false, IdCandidate = 2, IdElection = 1, IdDistrict = 1 }
            }.AsQueryable();

            var mockSet = MockDbSet.CreateMockDbSet(votes);
            _mockContext.Setup(c => c.Votes).Returns(mockSet.Object);

            // Act
            var result = await _controller.GetVotes();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Vote>>>(result);
            var returnValue = Assert.IsType<List<Vote>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetVote_ReturnsVoteById()
        {
            // Arrange
            int voteId = 1;
            var votes = new List<Vote>
            {
                new Vote { IdVote = voteId, IsValid = true, IdCandidate = 1, IdElection = 1, IdDistrict = 1 }
            }.AsQueryable();

            var mockSet = MockDbSet.CreateMockDbSet(votes);
            _mockContext.Setup(c => c.Votes).Returns(mockSet.Object);

            // Act
            var result = await _controller.GetVote(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Vote>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
            
            //Arrange
            _mockContext.Setup(m => m.Votes.FindAsync(voteId)).ReturnsAsync(votes.First(v => v.IdVote == voteId));

            //Act
            result = await _controller.GetVote(1);

            //Assert 
            actionResult = Assert.IsType<ActionResult<Vote>>(result);
            var returnValue = Assert.IsType<Vote>(actionResult.Value);
            Assert.Equal(1, returnValue.IdVote);
        }

        [Fact]
        public async Task PostVote_AddsNewVote()
        {
            // Arrange
            var voteModel = new VoteViewModel { IdElection = 1, IdCandidate = 1, IdDistrict = 1, IdVote = 1, IsValid = true };
            var votes = new List<Vote>().AsQueryable();

            var mockSet = MockDbSet.CreateMockDbSet(votes);
            _mockContext.Setup(c => c.Votes).Returns(mockSet.Object);

            // Act
            var result = await _controller.PostVote(voteModel);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Vote>>(result);
            var returnValue = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var resultValue = Assert.IsType<Vote>(returnValue.Value);
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        // Add more tests for other methods in VoteController
    }
}
