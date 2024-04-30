﻿using Sample.Pages;

namespace Sample
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new MainPage());
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var w = base.CreateWindow(activationState);
#if WINDOWS
            w.Width = 500;
            w.Height = 700;
#endif
            return w;
        }
    }
}
