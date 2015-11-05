// Written by Daniel Avery for CS 3500, September 2015.
// Version 1.2 (Simplified implementation, abstracted exceptions to static class)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  The allowed symbols are non-negative numbers written using double-precision 
    /// floating-point syntax; variables that consist of a letter or underscore followed by 
    /// zero or more letters, underscores, or digits; parentheses; and the four operator 
    /// symbols +, -, *, and /.  
    /// 
    /// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
    /// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable; 
    /// and "x 23" consists of a variable "x" and a number "23".
    /// 
    /// Associated with every formula are two delegates:  a normalizer and a validator.  The
    /// normalizer is used to convert variables into a canonical form, and the validator is used
    /// to add extra restrictions on the validity of a variable (beyond the standard requirement 
    /// that it consist of a letter or underscore followed by zero or more letters, underscores,
    /// or digits.)  Their use is described in detail in the constructor and method comments.
    /// </summary>
    public class Formula
    {
        /// <summary>
        /// List containing the ordered tokens of the formula
        /// </summary>
        private List<String> formulaTokens;


        /// <summary>
        /// List containing the ordered variables of the formula
        /// </summary>
        private List<String> variables;


        /// <summary>
        /// String representation of the formula
        /// </summary>
        private String formula;


        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically invalid,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer is the identity function, and the associated validator
        /// maps every string to true.  
        /// </summary>
        public Formula(String formula) :
            this(formula, s => s, s => true)
        {
        }


        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically incorrect,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer and validator are the second and third parameters,
        /// respectively.  
        /// 
        /// If the formula contains a variable v such that normalize(v) is not a legal variable, 
        /// throws a FormulaFormatException with an explanatory message. 
        /// 
        /// If the formula contains a variable v such that isValid(normalize(v)) is false,
        /// throws a FormulaFormatException with an explanatory message.
        /// 
        /// Suppose that N is a method that converts all the letters in a string to upper case, and
        /// that V is a method that returns true only if a string consists of one letter followed
        /// by one digit.  Then:
        /// 
        /// new Formula("x2+y3", N, V) should succeed
        /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
        /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
        /// </summary>
        public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid)
        {
            // Check if any of the arguments are null
            if (formula == null || normalize == null || isValid == null)
                throw new FormulaFormatException("Formula has null parameters.");

            // First convert the formula to individual tokens (stored privately in an IEnumerable)
            IEnumerable<string> tokens = GetTokens(formula);

            // Set up patterns for identifying tokens
            String lpPattern     = @"^\($";
            String rpPattern     = @"^\)$";
            String opPattern     = @"^[\+\-*/]$";
            String varPattern    = @"^[a-zA-Z_](?:[a-zA-Z_]|\d)*$";
            String doublePattern = @"^(?:\d+\.\d*|\d*\.\d+|\d+)(?:[eE][\+-]?\d+)?$";

            // Combine patterns (for comparing against several patterns at once)
            String numVarLpPattern = String.Format(@"({0})|({1})|({2})",
                                        lpPattern, varPattern, doublePattern);
            String numVarRpPattern = String.Format(@"({0})|({1})|({2})",
                                        rpPattern, varPattern, doublePattern);
            String opLpPattern     = String.Format(@"({0})|({1})",
                                        opPattern, lpPattern);

            // Preliminary checks ---------------------------------------------------------------

            // Check against the One Token Rule
            IEnumerator<string> enumerator = tokens.GetEnumerator();
            if (!enumerator.MoveNext())
                ThrowException.oneToken();

            // Check against the Starting Token Rule
            if (!Regex.IsMatch(enumerator.Current, numVarLpPattern))
                ThrowException.startingToken();

            // ----------------------------------------------------------------------------------

            // Use the class Lists to store valid tokens as they are found
            this.formulaTokens = new List<string>();
            this.variables = new List<string>();

            // Keep track of the number of left and right parentheses
            int lpCount = 0, rpCount = 0;

            // Iterate over each token, checking what it is and ensuring proper syntax
            foreach (string token in tokens)
            {
                // If token is a right parenthesis
                if (Regex.IsMatch(token, rpPattern))
                {
                    // Check against the Right Parenthesis Rule
                    if (++rpCount > lpCount)
                        ThrowException.rightParen();

                    // Check against the Parenthesis Following Rule
                    if (formulaTokens.Count > 0 &&
                        Regex.IsMatch(formulaTokens.ElementAt(formulaTokens.Count - 1), opLpPattern))
                        ThrowException.parenFollowing();
                }

                // If token is a left parenthesis
                else if (Regex.IsMatch(token, lpPattern))
                {
                    // Check against the Extra Following Rule
                    if (formulaTokens.Count > 0 &&
                        Regex.IsMatch(formulaTokens.ElementAt(formulaTokens.Count - 1), numVarRpPattern))
                        ThrowException.extraFollowing();

                    lpCount++;
                }

                // If token is an operator
                else if (Regex.IsMatch(token, opPattern))
                {
                    // Check against the Parenthesis Following Rule
                    if (formulaTokens.Count > 0 &&
                        Regex.IsMatch(formulaTokens.ElementAt(formulaTokens.Count - 1), opLpPattern))
                        ThrowException.parenFollowing();
                }

                // If token is a variable
                else if (Regex.IsMatch(token, varPattern))
                {
                    // Check against the Extra Following Rule
                    if (formulaTokens.Count > 0 &&
                        Regex.IsMatch(formulaTokens.ElementAt(formulaTokens.Count - 1), numVarRpPattern))
                        ThrowException.extraFollowing();

                    // Check the normalized variable against the validator
                    string newToken = normalize(token);

                    if (!isValid(newToken))
                        ThrowException.invalidVar();

                    // Add the normalized variable to the formulaTokens List
                    this.formulaTokens.Add(newToken);
                    this.formula += newToken;

                    // If not already there, add the normalized variable to the variables List
                    if (!this.variables.Contains(newToken))
                        this.variables.Add(newToken);

                    continue;
                }

                // If token is a double
                else if (Regex.IsMatch(token, doublePattern))
                {
                    // Check against the Extra Following Rule
                    if (formulaTokens.Count > 0 &&
                        Regex.IsMatch(formulaTokens.ElementAt(formulaTokens.Count - 1), numVarRpPattern))
                        ThrowException.extraFollowing();

                    // Use some of my own black magic to ensure the same value always gets the same string representation
                    // ... this took so long #ForeverAlone
                    string newToken = (Double.Parse(token, System.Globalization.NumberStyles.Float).ToString("G29"));

                    // Add the 'normalized' double to the formulaTokens List
                    this.formulaTokens.Add(newToken);
                    this.formula += newToken;

                    continue;
                }

                // If token is invalid (does not match one of the patterns), throw an exception
                else
                    ThrowException.invalidToken();

                this.formulaTokens.Add(token);
                this.formula += token;
            }

            // Final checks ---------------------------------------------------------------------

            // Check against the Balanced Parenthesis Rule
            if (lpCount != rpCount)
                ThrowException.balancedParen();

            // Check against the Ending Token Rule
            if (!Regex.IsMatch(formulaTokens.ElementAt(formulaTokens.Count - 1), numVarRpPattern))
                ThrowException.endingToken();
        }


        /// <summary>
        /// Evaluates this Formula, using the lookup delegate to determine the values of
        /// variables.  When a variable symbol v needs to be determined, it should be looked up
        /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to 
        /// the constructor.)
        /// 
        /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters 
        /// in a string to upper case:
        /// 
        /// new Formula("x+7", N, s => true).Evaluate(L) is 11
        /// new Formula("x+7").Evaluate(L) is 9
        /// 
        /// Given a variable symbol as its parameter, lookup returns the variable's value 
        /// (if it has one) or throws an ArgumentException (otherwise).
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.  
        /// The Reason property of the FormulaError should have a meaningful explanation.
        ///
        /// This method should never throw an exception.
        /// </summary>
        public object Evaluate(Func<string, double> lookup)
        {
            // Create stacks to hold values and operators
            Stack<double> values = new Stack<double>();
            Stack<String> operators = new Stack<String>();

            // Store the variable format for comparison later
            String varPattern = @"^[a-zA-Z_](?:[a-zA-Z_]|\d)*$";

            // Create a double to store potential double values to
            double value;

            try
            {
                // Go through each element and check what to do with it
                foreach (String token in formulaTokens)
                {
                    // If incoming element is multiplication, division, or left parenthesis, we'll just put it on the operator stack
                    if (token == "*" || token == "/" || token == "(")
                        operators.Push(token);

                    // Check if incoming element is addition or subtraction
                    else if (token == "+" || token == "-")
                    {
                        // If addition or subtraction was on top of the operator stack, we'll try to do an operation
                        if (hasOnTop(operators, "+", "-"))
                            addOrSubtract(operators, values);

                        // In any case, push the new operator onto the operator stack
                        operators.Push(token);
                    }

                    // Check if incoming element is a variable; if so lookup the value
                    else if (Regex.IsMatch(token, varPattern))
                    {
                        try
                        {
                            value = lookup(token);
                        }
                        catch (Exception)
                        {
                            return new FormulaError("Cell " + token + " has no numerical value.");
                        }

                        // If multiplication or division was on top of the operator stack, we'll try to do an operation
                        if (hasOnTop(operators, "*", "/"))
                            timesOrDivide(operators, values, value);

                        // If multiplication or division was not on top of the operator stack, just push the value onto the value stack
                        else
                            values.Push(value);
                    }

                    // Check if incoming element is a value; if so store the value
                    else if (double.TryParse(token, out value))
                    {
                        // If multiplication or division was on top of the operator stack, we'll try to do an operation
                        if (hasOnTop(operators, "*", "/"))
                            timesOrDivide(operators, values, value);

                        // If multiplication or division was not on top of the operator stack, just push the value onto the value stack
                        else
                            values.Push(value);
                    }

                    // Otherwise incoming element was a right parenthesis
                    else
                    {
                        // If addition or subtraction was on top of the operator stack, we'll try to do an operation
                        if (hasOnTop(operators, "+", "-"))
                            addOrSubtract(operators, values);

                        // A left parenthesis is guaranteed on the operators stack - pop it
                        operators.Pop();

                        // If multiplication or division was on top of the operator stack, we'll try to do an operation
                        if (hasOnTop(operators, "*", "/"))
                            timesOrDivide(operators, values, values.Pop());
                    }
                }

                // All elements have been analyzed; find out what to return

                // If there are no operators left and a single value, return the value
                if (operators.Count < 1 && values.Count == 1)
                    return values.Pop();

                // If there is one operator (+ or -) and two values, perform the operation and return the value
                else
                {
                    addOrSubtract(operators, values);
                    return values.Pop();
                }
            }
            catch(InvalidOperationException)
            {
                return new FormulaError("Formula contains division by zero.");
            }
        }


        /// <summary>
        /// Enumerates the normalized versions of all of the variables that occur in this 
        /// formula.  No normalization may appear more than once in the enumeration, even 
        /// if it appears more than once in this Formula.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
        /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
        /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
        /// </summary>
        public IEnumerable<String> GetVariables()
        {
            return new List<String>(this.variables); ;
        }


        /// <summary>
        /// Returns a string containing no spaces which, if passed to the Formula
        /// constructor, will produce a Formula f such that this.Equals(f).  All of the
        /// variables in the string should be normalized.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
        /// new Formula("x + Y").ToString() should return "x+Y"
        /// </summary>
        public override string ToString()
        {
            return this.formula;
        }


        /// <summary>
        /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
        /// whether or not this Formula and obj are equal.
        /// 
        /// Two Formulae are considered equal if they consist of the same tokens in the
        /// same order.  To determine token equality, all tokens are compared as strings 
        /// except for numeric tokens, which are compared as doubles, and variable tokens,
        /// whose normalized forms are compared as strings.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        ///  
        /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
        /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
        /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
        /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
        /// </summary>
        public override bool Equals(object obj)
        {
            // Immediately return false if the object being compared to is null or not a Formula
            if (obj == null || !(obj is Formula))
                return false;

            return this.formulaTokens.SequenceEqual(((Formula)obj).formulaTokens);
        }


        /// <summary>
        /// Reports whether f1 == f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return true.  If one is
        /// null and one is not, this method should return false.
        /// </summary>
        public static bool operator ==(Formula f1, Formula f2)
        {
            // If both elements are null return true
            if (((object)f1 == null) && ((object)f2 == null))
                return true;

            // If element one is null, but we've gotten here, then the second is not - return false
            if ((object)f1 == null)
                return false;

            // If we've gotten here, element one is not null, but element two may be - use element
            // one's .Equals method
            return f1.Equals(f2);
        }


        /// <summary>
        /// Reports whether f1 != f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return false.  If one is
        /// null and one is not, this method should return true.
        /// </summary>
        public static bool operator !=(Formula f1, Formula f2)
        {
            return !(f1 == f2);
        }


        /// <summary>
        /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
        /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
        /// randomly-generated unequal Formulae have the same hash code should be extremely small.
        /// </summary>
        public override int GetHashCode()
        {
            return this.formula.GetHashCode();
        }


        /// <summary>
        /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
        /// right paren; one of the four operator symbols; a string consisting of a letter or underscore
        /// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }

        }


        /// <summary>
        /// Checks that a stack is not empty, and the top is one of two specified elements
        /// </summary>
        /// <typeparam name="T">generic type, works for any stack</typeparam>
        /// <param name="stack">the stack being checked</param>
        /// <param name="checkA">the first acceptable element</param>
        /// <param name="checkB">the second acceptable element</param>
        /// <returns>true if one of the specified elements is on top of the stack; false if not</returns>
        public static bool hasOnTop<T>(Stack<T> stack, string checkA, string checkB)
        {
            if (stack.Count > 0 && (stack.Peek().Equals(checkA) || stack.Peek().Equals(checkB)))
                return true;

            return false;
        }


        /// <summary>
        /// Uses the operators and values stacks, and an input value, to perform either multiplication or division (depending on which
        /// operator is on top of the operators stack). If successful, the result is placed on the value stack. If there is a divide by
        /// zero problem, or the stacks empty prematurely, an ArgumentException is thrown.
        /// </summary>
        /// <param name="operators">stack of operators</param>
        /// <param name="values">stack of variables</param>
        /// <param name="valueB">input value - second operand (important for division)</param>
        private static void timesOrDivide(Stack<String> operators, Stack<double> values, double valueB)
        {
            // Retrieve value from top of value stack
            double valueA = values.Pop();

            // Do multiplication or division with the values, put result on stack
            if (operators.Pop() == "*")
                values.Push(valueA * valueB);
            else if (valueB == 0)
                throw new InvalidOperationException();
            else
                values.Push(valueA / valueB);
        }


        /// <summary>
        /// Uses the operators and values stacks, to perform either addition or subtraction (depending on which operator is on top of
        /// the operators stack). If successful, the result is placed on the value stack. If the stacks empty prematurely, an
        /// ArgumentException is thrown.
        /// </summary>
        /// <param name="operators">stack of operators</param>
        /// <param name="values">stack of values</param>
        private static void addOrSubtract(Stack<String> operators, Stack<double> values)
        {
            // Store the top two values on the value stack
            double valueB = values.Pop();
            double valueA = values.Pop();

            // Do addition or subtraction with the values, put result on stack
            if (operators.Pop() == "+")
                values.Push(valueA + valueB);
            else
                values.Push(valueA - valueB);
        }


        /// <summary>
        /// Defines the different kinds of FormulaFormatExceptions the constructor may encounter
        /// </summary>
        private static class ThrowException
        {
            public static void oneToken()
            {
                throw new FormulaFormatException("There are no tokens. Make sure the formula is not"
                        + " empty.");
            }
            public static void startingToken()
            {
                throw new FormulaFormatException("Invalid starting token. Make sure the formula beg"
                    + "ins with a number, variable, or opening parenthesis.");
            }
            public static void rightParen()
            {
                throw new FormulaFormatException("Unmatched closing parenthesis. Make sure the numb"
                    + "er of right parentheses never exceeds the number of left parentheses.");
            }
            public static void parenFollowing()
            {
                throw new FormulaFormatException("Invalid follower. Make sure left parentheses and "
                    + "operators are followed by either left parentheses or operands.");
            }
            public static void extraFollowing()
            {
                throw new FormulaFormatException("Invalid follower. Make sure numbers, variables, a"
                    + "nd right parentheses are followed by either right parentheses or operators.");
            }
            public static void invalidVar()
            {
                throw new FormulaFormatException("Normalized variable name invalid. Make sure varia"
                    + "bles follow validator specifications.");
            }
            public static void invalidToken()
            {
                throw new FormulaFormatException("Incorrect syntax. Make sure the formula contains "
                    + "only parentheses, operators, numbers, and variables, and that variables star"
                    + "t with at least one letter or underscore.");
            }
            public static void balancedParen()
            {
                throw new FormulaFormatException("Unbalanced parentheses. Make sure the formula has"
                    + "the same number of left and right parentheses.");
            }
            public static void endingToken()
            {
                throw new FormulaFormatException("Invalid final token. Make sure the formula ends w"
                    + "ith a number, variable, or right parenthesis.");
            }
        }
    }


    /// <summary>
    /// Used to report syntactic errors in the argument to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message)
            : base(message)
        {
        }
    }


    /// <summary>
    /// Used as a possible return value of the Formula.Evaluate method.
    /// </summary>
    public struct FormulaError
    {
        /// <summary>
        /// Constructs a FormulaError containing the explanatory reason.
        /// </summary>
        /// <param name="reason"></param>
        public FormulaError(String reason)
            : this()
        {
            Reason = reason;
        }

        /// <summary>
        ///  The reason why this FormulaError was created.
        /// </summary>
        public string Reason { get; private set; }
    }
}