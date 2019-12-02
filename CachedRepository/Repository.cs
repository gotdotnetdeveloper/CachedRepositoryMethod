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
        protected readonly CashService _cashService;

        public Repository(CashService cashService )
        {
            _cashService = cashService;
        }

   
    }
}
