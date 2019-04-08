using GalacticWasteManagement.Scripts;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalacticWasteManagement.Tests.Scripts
{
    public class ScriptUtilitiesTests
    {
        [Test]
        public void SplitInBatches_HappyDays()
        {
            var somethingToSplit = @"SELECT * FROM Table
              GO
              SELECT * FROM Table2";
            var actual = ScriptUtilities.SplitInBatches(somethingToSplit);
            Assert.AreEqual("SELECT * FROM Table", actual[0]);
            Assert.AreEqual("SELECT * FROM Table2", actual[1]);
            Assert.AreEqual(2, actual.Count());
        }

        [Test]
        public void SplitInBatches_HappyDays2()
        {
            var somethingToSplit = @"SELECT * FROM Table";
            var actual = ScriptUtilities.SplitInBatches(somethingToSplit);
            Assert.AreEqual("SELECT * FROM Table", actual[0]);
            Assert.AreEqual(1, actual.Count());
        }

        [Test]
        public void SplitInBatches_SemicolonsDoesntFuckThingsUp()
        {
            var somethingToSplit = @"SELECT * FROM Table
              GO;
              SELECT * FROM Table2";
            var actual = ScriptUtilities.SplitInBatches(somethingToSplit);
            Assert.AreEqual("SELECT * FROM Table", actual[0]);
            Assert.AreEqual("SELECT * FROM Table2", actual[1]);
            Assert.AreEqual(2, actual.Count());
        }

        [Test]
        public void SplitInBatches_CaseInsensitiveness()
        {
            var somethingToSplit = @"SELECT * FROM Table
              gO;
              SELECT * FROM Table2";
            var actual = ScriptUtilities.SplitInBatches(somethingToSplit);
            Assert.AreEqual("SELECT * FROM Table", actual[0]);
            Assert.AreEqual("SELECT * FROM Table2", actual[1]);
            Assert.AreEqual(2, actual.Count());
        }

        [Test]
        public void SplitInBatches_StringsShouldBeIgnored()
        {
            var somethingToSplit = @"SELECT * FROM Table WHERE Name = 'é du go LR?'";
            var actual = ScriptUtilities.SplitInBatches(somethingToSplit);
            Assert.AreEqual("SELECT * FROM Table WHERE Name = 'é du go ellä?'", actual[0]);
            Assert.AreEqual(1, actual.Count());
        }
    }
}
