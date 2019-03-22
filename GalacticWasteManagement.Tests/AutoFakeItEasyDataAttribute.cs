using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoFixture.NUnit3;

namespace GalacticWasteManagement.Tests
{
    public class AutoFakeItEasyDataAttribute : AutoDataAttribute
    {
        public AutoFakeItEasyDataAttribute()
            : base(() => new Fixture().Customize(new AutoFakeItEasyCustomization()))
        {
        }
    }
}