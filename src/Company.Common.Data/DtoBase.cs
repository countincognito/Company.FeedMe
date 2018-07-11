using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Company.Common.Data
{
    [Serializable]
    public abstract class DtoBase<TKey>
        where TKey : struct
    {
        public DtoBase(TKey id)
        {
            Id = id;
        }

        public TKey Id
        {
            get;
        }

        public static byte[] Serialize<T>(T obj) where T : class
        {
            Debug.Assert(IsDataContract(typeof(T)) || typeof(T).IsSerializable);
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            string json = JsonConvert.SerializeObject(obj);
            return Encoding.UTF8.GetBytes(json);
        }

        public static T DeSerialize<T>(byte[] array) where T : class
        {
            Debug.Assert(IsDataContract(typeof(T)) || typeof(T).IsSerializable);
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            string json = Encoding.UTF8.GetString(array);
            return JsonConvert.DeserializeObject<T>(json);
        }

        private static bool IsDataContract(Type type)
        {
            object[] attributes = type.GetCustomAttributes(typeof(DataContractAttribute), false);
            return attributes.Any();
        }
    }
}
