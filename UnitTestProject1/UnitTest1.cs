using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using CachedRepository;
using Castle.Components.DictionaryAdapter.Xml;
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
            builder.RegisterType<CashService>().As<CashService>();
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

            // без кеша
            concreteRepository.Get("1");
            // уже закешировано
            concreteRepository.Get("1");

            // не закешированно, так как другие параметры
            concreteRepository.Get("2");
            // снова закешированно
            concreteRepository.Get("2");


            // очистили от сесии, работаем без кеша
            cashService.CurrentSession = new Session();
            concreteRepository.Get("1");
            // добавили кеширование,
            cashService.CurrentSession.Add<ConcreteRepository>(nameof(ConcreteRepository.Get));
            // попытка получания записывает в кеш
            concreteRepository.Get("1");
            // прочитали из кеша
            concreteRepository.Get("1");
        }
    }
}
