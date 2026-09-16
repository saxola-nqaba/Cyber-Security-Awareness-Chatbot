
// System.Media.SoundPlayer for WAV playback:
//   Stack Overflow — Play WAV file in C#:
//   https://stackoverflow.com/questions/7556672/playing-a-wav-file-in-a-c-sharp-winforms-application
//
// Threading with System.Threading.Thread for async sound:
//   W3Schools — C# Threads:
//   https://www.w3schools.com/cs/cs_threads.php
//   Stack Overflow — Play sound on separate thread:
//   https://stackoverflow.com/questions/3067648/play-sound-on-separate-thread
//
// System.Drawing.Bitmap for image pixel access:
//   Stack Overflow — Convert image to ASCII art C#:
//   https://stackoverflow.com/questions/2265910/convert-an-image-to-grayscale
//
// File.Exists for safe file path checking:
//   W3Schools — C# Files:
//   https://www.w3schools.com/cs/cs_files.php

using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Text;
using System.Threading;

namespace Cyber_Security_Awareness_Chatbot
{
    /// <summary>
    /// Manages the application's greeting experience.
    /// Handles WAV voice playback on a background thread and
    /// converts PNG images to ASCII art for display in the GUI header.
    /// </summary>
    public class GreetingManager
    {
        // Private Fields 

        
        private readonly string _filePath = "greetings.wav.wav";

        // Public Methods 

       
        public void PlayGreetingSound()
        {
            Thread soundThread = new Thread(() =>
            {
                if (File.Exists(_filePath))
                {
                    try
                    {
                        SoundPlayer player = new SoundPlayer(_filePath);
                        // PlaySync blocks the thread until the sound finishes
                        player.PlaySync();
                    }
                    catch (Exception ex)
                    {
                        // Log the error message — in production this could write to a log file
                        Console.WriteLine($"[GreetingManager] Sound playback error: {ex.Message}");
                    }
                }
                else
                {
                    // Inform the developer the file is missing — not shown to the end user
                    Console.WriteLine($"[GreetingManager] Sound file not found at: {_filePath}");
                }
            });

            soundThread.Start();
            soundThread.Join(); // Wait for the sound thread to finish
        }

        
        public void PlayGreetingWithAsciiArt()
        {
            Thread soundThread = new Thread(() =>
            {
                if (File.Exists(_filePath))
                {
                    try
                    {
                        SoundPlayer player = new SoundPlayer(_filePath);
                        // Play() is non-blocking — audio plays in the background
                        player.Play();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[GreetingManager] Sound playback error: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"[GreetingManager] Sound file not found at: {_filePath}");
                }
            });

            // Start the sound thread — does not block the calling thread
            soundThread.Start();
        }

       
        public string ConvertImageToAscii(string imagePath, int width)
        {
            // Return early if the file does not exist
            if (!File.Exists(imagePath))
                return "[ ASCII image not found — place logo.png in the output folder ]";

            // Characters ordered from darkest to lightest for greyscale mapping
            string grayscaleChars = "@%#*+=-:. ";

            // StringBuilder is more efficient than string concatenation in a loop
            var ascii = new StringBuilder();

            try
            {
                using (Bitmap original = new Bitmap(imagePath))
                {
                    // Calculate height to preserve the image aspect ratio
                    
                    int height = (int)(original.Height / (double)original.Width * width * 0.55);

                    using (Bitmap resized = new Bitmap(original, new Size(width, height)))
                    {
                        for (int y = 0; y < resized.Height; y++)
                        {
                            for (int x = 0; x < resized.Width; x++)
                            {
                                // Get the colour of each pixel
                                Color pixel = resized.GetPixel(x, y);

                                // Convert to greyscale using simple average of RGB channels
                                int gray = (pixel.R + pixel.G + pixel.B) / 3;

                                // Map the greyscale value (0-255) to a character index
                                int index = gray * (grayscaleChars.Length - 1) / 255;
                                ascii.Append(grayscaleChars[index]);
                            }

                            // New line at the end of each row
                            ascii.AppendLine();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"[ Error converting image: {ex.Message} ]";
            }

            return ascii.ToString();
        }
    }
}