using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
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
        }

        private async Task ProcessSingleZipFileAsync(string zipFilePath)
        {
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
                    ValidatePartyXml(partyXmlPath);

                    string newFolderName = $"{applicationNo}-{Guid.NewGuid()}";
                    string newFolderPath = Path.Combine(_zipProcessingSettings.ExtractedFolderLocation, newFolderName);

                    Directory.Move(extractPath, newFolderPath);                   
                    _logger.LogInformation($"Renamed extracted folder to '{newFolderName}'");

                    // Notify via email
                    string message = $"ZIP file '{zipFilePath}' processed and extracted to '{newFolderPath}'.";
                    await _emailService.SendEmailAsync("ZIP File Processed", message);
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
        }

        private void ValidateExtractedFiles(string extractPath)
        {
            string[] files = Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories);
            var validExtensions = _zipProcessingSettings.ValidFileTypes.Select(ext => $".{ext.ToLower()}").ToArray();

            foreach (var file in files)
            {
                string fileExtension = Path.GetExtension(file).ToLower();
                if (!validExtensions.Contains(fileExtension))
                {
                    throw new Exception($"Invalid file type '{fileExtension}' found in extracted files: '{file}'.");
                }
            }

            _logger.LogInformation("All extracted files have valid formats.");
        }

        private int GetApplicationNumberFromXml(string xmlFilePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFilePath);

            XmlNode applicationNoNode = doc.SelectSingleNode("//party/applicationno");
            if (applicationNoNode == null || !int.TryParse(applicationNoNode.InnerText, out int applicationNo))
            {
                throw new Exception("Invalid or missing applicationno in party.xml");
            }

            return applicationNo;
        }


        private void ValidatePartyXml(string xmlFilePath)
        {
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
                    _logger.LogError($"Error: {e.Message}");
                    throw new Exception($"Validation error: {e.Message}");
                }
            };

            using (XmlReader reader = XmlReader.Create(xmlFilePath, settings))
            {
                while (reader.Read()) { } // Read the XML file, triggers validation
            }

            _logger.LogInformation($"'{xmlFilePath}' validated successfully against '{_zipProcessingSettings.PartyXsdFileNameWithLocation}'.");
        }
    }
}
