using AutoFixture;
using GalacticWasteManagement.Utilities;
using NUnit.Framework;

namespace GalacticWasteManagement.Tests
{
    public class HashingTests
    {
        [Test, AutoFakeItEasyData]
        public void TestingDeterministicBehaviour(string somethingToHash)
        {
            var f = new Fixture();
            var result1 = Hashing.CreateHash(somethingToHash);
            var result2 = Hashing.CreateHash(somethingToHash);
            Assert.AreEqual(result1, result2);
        }

        [Test, AutoFakeItEasyData]
        public void TestingHashesAreUnique(string somethingToHash, string somethingElseToHash)
        {
            var f = new Fixture();
            var result1 = Hashing.CreateHash(somethingToHash);
            var result2 = Hashing.CreateHash(somethingElseToHash);
            Assert.AreNotEqual(result1, result2);
        }
    }
}