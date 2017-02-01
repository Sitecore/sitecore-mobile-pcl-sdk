﻿
namespace Sitecore.MobileSDK.CrudTasks
{
  using System.Net.Http;
  using Sitecore.MobileSDK.API.Request;
  using Sitecore.MobileSDK.UrlBuilder.Search;
  using Sitecore.MobileSDK.API.Items;

  internal class RunStoredQuerryTasks : AbstractGetItemTask<ISitecoreStoredSearchRequest, ScItemsResponse>
  {
    public RunStoredQuerryTasks(RunStoredQuerryUrlBuilder urlBuilder, HttpClient httpClient)
      : base(httpClient)
    {
      this.urlBuilder = urlBuilder;
    }

    protected override string UrlToGetItemWithRequest(ISitecoreStoredSearchRequest request)
    {
      this.privateDb = request.ItemSource.Database;
      return this.urlBuilder.GetUrlForRequest(request);
    }

    public override string CurrentDb {
      get {
        return this.privateDb;
      }
    }

    private string privateDb = null;
    private readonly RunStoredQuerryUrlBuilder urlBuilder;
  }
}

