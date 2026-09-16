
// WPF code-behind pattern and InitializeComponent():
//   Microsoft Docs — WPF Code-Behind:
//   https://learn.microsoft.com/en-us/dotnet/desktop/wpf/xaml
//
// Dispatcher.Invoke for cross-thread UI updates:
//   Stack Overflow — Updating WPF UI from another thread:
//   https://stackoverflow.com/questions/9732709/the-calling-thread-cannot-access-this-object-because-a-different-thread-owns-it
//
// Dynamically creating WPF controls in code (Border, TextBlock, StackPanel):
//   Stack Overflow — Add controls dynamically in WPF:
//   https://stackoverflow.com/questions/2796470/wpf-create-a-border-programmatically
//
// ScrollViewer.ScrollToBottom() for auto-scrolling chat:
//   Stack Overflow — Auto scroll to bottom of WPF ListBox:
//   https://stackoverflow.com/questions/2006729/how-can-i-have-a-listbox-auto-scroll-when-a-new-item-is-added
//
//
// TabControl / TabItem usage in WPF:
//   Microsoft Docs — TabControl Class:
//   https://learn.microsoft.com/en-us/dotnet/api/system.windows.controls.tabcontrol
//
// DispatcherTimer for delayed UI actions:
//   Microsoft Docs — DispatcherTimer Class:
//   https://learn.microsoft.com/en-us/dotnet/api/system.windows.threading.dispatchertimer
//
// LINQ Take() and Select() for collection projection:
//   Microsoft Docs — Enumerable.Take:
//   https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.take
//   Microsoft Docs — Enumerable.Select:
//   https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.select

