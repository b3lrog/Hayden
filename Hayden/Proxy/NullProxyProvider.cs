﻿using System;
using System.Net;
using System.Net.Http;

namespace Hayden.Proxy
{
	public class NullProxyProvider : ProxyProvider
	{
		public NullProxyProvider(Action<HttpClientHandler> configureClientHandlerAction = null) : base(configureClientHandlerAction)
		{
			// use only a direct connection
			ProxyClients.Add(new HttpClientProxy(CreateNewClient((IWebProxy)null), "baseconnection/none"));
		}
	}
}