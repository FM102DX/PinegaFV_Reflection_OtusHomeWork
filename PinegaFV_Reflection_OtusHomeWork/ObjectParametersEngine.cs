using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PinegaFV_Reflection_OtusHomeWork
{
    public static class ObjectParameters
    {
        //дело в том, что у объекта есть field и property, и все его поля данных - они либо такие, либо такие
        //часть данных объекта храняться как филд, часть - как проперти
        //чтобы не перебирать филд и проепрти каждый раз в коде, делается objectParameter внутри которого и перебирается филд и проперти

        public static void SetObjectParameter(object x, string name, object value)
        {

            FieldInfo[] newObjectFields = x.GetType().GetFields();
            PropertyInfo[] newObjectProperties = x.GetType().GetProperties();

            foreach (FieldInfo f0 in newObjectFields)
            {

                if (f0.Name.ToUpperInvariant() == name.ToUpperInvariant())
                {
                    try
                    {
                        value = ConvertedObjectValue(f0.FieldType.ToString(), value); //чтобы внутри object было значение нужно типа
                        f0.SetValue(x, value);
                    }
                    catch
                    {

                    }
                }
            }

            foreach (PropertyInfo f1 in newObjectProperties)
            {
                if (f1.Name.ToUpperInvariant() == name.ToUpperInvariant())
                {
                    if (IsItOnlyGetter(x, name)) return;
                    try
                    {
                        value = ConvertedObjectValue(f1.PropertyType.ToString(), value);
                        f1.SetValue(x, value);
                    }
                    catch
                    {

                    }
                }
            }
        }
        public static ObjectParameter GetObjectParameterByName(object x, string name)
        {
            ObjectParameter op = new ObjectParameter();
            op.name = name;

            FieldInfo[] newObjectFields = x.GetType().GetFields();
            PropertyInfo[] newObjectProperties = x.GetType().GetProperties();

            foreach (FieldInfo f0 in newObjectFields)
            {

                if (f0.Name.ToUpperInvariant() == name.ToUpperInvariant())
                {
                    op.value = f0.GetValue(x);
                    return op;
                }
            }

            foreach (PropertyInfo f1 in newObjectProperties)
            {
                if (f1.Name.ToUpperInvariant() == name.ToUpperInvariant())
                {
                    op.value = f1.GetValue(x);
                    return op;
                }
            }
            return null;
        }
        public static List<ObjectParameter> GetObjectParameters(object x)
        {
            List<ObjectParameter> rez = new List<ObjectParameter>();

            FieldInfo[] newObjectFields = x.GetType().GetFields();
            PropertyInfo[] newObjectProperties = x.GetType().GetProperties();

            // 10/12/2020 - таким образом, экземпляр класса T, созданный в дженерике в рантайме, это экземпляр наследника, а не самого Т
            // их можно достать через fieldInfo и SetValue

            foreach (FieldInfo f0 in newObjectFields)  //это поля реального объекта, который только что был создан и будет добавлен в коллекцию
            {
                ObjectParameter op = new ObjectParameter();
                op.name = f0.Name;
                op.value = f0.GetValue(x);
                rez.Add(op);

            }

            foreach (PropertyInfo f1 in newObjectProperties)  //это поля реального объекта, который только что был создан и будет добавлен в коллекцию
            {
                ObjectParameter op = new ObjectParameter();
                op.name = f1.Name;
                op.value = f1.GetValue(x);
                rez.Add(op);
            }

            return rez;
        }

        public class ObjectParameter
        {
            public string name;
            public object value;
            public string typeStr;
        }

        public static bool IsItOnlyGetter(object x, string fieldName)
        {
            PropertyInfo[] newObjectProperties = x.GetType().GetProperties();

            foreach (PropertyInfo f1 in newObjectProperties)
            {
                if (f1.Name.ToUpperInvariant() == fieldName.ToUpperInvariant())
                {
                    //вот он нашел это поле
                    return (f1.CanRead && (!f1.CanWrite));
                }
            }
            return false;

        }

        public static object ConvertedObjectValue(string typeStr, object value)
        {
            //возвращает object - обертку исходя из того, какой тип передан в typeStr

            if (value == null) return null;

            if (value.GetType().ToString() == typeStr) { return value; }

            object rez = null;

            string _typeVar="";

            if (typeStr == typeof(System.String).FullName) _typeVar = "String";
            if (typeStr == typeof(System.Decimal).FullName || typeStr == typeof(System.Double).FullName) _typeVar = "DoubleDecimal";
            if (typeStr == typeof(System.Int16).FullName || typeStr == typeof(System.Int32).FullName) _typeVar = "Int";
            if (typeStr == typeof(System.Boolean).FullName) _typeVar = "Boolean";
            if (typeStr == typeof(System.DateTime).FullName) _typeVar = "DateTime";

            try
            {
                switch (_typeVar)
                {
                    case "String":
                        rez = Convert.ToString(value);
                        break;

                    case "DoubleDecimal":
                        rez = Convert.ToDouble(value);
                        break;

                    case "Int":
                        rez = Convert.ToInt32(value);
                        break;

                    case "Boolean":
                        rez = Convert.ToBoolean(value);
                        break;

                    case "DateTime":
                        rez = Convert.ToDateTime(value);
                        break;

                    default:
                        rez = value;
                        break;
                }
            }
            catch
            {
                //конвертация неуспешна
                return null;
            }
            return rez;
        }


    }
}
