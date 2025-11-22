# EasySave Backup Software Documentation

## 1. Introduction
### Software Overview
EasySave is a backup software developed by ProSoft, designed to optimize file copy management. It adheres to the company's development and documentation standards.

### Objective and Use Cases
This software allows users to perform full and differential backups of their data. It is intended for both businesses and individuals.

---
## 2. System Requirements
- **Supported Operating Systems**: Windows 10 and later versions
- **Minimum Configuration**:
  - Processor: 2 GHz or higher
  - RAM: 4 GB
  - Disk Space: 500 MB
  - .NET 8.0 required

---

## 3. Usage

### CLI Usage

#### Commands
**`-h, --help` :** display help.


#### `save`
**Usage :**
```bash
./EasySave.View.exe save [OPTIONS]
```

**Options :**
`-s, --source <PATH>` Source folder path.
`-d, --destination <PATH>` Destination folder path.
`-t, --type <TYPE>` Type of save : `0` for Full, `1` for Differential.
`-h, --help` Prints help for the `save` command.


#### `ls`
Lists all available jobs.


**Usage :**
```bash
./EasySave.View.exe ls
```


#### `exjob <ID>`
Executes a job with its ID.

**Usage :**
```bash
./EasySave.View.exe exjob <ID>
```

**Arguments :**
- `<ID>`: The ID of the job to execute (use `ls` to list available jobs).

#### `exmultjobs <RANGE>`
Executes multiple jobs within a specified range.

**Usage :**

```
./EasySave.View.exe exmultjobs <RANGE>
```

**Arguments :**
`<RANGE>`: Specifies multiple job IDs. (e.g.`1-3`, executes jobs 1 to 3)

### Graphical Interface Usage
#### User Interface
The user interface is now based on WPF (Windows Presentation Foundation), providing an enhanced visual experience and better interactivity.
To launch the GUI, please start the project named EasySave.ViewUI on visual studio 

![](https://hdoc.romainmahieu.fr/uploads/160a258d-d387-4010-b822-1db0485dbfa4.png)


#### Job Execution
- Navigate to the job execution menu through the graphical interface.
- Select the jobs you want to execute by checking the corresponding boxes.
- Click the "Execute jobs" button to run the selected jobs.
![](https://hdoc.romainmahieu.fr/uploads/e9a4d3fe-a8ff-4a0c-8c09-8302c97a6bcc.png)



#### Job Creation and Modification
- Navigate to the job editing menu.
- Select the job to edit from the dropdown list.
- Properly fill in the corresponding field information (any incorrect entry will be notified).
- The creation process follows the same steps.
![](https://hdoc.romainmahieu.fr/uploads/a757ae04-a7eb-442d-99be-78b655535d58.png)


#### Settings
- Access the settings menu.
- To change the language, select the language change menu.
- To revert to default settings if a mistake was made, select the corresponding menu.
- To change the extensions to encrypt during a save, navigate to the corresponding menu.
- To add extensions with higher priority on others, write extensions in the corresponding menu
- To change the max file size in parallel, change the value
- To add a business software that will stop jobs when loading, click the [...] button to select an exe file.
![](https://hdoc.romainmahieu.fr/uploads/d0bbd7f3-b588-4012-b625-96e223e3d920.png)

#### Decrypting a folder
- Access the decryption menu
- Chose the folder to decrypt (file must be the destination of one job)
- Just press Decrypt and that's it !
![](https://hdoc.romainmahieu.fr/uploads/5d7856ae-2085-46e4-bdc6-b4934e7d09a8.png)

### Distant Client 
To launch the GUI, please start the project named DistantClientEasySave.View on visual studio

![](https://hdoc.romainmahieu.fr/uploads/2f13cff1-4cba-48e6-ad88-9ec08f22cc57.png)

You can now act on jobs on the server by lauching, pause, stop or resume them. You can also see the progress of each jobs

![](https://hdoc.romainmahieu.fr/uploads/96e956c5-f15c-4aaf-903d-2e520dc27838.png)


---

## 4. Troubleshooting and FAQ
### Common Issues and Solutions
- **Installation Error**: Ensure that .NET 8.0 is installed.
- **Incomplete Backup**: Verify file access permissions.
- **Backup execution Issue**: Check that the selected version is valid.
- For any error-related issues, a JSON log file is available in the program's execution directory.

---

## 5. Notes and Updates
### Planned Features
- Continuous improvement of the WPF graphical interface.

## 6. Constraints and Best Practices
### Development Constraints
- Use of **Visual Studio 2022** and **GitLab**.
- Use of UML modeling.
- Adherence to **C# .NET 8.0** naming and code structuring conventions.
- Concise user documentation.
- Mandatory release notes.

This documentation will serve as a reference for all versions of EasySave and ensure the project's longevity within ProSoft.
