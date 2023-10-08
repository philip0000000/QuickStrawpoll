namespace QuickStrawpoll.Shared.DTOs
{
    public class PollDTO
    {
        public string? PollId { get; set; }
        public string? Title { get; set; }

        public List<OptionDataDTO> Options { get; set; } = new List<OptionDataDTO>();
    }
}
