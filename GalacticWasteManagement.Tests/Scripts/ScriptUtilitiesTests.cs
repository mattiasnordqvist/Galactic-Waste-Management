using NUnit.Framework;
using Shouldly;
using System.Linq;

namespace GalacticWasteManagement.Tests.Scripts
{
    public class ScriptUtilitiesTests
    {
        [Test]
        public void SplitInBatches_HappyDays()
        {


            var somethingToSplit = @"SELECT * FROM Table1
              GO
              SELECT * FROM Table2";
            var actual = new SqlServer.MsSql120ScriptParser().SplitInBatches(somethingToSplit);
            Assert.AreEqual(2, actual.Count());
        }

        [Test]
        public void SplitInBatches_HappyDays2()
        {
            var somethingToSplit = @"SELECT * FROM Table1";
            var actual = new SqlServer.MsSql120ScriptParser().SplitInBatches(somethingToSplit);
            Assert.AreEqual(1, actual.Count());
        }

        [Test]
        public void SplitInBatches_SemicolonsDoesntFuckThingsUp()
        {
            var somethingToSplit = @"SELECT * FROM Table1;
              GO
              SELECT * FROM Table2";
            var actual = new SqlServer.MsSql120ScriptParser().SplitInBatches(somethingToSplit);
            Assert.AreEqual(2, actual.Count());
        }

        [Test]
        public void SplitInBatches_CaseInsensitiveness()
        {
            var somethingToSplit = @"SELECT * FROM Table1;
              gO;
              SELECT * FROM Table2";
            var actual = new SqlServer.MsSql120ScriptParser().SplitInBatches(somethingToSplit);
            Assert.AreEqual(2, actual.Count());
        }

        [Test]
        public void SplitInBatches_StringsShouldBeIgnored()
        {
            var somethingToSplit = @"SELECT * FROM Table1 WHERE Name = 'é du go LR?'";
            var actual = new SqlServer.MsSql120ScriptParser().SplitInBatches(somethingToSplit);
            Assert.AreEqual(1, actual.Count());
        }

        [Test]
        public void CreateOrAlterSyntax_IsAllowed_With_MsSql130ScriptParser()
        {
            var sql = @"CREATE OR ALTER VIEW [dbo].[CompanyNames] AS SELECT Company.Name FROM Company";
            Should.NotThrow(() =>new SqlServer.MsSql130ScriptParser().SplitInBatches(sql));
        }
    }
}
