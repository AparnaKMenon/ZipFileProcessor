Objective

Files are uploaded to the courts website by a party to a case and are packaged as a ZIP file and automatically downloaded to a server on our network.
You are building a solution to process these ZIP files once they have reached our network.
A valid ZIP file has the following attributes:
	1. It is a ZIP file
	2. The ZIP file is not corrupt, that is the files inside can be queried
	3. The ZIP file contains only XLS|X, DOC|X, PDF, MSG and image files (sample documents attached)
	4. The ZIP file also contains an XML file called party.XML and it should the structured as per the XSD file attached.
If the file is valid it should be extracted to a folder named – [applicationno]-[guid], e.g. 1700017-CCD375E5-D546-44C6-A739-8267F4ABDA83 and a notification sent by email to an administrator.
If the file is not valid then an error should be raised and a notification sent by email to an administrator.
All work done by the system should be logged.
The system should be configurable as it is expected that requirements such as the administrator email, the extracted folder location, file types and the names of attributes in the XML file may change over time.
