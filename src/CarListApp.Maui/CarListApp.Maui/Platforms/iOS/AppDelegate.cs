using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using UIKit;
using Serilog;

namespace CarListApp.Maui;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp()
    {
        try
        {
            Log.Information("Creating MauiApp in iOS AppDelegate");
            var builder = MauiProgram.CreateMauiApp();
            if (builder == null)
            {
                Log.Error("MauiProgram.CreateMauiApp() returned null");
                throw new InvalidOperationException("Failed to create MauiApp");
            }
            return builder;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Failed to create MauiApp in iOS AppDelegate");
            throw;
        }
    }

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        try
        {
            Log.Information("iOS FinishedLaunching started");
            var result = base.FinishedLaunching(application, launchOptions);

            // Ensure the window takes up the full screen
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var viewController = Platform.GetCurrentUIViewController();
                    if (viewController?.View != null)
                    {
                        viewController.View.Frame = UIScreen.MainScreen.Bounds;
                        Log.Debug("Set iOS view controller frame to full screen");
                    }
                    else
                    {
                        Log.Warning("Could not access UIViewController or its View to set frame");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error setting view controller frame");
                }
            });

            Log.Information("iOS FinishedLaunching completed successfully");
            return result;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Error in iOS FinishedLaunching");
            throw;
        }
    }
}