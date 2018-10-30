

namespace Sitecore.MobileSDK
{
  using System;
  using System.Net;
  using System.IO;
  using System.Net.Http;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Linq;
  using System.Diagnostics;
  using System.Collections.Generic;

  using Sitecore.MobileSDK.API;
  using Sitecore.MobileSDK.API.Exceptions;
  using Sitecore.MobileSDK.API.Items;
  using Sitecore.MobileSDK.API.Request;
  using Sitecore.MobileSDK.API.Session;
  using Sitecore.MobileSDK.API.MediaItem;
  using Sitecore.MobileSDK.PasswordProvider.Interface;
  using Sitecore.MobileSDK.Items;
  using Sitecore.MobileSDK.PublicKey;
  using Sitecore.MobileSDK.TaskFlow;
  using Sitecore.MobileSDK.CrudTasks;
  using Sitecore.MobileSDK.CrudTasks.Resource;
  using Sitecore.MobileSDK.Validators;
  using Sitecore.MobileSDK.SessionSettings;
  using Sitecore.MobileSDK.UserRequest;
  using Sitecore.MobileSDK.UrlBuilder.Rest;
  using Sitecore.MobileSDK.UrlBuilder.SSC;
  using Sitecore.MobileSDK.UrlBuilder.ItemById;
  using Sitecore.MobileSDK.UrlBuilder.MediaItem;
  using Sitecore.MobileSDK.UrlBuilder.ItemByPath;
  using Sitecore.MobileSDK.UrlBuilder.CreateItem;
  using Sitecore.MobileSDK.UrlBuilder.UpdateItem;
  using Sitecore.MobileSDK.UrlBuilder.DeleteItem;
  using Sitecore.MobileSDK.UrlBuilder.Children;
  using Sitecore.MobileSDK.UrlBuilder.Search;
  using Sitecore.MobileSDK.API.Entities;
  using Sitecore.MobileSDK.UrlBuilder.Entity;
  using Sitecore.MobileSDK.CrudTasks.Entity;
  using Sitecore.MobileSDK.API.Request.Entity;

  public class ScApiSession : ISitecoreSSCSession
  {
    public ScApiSession(
      ISessionConfig config,
      IEntitySource entitySource,
      IScCredentials credentials,
      IMediaLibrarySettings mediaSettings,
      ItemSource defaultSource = null)
    {
      if (null == config)
      {
        throw new ArgumentNullException("ScApiSession.config cannot be null");
      }

      if (entitySource != null)
      { 
         this.entitySource = entitySource.ShallowCopy();
      }

      this.sessionConfig = config.SessionConfigShallowCopy();
      this.requestMerger = new UserRequestMerger(this.sessionConfig, defaultSource, this.entitySource);

      if (null != credentials)
      {
        this.credentials = credentials.CredentialsShallowCopy();
      }

      if (null != mediaSettings)
      {
        this.mediaSettings = mediaSettings.MediaSettingsShallowCopy();
      }

      this.cookies = new CookieContainer();
      this.handler = new HttpClientHandler();
      this.handler.CookieContainer = cookies;
      this.httpClient = new HttpClient(this.handler);
        }

    #region IDisposable
    void ReleaseResources()
    {
      Exception credentialsException = null;
      Exception httpClientException = null;

      if (null != this.credentials)
      {
        try
        {
          this.credentials.Dispose();
        }
        catch (Exception ex)
        {
          credentialsException = ex;
        }
        this.credentials = null;
      }

      if (null != this.httpClient)
      {
        try
        {
          this.httpClient.Dispose();
        }
        catch (Exception ex)
        {
          httpClientException = ex;
        }
        this.httpClient = null;
      }

      if (null != credentialsException)
      {
        throw credentialsException;
      }
      else if (null != httpClientException)
      {
        throw httpClientException;
      }
    }
      
    public virtual void Dispose()
    {
      this.ReleaseResources();
    }

    ~ScApiSession() 
    {

    }
    #endregion IDisposable

    #region ISitecoreSSCSessionState
    public IItemSource DefaultSource
    {
      get
      {
        return this.requestMerger.ItemSourceMerger.DefaultSource;
      }
    }

