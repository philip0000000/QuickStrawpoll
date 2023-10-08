using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickStrawpoll.Server.Data;
using QuickStrawpoll.Server.Models;
using QuickStrawpoll.Server.Utilities;
using QuickStrawpoll.Shared.DTOs;

namespace QuickStrawpoll.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PollsController : ControllerBase
    {
        private readonly QuickStrawpollContext _context;
        //private readonly ILogger<PollsController> _logger;

        //public PollsController(QuickStrawpollContext context, ILogger<PollsController> logger)
        public PollsController(QuickStrawpollContext context)
        {
            _context = context;
            //_logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePoll([FromBody] PollCreationRequest request)
        {
            // Model validation
            if (!ModelState.IsValid)
            {
                //_logger.LogWarning("Invalid model state for PollCreationRequest");
                return BadRequest(ModelState);
            }

            string generatedHash = HashUtility.GenerateUniqueHash(11);

            // Ensure the generated hash is unique
            while (await _context.Polls.AnyAsync(p => p.PollId == generatedHash))
            {
                generatedHash = HashUtility.GenerateUniqueHash(11);
            }

            List<string>? options = request.Options;
            if (options == null)
            {
                //_logger.LogError("Options were null in the PollCreationRequest");
                throw new ArgumentNullException(nameof(options), "Options must be provided to create a poll.");
            }

            // Convert options to OptionDatas and initialize a new Polls object.
            var optionDatasList = new List<OptionDatas>();
            for (int i = 0; i < options.Count; i++)
            {
                optionDatasList.Add(new OptionDatas
                {
                    OptionText = options[i],
                    OptionId = i + 1, // Value from 1 to NumOfOptions (options.Count)
                    VoteValue = 0
                });
            }

            var newPoll = new Polls
            {
                PollId = generatedHash,
                Title = request.Title,
                NumOfOptions = options.Count,
                AllowMultipleSelections = false,
                RequireParticipantsNames = false,
                UseGoogleReCAPTCHA = false,
                VotingMethod = "ipAddress",
                Options = optionDatasList
            };

            // Add to database
            _context.Polls.Add(newPoll);
            await _context.SaveChangesAsync();

            //_logger.LogInformation($"Poll with ID {newPoll.PollId} created successfully.");

            return Ok(newPoll.PollId); // return the created Poll ID or other appropriate response
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPolls()
        {
            var polls = await _context.Polls.ToListAsync();

            // Transforming Polls entities to PollDTO objects
            var pollsDTO = polls.Select(p => new PollDTO
            {
                PollId = p.PollId,
                Title = p.Title
            }).ToList();

            return Ok(pollsDTO);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPollById(string id)
        {
            var poll = await _context.Polls.Include(p => p.Options).FirstOrDefaultAsync(p => p.PollId == id);

            if (poll == null)
                return NotFound();

            var pollDTO = new PollDTO
            {
                PollId = poll.PollId,
                Title = poll.Title,
                Options = poll.Options.Select(o => new OptionDataDTO
                {
                    OptionText = o.OptionText,
                    VoteValue = o.VoteValue
                }).ToList()
            };

            return Ok(pollDTO);
        }

        [HttpPatch("updateVoteValue")]
        public async Task<IActionResult> UpdateVoteValue(string pollId, string optionText)
        {
            if (string.IsNullOrEmpty(pollId) || string.IsNullOrEmpty(optionText))
            {
                return BadRequest("PollId and OptionText are required.");
            }

            var option = await _context.OptionDatas
                               .Include(o => o.Polls) // Including related Polls data
                               .FirstOrDefaultAsync(o => o.Polls.PollId == pollId && o.OptionText == optionText);

            if (option == null)
            {
                return NotFound($"No option found with PollId: {pollId} and OptionText: {optionText}");
            }

            option.VoteValue += 1; // Incrementing the VoteValue by 1. Modify this logic if needed.

            _context.Update(option);
            await _context.SaveChangesAsync();

            return Ok("VoteValue updated successfully.");
        }

        [HttpDelete("{pollId}")]
        public async Task<IActionResult> DeletePoll(string pollId)
        {
            // Fetch the poll
            var poll = await _context.Polls.FindAsync(pollId);
            if (poll == null)
            {
                return NotFound();
            }

            // Remove related options
            var relatedOptions = _context.OptionDatas.Where(od => od.PollId == pollId);
            _context.OptionDatas.RemoveRange(relatedOptions);

            // Remove the poll
            _context.Polls.Remove(poll);

            await _context.SaveChangesAsync();

            return NoContent();  // Return a 204 No Content response to indicate successful deletion
        }
    }
}
