﻿
namespace Sitecore.MobileSDK.CrudTasks
{
  using System.Net.Http;

  using Sitecore.MobileSDK.UrlBuilder.UpdateItem;
  using Sitecore.MobileSDK.API.Request;

  using Newtonsoft.Json.Linq;
  using Newtonsoft.Json;

  internal class UpdateItemByIdTask : AbstractUpdateItemTask<IUpdateItemByIdRequest>
  {
    public UpdateItemByIdTask(UpdateItemByIdUrlBuilder urlBuilder, HttpClient httpClient)
      : base(httpClient)
    {
      this.urlBuilder = urlBuilder;
    }

    protected override string UrlToGetItemWithRequest(IUpdateItemByIdRequest request)
    {
      this.privateDb = request.ItemSource.Database;
      return this.urlBuilder.GetUrlForRequest(request);
    }

    public override string GetFieldsListString(IUpdateItemByIdRequest request)
    {
      string result = string.Empty;

      JObject jsonObject = new JObject();

      bool fieldsAvailable = (null != request.FieldsRawValuesByName);
      if (fieldsAvailable)
      {
        fieldsAvailable = (request.FieldsRawValuesByName.Count > 0);
      }

      if (fieldsAvailable)
      {
        foreach (var fieldElem in request.FieldsRawValuesByName)
        {
          jsonObject.Add(fieldElem.Key, fieldElem.Value);
        }
      }

      result = jsonObject.ToString(Formatting.None);

      return result;
    }

    public override string CurrentDb {
      get {
        return this.privateDb;
      }
    }

    private string privateDb = null;

    private readonly UpdateItemByIdUrlBuilder urlBuilder;
  }
}

