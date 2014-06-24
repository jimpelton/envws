using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EnvwsLib.ServiceProxies
{
	using EnvwsLib.ServiceContracts;

    public class OrchestratorServiceClientProxy : System.ServiceModel.ClientBase<IOrchestratorService>, IOrchestratorService 
    {
        
        public OrchestratorServiceClientProxy() {
        }
        
        public OrchestratorServiceClientProxy(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public OrchestratorServiceClientProxy(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public OrchestratorServiceClientProxy(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public OrchestratorServiceClientProxy(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public bool QueueJob(EnvwsLib.DataContracts.JobData job) {
            return base.Channel.QueueJob(job);
        }
        
        public bool RemoveJob(string jobGuid) {
            return base.Channel.RemoveJob(jobGuid);
        }
        
        public EnvwsLib.DataContracts.TrackerData[] TrackerStatus() {
            return base.Channel.TrackerStatus();
        }
        
        public bool Ping() {
            return base.Channel.Ping();
        }
        
        public int NumWaitingJobs() {
            return base.Channel.NumWaitingJobs();
        }
        
        public int NumRunningTrackers() {
            return base.Channel.NumRunningTrackers();
        }
        
        public int NumIdleTrackers() {
            return base.Channel.NumIdleTrackers();
        }
    }
}