using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyber_Security_Awareness_Chatbot
{
    public partial class MainWindow : Window
    {
        //  Private Fields 

        /// <summary>The chatbot engine instance. Output is routed via AppendBotMessage delegate.</summary>
        private readonly ChatBot _chatBot;

        /// <summary>Handles WAV voice greeting playback and ASCII art conversion.</summary>
        private readonly GreetingManager _greetingManager;

        /// <summary>Simulated NLP layer — detects intent before falling back to ChatBot.</summary>
        private readonly NlpProcessor _nlpProcessor;

        /// <summary>Handles all MySQL task persistence (Part 3, Task 1).</summary>
        private readonly DatabaseManager _dbManager;

        /// <summary>Drives the cybersecurity mini-game (Part 3, Task 2).</summary>
        private readonly QuizEngine _quizEngine;

        /// <summary>Records every significant chatbot action (Part 3, Task 4).</summary>
        private readonly ActivityLog _activityLog;

        /// <summary>Tracks whether the user has entered their name yet.</summary>
        private bool _nameCollected = false;

        /// <summary>Stores the validated user name for use in message bubbles.</summary>
        private string _userName = "";

        /// <summary>How many activity log entries are currently being displayed ("Show more").</summary>
        private int _logDisplayCount = ActivityLog.DefaultDisplayCount;

        //  Constructor 

        public MainWindow()
        {
            InitializeComponent();

            _greetingManager = new GreetingManager();
            _chatBot = new ChatBot(AppendBotMessage);
            _nlpProcessor = new NlpProcessor();
            _dbManager = new DatabaseManager();
            _quizEngine = new QuizEngine();
            _activityLog = new ActivityLog();

            Loaded += MainWindow_Loaded;
        }

        //  Window Loaded 

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _greetingManager.PlayGreetingWithAsciiArt();

            // Note: ASCII banner row was removed from the v3.0 layout to make room
            // for the tab strip; the greeting/voice still plays as required by Part 1.

            AppendBotMessage("Welcome to CyberGuard Awareness Assistant!");
            AppendBotMessage("I'm here to help you stay safe in the digital world.");
            AppendBotMessage("Before we begin — what's your name?");

            InputBox.Focus();

            // Check the database connection early so the user/marker can see it's wired up
            bool dbOk = _dbManager.TestConnection();
            DbStatusLabel.Text = dbOk ? "  •  DB: CONNECTED" : "  •  DB: OFFLINE";
            DbStatusLabel.Foreground = dbOk
                ? new SolidColorBrush(Color.FromRgb(57, 255, 20))
                : new SolidColorBrush(Color.FromRgb(255, 80, 80));

            RefreshTaskList();
            RefreshActivityLogList();
        }

        //  Input Event Handlers (Chat tab) 

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ProcessInput();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessInput();
        }

        private void QuickTopic_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag)
            {
                if (!_nameCollected)
                {
                    AppendBotMessage("Please tell me your name first so I can personalise your experience!");
                    return;
                }

                InputBox.Text = tag;
                ProcessInput();
            }
        }

        //  Core Input Processing 

        private void ProcessInput()
        {
            string rawInput = InputBox.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(rawInput)) return;

            InputBox.Clear();
            AppendUserMessage(rawInput);

            // First message is always treated as the user's name
            if (!_nameCollected)
            {
                _userName = rawInput.Length > 0 ? rawInput : "friend";
                _nameCollected = true;
                _chatBot.SetUserName(_userName);

                AppendBotMessage($"Great to meet you, {_userName}! 👋");
                AppendBotMessage("I'm CyberGuard — your cybersecurity awareness assistant.");
                AppendBotMessage($"Ask me anything about staying safe online, {_userName}. You can also use the tabs above for Tasks, the Quiz, and the Activity Log.");
                AppendBotMessage("TOPICS: passwords • phishing • privacy • wi-fi • MFA • malware • software updates • safe browsing");
                return;
            }

            //  NLP intent routing (Part 3, Task 3) 
            // Anything task/quiz/log related is intercepted here first.
            // Everything else falls through to the existing Part 2 ChatBot.
            NlpProcessor.Intent intent = _nlpProcessor.DetectIntent(rawInput);

            switch (intent)
            {
                case NlpProcessor.Intent.AddTask:
                    HandleAddTaskFromChat(rawInput);
                    break;

                case NlpProcessor.Intent.DeleteTask:
                    HandleDeleteTaskFromChat(rawInput);
                    break;

                case NlpProcessor.Intent.CompleteTask:
                    HandleCompleteTaskFromChat(rawInput);
                    break;

                case NlpProcessor.Intent.ViewTasks:
                    HandleViewTasksFromChat();
                    break;

                case NlpProcessor.Intent.StartQuiz:
                    HandleStartQuizFromChat();
                    break;

                case NlpProcessor.Intent.ShowActivityLog:
                    HandleShowActivityLogFromChat();
                    break;

                default:
                    // GetCybersecurity, ChatResponse, Unknown — let the Part 2 bot handle it
                    _chatBot.ProcessInput(rawInput);
                    break;
            }

            ChatScrollViewer.ScrollToBottom();
        }

        //  NLP → Task Assistant bridge 

        private void HandleAddTaskFromChat(string rawInput)
        {
            string title = _nlpProcessor.ExtractTaskTitle(rawInput);
            DateTime? reminder = _nlpProcessor.ExtractReminderDate(rawInput);

            var task = new CyberTask
            {
                Title = title,
                Description = $"Task created from chat: \"{title}\"",
                ReminderDate = reminder,
                IsCompleted = false,
                CreatedAt = DateTime.Now
            };

            bool ok = _dbManager.AddTask(task);

            if (ok)
            {
                string msg = reminder.HasValue
                    ? $"Task added: '{title}'. I'll remind you on {reminder.Value:dd MMM yyyy HH:mm}."
                    : $"Task added: '{title}'. Would you like to set a reminder? (e.g. 'remind me in 3 days')";

                AppendBotMessage(msg);
                _activityLog.Record(LogCategory.Task, $"Task added: '{title}'" + (reminder.HasValue ? $" (reminder set: {reminder.Value:dd MMM})" : " (no reminder set)"));
                if (reminder.HasValue)
                    _activityLog.Record(LogCategory.Reminder, $"Reminder set for '{title}' on {reminder.Value:dd MMM yyyy}");

                RefreshTaskList();
                RefreshActivityLogList();
            }
            else
            {
                AppendBotMessage("Sorry, I couldn't save that task — please check the database connection and try again.");
            }
        }

        private void HandleDeleteTaskFromChat(string rawInput)
        {
            int? id = _nlpProcessor.ExtractTaskId(rawInput);
            if (id == null)
            {
                AppendBotMessage("Which task would you like to delete? Try: 'delete task 3', or use the Tasks tab.");
                return;
            }

            bool ok = _dbManager.DeleteTask(id.Value);
            AppendBotMessage(ok ? $"Task #{id} deleted." : $"I couldn't find task #{id}.");

            if (ok)
            {
                _activityLog.Record(LogCategory.Task, $"Task #{id} deleted");
                RefreshTaskList();
                RefreshActivityLogList();
            }
        }

        private void HandleCompleteTaskFromChat(string rawInput)
        {
            int? id = _nlpProcessor.ExtractTaskId(rawInput);
            if (id == null)
            {
                AppendBotMessage("Which task is done? Try: 'complete task 2', or use the Tasks tab.");
                return;
            }

            bool ok = _dbManager.MarkTaskCompleted(id.Value);
            AppendBotMessage(ok ? $"Nice work — task #{id} marked as completed! ✅" : $"I couldn't find task #{id}.");

            if (ok)
            {
                _activityLog.Record(LogCategory.Task, $"Task #{id} marked as completed");
                RefreshTaskList();
                RefreshActivityLogList();
            }
        }

        private void HandleViewTasksFromChat()
        {
            var tasks = _dbManager.GetAllTasks();

            if (tasks.Count == 0)
            {
                AppendBotMessage("You don't have any tasks yet. Try: 'add a task to enable 2FA'.");
                return;
            }

            AppendBotMessage($"Here are your tasks, {_userName}:");
            foreach (var t in tasks.Take(10))
                AppendBotMessage($"#{t.Id} — {t}");

            MainTabControl.SelectedIndex = 1; // jump to Tasks tab
        }

        //  NLP → Quiz bridge 

        private void HandleStartQuizFromChat()
        {
            AppendBotMessage("Let's test your cybersecurity knowledge! Heading to the Quiz tab...");
            _activityLog.Record(LogCategory.Quiz, "Quiz started from chat command");
            RefreshActivityLogList();
            MainTabControl.SelectedIndex = 2;
            StartQuiz();
        }

        //  NLP → Activity Log bridge 

        private void HandleShowActivityLogFromChat()
        {
            var entries = _activityLog.GetRecent();

            if (!_activityLog.HasEntries)
            {
                AppendBotMessage("Nothing logged yet — once you add tasks or play the quiz, I'll keep a record here.");
                return;
            }

            AppendBotMessage("Here's a summary of recent actions:");
            int i = 1;
            foreach (var entry in entries)
                AppendBotMessage($"{i++}. {entry}");

            MainTabControl.SelectedIndex = 3; // jump to Log tab
        }

        //  ASCII Art (kept from Part 1/2, used only as fallback resource) 

        private void LoadAsciiArt()
        {
            // Retained for compatibility with GreetingManager.ConvertImageToAscii,
            // but the banner TextBlock was removed from the v3.0 tabbed layout.
            string[] paths = { "logo.png", "logo.txt", "ascii.txt" };
            foreach (string p in paths)
            {
                if (File.Exists(p))
                    return; // presence check only — no visual banner in this layout
            }
        }

       
        // TASKS TAB
       

        private void RefreshTaskList()
        {
            var tasks = _dbManager.GetAllTasks();
            TasksListBox.ItemsSource = null;
            TasksListBox.ItemsSource = tasks;
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TaskTitleBox.Text?.Trim() ?? "";
            string desc = TaskDescBox.Text?.Trim() ?? "";
            string reminderText = TaskReminderBox.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Please enter a task title.", "CyberGuard", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime? reminder = string.IsNullOrWhiteSpace(reminderText)
                ? null
                : _nlpProcessor.ExtractReminderDate(reminderText);

            var task = new CyberTask
            {
                Title = title,
                Description = string.IsNullOrWhiteSpace(desc) ? title : desc,
                ReminderDate = reminder,
                IsCompleted = false,
                CreatedAt = DateTime.Now
            };

            bool ok = _dbManager.AddTask(task);

            if (ok)
            {
                _activityLog.Record(LogCategory.Task, $"Task added via Tasks tab: '{title}'" + (reminder.HasValue ? $" (reminder: {reminder.Value:dd MMM yyyy})" : " (no reminder set)"));
                TaskTitleBox.Clear();
                TaskDescBox.Clear();
                TaskReminderBox.Clear();
                RefreshTaskList();
                RefreshActivityLogList();
            }
            else
            {
                MessageBox.Show("Could not save the task. Check your database connection.", "CyberGuard", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CompleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TasksListBox.SelectedItem is CyberTask task)
            {
                if (_dbManager.MarkTaskCompleted(task.Id))
                {
                    _activityLog.Record(LogCategory.Task, $"Task #{task.Id} ('{task.Title}') marked as completed via Tasks tab");
                    RefreshTaskList();
                    RefreshActivityLogList();
                }
            }
            else
            {
                MessageBox.Show("Select a task first.", "CyberGuard", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TasksListBox.SelectedItem is CyberTask task)
            {
                if (_dbManager.DeleteTask(task.Id))
                {
                    _activityLog.Record(LogCategory.Task, $"Task #{task.Id} ('{task.Title}') deleted via Tasks tab");
                    RefreshTaskList();
                    RefreshActivityLogList();
                }
            }
            else
            {
                MessageBox.Show("Select a task first.", "CyberGuard", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RefreshTasksButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshTaskList();
        }

       
        // QUIZ TAB
      

        private void StartQuizButton_Click(object sender, RoutedEventArgs e)
        {
            _activityLog.Record(LogCategory.Quiz, "Quiz started via Quiz tab");
            RefreshActivityLogList();
            StartQuiz();
        }

        private void StartQuiz()
        {
            _quizEngine.StartNewSession(10);
            QuizFeedbackText.Text = "";
            QuizScoreLabel.Text = "";
            ShowCurrentQuestion();
        }

        private void ShowCurrentQuestion()
        {
            QuizOptionsPanel.Children.Clear();

            if (!_quizEngine.IsActive || _quizEngine.CurrentQuestion == null)
            {
                QuizQuestionText.Text = "Quiz complete! 🎉";
                QuizProgressLabel.Text = "";
                QuizScoreLabel.Text = _quizEngine.GetFinalFeedback();
                _activityLog.Record(LogCategory.Quiz, $"Quiz completed — score {_quizEngine.Score}/{_quizEngine.TotalQuestions}");
                RefreshActivityLogList();
                return;
            }

            var q = _quizEngine.CurrentQuestion;
            QuizProgressLabel.Text = $"Question {_quizEngine.CurrentQuestionNumber} of {_quizEngine.TotalQuestions}  •  Score: {_quizEngine.Score}";
            QuizQuestionText.Text = q.QuestionText;
            QuizFeedbackText.Text = "";

            for (int i = 0; i < q.Options.Count; i++)
            {
                int optionIndex = i; // capture for closure
                var btn = new Button
                {
                    Content = q.Options[i],
                    Style = (Style)FindResource("QuizOptionStyle")
                };
                btn.Click += (s, e) => SubmitQuizAnswer(optionIndex);
                QuizOptionsPanel.Children.Add(btn);
            }
        }

        private void SubmitQuizAnswer(int selectedIndex)
        {
            var q = _quizEngine.CurrentQuestion;
            if (q == null) return;

            bool correct = _quizEngine.SubmitAnswer(selectedIndex);

            QuizFeedbackText.Text = (correct ? "✅ Correct! " : "❌ Not quite. ") + q.Explanation;
            QuizFeedbackText.Foreground = correct
                ? new SolidColorBrush(Color.FromRgb(57, 255, 20))
                : new SolidColorBrush(Color.FromRgb(255, 179, 0));

            _activityLog.Record(LogCategory.Quiz, $"Answered question {_quizEngine.CurrentQuestionNumber - 1} — {(correct ? "correct" : "incorrect")}");

            // Brief pause so the user can read feedback, then advance
            var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(2.2) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                ShowCurrentQuestion();
                RefreshActivityLogList();
            };
            timer.Start();

            foreach (var child in QuizOptionsPanel.Children)
                if (child is Button b) b.IsEnabled = false;
        }

        
        // ACTIVITY LOG TAB
        

        private void RefreshActivityLogList()
        {
            var entries = _activityLog.GetRecent(_logDisplayCount);
            ActivityLogListBox.ItemsSource = null;
            ActivityLogListBox.ItemsSource = entries.Select(en => en.ToString()).ToList();
        }

        private void ShowMoreLogButton_Click(object sender, RoutedEventArgs e)
        {
            _logDisplayCount += ActivityLog.DefaultDisplayCount;
            RefreshActivityLogList();
        }

        private void RefreshLogButton_Click(object sender, RoutedEventArgs e)
        {
            _logDisplayCount = ActivityLog.DefaultDisplayCount;
            RefreshActivityLogList();
        }

        
        // CHAT BUBBLE RENDERING 
        

        private void AppendBotMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                var bubble = CreateBubble(message, isBot: true);
                ChatPanel.Children.Add(bubble);
                ChatScrollViewer.ScrollToBottom();
            });
        }

        private void AppendUserMessage(string message)
        {
            var bubble = CreateBubble(message, isBot: false);
            ChatPanel.Children.Add(bubble);
            ChatScrollViewer.ScrollToBottom();
        }

        private UIElement CreateBubble(string message, bool isBot)
        {
            var outer = new Grid { Margin = new Thickness(0, 4, 0, 4) };

            var icon = new TextBlock
            {
                Text = isBot ? "🛡" : "👤",
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 4, 0, 0)
            };

            var text = new TextBlock
            {
                Text = message,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 13,
                Foreground = isBot
                    ? new SolidColorBrush(Color.FromRgb(200, 230, 255))
                    : new SolidColorBrush(Color.FromRgb(150, 255, 150)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Top
            };

            var bubbleBg = isBot
                ? new SolidColorBrush(Color.FromRgb(22, 32, 50))
                : new SolidColorBrush(Color.FromRgb(13, 43, 26));

            var borderBrush = isBot
                ? new SolidColorBrush(Color.FromRgb(30, 45, 69))
                : new SolidColorBrush(Color.FromRgb(20, 60, 35));

            var innerStack = new StackPanel { Orientation = Orientation.Horizontal };
            innerStack.Children.Add(icon);
            innerStack.Children.Add(text);

            var border = new Border
            {
                Background = bubbleBg,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(isBot ? 0 : 8, 8, 8, isBot ? 8 : 0),
                Padding = new Thickness(14, 10, 14, 10),
                MaxWidth = 650,
                Child = innerStack
            };

            if (isBot)
            {
                border.HorizontalAlignment = HorizontalAlignment.Left;
                outer.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else
            {
                border.HorizontalAlignment = HorizontalAlignment.Right;
                outer.HorizontalAlignment = HorizontalAlignment.Right;
            }

            outer.Children.Add(border);
            return outer;
        }
    }
}