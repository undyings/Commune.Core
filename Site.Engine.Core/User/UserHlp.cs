using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;
using Commune.Basis;
using Commune.Html;
using System.Drawing;
using NitroBolt.Wui;
using Commune.Data;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;
using Serilog;

namespace Site.Engine
{
  public class UserHlp
  {
    static DataLayer userConnection
    {
      get
      {
        return BaseContext.Default.UserConnection;
      }
    }

		public static bool IsBanned(LightObject user)
		{
			DateTime? bannedUntil = user.Get(UserType.BannedUntil);
			if (bannedUntil != null && bannedUntil.Value > DateTime.UtcNow)
				return true;

			return false;
		}

    static HBuilder h = HBuilder.Extension;

		public static void SendRegistrationConfirmation(SiteSettings settings, int userId, string login, string email, string salt)
		{
      string siteName = settings.Organization;

			string url = string.Format("{0}/confirmation?id={1}&hash={2}",
				settings.SiteHost, userId, UserHlp.CalcUserDataHash(userId, login, salt)
			);

			HElement answer = h.Div(
				h.P(string.Format("Вы указали этот адрес при регистрации на {0}", siteName)),
				h.P("Чтобы завершить регистрацию, пожалуйста, перейдите по ссылке:"),
				h.A(
					h.href(url),
					new HAttribute("target", "_blank"),
					url
				),
				h.P("Если вы не регистрировались на сайте, значит, произошла ошибка - просто проигнорируйте это письмо")
			);

			SmtpClient smtpClient = AuthHlp.CreateSmtpClient(
				settings.SmtpHost, settings.SmtpPort, settings.SmtpUserName, settings.SmtpPassword);
			AuthHlp.SendMail(smtpClient, settings.MailFrom, email,
				string.Format("Подтвердите регистрацию на {0}", siteName), answer.ToHtmlText()
			);

			Log.Information("Подтверждение регистрации: {0}", email);
		}

		public static string CalcUserDataHash(int userId, string userData, string salt)
    {
      List<byte> bytes = new();
      bytes.AddRange(BitConverter.GetBytes(userId));

      foreach (char ch in salt)
        bytes.AddRange(BitConverter.GetBytes(ch));

      foreach (char ch in userData)
        bytes.AddRange(BitConverter.GetBytes(ch));

			return BitConverter.ToString(SHA256.HashData(bytes.ToArray())).Replace("-", "");
    }

    public static string GetFirstLetter(LightObject user)
    {
      string firstName = user.Get(UserType.FirstName);
      if (!StringHlp.IsEmpty(firstName))
        return firstName.Substring(0, 1);

      string family = user.Get(UserType.Family);
      if (!StringHlp.IsEmpty(family))
        return family.Substring(0, 1);

      return "";
    }

    public static LightObject? GetCurrentUser(HttpContext httpContext, UserStorage userStorage)
    {
      string jsonLoginId = httpContext.UserName() ?? "";

      if (httpContext.IsInRole("service"))
        return null;

      if (StringHlp.IsEmpty(jsonLoginId))
        return null;

      LightObject? user = userStorage.FindUser(jsonLoginId);
      if (user == null)
        httpContext.Logout();

      return user;
    }

    public static string UserToString(LightObject user)
    {
      return string.Format("{0} {1}", user.Get(UserType.FirstName), user.Get(UserType.Family)).TrimStart();
    }

    //public static LightObject CheckOrCreateVkUser(UserStorage userStorage, JsonData userInfo)
    //{
    //  string uid = userInfo.JPath("response", "uid")?.ToString();

    //  if (StringHlp.IsEmpty(uid))
    //    return null;

    //  string xmlLogin = UserType.Login.CreateXmlIds("vk", uid);

    //  string firstName = userInfo.JPath("response", "first_name")?.ToString();
    //  string lastName = userInfo.JPath("response", "last_name")?.ToString();

    //  LightObject user = DataBox.LoadOrCreateObject(userConnection, UserType.User, xmlLogin);

    //  if (user.Get(UserType.Family) != lastName)
    //    user.Set(UserType.Family, lastName);

    //  if (user.Get(UserType.FirstName) != firstName)
    //    user.Set(UserType.FirstName, firstName);

