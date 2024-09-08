using Microsoft.Extensions.Options;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Schema;

namespace ZipFileProcessor.Services
{
    internal class ZipProcessingSettings
    {
        public string ZipFileLocation { get; set; }
        public string ExtractedFolderLocation { get; set; }
        public string[] ValidFileTypes { get; set; }
        public string PartyXmlFileName { get; set; }
        public string PartyXsdFileNameWithLocation { get; set; }
    }

    internal class ZipProcessingService : IZipProcessingService
    {
        private readonly IEmailService _emailService;
        private readonly ILoggerService _logger;
        private readonly ZipProcessingSettings _zipProcessingSettings;

        public ZipProcessingService(IEmailService emailService, ILoggerService logger, IOptions<ZipProcessingSettings> zipProcessingSettings)
        {
            _emailService = emailService;
            _logger = logger;
            _zipProcessingSettings = zipProcessingSettings.Value;
        }

        public async Task ProcessZipFilesAsync()
        {
            LogFunctionEntry();
            try
            {
                _logger.LogInformation("Starting to process ZIP files.");

                // Recursively get all ZIP files in the root directory
                string[] zipFiles = Directory.GetFiles(_zipProcessingSettings.ZipFileLocation, "*.zip", SearchOption.AllDirectories);

                foreach (string zipFilePath in zipFiles)
                {
                    // Process each ZIP file asynchronously
                    await ProcessSingleZipFileAsync(zipFilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while processing ZIP files.", ex);
            }
            LogFunctionExit();
        }

        private async Task ProcessSingleZipFileAsync(string zipFilePath)
        {
            LogFunctionEntry();
            string extractPath = string.Empty;
            try
            {                
                // Generate a unique folder name for extraction
                extractPath = Path.Combine(_zipProcessingSettings.ExtractedFolderLocation, Guid.NewGuid().ToString());
                Directory.CreateDirectory(extractPath);

                // Extract the ZIP file
                ZipFile.ExtractToDirectory(zipFilePath, extractPath);
                _logger.LogInformation($"Extracted '{zipFilePath}' to '{extractPath}'");

                // Validate file types
                ValidateExtractedFiles(extractPath);

                // Validate and rename folder based on applicationno from party.xml
                string partyXmlPath = Path.Combine(extractPath, _zipProcessingSettings.PartyXmlFileName);
                if (File.Exists(partyXmlPath))
                {
                    int applicationNo = GetApplicationNumberFromXml(partyXmlPath);
                    ValidatePartyXml(partyXmlPath, applicationNo);

                    string newFolderName = $"{applicationNo}-{Guid.NewGuid()}";
                    string newFolderPath = Path.Combine(_zipProcessingSettings.ExtractedFolderLocation, newFolderName);

                    Directory.Move(extractPath, newFolderPath);  
                    string message = $"Renamed extracted folder to '{newFolderName}'";
                    _logger.LogInformation(message);

                    // Notify via email
                    string subject = $"Application # {applicationNo} - ZIP File Extracted";
                    message = $"Application # {applicationNo} - ZIP file '{zipFilePath}' processed and extracted to '{newFolderPath}'.";
                    await _emailService.SendEmailAsync(subject, message);
                }
                else
                {
                    throw new FileNotFoundException($"party.xml not found in extracted folder '{extractPath}'");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while processing '{zipFilePath}'.", ex);
                if (!string.IsNullOrEmpty(extractPath) && Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true); // Clean up if something goes wrong
                }
            }
            LogFunctionExit();
        }

        private void ValidateExtractedFiles(string extractPath)
        {
            LogFunctionEntry();
            string[] files = Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories);
            var validExtensions = _zipProcessingSettings.ValidFileTypes.Select(ext => $".{ext.ToLower()}").ToArray();

            foreach (var file in files)
            {
                string fileExtension = Path.GetExtension(file).ToLower();
                if (!validExtensions.Contains(fileExtension))
                {
                    string message = $"Invalid file type '{fileExtension}' found in extracted files: '{file}'.";
                    _logger.LogError(message);

                    throw new Exception(message);
                }
            }

            _logger.LogInformation("All extracted files have valid formats.");
            LogFunctionExit();
        }

        private int GetApplicationNumberFromXml(string xmlFilePath)
        {
            LogFunctionEntry(); 
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFilePath);

            XmlNode applicationNoNode = doc.SelectSingleNode("//party/applicationno");
            if (applicationNoNode == null || !int.TryParse(applicationNoNode.InnerText, out int applicationNo))
            {
                throw new Exception("Invalid or missing applicationno in party.xml");
            }

            LogFunctionExit();
            return applicationNo;
        }


        private void ValidatePartyXml(string xmlFilePath, int applicationNo)
        {
            LogFunctionEntry();
            XmlSchemaSet schema = new XmlSchemaSet();
            schema.Add(null, _zipProcessingSettings.PartyXsdFileNameWithLocation);

            XmlReaderSettings settings = new XmlReaderSettings
            {
                Schemas = schema,
                ValidationType = ValidationType.Schema
            };

            settings.ValidationEventHandler += (sender, e) =>
            {
                if (e.Severity == XmlSeverityType.Warning)
                {
                    _logger.LogWarning($"Warning: {e.Message}");
                }
                else if (e.Severity == XmlSeverityType.Error)
                {
                    // Notify via email
                    string subject = $"Application # {applicationNo} - PartyXml Validation Error ";
                    string message = subject + " : {e.Message}";
                    _emailService.SendEmail(subject, message);

                    _logger.LogError($"Error: {e.Message}");
                    throw new Exception($"Validation error: {e.Message}");
                }
            };

            using (XmlReader reader = XmlReader.Create(xmlFilePath, settings))
            {
                while (reader.Read()) { } // Read the XML file, triggers validation
            }

            string message = $"'{xmlFilePath}' validated successfully against '{_zipProcessingSettings.PartyXsdFileNameWithLocation}'.";
            _logger.LogInformation(message);
            LogFunctionExit();
        }

        private void LogFunctionEntry([CallerMemberName] string methodName = "")
        {
            _logger.LogInformation($"Entering method: {methodName}");
        }
        private void LogFunctionExit([CallerMemberName] string methodName = "")
        {
            _logger.LogInformation($"Exiting method: {methodName}");
        }
    }
}
