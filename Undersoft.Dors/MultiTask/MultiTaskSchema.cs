using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Dors;
using System.Linq;

namespace System.Dors.MultiTask
{
    public class Campaign
    {
        public Campaign(string Name = null, MultiTaskPost Post = null)
        {
            CampaignName = (Name != null) ? Name : "MultiTaskWorld";
            IOPost = (Post != null) ? Post : new MultiTaskPost();
        }

        public string CampaignName { get; set; }

        public MultiTaskPost IOPost { get; set; }

        public Mission Get(string key)
        {
            Mission result = null;
            Missions.TryGetValue(key, out result);
            return result;
        }
        public void    Add(Mission value)
        {
            value.Campaign = this;
            value.Visor = new MultiTaskVisor(value);
            Missions.AddOrUpdate(value.MissionName, value, (k, v) => value);
        }
        public void    Add(string key, Mission value)
        {
            value.Campaign = this;
            value.Visor = new MultiTaskVisor(value);
            Missions.AddOrUpdate(key, value, (k, v) => value);
        }
        public void    Add(string key, IList<Objective> value)
        {
            Mission msn = new Mission(key, value);
            msn.Campaign = this;
            msn.Visor = new MultiTaskVisor(msn);
            Missions.AddOrUpdate(key, msn, (k, v) => msn);
        }
        public Mission Add(string key, IList<MultiTaskMethod> value)
        {
            Mission msn = new Mission(key, value);
            msn.Campaign = this;
            msn.Visor = new MultiTaskVisor(msn);
            Missions.AddOrUpdate(key, msn, (k, v) => v = msn);
            return msn;
        }
        public void    Add(string key, MultiTaskMethod value)
        {
            List<MultiTaskMethod> cml = new List<MultiTaskMethod>() { value };
            Mission msn = new Mission(key, cml);
            msn.Campaign = this;
            Missions.AddOrUpdate(key, msn, (k, v) => msn);
        }

        public KeyValuePair<string, Mission> SetPair
        {
            set
            {
                Add(value.Key, value.Value);
            }
        }
        public ConcurrentDictionary<string, Mission> Missions
        { get; } = new ConcurrentDictionary<string, Mission>(System.Environment.ProcessorCount * 2, 101);
        public Mission this[string key]
        {
            get
            {
                Mission result = null;
                Missions.TryGetValue(key, out result);
                return result;
            }
            set
            {
                value.Campaign = this;
                value.Visor = new MultiTaskVisor(value);
                Missions.AddOrUpdate(key, value, (k, v) => value);
            }
        }
    }

    public class Mission 
    {
        public Campaign Campaign { get; set; }

        public Mission(string Name)
        {
            MissionName = Name;
            MultiTaskCount = 1;           
        }
        public Mission(string Name, IList<Objective> ObjectiveList)
        {
            MissionName = Name;        
            foreach (Objective objective in ObjectiveList)
            {
                objective.Campaign = Campaign;
                objective.Mission = this;
                Objectives.AddOrUpdate(objective.Name, objective, (k, v) => objective);
            }
            MultiTaskCount = 1;
        }
        public Mission(string Name, IList<MultiTaskMethod> MethodList)
        {
            MissionName = Name;           
            foreach (MultiTaskMethod method in MethodList)
            {
                Objective objective = new Objective(method.MultiTaskName, method);
                objective.Campaign = Campaign;
                objective.Mission = this;
                Objectives.AddOrUpdate(method.MultiTaskName, objective, (k, v) =>v = objective);
            }
            MultiTaskCount = 1;
        }

        public int MultiTaskCount { get; set; }

        public MultiTaskVisor Visor { get; set; }

        public string MissionName { get; set; }

        public ConcurrentDictionary<string, Objective> Objectives
        { get; }  = new ConcurrentDictionary<string, Objective>(System.Environment.ProcessorCount * 2, 101);     

        public Objective Get(string key)
        {
            Objective result = null;
            Objectives.TryGetValue(key, out result);
            return result;
        }
        public void Add(Objective value)
        {
            value.Campaign = Campaign;
            value.Mission = this;
            Objectives.AddOrUpdate(value.Worker.WorkerName, value, (k, v) => value);
        }
        public void Add(MultiTaskMethod value)
        {
            Objective obj = new Objective(value.MultiTaskName, value);
            obj.Campaign = Campaign;
            obj.Mission = this;
            Objectives.AddOrUpdate(obj.Worker.WorkerName, obj, (k, v) => obj);
        }      
        public KeyValuePair<string, Objective> SetPair
        {
            set
            {
                Add(value.Value);
            }
        }
        public void AddRange(List<Objective> value)
        {
            foreach (Objective obj in value)
            {
                obj.Campaign = Campaign;
                obj.Mission = this;
                Objectives.AddOrUpdate(obj.Name, obj, (k, v) => obj);
            }
        }
        public void AddRange(List<MultiTaskMethod> value)
        {
            foreach (MultiTaskMethod obj in value)
            {
                Objective oj = new Objective(obj.MultiTaskName, obj);
                oj.Campaign = Campaign;
                oj.Mission = this;
                Objectives.AddOrUpdate(obj.MultiTaskName, oj, (k, v) => oj);
            }
        }
      
        public Objective this[string key]
        {
            get
            {
                Objective result = null;
                Objectives.TryGetValue(key, out result);
                return result;
            }
            set
            {
                value.Campaign = Campaign;
                value.Mission = this;
                Objectives.AddOrUpdate(key, value, (k, v) => value);
            }
        }       
    }

    public class Objective : IDorsEvent
    {       
        public Objective(string name, MultiTaskMethod method)
        {
            Name = name;
            Worker = new MultiTaskWorker(name, method);
            Worker.Objective = this;
            PostBox = new MultiTaskBox(Worker.WorkerName);
            PostBox.ObjectiveBox = this;          
        }   
        public Objective(MultiTaskWorker worker)
        {
            Name = worker.WorkerName;
            Worker = worker;
            Worker.Objective = this;
            PostBox = new MultiTaskBox(Worker.WorkerName);
            PostBox.ObjectiveBox = this;
        }

        public string Name { get; set; }

        public MultiTaskWorker Worker { get; set; }       

        public Mission Mission { get; set; }
        public Campaign Campaign { get; set; }

        public MultiTaskBox PostBox { get; set; }
        public DorsInvokeInfo InvokeInfo
        {
            get => Worker.Work.InvokeInfo;
            set => Worker.Work.InvokeInfo = value;
        }

        public void GoWork(params object[] input)
        {
            Worker.Input = input;
            Mission.Visor.GoWork(Worker);
        }

        public object Execute(params object[] parameters)
        {
            this.GoWork(parameters);
            return null;
        }
    }   
}
