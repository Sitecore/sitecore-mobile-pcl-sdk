﻿
namespace Sitecore.MobileSDK.CrudTasks.Entity
{
  using System.Net.Http;
  using Sitecore.MobileSDK.API.Entities;
  using Sitecore.MobileSDK.API.Request.Entity;
  using Sitecore.MobileSDK.UrlBuilder.Entity;

  internal class GetEntitiesByPathTask : AbstractGetEntityTask<IReadEntitiesByPathRequest, ScEntityResponse>
  {
    public GetEntitiesByPathTask(EntityByPathUrlBuilder<IReadEntitiesByPathRequest> urlBuilder, HttpClient httpClient)
      : base(httpClient)
    {
      this.urlBuilder = urlBuilder;
    }
    protected override string UrlToGetEntityWithRequest(IReadEntitiesByPathRequest request)
    {
      return this.urlBuilder.GetUrlForRequest(request);
    }

    private readonly EntityByPathUrlBuilder<IReadEntitiesByPathRequest> urlBuilder;

  }
}