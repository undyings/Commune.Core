using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using System.Net.Mail;
using Commune.Basis;
using Microsoft.AspNetCore.Http;
using Serilog;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Commune.Html
{
  public static class AuthHlp
  {
    public static string? UserName(this HttpContext context)
    {
      if (context != null && context.User != null && context.User.Identity != null)
        return context.User.Identity.Name;
      return null;
    }

    public static bool IsInRole(this HttpContext context, string role)
    {
      if (context != null && context.User != null && context.User.Identity != null)
        return context.User.IsInRole(role);
      return false;
    }

    //public static void SetUserFromCookie(this HttpContext context)
    //{
    //  SetUserFromCookie(context, false);
    //}

    //public static void SetUserFromCookie(this HttpContext context, bool logException)
    //{
    //  string? cookie = context.Request.Cookies[CookieAuthenticationDefaults.AuthenticationScheme];
    //  if (cookie != null)
    //  {
    //    try
    //    {
    //      var authTicket = FormsAuthentication.Decrypt(cookie);
    //      string[] roles = new string[0];
    //      if (!StringHlp.IsEmpty(authTicket.UserData))
    //        roles = authTicket.UserData.Split(',');

    //      SetUser(context, authTicket.Name, roles);
    //    }
    //    catch (Exception ex)
    //    {
    //      if (logException)
    //      {
    //        Log.Error(ex, "");
    //        Log.Information("Cookie: {0}", cookie);
    //      }
    //    }
    //  }
    //}

    public static void Logout(this HttpContext context)
    {
      //Log.Information("Logout1: {0}, {1}", context.User.Identity != null, context.UserName());
      context.SignOutAsync();
      context.User = new ClaimsPrincipal();

      //Log.Information("Logout2: {0}, {1}", context.User.Identity != null, context.UserName());
    }

    public static void SetUser(HttpContext context, string login, params string[] roles)
    {
      context.User = new System.Security.Principal.GenericPrincipal(
        new System.Security.Principal.GenericIdentity(login), roles);
    }

    public static void SetUserAndCookie(this HttpContext context, string login, params string[] roles)
    {
      UserInCookie(context, login, roles);
      SetUser(context, login, roles);
    }

    public static void UserInCookie(HttpContext context, string login, params string[] roles)
    {
			List<Claim> claims = new()
			{
				new Claim(ClaimTypes.Name, login)
			};
			foreach (string role in roles)
        claims.Add(new Claim(ClaimTypes.Role, role));

			ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
      AuthenticationProperties authProperties = new()
      {
        ExpiresUtc = DateTime.Now.AddMonths(1),
        IsPersistent = true
			};

      context.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(identity),
        authProperties
       ); //.RunSynchronously();

			//FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket
   //     (
   //        1, //version
   //        login, // user name
   //        DateTime.Now,             //creation
   //        DateTime.Now.AddMonths(1), //Expiration (you can set it to 1 month
   //        true,  //Persistent
   //        string.Join(",", roles)
   //     ); // additional informations

   //   string encryptedTicket = FormsAuthentication.Encrypt(authTicket);

   //   HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName,
   //     encryptedTicket);

   //   authCookie.Expires = authTicket.Expiration;
   //   authCookie.HttpOnly = true;

   //   context.Response.SetCookie(authCookie);
    }

    public static void SendMail(SmtpClient client, string from, string mailto,
      string caption, string messageAsHtml, params Attachment[] attachments)
    {
      using (MailMessage mail = new MailMessage())
      {
        mail.From = new MailAddress(from);
        mail.To.Add(new MailAddress(mailto));
        mail.Subject = caption;
        mail.BodyEncoding = System.Text.Encoding.UTF8;
        mail.Body = messageAsHtml;
        mail.IsBodyHtml = true;
        foreach (Attachment attachment in attachments)
          mail.Attachments.Add(attachment);

        client.Send(mail);
      }
    }

    public static SmtpClient CreateSmtpClient(string smtpServer, int smtpPort,
      string userName, string password)
    {
      SmtpClient client = new SmtpClient(smtpServer);
      if (smtpPort != 0)
        client.Port = smtpPort;
      if (!StringHlp.IsEmpty(userName))
        client.Credentials = new NetworkCredential(userName, password);
      client.DeliveryMethod = SmtpDeliveryMethod.Network;
      return client;
    }
  }
}
