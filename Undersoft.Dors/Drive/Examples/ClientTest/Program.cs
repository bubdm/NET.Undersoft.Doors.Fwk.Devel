﻿// SharedMemory (File: ClientTest\Program.cs)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Dors.Drive;

#if NET40Plus
using System.Threading.Tasks;
#endif

namespace ClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press <enter> to start client");

            Console.ReadLine();

            Console.WriteLine("Open existing shared memory circular buffer");
            using (DriveCircular theClient = new DriveCircular("E:\\MAPTEST.TREL", "TEST"))
            {
                Console.WriteLine("Buffer {0} opened, NodeBufferSize: {1}, NodeCount: {2}", theClient.Name, theClient.NodeBufferSize, theClient.NodeCount);

                long bufferSize = theClient.NodeBufferSize;
                byte[] writeDataProof;
                byte[] writeData = new byte[bufferSize];

                List<byte[]> dataList = new List<byte[]>();

                // Generate data for integrity check
                for (var j = 0; j < 256; j++)
                {
                    var data = new byte[bufferSize];
                    for (var i = 0; i < data.Length; i++)
                    {
                        data[i] = (byte)((i + j) % 255);
                    }
                    dataList.Add(data);
                }

                int skipCount = 0;
                long iterations = 0;
                long totalBytes = 0;
                long lastTick = 0;
                Stopwatch sw = Stopwatch.StartNew();

                int threadCount = 0;
                Action reader = () =>
                {
                    int myThreadIndex = Interlocked.Increment(ref threadCount);
                    int linesOut = 0;
                    bool finalLine = false;
                    for (; ; )
                    {
                        int amount = theClient.Read(writeData, 100);
                        //int amount = theClient.Read<byte>(writeData, 100);

                        if (amount == 0)
                        {
                            Interlocked.Increment(ref skipCount);
                        }
                        else
                        {
                            // Only check data integrity for first thread
                            if (threadCount == 1)
                            {
                                bool mismatch = false;

                                writeDataProof = dataList[((int)Interlocked.Read(ref iterations)) % 255];
                                for (var i = 0; i < writeDataProof.Length; i++)
                                {
                                    if (writeData[i] != writeDataProof[i])
                                    {
                                        mismatch = true;
                                        Console.WriteLine("Buffers don't match!");
                                        break;
                                    }
                                }

                                if (mismatch)
                                    break;
                            }

                            Interlocked.Add(ref totalBytes, amount);

                            Interlocked.Increment(ref iterations);
                        }

                        if (threadCount == 1 && Interlocked.Read(ref iterations) > 500)
                            finalLine = true;

                        if (myThreadIndex < 3 && (finalLine || sw.ElapsedTicks - lastTick > 1000000))
                        {
                            lastTick = sw.ElapsedTicks;
                            Console.WriteLine("Read: {0}, Wait: {1}, {2}MB/s", ((double)totalBytes / 1048576.0).ToString("F0"), skipCount, (((totalBytes / 1048576.0) / sw.ElapsedMilliseconds) * 1000).ToString("F2"));
                            linesOut++;
                            if (finalLine || (myThreadIndex > 1 && linesOut > 10))
                            {
                                Console.WriteLine("Completed.");
                                break;
                            }
                        }
                    }
                };

                Console.WriteLine("Testing data integrity (high CPU, low bandwidth)...");
                reader();
                Console.WriteLine("");

                skipCount = 0;
                iterations = 0;
                totalBytes = 0;
                lastTick = 0;
                sw.Reset();
                sw.Start();
                Console.WriteLine("Testing data throughput (low CPU, high bandwidth)...");
#if NET40Plus                
                Task c1 = Task.Factory.StartNew(reader);
                Task c2 = Task.Factory.StartNew(reader);
                Task c3 = Task.Factory.StartNew(reader);
                //Task c4 = Task.Factory.StartNew(reader);
#else
                ThreadPool.QueueUserWorkItem((o) => { reader(); });
                ThreadPool.QueueUserWorkItem((o) => { reader(); });
                ThreadPool.QueueUserWorkItem((o) => { reader(); });
#endif
                Console.ReadLine();
            }
        }
    }
}
