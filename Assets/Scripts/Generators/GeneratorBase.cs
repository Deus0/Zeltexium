using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex
{
    public class GeneratorBase : ManagerBase<GeneratorBase>
    {
        /// <summary>
        /// Generates content in a certain folder
        /// </summary>
        public virtual IEnumerator Generate()
        {

            yield break;
        }
    }
}
