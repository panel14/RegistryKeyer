
using Microsoft.Win32;
using System.Net;

using (RegistryKey root = Registry.LocalMachine)
{
    int curId = 1;
    string myKey = "HKEY_LOCAL_MACHINE";

    SearchSubKeys(root, myKey, curId);
}

// Write node in file. Format: <parentId> strValue=<string> lstringValue=<string> bstringValue=<string>
// All types is a string
// You can find file named "registry.txt" at <PATH_TO_PROJECT>/RegistryKeyer/bin/Debug
// If you have compiled binary version you can't read it, but can find "registry.txt" in same folder with programm

// CHANGE THIS FUNCTION TO GET EXPECTED DATA FILE
static int WriteNodeInfo(RegistryKey key, int parentId, string nodeName, int id)
{
    var names = key.GetValueNames();
    //List<string> values = new ();
    int ret = 0;
    foreach ( var name in names )
    {
        string curValue = "";
        var value = key.GetValue(name);
        if ( value != null )
        {
            Type type = value.GetType();

            if (type == typeof(string))
            {
                string val = value.ToString();
                if (val.Length > 0 )
                {
                    val = val.Replace(' ', '_');
                    val = val.Trim();
                }
                //values.Add(val);
                curValue = val;
            }
            else if (type == typeof(byte[])) 
            {
                byte[] bytes = value as byte[];
                Random random = new();
                int endIndex = (bytes.Length > 50) ? random.Next(50) : random.Next(bytes.Length);
                string bytesStr = Convert.ToBase64String(bytes)[..endIndex];

                if (bytesStr.Length > 0)
                {
                    bytesStr = bytesStr.Replace(' ', '_');
                    bytesStr = bytesStr.Trim();
                } 

               // values.Add(bytesStr);
               curValue = bytesStr;
            }
            else if (type == typeof(string[]))
            {
                string[] strings = value as string[];
                string oneString = string.Join(";", strings);

                if (oneString.Length > 0)
                {
                    oneString = oneString.Replace(" ", "_");
                    oneString = oneString.Trim();
                }

                //values.Add(oneString);
                curValue = oneString;
            }
        }

        string rValue = curValue.Replace(" ", "_");
        rValue = rValue.Trim();
        File.AppendAllText("registry.text", $"{id + 1} pName={nodeName} name={name} value={rValue}");

        ret++;
    }

    if (nodeName.Length > 0)
    {
        nodeName = nodeName.Replace(" ", "_");
        nodeName = nodeName.Trim();
    }
    int namesCount = 0;
    string nName = nodeName.Replace(" ", "_");
    File.AppendAllText("registry.txt", $"{id} pName={nName} name={nName} value=node\n");
    //foreach (var value in values)
    //{
    //    string vName = names[namesCount++].Replace(" ", "_");
    //    File.AppendAllText("registry.txt", $"{id + 1} pName={nName} name={vName} value={value}\n");
    //}

    return (ret > 0) ? 1: 0;
}

//Raw node info Shell Folders
static string ProcessKey(RegistryKey key)
{
    string processedValue = "{";

    var names = key.GetValueNames();

    if (names.Length == 0)
    {
        return "{0}";
    }

    foreach (var name in names)
    {
        var value = key.GetValue(name);
        if (value != null)
        {
            Type valueType = value.GetType();

            if (valueType == typeof(string))
            {
                string valueString = value.ToString();
                processedValue += name + ":" + valueString;
            }
            else if (valueType == typeof(byte[]))
            {
                byte[] bytes = value as byte[];
                Random random = new ();
                int endIndex = (bytes.Length > 50) ? random.Next (50) : random.Next (bytes.Length);
                string bytesStr = Convert.ToBase64String(bytes)[..endIndex];

                processedValue += name + ":" + bytesStr;
            }
            else if (valueType == typeof(int))
            {
                int iValue = (int)value;
                processedValue += name + ":" + iValue.ToString();
            }

            else if (valueType == typeof(string[]))
            {
                string[] strings = value as string[];
                string concatenated = string.Join(";", strings);
                processedValue += name + ":" + concatenated;
            }

            else
            {
                processedValue += name + "VALUE TYPE - " + value.ToString();
            }

            if (names.Length > 1)
            {
                processedValue += ",";
            }
        }
    }

    return processedValue + "}";
}

static void SearchSubKeys(RegistryKey root, string searchKey, int currentId)
{
    int idCounter = 1;
    foreach (string keyname in root.GetSubKeyNames())
    {
        try
        {
            using (RegistryKey key = root.OpenSubKey(keyname))
            {
                string prefix = $"PARENT ID: {currentId}";
                string values = ProcessKey(key);

                int innerIds = WriteNodeInfo(key, currentId, keyname, currentId);

                Console.WriteLine($"{prefix}; NODE: {keyname}; VALUES: {values}");
                SearchSubKeys(key, searchKey, currentId + idCounter + innerIds);
            }
        }
        catch (System.Security.SecurityException)
        {
        }
        idCounter++;
    }
}




