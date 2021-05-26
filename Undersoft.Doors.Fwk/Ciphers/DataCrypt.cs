//using System;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;
//using System.IO;
//using System.Runtime.Serialization.Formatters.Binary;
//using System.Reflection;

//namespace System.Doors
//{

//    public class InvalidDataCryptException : Exception
//    {
//        public InvalidDataCryptException(string message)
//            : base(message)
//        {

//        }
//    }


//    public class DataCrypt
//    {
//        private CipherFish cipherin;
//        private CipherFish cipherout;
//        private Stream     cryptstream;
//        private TcpClient  rchclient;

//        public DataCrypt(string server, int port, string key, string IVect)
//        {
//            // this.forEncryption = forEncryption;
//            try
//            {
//                cipher = new CipherFish();
//                cipherin = CipherUtilities.GetCipher("Blowfish/CTR/NoPadding");
//                cipherout.Init(false, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("Blowfish", Encoding.UTF8.GetBytes(key)), Encoding.UTF8.GetBytes(IVect)));
//                cipherin.Init(true, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("Blowfish", Encoding.UTF8.GetBytes(key)), Encoding.UTF8.GetBytes(IVect)));

//                rchclient = new TcpClient(server, port);

//                if (rchclient.Connected)
//                {
//                    NetworkStream stream = rchclient.GetStream();
//                    cstream = new CipherStream(stream, cipher, cipherin);
//                }
//            }
//            catch (Exception ex)
//            {
//                throw new InvalidBlowCryptException(ex.ToString());
//            }
//        }

//        public DataCrypt(string key, string IVect)
//        {
//            try
//            {
//                cipher = CipherUtilities.GetCipher("Blowfish/CTR/NoPadding");
//                cipherin = CipherUtilities.GetCipher("Blowfish/CTR/NoPadding");
//                cipherout.Init(false, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("Blowfish", Encoding.UTF8.GetBytes(key)), Encoding.UTF8.GetBytes(IVect)));
//                cipherin.Init(true, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("Blowfish", Encoding.UTF8.GetBytes(key)), Encoding.UTF8.GetBytes(IVect)));
//            }
//            catch (Exception ex)
//            {
//                throw new InvalidBlowCryptException(ex.ToString());
//            }
//        }

//        public static string key()
//        {
//            string methods = "";
//            foreach (MethodInfo item in typeof(DataCrypt).GetMethods())
//            {
//                methods += item.Name;
//            }
//            return methods.Substring(methods.Length / 3, 8);
//        }

//        public string Encrypt(string msg)
//        {
//            try
//            {
//                cipherout.Reset();
//                cipherin.Reset();
//                MemoryStream stream = new MemoryStream();
//                cstream = new CipherStream(stream, cipher, cipherin);

//                WriteStream(Encoding.UTF8.GetBytes(msg));
//                byte[] a = new byte[stream.Length];

//                stream.Position = 0;
//                stream.Read(a, 0, a.Length);

//                return Convert.ToBase64String(a);
//            }
//            catch (Exception ex)
//            {
//                throw new InvalidBlowCryptException(ex.ToString());
//            }
//        }

//        public string Decrypt(string msg)
//        {
//            try
//            {
//                MemoryStream stream = new MemoryStream();
//                cstream = new CipherStream(stream, cipher, cipherin);
//                byte[] buffer = Convert.FromBase64String(msg);
//                stream.Write(buffer, 0, buffer.Length);

//                stream.Position = 0;
//                return ReadStream().ToString();
//            }
//            catch (Exception ex)
//            {
//                throw new InvalidBlowCryptException(ex.ToString());
//            }
//        }

//        public void ReInit(byte[] IV, byte[] key)
//        {
//            cipherout.Init(false, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("Blowfish", key), IV));
//        }

//        public byte[] DoFinal()
//        {
//            return cipherout.DoFinal();
//        }

//        public byte[] DoFinal(byte[] buffer)
//        {
//            return cipherout.DoFinal(buffer);
//        }

//        public StringBuilder ReadStream()
//        {
//            StringBuilder sb = new StringBuilder();
//            try
//            {
//                if (cstream.CanRead)
//                {
//                    int test = 0;
//                    int available = 0;

//                    if (rchclient == null)
//                    {
//                        byte[] a = new byte[1024];
//                        int read = 0;
//                        while ((read = cstream.Read(a, 0, 1024)) != 0)
//                        {
//                            string response = Encoding.UTF8.GetString(a);
//                            response = response.Replace("\0", "").Trim();
//                            sb.Append(response);
//                        }
//                    }
//                    else
//                    {
//                        do
//                        {
//                            test++;
//                            Thread.Sleep(100);

//                            if (rchclient != null)
//                            {
//                                available = rchclient.Available;

//                                byte[] buffer = new byte[available];
//                                cstream.Read(buffer, 0, available);
//                                sb.Append(Encoding.UTF8.GetString(buffer));
//                                if (available == 0 && test != 10)
//                                {
//                                    Thread.Sleep(2000);
//                                }
//                            }
//                        }
//                        while (available == 0 && test != 10);
//                    }
//                }

//                return sb;
//            }
//            catch (Exception ex)
//            {
//                sb.Append(" - some error");
//                return sb;
//                throw new InvalidBlowCryptException(ex.ToString());
//            }
//        }

//        public bool WriteStream(byte[] buffer)
//        {
//            try
//            {
//                int dividebytes = buffer.Length / 8;
//                int restbytes = buffer.Length - (dividebytes * 8);
//                if (restbytes > 0)
//                {
//                    byte[] addbytes = new byte[restbytes];
//                    byte[] newbytes = new byte[(dividebytes + 1) * 8];
//                    buffer.CopyTo(newbytes, 0);
//                    for (int i = buffer.Length; i < newbytes.Length; i++)
//                    {
//                        newbytes[i] = 32;
//                    }

//                    buffer = newbytes;
//                }

//                if (cstream.CanWrite)
//                {
//                    cstream.Write(buffer, 0, buffer.Length);
//                    cstream.Flush();
//                    return true;
//                }
//                else
//                {
//                    cstream.Flush();
//                    return false;
//                }
//            }
//            catch (Exception ex)
//            {
//                cstream.Flush();
//                return false;
//                throw new InvalidBlowCryptException(ex.ToString());
//            }

//        }

//        public bool CloseStream()
//        {
//            try
//            {
//                cstream.Close();
//                return true;
//            }
//            catch (Exception ex)
//            {
//                return false;
//                throw new InvalidBlowCryptException(ex.ToString());
//            }

//        }

//        public byte[] DoFinal(byte[] buffer, int startIndex, int len)
//        {
//            return cipherout.DoFinal(buffer, startIndex, len);
//        }

//        public byte[] ProcessBytes(byte[] buffer)
//        {
//            return cipherout.ProcessBytes(buffer);
//        }

//        public byte[] ProcessBytes(byte[] buffer, int startIndex, int len)
//        {
//            return cipherout.ProcessBytes(buffer, startIndex, len);
//        }

//        public void Reset()
//        {
//            cipherout.Reset();
//        }
//    }



//}
