﻿namespace MobileSDKIntegrationTest
{
  using System;
  using System.Net.Http;
  using System.Threading.Tasks;
  using NUnit.Framework;
  using Sitecore.MobileSDK.API;
  using Sitecore.MobileSDK.API.Exceptions;
  using Sitecore.MobileSDK.API.Items;
  using Sitecore.MobileSDK.API.Session;
  using Sitecore.MobileSDK.MockObjects;

  [TestFixture]
  public class UpdateItemsTest
  {
    private TestEnvironment testData;
    private ISitecoreSSCSession session;
    private ISitecoreSSCSession noThrowCleanupSession;
    private const string SampleId = "{SAMPLEID-7808-4798-A461-1FB3EB0A43E5}";

    [SetUp]
    public void SetupSession()
    {
      this.testData = TestEnvironment.DefaultTestEnvironment();
      this.session = this.CreateSession();

      // Same as this.session
      var cleanupSession = this.CreateSession();

      this.noThrowCleanupSession = new NoThrowSSCSession(cleanupSession);
    }

    private ISitecoreSSCSession CreateSession()
    {
      HttpClientHandler handler = new HttpClientHandler();
      HttpClient httpClient = new HttpClient(handler);

      return SitecoreSSCSessionBuilder.AuthenticatedSessionWithHost(testData.InstanceUrl)
        .Credentials(testData.Users.Admin)
        .DefaultDatabase("master")
        .BuildSession(handler, httpClient);
    }


    private async Task RemoveAll()
    {
      await this.DeleteAllItems("master");
      await this.DeleteAllItems("web");
    }

    [TearDown]
    public void TearDown()
    {
      this.testData = null;

      this.session.Dispose();
      this.session = null;

      this.noThrowCleanupSession.Dispose();
      this.noThrowCleanupSession = null;
    }

   

    [Test]
    public async void TestUpdateItemByNotExistentId()
    {
      var textValue = RandomText();

      var request = ItemSSCRequestBuilder.UpdateItemRequestWithId(SampleId)
        .AddFieldsRawValuesByNameToSet("Text", textValue)
        .Build();

      var response = await this.session.UpdateItemAsync(request);

      Assert.AreEqual(405, response.StatusCode);

    }

    [Test]
    public void TestUpdateItemByInvalidIdReturnsException()
    {
      var exception = Assert.Throws<ArgumentException>(() => ItemSSCRequestBuilder.UpdateItemRequestWithId(testData.Items.Home.Path)
        .Build());
      Assert.AreEqual("UpdateItemByIdRequestBuilder.ItemId : wrong item id", exception.Message);
    }

    [Test]
    public void TestUpdateItemByEmptyIdReturnsException()
    {
      var exception = Assert.Throws<ArgumentException>(() => ItemSSCRequestBuilder.UpdateItemRequestWithId("")
        .Build());
      Assert.AreEqual("UpdateItemByIdRequestBuilder.ItemId : The input cannot be empty.", exception.Message);
    }

    [Test]
    public void TestUpdateItemByIdWithNullDatabaseDoNotReturnsException()
    {
      var request = ItemSSCRequestBuilder.UpdateItemRequestWithId(SampleId)
        .Database(null)
        .Build();
      Assert.IsNotNull(request);
    }

    [Test]
    public void TestUpdateItemByIdWithSpacesOnlyInLanguageReturnsException()
    {
      var exception = Assert.Throws<ArgumentException>(() => ItemSSCRequestBuilder.UpdateItemRequestWithId(SampleId)
        .Language("  ")
        .Build());
      Assert.AreEqual("UpdateItemByIdRequestBuilder.Language : The input cannot be empty.", exception.Message);
    }

    [Test]
    public void TestUpdateItemByIdWithNullReadFieldsReturnsException()
    {
      var exception = Assert.Throws<ArgumentNullException>(() => ItemSSCRequestBuilder.UpdateItemRequestWithId(SampleId)
        .AddFieldsToRead(null)
        .Build());
      Assert.IsTrue(exception.Message.Contains("UpdateItemByIdRequestBuilder.Fields"));
    }

    [Test]
    public void TestUpdateItemByIdWithDuplicateFieldsToUpdateReturnsException()
    {
      var exception = Assert.Throws<InvalidOperationException>(() => ItemSSCRequestBuilder.UpdateItemRequestWithId(SampleId)
        .AddFieldsRawValuesByNameToSet("Title", "Value1")
        .AddFieldsRawValuesByNameToSet("Title", "Value2")
        .Build());
      Assert.AreEqual("UpdateItemByIdRequestBuilder.FieldsRawValuesByName : duplicate fields are not allowed", exception.Message);
    }

    [Test]
    public void TestUpdateItemByIdWithTwoVersionsReturnsException()
    {
      var exception = Assert.Throws<InvalidOperationException>(() => ItemSSCRequestBuilder.UpdateItemRequestWithId(SampleId)
        .Version(1)
        .Version(2)
        .Build());
      Assert.AreEqual("UpdateItemByIdRequestBuilder.Version : Property cannot be assigned twice.", exception.Message);
    }

    [Test]
    public async void TestUpdateItemVersion1ById()
    {
      await this.RemoveAll();
      const int Version = 1;
      var textValue = RandomText();

      var request = ItemSSCRequestBuilder.UpdateItemRequestWithId(testData.Items.ItemWithVersions.Id)
        .AddFieldsRawValuesByNameToSet("Text", textValue+"123")
        .Version(Version)
        .Build();

      var result = await this.session.UpdateItemAsync(request);

      Assert.IsTrue(result.Updated);
    }

    private async Task<ISitecoreItem> CreateItem(string itemName, ISitecoreItem parentItem = null, ISitecoreSSCSession itemSession = null)
    {
      if (itemSession == null) {
        itemSession = session;
      }
      string parentPath = parentItem == null ? this.testData.Items.CreateItemsHere.Path : parentItem.Path;
      var request = ItemSSCRequestBuilder.CreateItemRequestWithParentPath(parentPath)
        .ItemTemplateId(testData.Items.Home.TemplateId)
        .ItemName(itemName)
        .Build();
      
      var createResponse = await itemSession.CreateItemAsync(request);

      Assert.IsTrue(createResponse.Created);

      var readRequest = ItemSSCRequestBuilder.ReadItemsRequestWithPath(this.testData.Items.CreateItemsHere.Path + "/" + itemName)
                                         .Build();

      var readResponse = await itemSession.ReadItemAsync(readRequest);

      return readResponse[0];
    }

    private static string RandomText()
    {
      return "UpdatedText" + new Random(10000);
    }

    private async Task DeleteAllItems(string database)
    {
      var getItemsToDelet = ItemSSCRequestBuilder.ReadChildrenRequestWithId(this.testData.Items.CreateItemsHere.Id)
          .Database(database)
          .Build();

      ScItemsResponse items = await this.noThrowCleanupSession.ReadChildrenAsync(getItemsToDelet);

      if (items != null && items.ResultCount > 0)
      {
        foreach (var item in items) {

          var deleteFromMaster = ItemSSCRequestBuilder.DeleteItemRequestWithId(item.Id)
            .Database(database)
            .Build();
          await this.noThrowCleanupSession.DeleteItemAsync(deleteFromMaster);
        }
      }
    }
  }
}