# Object
Unity C#部分源码链接：<href>https://github.com/Unity-Technologies/UnityCsReference
## Object基本介绍
Object类是Unity中可以在编辑器中引用的所有对象的基类。Object类所在的命名空间为UnityEngine.

**部分作用：** 继承自Object的类可以被拖放到Inspector中的字段中，
或使用Inspector面板中的Object字段旁字段选择器来选取。

## 字段解析
Object中的字段有：
```
IntPtr   m_CachedPtr;
private int m_InstanceID;
private string m_UnityRuntimeErrorString;
internal static int OffsetOfInstanceIDInCPlusPlusObject = -1;
const string objectIsNullMessage = "The Object you want to instantiate is null.";
const string cloneDestroyedMessage = "Instantiate failed because the clone was destroyed during creation. This can happen if DestroyImmediate is called in MonoBehaviour.Awake.";
```
以上字段中，只有m_InstanceID、objectIsNullMessage与cloneDestroyedMessage是C#层会接触到的，
其他几个字段不做分析，m_InstanceID是该实例的ID,可以用于判断是否为同一个对象。
objectIsNullMessage：当调用Instantiate方法时，若被复制对象为null时会抛出该错误信息。
cloneDestroyedMessage：当调用Instantiate方法时，复制对象被销毁时会抛出该错误信息。

## 属性解析
name:对象的名字(显示在UI面板中)。
```
public string name
{
    get { return GetName(this); }
    set { SetName(this, value); }
}
```
hideFlags:位掩码，可以用位运算判断hideFlags中包含的全部HideFlag，具体作用见注释。
```
public extern HideFlags hideFlags { get; set; }

public enum HideFlags
  {
      // A normal, visible object. This is the default.
      None = 0,

      // The object will not appear in the hierarchy and will not show up in the project view if it is stored in an asset.
      HideInHierarchy = 1,

      // It is not possible to view it in the inspector
      HideInInspector = 2,

      // The object will not be saved to the scene.
      DontSaveInEditor = 4,

      // The object is not be editable in the inspector
      NotEditable = 8,

      // The object will not be saved when building a player
      DontSaveInBuild = 16,

      // The object will not be unloaded by UnloadUnusedAssets
      DontUnloadUnusedAsset = 32,

      DontSave = 4 + 16 + 32,

      // A combination of not shown in the hierarchy and not saved to to scenes.
      HideAndDontSave = 1 + 4 + 8 + 16 + 32
  }
```

## 方法解析
<div id = "EnsureRunningOnMainThread"]</div>
用于判定当前代码是否在主线程中运行,若不是会抛出异常，表示该方法不支持在子线程中运行。

