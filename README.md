Sure! Here's a polished, well-structured, and visually appealing version of your README file for the **Compression Tool** project:

---

# Compression Tool

## Overview

**Compression Tool** is a Windows Forms application built with C# that enables users to compress and decompress files using multiple algorithms. It supports compressing individual files as well as archiving multiple files into a single compressed archive, providing a flexible and user-friendly interface.

---

## Features

* Compress and decompress individual files.
* Create and extract archives containing multiple files.
* Supports multiple compression algorithms (e.g., Huffman, Shannon-Fano).
* Drag-and-drop file selection.
* Pause, resume, and cancel ongoing operations.
* Progress tracking with real-time status updates.

---

## File Structure

| File Name                       | Description                                                    |
| ------------------------------- | -------------------------------------------------------------- |
| `CompressionManager.cs`         | Main entry point for compression management (currently empty). |
| `CompressionManager.Archive.cs` | Methods for creating, reading, and extracting archives.        |
| `CompressionManager.Legacy.cs`  | Compression/decompression for individual `.cmp` files.         |
| `CompressionManager.Folder.cs`  | Utilities for folder handling and full archiving support.      |

---

## Required Packages

Make sure the following NuGet packages are installed for the project to run correctly:

| Package Name                        | Version                     |
| ----------------------------------- | --------------------------- |
| **AForge**                          | 2.2.5                       |
| - AForge.Video                      | 2.2.5                       |
| - AForge.Imaging                    | 2.2.5                       |
| - AForge.Math                       | 2.2.5                       |
| **Emgu.CV**                         | 4.10.0.5680                 |
| - Emgu.CV.Bitmap                    | 4.10.0.5680                 |
| - Emgu.CV.runtime.windows           | 4.10.0.5680                 |
| **MathNet.Numerics**                | 6.0.0-beta2                 |
| **Microsoft.Windows.Compatibility** | 10.0.0-preview\.5.25277.114 |
| **NAudio**                          | 2.2.1                       |
| **OxyPlot.WindowsForms**            | 2.2.0                       |
| **SixLabors.ImageSharp**            | 3.1.10                      |
| **System.Drawing.Common**           | 10.0.0-preview\.5.25277.114 |

---

## Code Explanation

### `MainForm.cs`

The primary user interface for the application where users interact with compression features.

#### Key Components:

* **Buttons:**

  * `btnBrowse`: Open file dialog for selecting files.
  * `btnCompress`: Starts file compression.
  * `btnDecompress`: Starts file decompression.
  * `btnCancel`: Cancels ongoing operation.
  * `btnPauseResume`: Pauses or resumes compression/decompression.

* **ListBox:**
  Displays selected files for compression or decompression (`listBoxFiles`).

* **ComboBox:**
  Algorithm selection (`comboBoxAlgorithm`), e.g., Huffman, Shannon-Fano.

* **ProgressBar:**
  Shows compression/decompression progress.

* **Labels:**
  Displays current operation status (`labelStatus`).

#### Event Handlers:

* `btnBrowse_Click`: Opens file selector and updates list box.
* `btnCompress_Click`: Handles compression logic and output location.
* `btnDecompress_Click`: Handles decompression and extraction.
* `MainForm_DragEnter` and `MainForm_DragDrop`: Enable drag-and-drop file selection.

---

### `SelectFileFromArchiveForm.cs`

Form for selecting a specific file from an archive for extraction.

* **ListBox:** Shows files inside the archive.
* **Buttons:**

  * `btnOK`: Confirms file selection.
  * `btnCancel`: Closes the form without selection.

---

### `Program.cs`

Application entry point — initializes and launches the main form.

---

## How to Use

1. Click **Browse** to select files for compression or decompression.
2. Choose a compression algorithm from the dropdown.
3. Click **Compress** to compress files or **Decompress** to extract.
4. Use **Pause/Resume** or **Cancel** buttons to control ongoing operations.
5. Drag and drop files directly into the main window for quick selection.

---




