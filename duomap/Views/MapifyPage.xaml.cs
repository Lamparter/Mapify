﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using duomap.Helpers;
using duomap.Services;

using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Navigation;

namespace duomap.Views
{
    public sealed partial class MapifyPage : Page, INotifyPropertyChanged
    {
        private const double DefaultZoomLevel = 20;

        private readonly LocationService _locationService;

        private readonly BasicGeoposition _defaultPosition = new BasicGeoposition()
        {
            Latitude = 47.609425,
            Longitude = -122.3417
        };

        private double _zoomLevel;

        public double ZoomLevel
        {
            get { return _zoomLevel; }
            set { Set(ref _zoomLevel, value); }
        }

        private Geopoint _center;

        public Geopoint Center
        {
            get { return _center; }
            set { Set(ref _center, value); }
        }

        public MapifyPage()
        {
            _locationService = new LocationService();
            Center = new Geopoint(_defaultPosition);
            ZoomLevel = DefaultZoomLevel;
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await InitializeAsync();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Cleanup();
        }

        public async Task InitializeAsync()
        {
            if (_locationService != null)
            {
                _locationService.PositionChanged += LocationService_PositionChanged;

                var initializationSuccessful = await _locationService.InitializeAsync();

                if (initializationSuccessful)
                {
                    await _locationService.StartListeningAsync();
                }

                if (initializationSuccessful && _locationService.CurrentPosition != null)
                {
                    Center = _locationService.CurrentPosition.Coordinate.Point;
                }
                else
                {
                    Center = new Geopoint(_defaultPosition);
                }
            }

            if (mapControl != null)
            {
                mapControl.MapServiceToken = "xiNF26moZMCyig5t48WR~0ZbgNbvnNYNFf8f35JDyPw~AjUjv4h1GdnlCJ-sDF31NuAHjZhTrW-KV3lIlQB6-6fsLO8x6yJc8h75dcmnDs1U";
                AddMapIcon(Center, "Map_YourLocation".GetLocalized());
            }
        }

        public void Cleanup()
        {
            if (_locationService != null)
            {
                _locationService.PositionChanged -= LocationService_PositionChanged;
                _locationService.StopListening();
            }
        }

        private void LocationService_PositionChanged(object sender, Geoposition geoposition)
        {
            if (geoposition != null)
            {
                Center = geoposition.Coordinate.Point;
            }
        }

        private void AddMapIcon(Geopoint position, string title)
        {
            MapIcon mapIcon = new MapIcon()
            {
                Location = position,
                NormalizedAnchorPoint = new Point(0.5, 1.0),
                Title = title,
                Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/map.png")),
                ZIndex = 0
            };
            mapControl.MapElements.Add(mapIcon);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
