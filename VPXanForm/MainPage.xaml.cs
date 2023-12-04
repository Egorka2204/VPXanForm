using MediaManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Plugin.FilePicker;
using System.IO;
using MediaManager.Playback;
using PositionChangedEventArgs = MediaManager.Playback.PositionChangedEventArgs;
using MediaManager.Player;

namespace VPXanForm
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            CrossMediaManager.Current.StateChanged += Current_OnStateChanged;
            CrossMediaManager.Current.PositionChanged += Current_PositionChanged;

        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!CrossMediaManager.Current.IsPrepared())
            {
                //await InitPlay();

                // Set up Player Preferences
                //CrossMediaManager.Current.ShuffleMode = ShuffleMode.All;
                //CrossMediaManager.Current.PlayNextOnFailed = true;
                CrossMediaManager.Current.RepeatMode = RepeatMode.All;
                //CrossMediaManager.Current.AutoPlay = true;
            }
            else
            {
                //SetupCurrentMediaDetails(CrossMediaManager.Current.Queue.Current);
                SetupCurrentMediaPositionData(CrossMediaManager.Current.Position);
                SetupCurrentMediaPlayerState(CrossMediaManager.Current.State);
            }
        }

        private void OnTapGestureRecognizerTapped(object sender, EventArgs args)
        {
            if (gridPlayMenu.IsVisible == false)
            {
                gridPlayMenu.IsVisible = true;
            }
            else
            {
                gridPlayMenu.IsVisible = false;
            }

        }

        private async void PlayPauseButton_Clicked(object sender, EventArgs e)
        {
            if (CrossMediaManager.Current.State == MediaPlayerState.Paused)
            {
                PlayPauseButton.ImageSource = "Play.png";
            }
            else if(CrossMediaManager.Current.State == MediaPlayerState.Playing)
            {
                PlayPauseButton.ImageSource = "Stop.png";
            }
            await CrossMediaManager.Current.PlayPause();
        }

        //private async Task InitPlay()
        //{

        //    var currentMediaItem = await CrossMediaManager.Current.Play("https://media.geeksforgeeks.org/wp-content/uploads/20210314115545/sample-video.mp4");
        //    await SetupCurrentMediaDetails(currentMediaItem);
        //    SetupCurrentMediaPositionData(CrossMediaManager.Current.Position);
        //}

        private void SetupCurrentMediaPositionData(TimeSpan currentPlaybackPosition)
        {
            var formattingPattern = @"hh\:mm\:ss";
            if (CrossMediaManager.Current.Duration.Hours <= 0)
                formattingPattern = @"mm\:ss";

            var fullLengthString = CrossMediaManager.Current.Duration.ToString(formattingPattern);
            LabelPositionFirst.Text = $"{currentPlaybackPosition.ToString(formattingPattern)}";
            LabelPositionSecond.Text = $"{fullLengthString}";

            SliderPlayDisplay.Value = currentPlaybackPosition.Ticks;

        }
        private void Current_PositionChanged(object sender, PositionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                SetupCurrentMediaPositionData(e.Position);
            });
        }

        private void SetupCurrentMediaPlayerState(MediaPlayerState currentPlayerState)
        {

            if (currentPlayerState == MediaManager.Player.MediaPlayerState.Loading)
            {
                SliderPlayDisplay.Value = 0;
            }
            else if (currentPlayerState == MediaManager.Player.MediaPlayerState.Playing
                    && CrossMediaManager.Current.Duration.Ticks > 0)
            {
                SliderPlayDisplay.Maximum = CrossMediaManager.Current.Duration.Ticks;
            }
        }

        private void Current_OnStateChanged(object sender, StateChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                SetupCurrentMediaPlayerState(e.State);
            });
        }


        private async void ButtonAddVideo_OnClicked(object sender, EventArgs e)
        {
            try
            {
                string[] fileTypes = null;
                if (Device.RuntimePlatform == Device.Android)
                {
                    fileTypes = new string[] { "video/mp4" };
                }

                var pickedFile = await CrossFilePicker.Current.PickFile(fileTypes);
                if (pickedFile != null)
                {
                    var cachedFilePathName = Path.Combine(FileSystem.CacheDirectory, pickedFile.FileName);

                    if (!File.Exists(cachedFilePathName))
                        File.WriteAllBytes(cachedFilePathName, pickedFile.DataArray);

                    if (File.Exists(cachedFilePathName))
                    {
                        //var generatedMediaItem =
                        //    await CrossMediaManager.Current.Extractor.CreateMediaItem(cachedFilePathName);
                        //CrossMediaManager.Current.Queue.Add(generatedMediaItem);

                        //SetupPlaylist(true);
                        var generatedMediaItem =
                        await CrossMediaManager.Current.Extractor.CreateMediaItem(cachedFilePathName);
                        await CrossMediaManager.Current.Play(generatedMediaItem);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async void ButtonAddVideoUrl_OnClicked(object sender, EventArgs e)
        {
            string result = await DisplayPromptAsync("Ввод URL", "Введите ссылку URL", "OK", "Cancel", default, -1, Keyboard.Url);
            if (result != null)
            {
                await CrossMediaManager.Current.Play(result);
            }
        }

        private async void ForwardButton_Clicked(object sender, EventArgs e)
        {
            await CrossMediaManager.Current.StepForward();
        }

        private async void RewindButton_Clicked(object sender, EventArgs e)
        {
            await CrossMediaManager.Current.StepBackward();
        }
    }
}
