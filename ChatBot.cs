
// Dictionary usage and initialisation:
//   W3Schools — C# Dictionary:
//   https://www.w3schools.com/cs/cs_dictionaries.php
//
// StringComparer.OrdinalIgnoreCase for case-insensitive keys:
//   Stack Overflow — Case-insensitive Dictionary:
//   https://stackoverflow.com/questions/13230414/case-insensitive-dictionary
//
// Action<T> delegate used to decouple output from the UI:
//   Stack Overflow — Passing methods as parameters using delegates:
//   https://stackoverflow.com/questions/2082615/pass-method-as-parameter-using-c-sharp
//
// String.Contains with StringComparison parameter:
//   W3Schools — C# String Contains():
//   https://www.w3schools.com/cs/cs_strings_contains.php
//
// Random class for selecting varied responses:
//   W3Schools — C# Random:
//   https://www.w3schools.com/cs/cs_random.php


using System;
using System.Collections.Generic;

namespace Cyber_Security_Awareness_Chatbot
{
    /// <summary>
    /// Core chatbot engine for the CyberGuard Awareness Assistant.
    /// All output is routed through an Action delegate so the same
    /// class works for both the WPF GUI and any future console mode.
    /// </summary>
    public class ChatBot
    {
        // Private State Fields 

        /// <summary>The name the user provided at the start of the session.</summary>
        private string _userName = "friend";

        /// <summary>The cybersecurity topic the user expressed interest in (memory feature).</summary>
        private string _userInterest = "";

        /// <summary>The last topic discussed — used to handle follow-up requests.</summary>
        private string _lastTopic = "";

        /// <summary>Random number generator for selecting varied responses.</summary>
        private readonly Random _random;

        
        private readonly Action<string> _output;

        
        private readonly Dictionary<string, List<string>> _keywordResponses;

