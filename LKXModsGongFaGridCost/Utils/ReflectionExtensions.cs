using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GameData.Utilities;

namespace ConvenienceFrontend.Utils
{
    public static class ReflectionExtensions
    {
        public static T GetStaticFieldValue<T>(this Type type, string fieldname)
        {
            BindingFlags bindingAttr = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo field = type.GetField(fieldname, bindingAttr);
            return (T)((object)((field != null) ? field.GetValue(null) : null));
        }

        public static T GetFieldValue<T>(this object instance, string fieldname)
        {
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo field = instance.GetType().GetField(fieldname, bindingAttr);
            return (T)((object)((field != null) ? field.GetValue(instance) : null));
        }

        public static void SetPrivateField(this object instance, string fieldname, object value)
        {
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            instance.GetType().GetField(fieldname, bindingAttr).SetValue(instance, value);
        }

        public static T CallPrivateMethod<T>(this object instance, string methodname, params object[] param)
        {
            AdaptableLog.Info("CallPrivateMethod: " + methodname);
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            return (T)((object)instance.GetType().GetMethod(methodname, bindingAttr).Invoke(instance, param));
        }

        public static T CallPrivateMethod<T>(this object instance, string methodname, Type[] types, params object[] param)
        {
            AdaptableLog.Info("CallPrivateMethod: " + methodname);
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            return (T)((object)instance.GetType().GetMethod(methodname, bindingAttr, null, CallingConventions.Any, types, null).Invoke(instance, param));
        }

        public static void CallPrivateMethod(this object instance, string methodname, params object[] param)
        {
            AdaptableLog.Info("CallPrivateMethod: "+methodname);
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            instance.GetType().GetMethod(methodname, bindingAttr).Invoke(instance, param);
        }
    }
}
