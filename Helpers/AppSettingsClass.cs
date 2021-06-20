using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NGCore_Blog.Helpers
{
    public class AppSettingsClass
    {
        //properties for jwt Signature
        //in order to acess the values in appsetting.json files you need to use hardcoded class
        public string Site { get; set; }
        public string Audience { get; set; }
        public string ExpireTime { get; set; }
        public string Secret { get; set; }
    }
}
