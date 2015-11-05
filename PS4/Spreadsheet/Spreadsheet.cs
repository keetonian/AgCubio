// Written by Daniel Avery, updated in correspondence with Keeton Hodgson
// November 2015, Version 1.1

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpreadsheetUtilities;
using System.Text.RegularExpressions;
using System.Xml;
using System.Threading;

namespace SS
{
    /// <summary>
    /// An implementation of the AbstractSpreadsheet class, utilizing a DependencyGraph and a
    /// Dictionary to store cells and their dependencies. All features of the parent class,
    /// including cell manipulation, spreadsheet file i/o, exception throwing and handling,
    /// validation, normalization, and versioning are included as detailed.
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        /// <summary>
        /// Use a DependencyGraph to track dependencies in the spreadsheet
        /// </summary>
        private DependencyGraph Dependencies;


        /// <summary>
        /// Use a Dictionary to map cell names to cell contents
        /// </summary>
        private Dictionary<string, Cell> Cells;


        // ADDED FOR PS5
        /// <summary>
        /// True if this spreadsheet has been modified since it was created or saved                  
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public override bool Changed { get; protected set; }


        // ADDED FOR PS5
        /// <summary>
        /// Zero-parameter constructor makes a spreadsheet with "default" version, a validator that
        /// returns true for any string input, and a normalizer that normalizes cell names to
        /// themselves.
        /// </summary>
        public Spreadsheet()
            : this(s => true, s => s, "default")
        {
        }


