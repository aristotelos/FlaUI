using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.UIA3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FlaUI.WebDriver
{
    public class Session
    {
        public Session(Application? app)
        {
            App = app;
            SessionId = Guid.NewGuid().ToString();
            Automation = new UIA3Automation();
            InputState = new InputState();
            TimeoutsConfiguration = new TimeoutsConfiguration();
        }

        public string SessionId { get; }
        public UIA3Automation Automation { get; }
        public Application? App { get; }
        public InputState InputState { get; }
        private Dictionary<string, KnownElement> KnownElementsByElementReference { get; } = new Dictionary<string, KnownElement>();
        private Dictionary<string, KnownWindow> KnownWindowsByWindowHandle { get; } = new Dictionary<string, KnownWindow>();
        public TimeSpan ImplicitWaitTimeout => TimeSpan.FromMilliseconds(TimeoutsConfiguration.ImplicitWaitTimeoutMs);
        public TimeSpan? ScriptTimeout => TimeoutsConfiguration.ScriptTimeoutMs.HasValue ? TimeSpan.FromMilliseconds(TimeoutsConfiguration.ScriptTimeoutMs.Value) : null;

        public TimeoutsConfiguration TimeoutsConfiguration { get; set; }

        private KnownWindow? _currentWindow;

        public KnownWindow CurrentWindow
        {
            get
            {
                if (_currentWindow == null)
                {

                    if (App == null)
                    {
                        throw WebDriverResponseException.UnsupportedOperation("This operation is not supported for Root app");
                    }
                    var mainWindow = App.GetMainWindow(Automation);
                    _currentWindow = GetOrAddKnownWindow(mainWindow);
                }
                return _currentWindow;
            }
            set
            {
                _currentWindow = value;
            }
        }

        public KnownElement GetOrAddKnownElement(AutomationElement element)
        {
            var result = KnownElementsByElementReference.Values.FirstOrDefault(knownElement => knownElement.Element.Equals(element));
            if (result == null)
            {
                result = new KnownElement(element);
                KnownElementsByElementReference.Add(result.ElementReference, result);
            }
            return result;
        }

        public AutomationElement? FindKnownElementById(string elementId)
        {
            if (!KnownElementsByElementReference.TryGetValue(elementId, out var knownElement))
            {
                return null;
            }
            return knownElement.Element;
        }

        public KnownWindow GetOrAddKnownWindow(Window window)
        {
            var result = KnownWindowsByWindowHandle.Values.FirstOrDefault(knownElement => knownElement.Window.Equals(window));
            if (result == null)
            {
                result = new KnownWindow(window);
                KnownWindowsByWindowHandle.Add(result.WindowHandle, result);
            }
            return result;
        }

        public Window? FindKnownWindowByWindowHandle(string windowHandle)
        {
            if (!KnownWindowsByWindowHandle.TryGetValue(windowHandle, out var knownWindow))
            {
                return null;
            }
            return knownWindow.Window;
        }
    }
}
