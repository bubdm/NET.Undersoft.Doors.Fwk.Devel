using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Doors;

namespace System.Doors.MultiTask
{
    public class MultiThread
    { 
        public MultiTaskRegistry Registry;

        public MultiThread()
        {
            Registry = new MultiTaskRegistry();
        }
        public MultiThread(MultiThread copyMultiTask)
        {
            Registry = new MultiTaskRegistry();
            AddMultiTaskRange(copyMultiTask.Registry.Methods.Values.ToList());
        }
        public MultiThread(MultiTaskMethod antItem)
        {
            Registry = new MultiTaskRegistry();
            AddMultiTask(antItem);
        }
        public MultiThread(IList<MultiTaskMethod> antItems)
        {
            Registry = new MultiTaskRegistry();
            AddMultiTaskRange(antItems);
        }

        public void AddMultiTask(MultiTaskMethod antItem)
        {
            Registry.Set(antItem.MultiTaskName, antItem);
        }
        public void AddMultiTaskRange(IList<MultiTaskMethod> choreItems)
        {
            foreach (MultiTaskMethod ai in choreItems)
            {
                Registry.Set(ai.MultiTaskName, ai);
            }
        }

    }

    public class MultiTaskGo
    {
        public MultiTask Farm;
        public MultiTaskGo(string className, string methodName, params object[] input)
        {
            MultiTaskMethod am = new MultiTaskMethod(methodName, className);
            MultiThread _ant = new MultiThread(am);
            Farm = new MultiTask();
            Mission msn = Farm.Expanse(_ant, "MultiTaskGo_" + (Farm.Campaign.Missions.Count + 1).ToString());
            Farm.CallVisors();
            Farm.GoWork(am.MultiTaskName, input);
            msn.Visor.CloseMultiTask(false);
            GoMission = msn;
            Visor = GoMission.Visor;
        }
        public MultiTaskGo(bool safeClose, string className, string methodName, params object[] input)
        {
            MultiTaskMethod am = new MultiTaskMethod(methodName, className, input);
            MultiThread _ant = new MultiThread(am);
            Farm = new MultiTask();
            Mission msn = Farm.Expanse(_ant, "MultiTaskGo_" + (Farm.Campaign.Missions.Count + 1).ToString());
            Farm.CallVisors();
            Farm.GoWork(am.MultiTaskName, input);
            if (safeClose)
                msn.Visor.CloseMultiTask(safeClose);
            GoMission = msn;
            Visor = GoMission.Visor;
            Worker = GoMission.Objectives.Values.ElementAt(0).Worker;
        }
        public MultiTaskGo(object classObject, string methodName, params object[] input)
        {
            MultiTaskMethod am = new MultiTaskMethod(methodName, classObject, input);
            MultiThread _ant = new MultiThread(am);
            Farm = new MultiTask();
            Mission msn = Farm.Expanse(_ant, "MultiTaskGo_" + (Farm.Campaign.Missions.Count + 1).ToString());
            Farm.CallVisors();
            Farm.GoWork(am.MultiTaskName, input);
            msn.Visor.CloseMultiTask(false);
            GoMission = msn;
            Visor = GoMission.Visor;
            Worker = GoMission.Objectives.Values.ElementAt(0).Worker;
        }
        public MultiTaskGo(bool safeClose, object classObject, string methodName, params object[] input)
        {
            MultiTaskMethod am = new MultiTaskMethod(methodName, classObject, input);
            MultiThread _ant = new MultiThread(am);
            Farm = new MultiTask();
            Mission msn = Farm.Expanse(_ant, "MultiTaskGo_" + (Farm.Campaign.Missions.Count + 1).ToString());
            Farm.CallVisors();
            Farm.GoWork(am.MultiTaskName, input);
            if (safeClose)
                msn.Visor.CloseMultiTask(safeClose);
            GoMission = msn;
            Visor = GoMission.Visor;
            Worker = GoMission.Objectives.Values.ElementAt(0).Worker;
        }
        public MultiTaskGo(int threadCount, bool safeClose, List<MultiTaskMethod> methods)
        {
            MultiThread _ant = new MultiThread(methods);
            Farm = new MultiTask();
            Mission msn = Farm.Expanse(_ant, "MultiTaskGo_" + (Farm.Campaign.Missions.Count + 1).ToString(), threadCount);
            Farm.CallVisors();
            GoMission = msn;
            Visor = GoMission.Visor;
            Worker = GoMission.Objectives.Values.ElementAt(0).Worker;
            foreach (MultiTaskMethod m in methods)
                Farm.GoWork(m.MultiTaskName, m.InvokeInfo.Parameters);
            if(safeClose)
                msn.Visor.CloseMultiTask(safeClose);
        }
        public MultiTaskGo(int workerCount, int evokerCount, bool safeClose, MultiTaskMethod method, MultiTaskMethod evoker)
        {
            Farm = new MultiTask();
            Mission msn = Farm.Expanse(new MultiThread(method), "MultiTaskGo_" + (Farm.Campaign.Missions.Count + 1).ToString(), workerCount);
            Mission msn2 = Farm.Expanse(new MultiThread(evoker), "MultiTaskGo_" + (Farm.Campaign.Missions.Count + 1).ToString(), evokerCount);
            Farm.CallVisors();
            GoMission = msn;
            Visor = GoMission.Visor;
            Worker = GoMission.Objectives.Values.ElementAt(0).Worker;
            Worker.AddEvoker(msn2.Objectives.First().Value);
            Farm.GoWork(method.MultiTaskName, method.InvokeInfo.Parameters);
            if (safeClose)
                msn.Visor.CloseMultiTask(safeClose);
        }
        //public MultiTaskGo(object classObject, string methodName)
        //{
        //    MultiTaskMethod am = new MultiTaskMethod(methodName, classObject);
        //    MultiThread _ant = new MultiThread(am);
        //    Farm = new MultiTask();
        //    Mission msn = Farm.Expanse(_ant, "MultiTaskGo_" + (Farm.Campaign.Missions.Count + 1).ToString());
        //    Farm.CallVisors();
        //    Farm.GoWork(am.MultiTaskName);
        //    msn.Visor.CloseMultiTask(false);
        //    GoMission = msn;
        //    Visor = GoMission.Visor;
        //    Worker = GoMission.Objectives.Values.ElementAt(0).Worker;
        //}
        public void Close(bool safeClose = false)
        {
            GoMission.Visor.CloseMultiTask(safeClose);
        }
        public void GoWork()
        {
            Visor.GoWork(this.Worker);
        }
        public void GoWork(params object[] input)
        {
            this.Worker.Input = input;
            Visor.GoWork(this.Worker);
        }
        public Mission GoMission;
        public MultiTaskVisor Visor;
        public MultiTaskWorker Worker;
    }

