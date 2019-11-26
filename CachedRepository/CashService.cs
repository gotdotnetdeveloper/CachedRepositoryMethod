using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachedRepository
{
    /// <summary>
    /// 
    /// </summary>
  public  class CashService
    {
        Dictionary<CashId,object> _entryDictionary = new Dictionary<CashId, object>();

        void AddCash(CashId cashId, object o)
        {
           _entryDictionary.Add(cashId,o);
        }

        object GetCash(CashId cashId)
        {
           return _entryDictionary[cashId];
        }

        T GetCash<T>(CashId cashId)
        {
            return (T)_entryDictionary[cashId];
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
    

}
