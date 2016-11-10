﻿namespace WhiteLabeliOS
{
  using System;
  using System.Linq;
  using System.Drawing;

  using Foundation;
  using UIKit;

  using WhiteLabeliOS.FieldsTableView;

  using Sitecore.MobileSDK.API.Request.Parameters;
  using Sitecore.MobileSDK.API.Session;
  using Sitecore.MobileSDK.API;
  using Sitecore.MobileSDK.API.Items;
  using Sitecore.MobileSDK.API.Entities;
  using Sitecore.MobileSDK.Entities;

  public partial class GetItemByPathViewController : BaseTaskTableViewController
  {

    public GetItemByPathViewController(IntPtr handle) : base (handle)
    {
      Title = NSBundle.MainBundle.LocalizedString("getItemByPath", null);
    }

    public override void ViewDidLoad()
    {
      base.ViewDidLoad();
      this.TableView = this.FieldsTableView;

      this.ItemPathField.Text = "/sitecore/content/Home";

      this.ItemPathField.ShouldReturn = this.HideKeyboard;

      this.ItemPathField.Placeholder = NSBundle.MainBundle.LocalizedString ("Type item Path", null);
      this.fieldNameTextField.Placeholder = NSBundle.MainBundle.LocalizedString ("Type field name", null);

      string getItemButtonTitle = NSBundle.MainBundle.LocalizedString ("Get Item", null);
      getItemButton.SetTitle (getItemButtonTitle, UIControlState.Normal);
    }

    partial void OnGetItemButtonTouched (Foundation.NSObject sender)
    {
      if (String.IsNullOrEmpty(this.ItemPathField.Text))
      {
        AlertHelper.ShowLocalizedAlertWithOkOption("Error", "Please type item path");
      }
      else
      {
        this.HideKeyboard(this.ItemPathField);
        this.HideKeyboard(this.fieldNameTextField);

        this.SendRequest();
      }
    }

    partial void OnPayloadValueChanged (UIKit.UISegmentedControl sender)
    {
      
    }

    partial void OnButtonChangeState (UIKit.UIButton sender)
    {
      sender.Selected = !sender.Selected;
    }

    //private async void SendRequest()
    //{
    //  try
    //  {
    //    using (ISitecoreSSCSession session = this.instanceSettings.GetSession())
    //    {
    //      var request = ItemSSCRequestBuilder.ReadItemsRequestWithPath(this.ItemPathField.Text)
    //        .AddFieldsToRead(this.fieldNameTextField.Text)
    //        .Build();
          
    //      this.ShowLoader();

    //      ScItemsResponse response = await session.ReadItemAsync(request);

    //      if (response.Any())
    //      {
    //        this.ShowItemsList(response);
    //      }
    //      else
    //      {
    //        AlertHelper.ShowLocalizedAlertWithOkOption("Message", "Item is not exist");
    //      }
    //    }
    //  }
    //  catch(Exception e) 
    //  {
    //    this.CleanupTableViewBindings();
    //    AlertHelper.ShowLocalizedAlertWithOkOption("Error", e.Message);
    //  }
    //  finally
    //  {
    //    BeginInvokeOnMainThread(delegate
                                
    //    {
    //      this.FieldsTableView.ReloadData();
    //      this.HideLoader();
    //    });
    //  }
    //}

    //Entity
    //private async void SendRequest()
    //{
    //  //get all entities

    //  try {
    //    using (ISitecoreSSCSession session = this.instanceSettings.GetSession()) {
          
    //      var request = EntitySSCRequestBuilder.ReadEntitiesRequestWithPath()
    //                                           .Namespace("aggregate")
    //                                           .Controller("admin")
    //                                           .Action("Todo")
    //                                           .Build();

    //      this.ShowLoader();

    //      ScEntityResponse response = await session.ReadEntityAsync(request);

    //      if (response.Any()) {
    //        AlertHelper.ShowLocalizedAlertWithOkOption("Entities count", response.Count().ToString());
    //        foreach(var entity in response) {
    //          Console.WriteLine("ENTITY: " + entity["id"].RawValue);
    //        }

    //      } else {
    //        AlertHelper.ShowLocalizedAlertWithOkOption("Message", "Entities not found");
    //      }
    //    }
    //  } catch (Exception e) {
    //    this.CleanupTableViewBindings();
    //    AlertHelper.ShowLocalizedAlertWithOkOption("Error", e.Message);
    //  } finally {
    //    BeginInvokeOnMainThread(delegate {
    //      this.FieldsTableView.ReloadData();
    //      this.HideLoader();
    //    });
    //  }
    //}


    private async void SendRequest()
    {
      //get entity by id

      try {
        using (ISitecoreSSCSession session = this.instanceSettings.GetSession()) {

          var request = EntitySSCRequestBuilder.ReadEntityRequestById("1")
                                               .Namespace("aggregate")
                                               .Controller("admin")
                                               .TaskId("id")
                                               .Action("Todo")
                                               .Build();

          this.ShowLoader();

          ScEntityResponse response = await session.ReadEntityAsync(request);

          if (response.Any()) {
            AlertHelper.ShowLocalizedAlertWithOkOption("Entities count", response.Count().ToString());
            foreach (var entity in response) {
              Console.WriteLine("ENTITY: " + entity["id"].RawValue);
            }

          } else {
            AlertHelper.ShowLocalizedAlertWithOkOption("Message", "Entities not found");
          }
        }
      } catch (Exception e) {
        this.CleanupTableViewBindings();
        AlertHelper.ShowLocalizedAlertWithOkOption("Error", e.Message);
      } finally {
        BeginInvokeOnMainThread(delegate {
          this.FieldsTableView.ReloadData();
          this.HideLoader();
        });
      }
    }

  }

}