    //  Logger.AddMessage("VkUserUpdate: {0}", user.Box.DataChangeTick);

    //  if (user.Box.DataChangeTick != 0)
    //  {
    //    user.Set(ObjectType.ActTill, DateTime.UtcNow);
    //    user.Box.Update();
    //    userStorage.Update();
    //  }

    //  return user;
    //}

    //public static string VkAuthorizeUrl(string applicationId, string redirectUrl)
    //{
    //  return string.Format(
    //    @"http://oauth.vk.com/authorize?client_id={0}&redirect_uri={1}&response_type=code",
    //      applicationId, redirectUrl
    //  );
    //}

    //public static JsonData VkUserInfo(string applicationId, string secretKey, string code,
    //  string redirectUri, params string[] fields)
    //{
    //  using (WebClient webClient = new WebClient())
    //  {
    //    webClient.QueryString.Add("client_id", applicationId);
    //    webClient.QueryString.Add("client_secret", secretKey);
    //    webClient.QueryString.Add("code", code);
    //    webClient.QueryString.Add("redirect_uri", redirectUri);

    //    string answer = webClient.DownloadString("https://oauth.vk.com/access_token");

    //    JsonSerializer jsonSerializer = JsonSerializer.Create();
    //    JsonData json = new JsonData(jsonSerializer.Deserialize(
    //      new JsonTextReader(new StringReader(answer))
    //    ));
    //    object accessToken = json.JPath("access_token");
    //    object userId = json.JPath("user_id");

    //    //Logger.AddMessage("VkAnswer: {0}, {1}, {2}, {3}", code, answer, accessToken, userId);

    //    webClient.QueryString.Clear();
    //    webClient.QueryString.Add("uids", userId.ToString());
    //    webClient.QueryString.Add("fields", string.Join(",", fields));
    //    webClient.QueryString.Add("access_token", accessToken.ToString());

    //    byte[] bytes = webClient.DownloadData("https://api.vk.com/method/users.get");

    //    string userString = Encoding.UTF8.GetString(bytes).Replace("[", "").Replace("]", "");
    //    return new JsonData(jsonSerializer.Deserialize(
    //      new JsonTextReader(new StringReader(userString))
    //    ));
    //  }
    //}

    public static LightObject? LoadUser(string auth, string login)
    {
			using BoxDbContext db = userConnection.Create();

			return DataBox.LoadObject(userConnection, db, UserType.User, UserType.JsonId.Create(new LoginId() { Auth = auth, Login = login }));
		}

    public static bool SiteAuthorization(SiteSettings settings, 
      HttpContext httpContext, string login, string password)
    {
      if (login == "admin")
      {
        string adminPassword = settings.AdminPassword;
        if (!StringHlp.IsEmpty(adminPassword) && password == adminPassword)
        {
          httpContext.SetUserAndCookie("admin", "service", "edit", "seo");
          return true;
        }
        return false;
      }

      if (login == "edit")
      {
        string editPassword = settings.EditPassword;
        if (!StringHlp.IsEmpty(editPassword) && password == editPassword)
        {
          httpContext.SetUserAndCookie("admin", "service", "edit");
          return true;
        }

        if (settings.GuestEditModeEnabled)
        {
          httpContext.SetUserAndCookie(login, "edit", "service", "nosave");
          return true;
        }
        return false;
      }

      if (login == "seo")
      {
        string seoPassword = settings.SeoPassword;
        if (!StringHlp.IsEmpty(seoPassword) && password == seoPassword)
        {
          httpContext.SetUserAndCookie("admin", "service", "seo");
          return true;
        }

        if (settings.GuestEditModeEnabled)
        {
          httpContext.SetUserAndCookie(login, "seo", "service", "nosave");
          return true;
        }
        return false;
      }

      return false;
    }

    public static bool DirectAuthorization(HttpContext httpContext, SiteSettings settings)
    {
      string auth = httpContext.Get("auth");
      if (StringHlp.IsEmpty(auth))
        return false;

      if (auth == "logout")
      {
        httpContext.Logout();
        return false;
      }

      return SiteAuthorization(settings, httpContext, auth, httpContext.Get("psw"));

    }
  }
}
