using System.Collections.Generic;
using Xunit;
using GalacticWasteManagement.Utilities;

namespace GalacticWasteManagement.Tests
{
    public class IntersperseTests
    {
        [Fact]
        public void HappyDays()
        {
            var list = new List<int>{ 1, 2, 3, 4 };
            var interspersed = list.Intersperse(0);
            Assert.Equal(new List<int>{ 1, 0, 2, 0, 3, 0, 4 }, interspersed);
        }
    }
}
