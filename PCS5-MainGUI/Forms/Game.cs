using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using SharpDX.Windows;
using System.Diagnostics;

namespace PCS5_MainGUI.Forms
{
    public partial class Game : Form
    {
        Stopwatch clock;
        double totalTime;
        long frameCount;
        double measuredFPS;
        public Game()
        {
            InitializeComponent();
            InitializeDeviceResources();
        }
        private D3D11.Device d3dDevice;
        private D3D11.DeviceContext d3dDeviceContext;
        private SwapChain swapChain;
        private D3D11.RenderTargetView renderTargetView;

        private void InitializeDeviceResources()
        {
            ModeDescription backBufferDesc = new ModeDescription(Width, Height, new Rational(60, 1), Format.R8G8B8A8_UNorm);
            SwapChainDescription swapChainDesc = new SwapChainDescription()
            {
                ModeDescription = backBufferDesc,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput,
                BufferCount = 1,
                OutputHandle = renderControl1.Handle,
                IsWindowed = true
            };
            D3D11.Device.CreateWithSwapChain(DriverType.Hardware, D3D11.DeviceCreationFlags.None, swapChainDesc, out d3dDevice, out swapChain);
            d3dDeviceContext = d3dDevice.ImmediateContext;
            using (D3D11.Texture2D backBuffer = swapChain.GetBackBuffer<D3D11.Texture2D>(0))
            {
                renderTargetView = new D3D11.RenderTargetView(d3dDevice, backBuffer);
            }
            clock = Stopwatch.StartNew();
        }

        private void Draw()
        {
            d3dDeviceContext.OutputMerger.SetRenderTargets(renderTargetView);
            d3dDeviceContext.ClearRenderTargetView(renderTargetView, new SharpDX.Color(0,0,0));
            swapChain.Present(1, PresentFlags.None);
        }
        private void RenderCallback()
        {
            Draw();
            frameCount++;
            var timeElapsed = (double)clock.ElapsedTicks / Stopwatch.Frequency; ;
            totalTime += timeElapsed;
            if (totalTime >= 1.0f)
            {
                measuredFPS = Math.Round((double)frameCount / totalTime);
                frameCount = 0;
                totalTime = 0.0;
            }
            clock.Restart();
            this.Text = "D3D11, FPS: " + measuredFPS + "fps";
        }
        public void Dispose()
        {
            renderTargetView.Dispose();
            swapChain.Dispose();
            d3dDevice.Dispose();
            d3dDeviceContext.Dispose();
            renderControl1.Dispose();
        }

        private void Game_Load(object sender, EventArgs e)
        {
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            RenderCallback();
        }
    }
}
