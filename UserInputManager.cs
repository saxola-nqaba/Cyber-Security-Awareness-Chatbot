// String.IsNullOrWhiteSpace for robust empty-string checking:
//   W3Schools — C# Strings:
//   https://www.w3schools.com/cs/cs_strings.php
//   Stack Overflow — Difference between IsNullOrEmpty and IsNullOrWhiteSpace:
//   https://stackoverflow.com/questions/3305421/difference-between-isnullorwhitespace-and-isnullorempty
//
// String.Trim() to remove leading and trailing whitespace:
//   W3Schools — C# String Trim():
//   https://www.w3schools.com/cs/cs_strings_trim.php

namespace Cyber_Security_Awareness_Chatbot
{
    
    internal class UserInputManager
    {
        
        public string ValidateUserName(string userName)
        {
            // IsNullOrWhiteSpace catches null, empty string, and whitespace-only strings
            if (string.IsNullOrWhiteSpace(userName))
            {
                return "User"; // Safe default — never returns an empty string
            }

            // Trim removes any accidental leading/trailing spaces the user may have typed
            return userName.Trim();
        }
    }
}