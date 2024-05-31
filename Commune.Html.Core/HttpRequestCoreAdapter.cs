using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NitroBolt.Wui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commune.Html
{
	public class HttpRequestCoreAdapter : IRequestAdapter<RequestData, ContentResult>
	{
		public static ContentResult Process<TState>(string method, JObject? body, Func<TState, JsonData[], RequestData, HtmlResult<HElement>> hViewCreator)
		  where TState : class, IWuiState, new()
		{
			return HWebApiSynchronizeHandler.Process<RequestData, ContentResult, TState>(
				new RequestData(method, body),
				HttpRequestCoreAdapter.Instance, hViewCreator
			);
		}

		public readonly static HttpRequestCoreAdapter Instance = new();

		public JObject? Content(RequestData request)
		{
			return request.Body;
		}

		public bool IsGetMethod(RequestData request)
		{
			return request.Method == HttpMethods.Get;
		}

		public ContentResult? RawResponse(HtmlResult<HElement> result)
		{
			HtmlResult? htmlResult = result as HtmlResult;
			if (htmlResult != null && htmlResult.RawResponse != null)
			{
				ContentData raw = htmlResult.RawResponse;
				return new ContentResult()
				{
					Content = raw.Content,
					ContentType = raw.ContentType,
					StatusCode = raw.StatusCode
				};
			}
			return null;
		}

		public ContentResult ToResponse(string content, string contentType, HtmlResult<HElement> result)
		{
			return new ContentResult() { Content = content, ContentType = contentType };
		}
	}
}
