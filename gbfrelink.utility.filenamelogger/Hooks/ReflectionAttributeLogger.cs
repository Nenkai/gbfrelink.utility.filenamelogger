using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

using Reloaded.Hooks.Definitions;

using gbfrelink.utility.filenamelogger;

namespace gbfrelink.utility.filenamelogger.Hooks;

public unsafe delegate ulong RegisterObjectDelegate(ObjectDef* objectDef);

public class ReflectionAttributeLogger : HashLoggerBase<RegisterObjectDelegate>
{
    
    public unsafe ReflectionAttributeLogger(string name, string fileName, string pattern)
        : base(name, fileName, pattern)
    {
        FuncHook = RegisterObjectHook;
    }

    private ulong lastListAddress;

    public static HashSet<string> _knownObjects = new HashSet<string>();

    public unsafe ulong RegisterObjectHook(ObjectDef* objectDef)
    {
        string objectName = Marshal.PtrToStringAnsi((nint)objectDef->pObjectType->pName);
        string typeName = Marshal.PtrToStringAnsi((nint)objectDef->pObjectType->pTypeName);
        if (!_knownObjects.Contains(objectName))
        {
            if (!string.IsNullOrEmpty(typeName))
                Add($"class {objectName} : {typeName}");
            else
                Add($"class {objectName}");

            Add("{");

            IAttributeList* attrList = objectDef->pObjectType->pAttrList;
            int count = (int)(attrList->pEnd - attrList->pBegin);
            for (int i = 0; i < count; i++)
            {
                IAttribute* attr = attrList->pBegin[i];

                if (attr->field_0x08 == 1) // Swapped? weird
                {
                    var attrName = Extensions.PointerToString(attr->pTypeName);
                    var attrTypeName = Extensions.PointerToString(attr->pAttrName);
                    Add($"    {attrTypeName} {attrName}; // Offset 0x{attr->dwOffset}, 0x08:{attr->field_0x08}, 0x38:{attr->field_0x38}");
                }
                else
                {
                    var attrName = Extensions.PointerToString(attr->pAttrName);
                    var attrTypeName = Extensions.PointerToString(attr->pTypeName);
                    Add($"    {attrTypeName} {attrName}; // Offset 0x{attr->dwOffset}, 0x08:{attr->field_0x08}, 0x38:{attr->field_0x38}");
                }
            }
            Add("}");
            Add("\n");

            _knownObjects.Add(objectName);
        }

        return Hook.OriginalFunction(objectDef);
    }
}

[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
public unsafe struct ObjectDef
{
    [FieldOffset(0x00)]
    public byte* pName;

    [FieldOffset(0x08)]
    public int dwHash;

    [FieldOffset(0x10)]
    public IObjectType* pObjectType;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct IObjectType
{
    [FieldOffset(0)]
    public void* pVtable;

    [FieldOffset(0x08)]
    public byte* pName;

    [FieldOffset(0x28)]
    public IAttributeList* pAttrList;

    [FieldOffset(0x50)]
    public byte* pTypeName;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct IAttributeList
{
    [FieldOffset(0x00)]
    public IAttribute** pBegin;

    [FieldOffset(0x08)]
    public IAttribute** pEnd;

    [FieldOffset(0x10)]
    public IAttribute** pCapacity;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct IAttribute
{
    [FieldOffset(0)]
    public void* pVtable;

    [FieldOffset(0x08)]
    public int field_0x08;

    [FieldOffset(0x10)]
    public byte* pAttrName;

    [FieldOffset(0x18)]
    public byte* pTypeName;

    [FieldOffset(0x20)]
    public int dwOffset;

    [FieldOffset(0x24)]
    public int field_0x24;

    [FieldOffset(0x28)]
    public int field_0x28;

    [FieldOffset(0x21)]
    public int field_0x2C;

    [FieldOffset(0x30)]
    public int field_0x30;

    [FieldOffset(0x34)]
    public int field_0x34;

    [FieldOffset(0x38)]
    public int field_0x38;
}
