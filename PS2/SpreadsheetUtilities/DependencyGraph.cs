// Written by Daniel Avery for CS 3500, September 2015.
// Version 1.1 (Updated with Grading Tests, cleaned up implementation)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// (s1,t1) is an ordered pair of strings
    /// s1 depends on t1 --> t1 must be evaluated before s1
    /// 
    /// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
    /// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
    /// (Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
    /// set, and the element is already in the set, the set remains unchanged.)
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
    ///        
    ///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
    //
    // For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    //     dependents("a") = {"b", "c"}
    //     dependents("b") = {"d"}
    //     dependents("c") = {}
    //     dependents("d") = {"d"}
    //     dependees("a") = {}
    //     dependees("b") = {"a"}
    //     dependees("c") = {"a"}
    //     dependees("d") = {"b", "d"}
    /// </summary>
    public class DependencyGraph
    {
        /// <summary>
        /// One dictionary will map dependents to dependees
        /// </summary>
        private Dictionary<string, List<string>> dependents;


        /// <summary>
        /// One dictionary will map dependees to dependents
        /// </summary>
        private Dictionary<string, List<string>> dependees;


        /// <summary>
        /// Counter variable to track the number of ordered pairs in the DependencyGraph
        /// </summary>
        private int orderedPairs;


        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {
            dependents = new Dictionary<string, List<string>>();
            dependees = new Dictionary<string, List<string>>();
            orderedPairs = 0;
        }


        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
        public int Size
        {
            get { return orderedPairs; }
        }


        /// <summary>
        /// The size of dependees(s).
        /// This property is an example of an indexer.  If dg is a DependencyGraph, you would
        /// invoke it like this:
        /// dg["a"]
        /// It should return the size of dependees("a")
        /// </summary>
        public int this[string s]
        {
            get 
            {
                if(HasDependees(s))
                    return dependees[s].Count;

                return 0;
            }
        }


        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// </summary>
        public bool HasDependents(string s)
        {
            return dependents.ContainsKey(s);
        }


        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary>
        public bool HasDependees(string s)
        {
            return dependees.ContainsKey(s);
        }


        /// <summary>
        /// Enumerates dependents(s).
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            if(HasDependents(s))
                return dependents[s];

            return new List<string>();
        }

        /// <summary>
        /// Enumerates dependees(s).
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            if (HasDependees(s))
                return dependees[s];

            return new List<string>();
        }


        /// <summary>
        /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
        /// 
        /// <para>This should be thought of as:</para>   
        /// 
        ///   s depends on t
        ///
        /// </summary>
        /// <param name="s"> s cannot be evaluated until t is</param>
        /// <param name="t"> t must be evaluated first.  S depends on T</param>
        public void AddDependency(string s, string t)
        {
            // Add s to the dependents dictionary keys if it is not already there
            if (!HasDependents(s))
                dependents.Add(s, new List<string>());

            // Add t to the dependees dictionary keys if it is not already there
            if (!HasDependees(t))
                dependees.Add(t, new List<string>());

            // Check if the relationship exists in one dictionary; if not, it needs to be defined in both
            if (!dependents[s].Contains(t))
            {
                dependents[s].Add(t);
                dependees[t].Add(s);

                // Increment the count of ordered pairs
                orderedPairs++;
            }
        }


        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        public void RemoveDependency(string s, string t)
        {
            // First check if the ordered pair exists - the dependent s exists and has dependee t
            if(HasDependents(s) && dependents[s].Contains(t))
            {
                // If we got here, we can remove the relationship from both dictionaries
                dependents[s].Remove(t);
                dependees[t].Remove(s);

                // We can also decrement the count of ordered pairs now
                orderedPairs--;
            }
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r).  Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            // First remove all the existing ordered pairs involving dependent s
            if(HasDependents(s))
            {
                // Loop until there are no more ordered pairs involving s
                while(dependents[s].Count() > 0)
                    RemoveDependency(s, dependents[s].ElementAt(0));
            }

            // Now add the new dependees of s
            foreach (string item in newDependents)
                AddDependency(s, item);
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
            // First remove all the existing ordered pairs involving dependee s
            if (HasDependees(s))
            {
                // Loop until there are no more ordered pairs involving s
                while (dependees[s].Count() > 0)
                    RemoveDependency(dependees[s].ElementAt(0), s);
            }

            // Now add the new dependents of s
            foreach (string item in newDependees)
                AddDependency(item, s);
        }
    }
}