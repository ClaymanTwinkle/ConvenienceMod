using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceBackend.MergeBookPanel
{
    public static class ReflectionExtensions
    {
        // Token: 0x0600000A RID: 10 RVA: 0x00002D00 File Offset: 0x00000F00
        public static T GetFieldValue<T>(this object instance, string fieldname)
        {
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo field = instance.GetType().GetField(fieldname, bindingAttr);
            return (T)((object)((field != null) ? field.GetValue(instance) : null));
        }

        // Token: 0x0600000B RID: 11 RVA: 0x00002D30 File Offset: 0x00000F30
        public static void SetPrivateField(this object instance, string fieldname, object value)
        {
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            instance.GetType().GetField(fieldname, bindingAttr).SetValue(instance, value);
        }

        // Token: 0x0600000C RID: 12 RVA: 0x00002D54 File Offset: 0x00000F54
        public static T CallPrivateMethod<T>(this object instance, string methodname, params object[] param)
        {
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            return (T)((object)instance.GetType().GetMethod(methodname, bindingAttr).Invoke(instance, param));
        }

        // Token: 0x0600000D RID: 13 RVA: 0x00002D80 File Offset: 0x00000F80
        public static void CallPrivateMethod(this object instance, string methodname, params object[] param)
        {
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            instance.GetType().GetMethod(methodname, bindingAttr).Invoke(instance, param);
        }
    }
}
