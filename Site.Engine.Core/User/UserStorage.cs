using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Commune.Basis;
using Commune.Data;
using Microsoft.EntityFrameworkCore;

namespace Site.Engine
{
  public class UserStorage
  {
    class UsersReplica
    {
      public readonly Dictionary<int, LightObject> UserById;
      public readonly Dictionary<string, LightObject> UserByJsonLogin;
      public readonly Dictionary<string, LightObject> UserByEmail;

      public UsersReplica(Dictionary<int, LightObject> userById, Dictionary<string, LightObject> userByJsonLogin, Dictionary<string, LightObject> userByEmail)
      {
        this.UserById = userById;
        this.UserByJsonLogin = userByJsonLogin;
        this.UserByEmail = userByEmail;
      }
		}

    readonly object lockObj = new object();

    public LightObject? FindUser(int? userId)
    {
      if (userId == null)
        return null;

      lock (lockObj)
        return storageCache.Result.UserById.Find(userId.Value);
    }

    public LightObject[] All
    {
      get 
      { 
        lock (lockObj)
          return storageCache.Result.UserById.Values.ToArray(); 
      }
    }

    public LightObject? FindUser(string jsonLoginId)
    {
      lock (lockObj)
        return storageCache.Result.UserByJsonLogin.Find(jsonLoginId.ToLower());
    }

    public LightObject? FindUserByEmail(string email)
    {
      lock (lockObj)
        return storageCache.Result.UserByEmail.Find(email.ToLower());
    }

    long changeTick = 0;
    public void Update()
    {
      lock (lockObj)
        changeTick++;
    }

    readonly RawCache<UsersReplica> storageCache;

    readonly DataLayer userConnection;

    public UserStorage(DataLayer userConnection)
    {
      this.userConnection = userConnection;

      this.storageCache = new Cache<long, UsersReplica>(
        delegate
        {
          using BoxDbContext dbContext = this.userConnection.Create();

          ObjectBox userBox = new(userConnection, dbContext, dbContext => dbContext.Objects.ForType(UserType.User));
          int[] allObjectIds = userBox.AllObjectIds;

          Dictionary<int, LightObject> userById = new(allObjectIds.Length);
          Dictionary<string, LightObject> userByJsonLogin = new(allObjectIds.Length);
          Dictionary<string, LightObject> userByEmail = new(allObjectIds.Length);

          foreach (int userId in allObjectIds)
          {
            LightObject user = new(userBox, userId);
            userById[userId] = user;

            string xmlLogin = user.Head.JsonId.ToLower();
            if (!StringHlp.IsEmpty(xmlLogin))
              userByJsonLogin[xmlLogin] = user;

            string email = user.Get(UserType.Email).ToLower();
            if (email != "")
              userByEmail[email] = user;
          }

          return new UsersReplica(userById, userByJsonLogin, userByEmail);
        },
        delegate
        {
          return changeTick;
        }
      );
    }
  }
}
