using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickStrawpoll.Server.Models
{
    public class OptionDatas
    {
        [Key]
        public int DataId { get; set; }
        public string? PollId { get; set; }
        public int? OptionId { get; set; }
        public string? OptionText { get; set; }
        public int VoteValue { get; set; }

        [ForeignKey("PollId")]
        public Polls? Polls { get; set; }
    }
}