    public ISessionConfig Config
    {
      get
      {
        return this.sessionConfig;
      }
    }

    public IScCredentials Credentials
    {
      get
      {
        return this.credentials;
      }
    }

    public IMediaLibrarySettings MediaLibrarySettings 
    { 
      get
      {
        return this.mediaSettings;
      }
    }
    #endregion

    #region Forbidden Methods

    private ScApiSession()
    {
    }

    #endregion Forbidden Methods

    #region Encryption

    protected virtual async Task<ScAuthResponse> GetPublicKeyAsync(CancellationToken cancelToken = default(CancellationToken))
    {
      ScAuthResponse response = null;

      if (this.credentials != null) {

        string url = SessionConfigValidator.AutocompleteInstanceUrlForcingHttps(this.Config.InstanceUrl);
        IEnumerable<Cookie> prevCookies = this.cookies.GetCookies(new Uri(url)).Cast<Cookie>();
        bool noCookies = true;

        if (prevCookies.Count() > 0) {
          noCookies = this.CookiesExpired(prevCookies);
        }

        if (noCookies) {

          try {
            var sessionConfigBuilder = new SessionConfigUrlBuilder(this.restGrammar, this.sscGrammar);
            var taskFlow = new GetPublicKeyTasks(this.credentials, sessionConfigBuilder, this.sscGrammar, this.httpClient);

            response = await RestApiCallFlow.LoadRequestFromNetworkFlow(this.sessionConfig, taskFlow, cancelToken);

            IEnumerable<Cookie> newCookies = this.cookies.GetCookies(new Uri(url)).Cast<Cookie>();
            Debug.WriteLine(newCookies.ToString());

          } catch (ObjectDisposedException) {
            // CancellationToken.ThrowIfCancellationRequested()
            throw;
          } catch (OperationCanceledException) {
            // CancellationToken.ThrowIfCancellationRequested()
            // and TaskCanceledException
            throw;
          } catch (SitecoreMobileSdkException ex) {
            // throw unwrapped exception as if GetPublicKeyAsync() is an atomic phase
            throw new RsaHandshakeException("[Sitecore Mobile SDK] ASPXAUTH not received properly", ex.InnerException);
          } catch (Exception ex) {
            throw new RsaHandshakeException("[Sitecore Mobile SDK] ASPXAUTH not received properly", ex);
          }

        } else {
          response = new ScAuthResponse("200");
        }
      }

      return response;

    }

    private bool CookiesExpired(IEnumerable<Cookie> cookies)
    {
      foreach (Cookie cookie in cookies) {
        if (cookie.Expired) {
          return true;
        }
      }

      return false;
    }

    #endregion Encryption

    #region SearchItems

    public async Task<ScItemsResponse> RunSearchAsync(ISitecoreStoredSearchRequest request, CancellationToken cancelToken = default(CancellationToken))
    {
      ISitecoreStoredSearchRequest requestCopy = request.DeepCopySitecoreStoredSearchRequest();

      await this.GetPublicKeyAsync(cancelToken);

      ISitecoreStoredSearchRequest autocompletedRequest = this.requestMerger.FillSitecoreStoredSearchGaps(requestCopy);

      var urlBuilder = new RunStoredSearchUrlBuilder(this.restGrammar, this.sscGrammar);
      var taskFlow = new RunStoredSearchTasks(urlBuilder, this.httpClient);

      return await RestApiCallFlow.LoadRequestFromNetworkFlow(autocompletedRequest, taskFlow, cancelToken);
    }

    public async Task<ScItemsResponse> RunSearchAsync(ISitecoreSearchRequest request, CancellationToken cancelToken = default(CancellationToken))
    {
      ISitecoreSearchRequest requestCopy = request.DeepCopySitecoreSearchRequest();

      await this.GetPublicKeyAsync(cancelToken);

      ISitecoreSearchRequest autocompletedRequest = this.requestMerger.FillSitecoreSearchGaps(requestCopy);

      var urlBuilder = new RunSitecoreSearchUrlBuilder(this.restGrammar, this.sscGrammar);
      var taskFlow = new RunSitecoreSearchTasks(urlBuilder, this.httpClient);

      return await RestApiCallFlow.LoadRequestFromNetworkFlow(autocompletedRequest, taskFlow, cancelToken);
    }

