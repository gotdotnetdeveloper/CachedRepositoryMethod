using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CachedRepository
{
    /// <summary>
    /// In-Memory Cached Repository. The main Idea - Cached Repository get-function by invoke signature.  (Like stored procedire in SQL-Server). Cached Repository mast be maximum independed by user-code in buisiness-logic laer.
    /// </summary>
    public class Repository : IRepository
    {
        private readonly CashService _cashService;

        public Repository(CashService cashService )
        {
            _cashService = cashService;
        }

        /// <summary>
        /// Получение набора данных синхронно
        /// </summary>
        public bool TryGetFromCach<T>(object[] parameterList, out T o, string repositoryMethodName, Type repositoryType)
        where T:class
        {
            o = null;

            if (_cashService.CurrentSession != null)
            {
                var firstOrDefault = _cashService.CurrentSession.RepositoryMethodList.FirstOrDefault(x =>
                    x.MethodName == repositoryMethodName && x.RepositoryType == repositoryType);
                if (firstOrDefault != null)
                {
                    var hashCode = GetHashCodeByParameters(parameterList, repositoryMethodName);
                    if (!firstOrDefault.Cash.ContainsKey(hashCode))
                    {
                        Debug.WriteLine($"не найден ключ {hashCode} для отслеживаниея метода {repositoryMethodName} типа {repositoryType}");
                        return false;
                    }
                        
                    o = (T) firstOrDefault.Cash[hashCode];
#if DEBUG

                    Debug.WriteLine($"Получили из кеша. hashCode = {hashCode} с параметрами { String.Join(", ", parameterList.Select(c => c.ToString()))} результат из кеша {o}");
#endif


                    return true;
                }
            }

            Debug.WriteLine($"сессия не инициализированна {repositoryMethodName} типа {repositoryType}");
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

            Debug.WriteLine($"Установлен новый кеш. Метод {repositoryMethodName}, Хешкод параметров = {hashCide},  Параметры { String.Join(", ", parameterList.Select(c => c.ToString()))} результат:{o}");

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

            if (parameterList == null || parameterList.Length == 0)
                return tmp;

            tmp ^= parameterList.GetValue(0).GetHashCode();

            if (parameterList.Length > 1)
            {
                for (int i = 0; i < parameterList.Length; i++)
                {
                    tmp ^= parameterList[i].GetHashCode();
                }
            }

#if DEBUG

            Debug.WriteLine($"HashCode={tmp} На основании параметров { String.Join(", ", parameterList.Select(c => c.ToString()))}, имя={name}");
#endif
            return tmp;
        }

        /// <summary>
        /// Обработать асинхронный таск
        /// </summary>
        /// <param name="getTask"></param>
        /// <param name="parameter"></param>
        /// <param name="repositoryType"></param>
        /// <param name="repositoryMethodName"></param>
        /// <returns></returns>
        public Task<T> GetCashedTask<T>(Func<Task<T>> getTask, object[] parameter, Type repositoryType,
            [CallerMemberName] string repositoryMethodName = null) where T : class
        {
            if (_cashService.CurrentSession != null)
            {

                var firstOrDefault = _cashService.CurrentSession.RepositoryMethodList.FirstOrDefault(x =>
                    x.MethodName == repositoryMethodName && x.RepositoryType == repositoryType);
                if (firstOrDefault != null)
                {
                    if (TryGetFromCach<T>(parameter, out var returnValue, repositoryMethodName, repositoryType))
                    {
                        return Task.FromResult(returnValue);
                    }

                    var task = getTask.Invoke();

                    task.ContinueWith(t =>
                    {
                        SetCach(firstOrDefault, parameter, t.Result, repositoryMethodName);
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
        /// Обработать с кешированием метод.
        /// </summary>
        /// <param name="getMetoDelegate"></param>
        /// <param name="parameter"></param>
        /// <param name="repositoryType"></param>
        /// <param name="repositoryMethodName"></param>
        /// <returns></returns>
        public object GetCashed(Func<object[], object> getMetoDelegate, object[] parameter, Type repositoryType,
            [CallerMemberName] string repositoryMethodName = null)
        {
            if (_cashService.CurrentSession != null)
            {
                var firstOrDefault = _cashService.CurrentSession.RepositoryMethodList.FirstOrDefault(x => x.MethodName == repositoryMethodName && x.RepositoryType == repositoryType);
                if (firstOrDefault != null)
                {
                    if (TryGetFromCach(parameter, out object returnValue, repositoryMethodName, repositoryType))
                    {
                        
                        return returnValue;
                    }
                    returnValue = getMetoDelegate.Invoke(parameter);
                    SetCach(firstOrDefault, parameter, returnValue, repositoryMethodName);

                    
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
