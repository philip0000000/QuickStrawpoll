using System.ComponentModel.DataAnnotations;

namespace QuickStrawpoll.Shared.DTOs
{
    public class PollCreationRequest
    {
        [Required]
        [StringLength(255, MinimumLength = 1)]
        public string? Title { get; set; }

        [Required]
        public List<string>? Options { get; set; }

        public bool AllowMultipleSelections { get; set; }
        public bool RequireParticipantsNames { get; set; }
        public bool UseGoogleReCAPTCHA { get; set; }

        [Required]
        public string? VotingMethod { get; set; }
    }
}
