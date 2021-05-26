using System.Text;
using System.Linq;
using System.Collections;
using System.Doors;
using System.Doors.Data;

namespace System.Doors.Data.Depot
{

    public class TransactionManager
    {
        private DepotTransaction transaction;
        private TransactionContext context;
        private DepotTreatment treatment;                                                                  // Important Field !!! - Depot Treatment initiatie, filtering, sorting, saving, editing all treatment here. 

        private IDepotContext icontext;
        private DepotSite site;
            
        public TransactionManager(DepotTransaction _transaction)
        {
            transaction = _transaction;
            icontext = transaction.Context;
            context = transaction.MyHeader.Context;
            site = context.IdentitySite;
            treatment = new DepotTreatment(_transaction);         
        }

        public void HeaderContent(object content, object value, DirectionType _direction)
        {
            DirectionType direction = _direction;
            object _content = value;
            if (_content != null)
            {              
                Type[] ifaces = _content.GetType().GetInterfaces();
                if (ifaces.Contains(typeof(IDataSerial)))
                {
                    transaction.MyHeader.Context.ContentType = _content.GetType();

                    if (direction == DirectionType.Send)
                        _content = ((IDataSerial)value).GetHeader();

                    object[] messages_ = null;                   
                    if (treatment.Assign(_content, direction, out messages_)                               // Depot Treatment assign with handle its only place where its called and mutate data. 
                    ){
                        if (messages_.Length > 0)
                        {                         
                            context.ObjectsCount = messages_.Length;
                            for (int i = 0; i < context.ObjectsCount; i++)
                            {
                                IDataSerial message = ((IDataSerial[])messages_)[i];
                                IDataSerial head = (IDataSerial)((IDataSerial[])messages_)[i].GetHeader();
                                message.SerialCount = message.ItemsCount;
                                head.SerialCount = message.ItemsCount;                            
                            }
                               
                            if (direction == DirectionType.Send)
                                transaction.MyMessage.Content = messages_;
                            else
                                transaction.MyMessage.Content = ((IDataSerial)_content).GetHeader();
                        }
                   }                                                                                               
                }
            }
            content = _content;
        }
        public void MessageContent(ref object content, object value, DirectionType _direction)
        {
            DirectionType direction = _direction;
            object _content = value;
            if (_content != null)
            {          
                if (direction == DirectionType.Receive)
                {
                    Type[] ifaces = _content.GetType().GetInterfaces();
                    if (ifaces.Contains(typeof(IDataSerial)))
                    {                       
                        object[] messages_ = ((IDataSerial)value).GetMessage();
                        if (messages_ != null)
                        {                            
                            int length = messages_.Length;
                            for (int i = 0; i < length; i++)
                            {
                                IDataSerial message = ((IDataSerial[])messages_)[i];
                                IDataSerial head = (IDataSerial)((IDataSerial[])messages_)[i].GetHeader();
                                message.SerialCount = head.SerialCount;
                                message.DeserialCount = head.DeserialCount;
                            }                          

                            _content = messages_;
                        }
                    }
                }              
            }
            content = _content;
        }
    }

    public class TransactionMethod
    { 
        private DepotTransaction transaction;
        private TransactionContext context;

        private IDepotContext icontext;
        private DepotSite site;
        private DirectionType direction;
        private MessagePart part;
        private DepotProtocol protocol;
        private ProtocolMethod method;

        public TransactionMethod(DepotTransaction _transaction, MessagePart _part, DirectionType _direction)
        {
            transaction = _transaction;
            icontext = transaction.Context;
            context = transaction.MyHeader.Context;
            site = context.IdentitySite;
            direction = _direction;
            part = _part;
            protocol = icontext.Protocol;
            method = icontext.Method;
        }

