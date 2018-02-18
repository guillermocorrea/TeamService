using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamService.LocationClient;
using TeamService.Models;
using TeamService.Persistence;

namespace TeamService.Controllers
{
    [Route("/teams/{teamId}/[controller]")]
    public class MembersController : Controller
    {
        //private readonly IMapper _mapper;
        private readonly ITeamRepository _repository;
        private readonly ILocationClient _locationClient;

        public MembersController(ITeamRepository repo, //IMapper mapper,
            ILocationClient locationClient)
        {
            _repository = repo;
            //_mapper = mapper;
            _locationClient = locationClient;
        }

        [HttpGet]
        public virtual IActionResult GetMembers(Guid teamID)
        {
            Team team = _repository.Get(teamID);

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
        public async virtual Task<IActionResult> GetMember(Guid teamID, Guid memberId)
        {
            Team team = _repository.Get(teamID);

            if (team == null)
            {
                return NotFound();
            }
            else
            {
                var q = team.Members.Where(m => m.ID == memberId);

                if (q.Count() < 1)
                {
                    return NotFound();
                }
                else
                {
                    //var member =_mapper.Map<LocatedMember>(q.First());
                    //locatedMember.LastLocation = await _locationClient.GetLatestForMember(locatedMember.ID);
                    var member = q.First();
                    var locatedMember = new LocatedMember()
                    {
                        ID = member.ID,
                        FirstName = member.FirstName,
                        LastName = member.LastName,
                        LastLocation = await _locationClient.GetLatestForMember(member.ID)
                    };
                    return Ok(locatedMember);
                }
            }
        }

        [HttpPut]
        [Route("/teams/{teamId}/[controller]/{memberId}")]
        public virtual IActionResult UpdateMember([FromBody]Member updatedMember, Guid teamID, Guid memberId)
        {
            Team team = _repository.Get(teamID);

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
            Team team = _repository.Get(teamID);

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
            foreach (var team in _repository.List())
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
