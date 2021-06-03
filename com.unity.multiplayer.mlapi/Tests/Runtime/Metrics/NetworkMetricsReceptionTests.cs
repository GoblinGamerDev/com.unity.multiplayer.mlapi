using System;
using System.Collections;
using System.IO;
using System.Linq;
using MLAPI.Metrics;
using MLAPI.Serialization;
using NUnit.Framework;
using Unity.Multiplayer.NetStats.Metrics;
using Unity.Multiplayer.NetworkProfiler;
using Unity.Multiplayer.NetworkProfiler.Models;
using UnityEngine;
using UnityEngine.TestTools;

namespace MLAPI.RuntimeTests.Metrics
{
#if true
    public class NetworkMetricsReceptionTests : MetricsTestBase
    {
        NetworkManager m_Server;
        NetworkManager m_Client;
        NetworkMetrics m_ClientMetrics;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (!MultiInstanceHelpers.Create(1, out m_Server, out var clients))
            {
                Debug.LogError("Failed to create instances");
                Assert.Fail("Failed to create instances");
            }

            if (!MultiInstanceHelpers.Start(true, m_Server, clients))
            {
                Debug.LogError("Failed to start instances");
                Assert.Fail("Failed to start instances");
            }

            yield return MultiInstanceHelpers.Run(MultiInstanceHelpers.WaitForClientsConnected(clients));
            yield return MultiInstanceHelpers.Run(MultiInstanceHelpers.WaitForClientConnectedToServer(m_Server));

            m_Client = clients.First();
            m_ClientMetrics = m_Client.NetworkMetrics as NetworkMetrics;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            MultiInstanceHelpers.Destroy();

            yield return null;
        }

        [UnityTest]
        public IEnumerator NetworkMetrics_WhenNamedMessageReceived_TracksNamedMessageReceivedMetric()
        {
            var messageName = Guid.NewGuid().ToString();
            LogAssert.Expect(LogType.Log, $"Received from {m_Server.LocalClientId}");
            m_Client.CustomMessagingManager.RegisterNamedMessageHandler(messageName, (sender, payload) =>
            {
                Debug.Log($"Received from {sender}");
            });

            var found = false;
            m_ClientMetrics.Dispatcher.RegisterObserver(new TestObserver(collection =>
            {
                var namedMessageSentMetric = collection.Metrics.SingleOrDefault(x => x.Name == MetricNames.NamedMessageReceived);
                Assert.NotNull(namedMessageSentMetric);

                var typedMetric = namedMessageSentMetric as IEventMetric<NamedMessageEvent>;
                Assert.NotNull(typedMetric);
                if (typedMetric.Values.Any()) // We always get the metric, but when it has values, something has been tracked
                {
                    Assert.AreEqual(1, typedMetric.Values.Count);

                    var namedMessageSent = typedMetric.Values.First();
                    Assert.AreEqual(messageName, namedMessageSent.Name);
                    Assert.AreEqual(m_Client.LocalClientId, namedMessageSent.Connection.Id);

                    found = true;
                }
            }));

            m_Server.CustomMessagingManager.SendNamedMessage(messageName, m_Client.LocalClientId, Stream.Null);

            yield return WaitForAFewFrames(); // Client does not receive message synchronously

            Assert.True(found);
        }

        [UnityTest]
        public IEnumerator NetworkMetrics_WhenUnnamedMessageReceived_TracksUnnamedMessageReceivedMetric()
        {
            var found = false;
            m_ClientMetrics.Dispatcher.RegisterObserver(new TestObserver(collection =>
            {
                var namedMessageSentMetric = collection.Metrics.SingleOrDefault(x => x.Name == MetricNames.UnnamedMessageReceived);
                Assert.NotNull(namedMessageSentMetric);

                var typedMetric = namedMessageSentMetric as IEventMetric<UnnamedMessageEvent>;
                Assert.NotNull(typedMetric);
                if (typedMetric.Values.Any()) // We always get the metric, but when it has values, something has been tracked
                {
                    Assert.AreEqual(1, typedMetric.Values.Count);

                    var namedMessageSent = typedMetric.Values.First();
                    Assert.AreEqual(m_Client.LocalClientId, namedMessageSent.Connection.Id);

                    found = true;
                }
            }));

            m_Server.CustomMessagingManager.SendUnnamedMessage(m_Client.LocalClientId, new NetworkBuffer());

            yield return WaitForAFewFrames(); // Client does not receive message synchronously

            Assert.True(found);
        }
    }
#endif
}