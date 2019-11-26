using System;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using CachedRepository;
using CommonServiceLocator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ConcreteRepository>().As<ConcreteRepository>();
            var container = builder.Build();
            var csl = new AutofacServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => csl);


            var concreteRepository = csl.GetInstance<ConcreteRepository>();
            var value1 =  concreteRepository.Get("A");
            var value2 =  concreteRepository.Get("A");



            /*
             */



        }
    }
}
