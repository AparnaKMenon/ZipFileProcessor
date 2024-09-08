

# Court Document ZIP File Processor

This application processes ZIP files that are uploaded to the court's website by a party to a case and automatically downloaded to a server on our network. The solution validates and processes these files based on defined criteria.

## Table of Contents

- [Features](#features)
- [Requirements](#requirements)
- [Configuration](#configuration)
- [Installation](#installation)
- [Usage](#usage)
- [Logging](#logging)
- [Error Handling](#error-handling)
- [Future Enhancements](#future-enhancements)

## Features

- Validates ZIP files based on several conditions:
  - Must be a valid ZIP file.
  - Must not be corrupt (the internal files should be readable).
  - Should only contain XLS/X, DOC/X, PDF, MSG, and image files.
  - Must include an XML file named `party.xml`, conforming to the provided XSD schema.
- Extracts valid ZIP files into a folder named `[applicationno]-[guid]`, e.g., `1700017-CCD375E5-D546-44C6-A739-8267F4ABDA83`.
- Sends an email notification to the administrator on success or failure.
- Logs all actions performed by the system.
- Configurable settings via `appsettings.json` and `secrets.json`.

## Requirements

- .NET 6.0 or later
- A server to automatically download ZIP files.
- Access to an SMTP server for email notifications.
- The XSD file to validate `party.xml`:
  
  ```xml
  <xs:schema attributeFormDefault="unqualified" elementFormDefault="qualified" xmlns:xs="http://www.w3.org/2001/XMLSchema">
    <xs:element name="party">
      <xs:complexType>
        <xs:sequence>
          <xs:element type="xs:string" name="name"/>
          <xs:element type="xs:string" name="email"/>
          <xs:element type="xs:int" name="applicationno"/>
        </xs:sequence>
      </xs:complexType>
    </xs:element>
  </xs:schema>
  ```

## Configuration

### `appsettings.json`

This file contains key configuration values such as the administrator email, extracted folder path, valid file types, and XML attribute names. Here’s a sample configuration:

```json
"ZipProcessingSettings": {
        "ZipFileLocation": "./ZipSourceFolder",
        "ExtractedFolderLocation": "./Extracted-files",
        "ValidFileTypes": [ "xls", "xlsx", "doc", "docx", "pdf", "msg", "jpg", "jpeg", "png", "bmp", "gif", "tiff", "webp", "svg", "xml" ],
        "PartyXmlFileName": "party.xml",
        "PartyXsdFileNameWithLocation": "./party.xsd"
    },
    ...
```

### `secrets.json`

Sensitive data such as SMTP credentials are stored here. Make sure to set up secrets securely in the development environment.

```json
 "EmailSettings": {
        "SmtpHost": "*********************.com",
        "SmtpPort": 587,
        "SenderEmail": "*************@********.com",
        "SenderPassword": "**********************",
        "AdminEmail": "*************@********.com"
    } ...
```

### Logging

The application logs all activities and errors. The log file location can be configured in `appsettings.json`.

## Installation

1. Clone the repository.
2. Restore dependencies:
   ```bash
   dotnet restore
   ```
3. Build the project:
   ```bash
   dotnet build
   ```
4. Ensure that `appsettings.json` and `secrets.json` are correctly configured.
5. Deploy the application to your server.

## Usage

Once deployed, the application will monitor the designated folder for incoming ZIP files. It will:

- Validate the ZIP file.
- Extract it if valid or raise an error if not.
- Send a notification email to the administrator.

To manually run the application:
```bash
dotnet run
```

## Error Handling

If the ZIP file is invalid (either corrupted or contains unsupported file types), the application will:

- Raise an error.
- Log the error details.
- Send an email notification to the administrator.

## Future Enhancements

- UnitTest Projects for all Services
- More robust error logging and check on every error condition based on end to end testing.
- Coding optimization