        public void Resolve(object _argument0 = null, object _argument1 = null)
        {
            switch (protocol)
            {
                case DepotProtocol.QDTP:
                    switch (site)
                    {
                        case DepotSite.Server:
                            switch (direction)
                            {
                                case DirectionType.Receive:
                                    switch (part)
                                    {
                                        case MessagePart.Header:
                                            SrvRecHead(ref _argument0);
                                            break;
                                        case MessagePart.Message:
                                            SrvRecMsg(ref _argument0, (int)_argument1);
                                            break;
                                    }
                                    break;
                                case DirectionType.Send:
                                    switch (part)
                                    {
                                        case MessagePart.Header:
                                            SrvSendHead();
                                            break;
                                        case MessagePart.Message:
                                            SrvSendMsg();
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case DepotSite.Client:
                            switch (direction)
                            {
                                case DirectionType.Receive:
                                    switch (part)
                                    {
                                        case MessagePart.Header:
                                            CltRecHead(ref _argument0);
                                            break;
                                        case MessagePart.Message:
                                            CltRecMsg(ref _argument0, (int)_argument1);
                                            break;
                                    }
                                    break;
                                case DirectionType.Send:
                                    switch (part)
                                    {
                                        case MessagePart.Header:
                                            CltSendHead();
                                            break;
                                        case MessagePart.Message:
                                            CltSendMsg();
                                            break;
                                    }
                                    break;
                            }
                            break;
                    }
                    break;
            
                case DepotProtocol.NONE:              
                    switch (site)
                    {
                        case DepotSite.Server:
                            switch (direction)
                            {
                                case DirectionType.Receive:
                                    switch (part)
                                    {
                                        case MessagePart.Header:
                                            SrvRecHead(ref _argument0);
                                            break;
                                        case MessagePart.Message:
                                            SrvRecMsg(ref _argument0, (int)_argument1);
                                            break;
                                    }
                                    break;
                                case DirectionType.Send:
                                    switch (part)
                                    {
                                        case MessagePart.Header:
                                            SrvSendHead();
                                            break;
                                        case MessagePart.Message:
                                            SrvSendMsg();
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case DepotSite.Client:
                            switch (direction)
                            {
                                case DirectionType.Receive:
                                    switch (part)
                                    {
                                        case MessagePart.Header:
                                            CltRecHead(ref _argument0);
                                            break;
                                        case MessagePart.Message:
                                            CltRecMsg(ref _argument0, (int)_argument1);
                                            break;
                                    }
                                    break;
                                case DirectionType.Send:
                                    switch (part)
                                    {
                                        case MessagePart.Header:
                                            CltSendHead();
                                            break;
                                        case MessagePart.Message:
                                            CltSendMsg();
                                            break;
                                    }
                                    break;
                            }
                            break;
                    }
                    break;
                case DepotProtocol.HTTP:
                    switch(method)
                    {
                        case ProtocolMethod.GET:
                            switch (site)
                            {
                                case DepotSite.Server:
                                    switch (direction)
                                    {
                                        case DirectionType.Receive:
                                            switch (part)
                                            {
                                                case MessagePart.Header:
                                                    SrvRecGet(ref _argument0);
                                                    break;                                              
                                            }
                                            break;
                                        case DirectionType.Send:
                                            switch (part)
                                            {
                                                case MessagePart.Header:
                                                    SrvSendGet();
                                                    break;                                               
                                            }
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case ProtocolMethod.POST:
                            switch (site)
                            {
                                case DepotSite.Server:
                                    switch (direction)
                                    {
                                        case DirectionType.Receive:
                                            switch (part)
                                            {
                                                case MessagePart.Header:
                                                     SrvRecPost(ref _argument0);
                                                    break;
                                            }
                                            break;
                                        case DirectionType.Send:
                                            switch (part)
                                            {
                                                case MessagePart.Header:
                                                    SrvSendPost();
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case ProtocolMethod.OPTIONS:
                            switch (site)
                            {
                                case DepotSite.Server:
                                    switch (direction)
                                    {
                                        case DirectionType.Receive:
                                            switch (part)
                                            {
                                                case MessagePart.Header:
                                                    SrvRecOptions();
                                                    break;
                                            }
                                            break;
                                        case DirectionType.Send:
                                            switch (part)
                                            {
                                                case MessagePart.Header:
                                                    SrvSendOptions();
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                            }
                            break;
                    }
                    break;
            }
        }

        private void SrvRecHead(ref object array)
        {
            bool isError = false;
            string errorMessage = "";
            try
            {
                TransactionHeader headerObject = (TransactionHeader)transaction.MyHeader.Deserialize(ref array);
                if (headerObject != null)
                {
                    transaction.HeaderReceived =  headerObject;

                    if (transaction.HeaderReceived.Context.Identity.Mode != ServiceMode.None ?
                        DepotConfig.RegisterConsole(transaction.HeaderReceived.Context.Identity, true) :
                        MembersConfig.RegisterMember(transaction.HeaderReceived.Context.Identity, true))
                    {
                        transaction.MyHeader.Context.Identity.UserId = transaction.HeaderReceived.Context.Identity.UserId;
                        transaction.MyHeader.Context.Identity.Token = transaction.HeaderReceived.Context.Identity.Token;
                        transaction.MyHeader.Context.Identity.DeptId = transaction.HeaderReceived.Context.Identity.DeptId;

                        if (transaction.HeaderReceived.Context.ContentType != null)
                        {

                            object _content = transaction.HeaderReceived.Content;
                            ((IDataConfig)_content).State.Synced = false;

                            Type[] ifaces = _content.GetType().GetInterfaces();
                            if (ifaces.Contains(typeof(IDataSerial)) && ifaces.Contains(typeof(IDataMorph)))
                            {
                                int objectCount = transaction.HeaderReceived.Context.ObjectsCount;
                                icontext.Synchronic = transaction.HeaderReceived.Context.Synchronic;

                                object myheader = ((IDataMorph)_content).Locator();

                                if (myheader != null)
                                {
                                    if (objectCount == 0)
                                    {
                                        icontext.ReceiveMessage = false;

                                        if (((IDataConfig)_content).Config.DepotId.IsNotEmpty)
                                            transaction.MyHeader.Content = ((IDataMorph)myheader).Impactor(_content);
                                        else
                                        {
                                            ((IDataConfig)myheader).State.Expeled = true;
                                            transaction.MyHeader.Content = myheader;
                                        }
                                    }
                                    else
                                    {
                                        transaction.MyHeader.Content = ((IDataMorph)myheader).Impactor(_content);
                                        transaction.MessageReceived = new TransactionMessage(transaction, DirectionType.Receive, transaction.MyHeader.Content);
                                    }
                                }
                                else
                                {
                                    isError = true;
                                    errorMessage += "Prime not exist - incorrect object target ";
                                }
                            }
                            else
                            {
                                isError = true;
                                errorMessage += "Incorrect DPOT object - deserialization error ";
                            }
                        }
                        else
                        {
                            transaction.MyHeader.Content = new Hashtable() { { "Register", true } };
                            transaction.MyHeader.Context.Echo += "Registration success - ContentType: null ";
                        }
                    }
                    else
                    {
                        isError = true;
                        transaction.MyHeader.Content = new Hashtable() { { "Register", false } };
                        transaction.MyHeader.Context.Echo += "Registration failed - access denied ";
                    }
                }
                else
                {
                    isError = true;
                    errorMessage += "Incorrect DPOT object - deserialization error ";
                }
            }
            catch (Exception ex)
            {
                isError = true;
                errorMessage += ex.ToString();
            }

            if (isError)
            {
                transaction.Context.Close = true;
                transaction.Context.ReceiveMessage = false;
                transaction.Context.SendMessage = false;

                if (errorMessage != "")
                {
                    transaction.MyHeader.Content += errorMessage;
                    transaction.MyHeader.Context.Echo += errorMessage;
                }
                transaction.MyHeader.Context.Errors++;
            }
        }
        private void SrvRecMsg(ref object array, int readposition)
        {
            object serialTiersObj = ((object[])transaction.MessageReceived.Content)[readposition];
            object deserialTiersObj = ((IDataSerial)serialTiersObj).Deserialize(ref array, true);
            IDataSerial deserialTiers = (IDataSerial)deserialTiersObj;
            if (deserialTiers.DeserialCount <= deserialTiers.ProgressCount || deserialTiers.ProgressCount == 0)
            {
                transaction.Context.ObjectsLeft--;
                deserialTiers.ProgressCount = 0;               
            }
            array = null;
        }

        private void SrvSendHead()
        {
            transaction.Manager.HeaderContent(icontext.Transaction.MyHeader.Content, 
                                              icontext.Transaction.MyHeader.Content, 
                                              DirectionType.Send);

            if (transaction.MyHeader.Context.ObjectsCount == 0)
                icontext.SendMessage = false;

            icontext.Transaction.MyHeader.Serialize(icontext, 0, 0);
        }
        private void SrvSendMsg()
        {            
            int serialBlockId = ((IDataSerial[])transaction.MyMessage.Content)[icontext.ObjectPosition].Serialize(icontext, icontext.SerialBlockId, 5000);

            if (serialBlockId < 0)
            {
                if (icontext.ObjectPosition < (transaction.MyHeader.Context.ObjectsCount - 1))
                {
                    icontext.ObjectPosition++;
                    icontext.SerialBlockId = 0;
                    return;
                }           
            }
            icontext.SerialBlockId = serialBlockId;
        }

        private void CltRecHead(ref object array)
        {
            TransactionHeader headerObject = (TransactionHeader)transaction.MyHeader.Deserialize(ref array);

            if (headerObject != null)
            {
                transaction.HeaderReceived = headerObject;

                transaction.MyHeader.Context.Identity.Key = null;
                transaction.MyHeader.Context.Identity.Name = null;
                transaction.MyHeader.Context.Identity.UserId = transaction.HeaderReceived.Context.Identity.UserId;
                transaction.MyHeader.Context.Identity.Token = transaction.HeaderReceived.Context.Identity.Token;
                transaction.MyHeader.Context.Identity.DeptId = transaction.HeaderReceived.Context.Identity.DeptId;

                object reciveContent = transaction.HeaderReceived.Content;

                Type[] ifaces = reciveContent.GetType().GetInterfaces();
                if (ifaces.Contains(typeof(IDataSerial)) && ifaces.Contains(typeof(IDataMorph)))
                {
                    if (transaction.MyHeader.Content == null)
                        transaction.MyHeader.Content = ((IDataMorph)reciveContent).Locator();

                    object myContent = transaction.MyHeader.Content;

                    ((IDataMorph)myContent).Impactor(reciveContent);

                    int objectCount = transaction.HeaderReceived.Context.ObjectsCount;
                    if (objectCount == 0)
                        icontext.ReceiveMessage = false;
                    else
                        transaction.MessageReceived = new TransactionMessage(transaction, DirectionType.Receive, myContent);
                }
                else if(reciveContent is Hashtable)
                {
                    Hashtable hashTable = (Hashtable)reciveContent;
                    if (hashTable.Contains("Register"))
                    {
                        icontext.Denied = !(bool)hashTable["Register"];
                        if (icontext.Denied)
                        {
                            icontext.Close = true;
                            icontext.ReceiveMessage = false;
                            icontext.SendMessage = false;
                        }
                    }
                }
                else
                    icontext.SendMessage = false;
            }
        }
        private void CltRecMsg(ref object array, int readposition)
        {
            object serialTiersObj = ((object[])transaction.MessageReceived.Content)[readposition];
            IDataSerial serialTiers = (IDataSerial)serialTiersObj;

            if (serialTiers.ProgressCount == 0)
                ((IDataConfig)serialTiersObj).State.withPropagate = false;

            object deserialTiersObj = serialTiers.Deserialize(ref array, false);
            IDataSerial deserialTiers = (IDataSerial)deserialTiersObj;
            if (deserialTiers.DeserialCount <= deserialTiers.ProgressCount || deserialTiers.ProgressCount == 0)
            {
                transaction.Context.ObjectsLeft--;
                deserialTiers.ProgressCount = 0;
                ((IDataConfig)deserialTiersObj).State.withPropagate = true;
                ((IDataConfig)((IDataTiers)deserialTiersObj).Trell).State.Impact(((IDataConfig)deserialTiersObj).State, true, true);
                ((IDataConfig)((IDataTiers)deserialTiersObj).Trell).State.Synced = true;
            }
            array = null;
        }

        private void CltSendHead()
        {
            transaction.Manager.HeaderContent(icontext.Transaction.MyHeader.Content, 
                                              icontext.Transaction.MyHeader.Content, 
                                              DirectionType.Send);

            if (transaction.MyHeader.Context.ObjectsCount == 0)
                icontext.SendMessage = false;

            icontext.Transaction.MyHeader.Serialize(icontext, 0, 0);
        }
        private void CltSendMsg()
        {
            object serialtiers = ((object[])transaction.MyMessage.Content)[icontext.ObjectPosition];

            ((IDataConfig)serialtiers).State.Synced = false;
            int serialBlockId = ((IDataSerial)serialtiers).Serialize(icontext, icontext.SerialBlockId, 5000);
            if (serialBlockId < 0)
            {
                if (icontext.ObjectPosition < (transaction.MyHeader.Context.ObjectsCount - 1))
                {
                    icontext.ObjectPosition++;
                    icontext.SerialBlockId = 0;
                    return;
                }
            }
            icontext.SerialBlockId = serialBlockId;
        }

        private void SrvRecGet(ref object array)
        {
            icontext.SendMessage = false;
            icontext.ReceiveMessage = false;
            transaction.HeaderReceived = transaction.MyHeader;        
            icontext.HandleGetRequest();
        }
        private void SrvSendGet()
        {
            icontext.SendMessage = false;
            icontext.ReceiveMessage = false;           
        }

        private void SrvRecPost(ref object array)
        {
            if (SrvRecPostDpot(ref array))
                transaction.HeaderReceived = transaction.MyHeader;
            icontext.SendMessage = false;
            icontext.ReceiveMessage = false;
        }
        private bool SrvRecPostDpot(ref object array)
        {
            bool isError = false;
            string errorMessage = "";
            try
            {
                byte[] _array = (byte[])array;
                StringBuilder sb = new StringBuilder();
                sb.Append(_array.ToChars(CharCoding.UTF8));

                string dpttransx = sb.ToString();
                int msgid = dpttransx.IndexOf(",\"DepotMessage\":");
                string dptheadx = dpttransx.Substring(0, msgid) + "}";
                string dptmsgx = "{" + dpttransx.Substring(msgid, dpttransx.Length - msgid).Trim(',');

                string[] msgcntsx = dptmsgx.Split(new string[] { "\"Content\":" }, StringSplitOptions.RemoveEmptyEntries);
                string[] cntarrays = msgcntsx.Length > 0 ? msgcntsx[1].Split(new string[] { "\"DataTiers\":" }, StringSplitOptions.None) : null;
                int objectCount = 0;
                if (cntarrays != null)
                    for (int i = 1; i < cntarrays.Length; i += 1)
                    {
                        string[] itemarray = cntarrays[i].Split('[');
                        for (int x = 1; x < itemarray.Length; x += 1)
                        {
                            if (itemarray[x].IndexOf(']') > 0)
                                objectCount++;
                        }
                    }

                string msgcntx = msgcntsx[1].Trim(' ').Substring(0, 6);
                //int objectCount = (!msgcntx.Contains("null") &&
                //                  (!msgcntx.Contains("{}")) &&
                //                  msgcntsx[1].Contains("\"DataBag\"")) ? 1 : 0;

                object dptheadb = dptheadx;
                object dptmsgb = dptmsgx;

                isError = SrvRecPostDpotHeader(ref dptheadb, objectCount);
                if(objectCount > 0 && !isError)
                    isError = SrvRecPostDpotMessage(ref dptmsgb);

            }
            catch (Exception ex)
            {
                isError = true;
                errorMessage = ex.ToString();
            }

            if (isError)
            {
                transaction.Context.SendMessage = false;
                if (errorMessage != "")
                {
                    transaction.MyHeader.Content += errorMessage;
                    transaction.MyHeader.Context.Echo += errorMessage;
                }
                transaction.MyHeader.Context.Errors++;
            }
            return isError;
        }
        private bool SrvRecPostDpotHeader(ref object array, int objcount)
        { 
            bool isError = false;
            string errorMessage = "";
            try
            {
                TransactionHeader headerObject = (TransactionHeader)transaction.MyHeader.Deserialize(ref array, true, SerialFormat.Json);
                headerObject.Context.Identity.Ip = transaction.MyHeader.Context.RemoteEndPoint.Address.ToString();
                if (MembersConfig.RegisterMember(headerObject.Context.Identity, true))
                {
                    transaction.HeaderReceived = (headerObject != null) ? headerObject : null;
                    transaction.MyHeader.Context.Complexity = headerObject.Context.Complexity;
                    transaction.MyHeader.Context.Identity =  headerObject.Context.Identity;

                    if (headerObject.Context.ContentType != null)
                    {

                        object instance = new object();
                        DataJson.PrepareInstance(out instance, headerObject.Context.ContentType);
                        object content = headerObject.Content;
                        object result = ((IDataSerial)instance).Deserialize(ref content, true, SerialFormat.Json);
                        transaction.HeaderReceived.Content = result;
                        object _content = transaction.HeaderReceived.Content;

                        Type[] ifaces = _content.GetType().GetInterfaces();
                        if (ifaces.Contains(typeof(IDataSerial)) && ifaces.Contains(typeof(IDataMorph)))
                        {
                            int objectCount = objcount;

                            object myheader = ((IDataMorph)_content).Locator();

                            DataConfig clientConfig = ((IDataConfig)_content).Config;

                            if (myheader != null)
                            {
                                if (objectCount == 0)
                                {
                                    icontext.ReceiveMessage = false;

                                    if (clientConfig.DepotId.IsNotEmpty)
                                        transaction.MyHeader.Content = ((IDataMorph)myheader).Impactor(_content);
                                    else
                                    {
                                        ((IDataConfig)myheader).State.Expeled = true;
                                        transaction.MyHeader.Content = myheader;
                                    }
                                }
                                else
                                {
                                    transaction.MyHeader.Content = ((IDataMorph)myheader).Impactor(_content);
                                    transaction.MessageReceived = new TransactionMessage(transaction, DirectionType.Receive, transaction.MyHeader.Content);
                                }
                            }
                            else
                            {
                                isError = true;
                                errorMessage += "Prime not exist - incorrect object target ";
                            }
                        }
                        else
                        {
                            isError = true;
                            errorMessage += "Incorrect DPOT object - deserialization error ";
                        }
                    }
                    else
                    {
                        transaction.MyHeader.Content = new Hashtable() { { "Register", true } };
                        transaction.MyHeader.Context.Echo += "Registration success - ContentType: null ";
                    }
                }
                else
                {
                    isError = true;
                    transaction.MyHeader.Content = new Hashtable() { { "Register", false } };
                    transaction.MyHeader.Context.Echo += "Registration failed - access denied ";
                }

            }
            catch (Exception ex)
            {
                isError = true;
                errorMessage += ex.ToString();
            }

            if (isError)
            {
                transaction.Context.SendMessage = false;
                if (errorMessage != "")
                {
                    transaction.MyHeader.Content += errorMessage;
                    transaction.MyHeader.Context.Echo += errorMessage;
                }
                transaction.MyHeader.Context.Errors++;
            }
            return isError;
        }
        private bool SrvRecPostDpotMessage(ref object array)
        {
            bool isError = false;
            string errorMessage = "";
            try
            {
                ((IDataSerial[])transaction.MessageReceived.Content).DeserializeJsonTiers(ref array);                
            }
            catch (Exception ex)
            {
                isError = true;
                errorMessage += ex.ToString();
            }

            if (isError)
            {
                transaction.Context.SendMessage = false;
                if (errorMessage != "")
                {
                    transaction.MyHeader.Content = "Prime not exist - incorrect object path";
                    transaction.MyHeader.Context.Echo = "Error - Prime not exist - incorrect object path";
                }
                transaction.MyHeader.Context.Errors++;
            }
            return isError;
        }

        private void SrvSendPost()
        {
            icontext.SendMessage = false;
            icontext.ReceiveMessage = false;
            icontext.RequestBuilder.Clear();
            if (!icontext.Denied)
            {
                SrvSendPostDpot();
                icontext.HandlePostRequest("application/json");
            }
            else
                icontext.HandleDeniedRequest();
        }
        private void SrvSendPostDpot()
        {
            SrvSendPostDpotHeader();
            SrvSendPostDpotMessage();            
        }
        private void SrvSendPostDpotHeader()
        {
            transaction.Manager.HeaderContent(icontext.Transaction.MyHeader.Content, icontext.Transaction.MyHeader.Content, DirectionType.Send);
            transaction.MyHeader.SetJson(icontext.RequestBuilder);
        }
        private void SrvSendPostDpotMessage()
        {
            StringBuilder msgcnt = new StringBuilder();

            Type[] ifaces = transaction.MyMessage.Content.GetType().GetInterfaces();
            if (ifaces.Contains(typeof(ICollection)) && transaction.MyHeader.Context.Errors == 0)
                ((IDataSerial[])transaction.MyMessage.Content).SerializeJsonTiers(msgcnt, 0, 0, transaction.MyHeader.Context.Complexity);
            else
                msgcnt.Append("null");

            transaction.MyMessage.Content = new object();
            transaction.MyMessage.SetJson(icontext.RequestBuilder);
            string msg = msgcnt.ToString().Replace("}\r\n{", ",").Trim(new char[] { '\n', '\r' });
            icontext.RequestBuilder.Replace("\"Content\":{}", "\"Content\":" + msgcnt.ToString());          
            icontext.RequestBuilder.Replace("}\r\n{", ",");
            icontext.SendMessage = false;
            icontext.ReceiveMessage = false;
        }

        private void SrvRecOptions()
        {
            transaction.HeaderReceived = transaction.MyHeader;
            icontext.SendMessage = false;
            icontext.ReceiveMessage = false;
            icontext.HandleOptionsRequest("application/json");
        }
        private void SrvSendOptions()
        {
            icontext.SendMessage = false;
            icontext.ReceiveMessage = false;
        }
    }
}
