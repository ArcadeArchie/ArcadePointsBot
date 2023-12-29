using ArcadePointsBot.DNS.Multicast;
using ArcadePointsBot.DNS.Records;
using ArcadePointsBot.DNS.Serialization;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ArcadePointsBot.DNS.Tests;


public class MulticastServiceTest
{
    [Fact]
    public void Can_Create()
    {
        var mdns = new MulticastService();

        Assert.NotNull(mdns);
        Assert.True(mdns.IgnoreDuplicateMessages);
    }

    [Fact]
    public void StartStop()
    {
        var mdns = new MulticastService();

        mdns.Start();
        mdns.Stop();
    }

    [Fact]
    public void SendQuery()
    {
        var ready = new ManualResetEvent(false);
        var done = new ManualResetEvent(false);
        Message? msg = null;

        var mdns = new MulticastService();
        mdns.NetworkInterfaceDiscovered += (s, e) => ready.Set();
        mdns.QueryReceived += (s, e) =>
        {
            if (string.Equals(e.Message.Questions.First().Name, "some-service.local", StringComparison.OrdinalIgnoreCase))
            {
                msg = e.Message;
                Assert.False(e.IsLegacyUnicast);
                done.Set();
            }
        };
        try
        {
            mdns.Start();
            Assert.True(ready.WaitOne(TimeSpan.FromSeconds(1)), "ready timeout");
            mdns.SendQuery("some-service.local");
            Assert.True(done.WaitOne(TimeSpan.FromSeconds(1)), "query timeout");
            Assert.Equal("some-service.local", msg?.Questions.First().Name);
            Assert.Equal(DnsClass.IN, msg?.Questions.First().Class);
        }
        finally
        {
            mdns.Stop();
        }
    }


