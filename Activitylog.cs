
// Queue<T> for fixed-size FIFO collection:
//   Microsoft Docs — Queue<T> Class:
//   https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.queue-1
//
//
// List<T>.GetRange for slicing recent entries:
//   Microsoft Docs — List<T>.GetRange:
//   https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1.getrange

using System;
using System.Collections.Generic;

namespace Cyber_Security_Awareness_Chatbot
{
    /// <summary>
    /// Category labels used to classify each log entry.
    /// Makes it easy to filter or colour-code entries in the GUI.
    /// </summary>
    public enum LogCategory
    {
        Task,       // Task added, completed, or deleted
        Reminder,   // Reminder set on a task
        Quiz,       // Quiz started, answered, or completed
        NLP,        // Natural-language command recognised
        Chat        // General chatbot interactions
    }

    // Log Entry Model

    /// <summary>
    /// Represents one recorded action in the activity log.
    /// Immutable after creation — the log is append-only.
    /// </summary>
    public class LogEntry
    {
        /// <summary>When the action occurred (local time).</summary>
        public DateTime Timestamp { get; }

        /// <summary>What type of action this is.</summary>
        public LogCategory Category { get; }

        /// <summary>Human-readable description of the action.</summary>
        public string Description { get; }

        public LogEntry(LogCategory category, string description)
        {
            Timestamp = DateTime.Now;
            Category = category;
            Description = description;
        }

        /// <summary>
        /// Formatted one-liner for display in the chat window or log panel.
        /// e.g. "[14:32] TASK — Enable two-factor authentication added."
        /// </summary>
        public override string ToString()
        {
            string icon = Category switch
            {
                LogCategory.Task => "📋",
                LogCategory.Reminder => "🔔",
                LogCategory.Quiz => "🎯",
                LogCategory.NLP => "🧠",
                LogCategory.Chat => "💬",
                _ => "•"
            };

            return $"[{Timestamp:HH:mm}] {icon} {Category.ToString().ToUpper()} — {Description}";
        }
    }

    // Activity Log 

    /// <summary>
    /// In-memory append-only log of all significant chatbot actions.
    /// Capped at MaxEntries to keep memory usage bounded.
    /// The GUI can request the most recent N entries for display.
    /// </summary>
    public class ActivityLog
    {
        // Configuration 

        /// <summary>Maximum number of entries stored in memory.</summary>
        private const int MaxEntries = 100;

        /// <summary>How many entries to show when the user asks for the log.</summary>
        public const int DefaultDisplayCount = 10;

        //  Storage 

        /// <summary>
        /// Full log history. A List is used (rather than Queue) so we can
        /// easily slice the last N entries without converting.
        /// </summary>
        private readonly List<LogEntry> _entries = new();

        //  Public Methods 

        /// <summary>
        /// Appends a new entry to the log.
        /// If the log is full, the oldest entry is removed first (FIFO cap).
        /// </summary>
        public void Record(LogCategory category, string description)
        {
            if (_entries.Count >= MaxEntries)
                _entries.RemoveAt(0); // Drop the oldest entry

            _entries.Add(new LogEntry(category, description));
        }

        /// <summary>
        /// Returns the most recent <paramref name="count"/> entries in
        /// reverse-chronological order (newest first) for display.
        /// </summary>
        public List<LogEntry> GetRecent(int count = DefaultDisplayCount)
        {
            int start = Math.Max(0, _entries.Count - count);
            var slice = _entries.GetRange(start, _entries.Count - start);

            // Reverse so the newest entry appears at the top of the list
            slice.Reverse();
            return slice;
        }

        /// <summary>Returns all stored entries (newest first). Used for "Show more".</summary>
        public List<LogEntry> GetAll()
        {
            var all = new List<LogEntry>(_entries);
            all.Reverse();
            return all;
        }

        /// <summary>Total number of entries recorded in this session.</summary>
        public int TotalCount => _entries.Count;

        /// <summary>True if at least one entry has been recorded.</summary>
        public bool HasEntries => _entries.Count > 0;
    }
}