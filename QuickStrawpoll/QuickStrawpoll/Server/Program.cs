using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using QuickStrawpoll.Server.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Register DbContext
string connectionString = builder.Configuration["DATABASE_CONNECTION_STRING"];

builder.Services.AddDbContext<QuickStrawpollContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        // When connecting to a database, especially in cloud-hosted environments,
        // transient faults (temporary connection drops) can occur. Consider using
        // Polly in conjunction with EF Core to handle these.
        sqlOptions.EnableRetryOnFailure();
    }));

// Add HSTS options
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;  // Include subdomains in HSTS policy
    // If you're sure about preloading:
    // options.Preload = true;
});

// CORS setup
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("https://quickstrawpoll.azurewebsites.net")
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// App build
var app = builder.Build();

// For logging, to console
//builder.Logging.AddConsole();
//builder.Logging.SetMinimumLevel(LogLevel.Information);

// Custom error-handling middleware
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500; // or other status code as appropriate
        context.Response.ContentType = "application/json";

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionHandlerPathFeature?.Error is ArgumentNullException)
        {
            context.Response.StatusCode = 400; // Bad Request for argument null exception
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
            {
                StatusCode = 400,
                Message = exceptionHandlerPathFeature.Error.Message
            }));
        }
        // Handle other exceptions if needed...
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}

// HSTS and HTTPS redirection
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseCors();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

/*
--- 1. Info ---
 * Project type: Hosted Blazor WebAssembly
 * Clone of https://strawpoll.com/

--- 2. Data tables ---
 * The QuickStrawpoll.Polls table contains all the poll options.
 * QuickStrawpoll.OptionDatas contains the option data for ALL polls, with each row representing one option.
 * The PRIMARY KEY is 11 characters long and a combination of a to z, A to Z and 0 to 9. 36 character total.

CREATE SCHEMA QuickStrawpoll;

CREATE TABLE QuickStrawpoll.Polls (
    PollId NVARCHAR(11) PRIMARY KEY,    -- This is your unique 11-character hash.
    Title NVARCHAR(255) NOT NULL,       -- Store the poll title.
    NumOfOptions INT NOT NULL DEFAULT 0, -- This will store how many options a particular poll has.
    AllowMultipleSelections BIT NOT NULL DEFAULT 0, -- Boolean (true/false) to indicate if multiple selections are allowed.
    RequireParticipantsNames BIT NOT NULL DEFAULT 0, -- Boolean to indicate if participants' names are required.
    UseGoogleReCAPTCHA BIT NOT NULL DEFAULT 0,       -- Boolean to indicate if Google reCAPTCHA is used.
    VotingMethod NVARCHAR(50) NOT NULL DEFAULT 'ipAddress' -- The method of voting (ipAddress, browserSession, etc.).
);

CREATE TABLE QuickStrawpoll.OptionDatas (
    DataId INT IDENTITY(1,1) PRIMARY KEY,  -- Auto-incrementing integer as the unique identifier.
    PollId NVARCHAR(11) NOT NULL REFERENCES QuickStrawpoll.Polls(PollId), -- Foreign key linking to the Polls table.
    OptionId INT NOT NULL,                   -- Identifier ranging from 1 to NumOfOptions for each poll.
    OptionText NVARCHAR(255) NOT NULL,    -- The text of the option.
    VoteValue INT NOT NULL DEFAULT 0   -- Integer representing the vote count or value for this option.
);

private static Random random = new Random();
public static string GenerateUniqueHash(int length)
{
    // ensure that before saving to the database,
    // check if the hash already exists and if by the rare chance it does, regenerate it

    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());
}

--- 3. test value ---
-- Poll 1
INSERT INTO QuickStrawpoll.Polls (PollId, Title, NumOfOptions, AllowMultipleSelections, RequireParticipantsNames, UseGoogleReCAPTCHA, VotingMethod)
VALUES ('POLL001', 'Favorite Color?', 4, 0, 0, 1, 'ipAddress');

-- Option Data for Poll 1
INSERT INTO QuickStrawpoll.OptionDatas (PollId, OptionId, OptionText, VoteValue)
VALUES ('POLL001', 1, 'Red', 0), 
       ('POLL001', 2, 'Blue', 0), 
       ('POLL001', 3, 'Green', 0), 
       ('POLL001', 4, 'Yellow', 0);

---------------------------------------------

-- Poll 2
INSERT INTO QuickStrawpoll.Polls (PollId, Title, NumOfOptions, AllowMultipleSelections, RequireParticipantsNames, UseGoogleReCAPTCHA, VotingMethod)
VALUES ('POLL002', 'Best Type of Pizza?', 3, 0, 0, 0, 'browserSession');

-- Option Data for Poll 2
INSERT INTO QuickStrawpoll.OptionDatas (PollId, OptionId, OptionText, VoteValue)
VALUES ('POLL002', 1, 'Cheese', 0), 
       ('POLL002', 2, 'Pepperoni', 0), 
       ('POLL002', 3, 'Veggie', 0);

---------------------------------------------

-- Poll 3
INSERT INTO QuickStrawpoll.Polls (PollId, Title, NumOfOptions, AllowMultipleSelections, RequireParticipantsNames, UseGoogleReCAPTCHA, VotingMethod)
VALUES ('POLL003', 'Favorite Fruit?', 4, 0, 0, 0, 'ipAddress');

-- Option Data for Poll 3
INSERT INTO QuickStrawpoll.OptionDatas (PollId, OptionId, OptionText, VoteValue)
VALUES ('POLL003', 1, 'Apple', 0), 
       ('POLL003', 2, 'Banana', 0), 
       ('POLL003', 3, 'Orange', 0), 
       ('POLL003', 4, 'Grapes', 0);

---------------------------------------------

-- Poll 4
INSERT INTO QuickStrawpoll.Polls (PollId, Title, NumOfOptions, AllowMultipleSelections, RequireParticipantsNames, UseGoogleReCAPTCHA, VotingMethod)
VALUES ('POLL004', 'Which Animal Do You Like the Most?', 4, 0, 0, 0, 'browserSession');

-- Option Data for Poll 4
INSERT INTO QuickStrawpoll.OptionDatas (PollId, OptionId, OptionText, VoteValue)
VALUES ('POLL004', 1, 'Elephant', 0), 
       ('POLL004', 2, 'Tiger', 0), 
       ('POLL004', 3, 'Dolphin', 0), 
       ('POLL004', 4, 'Owl', 0);

---------------------------------------------

-- Poll 5
INSERT INTO QuickStrawpoll.Polls (PollId, Title, NumOfOptions, AllowMultipleSelections, RequireParticipantsNames, UseGoogleReCAPTCHA, VotingMethod)
VALUES ('POLL005', 'Favorite Season of the Year?', 4, 0, 0, 1, 'ipAddress');

-- Option Data for Poll 5
INSERT INTO QuickStrawpoll.OptionDatas (PollId, OptionId, OptionText, VoteValue)
VALUES ('POLL005', 1, 'Spring', 0), 
       ('POLL005', 2, 'Summer', 0), 
       ('POLL005', 3, 'Autumn', 0), 
       ('POLL005', 4, 'Winter', 0);

*/
