﻿using static Retyped.dom;

namespace ExpressCraft
{
    public class ConsoleForm : Form
    {
        public static bool ConsoleVisible = false;
        private static ConsoleForm _consoleForm = null;
        private HTMLDivElement logContent = null;
        private static WindowStateType prevWindowState = WindowStateType.Normal;
        private static bool firstLoad = true;
        private static Vector2 prevLocation;
        private static Vector2 prevSize = Settings.ConsoleDefaultSize;

        public void InternalClear()
        {
            logContent.Empty();
        }

        public void InternalLog(string source, ConsoleLogType logType = ConsoleLogType.Log)
        {
            var para = new HTMLParagraphElement() { className = "console-para" };
            switch(logType)
            {
                case ConsoleLogType.Debug:
                    para.style.color = Color.ForestGreen;
                    break;

                case ConsoleLogType.Error:
                    para.style.color = Color.Red;
                    break;
            }

            para.innerHTML = source;
            logContent.appendChild(para);
            if(logContent.children.length > 1000)
            {
                logContent.removeChild(logContent.children[0]);
            }
            para.scrollIntoView(false);
        }

        protected override void OnGotFocus()
        {
            if(Content != null)
            {
                Style.opacity = "1";
            }
            base.OnGotFocus();
        }

        protected override void OnLostFocus()
        {
            if(Content != null)
            {
                Style.opacity = "0.5";
            }
            base.OnLostFocus();
        }

        public ConsoleForm()
        {
            logContent = Div("console-body");
            this.Body.AppendChild(logContent);
            this.Body.style.background = Color.Black;
            this.Body.style.overflowY = "scroll";

            this.Text = document.title + " - Console";
            if(firstLoad)
            {
                this.StartPosition = FormStartPosition.Center;
                this.Size = prevSize;
            }
            else
            {
                this.StartPosition = FormStartPosition.Manual;
                this.Location = prevLocation;

                if(prevWindowState == WindowStateType.Maximized)
                {
                    prevSize = Settings.ConsoleDefaultSize;
                }

                this.Size = prevSize;

                if(prevWindowState == WindowStateType.Maximized)
                {
                    SetWindowState(prevWindowState);
                }
            }
        }

        protected override void OnShowed()
        {
            base.OnShowed();
            ConsoleVisible = true;
            firstLoad = false;
        }

        protected override void OnClosing()
        {
            base.OnClosing();

            prevSize = Size;
            prevLocation = Location;
            prevWindowState = WindowState;
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            ConsoleVisible = false;
        }

        private static void CheckConsoleState()
        {
            if(!ConsoleVisible)
            {
                _consoleForm = new ConsoleForm();
                _consoleForm.Show(true);
            }
        }

        public static void Log(string source, ConsoleLogType logType = ConsoleLogType.Log)
        {
            CheckConsoleState();
            _consoleForm.InternalLog(source, logType);
        }

        public new static void Clear()
        {
            CheckConsoleState();
            _consoleForm.InternalClear();
        }
    }

    public enum ConsoleLogType
    {
        Log,
        Debug,
        Error
    }
}