    public class MultiTask
    {
        public MultiTask(MultiThread _ant, Campaign _campaign = null)
        {
            ant = new MultiThread();
            if (_ant.Registry.Methods != null && (_ant.Registry.Methods.Count > 0))
                foreach(MultiTaskMethod cr in _ant.Registry.Methods.Values)
                        ant.Registry.Set(cr.MultiTaskName, cr);
            else
                ant = _ant;
            if (_campaign == null)
                campaign = new Campaign("MultiTaskWorld", new MultiTaskPost());
            else
                campaign = _campaign;
            campaign.Add("MultiTaskDestiny", _ant.Registry.Methods.Values.ToList());
            Campaign = campaign;
            Post = Campaign.IOPost;
            BuildFarm();
        }
        public MultiTask()
        {
            ant = new MultiThread();
            campaign = new Campaign("MultiTaskWorld");
            Post = campaign.IOPost;
        }

        private MultiThread ant;        
     
        public MultiTaskPost Post
        {
            get;
            set;
        }

        private Campaign campaign;
        public Campaign Campaign
        {
            get
            {
                return campaign;
            }
            set
            {
                campaign = value;
            }
        }
      
        public void Expanse(MultiThread _ant, Mission _mission = null)
        {
            if (ant.Registry.Methods != null && (ant.Registry.Methods.Count > 0))
                foreach (MultiTaskMethod cr in _ant.Registry.Methods.Values)
                    ant.Registry.Set(cr.MultiTaskName, cr);
            else
                ant = _ant;
            if (_mission != null)
            {
                _mission.AddRange(_ant.Registry.Methods.Values.ToList());
                Campaign.Add(_mission);
            }
        }
        public void Expanse(MultiTaskMethod antitem, Mission _mission = null)
        {
            MultiThread _ant = new MultiThread(new List<MultiTaskMethod>() { antitem }); 
            if (ant.Registry.Methods != null && (ant.Registry.Methods.Count > 0))
                foreach (MultiTaskMethod cr in _ant.Registry.Methods.Values)
                    ant.Registry.Set(cr.MultiTaskName, cr);
            else
                ant = _ant;
            if(_mission != null)
            {
                _mission.AddRange(_ant.Registry.Methods.Values.ToList());
                Campaign.Add(_mission);
            }
        }
        public void Expanse(List<MultiTaskMethod> antitems, Mission _mission = null)
        {
            MultiThread _ant = new MultiThread(new List<MultiTaskMethod>(antitems));
            if (ant.Registry.Methods != null && (ant.Registry.Methods.Count > 0))
                foreach (MultiTaskMethod cr in _ant.Registry.Methods.Values)
                    ant.Registry.Set(cr.MultiTaskName, cr);
            else
                ant = _ant;
            if (_mission != null)
            {
                _mission.AddRange(_ant.Registry.Methods.Values.ToList());
                Campaign.Add(_mission);
            }
        }
        public Mission Expanse(MultiThread _ant, string missionName, int antcount = 1)
        {
            if (ant.Registry.Methods != null && (ant.Registry.Methods.Count > 0))
                foreach (MultiTaskMethod cr in _ant.Registry.Methods.Values)
                    ant.Registry.Set(cr.MultiTaskName, cr);
            else
                ant = _ant;
            Mission msn = Campaign.Add(missionName, _ant.Registry.Methods.Values.ToList());
            msn.Visor.CreateMultiTask(antcount);
            return msn;
        }
        public Mission Expanse(MultiTaskMethod antitem, string missionName, int antcount = 1)
        {
            MultiThread _ant = new MultiThread(new List<MultiTaskMethod>() { antitem });
            if (ant.Registry.Methods != null && (ant.Registry.Methods.Count > 0))
                foreach (MultiTaskMethod cr in _ant.Registry.Methods.Values)
                    ant.Registry.Set(cr.MultiTaskName, cr);
            else
                ant = _ant;
            Mission msn = Campaign.Add(missionName, _ant.Registry.Methods.Values.ToList());
            msn.Visor.CreateMultiTask(antcount);
            return msn;
        }
        public Mission Expanse(List<MultiTaskMethod> antitems, string missionName, int antcount = 1)
        {
            MultiThread _ant = new MultiThread(new List<MultiTaskMethod>(antitems));
            if (ant.Registry.Methods != null && (ant.Registry.Methods.Count > 0))
                foreach (MultiTaskMethod cr in _ant.Registry.Methods.Values)
                    ant.Registry.Set(cr.MultiTaskName, cr);
            else
                ant = _ant;
            Mission msn = Campaign.Add(missionName, _ant.Registry.Methods.Values.ToList());
            msn.Visor.CreateMultiTask(antcount);
            return msn;
        }

