using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetSyncLib.Impl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NetSyncLib.Tests.PerformanceTests
{
    [TestClass]
    public class PerformanceTestClass
    {
        private static int num = 1000000;
        [TestMethod]
        public void TestDoubleLinkedDictionary()
        {
            DLD<int, int> dLD = new DLD<int, int>();
            Stopwatch stopwatch = new Stopwatch();
            //add test
            Console.Write("Adding: ");
            stopwatch.Start();
            for (int i = 0; i < num; i++)
            {
                dLD.Add(i, i);
            }
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Console.Write("GetVal: ");
            stopwatch.Restart();
            //get val test
            for (int i = 0; i < num; i++)
            {
                int val = dLD.GetValue(i);
            }
            //get key test
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Console.Write("GetKey: ");
            stopwatch.Restart();
            for (int i = 0; i < num; i++)
            {
                int key = dLD.GetKey(i);
            }
            //remove test
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Console.Write("RemoveByKey: ");
            stopwatch.Restart();
            for (int i = 0; i < num; i++)
            {
                dLD.Remove(i, MapperMode.byKey);
            }
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            stopwatch.Stop();
        }
        [TestMethod]
        public void TestDictionary()
        {
            Dictionary<int, int> dLD = new Dictionary<int, int>();
            Stopwatch stopwatch = new Stopwatch();
            //add test
            Console.Write("Adding: ");
            stopwatch.Start();
            for (int i = 0; i < num; i++)
            {
                dLD.Add(i, i);
            }
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Console.Write("GetVal: ");
            stopwatch.Restart();
            //get val test
            for (int i = 0; i < num; i++)
            {
                int val = dLD[i];
            }
            //get key test
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Console.Write("RemoveByKey: ");
            stopwatch.Restart();
            for (int i = 0; i < num; i++)
            {
                dLD.Remove(i);
            }
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            stopwatch.Stop();
        }
        [TestMethod]
        public void TestCallerMethodReflection()
        {
            EmptyNetObject emptyNet = new EmptyNetObject();
            for (int i = 0; i < num; i++)
            {
                if (!InitializeNetObjectTest(emptyNet)) i++;

            }

        }

        private bool RefNameTest([System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {

            return memberName.Equals(".ctor");
            //return new StackTrace().GetFrame(1).GetMethod().IsConstructor;
        }
        public static bool InitializeNetObjectTest<T>(T obj, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            if (memberName.Equals(".ctor"))
            {                
                if (obj.GetType() == typeof(T))
                { return true; }
                return false;
            }
            return true;
        }
    }
}
