using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ZATCA_V3.Helpers
{
    public static class Helper
    {
        public static string EncodeToBase64(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            return Convert.ToBase64String(bytes);
        }

        public static string DecodeFromBase64(string? encodedData)
        {
            byte[] bytes = Convert.FromBase64String(encodedData);
            return Encoding.UTF8.GetString(bytes);
        }

        public static void SaveToFile(string content, string filePath)
        {
            try
            {
                // Ensure the directory exists
                string? directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    if (directory != null) Directory.CreateDirectory(directory);
                }

                // Write the token to the file
                File.WriteAllText(filePath, content);

                Console.WriteLine($"Content saved to {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving token to file: {ex.Message}");
            }
        }

        public static string ReadFileToString(string filePath)
        {
            try
            {
                // Check if the file exists
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"File not found: {filePath}");
                    return null;
                }

                // Read the content of the file
                string fileContent = File.ReadAllText(filePath);

                Console.WriteLine($"File content read from {filePath}");

                return fileContent;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
                return null;
            }
        }

        public static string Hash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Convert the input string to a byte array and compute the hash
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                // Convert the hash bytes to a hexadecimal string
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    stringBuilder.Append(hashBytes[i]
                        .ToString("x2")); // "x2" formats each byte as a two-digit hexadecimal number
                }

                return stringBuilder.ToString();
            }
        }

        public static string GenerateRandomDigits(int length)
        {
            Random random = new Random();
            return random.Next((int)Math.Pow(10, length - 1), (int)Math.Pow(10, length)).ToString("D" + length);
        }


        public static string GetCertificateHashFromFile(string filePath)
        {
            // Read the certificate bytes from the file
            byte[] certificateBytes = File.ReadAllBytes(filePath);

            // Create an X509Certificate2 object from the certificate bytes
            using (var x509Certificate = new X509Certificate2(certificateBytes))
            {
                // Get the raw data of the certificate
                byte[] rawData = x509Certificate.RawData;

                // Compute the SHA-256 hash of the certificate
                using (var sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(rawData);

                    // Encode the hash with Base64
                    string base64EncodedHash = Convert.ToBase64String(hashBytes);

                    return base64EncodedHash;
                }
            }
        }

        public static string GetEncodedCertificateHash()
        {
            string certificateFilePath = "Utils/keys/hashed-cert.txt";
            string certificateHash = System.IO.File.ReadAllText(certificateFilePath);
            return EncodeToBase64(certificateHash);
        }


        public static XmlDocument LoadXmlFromFile(string filePath)
        {
            try
            {
                // Load the XML content from the file
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);
                return xmlDoc;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load XML from file. Error: {ex.Message}");
                return null;
            }
        }

        public static async Task RunCommandInCMD(string command)
        {
            // Create a process start info for CMD
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Start the process
            using Process process = new Process { StartInfo = processStartInfo };
            process.Start();

            // Read the standard output and error
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            // Wait for the process to exit
            process.WaitForExit();
            Console.WriteLine($"Command executed successfully. Output: {output}");
        }

        public static string ReadFromFile(string filePath)
        {
            try
            {
                using (StreamReader reader = File.OpenText(filePath))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
                return null;
            }
        }

        public static string RemoveKeyHeaders(string pemKey)
        {
            // Remove the first and last lines (-----BEGIN and -----END)
            pemKey = pemKey
                .Replace("-----BEGIN EC PRIVATE KEY-----", "")
                .Replace("-----BEGIN EC PUBLIC KEY-----", "")
                .Replace("-----END EC PRIVATE KEY-----", "")
                .Replace("-----END EC PUBLIC KEY-----", "")
                .Replace("\r", "")
                .Replace("\n", "")
                .Replace(" ", "");

            return pemKey;
        }
        public static string ExtractCsrContent(string output)
        {
            string beginMarker = "-----BEGIN CERTIFICATE REQUEST-----";
            string endMarker = "-----END CERTIFICATE REQUEST-----";

            int startIndex = output.IndexOf(beginMarker, StringComparison.Ordinal);
            int endIndex = output.IndexOf(endMarker, startIndex + beginMarker.Length, StringComparison.Ordinal);

            if (startIndex != -1 && endIndex != -1)
            {
                int contentStart = startIndex + beginMarker.Length;
                int contentLength = endIndex - contentStart;
                string csrContent = output.Substring(contentStart, contentLength);

                return csrContent;
            }
            else
            {
                // Handle the case where the markers are not found
                Console.WriteLine("Markers not found in the output.");
                return null;
            }
        }
    }
}