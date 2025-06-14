using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Web.Controllers;

[ApiController]
[Route("/api/migration/[action]")]
public class MigrationController
{
    private readonly IRepository<Competition> _competitions;
    private readonly IRepository<Pilot> _pilots;

    public MigrationController(IRepository<Competition> competitions, IRepository<Pilot> pilots)
    {
        _competitions = competitions;
        _pilots = pilots;
    }

    [HttpGet("/api/migration/streaks")]
    public async Task Streaks()
    {
        var competitions = await _competitions
            .GetAll()
            .OrderBy(c => c.StartedOn)
            .Where(c => c.State == CompetitionState.Closed)
            .ToListAsync();

        var pilotList = new List<Pilot>();

        foreach (var comp in competitions)
        {
            ProcessCompetition(comp, pilotList);
        }

        foreach (var pilotToUpdate in pilotList)
        {
            await UpdatePilotAsync(pilotToUpdate);
        }

        await _pilots.SaveChangesAsync();
    }

    private void ProcessCompetition(Competition comp, List<Pilot> pilotList)
    {
        var today = comp.StartedOn.AddDays(1);

        var pilotNames = comp.CompetitionResults
            .Select(x => x.PlayerName)
            .ToList();

        var pilotsSkippedDay = pilotList
            .Where(p => !pilotNames.Contains(p.Name))
            .ToList();

        foreach (var pilot in pilotsSkippedDay)
        {
            pilot.ResetDayStreak(today);
        }

        foreach (var pilotName in pilotNames)
        {
            var listed = pilotList.FirstOrDefault(x => x.Name == pilotName);

            if (listed is null)
            {
                pilotList.Add(new Pilot
                {
                    Name = pilotName,
                    DayStreak = 1,
                    DayStreakFreezes = new List<DayStreakFreeze>()
                });
            }
            else
            {
                listed.OnRaceFlown(today);
            }
        }
    }

    private async Task UpdatePilotAsync(Pilot pilotToUpdate)
    {
        var pilot = await _pilots.FindAsync(pilotToUpdate.Name);
        pilot.DayStreak = pilotToUpdate.DayStreak;
        pilot.MaxDayStreak = pilotToUpdate.MaxDayStreak;
        pilot.DayStreakFreezes.Clear();

        foreach (var freezie in pilotToUpdate.DayStreakFreezes)
        {
            pilot.DayStreakFreezes.Add(new DayStreakFreeze(freezie.CreatedOn)
            {
                SpentOn = freezie.SpentOn
            });
        }
    }
}
