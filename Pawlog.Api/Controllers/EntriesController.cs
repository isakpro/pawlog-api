using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pawlog.Api.Data;
using Pawlog.Api.Models;

namespace Pawlog.Api.Controllers;

[ApiController]
[Route("api/entries")]
public class EntriesController(PawlogDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DiaryEntry>>> GetAll()
    {
        var entries = await db.Entries
            .OrderByDescending(entry => entry.Date)
            .ThenByDescending(entry => entry.Id)
            .ToListAsync();

        return Ok(entries);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DiaryEntry>> GetById(int id)
    {
        var entry = await db.Entries.FindAsync(id);

        return entry is null ? NotFound() : Ok(entry);
    }

    [HttpPost]
    public async Task<ActionResult<DiaryEntry>> Create(EntryRequest request)
    {
        var entry = new DiaryEntry
        {
            Date = request.Date,
            Title = request.Title,
            Story = request.Story,
            TrainingGoal = request.TrainingGoal,
            GoalCompleted = request.GoalCompleted
        };

        db.Entries.Add(entry);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, entry);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DiaryEntry>> Update(int id, EntryRequest request)
    {
        var entry = await db.Entries.FindAsync(id);

        if (entry is null)
        {
            return NotFound();
        }

        entry.Date = request.Date;
        entry.Title = request.Title;
        entry.Story = request.Story;
        entry.TrainingGoal = request.TrainingGoal;
        entry.GoalCompleted = request.GoalCompleted;

        await db.SaveChangesAsync();

        return Ok(entry);
    }
}
