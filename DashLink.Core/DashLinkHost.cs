using DashLink.Core.Action;
using DashLink.Core.Builtin;
using DashLink.Core.Config;
using DashLink.Core.IO;
using DashLink.Net;
using DashLink.Net.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace DashLink.Core
{
    /// <summary>
    /// The root class for a DashLink device.
    /// </summary>
    public class DashLinkHost : IDisposable
    {
        /// <summary>
        /// Gets the action executor to which commands are bound to.
        /// </summary>
        public ActionExecutor ActionExecutor { get; }
        /// <summary>
        /// Gets the input binder to which commands are associated with inputs.
        /// </summary>
        public InputBinder Binder { get; }
        /// <summary>
        /// Gets the main configuration for this instance.
        /// </summary>
        public DashLinkConfig Config { get; }
        /// <summary>
        /// Gets the underlying connection to the DashLink device.
        /// </summary>
        public ConnectionInterface Interface { get; }
        /// <summary>
        /// Gets the LCD text cache.
        /// </summary>
        public LcdCache LcdCache { get; }
        /// <summary>
        /// Gets or sets the module loader for external modules.
        /// </summary>
        public IDashLinkModuleLoader ModuleLoader { get; set; }

        /// <summary>
        /// Gets the currently selected profile for this instance.
        /// </summary>
        public Profile CurrentProfile { get; private set; }

        /// <summary>
        /// Gets all currently loaded modules, which might include ones not being used by the current profile.
        /// </summary>
        public IEnumerable<IModule> LoadedModules => new HashSet<IModule>(modules.Values);
        private readonly Dictionary<string, IModule> modules;

        /// <summary>
        /// Create a new <see cref="DashLink.Core.DashLinkHost"/> instance using an empty configuration.
        /// </summary>
        /// <param name="connectionInterface">The connection interface with the DashLink device.</param>
        /// <param name="enableBuiltin">Automatically loads built-in modules for commands.</param>
        public DashLinkHost(ConnectionInterface connectionInterface, bool enableBuiltin = true) : this(connectionInterface, (DashLinkConfig)null, enableBuiltin)
        { }

        /// <summary>
        /// Create a new <see cref="DashLink.Core.DashLinkHost"/> instance loading the configuration at the specified file path.
        /// </summary>
        /// <param name="connectionInterface">The connection interface with the DashLink device.</param>
        /// <param name="configPath">The path to the configuration path to use.</param>
        /// <param name="enableBuiltin">Automatically loads built-in modules for commands.</param>
        /// <seealso cref="DashLink.Core.Config.DashLinkConfig"/>
        public DashLinkHost(ConnectionInterface connectionInterface, string configPath, bool enableBuiltin = true)
        {
            Interface = connectionInterface ?? throw new ArgumentNullException(nameof(connectionInterface));
            Config = string.IsNullOrWhiteSpace(configPath) ? throw new ArgumentNullException(nameof(configPath)) : JsonSerializer.Deserialize<DashLinkConfig>(File.ReadAllBytes(configPath));
            Binder = new InputBinder();
            ActionExecutor = new ActionExecutor();
            LcdCache = new LcdCache();
            modules = new Dictionary<string, IModule>();
            Setup(enableBuiltin);
        }

        /// <summary>
        /// Creates a new <see cref="DashLink.Core.DashLinkHost"/> instance with the provided configuration, or an empty configuration if null.
        /// </summary>
        /// <param name="connectionInterface">The connection interface with the DashLink device.</param>
        /// <param name="config">The configuration to use.</param>
        /// <param name="enableBuiltin">Automatically loads built-in modules for commands.</param>
        public DashLinkHost(ConnectionInterface connectionInterface, DashLinkConfig config, bool enableBuiltin = true)
        {
            Interface = connectionInterface ?? throw new ArgumentNullException(nameof(connectionInterface));
            Config = config ?? DashLinkConfig.EmptyConfig();
            Binder = new InputBinder();
            ActionExecutor = new ActionExecutor();
            LcdCache = new LcdCache();
            modules = new Dictionary<string, IModule>();
            Setup(enableBuiltin);
        }

        private void Setup(bool enableBuiltin)
        {
            Interface.OnButtonEvent += Interface_OnButtonEvent;
            Interface.OnRotaryEvent += Interface_OnRotaryEvent;

            // Load built-ins
            if (enableBuiltin)
            {
                LoadModule(new CommandModule());
                LoadModule(new ProfileModule());
            }

            SwitchProfile(Config.LastProfile, true);
        }

        /// <summary>
        /// Switches profiles to the default profile and returns it.
        /// </summary>
        /// <returns>The default <see cref="DashLink.Core.Config.Profile"/>.</returns>
        public Profile SwitchDefaultProfile()
        {
            Profile profile = InitDefaultProfile();

            if (profile == null)
            {
                return SwitchProfile(Config.DefaultProfile, true);
            }

            return profile;
        }

        /// <summary>
        /// Switches profiles to the one with specified the ID and returns it.
        /// </summary>
        /// <param name="id">The unique ID of the profile.</param>
        /// <param name="defaultFallback">If true, and the profile specified by the ID is not found, the default profile will be used instead.</param>
        /// <returns>The matching <see cref="DashLink.Core.Config.Profile"/>.</returns>
        public Profile SwitchProfile(string id, bool defaultFallback = false)
        {
            Profile profile;

            if (Config.Profiles == null || Config.Profiles.Count == 0)
            {
                profile = defaultFallback ? InitDefaultProfile() : throw new ConfigurationException(Config, "No valid profiles");
                return profile;
            }

            id = id ?? throw new ArgumentNullException(nameof(id));
            profile = Config.Profiles.Find(x => id.Equals(x.Id));

            if (defaultFallback && profile == null)
            {
                profile = Config.Profiles.Find(x => Config.DefaultProfile.Equals(x.Id));
                if (profile == null) profile = Profile.EmptyProfile(DashLinkConfig.DefaultId);
            }
            if (profile != null) ReloadProfile(profile);
            return profile;
        }

        private Profile InitDefaultProfile()
        {
            if (Config.Profiles == null || Config.Profiles.Count == 0)
            {
                Config.Profiles = Config.Profiles ?? new List<Profile>();

                var profile = Profile.EmptyProfile(DashLinkConfig.DefaultId);
                Config.Profiles.Add(profile);
                ReloadProfile(profile);
                return profile;
            }
            return null;
        }

        /// <summary>
        /// Loads an external module which provides command functions and other services.
        /// </summary>
        /// <param name="module">The module to load.</param>
        /// <param name="enable">If true, if the module should be immediately enabled.</param>
        /// <returns>True, if the module was loaded successfully</returns>
        public bool LoadModule(IModule module, bool enable = true)
        {
            module = module ?? throw new ArgumentNullException(nameof(module));
            if (modules.ContainsKey(module.Name)) return false;

            modules.Add(module.Name, module);
            module.Start(this);
            ActionExecutor.RegisterModule(module.Name, module, enable);
            return true;
        }

        /// <summary>
        /// Disables and optionally unloads a module by its name.
        /// </summary>
        /// <param name="moduleName">The name of the module to disable or unload.</param>
        /// <param name="keepLoaded">If true, the module will remain loaded but unused</param>
        /// <returns>True, if the module was found and disabled successfully</returns>
        public bool DisableModule(string moduleName, bool keepLoaded)
        {
            moduleName = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
            if (!modules.ContainsKey(moduleName)) return false;

            if (modules.TryGetValue(moduleName, out IModule module)) module.Stop();
            if (keepLoaded) ActionExecutor.DisableModule(moduleName);
            else ActionExecutor.UnregisterModule(moduleName);
            return true;
        }

        private void ReloadProfile(Profile profile)
        {
            CurrentProfile = profile;
            var unloaded = ActionExecutor.SetEnabledSelection(profile.Modules);

            if (unloaded.Count > 0 && ModuleLoader != null)
            {
                ModuleLoader.LoadModules(this, unloaded);
            }

            Binder.Reset();
            Binder.Populate(profile.Bindings);
        }

        /// <summary>
        /// Begins the LCD updater task. The task will watch for changes to the <see cref="LcdCache"/> but will only send changes if <see cref="Capabilities.LiquidCrystal"/> is set in the device's capability flags.
        /// </summary>
        public void BeginLcdUpdater()
        {
            if (Interface.InterfaceCapabilities.HasFlag(Capabilities.LiquidCrystal))
            {

            }
        }

        /// <summary>
        /// Forces an update of the LCD display by checking for changes to the <see cref="LcdCache"/>.
        /// </summary>
        /// <returns>True, if the device has the capability <see cref="Capabilities.LiquidCrystal"/>.</returns>
        public bool UpdateLcd()
        {
            if (Interface.InterfaceCapabilities.HasFlag(Capabilities.LiquidCrystal))
            {

            }
            return false;
        }

        private void Interface_OnButtonEvent(object sender, ButtonEventArgs e)
        {
            BeginExecute((e.IsPressed ? EventData.ButtonPressEventStr : EventData.ButtonReleaseEventStr) + e.ButtonId);
        }

        private void Interface_OnRotaryEvent(object sender, RotaryEventArgs e)
        {
            if (e.IsTurn)
            {
                BeginExecute(e.IsClockwiseTurn ? EventData.RotaryTurnClockwiseEventStr : EventData.RotaryTurnAnticlockwiseEventStr);
            }
            if (e.IsButtonPress)
            {
                BeginExecute(e.IsButtonDoublePress ? EventData.RotaryDoublePressEventStr : EventData.RotaryPressEventStr);
            }
        }

        internal void BeginExecute(string eventId)
        {
            foreach (string binding in Binder.GetBindings(eventId))
            {
                ActionExecutor.Call(binding, Config.AsyncCommands);
            }
        }

        public void Dispose()
        {
            // TODO
        }
    }
}
