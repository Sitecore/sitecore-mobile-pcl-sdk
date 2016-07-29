﻿namespace Sitecore.MobileSDK.Items
{
  using System;
  using System.Collections.Generic;
  using System.Threading;
  using Newtonsoft.Json;
  using Newtonsoft.Json.Linq;
  using Sitecore.MobileSDK.API.Exceptions;
  using Sitecore.MobileSDK.API.Fields;
  using Sitecore.MobileSDK.API.Items;
  using Sitecore.MobileSDK.Items.Fields;
  using Sitecore.MobileSDK.Session;

  public class ScItemsParser
  {
    private ScItemsParser()
    {
    }

    public static ScItemsResponse Parse(string responseString, CancellationToken cancelToken)
    {
      if (string.IsNullOrEmpty(responseString))
      {
        throw new ArgumentException("response cannot null or empty");
      }

      var response = JToken.Parse(responseString);

      var items = new List<ISitecoreItem>();

      //FIXME: @igk to manny result variants, do we still need universal parser?

      JToken results = null;

      try
      {
        results = response.Value<JToken>("Results");
      }
      catch(Exception e)
      {
        
      }

      if ( results != null)
      {
        response = results;
      }

      if (response is JArray)
      {
        foreach (JObject item in response)
        {
          cancelToken.ThrowIfCancellationRequested();

          ScItem newItem = ScItemsParser.ParseSource(item, cancelToken);
          items.Add(newItem);
        }
      }
      else if (response is JObject)
      {
        ScItem newItem = ScItemsParser.ParseSource(response as JObject, cancelToken);
        items.Add(newItem);
      }

      return new ScItemsResponse(items);
    }

    public static ScItem ParseSource(JObject item, CancellationToken cancelToken)
    {
      ScItem newItem;

      try {
        var source = ParseItemSource(item);

        List<IField> fields = ScFieldsParser.ParseFieldsData(item, cancelToken);
        var fieldsByName = new Dictionary<string, IField>(fields.Count);
        foreach (IField singleField in fields) {
          cancelToken.ThrowIfCancellationRequested();

          string lowercaseName = singleField.Name.ToLowerInvariant();
          fieldsByName[lowercaseName] = singleField;
        }

        newItem = new ScItem(source, fieldsByName);
      } catch (Exception e) { 
        throw new ParserException(TaskFlowErrorMessages.PARSER_EXCEPTION_MESSAGE + item.ToString(), e);
      }

      return newItem;
    }

    private static ItemSource ParseItemSource(JObject json)
    {
      var language = (string)json.GetValue("ItemLanguage");
      var version = (int)json.GetValue("ItemVersion");

      //FIXME: no database field in response!!!
      return new ItemSource(null, language, version);
    }

    private static T ParseOrFail<T>(JObject json, string path)
    {
      JToken objectToken = json.SelectToken(path);
      if (null == objectToken)
      {
        throw new JsonException("Invalid json");
      }

      return objectToken.Value<T>();
    }
  }
}