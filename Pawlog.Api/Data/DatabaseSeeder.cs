using Pawlog.Api.Models;

namespace Pawlog.Api.Data;

public static class DatabaseSeeder
{
    public static void Seed(PawlogDbContext db)
    {
        db.Database.EnsureCreated();

        if (db.Entries.Any())
        {
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);

        db.Entries.AddRange(
            new DiaryEntry
            {
                Date = today,
                Title = "First swim of the autumn",
                Story = "Stood in the shallows for ten minutes before deciding the water was acceptable, then refused to come back out.",
                TrainingGoal = "Come back on recall near water",
                GoalCompleted = false
            },
            new DiaryEntry
            {
                Date = today.AddDays(-1),
                Title = "Met the neighbour's cat",
                Story = "No barking this time. She sat down and waited until the cat lost interest.",
                TrainingGoal = "Stay calm around other animals",
                GoalCompleted = true
            });

        db.SaveChanges();
    }
}
