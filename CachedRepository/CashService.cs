using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CachedRepository
{
    /// <summary>
    /// Сервис кеширования.
    /// </summary>
  public  class CashService
    {
        /// <summary>
        /// Текущая сессия 
        /// </summary>
        public Session CurrentSession{ get; set; }

        /// <summary>
        /// Получение набора данных синхронно
        /// </summary>
        public bool TryGetFromCach<T>(object[] parameterList, out T o, string repositoryMethodName, Type repositoryType)
        where T : class
        {
            o = null;

            if (CurrentSession != null)
            {
                var firstOrDefault = CurrentSession.RepositoryMethodList.FirstOrDefault(x =>
                    x.MethodName == repositoryMethodName && x.RepositoryType == repositoryType);
                if (firstOrDefault != null)
                {
                    var hashCode = GetHashCodeByParameters(parameterList, repositoryMethodName);
                    if (!firstOrDefault.Cash.ContainsKey(hashCode))
                    {
                        Debug.WriteLine($"не найден ключ {hashCode} для отслеживаниея метода {repositoryMethodName} типа {repositoryType}");
                        return false;
                    }

                    o = (T)firstOrDefault.Cash[hashCode];
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
        /// <param name="repository"></param>
        /// <param name="repositoryMethodName"></param>
        /// <returns></returns>
        public Task<T> GetCashedTask<T>(Func<Task<T>> getTask, object[] parameter, IRepository repository,
            [CallerMemberName] string repositoryMethodName = null) where T : class
        {
            if (CurrentSession != null)
            {
                Type repositoryType = repository.GetType();
                var firstOrDefault = CurrentSession.RepositoryMethodList.FirstOrDefault(x =>
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
        /// <param name="repository"></param>
        /// <param name="repositoryMethodName"></param>
        /// <returns></returns>
        public object GetCashed(Func<object[], object> getMetoDelegate, object[] parameter, IRepository repository,
            [CallerMemberName] string repositoryMethodName = null)
        {
            if (CurrentSession != null)
            {
                Type repositoryType = repository.GetType();

                var firstOrDefault = CurrentSession.RepositoryMethodList.FirstOrDefault(x => x.MethodName == repositoryMethodName && x.RepositoryType == repositoryType);
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


    /// <summary>
    /// Идентификатор кешированного объекта. 
    /// </summary>
   public struct CashId
    {
        public Guid ObjectId { get; set; }
       // public Guid SessionId { get; set; }
        public Type ObjectType { get; set; }
    }

    /// <summary>
    /// Сессия.
    /// </summary>
    public class Session
    {
        /// <summary>
        /// Идентификатор сессии.
        /// </summary>
        public Guid? CurrentSessionId { get; set; }
        
        /// <summary>
        /// Набор кешированных методов в репозитори (методы, которые будут отслеживатся для кеширования)
        /// </summary>
       public HashSet<RepositoryMethod> RepositoryMethodList = new HashSet<RepositoryMethod>();
        
        /// <summary>
        /// Добавить метод для его отслеживания в кеше.
        /// </summary>
        /// <typeparam name="T">Репозиторий</typeparam>
        /// <param name="methodName"></param>
        public void Add<T>( string methodName) where T: IRepository
        {
            var rm = new RepositoryMethod(methodName, typeof(T));
            
            if(!RepositoryMethodList.Contains(rm))
                RepositoryMethodList.Add(rm);
        }

     
    }

    /// <summary>
    /// Описание метода в репозитории
    /// </summary>
    public class RepositoryMethod
    {
        private string _methodName;
        private Type _repositoryType;
        private ConcurrentDictionary<int, object> _cash;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="repositoryType"></param>
        public RepositoryMethod(string methodName, Type repositoryType)
        {
            _methodName =  methodName;
            _repositoryType = repositoryType;
        }
        /// <summary>
        /// Кешированные результаты.
        /// </summary>
        public ConcurrentDictionary<int, object> Cash => _cash ?? (_cash = new ConcurrentDictionary<int, object>());

  
        /// <summary>
        /// Название метода исполнения
        /// </summary>
        public string MethodName
        {
            get => _methodName;
            set => _methodName = value;
        }
        /// <summary>
        /// Тип репозитория.
        /// </summary>
        public Type RepositoryType
        {
            get => _repositoryType;
            set => _repositoryType = value;
        }

        public override int GetHashCode()
        {
            int hash = string.IsNullOrEmpty(MethodName) ? 0 : MethodName.GetHashCode();

            if (RepositoryType != null)
            {
                hash ^= RepositoryType.GetHashCode();
            }
            return hash;
        }

        public override bool Equals(object obj)
        {
            var repositoryMethod = obj as RepositoryMethod;
            if (repositoryMethod == null)
                return false;

            return (repositoryMethod.RepositoryType == this.RepositoryType) 
                   && (string.Compare(repositoryMethod.MethodName, this.MethodName
                           ,CultureInfo.InvariantCulture, CompareOptions.None) == 0 ) ;
        }
    }


}
