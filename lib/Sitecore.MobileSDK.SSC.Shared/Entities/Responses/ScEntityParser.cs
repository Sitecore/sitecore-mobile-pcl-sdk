﻿namespace Sitecore.MobileSDK.Entities
{
  using System;
  using System.Collections.Generic;
  using System.Threading;
  using Newtonsoft.Json.Linq;
  using Sitecore.MobileSDK.API.Exceptions;
  using Sitecore.MobileSDK.API.Fields;
  using Sitecore.MobileSDK.Items.Fields;
  using Sitecore.MobileSDK.Session;

  public static class ScEntityParser
  {
    public static ScEntity ParseSource(JObject item, CancellationToken cancelToken)
    {
      ScEntity newItem;

      try {

        List<IField> fields = ScFieldsParser.ParseFieldsData(item, cancelToken);
        var fieldsByName = new Dictionary<string, IField>(fields.Count);
        foreach (IField singleField in fields) {
          cancelToken.ThrowIfCancellationRequested();

          fieldsByName[singleField.Name] = singleField;
        }

        newItem = new ScEntity(fieldsByName);
      } catch (Exception e) {
        OperationCanceledException cancel = e as OperationCanceledException;
        if (cancel != null) {
          throw cancel;
        }

        throw new ParserException(TaskFlowErrorMessages.PARSER_EXCEPTION_MESSAGE + item.ToString(), e);
      }

      return newItem;
    }
  }
}
