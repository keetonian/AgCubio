using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FormulaEvaluator
{
    /// <summary>
    /// The Evaluator class is used for evaluating infix expressions
    /// </summary>
    public static class Evaluator
    {
        /// <summary>
        /// Stores a function that takes a String input (a variable) and returns an associated int value
        /// </summary>
        /// <param name="var">the variable to be looked up</param>
        /// <returns>the value of the variable</returns>
        public delegate int Lookup(String var);

        /// <summary>
        /// Evaluate takes in a String expression and returns the integer resultant. Throws an ArgumentException if expression is invalid.
        /// </summary>
        /// <param name="exp">the expression to be evaluated</param>
        /// <param name="variableEvaluator">a function used to look up variable values</param>
        /// <returns>the resultant value of the expression</returns>
        public static int Evaluate(String exp, Lookup variableEvaluator)
        {
            // create stacks to hold values and operators
            Stack<double> values = new Stack<double>();
            Stack<String> operators = new Stack<String>();

            // split the expression into substrings to check
            String[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

            // need a double to store potential values to
            double value;

            // need a Regex that can check correct variable format -- the format is any set of consecutive characters, followed by any
            // set of consecutive numbers, and there must be at least one character and at least one number
            Regex format = new Regex(@"^[a-zA-Z]+[0-9]+$");

            // go through each element and check what to do with it
            foreach (String preElement in substrings)
            {
                // create a no whitespaces version of the element
                String element = Regex.Replace(preElement, @"\s+", "");

                // check if we're reading an empty string (if so, skip to next element)
                if (element == "")
                {
                    continue;
                }

                // if incoming element is multiplication, division, or left parenthesis, we'll just put it on the operator stack
                else if (element == "*" || element == "/" || element == "(")
                {
                    operators.Push(element);
                }

                // check if incoming element is addition or subtraction
                else if (element == "+" || element == "-")
                {
                    // if addition or subtraction was on top of the operator stack, we'll try to do an operation
                    if (operators.hasOnTop("+","-"))
                    {
                        addOrSubtract(operators, values);
                    }
                    // in any case, push the new operator onto the operator stack
                    operators.Push(element);

                }
                // check if incoming element is a variable; if so lookup the value
                else if (format.IsMatch(element))
                {
                    value = (double) variableEvaluator(element);

                    // if multiplication or division was on top of the operator stack, we'll try to do an operation
                    if (operators.hasOnTop("*", "/"))
                    {
                        timesOrDivide(operators, values, value);
                    }
                    // if multiplication or division was not on top of the operator stack, just push the value onto the value stack
                    else
                    {
                        values.Push(value);
                    }
                }
                // check if incoming element is a value; if so store the value
                else if (double.TryParse(element, out value))
                {
                    // if multiplication or division was on top of the operator stack, we'll try to do an operation
                    if (operators.hasOnTop("*", "/"))
                    {
                        timesOrDivide(operators, values, value);
                    }
                    // if multiplication or division was not on top of the operator stack, just push the value onto the value stack
                    else
                    {
                        values.Push(value);
                    }
                }

                // check if incoming element was a right parenthesis
                else if (element == ")")
                {
                    // if addition or subtraction was on top of the operator stack, we'll try to do an operation
                    if (operators.hasOnTop("+", "-"))
                    {
                        addOrSubtract(operators, values);
                    }
                    // check for the expected left parenthesis and pop it, otherwise throw an error
                    if (operators.Count > 0 && operators.Peek() == "(")
                    {
                        operators.Pop();
                    }
                    else
                    {
                        throw new ArgumentException();
                    }
                    // if multiplication or division was on top of the operator stack, we'll try to do an operation
                    if (operators.hasOnTop("*","/"))
                    {
                        try
                        {
                            // store the top two values on the value stack
                            double valueB = (double)values.Pop();
                            double valueA = (double)values.Pop();

                            // do multiplication or division with the values, put result on stack
                            if (operators.Pop() == "*")
                            {
                                values.Push(valueA * valueB);
                            }
                            // if not multiplication, the operation was division
                            else
                            {
                                // check if there's going to be a divide by zero problem
                                if (valueB == 0)
                                {
                                    throw new ArgumentException();
                                }
                                values.Push(valueA / valueB);
                            }
                        }
                        // if there are not enough values on the value stack to complete an operation
                        catch (InvalidOperationException)
                        {
                            throw new ArgumentException();
                        }

                        // the right parenthesis doesn't actually need to be pushed, so we're done

                    }
                }

                // if none of the above, we've encountered an invalid symbol and need to throw an exception
                else
                {
                    throw new ArgumentException();
                }
            }

            // all elements have been analyzed; find out what to return, or if there was a problem
            // if there are no operators left and a single value, return the value
            if (operators.Count < 1 && values.Count == 1)
            {
                return (int) values.Pop();
            }
            // if there is one operator (+ or -) and two values, perform the operation and return the value
            else if (operators.Count == 1 && values.Count == 2)
            {
                // store the two values
                double valueB = values.Pop();
                double valueA = values.Pop();
                string op = operators.Pop();

                // do the specified operation, provided the operator is + or - (otherwise throw exception)
                if (op == "+")
                {
                    return (int) (valueA + valueB);
                }
                else if (op == "-")
                {
                    return (int) (valueA - valueB);
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            // otherwise something went wrong - throw an exception
            else
            {
                throw new ArgumentException();
            }
        }

        /// <summary>
        /// Checks that a stack is not empty, and the top is one of two specified elements
        /// </summary>
        /// <typeparam name="T">generic type, works for any stack</typeparam>
        /// <param name="stack">the stack being checked</param>
        /// <param name="checkA">the first acceptable element</param>
        /// <param name="checkB">the second accetable element</param>
        /// <returns>true if one of the specified elements is on top of the stack; false if not</returns>
        public static bool hasOnTop<T>(this Stack<T> stack, T checkA, T checkB)
        {
            if (stack.Count > 0 && (stack.Peek().Equals(checkA) || stack.Peek().Equals(checkB)))
            {
                return true;
            }
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
        public static void timesOrDivide(Stack<String> operators, Stack<double> values, double valueB)
        {
            try
            {
                // retrieve value from top of value stack
                double valueA = values.Pop();

                // do multiplication or division with the values, put result on stack
                if (operators.Pop() == "*")
                {
                    values.Push(valueA * valueB);
                }
                // if not multiplication, the operator was division
                else
                {
                    // check if there's going to be a divide by zero problem
                    if (valueB == 0)
                    {
                        throw new ArgumentException();
                    }
                    values.Push(valueA / valueB);
                }
            }
            // if value stack was empty
            catch (InvalidOperationException)
            {
                throw new ArgumentException();
            }
        }

        /// <summary>
        /// Uses the operators and values stacks, to perform either addition or subtraction (depending on which operator is on top of
        /// the operators stack). If successful, the result is placed on the value stack. If the stacks empty prematurely, an
        /// ArgumentException is thrown.
        /// </summary>
        /// <param name="operators">stack of operators</param>
        /// <param name="values">stack of values</param>
        public static void addOrSubtract (Stack<String> operators, Stack<double> values)
        {
            try
            {
                // store the top two values on the value stack
                double valueB = (double)values.Pop();
                double valueA = (double)values.Pop();

                // do addition or subtraction with the values, put result on stack
                if (operators.Pop() == "+")
                {
                    values.Push(valueA + valueB);
                }
                else
                {
                    values.Push(valueA - valueB);
                }
            }
            // if there are not enough values on the value stack to complete an operation
            catch (InvalidOperationException)
            {
                throw new ArgumentException();
            }
        }
    }
}
