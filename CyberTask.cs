
// C# auto-implemented properties:
//   Microsoft Docs — Auto-Implemented Properties:
//   https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/auto-implemented-properties
//
// Nullable reference types (DateTime?):
//   Microsoft Docs — Nullable value types:
//   https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-value-types

using System;

namespace Cyber_Security_Awareness_Chatbot
{
    /// <summary>
    /// Represents a single cybersecurity task created by the user.
    /// Maps directly to one row in the MySQL 'Tasks' table.
    /// </summary>
    public class CyberTask
    {
        //  Properties 

        /// <summary>Primary key from the database. 0 means not yet persisted.</summary>
        public int Id { get; set; }

        /// <summary>Short label for the task, e.g. "Enable two-factor authentication".</summary>
        public string Title { get; set; } = "";

        /// <summary>Longer explanation of what the task involves.</summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Optional date/time for a reminder.
        /// Null means the user did not request a reminder.
        /// </summary>
        public DateTime? ReminderDate { get; set; }

        /// <summary>True once the user marks the task as done.</summary>
        public bool IsCompleted { get; set; }

        /// <summary>UTC timestamp when the task was first created.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        //  Display Helpers 

        /// <summary>
        /// Returns a human-readable status label for use in the GUI.
        /// </summary>
        public string StatusLabel => IsCompleted ? "✅ Completed" : "⏳ Pending";

        /// <summary>
        /// Returns the reminder date as a friendly string, or "None" if not set.
        /// </summary>
        public string ReminderDisplay =>
            ReminderDate.HasValue
                ? ReminderDate.Value.ToString("dd MMM yyyy HH:mm")
                : "None";

        /// <summary>
        /// One-line summary used in the activity log and chat responses.
        /// </summary>
        public override string ToString() =>
            $"[{StatusLabel}] {Title}" +
            (ReminderDate.HasValue ? $" (Reminder: {ReminderDisplay})" : "");
    }
}