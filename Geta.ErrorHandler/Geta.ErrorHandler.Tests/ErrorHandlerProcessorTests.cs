using System;
using System.Web;
using Moq;
using NUnit.Framework;

namespace Geta.ErrorHandler.Tests
{
    [TestFixture]
    public class ErrorHandlerProcessorTests
    {
        [Test]
        public void IsJpgFileUrlResourceFile_IfRequestedWithIISPath_Test()
        {
            var testUrl = new Uri("http://localhost/app/hanlder.aspx?404;http://localhost/app/image.jpg");
            var contextMock = new Mock<HttpContextBase>();
            var requestMock = new Mock<HttpRequestBase>();

            contextMock.Setup(c => c.Request).Returns(requestMock.Object);
            requestMock.Setup(r => r.Url).Returns(testUrl);

            var processor = new ErrorHandlerProcessor(contextMock.Object);
            var isResourceFile = processor.IsResourceFile(testUrl);

            Assert.AreEqual(true, isResourceFile, "File `" + testUrl + "' is not a resource file.");
        }

        [Test]
        public void IsJpgFileUrlResourceFile_IfRequestedDirectly_Test()
        {
            var testUrl = new Uri("http://localhost/app/image.jpg");
            var contextMock = new Mock<HttpContextBase>();
            var requestMock = new Mock<HttpRequestBase>();

            contextMock.Setup(c => c.Request).Returns(requestMock.Object);
            requestMock.Setup(r => r.Url).Returns(testUrl);

            var processor = new ErrorHandlerProcessor(contextMock.Object);
            var isResourceFile = processor.IsResourceFile(testUrl);

            Assert.AreEqual(true, isResourceFile, "File `" + testUrl + "' is not a resource file.");
        }
    }
}
