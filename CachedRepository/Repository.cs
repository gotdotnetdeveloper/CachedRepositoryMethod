using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CachedRepository
{
    /// <summary>
    /// In-Memory Cached Repository. The main Idea - Cached Repository get-function by invoke signature.  (Like stored procedire in SQL-Server). Cached Repository mast be maximum independed by user-code in buisiness-logic laer.
    /// </summary>
    public class Repository : IRepository
    {
        private CashService _cashService;

        public Repository(CashService cashService )
        {
            _cashService = cashService;
        }

        /// <summary>
        /// Получение набора данных синхронно
        /// </summary>
        public bool TryGetFromCach<T>(object[] parameterList, out T o, string repositoryMethodName)
        where T:class
        {
            o = null;

            if (_cashService.CurrentSession != null)
            {

                var firstOrDefault = _cashService.CurrentSession.RepositoryMethodList.FirstOrDefault(x =>
                    x.MethodName == repositoryMethodName && x.RepositoryType == this.GetType());
                if (firstOrDefault != null)
                {
                    var hashCide = GetHashCodeByParameters(parameterList, repositoryMethodName);

                    if (!firstOrDefault.Cash.ContainsKey(hashCide))
                        return false;

                    o = (T) firstOrDefault.Cash[hashCide];
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Установить кеш
        /// </summary>
        /// <param name="firstOrDefault"></param>
        /// <param name="parameterList"></param>
        /// <param name="o"></param>
        /// <param name="repositoryMethodName"></param>
        /// <returns></returns>
        public void SetCach(RepositoryMethod firstOrDefault, object[] parameterList, object o,
            string repositoryMethodName)
        {
            var hashCide = GetHashCodeByParameters(parameterList, repositoryMethodName);
            firstOrDefault.Cash[hashCide] = o;
        }


        /// <summary>
        /// XOR по всем параметрам. 
        /// </summary>
        /// <param name="parameterList"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private int GetHashCodeByParameters(object[] parameterList, string name)
        {
            var tmp = string.IsNullOrEmpty(name) ? 54 : name.GetHashCode();

            if (parameterList == null || !parameterList.Any())
                return tmp;

            tmp ^= parameterList[0].GetHashCode();

            if (parameterList.Length > 1)
            {
                for (int i = 0; i < parameterList.Length; i++)
                {
                    tmp ^= parameterList[i].GetHashCode();
                }
            }
            return tmp;
        }

        /// <summary>
        /// Обработать асинхронный таск
        /// </summary>
        /// <param name="getTask"></param>
        /// <param name="parameter"></param>
        /// <param name="repositoryMethodName"></param>
        /// <returns></returns>
        public Task<T> GetCashedTask<T>(Func<Task<T>> getTask, object[] parameter,
            [CallerMemberName] string repositoryMethodName = null) where T : class
        {
            if (_cashService.CurrentSession != null)
            {

                var firstOrDefault = _cashService.CurrentSession.RepositoryMethodList.FirstOrDefault(x =>
                    x.MethodName == repositoryMethodName && x.RepositoryType == this.GetType());
                if (firstOrDefault != null)
                {
                    if (TryGetFromCach<T>(new object[] {parameter}, out var returnValue, repositoryMethodName))
                    {
                        return Task.FromResult<T>(returnValue);
                    }

                    var task = getTask.Invoke();

                    task.ContinueWith(t =>
                    {
                        SetCach(firstOrDefault, new object[] {parameter}, t.Result, repositoryMethodName);
                    });

                    return task;
                }
                else
                {
                    return getTask.Invoke();
                }

            }
            else
            {
                return getTask.Invoke();
            }

        }


        /// <summary>
        /// Обработать с кешированием функцию.
        /// </summary>
        /// <param name="getMetoDelegate"></param>
        /// <param name="parameter"></param>
        /// <param name="repositoryMethodName"></param>
        /// <returns></returns>
        public object GetCashed(Func<object[], object> getMetoDelegate, object[] parameter,
            [CallerMemberName] string repositoryMethodName = null)
        {

            if (_cashService.CurrentSession != null)
            {

                var firstOrDefault = _cashService.CurrentSession.RepositoryMethodList.FirstOrDefault(x =>
                    x.MethodName == repositoryMethodName && x.RepositoryType == this.GetType());
                if (firstOrDefault != null)
                {
                    object returnValue = null;
                    if (TryGetFromCach<object>(new[] {parameter}, out returnValue, repositoryMethodName))
                    {
                        return returnValue;
                    }

                    returnValue = getMetoDelegate.Invoke(parameter);
                    SetCach(firstOrDefault,new[] {parameter}, returnValue, repositoryMethodName);
                    return returnValue;
                }

                return getMetoDelegate.Invoke(parameter);
            }
            else
            {
                return getMetoDelegate.Invoke(parameter);
            }
        }
    }
}
