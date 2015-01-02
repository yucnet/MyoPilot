﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AR.Drone.Client;
using AR.Drone.Client.Command;
using AR.Drone.Client.Configuration;
using AR.Drone.Data;
using AR.Drone.Data.Navigation;
using AR.Drone.Data.Navigation.Native;
using AR.Drone.Media;
using AR.Drone.Video;
using AR.Drone.WinApp;
using MyoPilot.Input;
using System.Drawing.Text;
using System.IO;

namespace MyoPilot
{
    public partial class MainForm : Form
    {
        private readonly DroneClient droneClient;
        private readonly VideoPacketDecoderWorker videoPacketDecoderWorker;
        private Settings settings;
        private VideoFrame frame;
        private Bitmap frameBitmap;
        private uint frameNumber;
        private NavigationData navigationData;
        private InputManager inputManager;
        private PrivateFontCollection privateFontCollection;

        public MainForm()
        {
            InitializeComponent();

            videoPacketDecoderWorker = new VideoPacketDecoderWorker(PixelFormat.BGR24, true, OnVideoPacketDecoded);
            videoPacketDecoderWorker.Start();
            videoPacketDecoderWorker.UnhandledException += UnhandledException;

            droneClient = new DroneClient();
            droneClient.VideoPacketAcquired += OnVideoPacketAcquired;
            droneClient.NavigationDataAcquired += data => navigationData = data;
            droneClient.Start();

            LoadFontAwesome();
            InitInput();

            timerVideoUpdate.Enabled = true;
            timerInput.Enabled = true;
        }

        private void LoadFontAwesome()
        {
            string fontName = "FontAwesome";
            string fontFileName = "FontAwesome.otf";
            float fontSize = 16;
            Font fontAwesome = new Font(fontName, fontSize);
            // If the font is not installed, it will be substituted with Microsoft Sans Serif
            if (fontAwesome.Name != fontName)
            {
                // try loading the font from file
                string fontFile = Path.Combine(Environment.CurrentDirectory, fontFileName);
                if (File.Exists(fontFile))
                {
                    // privateFontCollection needs to be in scope while the font is used
                    // this is why it is a class variable
                    privateFontCollection = new PrivateFontCollection();
                    privateFontCollection.AddFontFile(fontFile);
                    FontFamily family = privateFontCollection.Families[0];
                    fontAwesome = new Font(family, fontSize);
                }
                else
                {
                    MessageBox.Show("Font Awesome by Dave Gandy not found. The application might not work correctly. Please download " + fontFileName + 
                        " from http://fontawesome.io and either install it or drop it in the application folder (" + Environment.CurrentDirectory + ")",
                        "Cannot load FontAwesome", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            //label1.Font = fontAwesome;
        }

        private void InitInput()
        {
            KeyboardInput keyboardInput = new KeyboardInput();
            this.GotFocus += (sender, e) => keyboardInput.Active = true;
            this.LostFocus += (sender, e) => keyboardInput.Active = false;

            inputManager = new InputManager();
            // Add UI Listeners
            inputManager.Emergency += inputManager_Emergency;
            inputManager.FlatTrim += inputManager_FlatTrim;
            inputManager.Hover += inputManager_Hover;
            inputManager.Land += inputManager_Land;
            inputManager.Progress += inputManager_Progress;
            inputManager.Takeoff += inputManager_Takeoff;
            // Add droneClient listeners
            inputManager.Emergency += droneClient.Emergency;
            inputManager.FlatTrim += droneClient.FlatTrim;
            inputManager.Hover += droneClient.Hover;
            inputManager.Land += droneClient.Land;
            inputManager.Progress += droneClient.Progress;
            inputManager.Takeoff += droneClient.Takeoff;

            inputManager.addControl(keyboardInput);
        }

        #region Input handling
        void inputManager_Takeoff()
        {
            throw new NotImplementedException();
        }

        void inputManager_Progress(FlightMode mode, float roll = 0, float pitch = 0, float yaw = 0, float gaz = 0)
        {
            throw new NotImplementedException();
        }

        void inputManager_Land()
        {
            throw new NotImplementedException();
        }

        void inputManager_Hover()
        {
            throw new NotImplementedException();
        }

        void inputManager_FlatTrim()
        {
            throw new NotImplementedException();
        }

        void inputManager_Emergency()
        {
            throw new NotImplementedException();
        }
        
        private void timerInput_Tick(object sender, EventArgs e)
        {
            inputManager.processInput();
        }
        #endregion

        #region Videostreaming
        private void OnVideoPacketAcquired(VideoPacket packet)
        {
            if (videoPacketDecoderWorker.IsAlive)
                videoPacketDecoderWorker.EnqueuePacket(packet);
        }

        private void OnVideoPacketDecoded(VideoFrame frame)
        {
            this.frame = frame;
        }
        
        private void timerVideoUpdate_Tick(object sender, EventArgs e)
        {
            if (frame == null || frameNumber == frame.Number)
                return;
            frameNumber = frame.Number;

            if (frameBitmap == null)
                frameBitmap = VideoHelper.CreateBitmap(ref frame);
            else
                VideoHelper.UpdateBitmap(ref frameBitmap, ref frame);

            videoFrame.Image = frameBitmap;
        }
        #endregion

        private void UnhandledException(object sender, Exception exception)
        {
            MessageBox.Show(exception.ToString(), "Unhandled Exception (Ctrl+C)", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Text += Environment.Is64BitProcess ? " [64-bit]" : " [32-bit]";
        }

        protected override void OnClosed(EventArgs e)
        {
            droneClient.Stop();
            droneClient.Dispose();
            videoPacketDecoderWorker.Dispose();

            base.OnClosed(e);
        }
                
    }
}
