﻿namespace Sitecore.MobileSDK.UserRequest
{
  using System.Collections.Generic;
  using Sitecore.MobileSDK.API.Request;
  using Sitecore.MobileSDK.API.Request.Paging;
  using Sitecore.MobileSDK.API.Request.Parameters;
  using Sitecore.MobileSDK.Items;
  using Sitecore.MobileSDK.Validators;

  public abstract class AbstractGetVersionedItemRequestBuilder<T> : 
      AbstractScopedRequestParametersBuilder<T>,
      IGetVersionedItemRequestParametersBuilder<T>
  where T : class
  {
    public IGetVersionedItemRequestParametersBuilder<T> Version(int? itemVersion)
    {
      BaseValidator.AssertPositiveNumber(itemVersion, this.GetType().Name + ".Version");

      BaseValidator.CheckForTwiceSetAndThrow(this.itemSourceAccumulator.VersionNumber, this.GetType().Name + ".Version");

      this.itemSourceAccumulator = new ItemSourcePOD(
        this.itemSourceAccumulator.Database,
        this.itemSourceAccumulator.Language,
        itemVersion);
      
      return this;
    }

    //REVIEW: "new" modifier
    #region Compatibility Casts
    new public IGetVersionedItemRequestParametersBuilder<T> Database(string sitecoreDatabase)
    {
      return (IGetVersionedItemRequestParametersBuilder<T>)base.Database(sitecoreDatabase);
    }

    new public IGetVersionedItemRequestParametersBuilder<T> Language(string itemLanguage)
    {
      return (IGetVersionedItemRequestParametersBuilder<T>)base.Language(itemLanguage);
    }

    new public IGetVersionedItemRequestParametersBuilder<T> AddFieldsToRead(IEnumerable<string> fields)
    {
      return (IGetVersionedItemRequestParametersBuilder<T>)base.AddFieldsToRead(fields);
    }

    new public IGetVersionedItemRequestParametersBuilder<T> AddFieldsToRead(params string[] fieldParams)
    {
      return (IGetVersionedItemRequestParametersBuilder<T>)base.AddFieldsToRead(fieldParams);
    }

    new public IPageNumberAccumulator<T> PageNumber(int pageNumber)
    {
      return (IPageNumberAccumulator<T>)base.PageNumber(pageNumber);
    }

    new public IGetVersionedItemRequestParametersBuilder<T> ItemsPerPage(int itemsCountPerPage)
    {
      return (IGetVersionedItemRequestParametersBuilder<T>)base.ItemsPerPage(itemsCountPerPage);
    }

    new public IGetVersionedItemRequestParametersBuilder<T> IncludeStandardTemplateFields(bool include)
    {
      return (IGetVersionedItemRequestParametersBuilder<T>)base.IncludeStandardTemplateFields(include);
    }
    #endregion Compatibility Casts
  }
}

