using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using SpreadsheetUtilities;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// Tester for the Formula class
    /// </summary>
    [TestClass()]
    public class UnitTest1
    {
        /// <summary>
        /// Simple validator function - returns true if the input string consists of one letter
        /// followed by one number
        /// </summary>
        /// <param name="s">The string to be checked</param>
        /// <returns>True if input string matches format, false if it doesn't</returns>
        public static bool isValid(string s)
        {
            Regex format = new Regex(@"^[A-Za-z]\d$");
            return format.IsMatch(s);
        }

        /// <summary>
        /// Simple normalizing function - returns the uppercase version of the input string
        /// </summary>
        /// <param name="s">The base string</param>
        /// <returns>A string that is the uppercase version of the input</returns>
        public static string normalize(string s)
        {
            return s.ToUpper();
        }

        /// <summary>
        /// A function used to provide values to variables. 
        /// Throws an ArgumentException if the variable is not mapped to a value.
        /// </summary>
        /// <param name="s">The variable to be looked up.</param>
        /// <returns>An integer value for the variable</returns>
        public static double lookupFunc(string s)
        {
            // An arbitrary dictionary of values
            // The var keyword is shorthand for repeating the right-hand type
            var values = new Dictionary<string, int>
            {
                 {"a7", 0}, {"b6", 45}, {"c2", 67}, {"d5", 89}
            };

            if (!values.ContainsKey(s)) throw new ArgumentException("Variable has no value");
            return values[s];
        }


        /// <summary>
        /// Tests that the constructor succeeds for a valid normalized input
        /// </summary>
        [TestMethod()]
        public void testConstructor1()
        {
            Formula expression = new Formula("x2+y3", normalize, isValid);
            Assert.AreEqual("X2+Y3", expression.ToString());
        }

        /// <summary>
        /// Tests that the constructor throws exception for an invalid normalized input
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void testConstructor2()
        {
            Formula expression = new Formula("(x+y3)", normalize, isValid);
        }

        /// <summary>
        /// Tests that the constructor throws exception for an input with incorrect syntax
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void testConstructor3()
        {
            Formula expression = new Formula("(2x+y3)", normalize, isValid);
        }

        /// <summary>
        /// Tests that the Evaluate method correctly evaluates a 'good' expression (w/o variables)
        /// </summary>
        [TestMethod()]
        public void testEvaluate1()
        {
            Formula expression = new Formula("((4*2)+3/2-3)", normalize, isValid);

            Assert.AreEqual(6.5, expression.Evaluate(lookupFunc));
        }

        /// <summary>
        /// Tests that the Evaluate method correctly evaluates a 'good' expression (w/ variables)
        /// </summary>
        [TestMethod()]
        public void testEvaluate2()
        {
            Formula expression = new Formula("(b6*2/3+(a7/5+2))");

            Assert.AreEqual(32.0, expression.Evaluate(lookupFunc));
        }

        /// <summary>
        /// Tests that the Evaluate method returns a FormulaError if the lookup function throws
        /// an ArgumentException (a variable is not defined)
        /// </summary>
        [TestMethod()]
        public void testEvaluate3()
        {
            Formula expression = new Formula("(b6*2/3+(d6/5+2))");

            Assert.IsInstanceOfType(expression.Evaluate(lookupFunc), typeof(FormulaError));
        }

        /// <summary>
        /// Tests that the Evaluate method returns a FormulaError if it encounters a divide by zero
        /// </summary>
        [TestMethod()]
        public void testEvaluate4()
        {
            Formula expression = new Formula("(b6*2/3+(d5/0+2))");

            Assert.IsInstanceOfType(expression.Evaluate(lookupFunc), typeof(FormulaError));
        }

        /// <summary>
        /// Tests that the Evaluate method returns a FormulaError if it encounters a divide by zero
        /// (where 0 is the value of a variable)
        /// </summary>
        [TestMethod()]
        public void testEvaluate5()
        {
            Formula expression = new Formula("(b6*2/3+(d5/a7+2))");

            Assert.IsInstanceOfType(expression.Evaluate(lookupFunc), typeof(FormulaError));
        }

        /// <summary>
        /// Tests that GetVariables enumerates normalized variables in order
        /// </summary>
        [TestMethod()]
        public void testGetVariables1()
        {
            IEnumerator<string> vars = new Formula("x+y*z", normalize, s => true).GetVariables().GetEnumerator();
            vars.MoveNext();
            Assert.AreEqual("X", vars.Current);
            vars.MoveNext();
            Assert.AreEqual("Y", vars.Current);
            vars.MoveNext();
            Assert.AreEqual("Z", vars.Current);
        }

        /// <summary>
        /// Tests that GetVariables enumerates repeated variables only once
        /// </summary>
        [TestMethod()]
        public void testGetVariables2()
        {
            IEnumerator<string> vars = new Formula("x+X*z/Z", normalize, s => true).GetVariables().GetEnumerator();
            vars.MoveNext();
            Assert.AreEqual("X", vars.Current);
            vars.MoveNext();
            Assert.AreEqual("Z", vars.Current);
        }

        /// <summary>
        /// Tests that GetVariables enumerates upper and lower variables when they are not normalized
        /// </summary>
        [TestMethod()]
        public void testGetVariables3()
        {
            IEnumerator<string> vars = new Formula("x+X*z").GetVariables().GetEnumerator();
            vars.MoveNext();
            Assert.AreEqual("x", vars.Current);
            vars.MoveNext();
            Assert.AreEqual("X", vars.Current);
            vars.MoveNext();
            Assert.AreEqual("z", vars.Current);
        }

        /// <summary>
        /// Tests that ToString returns the normalized no-spaces version of the input expression
        /// </summary>
        [TestMethod()]
        public void testToString1()
        {
            Formula expression = new Formula("x + y +     Z", normalize, s => true);

            Assert.AreEqual("X+Y+Z", expression.ToString());
        }

        /// <summary>
        /// Tests that ToString does not auto-normalize variables if not specified
        /// </summary>
        [TestMethod()]
        public void testToString2()
        {
            Formula expression = new Formula("x + y +     Z");

            Assert.AreEqual("x+y+Z", expression.ToString());
        }

        /// <summary>
        /// Tests that Equals recognizes equality regardless of spacing, and that normalization works
        /// properly
        /// </summary>
        [TestMethod()]
        public void testEquals1()
        {
            Formula expressionA = new Formula("x1+y2", normalize, s => true);
            Formula expressionB = new Formula("X1  +  Y2");
            Assert.IsTrue(expressionA.Equals(expressionB));
            Assert.IsTrue(expressionB.Equals(expressionA));
        }

        /// <summary>
        /// Tests that Equals returns false if variable character cases don't match
        /// </summary>
        [TestMethod()]
        public void testEquals2()
        {
            Formula expressionA = new Formula("x1+y2");
            Formula expressionB = new Formula("X1  +  Y2");
            Assert.IsFalse(expressionA.Equals(expressionB));
            Assert.IsFalse(expressionB.Equals(expressionA));
        }

        /// <summary>
        /// Tests that Equals returns false if tokens are not in the same order
        /// </summary>
        [TestMethod()]
        public void testEquals3()
        {
            Formula expressionA = new Formula("x1+y2");
            Formula expressionB = new Formula("y2+x1");
            Assert.IsFalse(expressionA.Equals(expressionB));
            Assert.IsFalse(expressionB.Equals(expressionA));
        }

        /// <summary>
        /// Tests that Equals interprets various versions of the same number as the same
        /// </summary>
        [TestMethod()]
        public void testEquals4()
        {
            Formula expressionA = new Formula("2.0 +x7");
            Formula expressionB = new Formula("2.000+   x7");
            Formula expressionC = new Formula("2 +  \n x7");
            Formula expressionD = new Formula("2e+0 + x7");
            Assert.IsTrue(expressionA.Equals(expressionB));
            Assert.IsTrue(expressionB.Equals(expressionC));
            Assert.IsTrue(expressionA.Equals(expressionC));
            Assert.IsTrue(expressionA.Equals(expressionD));
        }

        /// <summary>
        /// Tests that == recognizes equality regardless of spacing, and that normalization works
        /// properly
        /// </summary>
        [TestMethod()]
        public void testEqOperator1()
        {
            Formula expressionA = new Formula("x1+y2", normalize, s => true);
            Formula expressionB = new Formula("X1  +  Y2");
            Assert.IsTrue(expressionA == expressionB);
            Assert.IsTrue(expressionB == expressionA);
        }

        /// <summary>
        /// Tests that == returns false if variable character cases don't match
        /// </summary>
        [TestMethod()]
        public void testEqOperator2()
        {
            Formula expressionA = new Formula("x1+y2");
            Formula expressionB = new Formula("X1  +  Y2");
            Assert.IsFalse(expressionA == expressionB);
            Assert.IsFalse(expressionB == expressionA);
        }

        /// <summary>
        /// Tests that == returns false if tokens are not in the same order
        /// </summary>
        [TestMethod()]
        public void testEqOperator3()
        {
            Formula expressionA = new Formula("x1+y2");
            Formula expressionB = new Formula("y2+x1");
            Assert.IsFalse(expressionA == expressionB);
            Assert.IsFalse(expressionB == expressionA);
        }

        /// <summary>
        /// Tests that == interprets various versions of the same number as the same
        /// </summary>
        [TestMethod()]
        public void testEqOperator4()
        {
            Formula expressionA = new Formula("2.0 +x7");
            Formula expressionB = new Formula("2.000+   x7");
            Formula expressionC = new Formula("2 +  \n x7");
            Formula expressionD = new Formula("2e0 + x7");
            Assert.IsTrue(expressionA == expressionB);
            Assert.IsTrue(expressionB == expressionC);
            Assert.IsTrue(expressionA == expressionC);
            Assert.IsTrue(expressionA == expressionD);
        }

        /// <summary>
        /// Tests that == returns true if both elements are null
        /// </summary>
        [TestMethod()]
        public void testEqOperator5()
        {
            Formula expressionA = null;
            Formula expressionB = null;
            Assert.IsTrue(expressionA == expressionB);
            Assert.IsTrue(expressionB == expressionA);
        }

        /// <summary>
        /// Tests that == returns false if one element is null and one is not, and regardless of
        /// which order they are presented in
        /// </summary>
        [TestMethod()]
        public void testEqOperator6()
        {
            Formula expressionA = null;
            Formula expressionB = new Formula("x + y");
            Assert.IsFalse(expressionA == expressionB);
            Assert.IsFalse(expressionB == expressionA);
        }

        /// <summary>
        /// Tests that != recognizes equality (and returns false) regardless of spacing, and that
        /// normalization works properly
        /// </summary>
        [TestMethod()]
        public void testNotEqOperator1()
        {
            Formula expressionA = new Formula("x1+y2", normalize, s => true);
            Formula expressionB = new Formula("X1  +  Y2");
            Assert.IsFalse(expressionA != expressionB);
            Assert.IsFalse(expressionB != expressionA);
        }

        /// <summary>
        /// Tests that != returns true if variable character cases don't match
        /// </summary>
        [TestMethod()]
        public void testNotEqOperator2()
        {
            Formula expressionA = new Formula("x1+y2");
            Formula expressionB = new Formula("X1  +  Y2");
            Assert.IsTrue(expressionA != expressionB);
            Assert.IsTrue(expressionB != expressionA);
        }

        /// <summary>
        /// Tests that != returns true if tokens are not in the same order
        /// </summary>
        [TestMethod()]
        public void testNotEqOperator3()
        {
            Formula expressionA = new Formula("x1+y2");
            Formula expressionB = new Formula("y2+x1");
            Assert.IsTrue(expressionA != expressionB);
            Assert.IsTrue(expressionB != expressionA);
        }

        /// <summary>
        /// Tests that != interprets various versions of the same number as the same (and returns
        /// false)
        /// </summary>
        [TestMethod()]
        public void testNotEqOperator4()
        {
            Formula expressionA = new Formula("2.0 +x7");
            Formula expressionB = new Formula("2.000+   x7");
            Formula expressionC = new Formula("2 +  \n x7");
            Formula expressionD = new Formula("2e0 + x7");
            Assert.IsFalse(expressionA != expressionB);
            Assert.IsFalse(expressionB != expressionC);
            Assert.IsFalse(expressionA != expressionC);
            Assert.IsFalse(expressionA != expressionD);
        }

        /// <summary>
        /// Tests that != returns false if both elements are null
        /// </summary>
        [TestMethod()]
        public void testNotEqOperator5()
        {
            Formula expressionA = null;
            Formula expressionB = null;
            Assert.IsFalse(expressionA != expressionB);
            Assert.IsFalse(expressionB != expressionA);
        }

        /// <summary>
        /// Tests that != returns true if one element is null and one is not, and regardless of
        /// which order they are presented in
        /// </summary>
        [TestMethod()]
        public void testNotEqOperator6()
        {
            Formula expressionA = null;
            Formula expressionB = new Formula("x + y");
            Assert.IsTrue(expressionA != expressionB);
            Assert.IsTrue(expressionB != expressionA);
        }

        /// <summary>
        /// Tests that equivalent formulas produce the same hash code
        /// </summary>
        [TestMethod()]
        public void testGetHashCode1()
        {
            Formula expressionA = new Formula("x1+y2", normalize, s => true);
            Formula expressionB = new Formula("X1  +  Y2");
            Assert.IsTrue(expressionA.GetHashCode() == expressionB.GetHashCode());

            int hashC = new Formula("2.0 +x7").GetHashCode();
            int hashD = new Formula("2.000+   x7").GetHashCode();
            int hashE = new Formula("2 +  \n x7").GetHashCode();
            int hashF = new Formula("2e0 + x7").GetHashCode();
            Assert.IsTrue(hashC == hashD);
            Assert.IsTrue(hashD == hashE);
            Assert.IsTrue(hashE == hashF);
        }

        /// <summary>
        /// Tests that basic non-equivalent formulas do not produce the same hash code
        /// </summary>
        [TestMethod()]
        public void testGetHashCode2()
        {
            Formula expressionA = new Formula("x1+y2");
            Formula expressionB = new Formula("y2+x1");
            Formula expressionC = new Formula("x1+y2+2");
            Assert.IsFalse(expressionA.GetHashCode() == expressionB.GetHashCode());
            Assert.IsFalse(expressionA.GetHashCode() == expressionC.GetHashCode());
            Assert.IsFalse(expressionB.GetHashCode() == expressionC.GetHashCode());
        }

        /// <summary>
        /// Tests that the Parenthesis Following Rule is obeyed
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void testParenFollowingException()
        {
            Formula expression = new Formula("(x +y3 +)");
        }


        // -------- GRADING TESTS -----------------------------------------------------------


        /**
         * Did not use Test01 through Test16 for grading.

        [TestMethod()]
        public void Test01()
        {
            Formula f = new Formula("5");
            Assert.AreEqual(5.0, (double)f.Evaluate(s => 0), 1e-9);
        }

        [TestMethod()]
        public void Test02()
        {
            Formula f = new Formula("X5");
            Assert.AreEqual(13.0, (double)f.Evaluate(s => 13), 1e-9);
        }

        [TestMethod()]
        public void Test03()
        {
            Formula f = new Formula("5.2 + 3.7");
            Assert.AreEqual(8.9, (double)f.Evaluate(s => 0), 1e-9);
        }

        [TestMethod()]
        public void Test04()
        {
            Formula f = new Formula("18.1-10.1");
            Assert.AreEqual(8, (double)f.Evaluate(s => 0), 1e-9);
        }

        [TestMethod()]
        public void Test05()
        {
            Formula f = new Formula("2*4.1");
            Assert.AreEqual(8.2, (double)f.Evaluate(s => 0), 1e-9);
        }

        [TestMethod()]
        public void Test06()
        {
            Formula f = new Formula("16/2");
            Assert.AreEqual(8, (double)f.Evaluate(s => 0), 1e-9);
        }

        [TestMethod()]
        public void Test07()
        {
            Formula f = new Formula("2.1+X1");
            Assert.AreEqual(6.2, (double)f.Evaluate(s => 4.1), 1e-9);
        }

        [TestMethod()]
        public void Test08()
        {
            Formula f = new Formula("2*6.0+3.1");
            Assert.AreEqual(15.1, (double)f.Evaluate(s => 0), 1e-9);
        }

        [TestMethod()]
        public void Test09()
        {
            Formula f = new Formula("2+6.0*3.1");
            Assert.AreEqual(20.6, (double)f.Evaluate(s => 0), 1e-9);
        }

        [TestMethod()]
        public void Test10()
        {
            Formula f = new Formula("(2+6.0)*3.1");
            Assert.AreEqual(24.8, (double)f.Evaluate(s => 0), 1e-9);
        }

        [TestMethod()]
        public void Test11()
        {
            Formula f = new Formula("2*(3+5)");
            Assert.AreEqual(16, (double)f.Evaluate(s => 0), 1e-9);
        }

        [TestMethod()]
        public void Test12()
        {
            Formula f = new Formula("2+(3+5)");
            Assert.AreEqual(10, (double)f.Evaluate(s => 0), 1e-9);
        }

        [TestMethod()]
        public void Test13()
        {
            Formula f = new Formula("2+(3+5*9)");
            Assert.AreEqual(50, (double)f.Evaluate(s => 0), 1e-9);
        }

        [TestMethod()]
        public void Test14()
        {
            Formula f = new Formula("2+3*(3+5)");
            Assert.AreEqual(26, (double)f.Evaluate(s => 0), 1e-9);
        }

        [TestMethod()]
        public void Test15()
        {
            Formula f = new Formula("2+3*5+(3+4*8)*5+2");
            Assert.AreEqual(194, (double)f.Evaluate(s => 0), 1e-9);
        }
         
        */

        // Simple tests that return FormulaErrors
        [TestMethod()]
        public void Test16()
        {
            Formula f = new Formula("2+X1");
            Assert.IsInstanceOfType(f.Evaluate(s => { throw new ArgumentException("Unknown variable"); }), typeof(FormulaError));
        }

        [TestMethod()]
        public void Test17()
        {
            Formula f = new Formula("5/0");
            Assert.IsInstanceOfType(f.Evaluate(s => 0), typeof(FormulaError));
        }

        [TestMethod()]
        public void Test18()
        {
            Formula f = new Formula("(5 + X1) / (X1 - 3)");
            Assert.IsInstanceOfType(f.Evaluate(s => 3), typeof(FormulaError));
        }


        // Tests of syntax errors detected by the constructor
        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void Test19()
        {
            Formula f = new Formula("+");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void Test20()
        {
            Formula f = new Formula("2+5+");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void Test21()
        {
            Formula f = new Formula("2+5*7)");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void Test22()
        {
            Formula f = new Formula("((3+5*7)");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void Test23()
        {
            Formula f = new Formula("5x");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void Test24()
        {
            Formula f = new Formula("5+5x");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void Test25()
        {
            Formula f = new Formula("5+7+(5)8");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void Test26()
        {
            Formula f = new Formula("5 5");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void Test27()
        {
            Formula f = new Formula("5 + + 3");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void Test28()
        {
            Formula f = new Formula("");
        }

        // Some more complicated formula evaluations
        [TestMethod()]
        public void Test29()
        {
            Formula f = new Formula("y1*3-8/2+4*(8-9*2)/14*x7");
            Assert.AreEqual(5.14285714285714, (double)f.Evaluate(s => (s == "x7") ? 1 : 4), 1e-9);
        }

        [TestMethod()]
        public void Test30()
        {
            Formula f = new Formula("x1+(x2+(x3+(x4+(x5+x6))))");
            Assert.AreEqual(6, (double)f.Evaluate(s => 1), 1e-9);
        }

        [TestMethod()]
        public void Test31()
        {
            Formula f = new Formula("((((x1+x2)+x3)+x4)+x5)+x6");
            Assert.AreEqual(12, (double)f.Evaluate(s => 2), 1e-9);
        }

        [TestMethod()]
        public void Test32()
        {
            Formula f = new Formula("a4-a4*a4/a4");
            Assert.AreEqual(0, (double)f.Evaluate(s => 3), 1e-9);
        }

        // Test of the Equals method
        [TestMethod()]
        public void Test33()
        {
            Formula f1 = new Formula("X1+X2");
            Formula f2 = new Formula("X1+X2");
            Assert.IsTrue(f1.Equals(f2));
        }

        [TestMethod()]
        public void Test34()
        {
            Formula f1 = new Formula("X1+X2");
            Formula f2 = new Formula(" X1  +  X2   ");
            Assert.IsTrue(f1.Equals(f2));
        }

        [TestMethod()]
        public void Test35()
        {
            Formula f1 = new Formula("2+X1*3.00");
            Formula f2 = new Formula("2.00+X1*3.0");
            Assert.IsTrue(f1.Equals(f2));
        }

        [TestMethod()]
        public void Test36()
        {
            Formula f1 = new Formula("1e-2 + X5 + 17.00 * 19 ");
            Formula f2 = new Formula("   0.0100  +     X5+ 17 * 19.00000 ");
            Assert.IsTrue(f1.Equals(f2));
        }

        [TestMethod()]
        public void Test37()
        {
            Formula f = new Formula("2");
            Assert.IsFalse(f.Equals(null));
            Assert.IsFalse(f.Equals(""));
        }


        // Tests of == operator
        [TestMethod()]
        public void Test38()
        {
            Formula f1 = new Formula("2");
            Formula f2 = new Formula("2");
            Assert.IsTrue(f1 == f2);
        }

        [TestMethod()]
        public void Test39()
        {
            Formula f1 = new Formula("2");
            Formula f2 = new Formula("5");
            Assert.IsFalse(f1 == f2);
        }

        [TestMethod()]
        public void Test40()
        {
            Formula f1 = new Formula("2");
            Formula f2 = new Formula("2");
            Assert.IsFalse(null == f1);
            Assert.IsFalse(f1 == null);
            Assert.IsTrue(f1 == f2);
        }


        // Tests of != operator
        [TestMethod()]
        public void Test41()
        {
            Formula f1 = new Formula("2");
            Formula f2 = new Formula("2");
            Assert.IsFalse(f1 != f2);
        }

        [TestMethod()]
        public void Test42()
        {
            Formula f1 = new Formula("2");
            Formula f2 = new Formula("5");
            Assert.IsTrue(f1 != f2);
        }


        // Test of ToString method
        [TestMethod()]
        public void Test43()
        {
            Formula f = new Formula("2*5");
            Assert.IsTrue(f.Equals(new Formula(f.ToString())));
        }


        // Tests of GetHashCode method
        [TestMethod()]
        public void Test44()
        {
            Formula f1 = new Formula("2*5");
            Formula f2 = new Formula("2*5");
            Assert.IsTrue(f1.GetHashCode() == f2.GetHashCode());
        }

        [TestMethod()]
        public void Test45()
        {
            Formula f1 = new Formula("2*5");
            Formula f2 = new Formula("3/8*2+(7)");
            Assert.IsTrue(f1.GetHashCode() != f2.GetHashCode());
        }


        // Tests of GetVariables method
        [TestMethod()]
        public void Test46()
        {
            Formula f = new Formula("2*5");
            Assert.IsFalse(f.GetVariables().GetEnumerator().MoveNext());
        }

        [TestMethod()]
        public void Test47()
        {
            Formula f = new Formula("2*X2");
            List<string> actual = new List<string>(f.GetVariables());
            HashSet<string> expected = new HashSet<string>() { "X2" };
            Assert.AreEqual(actual.Count, 1);
            Assert.IsTrue(expected.SetEquals(actual));
        }

        [TestMethod()]
        public void Test48()
        {
            Formula f = new Formula("2*X2+Y3");
            List<string> actual = new List<string>(f.GetVariables());
            HashSet<string> expected = new HashSet<string>() { "Y3", "X2" };
            Assert.AreEqual(actual.Count, 2);
            Assert.IsTrue(expected.SetEquals(actual));
        }

        [TestMethod()]
        public void Test49()
        {
            Formula f = new Formula("2*X2+X2");
            List<string> actual = new List<string>(f.GetVariables());
            HashSet<string> expected = new HashSet<string>() { "X2" };
            Assert.AreEqual(actual.Count, 1);
            Assert.IsTrue(expected.SetEquals(actual));
        }

        [TestMethod()]
        public void Test50()
        {
            Formula f = new Formula("X1+Y2*X3*Y2+Z7+X1/Z8");
            List<string> actual = new List<string>(f.GetVariables());
            HashSet<string> expected = new HashSet<string>() { "X1", "Y2", "X3", "Z7", "Z8" };
            Assert.AreEqual(actual.Count, 5);
            Assert.IsTrue(expected.SetEquals(actual));
        }

        // Tests to make sure there can be more than one formula at a time
        [TestMethod()]
        public void Test51a()
        {
            Formula f1 = new Formula("2");
            Formula f2 = new Formula("3");
            Assert.IsTrue(f1.ToString().IndexOf("2") >= 0);
            Assert.IsTrue(f2.ToString().IndexOf("3") >= 0);
        }

        [TestMethod()]
        public void Test51b()
        {
            Formula f1 = new Formula("2");
            Formula f2 = new Formula("3");
            Assert.IsTrue(f1.ToString().IndexOf("2") >= 0);
            Assert.IsTrue(f2.ToString().IndexOf("3") >= 0);
        }

        [TestMethod()]
        public void Test51c()
        {
            Formula f1 = new Formula("2");
            Formula f2 = new Formula("3");
            Assert.IsTrue(f1.ToString().IndexOf("2") >= 0);
            Assert.IsTrue(f2.ToString().IndexOf("3") >= 0);
        }

        [TestMethod()]
        public void Test51d()
        {
            Formula f1 = new Formula("2");
            Formula f2 = new Formula("3");
            Assert.IsTrue(f1.ToString().IndexOf("2") >= 0);
            Assert.IsTrue(f2.ToString().IndexOf("3") >= 0);
        }

        [TestMethod()]
        public void Test51e()
        {
            Formula f1 = new Formula("2");
            Formula f2 = new Formula("3");
            Assert.IsTrue(f1.ToString().IndexOf("2") >= 0);
            Assert.IsTrue(f2.ToString().IndexOf("3") >= 0);
        }

        // Stress test for constructor
        [TestMethod()]
        public void Test52a()
        {
            Formula f = new Formula("(((((2+3*X1)/(7e-5+X2-X4))*X5+.0005e+92)-8.2)*3.14159) * ((x2+3.1)-.00000000008)");
        }

        // Stress test for constructor
        [TestMethod()]
        public void Test52b()
        {
            Formula f = new Formula("(((((2+3*X1)/(7e-5+X2-X4))*X5+.0005e+92)-8.2)*3.14159) * ((x2+3.1)-.00000000008)");
        }

        // Stress test for constructor
        [TestMethod()]
        public void Test52c()
        {
            Formula f = new Formula("(((((2+3*X1)/(7e-5+X2-X4))*X5+.0005e+92)-8.2)*3.14159) * ((x2+3.1)-.00000000008)");
        }

        // Stress test for constructor
        [TestMethod()]
        public void Test52d()
        {
            Formula f = new Formula("(((((2+3*X1)/(7e-5+X2-X4))*X5+.0005e+92)-8.2)*3.14159) * ((x2+3.1)-.00000000008)");
        }

        // Stress test for constructor
        [TestMethod()]
        public void Test52e()
        {
            Formula f = new Formula("(((((2+3*X1)/(7e-5+X2-X4))*X5+.0005e+92)-8.2)*3.14159) * ((x2+3.1)-.00000000008)");
        }


        // KEETON'S TESTS -----------------------------------------------------------------------


        //################################ BLACK BOX TESTING ################################

        //These tests should be fine to use on any implementation.

        //These are not true black box tests, as many of them I made after I started my implementations.

        //However, they may work for such a purpose.

        /// <summary>

        /// Tests the constructor. Creates an instance, then makes sure that it is equal with itself.

        /// Very basic test.

        /// </summary>

        [TestMethod]

        public void TestConstructor()
        {

            Formula f = new Formula("5e-3");

            Assert.IsTrue(f.Equals(f));

            System.Diagnostics.Debug.WriteLine(f.Evaluate(s => 5));

        }

        /// <summary>

        /// Tests adding one operator into the constructor.

        /// Tests "" (blank string)

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void TestNullConstructor()
        {

            Formula f = new Formula("");

        }

        /// <summary>

        /// Tests adding one operator into the constructor.

        /// Tests "" (blank string)

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void TestNullConstructor1()
        {

            Formula f = new Formula(null);

        }

        /// <summary>

        /// Tests adding one operator into the constructor.

        /// Tests "" (blank string)

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void TestNullConstructor2()
        {

            Formula f = new Formula("null", null, s => true);

        }

        /// <summary>

        /// Tests adding one operator into the constructor.

        /// Tests "" (blank string)

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void TestNullConstructor3()
        {

            Formula f = new Formula("null", s => s, null);

        }

        /// <summary>

        /// Tests adding one operator into the constructor.

        /// Tests +

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void TestConstructor2()
        {

            Formula f = new Formula("+");

        }

        /// <summary>

        /// Tests adding one operator into the constructor.

        /// Tests -

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void TestConstructor3()
        {

            Formula g = new Formula("-");

        }

        /// <summary>

        /// Tests adding one operator into the constructor.

        /// Tests (

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void TestConstructor4()
        {

            Formula h = new Formula("(");

        }

        /// <summary>

        /// Tests adding one operator into the constructor.

        /// Tests )

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void TestConstructor5()
        {

            Formula i = new Formula(")");

        }

        /// <summary>

        /// Tests adding one operator into the constructor.

        /// Tests *

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void TestConstructor6()
        {

            Formula j = new Formula("*");

        }

        /// <summary>

        /// Tests adding one operator into the constructor.

        /// Tests /

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void TestConstructor7()
        {

            Formula k = new Formula("/");

        }

        /// <summary>

        /// Tests adding one operator into the constructor.

        /// Tests " "

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void TestConstructor8()
        {

            Formula l = new Formula(" ");

        }

        /// <summary>

        /// Tests having invalid syntax at the end of the formula.

        /// Tests + at end.

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void iTestConstructor1()
        {

            Formula f = new Formula("1*4+");

        }

        /// <summary>

        /// Tests having invalid syntax at the end of the formula.

        /// "-" at end of an otherwise valid formula

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void iTestConstructor2()
        {

            Formula f = new Formula("a*5/3-");

        }

        /// <summary>

        /// Tests having invalid syntax at the end of the formula.

        /// Tests * at end.

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void iTestConstructor3()
        {

            Formula f = new Formula("aa3 + 34*");

        }

        /// <summary>

        /// Tests having invalid syntax at the end of the formula.

        /// Tests / at end.

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void iTestConstructor4()
        {

            Formula f = new Formula("a*8+(5-3)/");

        }

        /// <summary>

        /// Tests having invalid syntax at the end of the formula.

        /// Tests ( at end.

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void iTestConstructor5()
        {

            Formula f = new Formula("32.3 + (.0000024 + (3.2*0.5) /2+ 4)+(");

        }

        /// <summary>

        /// Tests for invalid syntax inside of the formula, not at the ends.

        /// Tests empty parentheses.

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void jTestConstructor()
        {

            Formula f = new Formula("a*(6+)");

        }

        /// <summary>

        /// Tests for invalid syntax inside of the formula, not at the ends.

        /// Tests empty parentheses.

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void jTestConstructor1()
        {

            Formula f = new Formula("a*()");

        }

        /// <summary>

        /// Tests for invalid syntax inside of the formula, not at the ends.

        /// Tests ) followed by a variable.

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void jTestConstructor2()
        {

            Formula f = new Formula("a*(6)a5");

        }

        /// <summary>

        /// Tests for invalid syntax inside of the formula, not at the ends.

        /// Tests ) followed by (

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void jTestConstructor3()
        {

            Formula f = new Formula("a*(6)(3)");

        }

        /// <summary>

        /// Tests for invalid syntax inside of the formula, not at the ends.

        /// Tests ( followed by +

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void jTestConstructor4()
        {

            Formula f = new Formula("a*(+6)*(3)");

        }

        /// <summary>

        /// Tests for invalid syntax inside of the formula, not at the ends.

        /// Tests ( followed by -

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void jTestConstructor5()
        {

            Formula f = new Formula("a*(-6)*(3)");

        }

        /// <summary>

        /// Tests for invalid syntax inside of the formula, not at the ends.

        /// Tests ( followed by *

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void jTestConstructor6()
        {

            Formula f = new Formula("a*(*6)*(3)");

        }

        /// <summary>

        /// Tests for invalid syntax inside of the formula, not at the ends.

        /// Tests ( followed by /

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void jTestConstructor7()
        {

            Formula f = new Formula("a*(/6)*(3)");

        }

        /// <summary>

        /// Tests for invalid syntax inside of the formula, not at the ends.

        /// Tests a number followed by (

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void jTestConstructor8()
        {

            Formula f = new Formula("1(3+4)");

        }

        /// <summary>

        /// Tests for invalid syntax inside of the formula, not at the ends.

        /// Tests two + operands in a row.

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void jTestConstructor9()
        {

            Formula f = new Formula("_/5++3");

        }

        /// <summary>

        /// Tests for invalid syntax inside of the formula, not at the ends.

        /// Tests two operands in a row.

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void jTestConstructor10()
        {

            Formula f = new Formula("_/5*/3");

        }

        /// <summary>

        /// Tests for invalid syntax inside of the formula, not at the ends.

        /// Tests two operands in a row.

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void jTestConstructor11()
        {

            Formula f = new Formula("_/5+-3");

        }

        /// <summary>

        /// Tests for invalid syntax inside of the formula, not at the ends.

        /// Tests for ) followed by a number.

        /// Similar to jTestConstructor2()

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void jTestConstructor12()
        {

            Formula f = new Formula("123 * abc *(3)4");

        }

        /// <summary>

        /// Tests for invalid syntax inside of the formula, not at the ends.

        /// Tests for two variables in a row, separated by a space.

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void jTestConstructor13()
        {

            Formula f = new Formula("1+ a a");

        }

        /// <summary>

        /// Tests for invalid syntax inside of the formula, not at the ends.

        /// Tests a formula with numbers and no operands.

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void jTestConstructor14()
        {

            Formula f = new Formula("1 5 9");

        }

        /// <summary>

        /// Tests for invalid syntax inside of the formula, not at the ends.

        /// Tests a formula with variables and no operands.

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void jTestConstructor15()
        {

            Formula f = new Formula("_ a r4 D3");

        }



        /// <summary>

        /// Tests unbalanced parentheses.

        /// Too many opening ( parentheses.

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void hTestConstructor()
        {

            Formula f = new Formula("(1+(3*4)/_+2*_____");

        }

        /// <summary>

        /// Tests unbalanced parentheses.

        /// Too many opening ( parentheses.

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void hTestConstructor1()
        {

            Formula f = new Formula("1+(((((3*4)/_)+2)*_____");

        }

        /// <summary>

        /// Tests unbalanced parentheses.

        /// Too many closing ) parentheses.

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void hTestConstructor2()
        {

            Formula f = new Formula("1+(((((3*4)/_)+2))))))*_____");

        }



        /// <summary>

        /// Tests variables.

        /// Variable cannot start with a number.

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void VarTestConstructor()
        {

            Formula f = new Formula("3asdfjdks3");

        }

        /// <summary>

        /// Tests variables.

        /// Variable cannot contain invalid characters.

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void VarTestConstructor1()
        {

            Formula f = new Formula("3&asdfjdks3");

        }

        /// <summary>

        /// Tests variables.

        /// Normalizer needs to create valid variables.

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void VarTestConstructor2()
        {

            Formula f = new Formula("asdfjdks3", s => "3a", d => true);

        }

        /// <summary>

        /// Tests variables.

        /// Variables must pass the validator test.

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void VarTestConstructor3()
        {

            Formula f = new Formula("asdfjdks3", s => "abc", d => false);

        }

        /// <summary>

        /// Tests variables.

        /// Invalid variables can be normalized to valid variables, but then still need to be validated.

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void VarTestConstructor4()
        {

            Formula f = new Formula("3asdfjdks3", s => "abc", d => d.Contains("ac"));

        }

        /// <summary>

        /// Tests variables.

        /// Valid syntax test.

        /// </summary>

        [TestMethod]

        public void VarTestConstructor6()
        {

            Formula f = new Formula("_3asdfjdks3");

        }

        /// <summary>

        /// Tests variables.

        /// Invalid variable test, contains symbols.

        /// </summary>

        [TestMethod]

        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]

        public void VarTestConstructor7()
        {

            Formula f = new Formula("asd&&fjdks3", s => "abc", d => true);

        }

        /// <summary>

        /// Tests variables.

        /// Valid variable test. This shouldn't throw an error.

        /// </summary>

        [TestMethod]

        public void VarTestConstructor8()
        {

            Formula f = new Formula("____________________________");

        }



        /// <summary>

        /// Tests for some basic equality with small functions.

        /// </summary>

        [TestMethod]

        public void TestEquals()
        {

            Formula f, g, h, i;

            f = new Formula("a1+1");

            g = f;

            h = new Formula("a1+1");

            i = new Formula("a2+1");

            Assert.IsTrue(f.Equals(g));

            Assert.IsTrue(f.Equals(h));

            Assert.IsTrue(h.Equals(g));

            Assert.IsFalse(f.Equals(i));

            Assert.IsFalse(g.Equals(i));

            Assert.IsFalse(h.Equals(i));

        }

        /// <summary>

        /// Equality testing with some basic normalizing functions thrown in (upper case and lower case norms).

        /// </summary>

        [TestMethod]

        public void TestEquals2()
        {

            Func<string, string> norm = s => s.ToUpper();

            Func<string, string> norm2 = (string s) => s.ToLower();

            Assert.IsTrue(new Formula("x1+y2", norm, s => true).Equals(new Formula("X1 + Y2")));

            Assert.IsFalse(new Formula("x1+y2").Equals(new Formula("X1+Y2")));

            Assert.IsTrue(new Formula("x1+y2").Equals(new Formula("X1+Y2", norm2, s => true)));

            Assert.IsFalse(new Formula("x1+y2").Equals(new Formula("y2+x1")));

            Assert.IsTrue(new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")));

        }



        /// <summary>

        /// Tests Equals()

        /// Tests to make sure that the == is properly working.

        /// </summary>

        [TestMethod]

        public void TestEquals4()
        {

            Formula f, g, h, i;

            f = new Formula("a1+1");

            g = f;

            h = new Formula("a1+1");

            i = new Formula("a2+1");

            Assert.IsTrue(f == g);

            Assert.IsTrue(f == h);

            Assert.IsTrue(h == g);

            Assert.IsFalse(f == i);

            Assert.IsFalse(g == i);

            Assert.IsFalse(h == i);

        }

        /// <summary>

        /// Makes sure that the equals method can equate doubles.

        /// </summary>

        [TestMethod]

        public void TestEquals5()
        {

            Formula e, f, g, h, i, j;

            e = new Formula("1.000000000");

            f = new Formula("1.0000");

            g = new Formula("1.0001");

            j = new Formula("1.00010000");

            h = new Formula("20.01");

            i = new Formula("20.010");

            Assert.IsTrue(e.Equals(f));

            Assert.IsTrue(g.Equals(j));

            Assert.IsFalse(f.Equals(g));

            Assert.IsTrue(h.Equals(i));

            Assert.IsFalse(h.Equals(g));

        }

        /// <summary>

        /// Tests Equals()

        /// Test that the != override is properly working.

        /// </summary>

        [TestMethod]

        public void TestEquals6()
        {

            Formula f, g, h, i, j;

            f = new Formula("a1+1");

            g = f;

            h = new Formula("a1+ 2");

            i = new Formula(" a2 + 3 ");

            j = new Formula("a2+3");



            Assert.IsFalse(f != g);

            Assert.IsTrue(f != h);

            Assert.IsTrue(h != g);

            Assert.IsTrue(f != i);

            Assert.IsTrue(g != i);

            Assert.IsTrue(h != i);

            Assert.IsFalse(i != j);

        }

        /// <summary>

        /// Testing Equals on null pointers.

        /// </summary>

        [TestMethod]

        public void TestEquals7()
        {

            Formula a = new Formula("hi * how * are * you");

            Object c = null;

            Formula d = null;

            Formula e = null;

            Assert.IsFalse(a.Equals(c));

            Assert.IsTrue(d == e);

            Assert.IsFalse(a == d);

            Assert.IsFalse(a.Equals(e));

            Assert.IsFalse(d != e);

            Assert.IsFalse((Formula)c != e);

            Assert.IsTrue(d != a);

        }

        /// <summary>

        /// Checking parent class object as a formula, should be the same as a formula with the same string input.

        /// </summary>

        [TestMethod]

        public void TestEquals8()
        {

            Object a = new Formula("r3+45");

            string c = "r3 + 45";

            Formula b = new Formula("r3+45");

            Assert.IsTrue(a.Equals(b));

            Assert.IsFalse(a.Equals(c));

        }



        /// <summary>

        /// Basic test, adds only one of each type of variable, makes sure that they are all contained in the formula

        /// and that they are returned when GetVariables() is called.

        /// </summary>

        [TestMethod]

        public void TestGetVariables()
        {

            Formula f = new Formula("a + b+ c + de +keeton +_Jim_Is_Great + Jim + r3 +k4 + A + B - D / _I_____Like_TO___CODE");

            HashSet<string> h = new HashSet<string>();

            foreach (string s in f.GetVariables())
            {

                Assert.IsFalse(h.Contains(s));

                h.Add(s);

            }

            Assert.IsTrue(h.Count == 13);

        }

        /// <summary>

        /// No variables.

        /// Nothing should be returned.

        /// </summary>

        [TestMethod]

        public void TestGetVariables1()
        {

            Formula f = new Formula("1 + 3 / 5 * 6 - (8)");

            HashSet<string> h = new HashSet<string>();

            foreach (string s in f.GetVariables())
            {

                Assert.IsFalse(h.Contains(s));

                h.Add(s);

            }

            Assert.IsTrue(h.Count == 0);

        }

        /// <summary>

        /// Adds several of the same variable into the constructor.

        /// Tests to make sure that only one of each variable is returned

        /// in the GetVariables() function.

        /// </summary>

        [TestMethod]

        public void TestGetVariables2()
        {

            string s = "hi";

            for (int j = 0; j < 3; j++)

                for (int i = 65; i < 80; i++)

                    s += "+" + (char)i;

            Formula f = new Formula(s);

            HashSet<string> h = new HashSet<string>();

            foreach (string k in f.GetVariables())
            {

                Assert.IsFalse(h.Contains(k));

                h.Add(k);

            }

            Assert.IsTrue(h.Count == (1 + 80 - 65));

            string pattern = @"\+";

            string[] vars = Regex.Split(s, pattern);

            foreach (string k in vars)

                Assert.IsTrue(h.Contains(k));

        }



        /// <summary>

        /// For testing the string output.

        /// </summary>

        [TestMethod]

        public void TestToString()
        {

            Formula c;

            c = new Formula("1");

            Assert.IsTrue(c.ToString() == "1");

        }

        /// <summary>

        /// Tests to make sure that ToString() returns a string that:

        /// Contains no spaces,

        /// Has properly normalized variables.

        /// </summary>

        [TestMethod]

        public void TestToString1()
        {

            Func<string, string> norm;

            norm = (string s) => s.ToUpper();

            Func<string, bool> valid = (string t) => true;

            Formula a, b, c, d;

            a = new Formula("A1 + A2 + a3");

            b = new Formula("A1 + A2 + a3");

            c = new Formula("A1 + A2 + a3", norm, valid);

            d = new Formula("a1+a2+a3", norm, valid);

            Assert.AreEqual(a.ToString(), "A1+A2+a3");

            Assert.AreEqual(a.ToString(), b.ToString());

            Assert.AreNotEqual(a.ToString(), c.ToString());

            Assert.AreEqual(c.ToString(), "A1+A2+A3");

            Assert.AreEqual(c.ToString(), d.ToString());

            d = new Formula("5 +(5+3+5) * asdfalsdkfjalskdfjasldkfjaslkfj240329 *3e20+4 + askdfjskSLDKFJSLDFKJLSDKF5473 *583743487 -(30 *59/32/43/ 43/4+3-7/3*5/3+900093/43/23 /12/32/34/54/asdkflaskdfjalskd89898989)");

            Assert.AreEqual(d, new Formula(d.ToString()));

        }

        /// <summary>

        /// White space in strings.

        /// Hashcodes return the same value for the same formula.

        /// Small changes in formulae.

        /// </summary>

        [TestMethod]

        public void TestStringAndHashcode()
        {

            Formula a = new Formula("(this+is)*(GOING-to+BE)/cool1");

            Formula b = new Formula(" ( this + is ) * \n \r ( GOING - to + BE) / cool1 ");//whitespaces are the only difference

            Assert.AreEqual(b.ToString(), a.ToString());

            Assert.IsTrue(b.GetHashCode() == a.GetHashCode());

            Formula c = new Formula(" ( this + is ) * \n ( GOING - to + bE) / cool1 ");

            Assert.IsTrue(b.GetHashCode() != c.GetHashCode());

            Assert.IsTrue(b.ToString() != c.ToString());

            Formula d = new Formula(" ( this + si ) * \n ( GOING - to + bE) / cool1 ");// is => si

            Assert.IsTrue(c.ToString() != d.ToString());

            Assert.IsFalse(c == d);

            Assert.IsFalse(c.GetHashCode() == d.GetHashCode());

        }



        /// <summary>

        /// Basic test of Evaluate. Using basic arithmetic.

        /// </summary>

        [TestMethod]

        public void TestEvaluate()
        {

            Formula f = new Formula("4+4.1");

            Func<string, double> d;

            d = (string s) => s.Length;

            Assert.IsTrue(f.Evaluate(d).Equals(8.1));

            Formula g = new Formula("4/4");

            Assert.IsTrue(g.Evaluate(d).Equals((double)1));

            Formula h = new Formula(".0234385 - .4");

            Assert.AreEqual(h.Evaluate(d), (.0234385 - .4));

            Formula i = new Formula("4*4+e");

            Assert.AreEqual(i.Evaluate(d), (double)17);

        }

        /// <summary>

        /// Tests the Evaluate Function.

        /// Tests a division by 0, the code is supposed to return

        /// a FormulaError with a reason.

        /// </summary>

        [TestMethod]

        public void TestEvaluate1()
        {

            Assert.AreEqual(new Formula("2").Evaluate(s => 2), (double)2);

            Func<string, double> d;

            d = (string s) => s.Length;

            Formula h = new Formula("4/4/0");

            Assert.IsTrue(h.Evaluate(d).GetType().ToString() == "SpreadsheetUtilities.FormulaError");

            FormulaError o = (FormulaError)h.Evaluate(d);

            Assert.IsTrue(o.Reason != null);

            Assert.IsTrue(o.Reason != "");

            Assert.IsTrue(o.Reason != " ");

        }

        /// <summary>

        /// Tests basic variable ability.

        /// </summary>

        [TestMethod]

        public void TestEvaluate2()
        {

            Func<string, double> d;

            d = (string s) => s.Length;

            Formula i = new Formula("4*a");

            Assert.AreEqual(i.Evaluate(d), (double)4);

            Formula k = new Formula("a + b + c / d +(e)");

            Assert.AreEqual(k.Evaluate(d), (double)4);

        }



        /// <summary>

        /// Looks up variable names, uses a normalizer in some examples.

        /// </summary>

        [TestMethod]

        public void TestEvaluate3()
        {

            Dictionary<string, double> d = new Dictionary<string, double>();

            d.Add("a", 50);

            d.Add("A", 60);

            d.Add("b", 55);

            d.Add("B", 130);

            d.Add("c", 2.3);

            d.Add("C", -3);

            d.Add("d", 0);

            d.Add("D", 2);

            d.Add("e", .004);

            d.Add("E", 43.002319);

            Func<string, double> lookup;

            lookup = (string s) => d[s];

            Func<string, string> upperc, lowerc, switchc;

            upperc = (string s) => s.ToUpper();

            lowerc = (string s) => s.ToLower();

            switchc = (string s) => { foreach (char c in s) { if (char.IsUpper(c)) { s = char.ToLower(c).ToString(); } else { s = char.ToUpper(c).ToString(); } } return s; };

            Formula f = new Formula("a+b", upperc, a => true);

            Assert.AreEqual(f.Evaluate(lookup), (d["A"] + d["B"]));

            Formula g = new Formula("A+B");

            Assert.AreEqual(f.Evaluate(lookup), g.Evaluate(lookup));

            Formula j = new Formula("C*E/B", lowerc, a => true);

            Assert.AreEqual(j.Evaluate(lookup), new Formula("c*e/b").Evaluate(lookup));

            Formula h = new Formula("D+e+C", switchc, s => true);

            Formula i = new Formula("d+E+c");

            Assert.AreEqual(i.Evaluate(lookup), h.Evaluate(lookup));

        }

        /// <summary>

        /// Tests Evaluate()

        /// Makes sure that a FormulaError is returned when

        /// a variable was unsuccessfully 'looked-up'.

        /// </summary>

        [TestMethod]

        public void TestEvaluate4()
        {

            Dictionary<string, double> d = new Dictionary<string, double>();

            d.Add("how_are_you", 5.2);

            Formula k = new Formula("a+b*(a+(2+5e-3)/2)");

            Assert.IsTrue(k.Evaluate(s => d[s]).GetType().ToString() == "SpreadsheetUtilities.FormulaError");

            FormulaError o = (FormulaError)k.Evaluate(s => d[s]);

            Assert.IsTrue(o.Reason != null);

            Assert.IsTrue(o.Reason != "");

            Assert.IsTrue(o.Reason != " ");

        }

        /// <summary>

        /// Stress test.

        /// Tests various interesting equations.

        /// </summary>

        [TestMethod]

        public void StressTest1()
        {

            Dictionary<string, double> d = new Dictionary<string, double>();

            d.Add("A1", 2.5);

            d.Add("__", 5.5);

            Formula f = new Formula("8+(3*(3*5)+(5))*6-3*( \t \n 3+8*3-9)*234*sdlfkasofwoilnvashasjgdslguoruvlasiru230453209587234879582348709532845");

            Formula g = new Formula("5 +(5+3+5) * asdfalsdkfjalskdfjasldkfjaslkfj240329 *.4 + askdfjskSLDKFJSLDFKJLSDKF5473 *58.3743487 -(30 *59/32/43/ 43/4+3-7/3*5/3+900093/43/23 /12/32/34/54/asdkflaskdfjalskd89898989)");

            Formula h = new Formula("(5e4 + e2 * 5.03 / 23 +54 -32) *(0)");

            Formula i = new Formula("a1+(1+(2-(3*(4/(5e5)/4)*3)-2)+1)+__", s => s.ToUpper(), s => true);

            Assert.AreEqual((int)((double)(i.Evaluate(s => d[s])) + .1), 10);

            Assert.AreEqual(f.Evaluate(s => 5), (double)-62872);

            Assert.AreEqual((int)((double)(g.Evaluate(s => (5e2 + .0802)))), 31798);

            Assert.AreEqual(h.Evaluate(s => 2.00005), (double)0);

        }
    }
}
