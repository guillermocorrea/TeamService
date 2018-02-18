using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamService.Controllers;
using TeamService.LocationClient;
using TeamService.Models;
using TeamService.Persistence;
using Xunit;

[assembly: CollectionBehavior(MaxParallelThreads = 1)]
namespace TeamService.Tests
{
    public class MembersControllerTest
    {
        [Fact]
        public void CreateMemberAddsTeamToList()
        {
            ITeamRepository repository = new TestMemoryTeamRepository();
            ILocationClient _locationClient = new MemoryLocationClient();
            MembersController controller = new MembersController(repository, _locationClient);

            Guid teamId = Guid.NewGuid();
            Team team = new Team("TestController", teamId);
            repository.Add(team);

            Guid newMemberId = Guid.NewGuid();
            Member newMember = new Member(newMemberId);
            controller.CreateMember(newMember, teamId);

            team = repository.Get(teamId);
            Assert.True(team.Members.Contains(newMember));
        }

        [Fact]
        public void CreateMembertoNonexistantTeamReturnsNotFound()
        {
            ITeamRepository repository = new TestMemoryTeamRepository();
            ILocationClient _locationClient = new MemoryLocationClient();
            MembersController controller = new MembersController(repository, _locationClient);

            Guid teamId = Guid.NewGuid();

            Guid newMemberId = Guid.NewGuid();
            Member newMember = new Member(newMemberId);
            var result = controller.CreateMember(newMember, teamId);

            Assert.True(result is NotFoundResult);
        }

        [Fact]
        public async Task GetExistingMemberReturnsMember()
        {
            ITeamRepository repository = new TestMemoryTeamRepository();
            ILocationClient _locationClient = new MemoryLocationClient();
            MembersController controller = new MembersController(repository, _locationClient);

            Guid teamId = Guid.NewGuid();
            Team team = new Team("TestTeam", teamId);
            var debugTeam = repository.Add(team);

            Guid memberId = Guid.NewGuid();
            Member newMember = new Member(memberId);
            newMember.FirstName = "Jim";
            newMember.LastName = "Smith";
            controller.CreateMember(newMember, teamId);

            Member member = (Member)(await controller.GetMember(teamId, memberId) as ObjectResult).Value;
            Assert.Equal(member.ID, newMember.ID);
        }

        [Fact]
        public void GetMembersReturnsMembers()
        {
            ITeamRepository repository = new TestMemoryTeamRepository();
            ILocationClient _locationClient = new MemoryLocationClient();
            MembersController controller = new MembersController(repository, _locationClient);

            Guid teamId = Guid.NewGuid();
            Team team = new Team("TestTeam", teamId);
            var debugTeam = repository.Add(team);

            Guid firstMemberId = Guid.NewGuid();
            Member newMember = new Member(firstMemberId);
            newMember.FirstName = "Jim";
            newMember.LastName = "Smith";
            controller.CreateMember(newMember, teamId);

            Guid secondMemberId = Guid.NewGuid();
            newMember = new Member(secondMemberId);
            newMember.FirstName = "John";
            newMember.LastName = "Doe";
            controller.CreateMember(newMember, teamId);

            ICollection<Member> members = (ICollection<Member>)(controller.GetMembers(teamId) as ObjectResult).Value;
            Assert.Equal(2, members.Count());
            Assert.NotNull(members.Where(m => m.ID == firstMemberId).First().ID);
            Assert.NotNull(members.Where(m => m.ID == secondMemberId).First().ID);
        }

        [Fact]
        public void GetMembersForNewTeamIsEmpty()
        {
            ITeamRepository repository = new TestMemoryTeamRepository();
            ILocationClient _locationClient = new MemoryLocationClient();
            MembersController controller = new MembersController(repository, _locationClient);

            Guid teamId = Guid.NewGuid();
            Team team = new Team("TestTeam", teamId);
            var debugTeam = repository.Add(team);

            ICollection<Member> members = (ICollection<Member>)(controller.GetMembers(teamId) as ObjectResult).Value;
            Assert.Empty(members);
        }

        [Fact]
        public void GetMembersForNonExistantTeamReturnNotFound()
        {
            ITeamRepository repository = new TestMemoryTeamRepository();
            ILocationClient _locationClient = new MemoryLocationClient();
            MembersController controller = new MembersController(repository, _locationClient);

            var result = controller.GetMembers(Guid.NewGuid());
            Assert.True(result is NotFoundResult);
        }

        [Fact]
        public async Task GetNonExistantTeamReturnsNotFound()
        {
            ITeamRepository repository = new TestMemoryTeamRepository();
            ILocationClient _locationClient = new MemoryLocationClient();
            MembersController controller = new MembersController(repository, _locationClient);

            var result = await controller.GetMember(Guid.NewGuid(), Guid.NewGuid());
            Assert.True(result is NotFoundResult);
        }

