using Shortnr.Web.Business;
using Shortnr.Web.Business.Implementations;
using System;
using System.Web.Http;
using Unity;
using Unity.AspNet.WebApi;

namespace Shortnr.Web
{
	public static class UnityConfig
	{
		private static Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() =>
		{
			var container = new UnityContainer();
			RegisterTypes(container);
			return container;
		});

		public static IUnityContainer GetConfiguredContainer()
		{
			return container.Value;
		}

		public static void RegisterTypes(IUnityContainer container)
		{
			container.RegisterType<IUrlManager, UrlManager>();
		}
	}
}