        public void BuildFarm()
        {
            CallVisors();
        }
        public void CallVisors()
        {
            foreach (Mission mission in campaign.Missions.Values)
            {
                if (mission.Visor == null)
                {
                    mission.Visor = new MultiTaskVisor(mission);
                }
                if (!mission.Visor.Ready)
                {
                    mission.Visor.CreateMultiTask();
                }
            }
        }

        public void GoWork(string workerName, params object[] input)
        {
            List<Objective> workerObjectives = Campaign.Missions
                .Values.Where(m => m.Objectives.ContainsKey(workerName))
                    .SelectMany(w => w.Objectives.Values).ToList();
            foreach(Objective objc in workerObjectives)
                objc.GoWork(input);
        }
        public void GoWorkers(Dictionary<string, object[]> workersAndInputs)
        {
            foreach (KeyValuePair<string, object[]> worker in workersAndInputs)
            {
                object input = worker.Value;
                string workerName = worker.Key;
                List<Objective> workerObjectives = Campaign.Missions.Values.Where(m => m.Objectives.ContainsKey(workerName)).SelectMany(w => w.Objectives.Values).ToList();
                foreach (Objective objc in workerObjectives)
                    objc.GoWork(input);
            }
        }
      
        public Mission this[string missionName]
        {
            get
            {
                return Campaign[missionName];
            }
        }

    }

