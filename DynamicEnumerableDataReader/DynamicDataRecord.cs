using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Data;

namespace ModjeskiNet.Data
{
    public class DynamicDataRecord
        : DynamicObject, IDataRecord
    {
        protected IDataReader Reader { get; private set; }

        public DynamicDataRecord(IDataReader reader)
        {
            Reader = reader;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            for (var i = 0; i < Reader.FieldCount; i++)
            {
                yield return Reader.GetName(i);
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            try
            {
                var index = Reader.GetOrdinal(binder.Name);
                if (Reader.IsDBNull(index))
                {
                    if (binder.ReturnType.IsValueType)
                        result = Activator.CreateInstance(binder.ReturnType);
                }
                else
                {
                    var value = Reader.GetValue(index);
                    if (value.GetType() == binder.ReturnType)
                        result = value;
                    else
                        result = Convert.ChangeType(value, binder.ReturnType);
                }

                return true;
            }
            catch(Exception)
            {
                result = null;
                return false;
            }
        }

        // implement IDataRecord to make this class
        // appear as a row;
        int IDataRecord.FieldCount
        {
            get { return Reader.FieldCount; }
        }

        bool IDataRecord.GetBoolean(int i)
        {
            return Reader.GetBoolean(i);
        }

        byte IDataRecord.GetByte(int i)
        {
            return Reader.GetByte(i);
        }

        long IDataRecord.GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return Reader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        char IDataRecord.GetChar(int i)
        {
            return Reader.GetChar(i);
        }

        long IDataRecord.GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return Reader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        IDataReader IDataRecord.GetData(int i)
        {
            return Reader.GetData(i);
        }

        string IDataRecord.GetDataTypeName(int i)
        {
            return Reader.GetDataTypeName(i);
        }

        DateTime IDataRecord.GetDateTime(int i)
        {
            return Reader.GetDateTime(i);
        }

        decimal IDataRecord.GetDecimal(int i)
        {
            return Reader.GetDecimal(i);
        }

        double IDataRecord.GetDouble(int i)
        {
            return Reader.GetDouble(i);
        }

        Type IDataRecord.GetFieldType(int i)
        {
            return Reader.GetFieldType(i);
        }

        float IDataRecord.GetFloat(int i)
        {
            return Reader.GetFloat(i);
        }

        Guid IDataRecord.GetGuid(int i)
        {
            return Reader.GetGuid(i);
        }

        short IDataRecord.GetInt16(int i)
        {
            return Reader.GetInt16(i);
        }

        int IDataRecord.GetInt32(int i)
        {
            return Reader.GetInt32(i);
        }

        long IDataRecord.GetInt64(int i)
        {
            return Reader.GetInt64(i);
        }

        string IDataRecord.GetName(int i)
        {
            return Reader.GetName(i);
        }

        int IDataRecord.GetOrdinal(string name)
        {
            return Reader.GetOrdinal(name);
        }

        string IDataRecord.GetString(int i)
        {
            return Reader.GetString(i);
        }

        object IDataRecord.GetValue(int i)
        {
            return Reader.GetValue(i);
        }

        int IDataRecord.GetValues(object[] values)
        {
            return Reader.GetValues(values);
        }

        bool IDataRecord.IsDBNull(int i)
        {
            return Reader.IsDBNull(i);
        }

        object IDataRecord.this[string name]
        {
            get { return Reader[name]; }
        }

        object IDataRecord.this[int i]
        {
            get { return Reader[i]; }
        }
    }
}
