using AutoPrintr.Core.Services;
using NUnit.Framework;

namespace AutoPrintr.Tests
{
    [TestFixture]
    public class FileServiceTests
    {
        [Test]
        public void TrySetupAccessControl()
        {
            var service = new FileService();
            service.SetupAccessControl(@"Data/Settings.json");
        }
    }
}
