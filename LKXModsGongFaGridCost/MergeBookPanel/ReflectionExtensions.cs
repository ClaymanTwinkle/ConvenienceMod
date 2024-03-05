using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceFrontend.MergeBookPanel
{
    public static class ReflectionExtensions
    {
        // Token: 0x06000068 RID: 104 RVA: 0x00006BB0 File Offset: 0x00004DB0
        public static T GetFieldValue<T>(this object instance, string fieldname)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo field = instance.GetType().GetField(fieldname, bindingFlags);
            return (T)((object)((field != null) ? field.GetValue(instance) : null));
        }

        // Token: 0x06000069 RID: 105 RVA: 0x00006BE0 File Offset: 0x00004DE0
        public static void SetPrivateField(this object instance, string fieldname, object value)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            instance.GetType().GetField(fieldname, bindingFlags).SetValue(instance, value);
        }

        // Token: 0x0600006A RID: 106 RVA: 0x00006C04 File Offset: 0x00004E04
        public static T CallPrivateMethod<T>(this object instance, string methodname, params object[] param)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            return (T)((object)instance.GetType().GetMethod(methodname, bindingFlags).Invoke(instance, param));
        }

        // Token: 0x0600006B RID: 107 RVA: 0x00006C30 File Offset: 0x00004E30
        public static void CallPrivateMethod(this object instance, string methodname, params object[] param)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            instance.GetType().GetMethod(methodname, bindingFlags).Invoke(instance, param);
        }
    }
}
