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
        Hashtable hash = new Hashtable();

        /// <summary>
        /// </summary>
        public bool TryGetFromCach( object[] parameterList , out object o ,  string repositoryMethodName )
        {
            o = null;

            var hashCide = GetHashCodeByParameters(parameterList, repositoryMethodName);

            if (!hash.ContainsKey(hashCide))
                return false;
         
            o = hash[hashCide];
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameterList"></param>
        /// <param name="o"></param>
        /// <param name="repositoryMethodName"></param>
        /// <returns></returns>
        public void SetCach(object[] parameterList, object o,  string repositoryMethodName )
        {
            var hashCide = GetHashCodeByParameters(parameterList, repositoryMethodName);

             hash[hashCide] = o;
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


    }
}
