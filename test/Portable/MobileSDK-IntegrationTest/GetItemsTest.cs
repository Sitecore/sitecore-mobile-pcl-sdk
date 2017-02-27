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

  [TestFixture]
  public class GetItemsTest
  {
    private TestEnvironment testData;
    private ISitecoreSSCReadonlySession sessionAuthenticatedUser;

    private const string ItemWithSpacesPath = "/sitecore/content/Home/Android/Static/Test item 1";
    private const string ItemWithSpacesName = "Test item 1";

    [SetUp]
    public void Setup()
    {
      HttpClientHandler handler = new HttpClientHandler();
      HttpClient httpClient = new HttpClient(handler);

      testData = TestEnvironment.DefaultTestEnvironment();
      this.sessionAuthenticatedUser =
        SitecoreSSCSessionBuilder.AuthenticatedSessionWithHost(this.testData.InstanceUrl)
          .Credentials(this.testData.Users.Admin)
          .BuildReadonlySession(handler, httpClient);
    }

    [TearDown]
    public void TearDown()
    {
      this.sessionAuthenticatedUser.Dispose();
      this.sessionAuthenticatedUser = null;
      this.testData = null;
    }

    [Test]
    public async void TestGetItemById()
    {
      var response = await GetItemById(this.testData.Items.Home.Id);

      testData.AssertItemsCount(1, response);
      testData.AssertItemsAreEqual(testData.Items.Home, response[0]);
    }

    [Test]
    public void TestGetItemByNotExistentId()
    {
      const string NotExistentId = "{3D6658D8-QQQQ-QQQQ-B3E2-D050FABCF4E1}";

      TestDelegate testCode = async () => {
        await GetItemById(NotExistentId);
      };

      var exception = Assert.Throws<ParserException>(testCode);
      Assert.IsTrue(exception.Message.Contains("unexpected format"));
    }

    [Test]
    public void TestGetItemByIdWithPathInParamsReturnsError()
    {
      Exception exception = Assert.Throws<ArgumentException>(() => ItemSSCRequestBuilder.ReadItemsRequestWithId(testData.Items.Home.Path).Build());
      Assert.AreEqual("ReadItemByIdRequestBuilder.ItemId : wrong item id", exception.Message);
    }

    [Test]
    public async void TestGetItemByPath()
    {
      var response = await GetItemByPath(testData.Items.Home.Path);

      testData.AssertItemsCount(1, response);
      testData.AssertItemsAreEqual(testData.Items.Home, response[0]);
    }

    [Test]
    public async void TestGetItemsChildrens()
    {
      var request = ItemSSCRequestBuilder.ReadChildrenRequestWithId(testData.Items.Home.Id).Build();

      var response = await this.sessionAuthenticatedUser.ReadChildrenAsync(request);

      testData.AssertItemsCount(4, response);
    }

    [Test]
    public async void TestGetItemsChildrensWithEmptyResult()
    {
      var request = ItemSSCRequestBuilder.ReadChildrenRequestWithId(testData.Items.TestFieldsItem.Id).Build();

      var response = await this.sessionAuthenticatedUser.ReadChildrenAsync(request);

      testData.AssertItemsCount(0, response);
    }

    [Test]
    public async void TestGetItemByPathWithSpaces()
    {
      var response = await GetItemByPath(ItemWithSpacesPath);

      testData.AssertItemsCount(1, response);
      var expectedItem = new TestEnvironment.Item
      {
        DisplayName = ItemWithSpacesName,
        Path = ItemWithSpacesPath,
        TemplateId = testData.Items.Home.TemplateId
      };
      testData.AssertItemsAreEqual(expectedItem, response[0]);
    }

    [Test]
    public async void TestGetItemByPathForTwoItemsWithTheSamePathExist()
    {
      var response = await GetItemByPath(ItemWithSpacesPath);

      testData.AssertItemsCount(1, response);
      var expectedItem = new TestEnvironment.Item
      {
        DisplayName = ItemWithSpacesName,
        Path = ItemWithSpacesPath,
        TemplateId = testData.Items.Home.TemplateId
      };
      testData.AssertItemsAreEqual(expectedItem, response[0]);

    }

    [Test]
    public void TestGetItemByNotExistentPath()
    {
      const string PathNotExistent = "/not/existent/path";

      TestDelegate testCode = async () => {
        await GetItemByPath(PathNotExistent);
      };

      var exception = Assert.Throws<ParserException>(testCode);
      Assert.IsTrue(exception.Message.Contains("unexpected format"));
    }

    [Test]
    public async void TestGetItemByPathWithInternationalName()
    {
      const string ItemInterationalPath = "/sitecore/content/Home/Android/Static/Japanese/宇都宮";
      var response = await GetItemByPath(ItemInterationalPath);
      testData.AssertItemsCount(1, response);
      var expectedItem = new TestEnvironment.Item
      {
        DisplayName = "宇都宮",
        Path = ItemInterationalPath,
        TemplateId = testData.Items.Home.TemplateId
      };
      testData.AssertItemsAreEqual(expectedItem, response[0]);
    }

    [Test]
    public async void TestGetItemByInternationalPath()
    {
      const string ItemInterationalPath = "/sitecore/content/Home/Android/Static/Japanese/宇都宮/ではまた明日";
      var response = await GetItemByPath(ItemInterationalPath);
      var expectedItem = new TestEnvironment.Item {
        DisplayName = "ではまた明日",
        Path = ItemInterationalPath,
        TemplateId = testData.Items.Home.TemplateId
      };
      testData.AssertItemsAreEqual(expectedItem, response[0]);
    }

    [Test]
    public void TestGetItemByNullIdReturnsError()
    {
      TestDelegate testCode = async () =>
      {
        var task = this.GetItemById(null);
        await task;
      };

      var exception = Assert.Throws<ArgumentNullException>(testCode);
      Assert.IsTrue(exception.Message.Contains("ReadItemByIdRequestBuilder.ItemId"));
    }

    [Test]
    public void TestGetItemByNullPathReturnsError()
    {
      TestDelegate testCode = async () =>
      {
        var task = this.GetItemByPath(null);
        await task;
      };

      var exception = Assert.Throws<ArgumentNullException>(testCode);
      Assert.IsTrue(exception.Message.Contains("ReadItemByPathRequestBuilder.ItemPath"));
    }

    [Test]
    public void TestGetItemByEmptyPathReturnsError()
    {
      TestDelegate testCode = async () =>
      {
        var task = this.GetItemByPath("");
        await task;
      };

      var exception = Assert.Throws<ArgumentException>(testCode);
      Assert.AreEqual("ReadItemByPathRequestBuilder.ItemPath : The input cannot be empty.", exception.Message);
    }

    [Test]
    public void TestGetItemByIdWithSpacesOnlyReturnsError()
    {
      TestDelegate testCode = async () => {
        var task = this.GetItemById(" ");
        await task;
      };

      var exception = Assert.Throws<ArgumentException>(testCode);
      Assert.AreEqual("ReadItemByIdRequestBuilder.ItemId : The input cannot be empty.", exception.Message);
    }

    [Test]
    public void TestGetItemByPathWithSpacesOnlyReturnsError()
    {
      TestDelegate testCode = async () =>
      {
        var task = this.GetItemByPath("  ");
        await task;
      };

      var exception = Assert.Throws<ArgumentException>(testCode);
      Assert.AreEqual("ReadItemByPathRequestBuilder.ItemPath : The input cannot be empty.", exception.Message);
    }

    [Test]
    public void TestGetItemByPathWithUserWithoutReadAccessToHomeItem()
    {

      HttpClientHandler handler = new HttpClientHandler();
      HttpClient httpClient = new HttpClient(handler);

      var sessionWithoutAccess =
        SitecoreSSCSessionBuilder.AuthenticatedSessionWithHost(this.testData.InstanceUrl)
          .Credentials(this.testData.Users.NoReadUserExtranet)
          .BuildReadonlySession(handler, httpClient);

      var request = ItemSSCRequestBuilder.ReadItemsRequestWithPath(this.testData.Items.Home.Path).Build();

      TestDelegate testCode = async () => {
        await sessionWithoutAccess.ReadItemAsync(request);
      };

      var exception = Assert.Throws<ParserException>(testCode);
      Assert.IsTrue(exception.Message.Contains("unexpected format"));
    }

    private async Task<ScItemsResponse> GetItemById(string id)
    {
      var request = ItemSSCRequestBuilder.ReadItemsRequestWithId(id).Build();
      var response = await this.sessionAuthenticatedUser.ReadItemAsync(request);
      return response;
    }

    private async Task<ScItemsResponse> GetItemByPath(string itemPath)
    {
      var request = ItemSSCRequestBuilder.ReadItemsRequestWithPath(itemPath).Build();
      var response = await this.sessionAuthenticatedUser.ReadItemAsync(request);
      return response;
    }



  }
}