
// List<T> shuffling using Fisher-Yates algorithm:
//   Stack Overflow — Randomize a List in C#:
//   https://stackoverflow.com/questions/273313/randomize-a-listt-in-c-sharp
//
// Math.Min for bounding collection size:
//   Microsoft Docs — Math.Min:
//   https://learn.microsoft.com/en-us/dotnet/api/system.math.min


using System;
using System.Collections.Generic;

namespace Cyber_Security_Awareness_Chatbot
{
    //  Quiz Question Model 

    /// <summary>
    /// Represents one question in the cybersecurity quiz.
    /// Supports both multiple-choice (4 options) and true/false (2 options) formats.
    /// </summary>
    public class QuizQuestion
    {
        /// <summary>The question text displayed to the user.</summary>
        public string QuestionText { get; set; } = "";

        /// <summary>
        /// Answer options. For true/false questions this will contain exactly
        /// ["True", "False"]; for multiple-choice it contains four options.
        /// </summary>
        public List<string> Options { get; set; } = new();

        /// <summary>
        /// Zero-based index into Options indicating the correct answer.
        /// e.g. 0 = first option, 2 = third option.
        /// </summary>
        public int CorrectIndex { get; set; }

        /// <summary>
        /// Brief explanation shown after the user answers.
        /// Reinforces the cybersecurity concept regardless of right/wrong.
        /// </summary>
        public string Explanation { get; set; } = "";

        /// <summary>Convenience accessor — returns the correct answer text.</summary>
        public string CorrectAnswer => Options[CorrectIndex];
    }

    //  Quiz Engine 

    /// <summary>
    /// Manages quiz state: question bank, current question tracking, and scoring.
    /// The UI layer calls NextQuestion() and SubmitAnswer() — it never touches
    /// the question list directly, keeping the logic fully decoupled from WPF.
    /// </summary>
    public class QuizEngine
    {
        //  Private State 

        private readonly List<QuizQuestion> _allQuestions;
        private List<QuizQuestion> _sessionQuestions = new();
        private int _currentIndex = 0;
        private int _score = 0;
        private readonly Random _random = new();

        //  Public Read-Only State 

        /// <summary>True while there are still unanswered questions in this session.</summary>
        public bool IsActive => _currentIndex < _sessionQuestions.Count;

        /// <summary>The question the user is currently answering (1-based for display).</summary>
        public int CurrentQuestionNumber => _currentIndex + 1;

        /// <summary>Total number of questions in this quiz session.</summary>
        public int TotalQuestions => _sessionQuestions.Count;

        /// <summary>Number of correct answers so far.</summary>
        public int Score => _score;

        /// <summary>The active question object, or null if the quiz has ended.</summary>
        public QuizQuestion? CurrentQuestion =>
            _currentIndex < _sessionQuestions.Count ? _sessionQuestions[_currentIndex] : null;

        //  Constructor 

        public QuizEngine()
        {
            _allQuestions = BuildQuestionBank();
        }

        //  Public Methods 

        /// <summary>
        /// Resets and starts a new quiz session, randomly selecting questions
        /// from the full bank so repeat plays feel fresh.
        /// </summary>
        /// <param name="questionCount">How many questions to include (default 10).</param>
        public void StartNewSession(int questionCount = 10)
        {
            _score = 0;
            _currentIndex = 0;

            // Shuffle a copy of the question bank (Fisher-Yates)
            var shuffled = new List<QuizQuestion>(_allQuestions);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            // Take only as many as requested (min with available)
            int take = Math.Min(questionCount, shuffled.Count);
            _sessionQuestions = shuffled.GetRange(0, take);
        }

        /// <summary>
        /// Evaluates the user's selected answer index.
        /// </summary>
        /// <param name="selectedIndex">Zero-based index of the option the user picked.</param>
        /// <returns>True if the answer was correct.</returns>
        public bool SubmitAnswer(int selectedIndex)
        {
            if (CurrentQuestion == null) return false;

            bool isCorrect = selectedIndex == CurrentQuestion.CorrectIndex;
            if (isCorrect) _score++;

            // Advance to the next question
            _currentIndex++;
            return isCorrect;
        }

        /// <summary>
        /// Returns a final score message based on the user's performance.
        /// Calibrated to encourage further learning at every level.
        /// </summary>
        public string GetFinalFeedback()
        {
            double percentage = TotalQuestions > 0
                ? (double)_score / TotalQuestions * 100
                : 0;

            return percentage switch
            {
                >= 90 => $"🏆 Outstanding! {_score}/{TotalQuestions} — You're a certified CyberGuard pro!",
                >= 70 => $"🎉 Great job! {_score}/{TotalQuestions} — Strong cybersecurity awareness!",
                >= 50 => $"👍 Good effort! {_score}/{TotalQuestions} — Keep learning to stay safe online.",
                _ => $"📚 {_score}/{TotalQuestions} — Don't worry! Every expert started somewhere. Review the topics and try again!"
            };
        }

        //  Question Bank 

