﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CachedRepository
{
    /// <summary>
    /// Concrete Repository for Test
    /// </summary>
    public class ConcreteRepository : Repository 
    {
        public object GetCashed(Func<object[], object> getMetoDelegate, 
            object[] parameter, [CallerMemberName] string repositoryMethodName = null)
        {
            object returnValue = null;

            if (TryGetFromCach(new[] {parameter}, out returnValue, repositoryMethodName))
            {
                return returnValue;
            }

            returnValue = getMetoDelegate.Invoke(parameter);
            SetCach(new[] {parameter}, returnValue, repositoryMethodName);
            return returnValue;
        }

        public Task<object> GetCashedTask(Func<object[], object> getMetoDelegate,
            object[] parameter, [CallerMemberName] string repositoryMethodName = null)
        {
            object returnValue = null;

            if (TryGetFromCach(new[] { parameter }, out returnValue, repositoryMethodName))
            {
                return returnValue;
            }

            returnValue = getMetoDelegate.Invoke(parameter);

            SetCach(new[] { parameter }, returnValue, repositoryMethodName);
            return returnValue;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Model> Get( string parameter)
        {
            return GetCashed(arg =>
            {
                #region Получение данных из БД

                if (string.IsNullOrEmpty(parameter))
                    return new List<Model>();

                var newItem = new List<Model>();
                int maxCount = 10;
                int i = 0;
                while (i < maxCount)
                {
                    i++;
                    newItem.Add(new Model() { Id = Guid.NewGuid() });
                }

                #endregion
                return newItem;
            }, new object[] {parameter}) as IEnumerable<Model>;

        }





        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<Model> GetAsinc(string parameter)
        {
            return GetCashed(arg =>
            {
                #region Получение данных из БД

                if (string.IsNullOrEmpty(parameter))
                    return new List<Model>();

                var newItem = new List<Model>();
                int maxCount = 10;
                int i = 0;
                while (i < maxCount)
                {
                    i++;
                    newItem.Add(new Model() { Id = Guid.NewGuid() });
                }

                #endregion
                return newItem;
            }, new object[] { parameter }) as IEnumerable<Model>;

        }
    }
}