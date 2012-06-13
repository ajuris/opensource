using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Geta.Community.EntityAttributeBuilder.Tests
{
    [TestClass]
    public class PerfTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            int max = 1000;
            int printOnCount = (max / 100);

            Console.WriteLine("First {0}", GC.GetTotalMemory(false));

            for (int i = 0; i < max; i++)
            {
                var metadata = new SampleType(-1).AsAttributeExtendable<SampleAttributeMetadata>();
                if (i % printOnCount == 0)
                {
                    Console.WriteLine(GC.GetTotalMemory(false));
                }
            }

            Console.WriteLine("Last {0}", GC.GetTotalMemory(true));
        }
    }
}
