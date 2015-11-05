// Written by Daniel Avery and Keeton Hodgson
// November 2015, Version 1.0

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;
using SS;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SpreadsheetTests
{
    /// <summary>
    /// Tester for the Spreadsheet class
    /// </summary>
    [TestClass()]
    public class UnitTest1
    {
        /// <summary>
        /// Test that GetNamesOfAllNonemptyCells works for straightforward nonempty cells
        /// </summary>
        [TestMethod()]
        public void TestGetNamesOfAllNonemptyCells1()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("a1", "3");
            ss.SetContentsOfCell("a2", "this");
            ss.SetContentsOfCell("a3", "=x1+y3");

            IEnumerable<String> correct = new HashSet<String> { "a1", "a2", "a3" };

            // Compare each element in both collections
            IEnumerator<String> a = correct.GetEnumerator();
            IEnumerator<String> b = ss.GetNamesOfAllNonemptyCells().GetEnumerator();

            while (a.MoveNext() && b.MoveNext())
            {
                Assert.AreEqual(a.Current, b.Current);
            }

            Assert.IsFalse(a.MoveNext() || b.MoveNext());
        }

        /// <summary>
        /// Test that GetNamesOfAllNonemptyCells works if an empty ("") cell is added
        /// </summary>
        [TestMethod()]
        public void TestGetNamesOfAllNonemptyCells2()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("a1", "3");
            ss.SetContentsOfCell("a2", "this");
            ss.SetContentsOfCell("a3", "=x2+y3");
            ss.SetContentsOfCell("a4", "");

            IEnumerable<String> correct = new HashSet<String> { "a1", "a2", "a3" };

            // Compare each element in both collections
            IEnumerator<String> a = correct.GetEnumerator();
            IEnumerator<String> b = ss.GetNamesOfAllNonemptyCells().GetEnumerator();

            while (a.MoveNext() && b.MoveNext())
            {
                Assert.AreEqual(a.Current, b.Current);
            }

            Assert.IsFalse(a.MoveNext() || b.MoveNext());
        }

        /// <summary>
        /// Test that an invalid name (invalid format) throws an exception
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestGetCellContents1()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.GetCellContents("2x");
        }

        /// <summary>
        /// Test that an invalid name (invalid char) throws an exception
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestGetCellContents2()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.GetCellContents("x23;");
        }

        /// <summary>
        /// Test that a null name throws an exception
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestGetCellContents3()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.GetCellContents(null);
        }

        /// <summary>
        /// Test that a non-initialized cell yields an empty string
        /// </summary>
        [TestMethod()]
        public void TestGetCellContents4()
        {
            Spreadsheet ss = new Spreadsheet();

            Assert.AreEqual("", ss.GetCellContents("f7"));
        }

        /// <summary>
        /// Test that GetCellContents returns accurate contents for all the usual types
        /// </summary>
        [TestMethod()]
        public void TestGetCellContents5()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("c4", "yolo");
            ss.SetContentsOfCell("b8", "5e+2");
            ss.SetContentsOfCell("ab4", "=x2+ 3.0");

            Assert.AreEqual("yolo", ss.GetCellContents("c4"));
            Assert.AreEqual(5e+2, ss.GetCellContents("b8"));
            Assert.AreEqual(new Formula("x2+3"), ss.GetCellContents("ab4"));
        }

        /// <summary>
        /// Tests that SetContentsofCell throws an exception if name is null
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetContentsofCell1()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell(null, "3");
        }

        /// <summary>
        /// Tests that SetContentsofCell throws an exception if name is invalid
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetContentsofCell2()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("*uu3", "3");
        }

        /// <summary>
        /// Tests that SetContentsofCell and GetCellContents work together properly
        /// </summary>
        [TestMethod()]
        public void TestSetContentsofCell3()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A1", "3");
            Assert.AreEqual(3.0, ss.GetCellContents("A1"));
        }

        /// <summary>
        /// Tests that SetContentsofCell returns the correct set
        /// </summary>
        [TestMethod()]
        public void TestSetContentsofCell4()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("B1", "=A1*2");
            ss.SetContentsOfCell("C1", "=B1+A1");

            IEnumerable<String> correct = new HashSet<String> { "A1", "B1", "C1" };

            // Compare each element in both collections
            IEnumerator<String> a = correct.GetEnumerator();
            IEnumerator<String> b = ss.SetContentsOfCell("A1", "3").GetEnumerator();

            while (a.MoveNext() && b.MoveNext())
            {
                Assert.AreEqual(a.Current, b.Current);
            }

            Assert.IsFalse(a.MoveNext() || b.MoveNext());
        }

        /// <summary>
        /// Tests that SetContentsofCell throws an exception on a direct circular dependency
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(CircularException))]
        public void TestSetContentsofCell5()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("B1", "=B1 * 4");
        }

        /// <summary>
        /// Tests that SetContentsofCell throws an exception on an indirect circular dependency
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(CircularException))]
        public void TestSetContentsofCell6()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("B1", "=A1/5 ");
            ss.SetContentsOfCell("C1", "=B1*2");
            ss.SetContentsOfCell("d3", "=c2+9*A1");
            ss.SetContentsOfCell("fdh32", "=C1 + d3");
            ss.SetContentsOfCell("A1", "=fdh32 + C1");
        }

        /// <summary>
        /// Tests that SetContentsofCell throws an exception on null string contents
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSetContentsofCell7()
        {
            string text = null;

            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("B1", text);
        }


        // ----------- GRADING TESTS (Adapted from PS4) ---------------------------------


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        // EMPTY SPREADSHEETS
        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test1()
        {
            Spreadsheet s = new Spreadsheet();
            s.GetCellContents(null);
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test2()
        {
            Spreadsheet s = new Spreadsheet();
            s.GetCellContents("1AA");
        }

        [TestMethod()]
        public void Test3()
        {
            Spreadsheet s = new Spreadsheet();
            Assert.AreEqual("", s.GetCellContents("A2"));
        }

        // SETTING CELL TO A DOUBLE
        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test4()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell(null, "1.5");
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test5()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("1A1A", "1.5");
        }

        [TestMethod()]
        public void Test6()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("Z7", "1.5");
            Assert.AreEqual(1.5, (double)s.GetCellContents("Z7"), 1e-9);
        }

        // SETTING CELL TO A STRING
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test7()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A8", (string)null);
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test8()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell(null, "hello");
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test9()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("1AZ", "hello");
        }

        [TestMethod()]
        public void Test10()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("Z7", "hello");
            Assert.AreEqual("hello", s.GetCellContents("Z7"));
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test12()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell(null, "=2");
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test13()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("1AZ", "=2");
        }

        [TestMethod()]
        public void Test14()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("Z7", "=3");
            Formula f = (Formula)s.GetCellContents("Z7");
            Assert.AreEqual(new Formula("3"), f);
            Assert.AreNotEqual(new Formula("2"), f);
        }

        // CIRCULAR FORMULA DETECTION
        [TestMethod()]
        [ExpectedException(typeof(CircularException))]
        public void Test15()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "=A1");
        }

        [TestMethod()]
        [ExpectedException(typeof(CircularException))]
        public void Test16()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A3", "=A4+A5");
            s.SetContentsOfCell("A5", "=A6+A7");
            s.SetContentsOfCell("A7", "=A1+A1");
        }

        [TestMethod()]
        [ExpectedException(typeof(CircularException))]
        public void Test17()
        {
            Spreadsheet s = new Spreadsheet();
            try
            {
                s.SetContentsOfCell("A1", "=A2+A3");
                s.SetContentsOfCell("A2", "15");
                s.SetContentsOfCell("A3", "30");
                s.SetContentsOfCell("A2", "=A3*A1");
            }
            catch (SS.CircularException e)
            {
                Assert.AreEqual(15, (double)s.GetCellContents("A2"), 1e-9);
                throw e;
            }
        }

        // NONEMPTY CELLS
        [TestMethod()]
        public void Test18()
        {
            Spreadsheet s = new Spreadsheet();
            Assert.IsFalse(s.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        [TestMethod()]
        public void Test19()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "");
            Assert.IsFalse(s.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        [TestMethod()]
        public void Test20()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "hello");
            Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "B1" }));
        }

        [TestMethod()]
        public void Test21()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "52.25");
            Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "B1" }));
        }

        [TestMethod()]
        public void Test22()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "=3.5");
            Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "B1" }));
        }

        [TestMethod()]
        public void Test23()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "17.2");
            s.SetContentsOfCell("C1", "hello");
            s.SetContentsOfCell("B1", "=3.5");
            Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "A1", "B1", "C1" }));
        }

        // RETURN VALUE OF SET CELL CONTENTS
        [TestMethod()]
        public void Test24()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "hello");
            s.SetContentsOfCell("C1", "=5");
            Assert.IsTrue(s.SetContentsOfCell("A1", "17.2").SetEquals(new HashSet<string>() { "A1" }));
        }

        [TestMethod()]
        public void Test25()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "17.2");
            s.SetContentsOfCell("C1", "=5");
            Assert.IsTrue(s.SetContentsOfCell("B1", "hello").SetEquals(new HashSet<string>() { "B1" }));
        }

        [TestMethod()]
        public void Test26()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "17.2");
            s.SetContentsOfCell("B1", "hello");
            Assert.IsTrue(s.SetContentsOfCell("C1", "=5").SetEquals(new HashSet<string>() { "C1" }));
        }

        [TestMethod()]
        public void Test27()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A2", "6");
            s.SetContentsOfCell("A3", "=A2+A4");
            s.SetContentsOfCell("A4", "=A2+A5");
            Assert.IsTrue(s.SetContentsOfCell("A5", "82.5").SetEquals(new HashSet<string>() { "A5", "A4", "A3", "A1" }));
        }

        // CHANGING CELLS
        [TestMethod()]
        public void Test28()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A1", "2.5");
            Assert.AreEqual(2.5, (double)s.GetCellContents("A1"), 1e-9);
        }

        [TestMethod()]
        public void Test29()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A1", "Hello");
            Assert.AreEqual("Hello", (string)s.GetCellContents("A1"));
        }

        [TestMethod()]
        public void Test30()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "Hello");
            s.SetContentsOfCell("A1", "=23");
            Assert.AreEqual(new Formula("23"), (Formula)s.GetCellContents("A1"));
            Assert.AreNotEqual(new Formula("24"), (Formula)s.GetCellContents("A1"));
        }

        // STRESS TESTS
        [TestMethod()]
        public void Test31()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=B1+B2");
            s.SetContentsOfCell("B1", "=C1-C2");
            s.SetContentsOfCell("B2", "=C3*C4");
            s.SetContentsOfCell("C1", "=D1*D2");
            s.SetContentsOfCell("C2", "=D3*D4");
            s.SetContentsOfCell("C3", "=D5*D6");
            s.SetContentsOfCell("C4", "=D7*D8");
            s.SetContentsOfCell("D1", "=E1");
            s.SetContentsOfCell("D2", "=E1");
            s.SetContentsOfCell("D3", "=E1");
            s.SetContentsOfCell("D4", "=E1");
            s.SetContentsOfCell("D5", "=E1");
            s.SetContentsOfCell("D6", "=E1");
            s.SetContentsOfCell("D7", "=E1");
            s.SetContentsOfCell("D8", "=E1");
            ISet<String> cells = s.SetContentsOfCell("E1", "0");
            Assert.IsTrue(new HashSet<string>() { "A1", "B1", "B2", "C1", "C2", "C3", "C4", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "E1" }.SetEquals(cells));
        }
        [TestMethod()]
        public void Test32()
        {
            Test31();
        }
        [TestMethod()]
        public void Test33()
        {
            Test31();
        }
        [TestMethod()]
        public void Test34()
        {
            Test31();
        }

        [TestMethod()]
        public void Test35()
        {
            Spreadsheet s = new Spreadsheet();
            ISet<String> cells = new HashSet<string>();
            for (int i = 1; i < 200; i++)
            {
                cells.Add("A" + i);
                Assert.IsTrue(cells.SetEquals(s.SetContentsOfCell("A" + i, "=A" + (i + 1))));
            }
        }
        [TestMethod()]
        public void Test36()
        {
            Test35();
        }
        [TestMethod()]
        public void Test37()
        {
            Test35();
        }
        [TestMethod()]
        public void Test38()
        {
            Test35();
        }
        [TestMethod()]
        public void Test39()
        {
            Spreadsheet s = new Spreadsheet();
            for (int i = 1; i < 200; i++)
            {
                s.SetContentsOfCell("A" + i, "=A" + (i + 1));
            }
            try
            {
                s.SetContentsOfCell("A150", "=A50");
                Assert.Fail();
            }
            catch (CircularException)
            {
            }
        }
        [TestMethod()]
        public void Test40()
        {
            Test39();
        }
        [TestMethod()]
        public void Test41()
        {
            Test39();
        }
        [TestMethod()]
        public void Test42()
        {
            Test39();
        }

        [TestMethod()]
        public void Test43()
        {
            Spreadsheet s = new Spreadsheet();
            for (int i = 0; i < 500; i++)
            {
                s.SetContentsOfCell("A1" + i, "=A1" + (i + 1));
            }
            HashSet<string> firstCells = new HashSet<string>();
            HashSet<string> lastCells = new HashSet<string>();
            for (int i = 0; i < 250; i++)
            {
                firstCells.Add("A1" + i);
                lastCells.Add("A1" + (i + 250));
            }

            Assert.IsTrue(s.SetContentsOfCell("A1249", "25.0").SetEquals(firstCells));
            Assert.IsTrue(s.SetContentsOfCell("A1499", "0").SetEquals(lastCells));
        }

        [TestMethod()]
        public void Test44()
        {
            Test43();
        }
        [TestMethod()]
        public void Test45()
        {
            Test43();
        }
        [TestMethod()]
        public void Test46()
        {
            Test43();
        }

        [TestMethod()]
        public void Test47()
        {
            RunRandomizedTest(47, 2519);
        }
        [TestMethod()]
        public void Test48()
        {
            RunRandomizedTest(48, 2521);
        }
        [TestMethod()]
        public void Test49()
        {
            RunRandomizedTest(49, 2526);
        }
        [TestMethod()]
        public void Test50()
        {
            RunRandomizedTest(50, 2521);
        }

        public void RunRandomizedTest(int seed, int size)
        {
            Spreadsheet s = new Spreadsheet();
            Random rand = new Random(seed);
            for (int i = 0; i < 10000; i++)
            {
                try
                {
                    switch (rand.Next(3))
                    {
                        case 0:
                            s.SetContentsOfCell(randomName(rand), "3.14");
                            break;
                        case 1:
                            s.SetContentsOfCell(randomName(rand), "hello");
                            break;
                        case 2:
                            s.SetContentsOfCell(randomName(rand), randomFormula(rand));
                            break;
                    }
                }
                catch (CircularException)
                {
                }
            }
            ISet<string> set = new HashSet<string>(s.GetNamesOfAllNonemptyCells());
            Assert.AreEqual(size, set.Count);
        }

        private String randomName(Random rand)
        {
            return "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Substring(rand.Next(26), 1) + (rand.Next(99) + 1);
        }

        private String randomFormula(Random rand)
        {
            String f = randomName(rand);
            for (int i = 0; i < 10; i++)
            {
                switch (rand.Next(4))
                {
                    case 0:
                        f += "+";
                        break;
                    case 1:
                        f += "-";
                        break;
                    case 2:
                        f += "*";
                        break;
                    case 3:
                        f += "/";
                        break;
                }
                switch (rand.Next(2))
                {
                    case 0:
                        f += 7.2;
                        break;
                    case 1:
                        f += randomName(rand);
                        break;
                }
            }
            return f;
        }


        // ----------- END OF ADAPTED TESTS ----------------------------------------
        // ----------- START OF NEW TESTS ------------------------------------------

        /// <summary>
        /// Test get with double value set
        /// </summary>
        [TestMethod()]
        public void TestGetCellValue1()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("a1", "2.6");
            Assert.AreEqual(2.6, ss.GetCellValue("a1"));
        }

        /// <summary>
        /// Test get with string value set
        /// </summary>
        [TestMethod()]
        public void TestGetCellValue2()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("a1", "hello");
            Assert.AreEqual("hello", ss.GetCellValue("a1"));
        }

        /// <summary>
        /// Test get with Formula set (that returns FormulaError)
        /// </summary>
        [TestMethod()]
        public void TestGetCellValue3()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("a1", "=3e9 + b2");
            Assert.IsTrue(ss.GetCellValue("a1") is FormulaError);
        }

        /// <summary>
        /// Test get with Formula set (that returns double)
        /// </summary>
        [TestMethod()]
        public void TestGetCellValue4()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("a1", "=               b1 + c1+d1");
            ss.SetContentsOfCell("b1", "3");
            ss.SetContentsOfCell("c1", "=b1 + 2");
            ss.SetContentsOfCell("d1", "=c1 + b1");
            Assert.AreEqual(16.0, ss.GetCellValue("a1"));
        }

        /// <summary>
        /// Test that empty Formula set throws exception
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestSetwithFormula()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("a1", "= ");
        }

        /// <summary>
        /// Test another empty Formula set
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestSetwithFormula2()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("a1", "=       ");
        }

        /// <summary>
        /// Test that '=' set becomes a string cell
        /// </summary>
        [TestMethod()]
        public void TestSetwithFormula3()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("a1", "=");
            Assert.AreEqual("=", ss.GetCellContents("a1"));
            Assert.AreEqual("=", ss.GetCellValue("a1"));
        }

        /// <summary>
        /// Test that Formula cell retains old info if new info causes circular exception
        /// </summary>
        [TestMethod()]
        public void TestSetwithFormula4()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("a1", "= b1+c1");
            ss.SetContentsOfCell("b1", "2");
            ss.SetContentsOfCell("c1", "b1*3");

            Assert.IsTrue(ss.Changed);
            ss.Save("TestSpreadsheet.xml");
            Assert.IsFalse(ss.Changed);

            try
            {
                ss.SetContentsOfCell("b1", "= a1 +500");
            }
            catch (CircularException)
            {
                Assert.AreEqual(2.0, ss.GetCellContents("b1"));
                Assert.AreEqual(2.0, ss.GetCellValue("b1"));
                Assert.IsFalse(ss.Changed);
            }
        }

        /// <summary>
        /// Same as last test, but Changed should be true from previous set after Save
        /// </summary>
        [TestMethod()]
        public void TestSetwithFormula5()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("a1", "= b1+c1");
            ss.SetContentsOfCell("b1", "2");
            ss.SetContentsOfCell("c1", "b1*3");

            Assert.IsTrue(ss.Changed);
            ss.Save("TestSpreadsheet.xml");
            Assert.IsFalse(ss.Changed);

            try
            {
                ss.SetContentsOfCell("c1", "=d1+4");
                ss.SetContentsOfCell("b1", "= a1 +500");
            }
            catch (CircularException)
            {
                Assert.AreEqual(2.0, ss.GetCellContents("b1"));
                Assert.AreEqual(2.0, ss.GetCellValue("b1"));
                Assert.IsTrue(ss.Changed);
            }
        }

        /// <summary>
        /// Test spreadsheet is not changed if same value is added to same cell
        /// </summary>
        [TestMethod()]
        public void TestSetwithSameValue1()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("a1", "= a2 +a4");
            ss.Save("TestSpreadsheet.xml");
            ss.SetContentsOfCell("a1", "= a2 +  a4");
            Assert.IsFalse(ss.Changed);
        }

        /// <summary>
        /// Test spreadsheet is not changed if same value is added to same cell
        /// </summary>
        [TestMethod()]
        public void TestSetwithSameValue2()
        {
            Spreadsheet ss = new Spreadsheet(s => true, s => s.ToUpper(), "3");
            ss.SetContentsOfCell("a1", "hello");
            ss.Save("TestSpreadsheet.xml");
            ss.SetContentsOfCell("A1", "hello");
            Assert.IsFalse(ss.Changed);
        }

        /// <summary>
        /// Test spreadsheet is not changed if same value is added to same cell
        /// </summary>
        [TestMethod()]
        public void TestSetwithSameValue3()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("a1", "2.0");
            ss.Save("TestSpreadsheet.xml");
            ss.SetContentsOfCell("a1", "2e0");
            Assert.IsFalse(ss.Changed);
        }

        /// <summary>
        /// Test that spreadsheets are saved and created from files properly
        /// </summary>
        [TestMethod()]
        public void TestSave()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("s1", "hello");
            ss.SetContentsOfCell("ab3", "45");
            ss.SetContentsOfCell("f9", "= c2+ c3");
            ss.SetContentsOfCell("c2", "= ab3*5");
            ss.SetContentsOfCell("c3", "5.7");

            ss.Save("TestSpreadsheet.xml");

            Spreadsheet ss2 = new Spreadsheet("TestSpreadsheet.xml", s => true, s => s, "default");

            Assert.IsTrue(ss2.SetContentsOfCell("z0", "=f9 + c2").SetEquals(new HashSet<String>() { "z0" }));
            Assert.AreEqual(455.7, ss2.GetCellValue("z0"));
        }


        // PS5 GRADING TESTS --------------------------------------------------------------------


        /// Verifies cells and their values, which must alternate.
        public void VV(AbstractSpreadsheet sheet, params object[] constraints)
        {
            for (int i = 0; i < constraints.Length; i += 2)
            {
                if (constraints[i + 1] is double)
                {
                    Assert.AreEqual((double)constraints[i + 1], (double)sheet.GetCellValue((string)constraints[i]), 1e-9);
                }
                else
                {
                    Assert.AreEqual(constraints[i + 1], sheet.GetCellValue((string)constraints[i]));
                }
            }
        }


        /// For setting a spreadsheet cell.
        public IEnumerable<string> Set(AbstractSpreadsheet sheet, string name, string contents)
        {
            List<string> result = new List<string>(sheet.SetContentsOfCell(name, contents));
            return result;
        }

        /// Tests IsValid
        [TestMethod()]
        public void IsValidTest1()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "x");
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void IsValidTest2()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => s[0] != 'A', s => s, "");
            ss.SetContentsOfCell("A1", "x");
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        public void IsValidTest3()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "= A1 + C1");
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void IsValidTest4()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => s[0] != 'A', s => s, "");
            ss.SetContentsOfCell("B1", "= A1 + C1");
        }

        /// Tests Normalize
        [TestMethod()]
        public void NormalizeTest1()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "hello");
            Assert.AreEqual("", s.GetCellContents("b1"));
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        public void NormalizeTest2()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => true, s => s.ToUpper(), "");
            ss.SetContentsOfCell("B1", "hello");
            Assert.AreEqual("hello", ss.GetCellContents("b1"));
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        public void NormalizeTest3()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("a1", "5");
            s.SetContentsOfCell("A1", "6");
            s.SetContentsOfCell("B1", "= a1");
            Assert.AreEqual(5.0, (double)s.GetCellValue("B1"), 1e-9);
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        public void NormalizeTest4()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => true, s => s.ToUpper(), "");
            ss.SetContentsOfCell("a1", "5");
            ss.SetContentsOfCell("A1", "6");
            ss.SetContentsOfCell("B1", "= a1");
            Assert.AreEqual(6.0, (double)ss.GetCellValue("B1"), 1e-9);
        }

        /// Simple tests
        [TestMethod()]
        public void EmptySheet()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            VV(ss, "A1", "");
        }

        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        public void OneString()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            OneString(ss);
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        public void OneString(AbstractSpreadsheet ss)
        {
            Set(ss, "B1", "hello");
            VV(ss, "B1", "hello");
        }

        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        public void OneNumber()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            OneNumber(ss);
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        public void OneNumber(AbstractSpreadsheet ss)
        {
            Set(ss, "C1", "17.5");
            VV(ss, "C1", 17.5);
        }

        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        public void OneFormula()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            OneFormula(ss);
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        public void OneFormula(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "B1", "5.2");
            Set(ss, "C1", "= A1+B1");
            VV(ss, "A1", 4.1, "B1", 5.2, "C1", 9.3);
        }

        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        public void Changed()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Assert.IsFalse(ss.Changed);
            Set(ss, "C1", "17.5");
            Assert.IsTrue(ss.Changed);
        }

        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        public void DivisionByZero1()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            DivisionByZero1(ss);
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        public void DivisionByZero1(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "B1", "0.0");
            Set(ss, "C1", "= A1 / B1");
            Assert.IsInstanceOfType(ss.GetCellValue("C1"), typeof(FormulaError));
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        public void DivisionByZero2()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            DivisionByZero2(ss);
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        public void DivisionByZero2(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "5.0");
            Set(ss, "A3", "= A1 / 0.0");
            Assert.IsInstanceOfType(ss.GetCellValue("A3"), typeof(FormulaError));
        }


        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        public void EmptyArgument()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            EmptyArgument(ss);
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        public void EmptyArgument(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "C1", "= A1 + B1");
            Assert.IsInstanceOfType(ss.GetCellValue("C1"), typeof(FormulaError));
        }

        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        public void StringArgument()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            StringArgument(ss);
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        public void StringArgument(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "B1", "hello");
            Set(ss, "C1", "= A1 + B1");
            Assert.IsInstanceOfType(ss.GetCellValue("C1"), typeof(FormulaError));
        }

        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        public void ErrorArgument()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ErrorArgument(ss);
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        public void ErrorArgument(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "B1", "");
            Set(ss, "C1", "= A1 + B1");
            Set(ss, "D1", "= C1");
            Assert.IsInstanceOfType(ss.GetCellValue("D1"), typeof(FormulaError));
        }

        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        public void NumberFormula1()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            NumberFormula1(ss);
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        public void NumberFormula1(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "C1", "= A1 + 4.2");
            VV(ss, "C1", 8.3);
        }

        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        public void NumberFormula2()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            NumberFormula2(ss);
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        public void NumberFormula2(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "= 4.6");
            VV(ss, "A1", 4.6);
        }


        /// Repeats the simple tests all together
        [TestMethod()]
        public void RepeatSimpleTests()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Set(ss, "A1", "17.32");
            Set(ss, "B1", "This is a test");
            Set(ss, "C1", "= A1+B1");
            OneString(ss);
            OneNumber(ss);
            OneFormula(ss);
            DivisionByZero1(ss);
            DivisionByZero2(ss);
            StringArgument(ss);
            ErrorArgument(ss);
            NumberFormula1(ss);
            NumberFormula2(ss);
        }

        /// Four kinds of formulas
        [TestMethod()]
        public void Formulas()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Formulas(ss);
        }
        /// <summary>
         /// From the grading suite
         /// </summary>
        public void Formulas(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.4");
            Set(ss, "B1", "2.2");
            Set(ss, "C1", "= A1 + B1");
            Set(ss, "D1", "= A1 - B1");
            Set(ss, "E1", "= A1 * B1");
            Set(ss, "F1", "= A1 / B1");
            VV(ss, "C1", 6.6, "D1", 2.2, "E1", 4.4 * 2.2, "F1", 2.0);
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        public void Formulasa()
        {
            Formulas();
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        public void Formulasb()
        {
            Formulas();
        }


        /// Are multiple spreadsheets supported?
        [TestMethod()]
        public void Multiple()
        {
            AbstractSpreadsheet s1 = new Spreadsheet();
            AbstractSpreadsheet s2 = new Spreadsheet();
            Set(s1, "X1", "hello");
            Set(s2, "X1", "goodbye");
            VV(s1, "X1", "hello");
            VV(s2, "X1", "goodbye");
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        public void Multiplea()
        {
            Multiple();
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        public void Multipleb()
        {
            Multiple();
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        public void Multiplec()
        {
            Multiple();
        }

        /// Reading/writing spreadsheets
        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void SaveTest1()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.Save("q:\\missing\\save.txt");
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void SaveTest2()
        {
            AbstractSpreadsheet ss = new Spreadsheet("q:\\missing\\save.txt", s => true, s => s, "");
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        public void SaveTest3()
        {
            AbstractSpreadsheet s1 = new Spreadsheet();
            Set(s1, "A1", "hello");
            s1.Save("save1.txt");
            s1 = new Spreadsheet("save1.txt", s => true, s => s, "default");
            Assert.AreEqual("hello", s1.GetCellContents("A1"));
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void SaveTest4()
        {
            using (StreamWriter writer = new StreamWriter("save2.txt"))
            {
                writer.WriteLine("This");
                writer.WriteLine("is");
                writer.WriteLine("a");
                writer.WriteLine("test!");
            }
            AbstractSpreadsheet ss = new Spreadsheet("save2.txt", s => true, s => s, "");
        }
        /// <summary>
        /// From the grading suite
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void SaveTest5()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.Save("save3.txt");
            ss = new Spreadsheet("save3.txt", s => true, s => s, "version");
        }
        /// <summary>
        /// 
        /// </summary>
        [TestMethod()]
        public void SaveTest6()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => true, s => s, "hello");
            ss.Save("save4.txt");
            Assert.AreEqual("hello", new Spreadsheet().GetSavedVersion("save4.txt"));
        }
        /// <summary>
        /// 
        /// </summary>
        [TestMethod()]
        public void SaveTest7()
        {
            using (XmlWriter writer = XmlWriter.Create("save5.txt"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "");

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "A1");
                writer.WriteElementString("contents", "hello");
                writer.WriteEndElement();

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "A2");
                writer.WriteElementString("contents", "5.0");
                writer.WriteEndElement();

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "A3");
                writer.WriteElementString("contents", "4.0");
                writer.WriteEndElement();

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "A4");
                writer.WriteElementString("contents", "= A2 + A3");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            AbstractSpreadsheet ss = new Spreadsheet("save5.txt", s => true, s => s, "");
            VV(ss, "A1", "hello", "A2", 5.0, "A3", 4.0, "A4", 9.0);
        }
        /// <summary>
        /// 
        /// </summary>
        [TestMethod()]
        public void SaveTest8()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Set(ss, "A1", "hello");
            Set(ss, "A2", "5.0");
            Set(ss, "A3", "4.0");
            Set(ss, "A4", "= A2 + A3");
            ss.Save("save6.txt");
            using (XmlReader reader = XmlReader.Create("save6.txt"))
            {
                int spreadsheetCount = 0;
                int cellCount = 0;
                bool A1 = false;
                bool A2 = false;
                bool A3 = false;
                bool A4 = false;
                string name = null;
                string contents = null;

                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "spreadsheet":
                                Assert.AreEqual("default", reader["version"]);
                                spreadsheetCount++;
                                break;

                            case "cell":
                                cellCount++;
                                break;

                            case "name":
                                reader.Read();
                                name = reader.Value;
                                break;

                            case "contents":
                                reader.Read();
                                contents = reader.Value;
                                break;
                        }
                    }
                    else
                    {
                        switch (reader.Name)
                        {
                            case "cell":
                                if (name.Equals("A1")) { Assert.AreEqual("hello", contents); A1 = true; }
                                else if (name.Equals("A2")) { Assert.AreEqual(5.0, Double.Parse(contents), 1e-9); A2 = true; }
                                else if (name.Equals("A3")) { Assert.AreEqual(4.0, Double.Parse(contents), 1e-9); A3 = true; }
                                else if (name.Equals("A4")) { contents = contents.Replace(" ", ""); Assert.AreEqual("=A2+A3", contents); A4 = true; }
                                else Assert.Fail();
                                break;
                        }
                    }
                }
                Assert.AreEqual(1, spreadsheetCount);
                Assert.AreEqual(4, cellCount);
                Assert.IsTrue(A1);
                Assert.IsTrue(A2);
                Assert.IsTrue(A3);
                Assert.IsTrue(A4);
            }
        }


        /// Fun with formulas
        [TestMethod()]
        public void Formula1()
        {
            Formula1(new Spreadsheet());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ss"></param>
        public void Formula1(AbstractSpreadsheet ss)
        {
            Set(ss, "a1", "= a2 + a3");
            Set(ss, "a2", "= b1 + b2");
            Assert.IsInstanceOfType(ss.GetCellValue("a1"), typeof(FormulaError));
            Assert.IsInstanceOfType(ss.GetCellValue("a2"), typeof(FormulaError));
            Set(ss, "a3", "5.0");
            Set(ss, "b1", "2.0");
            Set(ss, "b2", "3.0");
            VV(ss, "a1", 10.0, "a2", 5.0);
            Set(ss, "b2", "4.0");
            VV(ss, "a1", 11.0, "a2", 6.0);
        }
        /// <summary>
        /// 
        /// </summary>
        [TestMethod()]
        public void Formula2()
        {
            Formula2(new Spreadsheet());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ss"></param>
        public void Formula2(AbstractSpreadsheet ss)
        {
            Set(ss, "a1", "= a2 + a3");
            Set(ss, "a2", "= a3");
            Set(ss, "a3", "6.0");
            VV(ss, "a1", 12.0, "a2", 6.0, "a3", 6.0);
            Set(ss, "a3", "5.0");
            VV(ss, "a1", 10.0, "a2", 5.0, "a3", 5.0);
        }
        /// <summary>
        /// 
        /// </summary>
        [TestMethod()]
        public void Formula3()
        {
            Formula3(new Spreadsheet());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ss"></param>
        public void Formula3(AbstractSpreadsheet ss)
        {
            Set(ss, "a1", "= a3 + a5");
            Set(ss, "a2", "= a5 + a4");
            Set(ss, "a3", "= a5");
            Set(ss, "a4", "= a5");
            Set(ss, "a5", "9.0");
            VV(ss, "a1", 18.0);
            VV(ss, "a2", 18.0);
            Set(ss, "a5", "8.0");
            VV(ss, "a1", 16.0);
            VV(ss, "a2", 16.0);
        }
        /// <summary>
        /// 
        /// </summary>
        [TestMethod()]
        public void Formula4()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Formula1(ss);
            Formula2(ss);
            Formula3(ss);
        }
        /// <summary>
        /// 
        /// </summary>
        [TestMethod()]
        public void Formula4a()
        {
            Formula4();
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod()]
        public void MediumSheet()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            MediumSheet(ss);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ss"></param>
        public void MediumSheet(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "1.0");
            Set(ss, "A2", "2.0");
            Set(ss, "A3", "3.0");
            Set(ss, "A4", "4.0");
            Set(ss, "B1", "= A1 + A2");
            Set(ss, "B2", "= A3 * A4");
            Set(ss, "C1", "= B1 + B2");
            VV(ss, "A1", 1.0, "A2", 2.0, "A3", 3.0, "A4", 4.0, "B1", 3.0, "B2", 12.0, "C1", 15.0);
            Set(ss, "A1", "2.0");
            VV(ss, "A1", 2.0, "A2", 2.0, "A3", 3.0, "A4", 4.0, "B1", 4.0, "B2", 12.0, "C1", 16.0);
            Set(ss, "B1", "= A1 / A2");
            VV(ss, "A1", 2.0, "A2", 2.0, "A3", 3.0, "A4", 4.0, "B1", 1.0, "B2", 12.0, "C1", 13.0);
        }
        /// <summary>
        /// 
        /// </summary>
        [TestMethod()]
        public void MediumSheeta()
        {
            MediumSheet();
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod()]
        public void MediumSave()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            MediumSheet(ss);
            ss.Save("save7.txt");
            ss = new Spreadsheet("save7.txt", s => true, s => s, "default");
            VV(ss, "A1", 2.0, "A2", 2.0, "A3", 3.0, "A4", 4.0, "B1", 1.0, "B2", 12.0, "C1", 13.0);
        }
        /// <summary>
        /// !!!!!!!!!!!!!!!!!!!!!! For some reason this test failed once!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// </summary>
        [TestMethod()]
        public void MediumSavea()
        {
            MediumSave();
        }


        /// A long chained formula.  If this doesn't finish within 60 seconds, it fails.
        /// FOR SOME REASON THIS FAILED ONCE
        [TestMethod()]
        public void LongFormulaTest()
        {
            object result = "";
            Thread t = new Thread(() => LongFormulaHelper(out result));
            t.Start();
            t.Join(60 * 1000);
            if (t.IsAlive)
            {
                t.Abort();
                Assert.Fail("Computation took longer than 60 seconds");
            }
            Assert.AreEqual("ok", result);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        public void LongFormulaHelper(out object result)
        {
            try
            {
                AbstractSpreadsheet s = new Spreadsheet();
                s.SetContentsOfCell("sum1", "= a1 + a2");
                int i;
                int depth = 100;

                for (i = 1; i <= depth * 2; i += 2)
                {
                    s.SetContentsOfCell("a" + i, "= a" + (i + 2) + " + a" + (i + 3));
                    s.SetContentsOfCell("a" + (i + 1), "= a" + (i + 2) + "+ a" + (i + 3));
                }

                s.SetContentsOfCell("a" + i, "1");
                s.SetContentsOfCell("a" + (i + 1), "1");
                Assert.AreEqual(Math.Pow(2, depth + 1), (double)s.GetCellValue("sum1"), 1.0);
                s.SetContentsOfCell("a" + i, "0");
                Assert.AreEqual(Math.Pow(2, depth), (double)s.GetCellValue("sum1"), 1.0);
                s.SetContentsOfCell("a" + (i + 1), "0");
                Assert.AreEqual(0.0, (double)s.GetCellValue("sum1"), 0.1);
                result = "ok";
            }
            catch (Exception e)
            {
                result = e;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod()]
        public void DependentValueAfterEmptySetter()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("B1", "= A1 + A2");
            ss.SetContentsOfCell("A1", "2");
            ss.SetContentsOfCell("A2", "3");
            Assert.AreEqual(5.0, ss.GetCellValue("B1"));
            ss.SetContentsOfCell("A1", "");
            Assert.IsInstanceOfType(ss.GetCellValue("B1"), typeof(FormulaError));
        }




        //Keeton's Tests :)
        /// <summary>
        /// Tests to make sure that the set returned has the right amount of items in it.
        /// </summary>
        [TestMethod]
        public void testSetCells()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Assert.AreEqual(1, ss.SetContentsOfCell("A2", "=A1").Count);
            Assert.AreEqual(2, ss.SetContentsOfCell("A1", "hello").Count);
            Assert.AreEqual(2, ss.SetContentsOfCell("A1", "43.5").Count);
            Assert.AreEqual(1, ss.SetContentsOfCell("A2", "=A1").Count);
            Assert.AreEqual(2, ss.SetContentsOfCell("A1", "=A5 + A4 + A3 + 43.5").Count);
            Assert.AreEqual(1, ss.SetContentsOfCell("A2", "=A1").Count);
            Assert.AreEqual(1, ss.SetContentsOfCell("A6", "=A2 + A1").Count);
            Assert.AreEqual(3, ss.SetContentsOfCell("A1", "43").Count);
            Assert.AreEqual(2, ss.SetContentsOfCell("A2", "43").Count);
            Assert.AreEqual(1, ss.SetContentsOfCell("A6", "hello").Count);
            Assert.AreEqual(1, ss.SetContentsOfCell("A1", "=A4 + A8").Count);

        }


        /// <summary>
        /// Test that creating a spreadsheet from a file with an invalid cell name throws exception
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestInvalidNamefromFile()
        {
            using (XmlWriter writer = XmlWriter.Create("test.xml"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "lol");
                writer.WriteStartElement("cell");
                writer.WriteElementString("name", ";3");
                writer.WriteElementString("contents", "hello");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            Spreadsheet ss = new Spreadsheet("text.xml", s => true, s => s, "lol");
        }


        /// <summary>
        /// Test that creating a spreadsheet from a file with a circular dependecy throws exception
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestCircularDependencyfromFile()
        {
            using (XmlWriter writer = XmlWriter.Create("test.xml"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "lol");
                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "a5");
                writer.WriteElementString("contents", "=a6");
                writer.WriteEndElement();
                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "a6");
                writer.WriteElementString("contents", "=a7");
                writer.WriteEndElement();
                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "a7");
                writer.WriteElementString("contents", "=a5");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            Spreadsheet ss = new Spreadsheet("text.xml", s => true, s => s, "lol");
        }


        /// <summary>
        /// Test that creating a spreadsheet from a file with a bad formula throws exception
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestInvalidFormulafromFile()
        {
            using (XmlWriter writer = XmlWriter.Create("test.xml"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "lol");
                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "a5");
                writer.WriteElementString("contents", "=a6");
                writer.WriteEndElement();
                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "a6");
                writer.WriteElementString("contents", "=a7/)");
                writer.WriteEndElement();
                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "a7");
                writer.WriteElementString("contents", "=a5");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            Spreadsheet ss = new Spreadsheet("text.xml", s => true, s => s, "lol");
        }
    }


    /// <summary>
    /// Class for testing the specific regex needed for the IsValid function of our final spreadsheet.
    ///     Valid variables:
    ///         1 letter, [A-Z]
    ///         1 number, [1-99]
    /// </summary>
    [TestClass]
    public class TestRegex
    {
        private string pattern = @"^[a-zA-Z]{1}[1-9]{1}[0-9]?$";//needs testing


        /// <summary>
        /// Tests an isValid function, see if it works.
        /// </summary>
        [TestMethod()]
        public void TestRegex0()
        {
            Regex r = new Regex(pattern);
            AbstractSpreadsheet ss = new Spreadsheet(s => r.IsMatch(s), s => s.ToUpper(), "PS6");
            ss.SetContentsOfCell("a3", "hello");//Ideally, this should work, because our normalize function should make it work.
            Assert.AreEqual(ss.GetCellContents("a3"), ss.GetCellContents("A3"));

        }

        /// <summary>
        /// Tests an isValid function, see if it works.
        /// </summary>
        [TestMethod()]
        public void TestRegex111()
        {
            Regex r = new Regex(pattern);
            AbstractSpreadsheet ss = new Spreadsheet(s => r.IsMatch(s), s => s.ToUpper(), "PS6");

            ss.SetContentsOfCell("a10", "=a4");
            Assert.AreEqual(ss.GetCellContents("a10"), ss.GetCellContents("A10"));

        }

        /// <summary>
        /// Tests an isValid function, see if it works.
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(SS.InvalidNameException))]
        public void TestRegex1()
        {
            Regex r = new Regex(pattern);
            AbstractSpreadsheet ss = new Spreadsheet(s => r.IsMatch(s), s => s.ToUpper(), "PS6");

            ss.SetContentsOfCell("a100", "=a4");//This should not work.
        }

        /// <summary>
        /// Tests an isValid function, see if it works.
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(SS.InvalidNameException))]
        public void TestRegex2()
        {
            Regex r = new Regex(pattern);
            AbstractSpreadsheet ss = new Spreadsheet(s => r.IsMatch(s), s => s.ToUpper(), "PS6");

            ss.SetContentsOfCell("A100", "=a4");//This should not work.
        }

        /// <summary>
        /// Tests an isValid function, see if it works.
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(SS.InvalidNameException))]
        public void TestRegex3()
        {
            Regex r = new Regex(pattern);
            AbstractSpreadsheet ss = new Spreadsheet(s => r.IsMatch(s), s => s.ToUpper(), "PS6");

            ss.SetContentsOfCell("a0", "hello");//Ideally, this should not work, because of the number with the a.
        }

        /// <summary>
        /// Tests an isValid function, see if it works.
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(SS.InvalidNameException))]
        public void TestRegex4()
        {
            Regex r = new Regex(pattern);
            AbstractSpreadsheet ss = new Spreadsheet(s => r.IsMatch(s), s => s.ToUpper(), "PS6");

            ss.SetContentsOfCell("A0", "hello");//Still shouldn't work.
        }

        /// <summary>
        /// Tests an isValid function, see if it works.
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(SS.InvalidNameException))]
        public void TestRegex5()
        {
            Regex r = new Regex(pattern);
            AbstractSpreadsheet ss = new Spreadsheet(s => r.IsMatch(s), s => s.ToUpper(), "PS6");

            ss.SetContentsOfCell("aa10", "=a4");//This should not work.
        }

        /// <summary>
        /// Tests an isValid function, see if it works.
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(SS.InvalidNameException))]
        public void TestRegex6()
        {
            Regex r = new Regex(pattern);
            AbstractSpreadsheet ss = new Spreadsheet(s => r.IsMatch(s), s => s.ToUpper(), "PS6");

            ss.SetContentsOfCell("AB3", "=a4");//This should not work.
        }

        /// <summary>
        /// Tests an isValid function, see if it works.
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(SS.InvalidNameException))]
        public void TestRegex7()
        {
            Regex r = new Regex(pattern);
            AbstractSpreadsheet ss = new Spreadsheet(s => r.IsMatch(s), s => s.ToUpper(), "PS6");

            ss.SetContentsOfCell("A", "=a4");//This should not work.
        }

        /// <summary>
        /// Tests an isValid function, see if it works.
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(SS.InvalidNameException))]
        public void TestRegex8()
        {
            Regex r = new Regex(pattern);
            AbstractSpreadsheet ss = new Spreadsheet(s => r.IsMatch(s), s => s.ToUpper(), "PS6");

            ss.SetContentsOfCell("9", "=a4");//This should not work.
        }

        /// <summary>
        /// Tests an isValid function, see if it works.
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(SS.InvalidNameException))]
        public void TestRegex10()
        {
            Regex r = new Regex(pattern);
            AbstractSpreadsheet ss = new Spreadsheet(s => r.IsMatch(s), s => s.ToUpper(), "PS6");

            ss.SetContentsOfCell("A07", "=a4");//This should not work.
        }

        /// <summary>
        /// Tests an isValid function, see if it works.
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(SS.InvalidNameException))]
        public void TestRegex11()
        {
            Regex r = new Regex(pattern);
            AbstractSpreadsheet ss = new Spreadsheet(s => r.IsMatch(s), s => s.ToUpper(), "PS6");

            ss.SetContentsOfCell("bc32", "=a4");//This should not work.
        }

        /// <summary>
        /// Tests an isValid function, see if it works.
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(SS.InvalidNameException))]
        public void TestRegex12()
        {
            Regex r = new Regex(pattern);
            AbstractSpreadsheet ss = new Spreadsheet(s => r.IsMatch(s), s => s.ToUpper(), "PS6");

            ss.SetContentsOfCell("32B", "=a4");//This should not work.
        }
    }
}
