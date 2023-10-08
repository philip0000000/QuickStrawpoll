using System.ComponentModel.DataAnnotations;

namespace QuickStrawpoll.Server.Models
{
    public class Polls
    {
        [Key]
        public string? PollId { get; set; }
        public string? Title { get; set; }
        public int NumOfOptions { get; set; }
        public bool AllowMultipleSelections { get; set; }
        public bool RequireParticipantsNames { get; set; }
        public bool UseGoogleReCAPTCHA { get; set; }
        public string? VotingMethod { get; set; }

        public List<OptionDatas> Options { get; set; } = new List<OptionDatas>();
    }
}