    public async Task<ScItemsResponse> RunStoredQuerryAsync(ISitecoreStoredSearchRequest request, CancellationToken cancelToken = default(CancellationToken))
    {
      ISitecoreStoredSearchRequest requestCopy = request.DeepCopySitecoreStoredSearchRequest();

      await this.GetPublicKeyAsync(cancelToken);

      ISitecoreStoredSearchRequest autocompletedRequest = this.requestMerger.FillSitecoreStoredSearchGaps(requestCopy);

      var urlBuilder = new RunStoredQuerryUrlBuilder(this.restGrammar, this.sscGrammar);
      var taskFlow = new RunStoredQuerryTasks(urlBuilder, this.httpClient);

      return await RestApiCallFlow.LoadRequestFromNetworkFlow(autocompletedRequest, taskFlow, cancelToken);
    }

    #endregion SearchItems

    #region Entity

    public async Task<ScEntityResponse> ReadEntityAsync(IReadEntitiesByPathRequest request, CancellationToken cancelToken = default(CancellationToken))
    {
      IReadEntitiesByPathRequest requestCopy = request.DeepCopyReadEntitiesByPathRequest();

      //await this.GetPublicKeyAsync(cancelToken);
      IReadEntitiesByPathRequest autocompletedRequest = this.requestMerger.FillReadEntitiesByPathGaps(requestCopy);

      var urlBuilder = new GetEntitiesUrlBuilder<IReadEntitiesByPathRequest>(this.restGrammar, this.sscGrammar);
      var taskFlow = new GetEntitiesByPathTask(urlBuilder, this.httpClient);

      return await RestApiCallFlow.LoadRequestFromNetworkFlow(autocompletedRequest, taskFlow, cancelToken);
    }

    public async Task<ScEntityResponse> ReadEntityAsync(IReadEntityByIdRequest request, CancellationToken cancelToken = default(CancellationToken))
    {
      IReadEntityByIdRequest requestCopy = request.DeepCopyReadEntitiesByIdRequest();

      //await this.GetPublicKeyAsync(cancelToken);
      IReadEntityByIdRequest autocompletedRequest = this.requestMerger.FillReadEntityByIdGaps(requestCopy);

      var urlBuilder = new EntityByIdUrlBuilder<IReadEntityByIdRequest>(this.restGrammar, this.sscGrammar);
      var taskFlow = new GetEntityByIdTask(urlBuilder, this.httpClient);

      return await RestApiCallFlow.LoadRequestFromNetworkFlow(autocompletedRequest, taskFlow, cancelToken);
    }

    public async Task<ScCreateEntityResponse> CreateEntityAsync(ICreateEntityRequest request, CancellationToken cancelToken = default(CancellationToken))
    {
      ICreateEntityRequest requestCopy = request.DeepCopyCreateEntityRequest();

      //await this.GetPublicKeyAsync(cancelToken);

      ICreateEntityRequest autocompletedRequest = this.requestMerger.FillCreateEntityGaps(requestCopy);

      var urlBuilder = new GetEntitiesUrlBuilder<ICreateEntityRequest>(this.restGrammar, this.sscGrammar);
      var taskFlow = new CreateEntityTask<ICreateEntityRequest>(urlBuilder, this.httpClient);

      return await RestApiCallFlow.LoadRequestFromNetworkFlow(autocompletedRequest, taskFlow, cancelToken);
    }