```
private void EnsureRunningOnMainThread()
{
    if (!CurrentThreadIsMainThread())
        throw new System.InvalidOperationException("EnsureRunningOnMainThread can only be called from the main thread");
}
```
获取实例的唯一ID，因为调用了[EnsureRunningOnMainThread](#EnsureRunningOnMainThread)方法,
该方法也只能在主线程中调用。
```
[System.Security.SecuritySafeCritical]
public unsafe int GetInstanceID()
{
    //Because in the player we dissalow calling GetInstanceID() on a non-mainthread, we're also
    //doing this in the editor, so people notice this problem early. even though technically in the editor,
    //it is a threadsafe operation.
    EnsureRunningOnMainThread();
    return m_InstanceID;
}
```
获取Hash值。
```
public override int GetHashCode()
{
    //in the editor, we store the m_InstanceID in the c# objects. It's actually possible to have multiple c# objects
    //pointing to the same c++ object in some edge cases, and in those cases we'd like GetHashCode() and Equals() to treat
    //these objects as equals.
    return m_InstanceID;
}
```
比较方法，两个对象都继承自Object且InstanceID相同时返回true。
```
public override bool Equals(object other)
{
    Object otherAsObject = other as Object;
    // A UnityEngine.Object can only be equal to another UnityEngine.Object - or null if it has been destroyed.
    // Make sure other is a UnityEngine.Object if "as Object" fails. The explicit "is" check is required since the == operator
    // in this class treats destroyed objects as equal to null
    if (otherAsObject == null && other != null && !(other is Object))
        return false;
    return CompareBaseObjects(this, otherAsObject);
}

static bool CompareBaseObjects(UnityEngine.Object lhs, UnityEngine.Object rhs)
{
    bool lhsNull = ((object)lhs) == null;
    bool rhsNull = ((object)rhs) == null;

    if (rhsNull && lhsNull) return true;

    if (rhsNull) return !IsNativeObjectAlive(lhs);
    if (lhsNull) return !IsNativeObjectAlive(rhs);

    return lhs.m_InstanceID == rhs.m_InstanceID;
}
```
Instantiate方法(由给定的Prefab或Object创建一个新的Object)：
```
public static Object Instantiate(Object original, Vector3 position, Quaternion rotation)
{
    CheckNullArgument(original, objectIsNullMessage);

    if (original is ScriptableObject)
        throw new ArgumentException("Cannot instantiate a ScriptableObject with a position and rotation");
    //在根路径实例化一个对象
    var obj = Internal_InstantiateSingle(original, position, rotation);

    if (obj == null)
        throw new UnityException(cloneDestroyedMessage);

    return obj;
}

public static Object Instantiate(Object original, Vector3 position, Quaternion rotation, Transform parent)
{
    if (parent == null)
        return Instantiate(original, position, rotation);

    CheckNullArgument(original, objectIsNullMessage);

    // 在parent路径下实例化一个对象
    var obj = Internal_InstantiateSingleWithParent(original, parent, position, rotation);

    if (obj == null)
        throw new UnityException(cloneDestroyedMessage);

    return obj;
}
```
Destroy方法：移除一个GameObject或者Component等。
```
[NativeMethod(Name = "Scripting::DestroyObjectFromScripting", IsFreeFunction = true, ThrowsException = true)]
public extern static void Destroy(Object obj, [uei.DefaultValue("0.0F")] float t);

[uei.ExcludeFromDocs]
public static void Destroy(Object obj)
{
    float t = 0.0F;
    Destroy(obj, t); // t为延迟时间
}
```
DestroyImmediate方法：立刻移除Object，官方建议大多数情况都使用Destroy来移除。
```
// Destroys the object /obj/ immediately. It is strongly recommended to use Destroy instead.
[NativeMethod(Name = "Scripting::DestroyObjectFromScriptingImmediate", IsFreeFunction = true, ThrowsException = true)]
public extern static void DestroyImmediate(Object obj, [uei.DefaultValue("false")]  bool allowDestroyingAssets);

[uei.ExcludeFromDocs]
public static void DestroyImmediate(Object obj)
{
    bool allowDestroyingAssets = false;
    DestroyImmediate(obj, allowDestroyingAssets);
}
```
FindObjectsOfType方法：查找type类型的Object，默认返回值不包含disactive的对象。
FindObjectOfType:返回FindObjectsOfType查找的对象中的第一个对象。
```
// Returns a list of all active loaded objects of Type /type/.
public static Object[] FindObjectsOfType(Type type)
{
    return FindObjectsOfType(type, false);
}

// Returns a list of all loaded objects of Type /type/.
[TypeInferenceRule(TypeInferenceRules.ArrayOfTypeReferencedByFirstArgument)]
[FreeFunction("UnityEngineObjectBindings::FindObjectsOfType")]
public extern static Object[] FindObjectsOfType(Type type, bool includeInactive);

public static Object FindObjectOfType(System.Type type, bool includeInactive)
{
    Object[] objects = FindObjectsOfType(type, includeInactive);
    if (objects.Length > 0)
        return objects[0];
    else
        return null;
}
```
