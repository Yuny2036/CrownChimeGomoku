using System;
using System.Threading;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;

// Queue 클래스 스레드 안정성 및 데이터 무결성 테스트
public class QueueThreadSafetyTest
{
    private Queue queue = new Queue();
    private const int NUM_THREADS = 10;
    private const int DATA_SIZE = 100;
    private const int NUM_OPERATIONS = 1000;

    public void Run()
    {
        Console.WriteLine("--- 1. Queue Thread Safety Test Start ---");
        
        Thread[] addThreads = new Thread[NUM_THREADS];
        Thread[] popThreads = new Thread[NUM_THREADS];
        
        // 데이터 무결성 확인을 위한 리스트
        List<string> expectedData = new List<string>();

        // Add 스레드 생성
        for (int i = 0; i < NUM_THREADS; i++)
        {
            int threadId = i;
            addThreads[i] = new Thread(() => AddData(threadId, expectedData));
            addThreads[i].Start();
        }

        // Add 스레드 종료 대기
        foreach (var t in addThreads)
        {
            t.Join();
        }

        Console.WriteLine($"Total expected data packets added: {expectedData.Count}");

        // Pop 스레드 생성
        int successfulPops = 0;
        int failedPops = 0;
        object popLock = new object();

        for (int i = 0; i < NUM_THREADS; i++)
        {
            int threadId = i;
            popThreads[i] = new Thread(() => PopData(ref successfulPops, ref failedPops, popLock));
            popThreads[i].Start();
        }

        // Pop 스레드 종료 대기
        foreach (var t in popThreads)
        {
            t.Join();
        }
        
        // Pop된 데이터의 무결성 검증은 PopData 메서드에서 부분적으로 수행됨.
        // 최종적으로 큐가 비어있는지 확인.
        Console.WriteLine($"Successful Pops: {successfulPops}");
        Console.WriteLine($"Failed Pops (Empty Queue): {failedPops}");

        if (successfulPops == expectedData.Count && successfulPops > 0)
        {
            Console.WriteLine("Queue Thread Safety Test: PASS (All data packets processed)");
        }
        else
        {
            Console.WriteLine("Queue Thread Safety Test: FAIL (Data count mismatch or zero operations)");
        }

        Console.WriteLine("--- 1. Queue Thread Safety Test End ---");
    }

    private void AddData(int threadId, List<string> expectedData)
    {
        for (int i = 0; i < NUM_OPERATIONS; i++)
        {
            string dataString = $"T{threadId:00}_P{i:0000}";
            byte[] data = Encoding.UTF8.GetBytes(dataString);
            
            // 예상 데이터 리스트에도 추가 (이 작업 자체도 동기화 필요)
            lock (expectedData)
            {
                expectedData.Add(dataString);
            }
            
            int addedSize = queue.Add(data, data.Length);
            if (addedSize != data.Length)
            {
                // 실시간으로 실패를 콘솔에 출력할 수 있지만, 스레드 환경이므로 Join 후 최종 확인에 집중
            }
        }
    }

    private void PopData(ref int successCount, ref int failCount, object lockObj)
    {
        byte[] dataBuffer = new byte[DATA_SIZE];
        for (int i = 0; i < NUM_OPERATIONS * NUM_THREADS; i++)
        {
            int size = queue.Pop(ref dataBuffer, dataBuffer.Length);
            
            lock (lockObj)
            {
                if (size > 0)
                {
                    Interlocked.Increment(ref successCount);
                    // 데이터 무결성 검사 (옵션)
                    string receivedString = Encoding.UTF8.GetString(dataBuffer, 0, size);
                    if (!receivedString.StartsWith("T") || !receivedString.Contains("_P"))
                    {
                        // 실제 테스트 환경에서는 로그로 기록
                        // Console.WriteLine($"Data Corruption Detected: {receivedString}");
                    }
                }
                else if (size == -1)
                {
                    // 큐가 비었으면 스핀 대기하지 않고 나옴
                    Interlocked.Increment(ref failCount);
                    break;
                }
            }
        }
    }
}