using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using slf4net;
using Moq;

namespace Ellucian.Web.Infrastructure.TestUtil
{
    public class MockLogger
    {
        // Singleton...
        private static Mock<ILogger> _instance;
        private static string _verifyMessage = "Logging was not invoked";

        public static Mock<ILogger> Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Mock<ILogger>();
                    Create();
                }
                return _instance;
            }
        }

        private MockLogger()
        {
        }

        private static void Create()
        {
            //Sally, I don't think we need to mock these boolean props - if code needs them they can mock them after getting this instance: e.g.
            // _loggerMock = MockLogger.Instance;
            // _loggerMock.SetupGet(x => x.IsDebugEnabled).Returns(false);
            //
            // just make a note in this class to this effect incase someone comes looking...

            //bool IsDebugEnabled { get; }
            //bool IsErrorEnabled { get; }
            //bool IsInfoEnabled { get; }
            //bool IsTraceEnabled { get; }
            //bool IsWarnEnabled { get; }

            //string Name { get; }

            _instance.Setup(x => x.Debug(It.IsAny<string>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Debug(It.IsAny<Exception>(), It.IsAny<string>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Debug(It.IsAny<string>(), It.IsAny<object[]>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Debug(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Debug(It.IsAny<IFormatProvider>(), It.IsAny<string>(), It.IsAny<object[]>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Debug(It.IsAny<Exception>(), It.IsAny<IFormatProvider>(), It.IsAny<string>(), It.IsAny<object[]>())).Verifiable(_verifyMessage);
            
            _instance.Setup(x => x.Error(It.IsAny<string>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Error(It.IsAny<Exception>(), It.IsAny<string>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Error(It.IsAny<string>(), It.IsAny<object[]>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Error(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Error(It.IsAny<IFormatProvider>(), It.IsAny<string>(), It.IsAny<object[]>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Error(It.IsAny<Exception>(), It.IsAny<IFormatProvider>(), It.IsAny<string>(), It.IsAny<object[]>())).Verifiable(_verifyMessage);
            
            _instance.Setup(x => x.Info(It.IsAny<string>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Info(It.IsAny<Exception>(), It.IsAny<string>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Info(It.IsAny<string>(), It.IsAny<object[]>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Info(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Info(It.IsAny<IFormatProvider>(), It.IsAny<string>(), It.IsAny<object[]>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Info(It.IsAny<Exception>(), It.IsAny<IFormatProvider>(), It.IsAny<string>(), It.IsAny<object[]>())).Verifiable(_verifyMessage);
            
            _instance.Setup(x => x.Trace(It.IsAny<string>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Trace(It.IsAny<Exception>(), It.IsAny<string>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Trace(It.IsAny<string>(), It.IsAny<object[]>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Trace(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Trace(It.IsAny<IFormatProvider>(), It.IsAny<string>(), It.IsAny<object[]>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Trace(It.IsAny<Exception>(), It.IsAny<IFormatProvider>(), It.IsAny<string>(), It.IsAny<object[]>())).Verifiable(_verifyMessage);
            
            _instance.Setup(x => x.Warn(It.IsAny<string>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Warn(It.IsAny<Exception>(), It.IsAny<string>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Warn(It.IsAny<string>(), It.IsAny<object[]>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Warn(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Warn(It.IsAny<IFormatProvider>(), It.IsAny<string>(), It.IsAny<object[]>())).Verifiable(_verifyMessage);
            _instance.Setup(x => x.Warn(It.IsAny<Exception>(), It.IsAny<IFormatProvider>(), It.IsAny<string>(), It.IsAny<object[]>())).Verifiable(_verifyMessage);
        }
    }
}