    public class MultiTaskVisor
    {
        readonly object holder = new object();
        readonly object holderIO = new object();

        private Thread[] MultiTasks;
        private ConcurrentQueue<MultiTaskWorker> MultiTaskQueue = 
            new ConcurrentQueue<MultiTaskWorker>();

        public MultiTaskPost IOPost;
        public Mission Mission;
        public Campaign Campaign;
        public bool Ready;
        private int MultiTaskCount;

        public MultiTaskVisor(Mission mission)
        {
            Mission = mission;
            Campaign = Mission.Campaign;
            IOPost = Campaign.IOPost;
            MultiTaskCount = Mission.MultiTaskCount;
            Ready = false;
        }

        public void CreateMultiTask(int antcount = 1)
        {
            if (antcount > 1)
            {
                MultiTaskCount = antcount;
                Mission.MultiTaskCount = antcount;
            }
            else
                MultiTaskCount = Mission.MultiTaskCount;

            MultiTasks = new Thread[MultiTaskCount];
            // Create and start a separate thread for each MultiTask
            for (int i = 0; i < MultiTaskCount; i++)
            {               
                MultiTasks[i] = new Thread(StartMultiTask);
                MultiTasks[i].IsBackground = true;
                MultiTasks[i].Start();               
            }
        }
        public void ResetMultiTask(int antcount = 1)
        {
            CloseMultiTask(true);
            CreateMultiTask(antcount);
        }
        public void GoWork(MultiTaskWorker worker)
        {
            lock (holder)
            {
                if (worker != null)
                {
                    MultiTaskWorker Worker = CloneWorker(worker);
                    Monitor.Pulse(holder);
                    MultiTaskQueue.Enqueue(Worker);
                }
                else
                {
                    Monitor.Pulse(holder);
                    MultiTaskQueue.Enqueue(worker);
                }
            }
        }

        private void StartMultiTask()
        {
            while (true)
            {
                MultiTaskWorker worker;
                object input = null;
                lock (holder)
                {
                    do
                    {
                        while (MultiTaskQueue.Count == 0)
                        {
                            Monitor.Wait(holder);
                        }
                    } while (!MultiTaskQueue.TryDequeue(out worker));

                    if (worker != null)
                        input = worker.Input;
                }

                if (worker == null)
                    return;

                /*--------MultiTask DELEGATE START-------------*/

                object output = null;

                if (input != null)
                    output = worker.Work.Execute(input);
                else
                    output = worker.Work.Execute();

                Outpost(worker, output);

                /*--------CHORE&ENTRY&OUTPUT-------------*/ 

                if (worker == null)
                {
                    return;
                }
            }
        }
        public void  CloseMultiTask(bool SafeClose)
        {
            // Enqueue one null item per worker to make each exit.
            foreach (Thread MultiTask in MultiTasks)
            {
                GoWork(null);
            }
            // Wait for workers to finish
            if (SafeClose)
            {
                foreach (Thread MultiTask in MultiTasks)
                {
                    MultiTask.Join();
                }
            }
        }

