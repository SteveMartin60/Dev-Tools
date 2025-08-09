using IconsRestorer.Code;
using System.Collections.Generic;
using System.Windows.Input;

namespace IconsRestorer.ViewModels
{
    internal class MainViewModel
    {
        private readonly DesktopRegistry _registry = new DesktopRegistry();
        private readonly desktop _desktop = new desktop();
        private readonly storage _storage = new storage();

        public ICommand SavePositions
        {
            get
            {
                return new DelegateCommand(
                        arg =>
                        {
                            var registryValues = _registry.GetRegistryValues();
                            var iconPositions = _desktop.GetIconsPositions();
                            _storage.SaveIconPositions(iconPositions, registryValues);
                        }
                    );
            }
        }

        public ICommand RestorePositions
        {
            get
            {
                return new DelegateCommand(
                        arg =>
                        {
                            var (iconPositions, registryValues) = _storage.LoadIconPositions();

                            _registry.SetRegistryValues(registryValues);

                            _desktop.SetIconPositions(iconPositions);

                            _desktop.Refresh(immediate: false);
                        }
                    );
            }
        }
    }
}
