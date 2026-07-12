namespace BattleArenaBackendAPI.Services
{
    public interface IMatchService
    {
        Task<Guid> StartMatchAsync(Guid userId);
        Task SubmitScoreAsync(Guid userId, Guid matchId, int score);
    }
}
