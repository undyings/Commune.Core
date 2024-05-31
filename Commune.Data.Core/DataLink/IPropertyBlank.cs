using System;
using System.Collections.Generic;
using System.Text;

namespace Commune.Data
{
  public interface IPropertyBlank<T>
    where T : class
  {
		int Kind { get; }
	}

	public interface IPropertyBlank<T, TField> : IPropertyBlank<T>
    where T : class
  {
    FieldBlank<T, TField> Field { get; }
  }

  public class PropertyBlank<TField> : IPropertyBlank<PropertyRow, TField>
  {
    readonly int kind;
    readonly FieldBlank<PropertyRow, TField> field;

    public PropertyBlank(int propertyKind, FieldBlank<PropertyRow, TField> field)
    {
      this.kind = propertyKind;
      this.field = field;
    }

    public FieldBlank<PropertyRow, TField> Field
    {
      get { return this.field; }
    }

    public int Kind
    {
      get { return this.kind; }
    }
  }

  public class LinkBlank : IPropertyBlank<LinkRow, int>
  {
    readonly static FieldBlank<LinkRow, int> field = new(0,
      delegate (LinkRow link) { return link.ChildId; },
      delegate (LinkRow link, int childId)
      {
        link.ChildId = childId;
      }
    );

    readonly int kind;

    public LinkBlank(int linkKind)
    {
      this.kind = linkKind;
    }

    public int Kind
    {
      get
      {
        return this.kind;
      }
    }

    public FieldBlank<LinkRow, int> Field
    {
      get { return field; }
    }
  }

}
