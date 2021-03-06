﻿namespace WhiteLabelAndroid.Activities
{
  using System.Net;
  using Android.App;
  using Android.Content;
  using Android.Content.PM;
  using Android.OS;
  using Android.Views;
  using Android.Widget;
  using WhiteLabelAndroid.Activities.Create;
  using WhiteLabelAndroid.Activities.Media;
  using WhiteLabelAndroid.Activities.Read;

  [Activity(MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
  public class MainActivity : Activity
  {
    public override bool OnCreateOptionsMenu(IMenu menu)
    {
      this.MenuInflater.Inflate(Resource.Menu.main, menu);
      return base.OnCreateOptionsMenu(menu);
    }

    public override bool OnOptionsItemSelected(IMenuItem item)
    {
      switch (item.ItemId)
      {
        case Resource.Id.settings:
          this.StartActivity(new Intent(this, typeof(SettingsActivity)));
          break;
      }

      return true;
    }

    protected override void OnCreate(Bundle bundle)
    {
      ServicePointManager
          .ServerCertificateValidationCallback +=
          (sender, cert, chain, sslPolicyErrors) => true;
      
      base.OnCreate(bundle);
      this.SetContentView(Resource.Layout.activity_main);
      this.SetTitle(Resource.String.app_name);

      var container = this.FindViewById<LinearLayout>(Resource.Id.container_buttons);

      this.AddReadItemsButtons(container);
      this.AddCreateItemsButtons(container);
      this.AddDeleteItemsButtons(container);
      this.AddMediaItemsButtons(container);
      this.AddAuthenticateButtons(container);
    }

    private void AddReadItemsButtons(LinearLayout container)
    {
      var itemIdButton = new Button(this)
      {
        Text = this.GetString(Resource.String.text_get_item_by_id)
      };
      itemIdButton.Click += (sender, e) => this.StartActivity(typeof(ReadItemByIdActivity));

      var itemPathButton = new Button(this)
      {
        Text = this.GetString(Resource.String.text_get_item_by_path)
      };
      itemPathButton.Click += (sender, e) => this.StartActivity(typeof(ReadItemByPathActivtiy));
    
      container.AddView(itemIdButton);
      container.AddView(itemPathButton);
    }

    private void AddCreateItemsButtons(LinearLayout container)
    {
      var createItemByIdButton = new Button(this)
      {
        Text = this.GetString(Resource.String.text_create_item_by_id)
      };
      createItemByIdButton.Click += (sender, e) => this.StartActivity(typeof(CreateItemByIdActivity));

      container.AddView(createItemByIdButton);
    }

    private void AddDeleteItemsButtons(LinearLayout container)
    {
      var deleteItemButton = new Button(this)
      {
        Text = this.GetString(Resource.String.text_delete_item)
      };
      deleteItemButton.Click += (sender, e) => this.StartActivity(typeof(DeleteItemActivity));

      container.AddView(deleteItemButton);
    }

    private void AddMediaItemsButtons(LinearLayout container)
    {
      var downloadImageButton = new Button(this)
      {
        Text = this.GetString(Resource.String.text_download_image)
      };
      downloadImageButton.Click += (sender, e) => this.StartActivity(typeof(DownloadImageActivtiy));

      container.AddView(downloadImageButton);
    }

    private void AddAuthenticateButtons(LinearLayout container)
    {
      var authenticateButton = new Button(this)
      {
        Text = this.GetString(Resource.String.text_authenticate)
      };
      authenticateButton.Click += (sender, e) => this.StartActivity(typeof(AuthenticateActivity));

      container.AddView(authenticateButton);
    }

  }
}