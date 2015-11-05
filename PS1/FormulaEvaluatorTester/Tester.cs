using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using FormulaEvaluator;

namespace Tester
{
    /// <summary>
    /// The Tester class tests that FormulaEvaluator is evaluating expressions properly
    /// </summary>
    public static class Tester
    {
        /// <summary>
        /// A function used to provide values to variables. 
        /// Throws an ArgumentException if the variable is not mapped to a value.
        /// </summary>
        /// <param name="var">The variable to be looked up.</param>
        /// <returns>An integer value for the variable</returns>
        private static int MyLookupFunction(String var)
        {
            // An arbitrary dictionary of values
            // The var keyword is shorthand for repeating the right-hand type
            var values = new Dictionary<string, int>
            {
                 {"a7", 0}, {"b6", 45}, {"c", 67}, {"dBb1", 89}
            };

            if (!values.ContainsKey(var)) throw new ArgumentException("Variable has no value");
            return values[var];
        }

        /// <summary>
        /// Entry-point for this application.
        /// </summary>
        static void Main(string[] args)
        {
            // Passing in lookup function directly
            //Console.WriteLine(Evaluator.Evaluate("X1", MyLookupFunction));

            // Storing function in variable and passing in the variable
            //Evaluator.Lookup l2 = MyLookupFunction;
            //Console.WriteLine(Evaluator.Evaluate("X1", l2));

            // Using lambda expression
            Console.WriteLine(Evaluator.Evaluate("(((4+ &dBb1 )*a7)   / 2)+1", MyLookupFunction));
            Console.Read();
        }
    }
}
