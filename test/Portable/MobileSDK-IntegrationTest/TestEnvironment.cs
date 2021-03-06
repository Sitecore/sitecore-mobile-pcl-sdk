﻿namespace MobileSDKIntegrationTest
{
  using NUnit.Framework;
  using Sitecore.MobileSDK.API.Items;
  using Sitecore.MobileSDK.MockObjects;

  public class TestEnvironment
  {
    public static TestEnvironment DefaultTestEnvironment()
    {
      var result = new TestEnvironment
      {
        InstanceUrl = TestEndpointsConfig.InstanceUrl,

      };

      result.Items.Home.Id = "110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9";
      result.Items.Home.Path = "/sitecore/content/Home";
      result.Items.Home.DisplayName = "Home";
      result.Items.Home.TemplateId = "76036F5E-CBCE-46D1-AF0A-4143F9B557AA";

      result.Items.ItemWithVersions.Id = "B86C2CBB-7808-4798-A461-1FB3EB0A43E5";
      result.Items.ItemWithVersions.Path = "/sitecore/content/FieldsTest/TestFieldsVersionsAndDB";
      result.Items.ItemWithVersions.DisplayName = "TestFieldsVersionsAndDB";

      result.Items.TestFieldsItem.Id = "00CB2AC4-70DB-482C-85B4-B1F3A4CFE643";
      result.Items.TestFieldsItem.Path = "/sitecore/content/Home/Test Fields";
      result.Items.TestFieldsItem.DisplayName = "Test Fields";
      result.Items.TestFieldsItem.TemplateId = "5FC0D542-E27B-4E55-A1F0-702E959DCD6C";

      result.Items.AllowedItem.Id = "387B69B2-B2EA-4618-8C3E-2785DC0469A7";
      result.Items.AllowedItem.Path = "/sitecore/content/Home/Allowed_Parent/Allowed_Item";
      result.Items.AllowedItem.DisplayName = "Allowed_Item";

      result.Items.AllowedParent.Id = "2075CBFF-C330-434D-9E1B-937782E0DE49";
      result.Items.AllowedParent.Path = "/sitecore/content/Home/Allowed_Parent";
      result.Items.AllowedParent.DisplayName = "Allowed_Parent";

      result.Items.CreateItemsHere.Id = "C50613DC-D792-467C-832F-F93BB121D775";
      result.Items.CreateItemsHere.Path = "/sitecore/content/Home/Android/Folder for create items";
      result.Items.CreateItemsHere.DisplayName = "Folder for create items";

      result.Items.MediaImagesItem.Id = "15451229-7534-44EF-815D-D93D6170BFCB";
      result.Items.MediaImagesItem.Path = "/sitecore/media library/Images";
      result.Items.MediaImagesItem.TemplateId = "FE5DD826-48C6-436D-B87A-7C4210C7413B";

      result.Items.UploadMediaHere.Id = "EFBA81CC-69A3-4E32-BADB-379B6C347437";
      result.Items.UploadMediaHere.Path = "/Test Data/Create Edit Delete Media";

      return result;
    }

    private TestEnvironment() { }
    
    public string InstanceUrl { get; private set; }

    public UsersList Users = new UsersList();
    public ItemsList Items = new ItemsList();

    public class UsersList
    {
      public SSCCredentialsPOD Admin = new SSCCredentialsPOD("admin", "b", "sitecore");
      public SSCCredentialsPOD Anonymous = new SSCCredentialsPOD(null, null, null);
      public SSCCredentialsPOD Creatorex = new SSCCredentialsPOD("creatorex", "creatorex", "extranet");
      public SSCCredentialsPOD SitecoreCreator = new SSCCredentialsPOD("creator", "creator", "sitecore");
      public SSCCredentialsPOD NoReadUserExtranet = new SSCCredentialsPOD("noreadaccess", "noreadaccess", "extranet");
      public SSCCredentialsPOD FakeAnonymous = new SSCCredentialsPOD("FakeAnonymous", "b", "extranet");
      public SSCCredentialsPOD NotExistent = new SSCCredentialsPOD("notexistent", "notexistent", "sitecore");
      public SSCCredentialsPOD NoCreateAccess = new SSCCredentialsPOD("nocreate", "nocreate", "sitecore");
      public SSCCredentialsPOD NoReadUserSitecore = new SSCCredentialsPOD("noreaduser", "noreaduser", "sitecore");
    }

    public class ItemsList
    {
      public Item Home = new Item();
      public Item ItemWithVersions = new Item();
      public Item TestFieldsItem = new Item();
      public Item AllowedItem = new Item();
      public Item AllowedParent = new Item();
      public Item CreateItemsHere = new Item();

      public Item MediaImagesItem = new Item();
      public Item UploadMediaHere = new Item();
    }

    public static class Videos
    {
      public static string IMG_0994_MOV = "http://mediaitems.test24dk1.dk.sitecore.net/videos/IMG_0994.MOV";
    }

    public static class Images
    {
      public static class Tif
      {
        public const string _3000x3000 = "http://mediaitems.test24dk1.dk.sitecore.net/pictures/3000x3000.tif";
        public const string _60x60 = "http://mediaitems.test24dk1.dk.sitecore.net/pictures/60x60.tif";
      }

      public static class Gif
      {
        public const string _3000x3000 = "http://mediaitems.test24dk1.dk.sitecore.net/pictures/-3000x3000.gif";
        public const string Pictures_2 = "http://mediaitems.test24dk1.dk.sitecore.net/pictures/Pictures-2.gif";
      }

      public static class Png
      {
        public const string Bambuk = "http://mediaitems.test24dk1.dk.sitecore.net/pictures/wpapers_ru_%D0%91%D0%B0%D0%BC%D0%B1%D1%83%D0%BA.png";
      }

      public static class Jpeg
      {
        public const string _30x30 = "http://mediaitems.test24dk1.dk.sitecore.net/pictures/30X30.jpg";
      }
    }

    public class Item
    {
      public string Id { get; set; }
      public string Path { get; set; }
      public string DisplayName { get; set; }
      public string TemplateId { get; set; }
    }

    public void AssertItemsAreEqual(Item expected, ISitecoreItem actual)
    {
      if (null != expected.DisplayName)
      {
        Assert.AreEqual(expected.DisplayName, actual.DisplayName);
      }
      if (null != expected.Id)
      {
        Assert.AreEqual(expected.Id.ToUpper(), actual.Id.ToUpper());
      }
      if (null != expected.Path)
      {
        Assert.AreEqual(expected.Path.ToUpper(), actual.Path.ToUpper());
      }
      if (null != expected.TemplateId)
      {
        Assert.AreEqual(expected.TemplateId.ToUpper(), actual.TemplateId.ToUpper());
      }
    }

    public void AssertItemSourcesAreEqual(IItemSource expected, IItemSource actual)
    {
      Assert.AreEqual(expected.Database, actual.Database);
      Assert.AreEqual(expected.Language, actual.Language);
      Assert.AreEqual(expected.VersionNumber, actual.VersionNumber);
    }

    public void AssertItemsCount(int itemCount, ScItemsResponse response)
    {
      Assert.AreEqual(itemCount, response.ResultCount);
    }
  }
}