        private MultiTaskWorker CloneWorker(MultiTaskWorker worker)
        {
            MultiTaskWorker Worker = new MultiTaskWorker();
            Worker.WorkerName = worker.WorkerName;
            object input = worker.Input;
            //if (input != null)               
            //{
            //    MultiTaskIO antio = new MultiTaskIO(worker.Objective, input, null, null, worker.EvokersIn);                              
            //    Worker.Input = antio;
            //}
            //else
            //{
            //    MultiTaskIO antio = new MultiTaskIO(worker.Objective, null, null, null, worker.EvokersIn);
            //    antio.SenderObjective = worker.Objective;
            //    antio.SenderName = worker.WorkerName;
            //    antio.EvokersIn = worker.EvokersIn;
            //    Worker.Input = antio;
            //}
            
            Worker.EvokersIn = worker.EvokersIn;
            Worker.Objective = worker.Objective;
            Worker.Work = worker.Work;
            return Worker;
        }

        private void Outpost(MultiTaskWorker worker, object output)
        {
            if (output != null)
            {
                worker.Output = output;

                if (worker.EvokersIn != null && worker.EvokersIn.Any())
                {
                    List<MultiTaskIO> intios = new List<MultiTaskIO>();
                    foreach (MultiTaskEvoker evoker in worker.EvokersIn)
                    {
                        MultiTaskIO intio = new MultiTaskIO(worker.Objective, output, evoker.RecipientObjective, evoker);
                        intio.SenderBox = worker.Objective.PostBox;
                        intios.Add(intio);
                    }

                    if (intios.Any())
                        IOPost.Send(intios);
                }
               
            }
        }
    }

    public class MultiTaskWorker
    {
        public MultiTaskWorker()
        {
            input = new ConcurrentQueue<object>();
            output = new ConcurrentQueue<object>();
            EvokersIn = new MultiTaskEvokers();
        }
        public MultiTaskWorker(string Name, IDorsEvent Method)
        {
            Work = Method;
            WorkerName = Name;
            input = new ConcurrentQueue<object>();
            output = new ConcurrentQueue<object>();
            EvokersIn = new MultiTaskEvokers();
        }       

        public string WorkerName { get; set; }

        public IDorsEvent Work
        { get; set; }

        public Objective Objective
        { get; set; }

        private ConcurrentQueue<object> input;
        public object Input
        {

            get
            {
                object _entry = null;
                if (input.TryDequeue(out _entry))
                    return _entry;
                return null;
            }
            set
            {
                input.Enqueue(value);
            }


        }

        private ConcurrentQueue<object> output;
        public object Output
        {
            get
            {
                object _result = null;
                if (output.TryDequeue(out _result))
                    return _result;
                return null;
            }
            set
            {
                output.Enqueue(value);
            }

        }

        public void AddEvoker(string RecipientName, List<string> RelayNames)
        {
            EvokersIn.Add(new MultiTaskEvoker(Objective, RecipientName, RelayNames));
        }
        public void AddEvoker(Objective RecipientObjective, List<Objective> RelayObjectives)
        {
            EvokersIn.Add(new MultiTaskEvoker(Objective, RecipientObjective, RelayObjectives));
        }
        public void AddEvoker(string RecipientName)
        {
            EvokersIn.Add(new MultiTaskEvoker(Objective, RecipientName, new List<string>() { WorkerName }));
        }
        public void AddEvoker(Objective RecipientObjective)
        {
            EvokersIn.Add(new MultiTaskEvoker(Objective, RecipientObjective, new List<Objective>() { Objective }));
        }

        public MultiTaskEvokers EvokersIn { get; set; }
    }

    public class InvalidMultiTaskException : Exception
    {
        public InvalidMultiTaskException(string message)
            : base(message)
        { }
    }
}
