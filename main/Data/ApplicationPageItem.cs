using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCore.Data
{
    public class ApplicationPageItem
    {
        /// <summary>
        /// Caption of page
        /// </summary>
        public string ApplicationPageName { get; set; }

        /// <summary>
        /// Icon kind property of PackIcon used in content
        /// </summary>
        public string IconName { get; set; }

        /// <summary>
        /// Id of a page
        /// </summary>
        public int Id { get; set; }
    } 
}
