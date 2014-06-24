using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvwsLib.ServiceProxies
{
	using EnvwsLib.ServiceContracts;

    public class CheckInServiceClientProxy : System.ServiceModel.ClientBase<ICheckInService>, ICheckInService 
    {
        
        public CheckInServiceClientProxy() {
        }
        
        public CheckInServiceClientProxy(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public CheckInServiceClientProxy(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public CheckInServiceClientProxy(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public CheckInServiceClientProxy(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public bool CheckIn(EnvwsLib.DataContracts.TrackerData td) {
            return base.Channel.CheckIn(td);
        }
        
       // public System.Threading.Tasks.Task<bool> CheckInAsync(EnvwsLib.DataContracts.TrackerData td) {
       //     return base.Channel.CheckInAsync(td);
       // }
        
        public EnvwsLib.DataContracts.JobData RequestJob() {
            return base.Channel.RequestJob();
        }
        
        //public System.Threading.Tasks.Task<EnvwsLib.DataContracts.JobData> RequestJobAsync() {
        //    return base.Channel.RequestJobAsync();
        //}
        
        public void ReturnFinishedJob(EnvwsLib.DataContracts.JobData j) {
            base.Channel.ReturnFinishedJob(j);
        }
        
        //public System.Threading.Tasks.Task ReturnFinishedJobAsync(EnvwsLib.DataContracts.JobData j) {
        //    return base.Channel.ReturnFinishedJobAsync(j);
        //}
    }
}
