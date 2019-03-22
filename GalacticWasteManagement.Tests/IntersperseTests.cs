using System.Collections.Generic;
using GalacticWasteManagement.Utilities;
using NUnit.Framework;

namespace GalacticWasteManagement.Tests
{
    
    public class IntersperseTests
    {
        [Test]
        public void HappyDays()
        {
            var list = new List<int>{ 1, 2, 3, 4 };
            var interspersed = list.Intersperse(0);
            CollectionAssert.AreEquivalent(new List<int>{ 1, 0, 2, 0, 3, 0, 4 }, interspersed);
        }
    }
}