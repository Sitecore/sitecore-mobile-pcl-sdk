﻿namespace MobileSDKIntegrationTest
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using NUnit.Framework;
  using Sitecore.MobileSDK.API;
  using Sitecore.MobileSDK.API.Items;
  using Sitecore.MobileSDK.MockObjects;
  using Sitecore.MobileSDK.SessionSettings;

  [TestFixture]
  public class OperationCancelTest
  {
    private ScTestApiSession session;
    private TestEnvironment env;
    private MediaLibrarySettings mediaSettings;

    [SetUp]
    public void SetUp()
    {
      this.env = TestEnvironment.DefaultTestEnvironment();

      this.mediaSettings = new MediaLibrarySettings(
        "/sitecore/media library",
        "ashx",
        "~/media/");

      var config = new SessionConfig(this.env.InstanceUrl);
      var defaultSource = LegacyConstants.DefaultSource();

      this.session = new ScTestApiSession(config, null, this.env.Users.Admin, mediaSettings, defaultSource);
    }

    [TearDown]
    public void TearDown()
    {
      this.session.Dispose();
      this.session = null;
      this.env = null;
      this.mediaSettings = null;
    }

    [Test]
    public void TestCancelExceptionIsNotWrappedForItemByIdRequest()
    {
      TestDelegate testAction = async () =>
      {
        var cancel = new CancellationTokenSource();
        var request = ItemSSCRequestBuilder.ReadItemsRequestWithId(this.env.Items.Home.Id).Build();

        Task<ScItemsResponse> action = this.session.ReadItemAsync(request, cancel.Token);
        cancel.Cancel();

        await action;
      };
      Assert.Catch<OperationCanceledException>(testAction);
    }

    [Test]
    public void TestCancelExceptionIsNotWrappedForItemByPathRequest()
    {
      TestDelegate testAction = async () =>
      {
        var cancel = new CancellationTokenSource();
        var request = ItemSSCRequestBuilder.ReadItemsRequestWithPath("/sitecore/content/home").Build();

        Task<ScItemsResponse> action = this.session.ReadItemAsync(request, cancel.Token);
        cancel.Cancel();

        await action;
      };
      Assert.Catch<OperationCanceledException>(testAction);
    }

  }
}

