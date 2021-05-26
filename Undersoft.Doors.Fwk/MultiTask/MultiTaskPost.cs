using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Doors.MultiTask
{   
    public class MultiTaskPost
    {
        public MultiTaskPost()
        {
            iopost = new ConcurrentDictionary<string, MultiTaskBox>(System.Environment.ProcessorCount * 2, 101);
        }

        private Campaign CampaignPost { get; set; }

        private ConcurrentDictionary<string, MultiTaskBox> iopost;
        public MultiTaskBox Get(string key)
        {
            MultiTaskBox result = null;
            iopost.TryGetValue(key, out result);
            return result;
        }
        public void AddRecipient(string key, MultiTaskBox ioBox)
        {
            if (ioBox != null)
            {
                if (ioBox.ObjectiveBox != null)
                {
                    Objective objv = ioBox.ObjectiveBox;
                    iopost.AddOrUpdate(ioBox.RecipientName, ioBox, (k, v) => ioBox);
                }
                else
                {
                    List<Objective> objvl = CampaignPost.Missions.Where(m => m.Value.Objectives.ContainsKey(key)).SelectMany(os => os.Value.Objectives.Select(o => o.Value)).ToList();
                    if (objvl.Any())
                    {
                        Objective objv = objvl.First();
                        ioBox.ObjectiveBox = objv;
                        iopost.AddOrUpdate(key, ioBox, (k, v) => ioBox);
                    }
                }
            }
            else
            {
                List<Objective> objvl = CampaignPost.Missions.Where(m => m.Value.Objectives.ContainsKey(key)).SelectMany(os => os.Value.Objectives.Select(o => o.Value)).ToList();
                if (objvl.Any())
                {
                    Objective objv = objvl.First();
                    MultiTaskBox iobox = new MultiTaskBox(objv.Worker.WorkerName);
                    iobox.ObjectiveBox = objv;
                    iopost.AddOrUpdate(key, iobox, (k, v) => ioBox);
                }
            }
        }
        public void SetRecipient(MultiTaskBox value)
        {
            if (value != null)
            {
                if (value.ObjectiveBox != null)
                {
                    Objective objv = value.ObjectiveBox;
                    iopost.AddOrUpdate(value.RecipientName, value, (k, v) => value);
                }
                else
                {
                    List<Objective> objvl = CampaignPost.Missions.Where(m => m.Value.Objectives.ContainsKey(value.RecipientName)).SelectMany(os => os.Value.Objectives.Select(o => o.Value)).ToList();
                    if (objvl.Any())
                    {
                        Objective objv = objvl.First();
                        value.ObjectiveBox = objv;
                        iopost.AddOrUpdate(value.RecipientName, value, (k, v) => value);
                    }
                }
            }
        }
        public KeyValuePair<string, MultiTaskBox> AddPair
        {
            set
            {
                AddRecipient(value.Key, value.Value);
            }
        }  
        public ConcurrentDictionary<string, MultiTaskBox> Boxes
        {
            get
            {
                return iopost;
            }
        }
        public MultiTaskBox this[string key]
        {
            get
            {
                MultiTaskBox result = null;
                iopost.TryGetValue(key, out result);
                return result;
            }         
        }

        public void Send(MultiTaskIO io)
        {
            if (io.RecipientName != null && io.SenderName != null)
            {
                if (Boxes.ContainsKey(io.RecipientName))
                {
                    MultiTaskBox iobox = Get(io.RecipientName);
                    if (iobox != null)
                        iobox.AddIO(io);
                }
                else if(io.RecipientObjective != null)
                {
                    Objective objv = io.RecipientObjective;
                    MultiTaskBox iobox = new MultiTaskBox(objv.Worker.WorkerName);
                    iobox.ObjectiveBox = objv;
                    iobox.AddIO(io);
                    SetRecipient(iobox);
                }
                else if (CampaignPost != null)
                {
                    List<Objective> objvl = CampaignPost.Missions.Where(m => m.Value.Objectives.ContainsKey(io.RecipientName)).SelectMany(os => os.Value.Objectives.Select(o => o.Value)).ToList();
                    if (objvl.Any())
                    {
                        Objective objv = objvl.First();
                        MultiTaskBox iobox = new MultiTaskBox(objv.Worker.WorkerName);
                        iobox.ObjectiveBox = objv;
                        iobox.AddIO(io);
                        SetRecipient(iobox);
                    }
                }
            }

        }
        public void Send(List<MultiTaskIO> ios)
        {
            foreach (MultiTaskIO io in ios)
            {
                if (io.RecipientName != null && io.SenderName != null)
                {
                    if (Boxes.ContainsKey(io.RecipientName))
                    {
                        MultiTaskBox iobox = Get(io.RecipientName);
                        if (iobox != null)
                            iobox.AddIO(io);
                    }
                    else if (io.RecipientObjective != null)
                    {
                        Objective objv = io.RecipientObjective;
                        MultiTaskBox iobox = new MultiTaskBox(objv.Worker.WorkerName);
                        iobox.ObjectiveBox = objv;
                        iobox.AddIO(io);
                        SetRecipient(iobox);
                    }
                    else if (CampaignPost != null)
                    {
                        List<Objective> objvl = CampaignPost.Missions.Where(m => m.Value.Objectives.ContainsKey(io.RecipientName)).SelectMany(os => os.Value.Objectives.Select(o => o.Value)).ToList();
                        if (objvl.Any())
                        {
                            Objective objv = objvl.First();
                            MultiTaskBox iobox = new MultiTaskBox(objv.Worker.WorkerName);
                            iobox.ObjectiveBox = objv;
                            iobox.AddIO(io);
                            SetRecipient(iobox);
                        }
                    }
                }
            }

        }
        public void Send(MultiTaskIO[] ios)
        {
            if (ios.Any())
            {
                MultiTaskIO iod = ios.First();
                if (iod.RecipientName != null && iod.SenderName != null)
                {
                    if (Boxes.ContainsKey(iod.RecipientName))
                    {
                        MultiTaskBox iobox = Get(iod.RecipientName);
                        if (iobox != null)
                            iobox.AddIO(ios);
                    }
                    else if (iod.RecipientObjective != null)
                    {
                        Objective objv = iod.RecipientObjective;
                        MultiTaskBox iobox = new MultiTaskBox(objv.Worker.WorkerName);
                        iobox.ObjectiveBox = objv;
                        iobox.AddIO(iod);
                        SetRecipient(iobox);
                    }
                    else if (CampaignPost != null)
                    {
                        List<Objective> objvl = CampaignPost.Missions.Where(m => m.Value.Objectives.ContainsKey(iod.RecipientName)).SelectMany(os => os.Value.Objectives.Select(o => o.Value)).ToList();
                        if (objvl.Any())
                        {
                            Objective objv = objvl.First();
                            MultiTaskBox iobox = new MultiTaskBox(objv.Worker.WorkerName);
                            iobox.ObjectiveBox = objv;
                            iobox.AddIO(ios);
                            SetRecipient(iobox);
                        }
                    }
                }
            }
        }
    }

    public class MultiTaskBox
    {
        public MultiTaskBox(string Recipient)
        {
            RecipientName = Recipient;
            Evokers = new MultiTaskEvokers();
        }

        public string RecipientName { get; set; }

        public Objective ObjectiveBox { get; set; }

        public object GetObject(string key)
        {
            MultiTaskQueue _ioqueue = null;
            if (Queues.TryGetValue(key, out _ioqueue))
            {
                return _ioqueue.TakeOut();
            }
            return null;
        }
        public MultiTaskIO GetMultiTaskIO(string key)
        {
            MultiTaskQueue _ioqueue = null;
            if (Queues.TryGetValue(key, out _ioqueue))
            {
                return _ioqueue.IO;
            }
            return null;
        }

        public void AddIO(MultiTaskIO value)
        {
            if (value.SenderName != null)
            {              
                MultiTaskQueue queue = null;
                if (!Queues.ContainsKey(value.SenderName))
                {
                    queue = new MultiTaskQueue(value.SenderName, this);
                    if (Queues.TryAdd(value.SenderName, queue))
                    {
                        if (value.EvokerOut != null)
                            Evokers.Add(value.EvokerOut);
                        queue.AddOn(value);
                    }
                }
                else if (Queues.TryGetValue(value.SenderName, out queue))
                {
                    if (value.EvokerOut != null)
                        Evokers.Add(value.EvokerOut);
                    queue.AddOn(value);
                }
            }
        }
        public void AddIO(List<MultiTaskIO> value)
        {
            if (value != null && value.Any())
            {
                foreach (MultiTaskIO antio in value)
                {
                    MultiTaskQueue queue = null;
                    if (antio.SenderName != null)
                    {
                        if (!Queues.ContainsKey(antio.SenderName))
                        {
                            queue = new MultiTaskQueue(antio.SenderName, this);
                            if (Queues.TryAdd(antio.SenderName, queue))
                            {
                                if (antio.EvokerOut != null)
                                    Evokers.Add(antio.EvokerOut);                            
                                queue.AddOn(antio);
                            }
                        }
                        else if (Queues.TryGetValue(antio.SenderName, out queue))
                        {
                            if (value != null && value.Count > 0)
                            {
                                if (antio.EvokerOut != null)
                                    Evokers.Add(antio.EvokerOut);                           
                                queue.AddOn(antio);
                            }
                        }
                    }
                }
            }
        }
        public void AddIO(MultiTaskIO[] value)
        {
            if (value != null && value.Any())
            {
                foreach (MultiTaskIO antio in value)
                {
                    MultiTaskQueue queue = null;
                    if (antio.SenderName != null)
                    {
                        if (!Queues.ContainsKey(antio.SenderName))
                        {
                            queue = new MultiTaskQueue(antio.SenderName, this);
                            if (Queues.TryAdd(antio.SenderName, queue))
                            {
                                if (antio.EvokerOut != null)
                                    Evokers.Add(antio.EvokerOut);
                                queue.AddOn(antio);
                            }
                        }
                        else if (Queues.TryGetValue(antio.SenderName, out queue))
                        {
                            if (value != null && value.Length > 0)
                            {
                                if (antio.EvokerOut != null)
                                    Evokers.Add(antio.EvokerOut);
                                queue.AddOn(antio);
                            }
                        }
                    }
                }
            }
        }
        public void AddIO(string key, MultiTaskIO value)
        {       
            value.SenderName = key;
            MultiTaskQueue queue = null;
            if (!Queues.ContainsKey(key))
            {
                queue = new MultiTaskQueue(key, this);
                if (Queues.TryAdd(key, queue))
                {
                    if (value.EvokerOut != null)
                        Evokers.Add(value.EvokerOut);
                    queue.AddOn(value);
                }
            }
            else if (Queues.TryGetValue(key, out queue))
            {
                if (value.EvokerOut != null)
                    Evokers.Add(value.EvokerOut);
                queue.AddOn(value);
            }
        }
        public void AddIO(string key, List<MultiTaskIO> value)
        {
            MultiTaskQueue queue = null;
            if (!Queues.ContainsKey(key))
            {
                queue = new MultiTaskQueue(key, this);
                if (Queues.TryAdd(key, queue) && value != null && value.Count > 0)
                {
                    foreach (MultiTaskIO ioqueue in value)
                    {
                        if (ioqueue.EvokerOut != null)
                            Evokers.Add(ioqueue.EvokerOut);                     
                        ioqueue.SenderName = key;
                        queue.AddOn(ioqueue);
                    }
                }
            }
            else if (Queues.TryGetValue(key, out queue))
            {
                if (value != null && value.Count > 0)
                {
                    foreach (MultiTaskIO ioqueue in value)
                    {
                        if (ioqueue.EvokerOut != null)
                            Evokers.Add(ioqueue.EvokerOut);                     
                        ioqueue.SenderName = key;
                        queue.AddOn(ioqueue);
                    }
                }
            }
        }
        public void AddIO(string key, object ioqueues)
        {
            MultiTaskQueue queue = null;
            if (!Queues.ContainsKey(key))
            {
                queue = new MultiTaskQueue(key, this);
                if (Queues.TryAdd(key, queue) && ioqueues != null)
                {
                    queue.AddOn(ioqueues);
                }
            }
            else if (Queues.TryGetValue(key, out queue))
            {
                if (ioqueues != null)
                {
                    queue.AddOn(ioqueues);
                }
            }
        }

        public MultiTaskQueue GetQueue(string key)
        {
            MultiTaskQueue _ioqueue = null;
            Queues.TryGetValue(key, out _ioqueue);
            return _ioqueue;
        }
        public List<MultiTaskIO> TakeItems(List<string> keys)
        {
            List<MultiTaskIO> antios = Queues.Where(q => keys.Contains(q.Key)).Select(v => v.Value.IO).ToList();
            return antios;
        }
        public void AddNewQueue(string key, MultiTaskQueue value = null)
        {
            if (value != null)
                Queues.AddOrUpdate(key, value, (k, v) => value);
            else
                Queues.AddOrUpdate(key, new MultiTaskQueue(key, this), (k, v) => v = new MultiTaskQueue(key, this));
        }
        public void SetQueue(MultiTaskQueue value)
        {
            if (value != null)
                Queues.AddOrUpdate(value.SenderName, value, (k, v) => v = value);          
        }
        public KeyValuePair<string, MultiTaskQueue> AddPair
        {
            set
            {
                AddNewQueue(value.Key, value.Value);
            }
        }
        public ConcurrentDictionary<string, MultiTaskQueue> Queues
        { get; } = new ConcurrentDictionary<string, MultiTaskQueue>();

        public MultiTaskQueue this[string key]
        {
            get
            {
                MultiTaskQueue _ioqueue = null;
                Queues.TryGetValue(key, out _ioqueue);
                return _ioqueue;
            }
            set
            {
                value.RecipientBox = this;
                value.SenderName = key;
                Queues.AddOrUpdate(key, value, (k, v) => v = value);
            }
        }
        
        public MultiTaskEvokers Evokers { get; set; }

        public void QualifyToEvoke()
        {
            List<MultiTaskEvoker> toEvoke = new List<MultiTaskEvoker>();
            foreach (MultiTaskEvoker relay in Evokers)
            {
                if (relay.RelayNames.All(r => Queues.ContainsKey(r)))
                    if (relay.RelayNames.All(r => Queues[r].Any()))
                        toEvoke.Add(relay);
            }
            
            if(toEvoke.Any())
            {
                foreach (MultiTaskEvoker evoke in toEvoke)
                {
                    List<MultiTaskIO> antios = TakeItems(evoke.RelayNames);
                    object[] io = new object[0];
                    object begin = ObjectiveBox.Worker.Input;
                    if (begin != null)
                        io.Concat((object[])begin);
                    foreach (MultiTaskIO antio in antios)
                        io.Concat(antio.Parameters);

                    ObjectiveBox.GoWork(io);
                }
            }
        }
    }

    public class MultiTaskQueue
    {       
        public MultiTaskQueue(string senderName, MultiTaskBox recipient = null)
        {
            if (recipient != null)
                RecipientBox = recipient;
            SenderName = senderName;
            ioqueue = new ConcurrentQueue<MultiTaskIO>();
        }
        public MultiTaskQueue(string senderName, MultiTaskIO antIO, MultiTaskBox recipient = null)
        {
            if (recipient != null)
            {
                RecipientBox = recipient;               
            }
            SenderName = senderName;
            ioqueue = new ConcurrentQueue<MultiTaskIO>();
            IO = antIO;
        }
        public MultiTaskQueue(string senderName, List<MultiTaskIO> antios, MultiTaskBox recipient = null)
        {
            if (recipient != null)
            {
                RecipientBox = recipient;
            }
            if (antios != null && antios.Count > 0)
            {
                foreach (MultiTaskIO antio in antios)
                {                  
                    antio.SenderName = SenderName;
                    IO = antio;
                }
            }
        }
        public MultiTaskQueue(string senderName, object IO, MultiTaskBox recipient = null)
        {
            if (recipient != null)
                RecipientBox = recipient;
            SenderName = senderName;
            if (IO != null)
            {
                if (IO.GetType() == typeof(Dictionary<string, object>))
                {
                    MultiTaskIO result = new MultiTaskIO(senderName, IO);
                    IO = result;
                }
            }

        }

        public MultiTaskBox RecipientBox;
        public string SenderName { get; set; }

        public bool Any()
        {
            return ioqueue.Count > 0;
        }
        public int  Count
        {
            get
            {
                return ioqueue.Count;
            }
        }

        public void AddOn(string senderName, object io)
        {
            SenderName = senderName;
            if (io != null)
            {
                MultiTaskIO result = new MultiTaskIO(senderName);
                if (io.GetType() == typeof(object[]))
                    result.Parameters = (object[])io;
                else
                    result.Parameters = new object[] { io };
                IO = result;            
            }
        }
        public void AddOn(object io)
        {
            if (io != null)
            {
                MultiTaskIO result = new MultiTaskIO(SenderName);
                if (io.GetType() == typeof(object[]))
                    result.Parameters = (object[])io;
                else
                    result.Parameters = new object[] { io };
                IO = result;
            }
        }
        public void AddOn(MultiTaskIO antio)
        {
            if (antio != null)
                IO = antio;
        }
        public void AddOn(List<MultiTaskIO> ioList)
        {
            if (ioList != null && ioList.Count > 0)
            {
                foreach (MultiTaskIO result in ioList)
                    IO = result;
            }
        }
        public object TakeOut()
        {
            return IO;
        }

        private ConcurrentQueue<MultiTaskIO> ioqueue;     
        public MultiTaskIO IO
        {
            get
            {          
                MultiTaskIO _result = null;
                if (ioqueue.TryDequeue(out _result))
                    return _result;
                else
                    return null;
            }
            set
            {
                value.SenderName = SenderName;
                ioqueue.Enqueue(value);
                if (RecipientBox != null)
                    RecipientBox.QualifyToEvoke();
            }
        }
    }

    //public class MultiTaskIO
    //{
    //    public MultiTaskIO(string senderName, object _io, string recipient = null, MultiTaskEvoker evokerout = null, MultiTaskEvokers evokersin = null)
    //    {
    //        if (recipient != null)       
    //            RecipientName = recipient;

    //        SenderName = senderName;

    //        if (evokersin != null)
    //            EvokersIn = evokersin;

    //        if (evokerout != null)
    //            EvokerOut = evokerout;

    //        if (_io != null)
    //        {
    //            if (_io is object[])
    //            {
    //                io = ((object[])_io).Select((o, y) => new KeyValuePair<string, object>(y.ToString(), o)).ToDictionary(k => k.Key, v => v.Value);
    //            }
    //            else if (_io is IDictionary)
    //            {
    //                io = new Dictionary<string, object>((Dictionary<string, object>)_io);
    //            }
    //            else if (_io.GetType() == typeof(MultiTaskIO))
    //            {
    //                io = new Dictionary<string, object>(((MultiTaskIO)_io).IO);
    //            }
    //            else
    //            {
    //                io = new Dictionary<string, object>() { { "io", _io } };
    //            }
    //        }
    //    }       
    //    public MultiTaskIO(string senderName, string recipient = null, MultiTaskEvoker evokerout = null, MultiTaskEvokers evokersin = null)
    //    {
    //        if (recipient != null)
    //            RecipientName = recipient;

    //        if (evokerout != null)
    //            EvokerOut = evokerout;

    //        if (evokersin != null)
    //            EvokersIn = evokersin;

    //        io = new Dictionary<string, object>();
    //    }
    //    public MultiTaskIO(Objective senderObjective, object _io, Objective recipient = null, MultiTaskEvoker evokerout = null, MultiTaskEvokers evokersin = null)
    //    {
    //        if (recipient != null)
    //        {
    //            RecipientObjective = recipient;
    //            RecipientName = recipient.Worker.WorkerName;
    //        }

    //        SenderObjective = senderObjective;
    //        SenderName = senderObjective.Worker.WorkerName;

    //        if (evokersin != null)
    //            EvokersIn = evokersin;

    //        if (evokerout != null)
    //            EvokerOut = evokerout;

    //        if (_io != null)
    //        {
    //            if(_io is object[])
    //            {
    //                io = ((object[])_io).Select((o, y) => new KeyValuePair<string, object>(y.ToString(), o)).ToDictionary(k => k.Key, v => v.Value);
    //            }
    //            else if (_io is IDictionary)
    //            {
    //                io = new Dictionary<string, object>((Dictionary<string, object>)_io);
    //            }
    //            else if(_io.GetType() == typeof(MultiTaskIO))
    //            {
    //                io = new Dictionary<string, object>(((MultiTaskIO)_io).IO);
    //            }
    //            else
    //            {
    //                io = new Dictionary<string, object>() { { "io", _io } };
    //            }
    //        }
    //    }
    //    public MultiTaskIO(Objective senderObjective, Objective recipient = null, MultiTaskEvoker evokerout = null, MultiTaskEvokers evokersin = null)
    //    {
    //        if (recipient != null)
    //        {
    //            RecipientObjective = recipient;
    //            RecipientName = recipient.Worker.WorkerName;
    //        }
    //        SenderObjective = senderObjective;
    //        SenderName = senderObjective.Worker.WorkerName;


    //        if (evokerout != null)
    //            EvokerOut = evokerout;
    //        if (evokersin != null)
    //            EvokersIn = evokersin;

    //        io = new Dictionary<string, object>();
    //    }

    //    public MultiTaskEvoker  EvokerOut { get; set; }
    //    public MultiTaskEvokers EvokersIn { get; set; }
       
    //    public MultiTaskBox SenderBox;
    //    public string RecipientName { get; set; }
    //    public Objective RecipientObjective { get; set; }
    //    public string SenderName { get; set; }
    //    public Objective SenderObjective { get; set; }

    //    private Dictionary<string, object> io;
    //    public object Get(string key)
    //    {
    //        object _result = null;
    //        io.TryGetValue(key, out _result);
    //        return _result;
    //    }
    //    public void Add(string key, object value)
    //    {
    //        if(!io.ContainsKey(key))
    //            io.Add(key, value);
    //    }
    //    public void Set(string key, object value)
    //    {
    //        if (!io.ContainsKey(key))
    //            io.Add(key, value);
    //        else
    //            io[key] = value;
    //    }
    //    public KeyValuePair<string, object> SetPair
    //    {
    //        set
    //        {
    //            Set(value.Key, value.Value);
    //        }
    //    }
    //    public void SetRange(Dictionary<string, object> value)
    //    {
    //        io.PutRange(value);
    //    }
    //    public void AddNewRange(Dictionary<string, object> value)
    //    {
    //        io.AddRange(value);
    //    }
    //    public object[] AsArray
    //    {
    //        get
    //        {
    //            return (io.Count > 0) ? io.Values.ToArray() : null;
    //        }
    //        set
    //        {

    //            io.Keys.Select((x, y) => io[x] = (value.Length > y) ? value[y] : io[x]).ToArray();
    //        }
    //    }
    //    public Dictionary<string, object> IO
    //    {
    //        get
    //        {
    //            return io;
    //        }
    //        set
    //        {
    //            io = value;
    //        }
    //    }
    //    public object this[string key]
    //    {
    //        get
    //        {
    //            object _result = null;
    //            io.TryGetValue(key, out _result);
    //            return _result;
    //        }
    //        set
    //        {
    //            if (io != null)
    //            {
    //                if (!io.ContainsKey(key))
    //                    io.Add(key, value);
    //                else
    //                    io[key] = value;
    //            }
    //            else
    //            {
    //                io = new Dictionary<string, object>();
    //                io.Add(key, value);
    //            }
    //        }
    //    }
    //}

    public class MultiTaskIO
    {
        public MultiTaskIO(string Sender, object Params = null, string Recipient = null, MultiTaskEvoker Out = null, MultiTaskEvokers In = null)
        {
            SenderName = Sender;

            if (Params != null)
                if (Params is object[])
                    Parameters = (object[])Params;
                else
                    Parameters = new object[] { Params };

            if (Recipient != null)
                RecipientName = Recipient;

            if (Out != null)
                EvokerOut = Out;

            if (In != null)
                EvokersIn = In;
        }
        public MultiTaskIO(Objective Sender, object Params = null, Objective Recipient = null, MultiTaskEvoker Out = null, MultiTaskEvokers In = null)
        {
            if (Params != null)
                if (Params is object[])
                    Parameters = (object[])Params;
                else
                    Parameters = new object[] { Params };

            if (Recipient != null)
            {
                RecipientObjective = Recipient;
                RecipientName = Recipient.Worker.WorkerName;
            }

            SenderObjective = Sender;
            SenderName = Sender.Worker.WorkerName;

            if (Out != null)
                EvokerOut = Out;

            if (In != null)
                EvokersIn = In;
        }

        public MultiTaskEvoker  EvokerOut { get; set; }
        public MultiTaskEvokers EvokersIn { get; set; }

        public MultiTaskBox SenderBox;

        public string    RecipientName { get; set; }
        public Objective RecipientObjective { get; set; }

        public string    SenderName { get; set; }
        public Objective SenderObjective { get; set; }

        public object[] Parameters;
    }

    public class MultiTaskEvokers : Collection<MultiTaskEvoker>
    {
        public MultiTaskEvokers()
        {
        }

        public new void Add(MultiTaskEvoker evoker) { if (!this.Have(evoker.RelayNames)) { base.Add(evoker); } }
        public void AddRange(List<MultiTaskEvoker> _evokers)
        {
            foreach (MultiTaskEvoker evoker in _evokers)
                Add(evoker);
        }
        public bool Have(List<Objective> objectives)
        {
            return Items.Where(t => t.RelayObjectives.Where(ro =>  objectives.All(o => ReferenceEquals(ro, o))).Any()).Any();
        }
        public bool Have(List<string> relayNames)
        {
            return Items.Where(t => t.RelayNames.SequenceEqual(relayNames)).Any();
        }

        public MultiTaskEvoker this[string RelayName]
        {
            get
            {
                return Items.Where(c => c.RelayNames.Contains(RelayName)).First();
            }
        }
        public MultiTaskEvoker this[Objective objective]
        {
            get
            {
                return Items.Where(c => c.RelayObjectives.Where(ro => ReferenceEquals(ro, objective)).Any()).First();
            }
        }      
    }

    public class MultiTaskEvoker
    {      
        public MultiTaskEvoker(Objective senderObjective, string recipientName, IList<string> relayNames)
        {
            SenderObjective = senderObjective;
            SenderName = senderObjective.Worker.WorkerName;
            List<Objective> objvl = SenderObjective.Campaign.Missions.Where(m => m.Value.Objectives.ContainsKey(recipientName)).SelectMany(os => os.Value.Objectives.Select(o => o.Value)).ToList();
            if(objvl.Any())
                RecipientObjective = objvl.First();
            RecipientName = recipientName;
            RelayNames = new List<string>(relayNames);
            RelayObjectives = SenderObjective.Campaign.Missions.Where(m => m.Value.Objectives.Keys.Where(k => relayNames.Contains(k)).Any()).SelectMany(os => os.Value.Objectives.Select(o => o.Value)).ToList();
        }
        public MultiTaskEvoker(Objective senderObjective, string recipientName, IList<Objective> relayObjectives)
        {
            SenderObjective = senderObjective;
            SenderName = senderObjective.Worker.WorkerName;
            RecipientName = recipientName;
            List<Objective> objvl = SenderObjective.Campaign.Missions
                                        .Where(m => m.Value.Objectives.ContainsKey(recipientName))
                                            .SelectMany(os => os.Value.Objectives.Select(o => o.Value)).ToList();
            if (objvl.Any())
                RecipientObjective = objvl.First();
            RelayObjectives = new List<Objective>(relayObjectives);
            RelayNames.AddRange(RelayObjectives.Select(rn => rn.Worker.WorkerName));
        }
        public MultiTaskEvoker(Objective senderObjective, Objective recipientObjective, List<Objective> relayObjectives)
        {
            SenderObjective = senderObjective;
            SenderName = senderObjective.Worker.WorkerName;
            RecipientObjective = recipientObjective;
            RecipientName = recipientObjective.Worker.WorkerName;
            RelayObjectives = relayObjectives;
            RelayNames.AddRange(RelayObjectives.Select(rn => rn.Worker.WorkerName));
        }
        public MultiTaskEvoker(Objective senderObjective, Objective recipientObjective, List<string> relayNames)
        {
            SenderObjective = senderObjective;
            SenderName = senderObjective.Worker.WorkerName;
            RecipientObjective = recipientObjective;
            RecipientName = recipientObjective.Worker.WorkerName;
            RelayNames = relayNames;
            RelayObjectives = SenderObjective.Campaign.Missions.Where(m => m.Value.Objectives.Keys.Where(k => relayNames.Contains(k)).Any()).SelectMany(os => os.Value.Objectives.Select(o => o.Value)).ToList();
        }
     
        public string EvokerName { get; set; }

        public string    RecipientName { get; set; }
        public Objective RecipientObjective { get; set; }

        public string SenderName { get; set; }
        public Objective SenderObjective { get; set; }

        public List<string>    RelayNames = new List<string>();
        public List<Objective> RelayObjectives = new List<Objective>();

        public MultiTaskEvokerType  EvokerType { get; set; }
    }

    public enum MultiTaskEvokerType
    {
        Always,
        Single,
        Schedule,
        Nome
    }

}
