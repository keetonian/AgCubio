I certify that the work to create this GUI was done entirely by myself and my partner – Daniel Avery, Keeton Hodgson



--- Build Guidelines (from Assignment Instructions)



Single window on startup, empty spreadsheet

One or more independent spreadsheet windows possible

Application closes when last window terminates

UI with 26 columns [A-Z] and 99 rows [1-99]

Standard layout - A1 in upper left, Z99 in lower right

Values of cells displayed in the grid

Cell names are case insensitive (a1 = A1)

Cells not on grid are invalid

Read/write version string = ‘ps6’

Always have one selected cell, with default start cell

At least one way to select a different cell

Independent of grid, need:
	A non-editable text box showing the cell name of the selected cell
	A non-editable text box showing the cell value of the selected cell
	An editable text box showing the contents of the selected cell

Include way to change the contents of the selected cell

Successful cell change updates displayed value of each dependent cell

Unsuccessful cell change displays error message and does not change the spreadsheet

File Menu that allows:
	Creation of new empty spreadsheet in its own GUI window
	Saving of the current spreadsheet to a file
	Opening of a spreadsheet from a file, replacing the current
	Closing of the spreadsheet window

Default to use of .sprd extension for file saving/reading

All file dialogs should allow the user to (1) choose whether to display only .sprd files or (2) to display all files. Option 1 should be the default.
	If the user chooses option 1, all files that are selected or entered must have the .sprd extension
	If the user chooses option 2, this restriction is not imposed.

If an operation would result in the loss of unsaved data, a warning dialog is displayed asking to save the data

If an operation would result in a file being overwritten (other than the file to which the current spreadsheet was most recently saved), a warning dialog is displayed. If any of these operations result in exceptions, a suitable error message is displayed.

Help menu that the TAs can use to learn how to use spreadsheet. Must explain how to change selections and edit cell contents.
	Additional feature(s) documented here (as well as in code)

Coded UI test suite that automatically exercises GUI interface

Required additional features to uniquely define this spreadsheet



--- Design Decisions



[11/4/15] Put the Help information in a pdf opened by About Spreadsheet

[11/4/15] Handle all Keypad/Enter/Tab Scrolling in the SpreadsheetPanel

[11/4/15] Show FormulaError message in dialog box when cell is set

[11/4/15] Have empty cells display “Enter cell contents” in the Text Editor

[11/4/15] Allow last-chance saving when opening a file, before the existing spreadsheet is replaced

[11/4/15] Dock the Text Editor on top of the grid, with File Menu and cell name and value displayed above

[11/3/15] Display FormulaErrors as “FormulaERR” values

[11/3/15] Evaluate all dependent cells during the cell setting

[11/3/15] Save values to the SpreadsheetPanel cell address (along with Spreadsheet) for immediate access in value label setting

[10/31/15] Use SpreadsheetPanel.csproj, copy project into repository so that both partners can pull from git without problems (presumably from the remote dll)



--- External Code Resources



PS6 Skeleton Code – SpreadsheetPanel.csproj



--- Implementation Notes



Additional Feature: Help Menu Request Support
	Link to mailing Keeton Hodgson

Additional Feature: Help Menu Easter Eggs
	Allow change of background colors

Additional Feature: Keypad/Enter/Tab Scrolling

Additional Feature: Keypad/Enter/Tab Navigating

Additional Feature: Delete Key Empties Cell

Note: First key press (other than Enter or Tab) focuses on the cell’s Text Editor. The 	value of the key is not recorded in the Text Editor.

Note: Tab within Text Editor immediately focuses on next cell’s Text Editor, while Enter within Text Editor does not.

Note: Clicking to a new cell while in the Text Editor leaves the previous cell unchanged.

Venting: While git has been good for solo versioning, it seemed to make working in pairs harder. The need to commit before reverting and then syncing,
	along with the consistent need to diff to resolve conflicts in the merge, both proved very tiresome and eventually made us prefer to avoid changes
	whenever not absolutely necessary.


And Smiles :)