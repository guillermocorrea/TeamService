using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamService.Models;
using TeamService.Persistence;

namespace TeamService.Controllers
{
    [Route("/teams/{teamId}/[controller]")]
    public class MembersController : Controller
    {
        ITeamRepository repository;

        public MembersController(ITeamRepository repo)
        {
            repository = repo;
        }

        [HttpGet]
        public virtual IActionResult GetMembers(Guid teamID)
        {
            Team team = repository.Get(teamID);

            if (team == null)
            {
                return this.NotFound();
            }
            else
            {
                return this.Ok(team.Members);
            }
        }


        [HttpGet]
        [Route("/teams/{teamId}/[controller]/{memberId}")]
        public virtual IActionResult GetMember(Guid teamID, Guid memberId)
        {
            Team team = repository.Get(teamID);

            if (team == null)
            {
                return this.NotFound();
            }
            else
            {
                var q = team.Members.Where(m => m.ID == memberId);

                if (q.Count() < 1)
                {
                    return this.NotFound();
                }
                else
                {
                    return this.Ok(q.First());
                }
            }
        }

        [HttpPut]
        [Route("/teams/{teamId}/[controller]/{memberId}")]
        public virtual IActionResult UpdateMember([FromBody]Member updatedMember, Guid teamID, Guid memberId)
        {
            Team team = repository.Get(teamID);

            if (team == null)
            {
                return this.NotFound();
            }
            else
            {
                var q = team.Members.Where(m => m.ID == memberId);

                if (q.Count() < 1)
                {
                    return this.NotFound();
                }
                else
                {
                    team.Members.Remove(q.First());
                    team.Members.Add(updatedMember);
                    return this.Ok();
                }
            }
        }

        [HttpPost]
        public virtual IActionResult CreateMember([FromBody]Member newMember, Guid teamID)
        {
            Team team = repository.Get(teamID);

            if (team == null)
            {
                return this.NotFound();
            }
            else
            {
                team.Members.Add(newMember);
                var teamMember = new { TeamID = team.ID, MemberID = newMember.ID };
                return this.Created($"/teams/{teamMember.TeamID}/[controller]/{teamMember.MemberID}", teamMember);
            }
        }

        [HttpGet]
        [Route("/members/{memberId}/team")]
        public IActionResult GetTeamForMember(Guid memberId)
        {
            var team = GetTeamForMemberHelper(memberId);
            if (team != null)
            {
                return this.Ok(team);
            }
            else
            {
                return this.NotFound();
            }
        }

        private Team GetTeamForMemberHelper(Guid memberId)
        {
            foreach (var team in repository.List())
            {
                var member = team.Members.FirstOrDefault(m => m.ID == memberId);
                if (member != null)
                {
                    return team;
                }
            }
            return null;
        }
    }
}
