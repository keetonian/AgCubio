// Written by Daniel Avery and Keeton Hodgson
// November 2015, Version 1.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SS;
using System.Text.RegularExpressions;
using SpreadsheetUtilities;

namespace SpreadSheetGUI
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// The spreadsheet that this form contains.
        /// </summary>
        private Spreadsheet ss;


        /// <summary>
        /// Constructor. 
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            // Construct underlying spreadsheet, validating grid cells, normalizing to uppercase,
            //   and using version "ps6" -- to specs
            ss = new Spreadsheet
                (s => Regex.IsMatch(s, @"^[a-zA-Z]{1}[1-9]{1}[0-9]?$"), s => s.ToUpper(), "ps6");

            // Add the selection changed event to the panels so that you can update them.
            this.spreadsheetPanel1.SelectionChanged += selectionChanged;
        }


        /// <summary>
        /// Closes the program.
        /// </summary>
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Saves the current instance of the spreadsheet to the specified file location
        /// </summary>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFile();
        }


        /// <summary>
        /// Helper method -- opens a SaveFileDialog for spreadsheet saving
        /// </summary>
        private void saveFile()
        {
            // Brings up a dialogue box that saves the current spreadsheet with the name that is wanted. Version = ps6.
            SaveFileDialog savefile = new SaveFileDialog();

            // Set a default file name
            savefile.FileName = "untitled.sprd";
            // Set extension filters
            savefile.Filter = "Spreadsheet files (*.sprd)|*.sprd|All files (*.*)|*.*";

            if (savefile.ShowDialog() == DialogResult.OK)
                ss.Save(savefile.FileName);
        }


        /// <summary>
        /// Opens a file into the current spreadsheet window, replacing the current spreadsheet
        /// </summary>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Called when someone clicks open in the file menu, or presses Ctrl+O
            // Opens the file menu, allows the user to specify a spreadsheet to open, and loads it into the screen.
            OpenFileDialog openfile = new OpenFileDialog();

            // Set extension filters
            openfile.Filter = "Spreadsheet files (*.sprd)|*.sprd|All files (*.*)|*.*";

            // If we are opening a file
            if (openfile.ShowDialog() == DialogResult.OK)
            {
                // If the current spreadsheet has been modified, display a dialog for saving it
                if (ss.Changed)
                {
                    DialogResult result = MessageBox.Show
                    ("Unsaved changes. Save changes to existing sheet?",
                    "Warning", MessageBoxButtons.YesNo,MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1);

                    if (result == DialogResult.Yes)
                        saveFile();
                }

                // Empty the values of all the current nonempty cells
                foreach (string cell in ss.GetNamesOfAllNonemptyCells())
                {
                    int depCol, depRow;
                    nameToCoords(cell, out depCol, out depRow);

                    spreadsheetPanel1.SetValue(depCol, depRow, "");
                }

                // Load the new underlying spreadsheet from the file
                this.ss = new Spreadsheet
                    (openfile.FileName, s => Regex.IsMatch(s, @"^[a-zA-Z]{1}[1-9]{1}[0-9]?$"),
                    s => s.ToUpper(), "ps6");

                // Populate the panels with cell values from the new spreadsheet
                foreach (string cell in ss.GetNamesOfAllNonemptyCells())
                {
                    int depCol, depRow;
                    nameToCoords(cell, out depCol, out depRow);

                    spreadsheetPanel1.SetValue(depCol, depRow, GetGUIAdjustedValue(cell));
                }
            }
        }

        /// <summary>
        /// Runs a new spreadsheet if File > New is selected
        /// </summary>
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SpreadsheetAppContext.getAppContext().RunForm(new Form1());
        }

        /// <summary>
        /// Halts the close of the spreadsheet if there are unsaved changes.
        /// Puts up a message box asking the user if they want to save before closing.
        /// </summary>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ss.Changed)
            {
                DialogResult result = MessageBox.Show
                    ("Unsaved changes. Save before closing?", "Warning",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1);

                if (result == DialogResult.Cancel)
                    e.Cancel = true;
                else if (result == DialogResult.Yes)
                    saveFile();
            }
        }


        /// <summary>
        /// Gets the values of the new selection
        /// </summary>
        private void selectionChanged(SpreadsheetPanel sp)
        {
            // Get the current row and col
            int row, col;
            sp.GetSelection(out col, out row);

            // Set the Value Box to the value stored for the current cell's address in ssPanel
            string value;
            sp.GetValue(col, row, out value);
            this.Value_Box.Text = value;

            // Get the cell name based on the coordinates
            string name;
            coordsToName(col, row, out name);

            // Set the Cell Box to display the current cell name
            this.Cell_Box.Text = name;

            // Set the Text Editor to display the current cell's contents
            // If the contents are empty, display a default message
            object contents = ss.GetCellContents(name);
            if (contents.Equals(""))
                this.Text_Editor.Text = "Enter cell contents";
            else if (contents is Formula)
                this.Text_Editor.Text = '=' + contents.ToString();
            else
                this.Text_Editor.Text = contents.ToString();
            
            // Return to the panel selection
            this.spreadsheetPanel1.Focus();
        }



        /// <summary>
        /// This allows basic movements through the panels.
        /// </summary>
        private void spreadsheetPanel1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;

            int col, row;
            this.spreadsheetPanel1.GetSelection(out col, out row);

            // Use the pressed key to determine where to navigate/what to do
            switch (e.KeyData)
            {
                case (Keys.Enter):
                    row += 1;
                    break;

                case (Keys.Tab):
                    col += 1;
                    break;

                case (Keys.Up):
                    row -= 1;
                    break;

                case (Keys.Down):
                    row += 1;
                    break;

                case (Keys.Left):
                    col -= 1;
                    break;

                case (Keys.Right):
                    col += 1;
                    break;

                // Special non-navigation case: deletes the cell
                case (Keys.Delete):
                    SetSpreadsheetValue(col, row, "");
                    break;

                // If other keys were pressed, go to the Text Editor for the cell
                default:
                    this.Text_Editor.Focus();
                    return;
            }

            // Update the selected cell
            this.spreadsheetPanel1.SetSelection(col, row);
            selectionChanged(spreadsheetPanel1);
        }

        /// <summary>
        /// If Enter or Tab is pressed while in the Text_Editor, it shifts focus back to the panels
        /// Updates the cell contents.
        /// </summary>
        private void Text_Editor_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            this.Text_Editor.Focus();
            int col, row;
            spreadsheetPanel1.GetSelection(out col, out row);

            if (e.KeyData == Keys.Enter || e.KeyData == Keys.Tab)
            {
                // Set the cell based on the text in the Editor
                SetSpreadsheetValue(col, row, Text_Editor.Text);
                
                // Switch focus back to the panels so that navigation works.
                spreadsheetPanel1.Focus();

                if (e.KeyData == Keys.Enter)
                    row += 1;
                else
                    col += 1;
                
                // Update the selected cell
                this.spreadsheetPanel1.SetSelection(col, row);
                selectionChanged(spreadsheetPanel1);
            }

        }



        /// <summary>
        /// Set the contents of the cell and values of all dependents
        /// </summary>
        private void SetSpreadsheetValue(int col, int row, string contents)
        {
            // Get the name of the cell from the coordinates
            string name;
            coordsToName(col, row, out name);

            
            try
            {
                // Set the values for all dependent cells (skip the current cell)
                foreach (string cell in ss.SetContentsOfCell(name, contents).Skip(1))
                {
                    int depCol, depRow;
                    nameToCoords(cell, out depCol, out depRow);

                    spreadsheetPanel1.SetValue(depCol, depRow, GetGUIAdjustedValue(cell));
                }

                // Set the value and Value Box for the current cell, show dialog if FormulaError
                object value = ss.GetCellValue(name);
                if (value is FormulaError)
                {
                    MessageBox.Show(((FormulaError)value).Reason);
                    this.Value_Box.Text = "FormulaERR";
                }
                else
                    this.Value_Box.Text = value.ToString();

                spreadsheetPanel1.SetValue(col, row, this.Value_Box.Text);
            }
            catch(FormulaFormatException e)
            {
                // Display an error dialogue (the spreadsheet will not be changed)
                MessageBox.Show("(Formula rejected)\n" + e.Message,
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1);
            }
            catch (CircularException)
            {
                // Display an error dialogue (again, the spreadsheet will not be changed)
                MessageBox.Show("(Formula rejected)\nFormula results in circular dependency.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1);
            }
        }


        /// <summary>
        /// Gives the string version of a cell's value, or "FormulaERR" if it is a FormulaError
        /// </summary>
        private string GetGUIAdjustedValue(string name)
        {
            object value = ss.GetCellValue(name);
            return (value is FormulaError) ? "FormulaERR" : value.ToString();
        }

        /// <summary>
        /// Outputs the cell name based on the input coordinates
        /// </summary>
        private void coordsToName(int col, int row, out string name)
        {
            name = "" + (char)(col + 65) + (row + 1);
        }


        /// <summary>
        /// Outputs the coordinates based on the input cell name
        /// </summary>
        private void nameToCoords(string name, out int col, out int row)
        {
            col = (int)name.ElementAt(0) - 65;
            row = int.Parse(name.Substring(1)) - 1;
        }


        /// <summary>
        /// Sets background color to red
        /// </summary>
        private void redToolStripMenuItem_Click(object sender, EventArgs e)
        {
            spreadsheetPanel1.color = Color.Red;
            spreadsheetPanel1.UpdateColor();
        }


        /// <summary>
        /// Sets background color to white
        /// </summary>
        private void whiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            spreadsheetPanel1.color = Color.White;
            spreadsheetPanel1.UpdateColor();
        }


        /// <summary>
        /// Sets background color to blue
        /// </summary>
        private void blueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            spreadsheetPanel1.color = Color.LightBlue;
            spreadsheetPanel1.UpdateColor();
        }


        /// <summary>
        /// Sets background color to orange
        /// </summary>
        private void orangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            spreadsheetPanel1.color = Color.LightSalmon;
            spreadsheetPanel1.UpdateColor();
        }


        /// <summary>
        /// Sets background color to green
        /// </summary>
        private void greenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            spreadsheetPanel1.color = Color.LightGreen;
            spreadsheetPanel1.UpdateColor();
        }


        /// <summary>
        /// Sets background color to purple
        /// </summary>
        private void purpleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            spreadsheetPanel1.color = Color.Orchid;
            spreadsheetPanel1.UpdateColor();
        }


        /// <summary>
        /// Sets the user up to mail to my dear friend Keeton at customer support
        /// </summary>
        private void supportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:keeton.hodgson@utah.edu?subject=Support Request");
        }
        

        /// <summary>
        /// Opens the pdf file containg the help/introduction documentation
        /// </summary>
        private void aboutSpreadsheetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("IExplore.exe", "coe.utah.edu/~hodgson/spreadsheet.pdf");
        }
    }
}