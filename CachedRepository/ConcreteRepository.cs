using System;
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
       
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="cashService"></param>
        public ConcreteRepository(CashService cashService) : base(cashService)
        {
        }


        /// <summary>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Model> Get(string parameter)
        {
            return _cashService.GetCashed(arg =>
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
            }, new object[] { parameter },this) as IEnumerable<Model>;
        }


        public Task<Model> GetAsincCashed(string parameter)
        {
            var x= _cashService.GetCashedTask<Model>(
                () =>
                {
                    var tmp = parameter;
                    //TODO some task
                     Task<Model> t = new Task<Model>(wrewew);
                     return t;
                }
                , new object[] {parameter},this);

            return x;
        }


        private Model wrewew()
        {
            throw new NotImplementedException();
        }


    }
}