    [Fact]
    public void SendUnicastQuery()
    {
        var ready = new ManualResetEvent(false);
        var done = new ManualResetEvent(false);
        Message? msg = null;

        var mdns = new MulticastService();
        mdns.NetworkInterfaceDiscovered += (s, e) => ready.Set();
        mdns.QueryReceived += (s, e) =>
        {
            msg = e.Message;
            done.Set();
        };
        try
        {
            mdns.Start();
            Assert.True(ready.WaitOne(TimeSpan.FromSeconds(1)), "ready timeout");
            mdns.SendUnicastQuery("some-service.local");
            Assert.True(done.WaitOne(TimeSpan.FromSeconds(1)), "query timeout");
            Assert.Equal("some-service.local", msg?.Questions.First().Name);
            Assert.Equal(DnsClass.IN + 0x8000, msg?.Questions.First().Class);
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Fact]
    public void ReceiveAnswer()
    {
        var service = Guid.NewGuid().ToString() + ".local";
        var done = new ManualResetEvent(false);
        Message? response = null;

        using var mdns = new MulticastService();
        mdns.NetworkInterfaceDiscovered += (s, e) => mdns.SendQuery(service);
        mdns.QueryReceived += (s, e) =>
        {
            var msg = e.Message;
            if (msg.Questions.Any(q => q.Name == service))
            {
                var res = msg.CreateResponse();
                res.Answers.Add(new ARecord
                {
                    Name = service,
                    Address = IPAddress.Parse("127.1.1.1")
                });
                mdns.SendAnswer(res);
            }
        };
        mdns.AnswerReceived += (s, e) =>
        {
            var msg = e.Message;
            if (msg.Answers.Any(answer => answer.Name == service))
            {
                response = msg;
                done.Set();
            }
        };
        mdns.Start();
        Assert.True(done.WaitOne(TimeSpan.FromSeconds(1)), "answer timeout");
        Assert.NotNull(response);
        Assert.True(response.IsResponse);
        Assert.Equal(MessageStatus.NoError, response.Status);
        Assert.True(response.AA);
        var a = (ARecord)response.Answers[0];
        Assert.Equal(IPAddress.Parse("127.1.1.1"), a.Address);
    }

    [Fact]
    public async Task ReceiveLegacyUnicastAnswer()
    {
        var service = Guid.NewGuid().ToString() + ".local";
        var ready = new ManualResetEvent(false);

        var query = new Message();
        query.Questions.Add(new Question
        {
            Name = service,
            Type = DnsType.A
        });
        var packet = query.ToByteArray();
        var client = new UdpClient();
        using var mdns = new MulticastService();
        mdns.NetworkInterfaceDiscovered += (s, e) => ready.Set();
        mdns.QueryReceived += (s, e) =>
        {
            var msg = e.Message;
            if (msg.Questions.Any(q => q.Name == service))
            {
                var res = msg.CreateResponse();
                res.Answers.Add(new ARecord
                {
                    Name = service,
                    Address = IPAddress.Parse("127.1.1.1")
                });
                mdns.SendAnswer(res, e);
            }
        };
        mdns.Start();
        Assert.True(ready.WaitOne(TimeSpan.FromSeconds(1)), "ready timeout");
        await client.SendAsync(packet, packet.Length, "224.0.0.251", 5353);

        var r = await client.ReceiveAsync();
        var response = new Message();
        response.Read(r.Buffer, 0, r.Buffer.Length);
        Assert.True(response.IsResponse);
        Assert.Equal(MessageStatus.NoError, response.Status);
        Assert.True(response.AA);
        Assert.Single(response.Questions);
        var a = (ARecord)response.Answers[0];
        Assert.Equal(IPAddress.Parse("127.1.1.1"), a.Address);
        Assert.Equal(service, a.Name);
        Assert.Equal(TimeSpan.FromSeconds(10), a.TTL);
    }

    [Fact]
    public void ReceiveAnswer_IPv4()
    {
        var service = Guid.NewGuid().ToString() + ".local";
        var done = new ManualResetEvent(false);
        Message? response = null;

        using var mdns = new MulticastService();
        mdns.UseIpv4 = true;
        mdns.UseIpv6 = false;
        mdns.NetworkInterfaceDiscovered += (s, e) => mdns.SendQuery(service);
        mdns.QueryReceived += (s, e) =>
        {
            var msg = e.Message;
            if (msg.Questions.Any(q => q.Name == service))
            {
                var res = msg.CreateResponse();
                res.Answers.Add(new ARecord
                {
                    Name = service,
                    Address = IPAddress.Parse("127.1.1.1")
                });
                mdns.SendAnswer(res);
            }
        };
        mdns.AnswerReceived += (s, e) =>
        {
            var msg = e.Message;
            if (msg.Answers.Any(answer => answer.Name == service))
            {
                response = msg;
                done.Set();
            }
        };
        mdns.Start();
        Assert.True(done.WaitOne(TimeSpan.FromSeconds(1)), "answer timeout");
        Assert.NotNull(response);
        Assert.True(response.IsResponse);
        Assert.Equal(MessageStatus.NoError, response.Status);
        Assert.True(response.AA);
        var a = (ARecord)response.Answers[0];
        Assert.Equal(IPAddress.Parse("127.1.1.1"), a.Address);
    }

    [Fact]
    [Trait("Category", "IPv6")]
    public void ReceiveAnswer_IPv6()
    {
        var service = Guid.NewGuid().ToString() + ".local";
        var done = new ManualResetEvent(false);
        Message? response = null;

        using var mdns = new MulticastService();
        mdns.UseIpv4 = false;
        mdns.UseIpv6 = true;
        mdns.NetworkInterfaceDiscovered += (s, e) => mdns.SendQuery(service);
        mdns.QueryReceived += (s, e) =>
        {
            var msg = e.Message;
            if (msg.Questions.Any(q => q.Name == service))
            {
                var res = msg.CreateResponse();
                res.Answers.Add(new AAAARecord
                {
                    Name = service,
                    Address = IPAddress.Parse("::2")
                });
                mdns.SendAnswer(res);
            }
        };
        mdns.AnswerReceived += (s, e) =>
        {
            var msg = e.Message;
            if (msg.Answers.Any(answer => answer.Name == service))
            {
                response = msg;
                done.Set();
            }
        };
        mdns.Start();
        Assert.True(done.WaitOne(TimeSpan.FromSeconds(1)), "answer timeout");
        Assert.NotNull(response);
        Assert.True(response.IsResponse);
        Assert.Equal(MessageStatus.NoError, response.Status);
        Assert.True(response.AA);
        var a = (AAAARecord)response.Answers[0];
        Assert.Equal(IPAddress.Parse("::2"), a.Address);
    }

    [Fact]
    public void ReceiveErrorAnswer()
    {
        var service = Guid.NewGuid().ToString() + ".local";
        var done = new ManualResetEvent(false);

        var mdns = new MulticastService();
        mdns.NetworkInterfaceDiscovered += (s, e) => mdns.SendQuery(service);
        mdns.QueryReceived += (s, e) =>
        {
            var msg = e.Message;
            if (msg.Questions.Any(q => q.Name == service))
            {
                var res = msg.CreateResponse();
                res.Status = MessageStatus.Refused;
                res.Answers.Add(new ARecord
                {
                    Name = service,
                    Address = IPAddress.Parse("127.1.1.1")
                });
                mdns.SendAnswer(res);
            }
        };
        mdns.AnswerReceived += (s, e) =>
        {
            var msg = e.Message;
            if (msg.Answers.Any(a => a.Name == service))
            {
                done.Set();
            }
        };
        try
        {
            mdns.Start();
            Assert.False(done.WaitOne(TimeSpan.FromSeconds(0.5)), "answer was not ignored");
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Fact]
    public void Nics()
    {
        var done = new ManualResetEvent(false);
        var mdns = new MulticastService();
        IEnumerable<NetworkInterface>? nics = null;
        mdns.NetworkInterfaceDiscovered += (s, e) =>
        {
            nics = e.NetworkInterfaces;
            done.Set();
        };
        mdns.Start();
        try
        {
            Assert.True(done.WaitOne(TimeSpan.FromSeconds(1)), "timeout");
            Assert.NotEmpty(nics ?? []);
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Fact]
    public void SendQuery_TooBig()
    {
        var done = new ManualResetEvent(false);
        var mdns = new MulticastService();
        mdns.NetworkInterfaceDiscovered += (s, e) => done.Set();
        mdns.Start();
        try
        {
            Assert.True(done.WaitOne(TimeSpan.FromSeconds(1)), "no nic");
            var query = new Message();
            query.Questions.Add(new Question { Name = "foo.bar.org" });
            query.AdditionalRecords.Add(new NULLRecord { Name = "foo.bar.org", Data = new byte[9000] });
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                mdns.SendQuery(query);
            });
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Fact]
    public void SendAnswer_TooBig()
    {
        var done = new ManualResetEvent(false);
        var mdns = new MulticastService();
        mdns.NetworkInterfaceDiscovered += (s, e) => done.Set();
        mdns.Start();
        try
        {
            Assert.True(done.WaitOne(TimeSpan.FromSeconds(1)), "no nic");
            var answer = new Message();
            answer.Answers.Add(new ARecord { Name = "foo.bar.org", Address = IPAddress.Loopback });
            answer.Answers.Add(new NULLRecord { Name = "foo.bar.org", Data = new byte[9000] });
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                mdns.SendAnswer(answer);
            });
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Fact]
    public void Multiple_Services()
    {
        var service = Guid.NewGuid().ToString() + ".local";
        var done = new ManualResetEvent(false);
        Message? response = null;

        var a = new MulticastService();
        a.QueryReceived += (s, e) =>
        {
            var msg = e.Message;
            if (msg.Questions.Any(q => q.Name == service))
            {
                var res = msg.CreateResponse();
                var addresses = MulticastService.GetIPAddresses()
                    .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                foreach (var address in addresses)
                {
                    res.Answers.Add(new ARecord
                    {
                        Name = service,
                        Address = address
                    });
                }
                a.SendAnswer(res);
            }
        };

        var b = new MulticastService();
        b.NetworkInterfaceDiscovered += (s, e) => b.SendQuery(service);
        b.AnswerReceived += (s, e) =>
        {
            var msg = e.Message;
            if (msg.Answers.Any(ans => ans.Name == service))
            {
                response = msg;
                done.Set();
            }
        };
        try
        {
            a.Start();
            b.Start();
            Assert.True(done.WaitOne(TimeSpan.FromSeconds(1)), "answer timeout");
            Assert.NotNull(response);
            Assert.True(response.IsResponse);
            Assert.Equal(MessageStatus.NoError, response.Status);
            Assert.True(response.AA);
            Assert.NotEmpty(response.Answers);
        }
        finally
        {
            b.Stop();
            a.Stop();
        }
    }

    [Fact]
    public void IPAddresses()
    {
        var addresses = MulticastService.GetIPAddresses().ToArray();
        Assert.NotEmpty(addresses);
    }

    [Fact]
    public void Disposable()
    {
        using (var mdns = new MulticastService())
        {
            Assert.NotNull(mdns);
        }

        using (var mdns = new MulticastService())
        {
            Assert.NotNull(mdns);
            mdns.Start();
        }
    }

    [Fact]
    public async Task Resolve()
    {
        var service = Guid.NewGuid().ToString() + ".local";
        var query = new Message();
        query.Questions.Add(new Question { Name = service, Type = DnsType.ANY });
        var cancellation = new CancellationTokenSource(2000);

        using var mdns = new MulticastService();
        mdns.QueryReceived += (s, e) =>
        {
            var msg = e.Message;
            if (msg.Questions.Any(q => q.Name == service))
            {
                var res = msg.CreateResponse();
                res.Answers.Add(new ARecord
                {
                    Name = service,
                    Address = IPAddress.Parse("127.1.1.1")
                });
                mdns.SendAnswer(res);
            }
        };
        mdns.Start();
        var response = await mdns.ResolveAsync(query, cancellation.Token);
        Assert.NotNull(response);
        Assert.True(response.IsResponse);
        Assert.Equal(MessageStatus.NoError, response.Status);
        Assert.True(response.AA);
        var a = (ARecord)response.Answers[0];
        Assert.Equal(IPAddress.Parse("127.1.1.1"), a.Address);
    }

    [Fact]
    public void Resolve_NoAnswer()
    {
        var service = Guid.NewGuid().ToString() + ".local";
        var query = new Message();
        query.Questions.Add(new Question { Name = service, Type = DnsType.ANY });
        var cancellation = new CancellationTokenSource(500);

        using var mdns = new MulticastService();
        mdns.Start();
        Assert.Throws<AggregateException>(() =>
        {
            var _ = mdns.ResolveAsync(query, cancellation.Token).Result;
        });
    }

    [Fact]
    public async Task DuplicateResponse()
    {
        var service = Guid.NewGuid().ToString() + ".local";
        using var mdns = new MulticastService();
        var answerCount = 0;
        mdns.NetworkInterfaceDiscovered += (s, e) =>
        {
            mdns.SendQuery(service);
            Thread.Sleep(250);
            mdns.SendQuery(service);
        };
        mdns.QueryReceived += (s, e) =>
        {
            var msg = e.Message;
            if (msg.Questions.Any(q => q.Name == service))
            {
                var res = msg.CreateResponse();
                res.Answers.Add(new ARecord
                {
                    Name = service,
                    Address = IPAddress.Parse("127.1.1.1")
                });
                mdns.SendAnswer(res);
            }
        };
        mdns.AnswerReceived += (s, e) =>
        {
            var msg = e.Message;
            if (msg.Answers.Any(answer => answer.Name == service))
            {
                ++answerCount;
            };
        };
        mdns.Start();
        await Task.Delay(1000);
        Assert.Equal(1, answerCount);
    }

    [Fact]
    public async Task NoDuplicateResponse()
    {
        var service = Guid.NewGuid().ToString() + ".local";

        using var mdns = new MulticastService();
        var answerCount = 0;
        mdns.NetworkInterfaceDiscovered += (s, e) =>
        {
            mdns.SendQuery(service);
            Thread.Sleep(250);
            mdns.SendQuery(service);
        };
        mdns.QueryReceived += (s, e) =>
        {
            var msg = e.Message;
            if (msg.Questions.Any(q => q.Name == service))
            {
                var res = msg.CreateResponse();
                res.Answers.Add(new ARecord
                {
                    Name = service,
                    Address = IPAddress.Parse("127.1.1.1")
                });
                mdns.SendAnswer(res, checkDuplicate: false);
            }
        };
        mdns.AnswerReceived += (s, e) =>
        {
            var msg = e.Message;
            if (msg.Answers.Any(answer => answer.Name == service))
            {
                ++answerCount;
            };
        };
        mdns.Start();
        await Task.Delay(2000);
        Assert.Equal(1, answerCount);

        mdns.SendQuery(service);
        await Task.Delay(2000);
        Assert.Equal(2, answerCount);
    }

    [Fact]
    public void Multiple_Listeners()
    {
        var ready1 = new ManualResetEvent(false);
        var ready2 = new ManualResetEvent(false);
        using var mdns1 = new MulticastService();
        using var mdns2 = new MulticastService();
        mdns1.NetworkInterfaceDiscovered += (s, e) => ready1.Set();
        mdns1.Start();

        mdns2.NetworkInterfaceDiscovered += (s, e) => ready2.Set();
        mdns2.Start();

        Assert.True(ready1.WaitOne(TimeSpan.FromSeconds(1)), "ready1 timeout");
        Assert.True(ready2.WaitOne(TimeSpan.FromSeconds(1)), "ready2 timeout");
    }

    [Fact]
    public void MalformedMessage()
    {
        byte[]? malformedMessage = null;
        using var mdns = new MulticastService();
        mdns.MalformedMessage += (s, e) => malformedMessage = e;

        var msg = new byte[] { 0xff };
        var endPoint = new IPEndPoint(IPAddress.Loopback, 5353);
        var udp = new UdpReceiveResult(msg, endPoint);
        mdns.OnDnsMessage(this, udp);

        Assert.Equal(msg, malformedMessage);
    }
}
