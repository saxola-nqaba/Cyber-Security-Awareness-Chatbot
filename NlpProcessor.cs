
// String manipulation and keyword matching:
//   Microsoft Docs — String.Contains:
//   https://learn.microsoft.com/en-us/dotnet/api/system.string.contains
//
// Regular expressions for pattern matching:
//   Microsoft Docs — Regex Class:
//   https://learn.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex
//
// DateTime parsing:
//   Microsoft Docs — DateTime.TryParse:
//   https://learn.microsoft.com/en-us/dotnet/api/system.datetime.tryparse

using System;
using System.Text.RegularExpressions;

namespace Cyber_Security_Awareness_Chatbot
{
    /// <summary>
    /// Natural Language Processing (NLP) simulation for the CyberGuard chatbot.
    /// Detects user intent from varied phrasings and parses task/quiz/log requests.
    /// Supports flexible keyword matching to minimise "I didn't understand" responses.
    /// </summary>
    public class NlpProcessor
    {
        //  Intent Detection 

        /// <summary>
        /// Enumerates the main types of user intent the chatbot can handle.
        /// Each intent corresponds to a specific chatbot action.
        /// </summary>
        public enum Intent
        {
            Unknown,          // Could not determine the user's request
            AddTask,          // User wants to create a new cybersecurity task
            DeleteTask,       // User wants to remove a task
            CompleteTask,     // User wants to mark a task as done
            ViewTasks,        // User wants to see their task list
            StartQuiz,        // User wants to start the quiz/mini-game
            ShowActivityLog,  // User wants to see what the bot has done
            GetCybersecurity, // User asking for general cybersecurity advice (Part 2)
            ChatResponse      // General conversation or greeting
        }

        //  Public Methods 

        /// <summary>
        /// Analyzes the user's input and determines their primary intent.
        /// Uses keyword matching to handle varied phrasings of the same request.
        /// </summary>
        /// <param name="userInput">The user's message or command.</param>
        /// <returns>The detected Intent (or Unknown if no match).</returns>
        public Intent DetectIntent(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return Intent.Unknown;

            string lower = userInput.ToLower().Trim();

            //  Task-Related Intent Detection 

            // Detect "Add Task" intent
            // Handles: "add a task", "create task", "remind me to...", "set a reminder", etc.
            if (ContainsKeywords(lower, new[] { "add", "task" }) ||
                ContainsKeywords(lower, new[] { "create", "task" }) ||
                ContainsKeywords(lower, new[] { "remind", "me" }) ||
                ContainsKeywords(lower, new[] { "set", "reminder" }) ||
                ContainsKeywords(lower, new[] { "add", "reminder" }))
            {
                return Intent.AddTask;
            }

            // Detect "Delete Task" intent
            // Handles: "delete task", "remove task", "remove reminder", etc.
            if (ContainsKeywords(lower, new[] { "delete", "task" }) ||
                ContainsKeywords(lower, new[] { "remove", "task" }) ||
                ContainsKeywords(lower, new[] { "delete", "reminder" }) ||
                lower.Contains("remove") && lower.Contains("task"))
            {
                return Intent.DeleteTask;
            }

            // Detect "Complete Task" intent
            // Handles: "mark done", "complete task", "finish task", "I'm done with...", etc.
            if (ContainsKeywords(lower, new[] { "mark", "done" }) ||
                ContainsKeywords(lower, new[] { "complete", "task" }) ||
                ContainsKeywords(lower, new[] { "finish", "task" }) ||
                ContainsKeywords(lower, new[] { "done", "task" }) ||
                lower.Contains("i'm done") ||
                lower.Contains("finished"))
            {
                return Intent.CompleteTask;
            }

            // Detect "View Tasks" intent
            // Handles: "show tasks", "list tasks", "what tasks do I have", etc.
            if (ContainsKeywords(lower, new[] { "show", "tasks" }) ||
                ContainsKeywords(lower, new[] { "list", "tasks" }) ||
                ContainsKeywords(lower, new[] { "view", "tasks" }) ||
                lower.Contains("what tasks"))
            {
                return Intent.ViewTasks;
            }

            //  Quiz-Related Intent Detection 

            // Detect "Start Quiz" intent
            // Handles: "start quiz", "play quiz", "quiz me", "test my knowledge", etc.
            if (ContainsKeywords(lower, new[] { "quiz" }) ||
                ContainsKeywords(lower, new[] { "start", "game" }) ||
                ContainsKeywords(lower, new[] { "play", "game" }) ||
                ContainsKeywords(lower, new[] { "test", "knowledge" }) ||
                lower.Contains("quiz me") ||
                lower.Contains("let's play"))
            {
                return Intent.StartQuiz;
            }

            // ── Activity Log Detection ────────────────────────────────────────

            // Detect "Show Activity Log" intent
            // Handles: "show log", "activity log", "what have you done", "history", etc.
            if (ContainsKeywords(lower, new[] { "activity", "log" }) ||
                ContainsKeywords(lower, new[] { "show", "log" }) ||
                lower.Contains("what have you done") ||
                lower.Contains("what have you done for me") ||
                lower.Contains("show me what"))
            {
                return Intent.ShowActivityLog;
            }

            //  Cybersecurity Topic Detection (Part 2) 

            // Detect general cybersecurity inquiries
            // Handles: "tell me about password", "how do i...", "explain phishing", etc.
            if (ContainsKeywords(lower, new[] { "password", "safety" }) ||
                ContainsKeywords(lower, new[] { "phishing", "email" }) ||
                ContainsKeywords(lower, new[] { "two-factor", "authentication" }) ||
                ContainsKeywords(lower, new[] { "mfa", "2fa" }) ||
                ContainsKeywords(lower, new[] { "scam", "phishing" }) ||
                ContainsKeywords(lower, new[] { "privacy", "data" }) ||
                ContainsKeywords(lower, new[] { "malware", "virus" }) ||
                ContainsKeywords(lower, new[] { "wifi", "network" }) ||
                lower.Contains("safe browsing") ||
                lower.Contains("social engineering"))
            {
                return Intent.GetCybersecurity;
            }

            //  Default to Chat Response 

            // If no specific intent detected, treat as general chat
            // This ensures the bot rarely responds with "I didn't understand"
            return Intent.ChatResponse;
        }

