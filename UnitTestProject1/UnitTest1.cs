using System.Diagnostics;
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
            builder.RegisterType<ConcreteRepository>().As<ConcreteRepository>().SingleInstance(); 
            builder.RegisterType<CashService>().As<CashService>().SingleInstance(); 

            var container = builder.Build();
            var csl = new AutofacServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => csl);



            var concreteRepository = csl.GetInstance<ConcreteRepository>();
            var cashService = csl.GetInstance<CashService>();

            

            /*
             выбрать какие именно методы будут кешироватся.
             */
            cashService.CurrentSession = new Session();
            cashService.CurrentSession.Add<ConcreteRepository>( nameof(ConcreteRepository.Get) );


            Debug.WriteLine("без кеша"); 
            concreteRepository.Get("1");

            Debug.WriteLine("Второй раз - по тому же параметру -уже должно быть закешировано");
            concreteRepository.Get("1");

            Debug.WriteLine("не закешированно, так как другие параметры");
            concreteRepository.Get("2");

            Debug.WriteLine("снова закешированно");
            concreteRepository.Get("2");

            Debug.WriteLine("очистили от сесии, работаем без кеша");
            cashService.CurrentSession = new Session();
            concreteRepository.Get("1");

            Debug.WriteLine("добавили кеширование");
            cashService.CurrentSession.Add<ConcreteRepository>(nameof(ConcreteRepository.Get));

            
            Debug.WriteLine("попытка получания записывает в кеш");
            concreteRepository.Get("1");

            Debug.WriteLine("прочитали из кеша");
            concreteRepository.Get("1");
        }
    }
}
