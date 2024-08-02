using CargoMaker.Config;
using CargoMaker.Model;
using Microsoft.Extensions.Logging;
using Container = CargoMaker.Model.Container;

namespace CargoMaker.Builder {

    public interface IContainerBuilder
    {
        public List<Container> BuildContainers(ILogger log, ContainerConfig ContainerConfig, List<ItemToLoad> itemsToLoad);
    }
}