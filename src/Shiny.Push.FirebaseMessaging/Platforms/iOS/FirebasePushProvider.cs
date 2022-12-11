﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Firebase.CloudMessaging;
using Foundation;

namespace Shiny.Push;


public class FirebasePushProvider : IPushProvider //, IPushTagSupport
{

    IPushTagSupport? tagSupport;
    public IPushTagSupport? Tags => this.tagSupport ??= new FirebasePushTagSupport();


    public async Task<string> Register(NSData nativeToken)
    {
        //this.TryStartFirebase();

        Messaging.SharedInstance.ApnsToken = nativeToken;
        var fcmToken = await Messaging.SharedInstance.FetchTokenAsync();

        if (fcmToken == null)
            throw new InvalidOperationException("FCM Token is null");

        return fcmToken;
    }


    public Task UnRegister()
        => Messaging.SharedInstance.DeleteTokenAsync();

    //protected virtual void TryStartFirebase()
    //{
    //    if (App.DefaultInstance == null)
    //    {
    //        if (this.config.UseEmbeddedConfiguration)
    //        {
    //            App.Configure();
    //            if (Messaging.SharedInstance == null)
    //                throw new ArgumentException("Failed to configure firebase messaging - ensure you have GoogleService-Info.plist included in your iOS project and that it is set to a BundleResource");

    //            Messaging.SharedInstance!.AutoInitEnabled = true;
    //        }
    //        else
    //        {
    //            App.Configure(new Options(
    //                this.config.AppId!,
    //                this.config.SenderId!
    //            )
    //            {
    //                ApiKey = this.config.ApiKey,
    //                ProjectId = this.config.ProjectId
    //            });
    //        }
    //    }
    //}

    //public string[]? RegisteredTags => this.container.RegisteredTags;

    //public async Task AddTag(string tag)
    //{
    //    var tags = this.RegisteredTags?.ToList() ?? new List<string>(1);
    //    tags.Add(tag);

    //    await Messaging.SharedInstance.SubscribeAsync(tag);
    //    this.container.RegisteredTags = tags.ToArray();
    //}


    //public async Task RemoveTag(string tag)
    //{
    //    await Messaging
    //        .SharedInstance
    //        .UnsubscribeAsync(tag)
    //        .ConfigureAwait(false);

    //    if (this.RegisteredTags != null)
    //    {
    //        var tags = this.RegisteredTags.ToList();
    //        if (tags.Remove(tag))
    //            this.container.RegisteredTags = tags.ToArray();
    //    }
    //}


    //public async Task ClearTags()
    //{
    //    if (this.RegisteredTags != null)
    //    {
    //        foreach (var tag in this.RegisteredTags)
    //        {
    //            await Messaging
    //                .SharedInstance
    //                .UnsubscribeAsync(tag)
    //                .ConfigureAwait(false);
    //        }
    //    }
    //    this.container.RegisteredTags = null;
    //}


    //public async Task SetTags(params string[]? tags)
    //{
    //    await this.ClearTags().ConfigureAwait(false);
    //    if (tags != null)
    //    {
    //        foreach (var tag in tags)
    //            await this.AddTag(tag).ConfigureAwait(false);
    //    }
    //}

    public Task AddTag(string tag) => throw new NotImplementedException();
    public Task ClearTags() => throw new NotImplementedException();
    public Task RemoveTag(string tag) => throw new NotImplementedException();
    public Task<PushAccessState> RequestAccess(CancellationToken cancelToken = default) => throw new NotImplementedException();
    public Task SetTags(params string[]? tags) => throw new NotImplementedException();
}