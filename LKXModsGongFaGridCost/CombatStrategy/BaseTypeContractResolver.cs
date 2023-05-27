using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace ConvenienceFrontend.CombatStrategy
{
    public class BaseTypeContractResolver : DefaultContractResolver
    {
        // Token: 0x06000018 RID: 24 RVA: 0x00002EDC File Offset: 0x000010DC
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type.BaseType, memberSerialization);
        }

        // Token: 0x0400002F RID: 47
        public static BaseTypeContractResolver Instance = new BaseTypeContractResolver();
    }
}
