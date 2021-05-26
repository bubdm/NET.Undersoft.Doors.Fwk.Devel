using System.Text;
using System.Collections.Generic;
using System.Doors.Data;
using System.Doors;

namespace System.Doors.Data
{
    public static class MembersConfig
    {
        public static DepotIdentity GetPrimeMember()
        {
            return GetMemberIdentity(-1);
        }

        public static DepotIdentity GetMemberIdentity(int Id)
        {
            if (!DataBank.Vault.Have("System")) ConfigBuilder.Build();
            DataTier[] members = DataBank.Vault["System"]["Members", "Users"].Trell.Tiers.Collect("Id", Id);
            if (members != null && members.Length > 0)
            {
                DataTier member = members[0];
                DepotIdentity di = SetMemberIdentity(member);
                return di;
            }
            return null;
        } 
        public static DepotIdentity GetMemberIdentity(string FieldName, object Value)
        {          
            DataTier[] members = DataBank.Vault["System"]["Members", "Users"].Trell.Tiers.Collect(FieldName, Value);
            if (members!= null && members.Length > 0)
            {
                DataTier member = members[0];
                DepotIdentity di = SetMemberIdentity(member);              
                return di;
            }
            return null;
        }

        public static DataTier GetMemberData(string FieldName, object Value)
        {
            DataTier[] members = DataBank.Vault["System"]["Members", "Users"].Trell.Tiers.Collect(FieldName, Value);
            if (members != null && members.Length > 0)
            {
                DataTier member = members[0];
                return member;
            }
            return null;
        }
        public static DataTier GetSessionData(string token)
        {
            DataTier[] sessions = DataBank.Vault["System"]["Members", "UserSessions"].Trell.Tiers.Collect("Token", token);
            if (sessions != null && sessions.Length > 0)
            {
                DataTiers members = sessions[0].GetParent("Users");
                if (members.Count > 0)
                {
                    return members[0];
                }
            }
            return null;
        }
        public static DataTier GetSessionData(DataTier member, string id = "")
        {
            DataTiers sessions = member.GetChild("UserSessions");
            DataTier[] _sessions = null;
            if (id != string.Empty)
                _sessions = sessions.Collect("PartnerId", id);
            else
                _sessions = sessions.AsArray();

            if (_sessions != null && _sessions.Length > 0)
            {
                return sessions[0];
            }
            return null;
        }
        public static DataTier GetPartnersData(DataTier member, string id = "")
        {
            DataTiers partners = member.GetChild("PartnerUsers");
            DataTier[] _partners = null;
            if (id != string.Empty)
                _partners = partners.Collect("PartnerId", id);
            else
                _partners = partners.Collect("Default", true);

            if (_partners != null && _partners.Length > 0)
            {                
                return partners[0];
            }
            return null;
        }

        private static DepotIdentity SetMemberIdentity(DataTier member, string id = "")
        {
            DepotIdentity di = new DepotIdentity();
            di.Id =     (int)member["Id"];
            di.Name =   member["Login"].ToString();
            di.UserId = member["UserId"].ToString();

            DataTiers partners = member.GetChild("PartnerUsers");

            DataTier[] _partners = null;
            if (id != string.Empty)
                _partners = partners.Collect("PartnerId", id);
            else
                _partners = partners.Collect("Default", true);

            if (_partners != null && _partners.Length > 0)
            {
                DataTier partner = _partners[0];
                di.DeptId = partner["PartnerId"].ToString();
                di.DataPlace = partner["DataPlace"].ToString();
            }

            DataTiers sessions = member.GetChild("UserSessions");

            DataTier[] _sessions = null;
            if (id != string.Empty)
                _sessions = sessions.Collect("PartnerId", id);
            else
                _sessions = sessions.AsArray();

            if (_sessions != null && _sessions.Length > 0)
            {
                DataTier session = _sessions[0];

                di.RegisterTime = (DateTime)session["RegisterTime"];
                di.LastAction =   (DateTime)session["LastAction"];
                di.LifeTime =     (DateTime)session["LifeTime"];
            }
            return di;
        }
       