        // ADDED FOR PS5
        /// <summary>
        /// Constructs a spreadsheet by recording its variable validity test,
        /// its normalization method, and its version information.  The variable validity
        /// test is used throughout to determine whether a string that consists of one or
        /// more letters followed by one or more digits is a valid cell name.  The variable
        /// equality test should be used thoughout to determine whether two variables are
        /// equal.
        /// </summary>
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version)
            : base(isValid, normalize, version)
        {
            Dependencies = new DependencyGraph();
            Cells = new Dictionary<string, Cell>();
            Changed = false;
        }


        // ADDED FOR PS5
        /// <summary>
        /// Constructs a spreadsheet by reading a saved spreadsheet from a file, and applying
        /// the given validator, normalizer, and version
        /// </summary>
        public Spreadsheet(string filename, Func<string, bool> isValid, Func<string, string> normalize, string version)
            : this(isValid, normalize, version)
        {
            try
            {
                // Use an XmlReader to build a spreadsheet from filename
                using (XmlReader reader = XmlReader.Create(filename))
                {
                    // Initialize variables to hold cell names and contents
                    string name = "", contents = "";

                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "spreadsheet":
                                    // If versions do not match, throw an exception
                                    if (!reader["version"].Equals(version))
                                        throw new SpreadsheetReadWriteException("Version parameter "
                                        + "does not match saved version.");
                                    break;
                                    
                                case "name":
                                    reader.Read();
                                    name = reader.Value;
                                    break;

                                case "contents":
                                    reader.Read();
                                    contents = reader.Value;

                                    // Should now have name and contents to set a cell
                                    try
                                    {
                                        SetContentsOfCell(name, contents);
                                    }

                                    // Look for a number of possible spreadsheet errors
                                    catch (InvalidNameException)
                                    {
                                        throw new SpreadsheetReadWriteException("File contains inva"
                                            + "lid cell name: " + name);
                                    }
                                    catch (CircularException)
                                    {
                                        throw new SpreadsheetReadWriteException("File contains a ci"
                                            + "rcular dependency, introduced by cell " + name);
                                    }
                                    catch (FormulaFormatException e)
                                    {
                                        throw new SpreadsheetReadWriteException("File contains inva"
                                            + "lid formula in cell " + name + ": " + e.Message);
                                    }

                                    break;
                            }
                        }
                    }
                }
            }

            // If something else went wrong while reading the file
            catch (SystemException e) 
            {
                throw new SpreadsheetReadWriteException("Error reading from file: " +  e.Message);
            }
        }


        // ADDED FOR PS5
        /// <summary>
        /// Returns the version information of the spreadsheet saved in the named file.
        /// If there are any problems opening, reading, or closing the file, the method
        /// should throw a SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override string GetSavedVersion(String filename)
        {
            try
            {
                // Use an XmlReader to look through filename until version info is found
                using (XmlReader reader = XmlReader.Create(filename))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            if (reader.Name.Equals("spreadsheet"))
                                    return reader["version"];
                        }
                    }
                }
            }

            // If something went wrong while reading the file
            catch (SystemException e)
            {
                throw new SpreadsheetReadWriteException("Error reading from file: " + e.Message);
            }

            // If no version information was found in the file
            throw new SpreadsheetReadWriteException("No version information found.");
        }


        // ADDED FOR PS5
        /// <summary>
        /// Writes the contents of this spreadsheet to the named file using an XML format.
        /// The XML elements should be structured as follows:
        /// 
        /// <spreadsheet version="version information goes here">
        /// 
        /// <cell>
        /// <name>
        /// cell name goes here
        /// </name>
        /// <contents>
        /// cell contents goes here
        /// </contents>    
        /// </cell>
        /// 
        /// </spreadsheet>
        /// 
        /// There should be one cell element for each non-empty cell in the spreadsheet.  
        /// If the cell contains a string, it should be written as the contents.  
        /// If the cell contains a double d, d.ToString() should be written as the contents.  
        /// If the cell contains a Formula f, f.ToString() with "=" prepended should be written as the contents.
        /// 
        /// If there are any problems opening, writing, or closing the file, the method should throw a
        /// SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override void Save(String filename)
        {
            try
            {
                // use an XmlWriter to write a properly-formatted spreadsheet to filename
                using (XmlWriter writer = XmlWriter.Create(filename))
                {
                    writer.WriteStartDocument();

                    writer.WriteStartElement("spreadsheet");
                    writer.WriteAttributeString("version", Version);

                    // Print each cell, with name and contents, formatted
                    foreach (var Cell in Cells)
                    {
                        writer.WriteStartElement("cell");

                        writer.WriteElementString("name", Cell.Key);
                        writer.WriteElementString("contents", Cell.Value.ToString());

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();

                    writer.WriteEndDocument();
                }

                // The spreadsheet is saved; set Changed to false
                Changed = false;
            }
            catch(Exception e)
            {
                throw new SpreadsheetReadWriteException(e.Message);
            }
        }


        // ADDED FOR PS5
        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </summary>
        public override object GetCellValue(String name)
        {
            if (Cells.ContainsKey(name))
            {
                // If the cell has already been evaluated (and unaffected by other cell setters),
                //   get its value directly
                if (Cells[name].value != null)
                    return Cells[name].value;

                // Get the cell content
                object content = GetCellContents(name);

                // If the content is a Formula, the value is the evaluated Formula; otherwise the value
                //   is just the content
                if (content is Formula)
                {
                    // Get the formula variables and map each to values for the lookup
                    Dictionary<string, double> map = new Dictionary<string, double>();
                    IEnumerable<string> vars = ((Formula)content).GetVariables();

                    foreach (string v in vars)
                    {
                        object value = GetCellValue(v);

                        if (value is double)
                            map[v] = (double)value;
                    }

                    return Cells[name].value = ((Formula)content).Evaluate(s => map[s]);
                }

                return Cells[name].value = content;
            }

            return "";
        }


        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<String> GetNamesOfAllNonemptyCells()
        {
            // Cells.Keys are names of initialized cells
            return Cells.Keys;
        }


        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(String name)
        {
            // Error-checking
            if (!isCellName(name))
                throw new InvalidNameException();

            // Normalize the cell name
            name = Normalize(name);

            // Return an empty string if the cell is empty
            if (Cells.ContainsKey(name))
                return Cells[name].content;

            return "";
        }


        // ADDED FOR PS5
        /// <summary>
        /// If content is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        /// 
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor.  There are then three possibilities:
        /// 
        ///   (1) If the remainder of content cannot be parsed into a Formula, a 
        ///       SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       
        ///   (2) Otherwise, if changing the contents of!isCellName(name) the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown.
        ///       
        ///   (3) Otherwise, the contents of the named cell becomes f.
        /// 
        /// Otherwise, the contents of the named cell becomes content.
        /// 
        /// If an exception is not thrown, the method returns a set consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public override ISet<String> SetContentsOfCell(String name, String content)
        {
            // Error-checking on name and content
            if (content == null)
                throw new ArgumentNullException();

            if (!isCellName(name))
                throw new InvalidNameException();

            // Normalize the cell name
            name = Normalize(name);

            // Check if content is empty; if so, short-circuit
            if (content == "")
            {
                // If the cell is initialized, "remove" it and its dependencies
                if (Cells.ContainsKey(name))
                {
                    Dependencies.ReplaceDependents(name, new List<String>());
                    Cells.Remove(name);
                }

                // Return the set with name and dependents
                HashSet<String> result = new HashSet<String>(GetCellsToRecalculate(name));

                // Reset any stored values for dependents
                foreach (string cell in result.Skip(1))
                    Cells[cell].value = null;

                return result;
            }

            // Determine which procedure to perform based on whether content is a double, Formula,
            //   or just a string

            // Double
            double number;

            if (Double.TryParse(content, out number))
                return SetCellContents(name, number);
            
            // Formula
            if (content.Length > 1 && content.ElementAt(0) == '=')
                    return SetCellContents
                        (name, new Formula(content.Substring(1), Normalize, isCellName));

            // String
            return SetCellContents(name, content);
        }


        // MODIFIED PROTECTION FOR PS5
        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<String> SetCellContents(String name, double number)
        {
            return SetDoubleOrString(name, number);
        }


        // MODIFIED PROTECTION FOR PS5
        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<String> SetCellContents(String name, String text)
        {
            return SetDoubleOrString(name, text);
        }


        // MODIFIED PROTECTION FOR PS5
        /// <summary>
        /// If formula parameter is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException.
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// Set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<String> SetCellContents(String name, Formula formula)
        {
            // Save the current cell contents, in case something goes wrong
            string prevContent = GetCellContents(name).ToString();

            // Set the cell content (return early if new content matches old)
            if (!SetCell(name, formula))
                return new HashSet<String>(GetCellsToRecalculate(name));

            // Replace any dependencies where this cell is dependent (this may cause circular
            //   dependencies; we will deal with those next)
            Dependencies.ReplaceDependents(name, formula.GetVariables());

            // Return the resultant set. If circular dependencies are created, an exception would
            //   be thrown - but first we would undo the cell setting
            try
            {
                HashSet<String> result = new HashSet<String>(GetCellsToRecalculate(name));

                // Reset any stored values for dependents
                foreach (string cell in result)
                    Cells[cell].value = null;

                // If getting the resultant set threw no exceptions, we will be keeping the changes
                //   to the cell, so we can set Changed to true
                Changed = true;

                return result;
            }
            catch (CircularException e)
            {
                // If an exception was thrown, we'll reset the cell with the original contents,
                //   noting that we are making no changes (then we'll throw the exception)
                bool alreadyChanged = Changed;

                SetContentsOfCell(name, prevContent);

                Changed = alreadyChanged;

                throw e;
            }
        }


        /// <summary>
        /// If name is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
        /// 
        /// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected override IEnumerable<String> GetDirectDependents(String name)
        {
            // Return the direct dependents of name
            return Dependencies.GetDependees(name);
        }


        /// <summary>
        /// Checks if _string follows the convention of a spreadsheet cell name
        /// </summary>
        /// <param name="name">The string to be checked</param>
        /// <returns>true if the format is a match; false if not</returns>
        private bool isCellName(string name)
        {
            return (name != null
                && Regex.IsMatch(name, @"^[a-zA-Z]+\d+$")
                && IsValid(name));
        }


        /// <summary>
        /// Since the process for setting a cell with a double or a string is identical, it has
        /// been abstracted to this method.
        /// </summary>
        /// <param name="name">The cell name</param>
        /// <param name="content">The new cell content</param>
        /// <returns>The set containing name and all its dependents</returns>
        private ISet<String> SetDoubleOrString(string name, object content)
        {
            // Remove any possible pre-existing dependencies where this cell is dependent
            Dependencies.ReplaceDependents(name, new List<String>());

            // A little abstraction with a helper - set the cell content. If the content is new,
            //   the helper will return true; if the content matches the previous content, the
            //   helper will return false, and we'll defer to the previous value of Changed
            if (SetCell(name, content))
                Changed = true;

            // Return the set with name and dependents
            HashSet<String> result = new HashSet<String>(GetCellsToRecalculate(name));

            // Reset any stored values for dependents
            foreach (string cell in result)
                Cells[cell].value = null;

            return result;
        }


        /// <summary>
        /// Helper method - handles setting the content of a cell. If the new content matches the
        /// old content, this method returns false. Otherwise this method sets the new cell content
        /// and returns true.
        /// </summary>
        /// <param name="name">The cell name</param>
        /// <param name="content">The new cell content</param>
        /// <returns>true if name's content was changed; false if the new content matches the old
        /// content</returns>
        private bool SetCell(string name, object content)
        {
            // If the old content equals the new, return false
            if (Cells.ContainsKey(name) && Cells[name].content.Equals(content))
                return false;

            // Otherwise set name with content and return true
            Cells[name] = new Cell(content);

            return true;
        }


        /// <summary>
        /// Cell class for storing contents and values of cells
        /// </summary>
        private class Cell
        {
            /// <summary>
            /// The cell content
            /// </summary>
            public object content { get; set; }

            /// <summary>
            /// The cell value
            /// </summary>
            public object value { get; set; }

            /// <summary>
            /// Constructor - sets the cell content
            /// </summary>
            public Cell(object content)
            {
                this.content = content;
            }

            /// <summary>
            /// Overridden ToString() method - good for getting properly formatted Formula strings
            /// </summary>
            public override string ToString()
            {
                return (content is Formula) ?
                    '=' + ((Formula)content).ToString() : content.ToString();
            }
        }
    }
}