        /// <summary>
        /// Builds the full question bank of 12+ cybersecurity questions.
        /// Covers: phishing, passwords, MFA, privacy, malware, safe browsing,
        /// social engineering, Wi-Fi safety, and South African context.
        /// </summary>
        private static List<QuizQuestion> BuildQuestionBank()
        {
            return new List<QuizQuestion>
            {
                //  MULTIPLE CHOICE 

                new QuizQuestion
                {
                    QuestionText = "What should you do if you receive an email asking for your password?",
                    Options      = new List<string>
                    {
                        "A) Reply with your password",
                        "B) Delete the email",
                        "C) Report the email as phishing",
                        "D) Ignore it"
                    },
                    CorrectIndex = 2,
                    Explanation  = "✅ Reporting phishing emails helps protect you AND others. Legitimate organisations never ask for your password via email."
                },

                new QuizQuestion
                {
                    QuestionText = "Which of the following is the STRONGEST password?",
                    Options      = new List<string>
                    {
                        "A) password123",
                        "B) John1990",
                        "C) Tr0ub4dor&3",
                        "D) qwerty"
                    },
                    CorrectIndex = 2,
                    Explanation  = "✅ A strong password mixes uppercase, lowercase, numbers, and symbols. Avoid personal details like your name or birth year."
                },

                new QuizQuestion
                {
                    QuestionText = "What does HTTPS in a website URL indicate?",
                    Options      = new List<string>
                    {
                        "A) The website is government-approved",
                        "B) The connection is encrypted and more secure",
                        "C) The website is free to use",
                        "D) The website never contains malware"
                    },
                    CorrectIndex = 1,
                    Explanation  = "✅ HTTPS means the data between your browser and the server is encrypted. However, it does NOT guarantee the site is trustworthy — phishing sites also use HTTPS."
                },

                new QuizQuestion
                {
                    QuestionText = "What is Multi-Factor Authentication (MFA)?",
                    Options      = new List<string>
                    {
                        "A) Using multiple passwords for one account",
                        "B) A second verification step beyond your password",
                        "C) A type of antivirus software",
                        "D) Logging in from multiple devices simultaneously"
                    },
                    CorrectIndex = 1,
                    Explanation  = "✅ MFA adds a second layer — like a code sent to your phone — so even if your password is stolen, attackers still can't get in."
                },

                new QuizQuestion
                {
                    QuestionText = "Which of the following is a sign of a phishing email?",
                    Options      = new List<string>
                    {
                        "A) It comes from a known contact",
                        "B) It has a professional company logo",
                        "C) It creates urgency like 'Your account will be closed immediately!'",
                        "D) It contains no links"
                    },
                    CorrectIndex = 2,
                    Explanation  = "✅ Urgency and fear tactics are hallmarks of phishing. Scammers want you to act without thinking. Always pause and verify."
                },

                new QuizQuestion
                {
                    QuestionText = "What is ransomware?",
                    Options      = new List<string>
                    {
                        "A) Software that displays adverts",
                        "B) A virus that monitors your keystrokes",
                        "C) Malware that encrypts your files and demands payment",
                        "D) A tool used by ethical hackers"
                    },
                    CorrectIndex = 2,
                    Explanation  = "✅ Ransomware locks your files until you pay a ransom. Regular offline backups are your best defence — you can restore without paying."
                },

                new QuizQuestion
                {
                    QuestionText = "What is the safest way to use public Wi-Fi?",
                    Options      = new List<string>
                    {
                        "A) Only browse social media, not banking sites",
                        "B) Connect using a VPN to encrypt your traffic",
                        "C) Use incognito mode in your browser",
                        "D) Connect only to networks with a password"
                    },
                    CorrectIndex = 1,
                    Explanation  = "✅ A VPN encrypts all your traffic so attackers on the same network cannot intercept it. Incognito mode and passwords do NOT protect against network-level attacks."
                },

                new QuizQuestion
                {
                    QuestionText = "How often should you update your software and operating system?",
                    Options      = new List<string>
                    {
                        "A) Only when something breaks",
                        "B) Once a year",
                        "C) As soon as updates are available",
                        "D) Never — updates can introduce bugs"
                    },
                    CorrectIndex = 2,
                    Explanation  = "✅ Updates patch security vulnerabilities. Attackers actively exploit unpatched software, so delaying updates puts you at serious risk."
                },

                //  TRUE / FALSE 

                new QuizQuestion
                {
                    QuestionText = "TRUE or FALSE: It is safe to reuse the same password across multiple websites.",
                    Options      = new List<string> { "True", "False" },
                    CorrectIndex = 1,
                    Explanation  = "✅ FALSE — if one site is breached, attackers try your credentials on every other site (credential stuffing). Use a unique password per account."
                },

                new QuizQuestion
                {
                    QuestionText = "TRUE or FALSE: A padlock icon (🔒) in the browser bar means a website is safe and trustworthy.",
                    Options      = new List<string> { "True", "False" },
                    CorrectIndex = 1,
                    Explanation  = "✅ FALSE — the padlock only means your connection is encrypted. Phishing and scam sites can also have HTTPS padlocks. Always verify the full URL."
                },

                new QuizQuestion
                {
                    QuestionText = "TRUE or FALSE: Antivirus software alone is enough to fully protect your computer.",
                    Options      = new List<string> { "True", "False" },
                    CorrectIndex = 1,
                    Explanation  = "✅ FALSE — antivirus is one layer of protection. You also need safe browsing habits, regular updates, strong passwords, and MFA for comprehensive security."
                },

                new QuizQuestion
                {
                    QuestionText = "TRUE or FALSE: SARS (South African Revenue Service) will contact you by email asking you to click a link to claim your tax refund.",
                    Options      = new List<string> { "True", "False" },
                    CorrectIndex = 1,
                    Explanation  = "✅ FALSE — SARS-themed phishing emails are common in South Africa. SARS communicates through eFiling. Never click unsolicited refund links."
                },

                new QuizQuestion
                {
                    QuestionText = "TRUE or FALSE: Social engineering attacks target technology, not people.",
                    Options      = new List<string> { "True", "False" },
                    CorrectIndex = 1,
                    Explanation  = "✅ FALSE — social engineering manipulates PEOPLE using psychology (urgency, fear, trust) rather than exploiting software. Awareness is your best defence."
                }
            };
        }
    }
}