        //Constructor

        
        public ChatBot(Action<string> outputAction = null)
        {
            _random = new Random();

            // Use the provided delegate, or fall back to console output
            _output = outputAction ?? Console.WriteLine;

            // Initialise the response bank with multiple responses per topic
            // Lists allow random selection to keep conversations varied
            _keywordResponses = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "phishing", new List<string>
                    {
                        "Be cautious of emails asking for personal info — phishers often pose as trusted brands.",
                        "Never click links in unsolicited emails. Always verify the source first.",
                        "Hover over links to see where they actually lead before clicking. Phishers hide behind legit-looking URLs.",
                        "Legitimate companies will NEVER ask for your password or ID via email. Always double-check.",
                        "Watch for urgency language like 'Your account will be closed!' — that's a classic phishing tactic.",
                        "Check the sender's actual email address carefully — scammers use domains like 'support@paypa1.com'."
                    }
                },
                {
                    "password", new List<string>
                    {
                        "Use strong, unique passwords containing uppercase, lowercase, numbers, and symbols.",
                        "Never reuse passwords across different accounts — one breach can compromise everything.",
                        "Consider a password manager like Bitwarden or 1Password to generate and store passwords securely.",
                        "Your password should be at least 12 characters long. Longer is always better.",
                        "Avoid using personal details like your name, birthday, or pet's name in passwords.",
                        "A passphrase like 'PurpleGiraffe!Runs@Fast' is both strong and memorable."
                    }
                },
                {
                    "privacy", new List<string>
                    {
                        "Regularly review and tighten your social media privacy settings.",
                        "Limit the personal information you share online — less is always more.",
                        "Use a VPN, especially on public Wi-Fi, to protect your browsing data.",
                        "Be careful what you post — location data in photos can reveal where you live.",
                        "Read app permissions carefully. Does a flashlight app really need your contacts?"
                    }
                },
                {
                    "wifi", new List<string>
                    {
                        "Avoid using public Wi-Fi for banking or shopping — these networks can be monitored.",
                        "Always use a VPN on public Wi-Fi to encrypt your connection.",
                        "Secure your home Wi-Fi with WPA3 encryption and a strong password.",
                        "Turn off automatic Wi-Fi connection on your phone to prevent connecting to rogue hotspots.",
                        "Be wary of Wi-Fi networks with generic names like 'Free_Airport_WiFi' — they may be traps."
                    }
                },
                {
                    "mfa", new List<string>
                    {
                        "Multi-factor authentication (MFA) adds a second layer of security beyond your password.",
                        "Enable MFA on all important accounts — email, banking, and social media especially.",
                        "Use an authenticator app like Google Authenticator rather than SMS codes where possible.",
                        "Even if someone steals your password, MFA stops them from getting into your account.",
                        "MFA is one of the single most effective ways to protect yourself online. Enable it everywhere."
                    }
                },
                {
                    "antivirus", new List<string>
                    {
                        "Install reputable antivirus software and keep it updated to catch new threats.",
                        "Run regular full scans — many threats hide quietly until triggered.",
                        "Antivirus alone isn't enough — combine it with safe browsing habits.",
                        "Free options like Windows Defender are decent; paid suites offer more features.",
                        "Never disable your antivirus, even temporarily — that's when attackers strike."
                    }
                },
                {
                    "malware", new List<string>
                    {
                        "Malware includes viruses, ransomware, spyware, and trojans — all designed to harm you.",
                        "Never download software from untrusted sources. Stick to official websites and app stores.",
                        "Ransomware encrypts your files and demands payment — regular backups are your best defence.",
                        "If your device slows down unexpectedly, it could be a sign of malware infection.",
                        "Be very cautious about USB drives from unknown sources — they can carry malware."
                    }
                },
                {
                    "update", new List<string>
                    {
                        "Software updates patch security vulnerabilities that hackers actively exploit.",
                        "Enable automatic updates so you're always protected with the latest security patches.",
                        "Outdated software is one of the top causes of data breaches — don't delay updates.",
                        "This applies to your phone, computer, apps, AND your home router firmware.",
                        "If a device no longer receives security updates, consider replacing it."
                    }
                },
                {
                    "browsing", new List<string>
                    {
                        "Always check for HTTPS (the padlock icon) before entering sensitive information on a website.",
                        "Use a browser extension like uBlock Origin to block malicious ads and trackers.",
                        "Be careful about browser extensions — only install ones from trusted developers.",
                        "Clear your browser cookies regularly to limit the data companies collect on you.",
                        "Avoid clicking pop-up ads — many lead to malware or phishing sites."
                    }
                },
                {
                    "scam", new List<string>
                    {
                        "If an offer seems too good to be true, it almost certainly is — trust your instincts.",
                        "SARS, banks, and government agencies will never demand immediate payment via phone.",
                        "Verify any urgent requests from 'family' by calling them directly on a known number.",
                        "Romance scams are growing in South Africa — never send money to someone you haven't met.",
                        "Report scams to the South African Banking Risk Information Centre (SABRIC)."
                    }
                }
            };
        }

        // Public Method

       
        public void SetUserName(string name)
        {
            _userName = string.IsNullOrWhiteSpace(name) ? "friend" : name.Trim();
        }

       
        public void ProcessInput(string rawInput)
        {
            // Guard: ignore empty input
            if (string.IsNullOrWhiteSpace(rawInput))
            {
                Say("Please type something so I can help you.");
                return;
            }

            // Normalise to lowercase for case-insensitive comparisons
            string input = rawInput.ToLower().Trim();

            
            if (input == "exit" || input == "quit" || input == "bye")
            {
                Say($"Goodbye, {_userName}! Stay cyber-safe out there. 🛡");
                return;
            }

            
            // If the user says "I'm interested in X", remember X for later
            if (input.Contains("i like") || input.Contains("i'm interested in") ||
                input.Contains("i am interested in") || input.Contains("my favourite topic is"))
            {
                _userInterest = ExtractTopic(input);
                Say($"Noted! I'll remember that you're interested in {_userInterest}, {_userName}.");
                Say($"As someone interested in {_userInterest}, here's something useful:");
                RespondToTopic(_userInterest);
                return;
            }

           
            // Phrases like "tell me more" or "another tip" continue the last topic
            if (IsFollowUp(input))
            {
                HandleFollowUp();
                return;
            }

          
            string sentiment = DetectSentiment(input);
            EmotionalResponse(sentiment, input);

            
            RouteByKeyword(input);
        }

        // Private Routing Logic

        
        private void RouteByKeyword(string input)
        {
            if (ContainsAny(input, "phishing", "phish"))
            {
                SetLastTopic("phishing");
                Say("That's an important topic. " + RandomResponse("phishing"));
                Say($"💡 Tip for {_userName}: Always verify the sender before clicking anything.");
            }
            else if (ContainsAny(input, "scam", "fraud"))
            {
                SetLastTopic("scam");
                Say(RandomResponse("scam"));
                Say("Would you like to know how to report a scam in South Africa?");
            }
            else if (ContainsAny(input, "password", "passphrase", "credentials"))
            {
                SetLastTopic("password");
                Say("Good thinking — password safety is crucial. " + RandomResponse("password"));
            }
            else if (ContainsAny(input, "privacy", "private", "data protection"))
            {
                SetLastTopic("privacy");
                Say("Privacy matters. " + RandomResponse("privacy"));
            }
            else if (ContainsAny(input, "wifi", "wi-fi", "wireless", "hotspot"))
            {
                SetLastTopic("wifi");
                Say(RandomResponse("wifi"));
            }
            else if (ContainsAny(input, "mfa", "multi-factor", "two-factor", "2fa", "authenticator"))
            {
                SetLastTopic("mfa");
                Say("MFA is one of the best defences you have. " + RandomResponse("mfa"));
            }
            else if (ContainsAny(input, "antivirus", "virus", "malware", "ransomware", "spyware"))
            {
                // Choose the response bank based on which word was used
                string topic = input.Contains("antivirus") ? "antivirus" : "malware";
                SetLastTopic(topic);
                Say(RandomResponse(topic));
            }
            else if (ContainsAny(input, "update", "patch", "software"))
            {
                SetLastTopic("update");
                Say(RandomResponse("update"));
            }
            else if (ContainsAny(input, "browsing", "browser", "website", "https", "http"))
            {
                SetLastTopic("browsing");
                Say(RandomResponse("browsing"));
            }
            else if (ContainsAny(input, "social engineering", "manipulation", "pretexting"))
            {
                SetLastTopic("scam");
                Say("Social engineering is when attackers manipulate people rather than systems.");
                Say("Always verify who is asking for information before sharing anything — even if they sound official.");
            }
            else if (ContainsAny(input, "device", "phone", "laptop", "computer"))
            {
                SetLastTopic("malware");
                Say("Protecting your devices is essential.");
                Say("Use lock screens, enable full-disk encryption, and set up remote-wipe on your phone in case it's lost or stolen.");
            }
            else if (ContainsAny(input, "how are you", "how r u"))
            {
                Say("I'm running at full capacity and ready to help! 🛡");
                Say($"How can I assist you with cybersecurity today, {_userName}?");
            }
            else if (ContainsAny(input, "purpose", "what do you do", "who are you"))
            {
                Say($"I'm CyberGuard, {_userName} — your cybersecurity awareness assistant.");
                Say("I help South African citizens understand and defend against online threats like phishing, scams, and data breaches.");
            }
            else if (ContainsAny(input, "help", "topics", "ask", "what can"))
            {
                Say("Here's what I can help you with:");
                Say("🔑 Passwords  •  🎣 Phishing  •  🔒 Privacy  •  📶 Wi-Fi\n🔐 MFA  •  🦠 Malware/Antivirus  •  🔄 Software Updates  •  🌐 Safe Browsing");
            }
            else if (ContainsAny(input, "tip", "advice", "suggest"))
            {
                // Personalise with remembered interest if available
                if (!string.IsNullOrEmpty(_userInterest) && _keywordResponses.ContainsKey(_userInterest))
                {
                    Say($"Based on your interest in {_userInterest}, {_userName}:");
                    Say(RandomResponse(_userInterest));
                }
                else
                {
                    // Pick a random topic from all available topics
                    string[] allTopics = { "phishing", "password", "privacy", "wifi", "mfa", "malware", "update", "browsing" };
                    string randomTopic = allTopics[_random.Next(allTopics.Length)];
                    SetLastTopic(randomTopic);
                    Say($"Here's a random cybersecurity tip for you, {_userName}:");
                    Say(RandomResponse(randomTopic));
                }
            }
            else
            {
                // Default fallback for unrecognised input
                Say($"Hmm, I'm not sure I understood that, {_userName}.");
                Say("Try asking about: passwords, phishing, privacy, Wi-Fi, MFA, malware, or safe browsing.");
                Say("You can also use the quick topic chips on the left for fast access! 👇");
            }
        }


        private void HandleFollowUp()
        {
            if (string.IsNullOrEmpty(_lastTopic))
            {
                // No previous topic recorded — prompt the user to choose one
                Say($"Happy to continue, {_userName}! What topic would you like to explore?");
                Say("Try: passwords, phishing, privacy, Wi-Fi, MFA, or malware.");
                return;
            }

            if (_keywordResponses.ContainsKey(_lastTopic))
            {
                // Pull another random response from the same topic
                Say($"Sure! Here's another tip on {_lastTopic}:");
                Say(RandomResponse(_lastTopic));
            }
            else
            {
                // Fallback if the last topic key no longer exists in the dictionary
                Say($"Here's another cybersecurity tip, {_userName}:");
                string[] fallbackTopics = { "phishing", "password", "privacy" };
                Say(RandomResponse(fallbackTopics[_random.Next(fallbackTopics.Length)]));
            }
        }

        
        private void RespondToTopic(string topic)
        {
            if (_keywordResponses.ContainsKey(topic))
            {
                Say(RandomResponse(topic));
                SetLastTopic(topic);
            }
            else
            {
                Say("Here's a general tip: always keep your software updated and be suspicious of unsolicited messages.");
            }
        }

        // Sentiment Detection 

       
        private string DetectSentiment(string input)
        {
            if (ContainsAny(input, "frustrated", "overwhelmed", "confused", "lost", "don't understand"))
                return "frustrated";
            if (ContainsAny(input, "worried", "scared", "afraid", "nervous", "anxious", "concerned"))
                return "worried";
            if (ContainsAny(input, "curious", "interested", "keen", "want to know", "tell me more"))
                return "curious";
            if (ContainsAny(input, "angry", "annoyed", "hate", "terrible", "awful"))
                return "angry";
            if (ContainsAny(input, "happy", "great", "awesome", "love", "thanks", "thank you"))
                return "positive";
            return "neutral";
        }

        
        private void EmotionalResponse(string sentiment, string input)
        {
            switch (sentiment)
            {
                case "frustrated":
                    Say("I understand — cybersecurity can feel overwhelming at first. Let's take it one step at a time.");
                    break;

                case "worried":
                    // Reassure the user first
                    Say("It's completely understandable to feel worried. You're not alone — many people face these threats.");
                    Say("The good news is that with a few simple habits, you can protect yourself significantly.");

                    // Automatically provide a relevant tip without waiting for another message
                    if (ContainsAny(input, "scam", "fraud", "phish"))
                    {
                        Say("Here's something that will help — " + RandomResponse("scam"));
                        SetLastTopic("scam");
                    }
                    else if (ContainsAny(input, "hack", "breach", "stolen"))
                    {
                        Say("Here's a key protective measure: " + RandomResponse("password"));
                        SetLastTopic("password");
                    }
                    break;

                case "curious":
                    Say($"Love the curiosity, {_userName}! Let's dive in. 🔍");
                    break;

                case "angry":
                    Say("I hear your frustration — cyber threats are genuinely harmful. Let me help you take back control.");
                    break;

                case "positive":
                    Say($"Great attitude, {_userName}! Staying informed is your first line of defence. 💪");
                    break;

                    // "neutral" requires no emotional response — proceed straight to topic routing
            }
        }

        // Helper Methods 

        
        private bool IsFollowUp(string input)
        {
            return ContainsAny(input,
                "tell me more", "more info", "explain more", "another tip", "give me another",
                "more details", "keep going", "continue", "what else", "elaborate",
                "more please", "and then", "what about", "next tip");
        }

       
        private string ExtractTopic(string input)
        {
            string[] knownTopics = { "phishing", "password", "privacy", "scam", "wifi", "mfa", "browsing", "malware", "antivirus", "update" };
            foreach (string topic in knownTopics)
            {
                if (input.Contains(topic)) return topic;
            }
            // Default to generic topic if no keyword matched
            return "cybersecurity";
        }

      
        private string RandomResponse(string keyword)
        {
            if (_keywordResponses.TryGetValue(keyword, out var list) && list.Count > 0)
                return list[_random.Next(list.Count)];
            return "Always stay alert and think before you click.";
        }

        /// <summary>Records the most recently discussed topic for follow-up continuity.</summary>
        private void SetLastTopic(string topic)
        {
            _lastTopic = topic;
        }

        
        private static bool ContainsAny(string input, params string[] terms)
        {
            foreach (string term in terms)
            {
                if (input.Contains(term, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

       
        private void Say(string message)
        {
            _output?.Invoke(message);
        }
    }
}