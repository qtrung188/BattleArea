namespace BattleArenaBackendAPI.DTOs
{
    public class StartMatchRequest
    {
        public Guid UserId { get; set; }
    }


    public class submitScoreRequest
    {
        public Guid UserId { get; set; }
        public int Score { get; set; }
    }
}