        //  Task Parsing Methods 

        /// <summary>
        /// Extracts the task title from a user's add-task request.
        /// Example: "Add a task to enable two-factor authentication" → "enable two-factor authentication"
        /// </summary>
        public string ExtractTaskTitle(string userInput)
        {
            string lower = userInput.ToLower();

            // Remove common prefixes
            string[] prefixes = { "add a task", "add task", "create task", "remind me to", "set reminder for", "add reminder to" };

            foreach (var prefix in prefixes)
            {
                if (lower.StartsWith(prefix))
                {
                    return userInput.Substring(prefix.Length).Trim();
                }
            }

            // If no prefix match, return the whole input (fallback)
            return userInput.Trim();
        }

        /// <summary>
        /// Attempts to extract a reminder date/time from the user's input.
        /// Examples:
        ///   "remind me in 3 days" → DateTime 3 days from now
        ///   "set reminder for tomorrow" → Tomorrow's date
        ///   "remind me next week" → 7 days from now
        /// </summary>
        public DateTime? ExtractReminderDate(string userInput)
        {
            string lower = userInput.ToLower();

            //  Today/Tomorrow 
            if (lower.Contains("today"))
                return DateTime.Now.Date.AddHours(17); // 5 PM today

            if (lower.Contains("tomorrow"))
                return DateTime.Now.Date.AddDays(1).AddHours(17); // 5 PM tomorrow

            //  Next X Days 
            var daysMatch = Regex.Match(lower, @"in\s+(\d+)\s+days?");
            if (daysMatch.Success && int.TryParse(daysMatch.Groups[1].Value, out int days))
                return DateTime.Now.AddDays(days);

            //  Next Week/Month 
            if (lower.Contains("next week"))
                return DateTime.Now.AddDays(7);

            if (lower.Contains("next month"))
                return DateTime.Now.AddDays(30);

            //  Specific Times 
            var timeMatch = Regex.Match(lower, @"(\d{1,2}):(\d{2})\s*(?:am|pm)?");
            if (timeMatch.Success)
            {
                // User mentioned a time, but we'll default to today at that time
                return DateTime.Now;  // Simplified; production would parse the actual time
            }

            // No valid date pattern found
            return null;
        }

        /// <summary>
        /// Extracts the task ID from a delete/complete request.
        /// Example: "delete task 5" → 5
        /// </summary>
        public int? ExtractTaskId(string userInput)
        {
            // Look for patterns like "task 5", "task#5", "id 5", etc.
            var match = Regex.Match(userInput, @"(?:task\s*#?|id\s*)(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int taskId))
                return taskId;

            return null;
        }

        //  Helper Methods 

        /// <summary>
        /// Checks if a string contains ALL of the specified keywords (case-insensitive).
        /// Useful for detecting multi-keyword phrases like "add task" or "remind me to".
        /// </summary>
        private bool ContainsKeywords(string input, string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                if (!input.Contains(keyword))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns a user-friendly description of the detected intent.
        /// Useful for logging or displaying what the bot understood.
        /// </summary>
        public static string IntentToString(Intent intent) => intent switch
        {
            Intent.AddTask => "Add new task",
            Intent.DeleteTask => "Delete task",
            Intent.CompleteTask => "Mark task complete",
            Intent.ViewTasks => "View task list",
            Intent.StartQuiz => "Start quiz",
            Intent.ShowActivityLog => "Show activity log",
            Intent.GetCybersecurity => "Cybersecurity advice",
            Intent.ChatResponse => "Chat/greeting",
            Intent.Unknown => "Unknown intent",
            _ => "Unknown"
        };
    }
}