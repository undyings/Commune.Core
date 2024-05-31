using System;
using System.Collections.Generic;
using System.Text;
using Commune.Basis;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Commune.Data
{
  public struct UniversalKey
  {
    public readonly object[] KeyParts;

    public UniversalKey(params object[] keyParts)
    {
      this.KeyParts = keyParts;
    }

    public override int GetHashCode()
    {
      int hashCode = 0;
      foreach (object key in KeyParts)
      {
        int keyHashCode = -13890;
        if (key != null)
          keyHashCode = key.GetHashCode();
        hashCode = hashCode ^ keyHashCode;
      }
      return hashCode;
    }

    public override bool Equals(object? obj)
    {
      if (!(obj is UniversalKey))
        return false;

      UniversalKey universalKey = (UniversalKey)obj;

      object[] otherKeyParts = universalKey.KeyParts;
      if (KeyParts.Length != otherKeyParts.Length)
        return false;

      for (int i = 0; i < KeyParts.Length; ++i)
      {
        if (!object.Equals(KeyParts[i], otherKeyParts[i]))
          return false;
      }

      return true;
    }

    public override string ToString()
    {
      return StringHlp.Join(", ", "{0}", KeyParts);
    }
  }

}
