using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlockchainApp.Source.Common.Utils.TypeUtils
{
    public class Assertutils
    {
        public static void ThrowsExceptionWithMessage(Action func, string message)
        {
            var exceptionThrown = false;
            var actualMessage = "";

            try
            {
                func.Invoke();
            }
            catch (Exception ex)
            {
                exceptionThrown = true;
                actualMessage = ex.Message;
            }

            if (!exceptionThrown)
                throw new AssertFailedException($"An exception with message '{message}' was expected, but not thrown");
            if (actualMessage != message)
                throw new AssertFailedException($"An exception with message '{message}' was expected, but the says '{actualMessage}' instead");
        }
    }
}
