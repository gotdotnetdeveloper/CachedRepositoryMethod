using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;

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
        
        
        
        
        
        
        
        
        
        
        /* Создать сессию текущую  */

        //Dictionary<CashId,object> _entryDictionary = new Dictionary<CashId, object>();
        //void AddCash(CashId cashId, object o)
        //{
        //   _entryDictionary.Add(cashId,o);
        //}
        //object GetCash(CashId cashId)
        //{
        //   return _entryDictionary[cashId];
        //}
        //T GetCash<T>(CashId cashId)
        //{
        //    return (T)_entryDictionary[cashId];
        //}

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