        [Fact]
        public async Task GetNonExistantMemberReturnsNotFound()
        {
            ITeamRepository repository = new TestMemoryTeamRepository();
            ILocationClient _locationClient = new MemoryLocationClient();
            MembersController controller = new MembersController(repository, _locationClient);

            Guid teamId = Guid.NewGuid();
            Team team = new Team("TestTeam", teamId);
            var debugTeam = repository.Add(team);

            var result = await controller.GetMember(teamId, Guid.NewGuid());
            Assert.True(result is NotFoundResult);
        }

        [Fact]
        public void UpdateMemberOverwrites()
        {
            ITeamRepository repository = new TestMemoryTeamRepository();
            ILocationClient _locationClient = new MemoryLocationClient();
            MembersController controller = new MembersController(repository, _locationClient);

            Guid teamId = Guid.NewGuid();
            Team team = new Team("TestTeam", teamId);
            var debugTeam = repository.Add(team);

            Guid memberId = Guid.NewGuid();
            Member newMember = new Member(memberId);
            newMember.FirstName = "Jim";
            newMember.LastName = "Smith";
            controller.CreateMember(newMember, teamId);

            team = repository.Get(teamId);

            Member updatedMember = new Member(memberId);
            updatedMember.FirstName = "Bob";
            updatedMember.LastName = "Jones";
            controller.UpdateMember(updatedMember, teamId, memberId);

            team = repository.Get(teamId);
            Member testMember = team.Members.Where(m => m.ID == memberId).First();

            Assert.Equal(testMember.FirstName, "Bob");
            Assert.Equal(testMember.LastName, "Jones");
        }

        [Fact]
        public void UpdateMemberToNonexistantMemberReturnsNoMatch()
        {
            ITeamRepository repository = new TestMemoryTeamRepository();
            ILocationClient _locationClient = new MemoryLocationClient();
            MembersController controller = new MembersController(repository, _locationClient);

            Guid teamId = Guid.NewGuid();
            Team team = new Team("TestController", teamId);
            repository.Add(team);

            Guid memberId = Guid.NewGuid();
            Member newMember = new Member(memberId);
            newMember.FirstName = "Jim";
            controller.CreateMember(newMember, teamId);

            Guid nonMatchedGuid = Guid.NewGuid();
            Member updatedMember = new Member(nonMatchedGuid);
            updatedMember.FirstName = "Bob";
            var result = controller.UpdateMember(updatedMember, teamId, nonMatchedGuid);

            Assert.True(result is NotFoundResult);
        }

        [Fact]
        public void UpdateNonexistantMemberReturnsNoMatch()
        {
            ITeamRepository repository = new TestMemoryTeamRepository();
            ILocationClient _locationClient = new MemoryLocationClient();
            MembersController controller = new MembersController(repository, _locationClient);

            Guid teamId = Guid.NewGuid();
            Team team = new Team("TestController", teamId);
            repository.Add(team);

            Guid memberId = Guid.NewGuid();
            Member newMember = new Member(memberId);
            newMember.FirstName = "Jim";
            controller.CreateMember(newMember, teamId);

            Guid nonMatchedGuid = Guid.NewGuid();
            Member updatedMember = new Member(nonMatchedGuid);
            updatedMember.FirstName = "Bob";
            var result = controller.UpdateMember(updatedMember, Guid.NewGuid(), nonMatchedGuid);

            Assert.True(result is NotFoundResult);
        }

        [Fact]
        public void GetTeamForExistingMemberReturnsMember()
        {
            ITeamRepository repository = new TestMemoryTeamRepository();
            ILocationClient _locationClient = new MemoryLocationClient();
            MembersController controller = new MembersController(repository, _locationClient);

            Guid teamId = Guid.NewGuid();
            Team team = new Team("TestTeam", teamId);
            var debugTeam = repository.Add(team);

            Guid memberId = Guid.NewGuid();
            Member newMember = new Member(memberId);
            newMember.FirstName = "Jim";
            newMember.LastName = "Smith";
            controller.CreateMember(newMember, teamId);

            Team foundTeam = (Team)(controller.GetTeamForMember(memberId) as ObjectResult).Value;
            Assert.Equal(team.ID, foundTeam.ID);
        }

        [Fact]
        public void GetTeamForNonExistingMemberReturnsNotFound()
        {
            ITeamRepository repository = new TestMemoryTeamRepository();
            ILocationClient _locationClient = new MemoryLocationClient();
            MembersController controller = new MembersController(repository, _locationClient);

            Guid teamId = Guid.NewGuid();
            Team team = new Team("TestTeam", teamId);
            var debugTeam = repository.Add(team);

            Guid memberId = Guid.NewGuid();
            Member newMember = new Member(memberId);
            newMember.FirstName = "Jim";
            newMember.LastName = "Smith";
            controller.CreateMember(newMember, teamId);

            var result = (controller.GetTeamForMember(Guid.Empty) as IActionResult);
            Assert.True(result is NotFoundResult);
        }
    }
}