        public static bool RegisterMember(DepotIdentity memberIdentity, bool encoded = false)
        {
            DepotIdentity di = memberIdentity;
            di.Active = false;
            if (!DataBank.Vault.Have("System")) ConfigBuilder.Build();

            DataTier member = null;
            bool verify = false;
            if (di.UserId != null && di.UserId != "")
                member = GetMemberData("UserId", di.UserId);
            else if (di.Name != null && di.Name != "")
            {
                string name = (encoded) ? Encoding.ASCII.GetString(Convert.FromBase64String(di.Name)) : di.Name;
                member = GetMemberData("Login", name);
            }

            if (member != null)
            {
                DataTier partner = GetPartnersData(member, di.DeptId);
                DataTier session = GetSessionData(member, partner["PartnerId"].ToString());

                session["Ip"] = di.Ip;

                if (di.Key != null && di.Key != "")
                {
                    string key = (encoded) ? Encoding.ASCII.GetString(Convert.FromBase64String(di.Key)) : di.Key;
                    verify = VerifyMemberIdentity(member, key);
                    if(verify)
                        di.Token = CreateMemberToken(member);
                }
                else if (di.Token != null && di.Token != "")
                {
                    verify = VerifyMemberToken(member, di.Token);                  
                }

                if (verify)
                {
                    di.Key = null;
                    di.UserId = member["UserId"].ToString();
                    di.DeptId = partner["PartnerId"].ToString();
                    di.DataPlace = partner["DataPlace"].ToString();
                    di.RegisterTime = (DateTime)session["RegisterTime"];
                    di.LastAction = (DateTime)session["LastAction"];
                    di.LifeTime = (DateTime)session["LifeTime"];
                    di.Active = true;
                }
            }

            return verify;
        }
        public static bool RegisterMember(string name, string key, out DepotIdentity di, string ip = "")
        {    
            di = GetMemberIdentity("Login", name);          
            if (di != null)
            {
                if (ip != "") di.Ip = ip;
                di.Key = key;
                return RegisterMember(di);
            }
            return false;
        }
        public static bool RegisterMember(string token, out DepotIdentity di, string ip = "")
        {
            DataTier session = GetSessionData(token);
            if (session != null)
            {
                di = SetMemberIdentity(session);
                if (di != null)
                {
                    if (ip != "") di.Ip = ip;
                    di.Token = token;
                    return RegisterMember(di);
                }
                return false;
            }
            di = null;
            return false;
        }

        public static bool VerifyMemberIdentity(DataTier member, string passwd)
        {
            bool verify = false;
          
            if (!DataBank.Vault.Have("System")) ConfigBuilder.Build();

            string hashpasswd = member["Password"].ToString();
            string saltpasswd = member["PasswordSalt"].ToString();
            int fomatpasswd = (int)member["PasswordFormat"];
            verify = KeyHasher.Verify(hashpasswd, saltpasswd, passwd);

            return verify;
        }

        public static bool VerifyMemberToken(DataTier member, string token)
        {
            bool verify = false;

            if (!DataBank.Vault.Have("System")) ConfigBuilder.Build();

            DataTiers sessions = member.GetChild("UserSessions");

            if (sessions != null && sessions.Count > 0)
            {
                DataTier session = sessions[0];

                string _token = session["Token"].ToString();

                if (_token.Equals(token))
                {
                    DateTime time = DateTime.Now;
                    DateTime registerTime = (DateTime)session["RegisterTime"];
                    DateTime lastAction = (DateTime)session["LastAction"];
                    DateTime lifeTime = (DateTime)session["LifeTime"];
                    if (lifeTime > time)
                        verify = true;
                    else if(lastAction > time.AddMinutes(-30))
                    {
                        session["LifeTime"] = time.AddMinutes(30);
                        session["LastAction"] = time;
                        verify = true;
                    }
                }
            }

            return verify;
        }

        public static string CreateMemberToken(DataTier member)
        {
            string token = null;

            if (!DataBank.Vault.Have("System")) ConfigBuilder.Build();

            DataTiers sessions = member.GetChild("UserSessions");

            if (sessions != null && sessions.Count > 0)
            {
                DataTier session = sessions[0];

                string _token = member["Password"].ToString();
                string timesalt = Convert.ToBase64String(DateTime.Now.Ticks.ToString().ToBytes(CharCoding.ASCII));
                token = KeyHasher.Encrypt(_token, 1, timesalt);
                session["Token"] = token;
                DateTime time = DateTime.Now;
                session["RegisterTime"] = time;
                session["LifeTime"] = time.AddMinutes(30);
                session["LastAction"] = time;              
            }

            return token;
        }
    }

}
