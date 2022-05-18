using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace QTTabBarLib
{
    internal class PreMergeToMergedDeserializationBinder : SerializationBinder
    {

        public override Type BindToType(string assemblyName, string typeName)
        {

            Type typeToDeserialize = null;

            // For each assemblyName/typeName that you want to deserialize to
            // a different type, set typeToDeserialize to the desired type.
            String exeAssembly = Assembly.GetExecutingAssembly().FullName;

            QTUtility2.log("PreMergeToMergedDeserializationBinder exeAssembly:" + exeAssembly + " typeName: " + typeName );
            // The following line of code returns the type.
            typeToDeserialize = Type.GetType(String.Format("{0}, {1}", typeName, exeAssembly));

            return typeToDeserialize;
        }
    }

}
