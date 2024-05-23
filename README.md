# Remote System Information Gathering

## Overview
This project facilitates the remote gathering of system information by enabling communication between a server and multiple clients. The server manages client connections and allows the execution of remote Windows Management Instrumentation (WMI) queries.

## Features
- **Server-Client Communication:** The server manages a series of client connections, facilitating remote WMI query execution.
- **Client List Transmission:** Upon connection, the server sends the list of existing clients to the newly connected client. Simultaneously, the server notifies all existing clients to add the new client to their list.
- **WMI Query Execution:** Clients compose WMI queries and select either all clients or a subset of clients to execute the command remotely. The server notifies each selected client to execute the received command and collects the displayed result.
- **Result Retrieval:** The server responds to the client who provided the command with the execution results, individually for each selected client.
- **Client Disconnect Handling:** When a client application closes, it notifies the server to request that the remaining clients remove it from the list of existing clients.

## Client Interaction

When a new client connects to the server or an existing client disconnects, the client application automatically receives the updated list of connected clients.

To interact with the connected clients, follow these steps:

- **View Connected Clients:** The list of connected clients is automatically updated whenever a new client connects or an existing client disconnects.

- **Send WMI Query:** Compose a WMI query (e.g., `SELECT * FROM Win32_Battery`) and specify the IDs of the clients to which you want to send the instruction (e.g., `1,2,3`).

- **Disconnect:** Type `exit` to disconnect from the server and close the client application.

  
## How to Run
1. **Open Terminals:**
   - Open several terminal windows.
   - In each terminal, navigate to the folder containing the solution by using the command `cd path/to/solution/folder`.

2. **Start the Server:**
   - In the first terminal, run the application with `dotnet run`.
   - Press `1` to start the server.

3. **Start the Clients:**
   - In the other terminals, run the application with `dotnet run`.
   - Press `2` to start each client.
   - You can open multiple terminals for clients to send commands from one client to another.

4. Upon connection, the client will receive a list of connected clients.
5. To send a WMI query, enter a WMI instruction like `SELECT * FROM Win32_Battery` and then the IDs of the target clients (e.g., `1,2,3`).
6. Results for each selected client will be displayed.
7. To disconnect, type `exit`.

  
## Sample WMI Queries

Here are some common WMI queries that you can use to retrieve system information:

- **Get Operating System Information:**
  ```sql
  SELECT * FROM Win32_OperatingSystem
Returns information about the operating system, including its version and other details.

- **Display Processor Information:**
  ```sql
  SELECT * FROM Win32_Processor
Provides details about processors, including their name, number of cores, and frequency.

- **List Installed Drivers:**
  ```sql
  SELECT * FROM Win32_PnPSignedDriver
Lists the installed drivers on the system.

- **Check Battery Status (for laptops only):**
  ```sql
  SELECT * FROM Win32_Battery
Checks the battery status, applicable only to laptops.

- **Show Disk Information:**
  ```sql
  SELECT * FROM Win32_DiskDrive
Retrieves details about disk drives, including their model, free space, and type.

- **List All Running Processes:**
  ```sql
  SELECT * FROM Win32_Process
Lists all running processes.

- **Check Service Status:**
  ```sql
  SELECT * FROM Win32_Service
Retrieves information about services, including their status, name, and type.

- **Display Video Controller Information:**
  ```sql
  SELECT * FROM Win32_VideoController
Provides details about the video controller, including its model and video memory.

- **Check RAM Memory Status:**
  ```sql
  SELECT * FROM Win32_PhysicalMemory
Retrieves information about RAM modules.

- **Show User Information:**
  ```sql
  SELECT * FROM Win32_UserAccount
Displays details about local users.

You can use these queries to gather specific information about the system using WMI.

## Final Notes
This project demonstrates a client-server architecture for remote system information gathering using WMI queries. It allows efficient management of multiple client connections and provides a robust mechanism for executing and collecting the results of WMI queries remotely.
