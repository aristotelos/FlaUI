using FlaUI.Core.AutomationElements;
using FlaUI.WebDriver.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlaUI.WebDriver.Controllers
{
    [Route("session/{sessionId}/[controller]")]
    [ApiController]
    public class WindowController : ControllerBase
    {
        private readonly ILogger<WindowController> _logger;
        private readonly ISessionRepository _sessionRepository;

        public WindowController(ILogger<WindowController> logger, ISessionRepository sessionRepository)
        {
            _logger = logger;
            _sessionRepository = sessionRepository;
        }

        [HttpDelete]
        public async Task<ActionResult> CloseWindow([FromRoute] string sessionId)
        {
            var session = GetSession(sessionId);
            session.CurrentWindow.Window.Close();
            return await Task.FromResult(WebDriverResult.Success(GetWindowHandles(session)));
        }

        [HttpGet("handles")]
        public async Task<ActionResult> GetWindowHandles([FromRoute] string sessionId)
        {
            var session = GetSession(sessionId);
            var windowHandles = GetWindowHandles(session);
            return await Task.FromResult(WebDriverResult.Success(windowHandles));
        }

        [HttpGet]
        public async Task<ActionResult> GetWindowHandle([FromRoute] string sessionId)
        {
            var session = GetSession(sessionId);
            return await Task.FromResult(WebDriverResult.Success(session.CurrentWindow.WindowHandle));
        }

        [HttpPost]
        public async Task<ActionResult> SwitchWindow([FromRoute] string sessionId, [FromBody] SwitchWindowRequest switchWindowRequest)
        {
            var session = GetSession(sessionId);
            if (session.App == null)
            {
                throw WebDriverResponseException.UnsupportedOperation("Close window not supported for Root app");
            }
            var window = session.FindKnownWindowByWindowHandle(switchWindowRequest.Handle);
            if (window == null)
            {
                throw WebDriverResponseException.NoSuchWindow(switchWindowRequest.Handle);
            }
            session.CurrentWindow = session.GetOrAddKnownWindow(window);
            return await Task.FromResult(WebDriverResult.Success());
        }

        private IEnumerable<string> GetWindowHandles(Session session)
        {
            if (session.App == null)
            {
                throw WebDriverResponseException.UnsupportedOperation("Window operations not supported for Root app");
            }
            var windows = session.App.GetAllTopLevelWindows(session.Automation);
            var knownWindows = windows.Select(session.GetOrAddKnownWindow);
            return knownWindows.Select(knownWindows => knownWindows.WindowHandle);
        }

        private Session GetSession(string sessionId)
        {
            var session = _sessionRepository.FindById(sessionId);
            if (session == null)
            {
                throw WebDriverResponseException.SessionNotFound(sessionId);
            }
            return session;
        }
    }
}
