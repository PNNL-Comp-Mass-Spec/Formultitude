// Decompiled with JetBrains decompiler
// Type: TestFSDBSearch.My.InternalXmlHelper
// Assembly: TestFSDBSearch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C36EBD2C-6652-4FDC-A74D-B15E46A79224
// Assembly location: F:\Documents\Projects\NikolaTolic\Formularity\Lib\TestFSDBSearch.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace TestFSDBSearch.My
{
  [EditorBrowsable(EditorBrowsableState.Never)]
  [DebuggerNonUserCode]
  [CompilerGenerated]
  internal sealed class InternalXmlHelper
  {
    [EditorBrowsable(EditorBrowsableState.Never)]
    private InternalXmlHelper()
    {
    }

    public static string get_Value(IEnumerable<XElement> source)
    {
      IEnumerator<XElement> enumerator;
      try
      {
        enumerator = source.GetEnumerator();
        if (enumerator.MoveNext())
          return enumerator.Current.Value;
      }
      catch (Exception ex)
      {
          Console.WriteLine(ex.Message);
      }
      return (string) null;
    }

    public static void set_Value(IEnumerable<XElement> source, string value)
    {
      IEnumerator<XElement> enumerator;
        try
        {
            enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
                return;
            enumerator.Current.Value = value;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public static string get_AttributeValue(IEnumerable<XElement> source, XName name)
    {
      IEnumerator<XElement> enumerator;
      try
      {
        enumerator = source.GetEnumerator();
        if (enumerator.MoveNext())
          return (string) enumerator.Current.Attribute(name);
      }
      catch (Exception ex)
      {
          Console.WriteLine(ex.Message);
      }
      return (string) null;
    }

    public static void set_AttributeValue(IEnumerable<XElement> source, XName name, string value)
    {
      IEnumerator<XElement> enumerator;
      try
      {
        enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
          return;
        enumerator.Current.SetAttributeValue(name, (object) value);
      }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

        public static string get_AttributeValue(XElement source, XName name)
    {
      return (string) source.Attribute(name);
    }

    public static void set_AttributeValue(XElement source, XName name, string value)
    {
      source.SetAttributeValue(name, (object) value);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static XAttribute CreateAttribute(XName name, object value)
    {
      if (value == null)
        return (XAttribute) null;
      return new XAttribute(name, RuntimeHelpers.GetObjectValue(value));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static XAttribute CreateNamespaceAttribute(XName name, XNamespace ns)
    {
      XAttribute xattribute = new XAttribute(name, (object) ns.NamespaceName);
      xattribute.AddAnnotation((object) ns);
      return xattribute;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static object RemoveNamespaceAttributes(string[] inScopePrefixes, XNamespace[] inScopeNs, List<XAttribute> attributes, object obj)
    {
      if (obj != null)
      {
        XElement e = obj as XElement;
        if (e != null)
        {
          // ISSUE: reference to a compiler-generated method
          return (object) InternalXmlHelper.RemoveNamespaceAttributes(inScopePrefixes, inScopeNs, attributes, e);
        }
        IEnumerable enumerable = obj as IEnumerable;
        if (enumerable != null)
        {
          // ISSUE: reference to a compiler-generated method
          return (object) InternalXmlHelper.RemoveNamespaceAttributes(inScopePrefixes, inScopeNs, attributes, enumerable);
        }
      }
      return obj;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IEnumerable RemoveNamespaceAttributes(string[] inScopePrefixes, XNamespace[] inScopeNs, List<XAttribute> attributes, IEnumerable obj)
    {
      if (obj == null)
        return obj;
      IEnumerable<XElement> source = obj as IEnumerable<XElement>;
      if (source != null)
      {
        // ISSUE: object of a compiler-generated type is created
        // ISSUE: reference to a compiler-generated method
        return (IEnumerable) source.Select<XElement, XElement>(new Func<XElement, XElement>(new InternalXmlHelper.RemoveNamespaceAttributesClosure(inScopePrefixes, inScopeNs, attributes).ProcessXElement));
      }
      // ISSUE: object of a compiler-generated type is created
      // ISSUE: reference to a compiler-generated method
      return (IEnumerable) obj.Cast<object>().Select<object, object>(new Func<object, object>(new InternalXmlHelper.RemoveNamespaceAttributesClosure(inScopePrefixes, inScopeNs, attributes).ProcessObject));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static XElement RemoveNamespaceAttributes(string[] inScopePrefixes, XNamespace[] inScopeNs, List<XAttribute> attributes, XElement e)
    {
      XAttribute nextAttribute;
      if (e != null)
      {
        for (XAttribute xattribute = e.FirstAttribute; xattribute != null; xattribute = nextAttribute)
        {
          nextAttribute = xattribute.NextAttribute;
          if (xattribute.IsNamespaceDeclaration)
          {
            XNamespace xnamespace1 = xattribute.Annotation<XNamespace>();
            string localName1 = xattribute.Name.LocalName;
            if ((object) xnamespace1 != null)
            {
              if (inScopePrefixes != null && inScopeNs != null)
              {
                int num1 = checked (inScopePrefixes.Length - 1);
                int num2 = 0;
                int num3 = num1;
                int index = num2;
                while (index <= num3)
                {
                  string inScopePrefix = inScopePrefixes[index];
                  XNamespace inScopeN = inScopeNs[index];
                  if (localName1.Equals(inScopePrefix))
                  {
                    if (xnamespace1 == inScopeN)
                      xattribute.Remove();
                    xattribute = (XAttribute) null;
                    break;
                  }
                  checked { ++index; }
                }
              }
              if (xattribute != null)
              {
                if (attributes != null)
                {
                  int num1 = checked (attributes.Count - 1);
                  int num2 = 0;
                  int num3 = num1;
                  int index = num2;
                  while (index <= num3)
                  {
                    XAttribute attribute = attributes[index];
                    string localName2 = attribute.Name.LocalName;
                    XNamespace xnamespace2 = attribute.Annotation<XNamespace>();
                    if ((object) xnamespace2 != null && localName1.Equals(localName2))
                    {
                      if (xnamespace1 == xnamespace2)
                        xattribute.Remove();
                      xattribute = (XAttribute) null;
                      break;
                    }
                    checked { ++index; }
                  }
                }
                if (xattribute != null)
                {
                  xattribute.Remove();
                  attributes.Add(xattribute);
                }
              }
            }
          }
        }
      }
      return e;
    }

        private class RemoveNamespaceAttributesClosure
        {
            private string[] inScopePrefixes;
            private XNamespace[] inScopeNs;
            private List<XAttribute> attributes;

            public RemoveNamespaceAttributesClosure(string[] inScopePrefixes, XNamespace[] inScopeNs, List<XAttribute> attributes)
            {
                this.inScopePrefixes = inScopePrefixes;
                this.inScopeNs = inScopeNs;
                this.attributes = attributes;
            }

            public XElement ProcessXElement(XElement arg)
            {
                throw new NotImplementedException();
            }

            public object ProcessObject(object arg)
            {
                throw new NotImplementedException();
            }
        }
    }
}