    public async Task<ScUpdateEntityResponse> UpdateEntityAsync(IUpdateEntityRequest request, CancellationToken cancelToken = default(CancellationToken))
    {
      IUpdateEntityRequest requestCopy = request.DeepCopyUpdateEntityRequest();

      //await this.GetPublicKeyAsync(cancelToken);

      IUpdateEntityRequest autocompletedRequest = this.requestMerger.FillUpdateEntityGaps(requestCopy);

      var urlBuilder = new EntityByIdUrlBuilder<IUpdateEntityRequest>(this.restGrammar, this.sscGrammar);
      var taskFlow = new UpdateEntityTask<IUpdateEntityRequest>(urlBuilder, this.httpClient);

      return await RestApiCallFlow.LoadRequestFromNetworkFlow(autocompletedRequest, taskFlow, cancelToken);
    }

    public async Task<ScDeleteEntityResponse> DeleteEntityAsync(IDeleteEntityRequest request, CancellationToken cancelToken = default(CancellationToken))
    {
      IDeleteEntityRequest requestCopy = request.DeepCopyDeleteEntityRequest();

      //await this.GetPublicKeyAsync(cancelToken);

      IDeleteEntityRequest autocompletedRequest = this.requestMerger.FillDeleteEntityGaps(requestCopy);

      var urlBuilder = new EntityByIdUrlBuilder<IDeleteEntityRequest>(this.restGrammar, this.sscGrammar);
      var taskFlow = new DeleteEntityTask(urlBuilder, this.httpClient);

      return await RestApiCallFlow.LoadRequestFromNetworkFlow(autocompletedRequest, taskFlow, cancelToken);
    }

    #endregion Entity

    #region GetItems

    public async Task<ScItemsResponse> ReadItemAsync(IReadItemsByPathRequest request, CancellationToken cancelToken = default(CancellationToken))
    {
      IReadItemsByPathRequest requestCopy = request.DeepCopyGetItemByPathRequest();

      await this.GetPublicKeyAsync(cancelToken);
      IReadItemsByPathRequest autocompletedRequest = this.requestMerger.FillReadItemByPathGaps(requestCopy);

      var urlBuilder = new ItemByPathUrlBuilder(this.restGrammar, this.sscGrammar);
      var taskFlow = new GetItemsByPathTasks(urlBuilder, this.httpClient);

      return await RestApiCallFlow.LoadRequestFromNetworkFlow(autocompletedRequest, taskFlow, cancelToken);
    }

    public async Task<ScItemsResponse> ReadChildrenAsync(IReadItemsByIdRequest request, CancellationToken cancelToken = default(CancellationToken))
    {
      IReadItemsByIdRequest requestCopy = request.DeepCopyGetItemByIdRequest();

      await this.GetPublicKeyAsync(cancelToken);

      IReadItemsByIdRequest autocompletedRequest = this.requestMerger.FillReadItemByIdGaps(requestCopy);

      var urlBuilder = new ChildrenByIdUrlBuilder(this.restGrammar, this.sscGrammar);
      var taskFlow = new GetChildrenByIdTasks(urlBuilder, this.httpClient);

      return await RestApiCallFlow.LoadRequestFromNetworkFlow(autocompletedRequest, taskFlow, cancelToken);
    }

    public async Task<ScItemsResponse> ReadItemAsync(IReadItemsByIdRequest request, CancellationToken cancelToken = default(CancellationToken))
    {
      IReadItemsByIdRequest requestCopy = request.DeepCopyGetItemByIdRequest();

      await this.GetPublicKeyAsync(cancelToken);
      IReadItemsByIdRequest autocompletedRequest = this.requestMerger.FillReadItemByIdGaps(requestCopy);

      var urlBuilder = new ItemByIdUrlBuilder(this.restGrammar, this.sscGrammar);
      var taskFlow = new GetItemsByIdTasks(urlBuilder, this.httpClient);

      return await RestApiCallFlow.LoadRequestFromNetworkFlow(autocompletedRequest, taskFlow, cancelToken);
    }

