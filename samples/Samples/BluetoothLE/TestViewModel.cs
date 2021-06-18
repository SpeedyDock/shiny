﻿using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Samples.Infrastructure;
using Shiny.BluetoothLE;


namespace Samples.BluetoothLE
{
    public class TestViewModel : ViewModel
    {
        readonly IBleManager bleManager;
        readonly IDialogs dialogs;

        IPeripheral peripheral;
        CancellationTokenSource? cancelSrc;
        CompositeDisposable? disp;


        public TestViewModel(IBleManager bleManager, IDialogs dialogs)
        {
            this.bleManager = bleManager;
            this.dialogs = dialogs;

            this.Start = ReactiveCommand.CreateFromTask<string>(
                async arg =>
                {
                    this.disp = new CompositeDisposable();
                    this.cancelSrc = new CancellationTokenSource();
                    this.Logs = String.Empty;

                    switch (arg)
                    {
                        case "managedperipheral":
                            await this.DoManagedPeripheral();
                            break;

                        case "pairing":
                            await this.PairingTest();
                            break;
                    }
                }
            );
            this.BindBusyCommand(this.Start);

            this.Stop = ReactiveCommand.Create(
                () =>
                {
                    this.disp?.Dispose();
                    this.cancelSrc?.Cancel();
                    this.peripheral?.CancelConnection();
                    this.Append("Stopped");
                    this.IsBusy = false;
                },
                this.WhenAny(x => x.IsBusy, x => x.GetValue())
            );
        }


        public ICommand Start { get; }
        public ICommand Stop { get; }
        [Reactive] public string Logs { get; private set; }

        void Append(string txt) => this.Logs = $"{txt}{Environment.NewLine}{this.Logs}";


        async Task DoManagedPeripheral()
        {
            const string serviceUuid = "FFF0";
            const string rxUuid = "FFF1";
            const string txUuid = "FFF2";

            await this.FindDevice();

            using (var managed = this.peripheral.CreateManaged())
            {
                await managed.EnableNotifications(true, serviceUuid, rxUuid);

                this.Append("Ok - Disconnect the device now, we'll wait");
                await this.peripheral.WhenDisconnected().Take(1).ToTask(this.cancelSrc.Token);
                peripheral.WhenConnected().Take(1).Subscribe(_ => this.Append("RE-CONNECTED - awaiting ready on rx notification"));

                var ready = managed.WhenNotificationReady().Take(1).ToTask(this.cancelSrc.Token);
                this.Append("Ok - Reconnect the device");

                await ready.ConfigureAwait(false);
                this.Append("Ready fired!  SUCCESS");

                var responseTask = managed
                    .WhenNotificationReceived(serviceUuid, rxUuid)
                    .Select(Encoding.ASCII.GetString)
                    .Take(1)
                    .ToTask(this.cancelSrc.Token);

                this.Append("Sending TX");
                await managed.Write(serviceUuid, txUuid, Encoding.ASCII.GetBytes("010D\r"));
                var response = await responseTask.ConfigureAwait(false);
                this.Append("RX Received - " + response);
            }
        }


        async Task PairingTest()
        {
            await this.FindDevice();

            //await this.peripheral.WithConnectIf().ToTask(this.cancelSrc.Token);
            this.Append("Device Connected - trying to pair");

            var pin = await this.dialogs.Input("Set PIN?");
            var result = await this.peripheral.TryPairingRequest(pin).ToTask(this.cancelSrc.Token);
            if (result == null)
            {
                this.Append("Pairing Not Supported");
            }
            else
            {
                this.Append("Pairing Result: " + result.Value);
            }
        }


        async Task FindDevice()
        {
            this.Append("Scanning..");
            this.peripheral = await this
                .bleManager
                .ScanForUniquePeripherals(new ScanConfig { ServiceUuids = { "FFF0" } })
                .Take(1)
                .ToTask(this.cancelSrc.Token);

            this.Append("Device Found");
        }
    }
}