    public async Task<Stream> DownloadMediaResourceAsync(IMediaResourceDownloadRequest request, CancellationToken cancelToken = default(CancellationToken))
    {
      IMediaResourceDownloadRequest requestCopy = request.DeepCopyReadMediaRequest();

      await this.GetPublicKeyAsync(cancelToken);

      IMediaResourceDownloadRequest autocompletedRequest = this.requestMerger.FillReadMediaItemGaps(requestCopy);

      MediaItemUrlBuilder urlBuilder = new MediaItemUrlBuilder(
        this.restGrammar,
        this.sscGrammar,
        this.sessionConfig,
        this.mediaSettings,
        autocompletedRequest.ItemSource);

      var taskFlow = new GetResourceTask(urlBuilder, this.httpClient);
      return await RestApiCallFlow.LoadResourceFromNetworkFlow(autocompletedRequest, taskFlow, cancelToken);
    }
    #endregion GetItems

    #region CreateItems

    public async Task<ScCreateItemResponse> CreateItemAsync(ICreateItemByPathRequest request, CancellationToken cancelToken = default(CancellationToken))
    {
      ICreateItemByPathRequest requestCopy = request.DeepCopyCreateItemByPathRequest();
     
      await this.GetPublicKeyAsync(cancelToken);

      ICreateItemByPathRequest autocompletedRequest = this.requestMerger.FillCreateItemByPathGaps(requestCopy);

      var urlBuilder = new CreateItemByPathUrlBuilder(this.restGrammar, this.sscGrammar);
      var taskFlow = new CreateItemByPathTask<ICreateItemByPathRequest>(urlBuilder, this.httpClient);

      return await RestApiCallFlow.LoadRequestFromNetworkFlow(autocompletedRequest, taskFlow, cancelToken);
    }

    #endregion CreateItems

    #region Update Items

    public async Task<ScUpdateItemResponse> UpdateItemAsync(IUpdateItemByIdRequest request, CancellationToken cancelToken = default(CancellationToken))
    {
      IUpdateItemByIdRequest requestCopy = request.DeepCopyUpdateItemByIdRequest();

      await this.GetPublicKeyAsync(cancelToken);

      IUpdateItemByIdRequest autocompletedRequest = this.requestMerger.FillUpdateItemByIdGaps(requestCopy);

      var urlBuilder = new UpdateItemByIdUrlBuilder(this.restGrammar, this.sscGrammar);
      var taskFlow = new UpdateItemByIdTask(urlBuilder, this.httpClient);

      return await RestApiCallFlow.LoadRequestFromNetworkFlow(autocompletedRequest, taskFlow, cancelToken);
    }

    #endregion Update Items

    #region DeleteItems

    public async Task<ScDeleteItemsResponse> DeleteItemAsync(IDeleteItemsByIdRequest request, CancellationToken cancelToken = default(CancellationToken))
    {
      IDeleteItemsByIdRequest requestCopy = request.DeepCopyDeleteItemRequest();

      await this.GetPublicKeyAsync(cancelToken);

      IDeleteItemsByIdRequest autocompletedRequest = this.requestMerger.FillDeleteItemByIdGaps(requestCopy);

      var urlBuilder = new DeleteItemByIdUrlBuilder(this.restGrammar, this.sscGrammar);
      var taskFlow = new DeleteItemTasks<IDeleteItemsByIdRequest>(urlBuilder, this.httpClient);

      return await RestApiCallFlow.LoadRequestFromNetworkFlow(autocompletedRequest, taskFlow, cancelToken);
    }

    #endregion DeleteItems

    #region Authentication

    public async Task<ScAuthResponse> AuthenticateAsync(CancellationToken cancelToken = default(CancellationToken))
    {
      var result = await this.GetPublicKeyAsync(cancelToken);

      return result;
    }

    #endregion Authentication

    #region Private Variables

    private readonly UserRequestMerger requestMerger;
    private HttpClient httpClient;
    private CookieContainer cookies;
    private HttpClientHandler handler;

    protected readonly ISessionConfig sessionConfig;
    protected readonly IEntitySource entitySource;
    private IScCredentials credentials;
    private readonly IMediaLibrarySettings mediaSettings;

    private readonly IRestServiceGrammar restGrammar = RestServiceGrammar.ItemSSCV2Grammar();
    private readonly ISSCUrlParameters sscGrammar =SSCUrlParameters.ItemSSCV2UrlParameters();

    #endregion Private Variables
  }
}
